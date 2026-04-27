namespace SplashSphere.Domain.Entities;

/// <summary>
/// Defines how much of a <see cref="SupplyItem"/> is consumed each time a
/// <see cref="Service"/> is performed. Optionally scoped by <see cref="Size"/>
/// (e.g. a Large vehicle uses more shampoo than a Small one).
/// When <see cref="SizeId"/> is null, <see cref="QuantityPerUse"/> is the default
/// for all sizes that do not have a specific override.
/// <para>
/// Used during transaction completion to auto-generate <c>UsageOut</c>
/// <see cref="StockMovement"/> records and decrement <see cref="SupplyItem.CurrentStock"/>.
/// </para>
/// </summary>
public sealed class ServiceSupplyUsage : ITenantScoped
{
    private ServiceSupplyUsage() { } // EF Core

    public ServiceSupplyUsage(
        string tenantId,
        string serviceId,
        string supplyItemId,
        string? sizeId,
        decimal quantityPerUse)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        ServiceId = serviceId;
        SupplyItemId = supplyItemId;
        SizeId = sizeId;
        QuantityPerUse = quantityPerUse;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string SupplyItemId { get; set; } = string.Empty;

    /// <summary>Nullable — when null, this is the default usage for all vehicle sizes.</summary>
    public string? SizeId { get; set; }

    /// <summary>Amount of supply consumed per service execution, in <see cref="SupplyItem.Unit"/>s.</summary>
    public decimal QuantityPerUse { get; set; }

    public string? Notes { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Service Service { get; set; } = null!;
    public SupplyItem SupplyItem { get; set; } = null!;
    public Size? Size { get; set; }
}
