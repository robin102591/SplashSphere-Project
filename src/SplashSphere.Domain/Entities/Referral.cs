namespace SplashSphere.Domain.Entities;

/// <summary>
/// Tracks a customer referral within a single tenant's loyalty program.
/// <para>
/// Referrals are <b>per-tenant</b>: Maria's code for AquaShine is distinct from
/// her code for SpeedyWash. Both participants earn points when the referred
/// customer completes their first wash (<see cref="ReferralStatus.Completed"/>).
/// </para>
/// <para>
/// Unused codes expire after 90 days via the <c>ExpireReferrals</c> Hangfire job
/// (see Prompt 22.4).
/// </para>
/// </summary>
public sealed class Referral : IAuditableEntity, ITenantScoped
{
    private Referral() { } // EF Core

    public Referral(
        string tenantId,
        string referrerCustomerId,
        string referralCode,
        int referrerPointsReward,
        int referredPointsReward)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        ReferrerCustomerId = referrerCustomerId;
        ReferralCode = referralCode.ToUpperInvariant();
        ReferrerPointsEarned = referrerPointsReward;
        ReferredPointsEarned = referredPointsReward;
        Status = ReferralStatus.Pending;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>FK to the tenant's <see cref="Customer"/> who owns the code.</summary>
    public string ReferrerCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// FK to the referred <see cref="Customer"/> — set when the code is applied
    /// during sign-up or join. Nullable while the referral is still Pending
    /// (code shared but not yet redeemed).
    /// </summary>
    public string? ReferredCustomerId { get; set; }

    /// <summary>
    /// Human-readable code (e.g. "MARIA-AQUA-7X3F"). Unique per tenant.
    /// Uppercase at rest.
    /// </summary>
    public string ReferralCode { get; set; } = string.Empty;

    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;

    /// <summary>Points the referrer earned when the referral completed.</summary>
    public int ReferrerPointsEarned { get; set; }

    /// <summary>Points the referred customer earned on their first wash.</summary>
    public int ReferredPointsEarned { get; set; }

    /// <summary>Set when referred customer completes their first wash.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Set when the code is marked Expired by the scheduled job.</summary>
    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Customer Referrer { get; set; } = null!;
    public Customer? Referred { get; set; }
}
