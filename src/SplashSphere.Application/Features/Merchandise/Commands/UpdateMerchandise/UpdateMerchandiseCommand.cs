using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Commands.UpdateMerchandise;

/// <summary>
/// Updates merchandise details. SKU is immutable after creation.
/// StockQuantity is managed exclusively via AdjustStockCommand.
/// </summary>
public sealed record UpdateMerchandiseCommand(
    string Id,
    string Name,
    decimal Price,
    int LowStockThreshold,
    string? CategoryId,
    string? Description,
    decimal? CostPrice) : ICommand;
