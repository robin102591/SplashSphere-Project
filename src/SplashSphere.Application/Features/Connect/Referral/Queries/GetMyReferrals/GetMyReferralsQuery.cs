using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Referral.Queries.GetMyReferrals;

/// <summary>
/// List the caller's referrals at a tenant (only rows where someone actually
/// used the code, i.e. <c>ReferredCustomerId</c> is not null). Newest first.
/// Empty list when the caller is not linked or has shared no code.
/// </summary>
public sealed record GetMyReferralsQuery(string TenantId)
    : IQuery<IReadOnlyList<ConnectReferralListItemDto>>;
