namespace SplashSphere.Domain.Entities;

/// <summary>
/// Customer's vehicle registered in the Connect app — <b>global</b> (not tenant-scoped).
/// <para>
/// <b>Intentionally lightweight:</b> carries make, model, plate, colour, and year only.
/// There is <b>no VehicleTypeId or SizeId</b> — the car wash cashier assigns those
/// classifications when the vehicle physically arrives (stored on the tenant's
/// <see cref="Car"/> entity). This prevents pricing fraud and matches real-world
/// operations where attendants visually assess each vehicle.
/// </para>
/// </summary>
public sealed class ConnectVehicle : IAuditableEntity
{
    private ConnectVehicle() { } // EF Core

    public ConnectVehicle(
        string connectUserId,
        string makeId,
        string modelId,
        string plateNumber,
        string? color = null,
        int? year = null)
    {
        Id = Guid.NewGuid().ToString();
        ConnectUserId = connectUserId;
        MakeId = makeId;
        ModelId = modelId;
        // PH plates are conventionally uppercase, trimmed
        PlateNumber = plateNumber.ToUpperInvariant().Trim();
        Color = color;
        Year = year;
    }

    public string Id { get; set; } = string.Empty;
    public string ConnectUserId { get; set; } = string.Empty;

    /// <summary>FK to <see cref="GlobalMake"/> — NOT the tenant-scoped <see cref="Make"/>.</summary>
    public string MakeId { get; set; } = string.Empty;

    /// <summary>FK to <see cref="GlobalModel"/> — NOT the tenant-scoped <see cref="Model"/>.</summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>LTO-issued plate number, stored uppercase and trimmed.</summary>
    public string PlateNumber { get; set; } = string.Empty;

    public string? Color { get; set; }
    public int? Year { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public ConnectUser ConnectUser { get; set; } = null!;
    public GlobalMake Make { get; set; } = null!;
    public GlobalModel Model { get; set; } = null!;
}
