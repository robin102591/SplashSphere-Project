using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Queries.GetEquipment;

public sealed class GetEquipmentQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetEquipmentQuery, PagedResult<EquipmentDto>>
{
    public async Task<PagedResult<EquipmentDto>> Handle(
        GetEquipmentQuery request, CancellationToken cancellationToken)
    {
        var query = db.Equipment.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(e => e.BranchId == request.BranchId);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EquipmentDto(
                e.Id,
                e.Branch.Name,
                e.Name,
                e.Brand,
                e.Model,
                e.SerialNumber,
                e.Status.ToString(),
                e.Location,
                e.IsActive,
                e.MaintenanceLogs.Max(m => (DateTime?)m.PerformedDate),
                e.MaintenanceLogs
                    .OrderByDescending(m => m.PerformedDate)
                    .Select(m => m.NextDueDate)
                    .FirstOrDefault(),
                e.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<EquipmentDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
