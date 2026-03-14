namespace SplashSphere.Domain.Entities;

/// <summary>
/// A single package line item within a <see cref="Transaction"/>.
/// Mirrors <see cref="TransactionService"/> but references a <see cref="ServicePackage"/>
/// instead of an individual service. Package commissions are always percentage-based.
/// <para>
/// Stores vehicle dimension snapshots (<see cref="VehicleTypeId"/>, <see cref="SizeId"/>)
/// and the resolved price/commission at transaction time for immutable audit history.
/// </para>
/// Cascade delete: deleted when parent <see cref="Transaction"/> is deleted.
/// </summary>
public sealed class TransactionPackage : IAuditableEntity
{
    private TransactionPackage() { } // EF Core

    public TransactionPackage(
        string tenantId,
        string transactionId,
        string packageId,
        string vehicleTypeId,
        string sizeId,
        decimal unitPrice,
        decimal totalCommission)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        TransactionId = transactionId;
        PackageId = packageId;
        VehicleTypeId = vehicleTypeId;
        SizeId = sizeId;
        UnitPrice = unitPrice;
        TotalCommission = totalCommission;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;

    /// <summary>Snapshot of the vehicle type used in the package pricing/commission lookup.</summary>
    public string VehicleTypeId { get; set; } = string.Empty;

    /// <summary>Snapshot of the vehicle size used in the package pricing/commission lookup.</summary>
    public string SizeId { get; set; } = string.Empty;

    /// <summary>Resolved package price after matrix lookup. Precision (10, 2).</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total commission pool for this package line item (before employee split).
    /// Always calculated as a percentage of <see cref="UnitPrice"/>. Precision (10, 2).
    /// </summary>
    public decimal TotalCommission { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Transaction Transaction { get; set; } = null!;
    public ServicePackage Package { get; set; } = null!;
    public VehicleType VehicleType { get; set; } = null!;
    public Size Size { get; set; } = null!;

    /// <summary>Per-employee commission split records. Cascade delete.</summary>
    public ICollection<PackageEmployeeAssignment> EmployeeAssignments { get; set; } = [];
}
