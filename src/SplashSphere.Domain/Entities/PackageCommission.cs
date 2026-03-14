namespace SplashSphere.Domain.Entities;

/// <summary>
/// One cell in the package commission matrix: the commission percentage for a specific
/// (package, vehicleType, size) combination.
/// <para>
/// Package commissions are <b>always percentage-based</b> — there is no fixed or hybrid
/// option for packages (unlike <see cref="ServiceCommission"/>).
/// Formula: <c>packagePrice × PercentageRate / 100</c>, then split equally among assigned employees.
/// </para>
/// Unique constraint: (packageId, vehicleTypeId, sizeId).
/// </summary>
public sealed class PackageCommission : IAuditableEntity
{
    private PackageCommission() { } // EF Core

    public PackageCommission(
        string tenantId,
        string packageId,
        string vehicleTypeId,
        string sizeId,
        decimal percentageRate)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        PackageId = packageId;
        VehicleTypeId = vehicleTypeId;
        SizeId = sizeId;
        PercentageRate = percentageRate;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string VehicleTypeId { get; set; } = string.Empty;
    public string SizeId { get; set; } = string.Empty;

    /// <summary>
    /// Percentage rate (e.g. 12.00 means 12% of the package price).
    /// Precision (5, 2).
    /// </summary>
    public decimal PercentageRate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public ServicePackage Package { get; set; } = null!;
    public VehicleType VehicleType { get; set; } = null!;
    public Size Size { get; set; } = null!;
}
