namespace SplashSphere.Domain.Entities;

/// <summary>
/// Groups <see cref="SupplyItem"/>s into logical categories
/// (e.g. "Chemicals", "Towels &amp; Cloths", "Wax &amp; Polish").
/// Scoped per tenant.
/// </summary>
public sealed class SupplyCategory : IAuditableEntity, ITenantScoped
{
    private SupplyCategory() { } // EF Core

    public SupplyCategory(string tenantId, string name, string? description = null)
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
    public ICollection<SupplyItem> Items { get; set; } = [];
}
