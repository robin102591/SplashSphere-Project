using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Tenant-level configuration for payroll period scheduling.
/// One record per tenant; created on demand (upsert).
/// </summary>
public sealed class PayrollSettings : IAuditableEntity
{
    private PayrollSettings() { } // EF Core

    public PayrollSettings(string tenantId)
    {
        Id       = Guid.NewGuid().ToString();
        TenantId = tenantId;
    }

    public string Id       { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// How often payroll periods are created: Weekly or SemiMonthly.
    /// Default: Weekly.
    /// </summary>
    public PayrollFrequency Frequency { get; set; } = PayrollFrequency.Weekly;

    /// <summary>
    /// The day of the week each 7-day payroll period begins (Weekly only).
    /// Default: Monday (preserves ISO week behavior).
    /// Ignored when <see cref="Frequency"/> is <see cref="PayrollFrequency.SemiMonthly"/>.
    /// </summary>
    public DayOfWeek CutOffStartDay { get; set; } = DayOfWeek.Monday;

    // ── Audit ──────────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation ─────────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
}
