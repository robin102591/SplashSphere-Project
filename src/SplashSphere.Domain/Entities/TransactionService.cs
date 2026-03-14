namespace SplashSphere.Domain.Entities;

/// <summary>
/// A single service line item within a <see cref="Transaction"/>.
/// <para>
/// Stores resolved snapshots of the vehicle dimension (<see cref="VehicleTypeId"/>,
/// <see cref="SizeId"/>) and the computed price/commission at transaction time, so that
/// subsequent changes to pricing or commission matrices do not alter historical records.
/// </para>
/// <para>
/// <see cref="TotalCommission"/> is the commission pool for this line item before employee
/// splitting. The actual per-employee split amounts are stored on each
/// <see cref="ServiceEmployeeAssignment"/> child record.
/// </para>
/// Cascade delete: deleted when parent <see cref="Transaction"/> is deleted.
/// </summary>
public sealed class TransactionService : IAuditableEntity
{
    private TransactionService() { } // EF Core

    public TransactionService(
        string tenantId,
        string transactionId,
        string serviceId,
        string vehicleTypeId,
        string sizeId,
        decimal unitPrice,
        decimal totalCommission)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        TransactionId = transactionId;
        ServiceId = serviceId;
        VehicleTypeId = vehicleTypeId;
        SizeId = sizeId;
        UnitPrice = unitPrice;
        TotalCommission = totalCommission;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>Snapshot of the vehicle type used in the pricing/commission matrix lookup.</summary>
    public string VehicleTypeId { get; set; } = string.Empty;

    /// <summary>Snapshot of the vehicle size used in the pricing/commission matrix lookup.</summary>
    public string SizeId { get; set; } = string.Empty;

    /// <summary>
    /// Resolved price for this service after pricing matrix lookup and modifier application.
    /// Falls back to <c>Service.BasePrice</c> when no matrix row exists. Precision (10, 2).
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total commission pool for this line item (before employee split). Precision (10, 2).
    /// Zero when no commission matrix row exists for this combination.
    /// </summary>
    public decimal TotalCommission { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Transaction Transaction { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public VehicleType VehicleType { get; set; } = null!;
    public Size Size { get; set; } = null!;

    /// <summary>Per-employee commission split records. Cascade delete.</summary>
    public ICollection<ServiceEmployeeAssignment> EmployeeAssignments { get; set; } = [];
}
