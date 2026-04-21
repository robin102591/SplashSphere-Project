namespace SplashSphere.Domain.Enums;

/// <summary>
/// Lifecycle state of a <see cref="Entities.Referral"/>.
/// </summary>
public enum ReferralStatus
{
    /// <summary>Referred customer signed up but has not completed their first wash.</summary>
    Pending = 1,

    /// <summary>Referred customer completed their first wash — both parties earn points.</summary>
    Completed = 2,

    /// <summary>Referral code expired without being redeemed (see ExpireReferrals job).</summary>
    Expired = 3,
}
