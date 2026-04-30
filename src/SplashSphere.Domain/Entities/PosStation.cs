using SplashSphere.Domain.Interfaces;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// A logical POS workstation: one cashier device + optional customer display.
/// A branch with multiple cashiers needs multiple stations so each
/// transaction stream stays paired to its own customer-facing display
/// (SignalR group <c>display:{branchId}:{stationId}</c>).
/// </summary>
public sealed class PosStation : IAuditableEntity, ITenantScoped
{
    private PosStation() { } // EF Core

    public PosStation(string tenantId, string branchId, string name)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        BranchId = branchId;
        Name = name;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;

    /// <summary>Display name shown in the cashier login picker (e.g. "Counter A").</summary>
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
