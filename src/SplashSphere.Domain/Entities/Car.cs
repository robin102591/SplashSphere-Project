namespace SplashSphere.Domain.Entities;

/// <summary>
/// A vehicle registered in the system, identified primarily by its plate number.
/// <para>
/// Uniqueness: <see cref="PlateNumber"/> is unique per tenant
/// (composite unique index on [PlateNumber, TenantId]), allowing the same plate to exist
/// across separate tenant databases. A plain index on <see cref="PlateNumber"/> supports
/// the fast POS lookup endpoint: <c>GET /cars/lookup/{plateNumber}</c>.
/// </para>
/// <see cref="CustomerId"/> is nullable — a walk-in vehicle can be serviced and queued
/// without a registered customer record.
/// </summary>
public sealed class Car : IAuditableEntity
{
    private Car() { } // EF Core

    public Car(
        string tenantId,
        string vehicleTypeId,
        string sizeId,
        string plateNumber,
        string? customerId = null,
        string? makeId = null,
        string? modelId = null,
        string? color = null,
        int? year = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        CustomerId = customerId;
        VehicleTypeId = vehicleTypeId;
        SizeId = sizeId;
        MakeId = makeId;
        ModelId = modelId;
        // Philippine plates are conventionally uppercase (e.g. "ABC 1234")
        PlateNumber = plateNumber.ToUpperInvariant().Trim();
        Color = color;
        Year = year;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Nullable — walk-in vehicles can be serviced without a customer record.</summary>
    public string? CustomerId { get; set; }

    public string VehicleTypeId { get; set; } = string.Empty;
    public string SizeId { get; set; } = string.Empty;

    /// <summary>Nullable — make may be unknown for walk-in vehicles.</summary>
    public string? MakeId { get; set; }

    /// <summary>Nullable — model may be unknown or not in the tenant's list.</summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// LTO-issued plate number, stored uppercase and trimmed.
    /// Composite unique with <see cref="TenantId"/>; indexed for fast POS plate lookup.
    /// </summary>
    public string PlateNumber { get; set; } = string.Empty;

    public string? Color { get; set; }

    /// <summary>Model year (e.g. 2019). Nullable — may be unknown.</summary>
    public int? Year { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Customer? Customer { get; set; }
    public VehicleType VehicleType { get; set; } = null!;
    public Size Size { get; set; } = null!;
    public Make? Make { get; set; }
    public Model? Model { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<QueueEntry> QueueEntries { get; set; } = [];
}
