using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Merchandise.Queries.GetLowStockMerchandise;

public sealed class GetLowStockMerchandiseQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetLowStockMerchandiseQuery, List<LowStockItemDto>>
{
    public async Task<List<LowStockItemDto>> Handle(
        GetLowStockMerchandiseQuery request,
        CancellationToken cancellationToken)
    {
        return await db.Merchandise
            .AsNoTracking()
            .Where(m => m.IsActive && m.StockQuantity <= m.LowStockThreshold)
            .OrderBy(m => m.StockQuantity)
            .Select(m => new LowStockItemDto(
                m.Id,
                m.Name,
                m.Sku,
                m.StockQuantity,
                m.LowStockThreshold))
            .ToListAsync(cancellationToken);
    }
}
