using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Queries.GetMyRoyalties;

public sealed record GetMyRoyaltiesQuery(
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<RoyaltyPeriodDto>>;
