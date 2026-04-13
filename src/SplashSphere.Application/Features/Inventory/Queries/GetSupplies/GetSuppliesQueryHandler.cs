using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSupplies;

public sealed class GetSuppliesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSuppliesQuery, PagedResult<SupplyItemDto>>
{
    public async Task<PagedResult<SupplyItemDto>> Handle(
        GetSuppliesQuery request, CancellationToken cancellationToken)
    {
        var query = db.SupplyItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CategoryId))
            query = query.Where(i => i.CategoryId == request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.BranchId))
            query = query.Where(i => i.BranchId == request.BranchId);

        if (!string.IsNullOrWhiteSpace(request.StockStatus))
        {
            query = request.StockStatus.ToLowerInvariant() switch
            {
                "low" => query.Where(i => i.ReorderLevel.HasValue && i.CurrentStock <= i.ReorderLevel.Value && i.CurrentStock > 0),
                "out" => query.Where(i => i.CurrentStock <= 0),
                _ => query,
            };
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(i => i.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new SupplyItemDto(
                i.Id,
                i.BranchId,
                i.Branch.Name,
                i.CategoryId,
                i.Category != null ? i.Category.Name : null,
                i.Name,
                i.Description,
                i.Unit,
                i.CurrentStock,
                i.ReorderLevel,
                i.AverageUnitCost,
                i.IsActive,
                i.ReorderLevel.HasValue && i.CurrentStock <= i.ReorderLevel.Value,
                i.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<SupplyItemDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
