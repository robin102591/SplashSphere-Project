namespace SplashSphere.Domain.Entities;

/// <summary>
/// A car wash service offered by a tenant (e.g. "Basic Exterior Wash", "Full Detail").
/// Pricing is looked up from a <see cref="ServicePricing"/> matrix keyed on
/// (vehicleTypeId, sizeId). If no matrix row exists, <see cref="BasePrice"/> is used
/// as the fallback. Commissions come from a separate <see cref="ServiceCommission"/> matrix.
/// </summary>
public sealed class Service : IAuditableEntity, ITenantScoped
{
    private Service() { } // EF Core

    public Service(string tenantId, string categoryId, string name, decimal basePrice, string? description = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        BasePrice = basePrice;
        Description = description;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Fallback price used when no <see cref="ServicePricing"/> row matches
    /// the vehicle type + size combination.
    /// Stored in PHP (Philippine Peso), precision (10, 2).
    /// </summary>
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public ServiceCategory Category { get; set; } = null!;
    public ICollection<ServicePricing> Pricing { get; set; } = [];
    public ICollection<ServiceCommission> Commissions { get; set; } = [];
    public ICollection<PackageService> PackageServices { get; set; } = [];
    public ICollection<TransactionService> TransactionServices { get; set; } = [];
    public ICollection<ServiceSupplyUsage> SupplyUsages { get; set; } = [];
}
