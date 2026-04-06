namespace SplashSphere.Domain.Enums;

/// <summary>
/// Loyalty program membership tiers. Higher tiers earn points faster
/// via configurable multipliers set in <see cref="Entities.LoyaltyTierConfig"/>.
/// Tier progression is one-directional (only upgrades).
/// </summary>
public enum LoyaltyTier
{
    Standard = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3,
}
