using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Queries.GetStockMovements;

public sealed record GetStockMovementsQuery(
    string? SupplyItemId = null,
    string? MerchandiseId = null,
    MovementType? Type = null,
    string? BranchId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<StockMovementDto>>;
