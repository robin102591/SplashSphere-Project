using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Sizes.Queries.GetSizes;

public sealed record GetSizesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IQuery<PagedResult<SizeDto>>;
