using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Inventory.Queries.GetSupplies;

public sealed record GetSuppliesQuery(
    string? CategoryId = null,
    string? BranchId = null,
    string? StockStatus = null,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResult<SupplyItemDto>>;
