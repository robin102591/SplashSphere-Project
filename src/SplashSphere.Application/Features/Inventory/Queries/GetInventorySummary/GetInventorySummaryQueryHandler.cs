using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Inventory.Queries.GetInventorySummary;

public sealed class GetInventorySummaryQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventorySummaryQuery, InventorySummaryDto>
{
    public async Task<InventorySummaryDto> Handle(
        GetInventorySummaryQuery request, CancellationToken cancellationToken)
    {
        var query = db.SupplyItems.AsNoTracking().Where(s => s.IsActive);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(s => s.BranchId == request.BranchId);

        var totalSupplyItems = await query.CountAsync(cancellationToken);

        var lowStockCount = await query
            .Where(s => s.ReorderLevel.HasValue && s.CurrentStock <= s.ReorderLevel.Value && s.CurrentStock > 0)
            .CountAsync(cancellationToken);

        var outOfStockCount = await query
            .Where(s => s.CurrentStock <= 0)
            .CountAsync(cancellationToken);

        var totalStockValue = await query
            .SumAsync(s => (decimal?)(s.CurrentStock * s.AverageUnitCost) ?? 0, cancellationToken);

        // Top 10 low stock items ordered by ratio (CurrentStock / ReorderLevel) ascending
        var lowStockItems = await query
            .Where(s => s.ReorderLevel.HasValue && s.ReorderLevel.Value > 0 && s.CurrentStock <= s.ReorderLevel.Value)
            .OrderBy(s => s.CurrentStock / s.ReorderLevel!.Value)
            .Take(10)
            .Select(s => new LowStockItemDto(
                s.Id,
                s.Name,
                s.Unit,
                s.Branch.Name,
                s.CurrentStock,
                s.ReorderLevel,
                s.AverageUnitCost))
            .ToListAsync(cancellationToken);

        return new InventorySummaryDto(
            totalSupplyItems,
            lowStockCount,
            outOfStockCount,
            totalStockValue,
            lowStockItems);
    }
}
