using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.ServiceCategories.Queries.GetServiceCategories;

public sealed record GetServiceCategoriesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IQuery<PagedResult<ServiceCategoryDto>>;
