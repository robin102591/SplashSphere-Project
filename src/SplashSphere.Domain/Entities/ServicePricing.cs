namespace SplashSphere.Domain.Entities;

/// <summary>
/// One cell in the service pricing matrix: the price for a specific
/// (service, vehicleType, size) combination.
/// Unique constraint: (serviceId, vehicleTypeId, sizeId).
/// Tenant-scoped for global query filter isolation.
/// </summary>
public sealed class ServicePricing : IAuditableEntity
{
    private ServicePricing() { } // EF Core

    public ServicePricing(string tenantId, string serviceId, string vehicleTypeId, string sizeId, decimal price)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        ServiceId = serviceId;
        VehicleTypeId = vehicleTypeId;
        SizeId = sizeId;
        Price = price;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string SizeId { get; set; } = string.Empty;

    /// <summary>PHP price for this vehicle type + size cell. Precision (10, 2).</summary>
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Service Service { get; set; } = null!;
    public VehicleType VehicleType { get; set; } = null!;
    public Size Size { get; set; } = null!;
}
