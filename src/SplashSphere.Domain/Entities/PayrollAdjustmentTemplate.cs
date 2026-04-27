namespace SplashSphere.Domain.Entities;

/// <summary>
/// A reusable payroll adjustment preset (e.g. SSS, PhilHealth, Pag-IBIG, overtime bonus)
/// configured per tenant. Stores a default amount and type so admins can quickly apply
/// standard deductions or bonuses during weekly payroll review.
/// <para>
/// Soft-deleted via <see cref="IsActive"/> toggle; never hard-deleted.
/// </para>
/// Unique constraint: [TenantId, Name] — no duplicate names within a tenant.
/// </summary>
public sealed class PayrollAdjustmentTemplate : IAuditableEntity, ITenantScoped
{
    private PayrollAdjustmentTemplate() { } // EF Core

    public PayrollAdjustmentTemplate(
        string tenantId,
        string name,
        AdjustmentType type,
        decimal defaultAmount)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Name = name;
        Type = type;
        DefaultAmount = defaultAmount;
        IsActive = true;
        SortOrder = 0;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Display name (e.g. "SSS Contribution"). Max 100 characters.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether this template adds a bonus or deduction.</summary>
    public AdjustmentType Type { get; set; }

    /// <summary>Default amount in PHP. Precision (10, 2).</summary>
    public decimal DefaultAmount { get; set; }

    /// <summary>Soft-delete flag. Inactive templates are hidden from the bulk-apply dialog.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Display order in UI lists. Lower values appear first.</summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// True for templates auto-created during onboarding (SSS, PhilHealth, Pag-IBIG, Tax).
    /// System-default templates cannot be hard-deleted.
    /// </summary>
    public bool IsSystemDefault { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public ICollection<PayrollAdjustment> Adjustments { get; set; } = new List<PayrollAdjustment>();
}
