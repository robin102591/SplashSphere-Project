namespace SplashSphere.Domain.Entities;

/// <summary>
/// A line item on a <see cref="PurchaseOrder"/>. Each line references either a
/// <see cref="SupplyItem"/> or a <see cref="Merchandise"/> item (or neither, for
/// ad-hoc items not yet in the system). <see cref="ItemName"/> is denormalised
/// for display purposes.
/// <para>
/// On receiving goods, <see cref="ReceivedQuantity"/> is incremented and a
/// <see cref="StockMovement"/> of type <c>PurchaseIn</c> is created.
/// </para>
/// </summary>
public sealed class PurchaseOrderLine : IAuditableEntity
{
    private PurchaseOrderLine() { } // EF Core

    public PurchaseOrderLine(string purchaseOrderId, string itemName, decimal quantity, decimal unitCost)
    {
        Id = Guid.NewGuid().ToString();
        PurchaseOrderId = purchaseOrderId;
        ItemName = itemName;
        Quantity = quantity;
        UnitCost = unitCost;
        TotalCost = quantity * unitCost;
    }

    public string Id { get; set; } = string.Empty;
    public string PurchaseOrderId { get; set; } = string.Empty;

    /// <summary>Set when this line is for an internal supply item.</summary>
    public string? SupplyItemId { get; set; }

    /// <summary>Set when this line is for a merchandise (retail) item.</summary>
    public string? MerchandiseId { get; set; }

    /// <summary>Denormalised item name for display, copied from the linked item at creation time.</summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>Ordered quantity.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Quantity received so far. Incremented as goods arrive.</summary>
    public decimal ReceivedQuantity { get; set; }

    /// <summary>Cost per unit on this order. Precision (10, 2).</summary>
    public decimal UnitCost { get; set; }

    /// <summary>Quantity multiplied by UnitCost. Precision (12, 2).</summary>
    public decimal TotalCost { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public SupplyItem? SupplyItem { get; set; }
    public Merchandise? Merchandise { get; set; }
}
