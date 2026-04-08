using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Contract between a Franchisor and a Franchisee tenant.
/// Bridges two tenants — excluded from global tenant query filters.
/// </summary>
public sealed class FranchiseAgreement : IAuditableEntity
{
    private FranchiseAgreement() { } // EF Core

    public FranchiseAgreement(string franchisorTenantId, string franchiseeTenantId, string territoryName)
    {
        Id = Guid.NewGuid().ToString();
        FranchisorTenantId = franchisorTenantId;
        FranchiseeTenantId = franchiseeTenantId;
        TerritoryName = territoryName;
    }

    public string Id { get; set; } = string.Empty;
    public string FranchisorTenantId { get; set; } = string.Empty;
    public string FranchiseeTenantId { get; set; } = string.Empty;
    public string AgreementNumber { get; set; } = string.Empty;

    // ── Territory ────────────────────────────────────────────────────────────
    public string TerritoryName { get; set; } = string.Empty;
    public string? TerritoryDescription { get; set; }
    public bool ExclusiveTerritory { get; set; }

    // ── Contract terms ───────────────────────────────────────────────────────
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal InitialFranchiseFee { get; set; }
    public AgreementStatus Status { get; set; } = AgreementStatus.Draft;

    // ── Customized rates (override FranchiseSettings if needed) ──────────────
    public decimal? CustomRoyaltyRate { get; set; }
    public decimal? CustomMarketingFeeRate { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────
    public Tenant FranchisorTenant { get; set; } = null!;
    public Tenant FranchiseeTenant { get; set; } = null!;
    public ICollection<RoyaltyPeriod> RoyaltyPeriods { get; set; } = [];
}
