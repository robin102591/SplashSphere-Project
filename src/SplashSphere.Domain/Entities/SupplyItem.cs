namespace SplashSphere.Domain.Entities;

/// <summary>
/// An internal consumable supply used during car wash services
/// (e.g. car shampoo, microfiber towels, tire black).
/// Unlike <see cref="Merchandise"/> (sold to customers), supply items are consumed
/// internally and tracked via <see cref="StockMovement"/> records.
/// <para>
/// Each item is branch-scoped — different branches maintain independent stock levels.
/// <see cref="CurrentStock"/> is updated as movements are recorded.
/// </para>
/// </summary>
public sealed class SupplyItem : IAuditableEntity
{
    private SupplyItem() { } // EF Core

    public SupplyItem(
        string tenantId,
        string branchId,
        string name,
        string unit,
        string? categoryId = null,
        decimal? reorderLevel = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
        Name = name;
        Unit = unit;
        CategoryId = categoryId;
        ReorderLevel = reorderLevel;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Unit of measure — e.g. "Liters", "Pieces", "Gallons".</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Current on-hand stock in <see cref="Unit"/>s. Updated by stock movements.</summary>
    public decimal CurrentStock { get; set; }

    /// <summary>When <see cref="CurrentStock"/> falls to or below this level, <see cref="IsLowStock"/> is true.</summary>
    public decimal? ReorderLevel { get; set; }

    /// <summary>Weighted average cost per unit, recalculated on each PurchaseIn movement. Precision (10, 2).</summary>
    public decimal AverageUnitCost { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Computed ─────────────────────────────────────────────────────────────

    /// <summary>
    /// True when on-hand stock is at or below <see cref="ReorderLevel"/>.
    /// Not mapped to a database column — calculated in memory.
    /// </summary>
    public bool IsLowStock => ReorderLevel.HasValue && CurrentStock <= ReorderLevel.Value;

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public SupplyCategory? Category { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = [];
    public ICollection<ServiceSupplyUsage> ServiceUsages { get; set; } = [];
}
