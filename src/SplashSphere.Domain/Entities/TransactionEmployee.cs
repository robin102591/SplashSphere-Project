namespace SplashSphere.Domain.Entities;

/// <summary>
/// Aggregated commission summary for a single employee within a <see cref="Transaction"/>.
/// Created in Step 7 of the transaction creation algorithm by summing all
/// <see cref="ServiceEmployeeAssignment.CommissionAmount"/> and
/// <see cref="PackageEmployeeAssignment.CommissionAmount"/> rows for this employee.
/// <para>
/// This denormalised summary makes payroll period closing efficient — the close job
/// sums <see cref="TotalCommission"/> across all <c>TransactionEmployee</c> rows for
/// an employee in a period rather than joining through assignment tables.
/// </para>
/// Unique constraint: [TransactionId, EmployeeId] — one summary row per employee per transaction.
/// Cascade delete: deleted when parent <see cref="Transaction"/> is deleted.
/// </summary>
public sealed class TransactionEmployee : IAuditableEntity
{
    private TransactionEmployee() { } // EF Core

    public TransactionEmployee(
        string tenantId,
        string transactionId,
        string employeeId,
        decimal totalCommission)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        TransactionId = transactionId;
        EmployeeId = employeeId;
        TotalCommission = totalCommission;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// Sum of all commission split amounts earned by this employee across all services
    /// and packages in this transaction. Rounded using
    /// <c>MidpointRounding.AwayFromZero</c> at the individual assignment level.
    /// Precision (10, 2).
    /// </summary>
    public decimal TotalCommission { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Transaction Transaction { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
