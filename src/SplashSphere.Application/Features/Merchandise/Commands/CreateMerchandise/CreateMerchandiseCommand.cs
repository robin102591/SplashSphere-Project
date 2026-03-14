using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Commands.CreateMerchandise;

public sealed record CreateMerchandiseCommand(
    string Name,
    string Sku,
    decimal Price,
    int StockQuantity,
    int LowStockThreshold,
    string? CategoryId,
    string? Description,
    decimal? CostPrice) : ICommand<string>;
