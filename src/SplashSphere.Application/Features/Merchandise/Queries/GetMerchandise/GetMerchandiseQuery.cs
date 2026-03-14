using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Merchandise.Queries.GetMerchandise;

public sealed record GetMerchandiseQuery(
    int Page = 1,
    int PageSize = 20,
    string? CategoryId = null,
    bool? LowStockOnly = null,
    string? Search = null) : IQuery<PagedResult<MerchandiseDto>>;
