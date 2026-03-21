namespace SplashSphere.Domain.Entities;

/// <summary>
/// Tenant-level configuration for cashier shift behaviour.
/// One record per tenant; created on demand (upsert).
/// </summary>
public sealed class ShiftSettings : IAuditableEntity
{
    private ShiftSettings() { } // EF Core

    public ShiftSettings(string tenantId)
    {
        Id       = Guid.NewGuid().ToString();
        TenantId = tenantId;
    }

    public string Id       { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Default opening cash fund pre-filled in the Open Shift form (₱).</summary>
    public decimal DefaultOpeningFund { get; set; } = 2000m;

    /// <summary>
    /// Variance threshold (₱) for auto-approval.
    /// |Variance| ≤ this → ReviewStatus.Approved automatically.
    /// </summary>
    public decimal AutoApproveThreshold { get; set; } = 50m;

    /// <summary>
    /// Variance threshold (₱) for auto-flagging.
    /// |Variance| > this → ReviewStatus.Flagged automatically.
    /// Values between AutoApproveThreshold and this → ReviewStatus.Pending.
    /// </summary>
    public decimal FlagThreshold { get; set; } = 200m;

    /// <summary>Whether cashiers must have an open shift before processing transactions.</summary>
    public bool RequireShiftForTransactions { get; set; } = true;

    /// <summary>Manila local time at which the end-of-day reminder is shown on the POS.</summary>
    public TimeOnly EndOfDayReminderTime { get; set; } = new(20, 0);

    // ── Audit ──────────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigation ─────────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
}
