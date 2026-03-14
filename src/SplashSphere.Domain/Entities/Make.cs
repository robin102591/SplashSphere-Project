namespace SplashSphere.Domain.Entities;

/// <summary>
/// Vehicle manufacturer / brand (e.g. Toyota, Honda, Mitsubishi).
/// Each <see cref="Make"/> owns a set of <see cref="Model"/> records.
/// Scoped per tenant so operators can add local or lesser-known brands.
/// </summary>
public sealed class Make : IAuditableEntity
{
    private Make() { } // EF Core

    public Make(string tenantId, string name)
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
    public ICollection<Model> Models { get; set; } = [];
}
