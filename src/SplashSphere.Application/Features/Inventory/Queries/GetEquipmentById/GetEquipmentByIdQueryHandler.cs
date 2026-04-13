using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetEquipmentById;

public sealed class GetEquipmentByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetEquipmentByIdQuery, EquipmentDetailDto?>
{
    public async Task<EquipmentDetailDto?> Handle(
        GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Equipment
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .Select(e => new EquipmentDetailDto(
                e.Id,
                e.BranchId,
                e.Branch.Name,
                e.Name,
                e.Brand,
                e.Model,
                e.SerialNumber,
                e.Status.ToString(),
                e.PurchaseDate,
                e.PurchaseCost,
                e.WarrantyExpiry,
                e.Location,
                e.Notes,
                e.IsActive,
                e.CreatedAt,
                e.MaintenanceLogs
                    .OrderByDescending(m => m.PerformedDate)
                    .Select(m => new MaintenanceLogDto(
                        m.Id,
                        m.Type.ToString(),
                        m.Description,
                        m.Cost,
                        m.PerformedBy,
                        m.PerformedDate,
                        m.NextDueDate,
                        m.NextDueHours,
                        m.Notes))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
