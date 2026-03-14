using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Queries.GetMerchandise;

public sealed class GetMerchandiseQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMerchandiseQuery, PagedResult<MerchandiseDto>>
{
    public async Task<PagedResult<MerchandiseDto>> Handle(
        GetMerchandiseQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Merchandise.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CategoryId))
            query = query.Where(m => m.CategoryId == request.CategoryId);

        // IsLowStock is a computed property — replicate the condition in SQL.
        if (request.LowStockOnly == true)
            query = query.Where(m => m.StockQuantity <= m.LowStockThreshold);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(m =>
                m.Name.Contains(search) ||
                m.Sku.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.Category != null ? m.Category.Name : null)
            .ThenBy(m => m.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MerchandiseDto(
                m.Id,
                m.Name,
                m.Sku,
                m.Description,
                m.CategoryId,
                m.Category != null ? m.Category.Name : null,
                m.Price,
                m.CostPrice,
                m.StockQuantity,
                m.LowStockThreshold,
                m.StockQuantity <= m.LowStockThreshold,
                m.IsActive,
                m.CreatedAt,
                m.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<MerchandiseDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
