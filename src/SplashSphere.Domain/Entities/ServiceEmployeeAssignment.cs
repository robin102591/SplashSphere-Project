namespace SplashSphere.Domain.Entities;

/// <summary>
/// Records which employee worked on a specific <see cref="TransactionService"/> line item
/// and their individual commission split amount.
/// <para>
/// <b>Commission split algorithm</b> (Step 3 of transaction creation):
/// <code>commissionPerEmployee = Math.Round(totalCommission / employeeCount, 2, MidpointRounding.AwayFromZero)</code>
/// All assigned employees receive the same rounded amount.
/// </para>
/// Unique constraint: [TransactionServiceId, EmployeeId] — an employee can only be
/// assigned once per service line item.
/// Cascade delete: deleted when parent <see cref="TransactionService"/> is deleted.
/// </summary>
public sealed class ServiceEmployeeAssignment : IAuditableEntity, ITenantScoped
{
    private ServiceEmployeeAssignment() { } // EF Core

    public ServiceEmployeeAssignment(
        string tenantId,
        string transactionServiceId,
        string employeeId,
        decimal commissionAmount)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        TransactionServiceId = transactionServiceId;
        EmployeeId = employeeId;
        CommissionAmount = commissionAmount;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TransactionServiceId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// This employee's share of the service commission pool.
    /// Rounded with <c>MidpointRounding.AwayFromZero</c> to 2 decimal places.
    /// Precision (10, 2).
    /// </summary>
    public decimal CommissionAmount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public TransactionService TransactionService { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
