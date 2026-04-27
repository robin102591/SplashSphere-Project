using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Loyalty.Queries.GetRewards;

/// <summary>
/// List the active rewards the tenant offers. <see cref="ConnectRewardDto.IsAffordable"/>
/// is set based on the caller's current points balance at this tenant.
/// Returns an empty list if the tenant doesn't offer loyalty or the caller is not linked.
/// </summary>
public sealed record GetRewardsQuery(string TenantId) : IQuery<IReadOnlyList<ConnectRewardDto>>;
