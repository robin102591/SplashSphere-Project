using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Merchandise.Queries.GetLowStockMerchandise;

public sealed record LowStockItemDto(
    string Id,
    string Name,
    string Sku,
    int StockQuantity,
    int LowStockThreshold);

public sealed record GetLowStockMerchandiseQuery : IQuery<List<LowStockItemDto>>;
