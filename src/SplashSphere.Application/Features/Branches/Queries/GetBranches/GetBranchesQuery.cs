using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Branches.Queries.GetBranches;

/// <summary>Returns a paginated list of branches for the current tenant.</summary>
public sealed record GetBranchesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IQuery<PagedResult<BranchDto>>;
