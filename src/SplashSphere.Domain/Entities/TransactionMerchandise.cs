namespace SplashSphere.Domain.Entities;

/// <summary>
/// A merchandise line item within a <see cref="Transaction"/>.
/// <para>
/// <see cref="UnitPrice"/> is a snapshot of <c>Merchandise.Price</c> at the time of sale
/// so that future price changes do not alter historical revenue figures.
/// </para>
/// Inventory (<c>Merchandise.StockQuantity</c>) is decremented on transaction completion.
/// A sale is blocked at creation time if the requested <see cref="Quantity"/> exceeds
/// available stock (enforced in <c>CreateTransactionCommandHandler</c> Step 5).
/// <para>
/// Cascade delete: deleted when parent <see cref="Transaction"/> is deleted.
/// Inventory is <b>not</b> restored on cascade delete — use the Refund or Cancel flow.
/// </para>
/// </summary>
public sealed class TransactionMerchandise : IAuditableEntity, ITenantScoped
{
    private TransactionMerchandise() { } // EF Core

    public TransactionMerchandise(
        string tenantId,
        string transactionId,
        string merchandiseId,
        int quantity,
        decimal unitPrice)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        TransactionId = transactionId;
        MerchandiseId = merchandiseId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string MerchandiseId { get; set; } = string.Empty;

    /// <summary>Number of units sold. Must be ≥ 1 and ≤ available stock.</summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Snapshot of <c>Merchandise.Price</c> at sale time. Precision (10, 2).
    /// </summary>
    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Computed ─────────────────────────────────────────────────────────────

    /// <summary>Line total: <c>Quantity × UnitPrice</c>.</summary>
    public decimal LineTotal => Quantity * UnitPrice;

    // ── Navigations ──────────────────────────────────────────────────────────

    public Transaction Transaction { get; set; } = null!;
    public Merchandise Merchandise { get; set; } = null!;
}
