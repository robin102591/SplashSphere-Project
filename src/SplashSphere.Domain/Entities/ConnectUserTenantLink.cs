namespace SplashSphere.Domain.Entities;

/// <summary>
/// Bridge between a global <see cref="ConnectUser"/> and a tenant's per-tenant
/// <see cref="Customer"/> record. Created when a customer joins a specific car wash
/// via the Connect app ("Join &amp; Book").
/// <para>
/// Composite unique on <c>(ConnectUserId, TenantId)</c> — a customer can only have
/// one active link per tenant.
/// </para>
/// </summary>
public sealed class ConnectUserTenantLink : IAuditableEntity
{
    private ConnectUserTenantLink() { } // EF Core

    public ConnectUserTenantLink(string connectUserId, string tenantId, string customerId)
    {
        Id = Guid.NewGuid().ToString();
        ConnectUserId = connectUserId;
        TenantId = tenantId;
        CustomerId = customerId;
        LinkedAt = DateTime.UtcNow;
    }

    public string Id { get; set; } = string.Empty;
    public string ConnectUserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    /// <summary>FK to the tenant's <see cref="Customer"/> record.</summary>
    public string CustomerId { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime LinkedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public ConnectUser ConnectUser { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
