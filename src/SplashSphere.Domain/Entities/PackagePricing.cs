namespace SplashSphere.Domain.Entities;

/// <summary>
/// One cell in the package pricing matrix: the bundled price for a specific
/// (package, vehicleType, size) combination.
/// Unique constraint: (packageId, vehicleTypeId, sizeId).
/// </summary>
public sealed class PackagePricing : IAuditableEntity
{
    private PackagePricing() { } // EF Core

    public PackagePricing(string tenantId, string packageId, string vehicleTypeId, string sizeId, decimal price)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        PackageId = packageId;
        VehicleTypeId = vehicleTypeId;
        SizeId = sizeId;
        Price = price;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string SizeId { get; set; } = string.Empty;

    /// <summary>Bundled PHP price for this vehicle type + size cell. Precision (10, 2).</summary>
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public ServicePackage Package { get; set; } = null!;
    public VehicleType VehicleType { get; set; } = null!;
    public Size Size { get; set; } = null!;
}
