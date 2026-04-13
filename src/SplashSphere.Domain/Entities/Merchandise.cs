namespace SplashSphere.Domain.Entities;

/// <summary>
/// A physical product sold at the POS alongside car wash services
/// (e.g. air fresheners, wax, tire shine spray).
/// <para>
/// Uniqueness: <see cref="Sku"/> is unique per tenant
/// (composite unique index on [Sku, TenantId]).
/// </para>
/// Inventory is decremented when a transaction reaches
/// <see cref="TransactionStatus.Completed"/>; a sale is blocked if
/// <see cref="StockQuantity"/> would go below zero.
/// </para>
/// </summary>
public sealed class Merchandise : IAuditableEntity
{
    private Merchandise() { } // EF Core

    public Merchandise(
        string tenantId,
        string name,
        string sku,
        decimal price,
        int stockQuantity,
        int lowStockThreshold,
        string? categoryId = null,
        string? description = null,
        decimal? costPrice = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        Sku = sku.Trim().ToUpperInvariant();
        Description = description;
        Price = price;
        CostPrice = costPrice;
        StockQuantity = stockQuantity;
        LowStockThreshold = lowStockThreshold;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Nullable — merchandise may be uncategorised.</summary>
    public string? CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stock-keeping unit — trimmed and uppercased on assignment.
    /// Composite unique with <see cref="TenantId"/>.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Retail selling price in PHP. Precision (10, 2).</summary>
    public decimal Price { get; set; }

    /// <summary>Optional purchase/cost price in PHP for margin reporting. Precision (10, 2).</summary>
    public decimal? CostPrice { get; set; }

    /// <summary>
    /// Current on-hand stock count.
    /// Decremented on transaction completion; never goes below zero.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// When <see cref="StockQuantity"/> reaches or falls below this value,
    /// <c>CheckLowStockAlerts</c> fires a <c>LowStockAlertEvent</c>.
    /// </summary>
    public int LowStockThreshold { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Computed ─────────────────────────────────────────────────────────────

    /// <summary>
    /// True when on-hand stock is at or below <see cref="LowStockThreshold"/>.
    /// Surfaced as a flag in list/detail DTOs; also drives the daily alert job.
    /// Not mapped to a database column — calculated in memory.
    /// </summary>
    public bool IsLowStock => StockQuantity <= LowStockThreshold;

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public MerchandiseCategory? Category { get; set; }
    public ICollection<TransactionMerchandise> TransactionMerchandise { get; set; } = [];
    public ICollection<StockMovement> StockMovements { get; set; } = [];
}
