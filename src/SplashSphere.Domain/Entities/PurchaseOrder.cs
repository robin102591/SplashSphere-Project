using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// A purchase order issued to a <see cref="Supplier"/> for restocking supplies or merchandise.
/// PO numbers follow the format <c>PO-{YYYY}-{Sequence}</c>.
/// <para>
/// Lifecycle: <see cref="PurchaseOrderStatus.Draft"/> → <see cref="PurchaseOrderStatus.Sent"/>
/// → <see cref="PurchaseOrderStatus.PartiallyReceived"/> → <see cref="PurchaseOrderStatus.Received"/>.
/// Can be <see cref="PurchaseOrderStatus.Cancelled"/> from Draft or Sent.
/// </para>
/// </summary>
public sealed class PurchaseOrder : IAuditableEntity, ITenantScoped
{
    private PurchaseOrder() { } // EF Core

    public PurchaseOrder(string tenantId, string branchId, string supplierId, string poNumber)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
        SupplierId = supplierId;
        PoNumber = poNumber;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;

    /// <summary>Human-readable PO identifier: <c>PO-{YYYY}-{Sequence}</c>.</summary>
    public string PoNumber { get; set; } = string.Empty;

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    /// <summary>Sum of all line totals. Precision (12, 2).</summary>
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseOrderLine> Lines { get; set; } = [];
}
