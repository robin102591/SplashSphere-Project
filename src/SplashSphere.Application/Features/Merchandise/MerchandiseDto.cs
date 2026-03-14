namespace SplashSphere.Application.Features.Merchandise;

public sealed record MerchandiseDto(
    string Id,
    string Name,
    string Sku,
    string? Description,
    string? CategoryId,
    string? CategoryName,
    decimal Price,
    decimal? CostPrice,
    int StockQuantity,
    int LowStockThreshold,
    bool IsLowStock,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
