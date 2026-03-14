using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.MerchandiseCategories.Queries.GetMerchandiseCategories;

public sealed record GetMerchandiseCategoriesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IQuery<PagedResult<MerchandiseCategoryDto>>;
