namespace SplashSphere.Domain.Entities;

/// <summary>
/// Records which employee worked on a specific <see cref="TransactionPackage"/> line item
/// and their individual commission split amount.
/// Mirrors <see cref="ServiceEmployeeAssignment"/> but for package line items.
/// <para>
/// Package commission is always a percentage of the package price, split equally
/// among all assigned employees using <c>MidpointRounding.AwayFromZero</c>.
/// </para>
/// Unique constraint: [TransactionPackageId, EmployeeId] — an employee can only be
/// assigned once per package line item.
/// Cascade delete: deleted when parent <see cref="TransactionPackage"/> is deleted.
/// </summary>
public sealed class PackageEmployeeAssignment : IAuditableEntity
{
    private PackageEmployeeAssignment() { } // EF Core

    public PackageEmployeeAssignment(
        string tenantId,
        string transactionPackageId,
        string employeeId,
        decimal commissionAmount)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        TransactionPackageId = transactionPackageId;
        EmployeeId = employeeId;
        CommissionAmount = commissionAmount;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TransactionPackageId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// This employee's share of the package commission pool.
    /// Rounded with <c>MidpointRounding.AwayFromZero</c> to 2 decimal places.
    /// Precision (10, 2).
    /// </summary>
    public decimal CommissionAmount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public TransactionPackage TransactionPackage { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
