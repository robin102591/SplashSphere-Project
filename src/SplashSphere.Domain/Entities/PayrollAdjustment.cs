namespace SplashSphere.Domain.Entities;

/// <summary>
/// An itemised bonus or deduction line on a <see cref="PayrollEntry"/>.
/// Created when an admin applies an adjustment (individually or via bulk apply).
/// <para>
/// The parent entry's <see cref="PayrollEntry.Bonuses"/> and <see cref="PayrollEntry.Deductions"/>
/// fields are denormalised totals recalculated from these rows.
/// </para>
/// </summary>
public sealed class PayrollAdjustment : IAuditableEntity
{
    private PayrollAdjustment() { } // EF Core

    public PayrollAdjustment(
        string tenantId,
        string payrollEntryId,
        AdjustmentType type,
        string category,
        decimal amount,
        string? notes = null,
        string? templateId = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        PayrollEntryId = payrollEntryId;
        Type = type;
        Category = category;
        Amount = amount;
        Notes = notes;
        TemplateId = templateId;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PayrollEntryId { get; set; } = string.Empty;

    /// <summary>Whether this adjustment adds a bonus or deduction.</summary>
    public AdjustmentType Type { get; set; }

    /// <summary>Category label, e.g. "SSS", "PhilHealth", "Cash Advance", or custom text. Max 100 chars.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Adjustment amount in PHP. Always positive. Precision (10, 2).</summary>
    public decimal Amount { get; set; }

    /// <summary>Optional notes for this adjustment line.</summary>
    public string? Notes { get; set; }

    /// <summary>FK to the template that created this adjustment (null for manual entries).</summary>
    public string? TemplateId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public PayrollEntry Entry { get; set; } = null!;
    public PayrollAdjustmentTemplate? Template { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
