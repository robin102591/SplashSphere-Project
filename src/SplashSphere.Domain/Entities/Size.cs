namespace SplashSphere.Domain.Entities;

/// <summary>
/// Classifies a vehicle by physical size (e.g. Small, Medium, Large, XL).
/// Together with <see cref="VehicleType"/>, it forms the two-dimensional key used to
/// look up <c>ServicePricing</c> and <c>ServiceCommission</c> matrix entries.
/// Scoped per tenant.
/// </summary>
public sealed class Size : IAuditableEntity, ITenantScoped
{
    private Size() { } // EF Core

    public Size(string tenantId, string name)
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
    public ICollection<ServiceSupplyUsage> ServiceSupplyUsages { get; set; } = [];
}
