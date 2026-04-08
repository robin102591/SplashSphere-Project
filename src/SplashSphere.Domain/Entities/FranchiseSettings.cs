using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Franchise network configuration owned by a Franchisor tenant.
/// Defines default royalty rates, standardization controls, and network limits.
/// </summary>
public sealed class FranchiseSettings : IAuditableEntity
{
    private FranchiseSettings() { } // EF Core

    public FranchiseSettings(string tenantId)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    // ── Royalty configuration ────────────────────────────────────────────────
    public decimal RoyaltyRate { get; set; }
    public decimal MarketingFeeRate { get; set; }
    public decimal TechnologyFeeRate { get; set; }
    public RoyaltyBasis RoyaltyBasis { get; set; } = RoyaltyBasis.GrossRevenue;
    public RoyaltyFrequency RoyaltyFrequency { get; set; } = RoyaltyFrequency.Monthly;

    // ── Standardization controls ─────────────────────────────────────────────
    public bool EnforceStandardServices { get; set; }
    public bool EnforceStandardPricing { get; set; }
    public bool AllowLocalServices { get; set; }
    public decimal? MaxPriceVariance { get; set; }
    public bool EnforceBranding { get; set; }

    // ── Network defaults ─────────────────────────────────────────────────────
    public PlanTier DefaultFranchiseePlan { get; set; } = PlanTier.Growth;
    public int MaxBranchesPerFranchisee { get; set; } = 3;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────
    public Tenant Tenant { get; set; } = null!;
}
