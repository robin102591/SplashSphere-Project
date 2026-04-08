using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Queries.GetFranchisees;

public sealed record GetFranchiseesQuery(
    string? Search,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<FranchiseeListItemDto>>;
