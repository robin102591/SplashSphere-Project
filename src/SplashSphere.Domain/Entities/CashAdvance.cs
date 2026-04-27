namespace SplashSphere.Domain.Entities;

/// <summary>
/// An employee cash advance with a lifecycle from request through full repayment.
/// Active advances are automatically deducted from payroll each period via
/// <see cref="DeductionPerPeriod"/> until <see cref="RemainingBalance"/> reaches zero.
/// </summary>
public sealed class CashAdvance : IAuditableEntity, ITenantScoped
{
    private CashAdvance() { } // EF Core

    public CashAdvance(
        string tenantId,
        string employeeId,
        decimal amount,
        decimal deductionPerPeriod,
        string? reason = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        EmployeeId = employeeId;
        Amount = amount;
        RemainingBalance = amount;
        DeductionPerPeriod = deductionPerPeriod;
        Reason = reason;
        Status = CashAdvanceStatus.Pending;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>Original advance amount in PHP. Precision (10, 2).</summary>
    public decimal Amount { get; set; }

    /// <summary>Outstanding balance still owed. Decremented each payroll period. Precision (10, 2).</summary>
    public decimal RemainingBalance { get; set; }

    /// <summary>Current lifecycle status.</summary>
    public CashAdvanceStatus Status { get; set; }

    /// <summary>Optional reason for the advance request. Max 500 chars.</summary>
    public string? Reason { get; set; }

    /// <summary>FK to User who approved this advance. Null until approved.</summary>
    public string? ApprovedById { get; set; }

    /// <summary>UTC timestamp of approval.</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Amount to deduct from each payroll period. Precision (10, 2).</summary>
    public decimal DeductionPerPeriod { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Employee Employee { get; set; } = null!;
    public User? ApprovedBy { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
