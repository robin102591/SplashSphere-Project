using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Immutable audit trail of plan tier changes for a tenant.
/// Created on every upgrade, downgrade, or SaaS admin override.
/// </summary>
public sealed class PlanChangeLog : IAuditableEntity
{
    private PlanChangeLog() { } // EF Core

    public PlanChangeLog(
        string tenantId,
        PlanTier fromPlan,
        PlanTier toPlan,
        string changedBy,
        string reason)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        FromPlan = fromPlan;
        ToPlan = toPlan;
        ChangedBy = changedBy;
        Reason = reason;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public PlanTier FromPlan { get; set; }
    public PlanTier ToPlan { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
}
