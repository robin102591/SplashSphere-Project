namespace SplashSphere.Domain.Entities;

/// <summary>
/// A vendor or supplier from whom the tenant purchases supplies and merchandise.
/// Linked to <see cref="PurchaseOrder"/>s for procurement tracking.
/// Scoped per tenant.
/// </summary>
public sealed class Supplier : IAuditableEntity
{
    private Supplier() { } // EF Core

    public Supplier(string tenantId, string name)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Name = name;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
}
