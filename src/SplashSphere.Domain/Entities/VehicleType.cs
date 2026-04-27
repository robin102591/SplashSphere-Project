namespace SplashSphere.Domain.Entities;

/// <summary>
/// Classifies a vehicle by body type (e.g. Sedan, SUV, Van, Truck, Motorcycle).
/// Together with <see cref="Size"/>, it forms the two-dimensional key used to
/// look up <c>ServicePricing</c> and <c>ServiceCommission</c> matrix entries.
/// Scoped per tenant so operators can customise the list.
/// </summary>
public sealed class VehicleType : IAuditableEntity, ITenantScoped
{
    private VehicleType() { } // EF Core

    public VehicleType(string tenantId, string name)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Name = name;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
}
