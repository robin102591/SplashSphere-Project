namespace SplashSphere.Domain.Entities;

/// <summary>
/// Groups related car wash services for display and reporting purposes
/// (e.g. "Exterior Wash", "Interior Detailing", "Engine Treatment").
/// Scoped per tenant.
/// </summary>
public sealed class ServiceCategory : IAuditableEntity, ITenantScoped
{
    private ServiceCategory() { } // EF Core

    public ServiceCategory(string tenantId, string name, string? description = null)
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
    public ICollection<Service> Services { get; set; } = [];
}
