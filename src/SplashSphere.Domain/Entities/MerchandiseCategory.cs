namespace SplashSphere.Domain.Entities;

/// <summary>
/// Groups related merchandise items sold at the POS
/// (e.g. "Car Care Products", "Accessories", "Air Fresheners").
/// Scoped per tenant.
/// </summary>
public sealed class MerchandiseCategory : IAuditableEntity, ITenantScoped
{
    private MerchandiseCategory() { } // EF Core

    public MerchandiseCategory(string tenantId, string name, string? description = null)
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
    public ICollection<Merchandise> Items { get; set; } = [];
}
