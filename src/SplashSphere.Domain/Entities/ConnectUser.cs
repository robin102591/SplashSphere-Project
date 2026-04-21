namespace SplashSphere.Domain.Entities;

/// <summary>
/// Global customer identity in SplashSphere Connect (the customer-facing app).
/// <para>
/// Unlike <see cref="Customer"/>, a <c>ConnectUser</c> is <b>not</b> tenant-scoped —
/// it represents the customer's global profile across every SplashSphere-powered car wash.
/// The <see cref="Phone"/> is the primary global identifier (unique across all tenants);
/// per-tenant <see cref="Customer"/> records are reached via <see cref="TenantLinks"/>.
/// </para>
/// </summary>
public sealed class ConnectUser : IAuditableEntity
{
    private ConnectUser() { } // EF Core

    public ConnectUser(string phone, string name, string? email = null, string? avatarUrl = null)
    {
        Id = Guid.NewGuid().ToString();
        Phone = phone;
        Name = name;
        Email = email;
        AvatarUrl = avatarUrl;
    }

    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Philippine mobile number stored in canonical form (e.g. "+639171234567").
    /// Globally unique — acts as the cross-tenant customer identity.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    /// <summary>Links this global identity to each tenant's <see cref="Customer"/> record.</summary>
    public ICollection<ConnectUserTenantLink> TenantLinks { get; set; } = [];

    /// <summary>Global vehicle registry owned by this customer.</summary>
    public ICollection<ConnectVehicle> Vehicles { get; set; } = [];
}
