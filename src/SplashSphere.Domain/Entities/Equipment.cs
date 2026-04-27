using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// A physical piece of equipment at a branch (e.g. pressure washer, vacuum,
/// foam cannon, air compressor). Tracks purchase info, warranty, and current
/// operational status. Maintenance activities are logged via <see cref="MaintenanceLog"/>.
/// </summary>
public sealed class Equipment : IAuditableEntity, ITenantScoped
{
    private Equipment() { } // EF Core

    public Equipment(string tenantId, string branchId, string name)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
        Name = name;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Operational;
    public DateTime? PurchaseDate { get; set; }

    /// <summary>Original purchase cost in PHP. Precision (10, 2).</summary>
    public decimal? PurchaseCost { get; set; }

    public DateTime? WarrantyExpiry { get; set; }

    /// <summary>Physical location within the branch — e.g. "Bay 1", "Detailing area".</summary>
    public string? Location { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = [];
}
