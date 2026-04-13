using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSupplyById;

public sealed class GetSupplyByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSupplyByIdQuery, SupplyItemDetailDto?>
{
    public async Task<SupplyItemDetailDto?> Handle(
        GetSupplyByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.SupplyItems
            .AsNoTracking()
            .Where(i => i.Id == request.Id)
            .Select(i => new
            {
                i.Id,
                i.BranchId,
                BranchName = i.Branch.Name,
                i.CategoryId,
                CategoryName = i.Category != null ? i.Category.Name : null,
                i.Name,
                i.Description,
                i.Unit,
                i.CurrentStock,
                i.ReorderLevel,
                i.AverageUnitCost,
                i.IsActive,
                IsLowStock = i.ReorderLevel.HasValue && i.CurrentStock <= i.ReorderLevel.Value,
                i.CreatedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return null;

        var movements = await db.StockMovements
            .AsNoTracking()
            .Where(m => m.SupplyItemId == request.Id)
            .OrderByDescending(m => m.MovementDate)
            .Take(20)
            .Select(m => new StockMovementDto(
                m.Id,
                m.Branch.Name,
                m.SupplyItem != null ? m.SupplyItem.Name : "",
                m.Type.ToString(),
                m.Quantity,
                m.UnitCost,
                m.TotalCost,
                m.Reference,
                m.Notes,
                m.PerformedByUserId,
                m.MovementDate))
            .ToListAsync(cancellationToken);

        return new SupplyItemDetailDto(
            item.Id,
            item.BranchId,
            item.BranchName,
            item.CategoryId,
            item.CategoryName,
            item.Name,
            item.Description,
            item.Unit,
            item.CurrentStock,
            item.ReorderLevel,
            item.AverageUnitCost,
            item.IsActive,
            item.IsLowStock,
            item.CreatedAt,
            movements);
    }
}
