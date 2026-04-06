using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Defines one loyalty tier's thresholds and benefits within a tenant's loyalty program.
/// Child of <see cref="LoyaltyProgramSettings"/>. Each tenant can customise tier names,
/// point thresholds, and earning multipliers.
/// </summary>
public sealed class LoyaltyTierConfig : IAuditableEntity
{
    private LoyaltyTierConfig() { } // EF Core

    public LoyaltyTierConfig(
        string tenantId,
        string loyaltyProgramSettingsId,
        LoyaltyTier tier,
        string name,
        int minimumLifetimePoints,
        decimal pointsMultiplier)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        LoyaltyProgramSettingsId = loyaltyProgramSettingsId;
        Tier = tier;
        Name = name;
        MinimumLifetimePoints = minimumLifetimePoints;
        PointsMultiplier = pointsMultiplier;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string LoyaltyProgramSettingsId { get; set; } = string.Empty;

    /// <summary>The enum tier this config row represents.</summary>
    public LoyaltyTier Tier { get; set; }

    /// <summary>Customisable display name (e.g. "Gold Member").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Lifetime points a customer must accumulate to reach this tier.</summary>
    public int MinimumLifetimePoints { get; set; }

    /// <summary>Multiplier applied to base points earned (e.g. 1.5 = 50% bonus).</summary>
    public decimal PointsMultiplier { get; set; } = 1;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public LoyaltyProgramSettings Settings { get; set; } = null!;
}
