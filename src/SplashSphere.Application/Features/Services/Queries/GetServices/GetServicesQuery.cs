using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Services.Queries.GetServices;

public sealed record GetServicesQuery(
    int Page = 1,
    int PageSize = 20,
    string? CategoryId = null,
    string? Search = null) : IQuery<PagedResult<ServiceSummaryDto>>;
