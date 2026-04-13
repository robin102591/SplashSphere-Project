using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Inventory.Queries.GetEquipmentMaintenanceReport;

public sealed class GetEquipmentMaintenanceReportQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetEquipmentMaintenanceReportQuery, EquipmentMaintenanceReportDto>
{
    public async Task<EquipmentMaintenanceReportDto> Handle(
        GetEquipmentMaintenanceReportQuery request, CancellationToken cancellationToken)
    {
        var equipQuery = db.Equipment.AsNoTracking().Where(e => e.IsActive);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            equipQuery = equipQuery.Where(e => e.BranchId == request.BranchId);

        var totalEquipment = await equipQuery.CountAsync(cancellationToken);

        var needsMaintenanceCount = await equipQuery
            .Where(e => e.Status == EquipmentStatus.NeedsMaintenance)
            .CountAsync(cancellationToken);

        var underRepairCount = await equipQuery
            .Where(e => e.Status == EquipmentStatus.UnderRepair)
            .CountAsync(cancellationToken);

        // Maintenance costs this month (UTC-based, current calendar month)
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var logQuery = db.MaintenanceLogs.AsNoTracking()
            .Where(ml => ml.PerformedDate >= monthStart);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            logQuery = logQuery.Where(ml => ml.Equipment.BranchId == request.BranchId);

        var totalMaintenanceCostThisMonth = await logQuery
            .SumAsync(ml => (decimal?)ml.Cost ?? 0, cancellationToken);

        // Get the last maintenance log per equipment for description
        // and all equipment with NextDueDate within 30 days or overdue
        var threshold = now.AddDays(30);

        var equipmentWithDueDates = await equipQuery
            .Where(e => e.MaintenanceLogs.Any(ml => ml.NextDueDate.HasValue))
            .Select(e => new
            {
                e.Id,
                e.Name,
                BranchName = e.Branch.Name,
                LastLog = e.MaintenanceLogs
                    .OrderByDescending(ml => ml.PerformedDate)
                    .Select(ml => new { ml.Description, ml.NextDueDate })
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        // Filter and map to upcoming and overdue
        var upcoming = equipmentWithDueDates
            .Where(e => e.LastLog?.NextDueDate != null
                        && e.LastLog.NextDueDate.Value > now
                        && e.LastLog.NextDueDate.Value <= threshold)
            .OrderBy(e => e.LastLog!.NextDueDate)
            .Select(e => new MaintenanceDueItemDto(
                e.Id,
                e.Name,
                e.BranchName,
                e.LastLog!.Description,
                e.LastLog.NextDueDate,
                (int)(e.LastLog.NextDueDate!.Value - now).TotalDays))
            .ToList();

        var overdue = equipmentWithDueDates
            .Where(e => e.LastLog?.NextDueDate != null && e.LastLog.NextDueDate.Value <= now)
            .OrderBy(e => e.LastLog!.NextDueDate)
            .Select(e => new MaintenanceDueItemDto(
                e.Id,
                e.Name,
                e.BranchName,
                e.LastLog!.Description,
                e.LastLog.NextDueDate,
                (int)(e.LastLog.NextDueDate!.Value - now).TotalDays))
            .ToList();

        return new EquipmentMaintenanceReportDto(
            totalEquipment,
            needsMaintenanceCount,
            underRepairCount,
            totalMaintenanceCostThisMonth,
            upcoming,
            overdue);
    }
}
