namespace SplashSphere.Domain.Entities;

/// <summary>
/// A specific vehicle model under a <see cref="Make"/> (e.g. Vios under Toyota).
/// Used during customer/car registration to classify the vehicle.
/// Scoped per tenant via the parent <see cref="Make"/>.
/// </summary>
public sealed class Model : IAuditableEntity
{
    private Model() { } // EF Core

    public Model(string tenantId, string makeId, string name)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        MakeId = makeId;
        Name = name;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string MakeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Make Make { get; set; } = null!;
}
