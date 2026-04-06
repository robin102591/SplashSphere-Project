using MediatR;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltyRewards;

public sealed record GetLoyaltyRewardsQuery(
    bool? ActiveOnly,
    int Page = 1,
    int PageSize = 50) : IRequest<PagedResult<LoyaltyRewardDto>>;
