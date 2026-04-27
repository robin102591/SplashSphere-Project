using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// A reward that customers can redeem using loyalty points.
/// Rewards can grant a free service/package or a peso/percentage discount.
/// </summary>
public sealed class LoyaltyReward : IAuditableEntity, ITenantScoped
{
    private LoyaltyReward() { } // EF Core

    public LoyaltyReward(
        string tenantId,
        string name,
        RewardType rewardType,
        int pointsCost)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        Name = name;
        RewardType = rewardType;
        PointsCost = pointsCost;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public RewardType RewardType { get; set; }

    /// <summary>Number of points required to redeem this reward.</summary>
    public int PointsCost { get; set; }

    /// <summary>FK to Service when <see cref="RewardType"/> is FreeService.</summary>
    public string? ServiceId { get; set; }

    /// <summary>FK to ServicePackage when <see cref="RewardType"/> is FreePackage.</summary>
    public string? PackageId { get; set; }

    /// <summary>Peso amount off for DiscountAmount type.</summary>
    public decimal? DiscountAmount { get; set; }

    /// <summary>Percentage off (0-100) for DiscountPercent type.</summary>
    public decimal? DiscountPercent { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Service? Service { get; set; }
    public ServicePackage? Package { get; set; }
}
