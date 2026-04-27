namespace SplashSphere.Domain.Entities;

/// <summary>
/// A bundle of multiple <see cref="Service"/> items sold together at a discounted price.
/// Like individual services, packages have their own pricing matrix (<see cref="PackagePricing"/>)
/// and commission matrix (<see cref="PackageCommission"/>), but package commissions are
/// always <see cref="CommissionType.Percentage"/> — never fixed or hybrid.
/// </summary>
public sealed class ServicePackage : IAuditableEntity, ITenantScoped
{
    private ServicePackage() { } // EF Core

    public ServicePackage(string tenantId, string name, string? description = null)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Name = name;
        Description = description;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public ICollection<PackageService> PackageServices { get; set; } = [];
    public ICollection<PackagePricing> Pricing { get; set; } = [];
    public ICollection<PackageCommission> Commissions { get; set; } = [];
    public ICollection<TransactionPackage> TransactionPackages { get; set; } = [];
}
