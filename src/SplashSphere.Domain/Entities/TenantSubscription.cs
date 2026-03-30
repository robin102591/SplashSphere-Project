using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Tracks a tenant's subscription plan, billing cycle, and usage limits.
/// One-to-one relationship with <see cref="Tenant"/>.
/// Created during onboarding (Trial) and updated on plan change or payment.
/// </summary>
public sealed class TenantSubscription : IAuditableEntity
{
    private TenantSubscription() { } // EF Core

    public TenantSubscription(string tenantId, PlanTier planTier, SubscriptionStatus status)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        PlanTier = planTier;
        Status = status;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public PlanTier PlanTier { get; set; } = PlanTier.Trial;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;

    // ── Trial tracking ───────────────────────────────────────────────────────
    public DateTime TrialStartDate { get; set; }
    public DateTime TrialEndDate { get; set; }
    public bool TrialExpired => Status == SubscriptionStatus.Trial && DateTime.UtcNow > TrialEndDate;

    // ── Billing cycle ────────────────────────────────────────────────────────
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? NextBillingDate { get; set; }

    // ── SaaS admin overrides ─────────────────────────────────────────────────
    public int? MaxBranchesOverride { get; set; }
    public int? MaxEmployeesOverride { get; set; }
    public int? SmsPerMonthOverride { get; set; }
    /// <summary>JSON dictionary of feature key → enabled/disabled overrides.</summary>
    public string? FeatureOverrides { get; set; }

    // ── Usage tracking ───────────────────────────────────────────────────────
    public int SmsUsedThisMonth { get; set; }
    public DateTime SmsCountResetDate { get; set; }

    // ── Audit ────────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
}
