using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Queries.GetRoyaltyPeriods;

public sealed record GetRoyaltyPeriodsQuery(
    string? FranchiseeTenantId,
    RoyaltyStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<RoyaltyPeriodDto>>;
