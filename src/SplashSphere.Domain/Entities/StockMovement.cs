using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// An immutable audit record of a stock change for either a <see cref="SupplyItem"/>
/// or a <see cref="Merchandise"/> item. Exactly one of <see cref="SupplyItemId"/> or
/// <see cref="MerchandiseId"/> is set — never both, never neither.
/// <para>
/// <see cref="Quantity"/> is always positive; the <see cref="Type"/> determines
/// whether stock increases ("In" types) or decreases ("Out" types).
/// </para>
/// </summary>
public sealed class StockMovement : IAuditableEntity
{
    private StockMovement() { } // EF Core

    public StockMovement(string tenantId, string branchId, MovementType type, decimal quantity)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
        Type = type;
        Quantity = quantity;
        MovementDate = DateTime.UtcNow;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;

    /// <summary>Set when this movement relates to an internal supply item.</summary>
    public string? SupplyItemId { get; set; }

    /// <summary>Set when this movement relates to a merchandise (retail) item.</summary>
    public string? MerchandiseId { get; set; }

    public MovementType Type { get; set; }

    /// <summary>Always positive — direction is determined by <see cref="Type"/>.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Cost per unit at the time of movement. Precision (10, 2).</summary>
    public decimal? UnitCost { get; set; }

    /// <summary>Quantity multiplied by UnitCost. Precision (12, 2).</summary>
    public decimal? TotalCost { get; set; }

    /// <summary>External reference such as PO number, transaction number, or transfer ID.</summary>
    public string? Reference { get; set; }

    public string? Notes { get; set; }

    /// <summary>The user who performed or authorised this movement.</summary>
    public string? PerformedByUserId { get; set; }

    public DateTime MovementDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public SupplyItem? SupplyItem { get; set; }
    public Merchandise? Merchandise { get; set; }
}
