using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.Referral;

/// <summary>
/// The caller's referral code at a tenant, plus aggregate stats. Returned by
/// <c>GET /api/v1/connect/carwashes/{tenantId}/referral-code</c>.
/// <para>
/// If the caller has never shared a code at this tenant, the handler lazily
/// issues one — so this endpoint always returns a code for a joined customer.
/// </para>
/// </summary>
public sealed record ConnectReferralCodeDto(
    string Code,
    int ReferrerPointsReward,
    int ReferredPointsReward,
    int TotalReferrals,
    int CompletedReferrals,
    int PendingReferrals,
    int PointsEarned);

/// <summary>One row on the "My Referrals" screen.</summary>
public sealed record ConnectReferralListItemDto(
    string Id,
    string? ReferredName,
    ReferralStatus Status,
    int ReferrerPointsEarned,
    DateTime? CompletedAt,
    DateTime CreatedAt);

/// <summary>Result of <c>POST /api/v1/connect/auth/apply-referral</c>.</summary>
public sealed record ApplyReferralResultDto(
    string ReferralId,
    string TenantId,
    string TenantName,
    int ReferredPointsReward);
