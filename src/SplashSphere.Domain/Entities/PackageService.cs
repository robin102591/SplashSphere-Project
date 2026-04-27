namespace SplashSphere.Domain.Entities;

/// <summary>
/// Join entity linking a <see cref="ServicePackage"/> to the individual
/// <see cref="Service"/> items it includes.
/// Unique constraint: (packageId, serviceId).
/// </summary>
public sealed class PackageService : IAuditableEntity, ITenantScoped
{
    private PackageService() { } // EF Core

    public PackageService(string tenantId, string packageId, string serviceId)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        PackageId = packageId;
        ServiceId = serviceId;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public ServicePackage Package { get; set; } = null!;
    public Service Service { get; set; } = null!;
}
