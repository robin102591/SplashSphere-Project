namespace SplashSphere.Domain.Subscription;

/// <summary>
/// Immutable definition of what a plan tier includes — features, limits, and pricing.
/// These are static config, not stored in the database.
/// </summary>
public sealed class PlanDefinition
{
    public required Enums.PlanTier Tier { get; init; }
    public required string Name { get; init; }
    public required decimal MonthlyPrice { get; init; }
    public required int MaxBranches { get; init; }
    public required int MaxEmployees { get; init; }
    public required int SmsPerMonth { get; init; }

    /// <summary>
    /// Maximum POS workstations per branch. The customer-display feature pairs
    /// one display to one station, so this also caps how many independent
    /// counter screens a branch can run.
    /// </summary>
    public required int MaxPosStationsPerBranch { get; init; }

    /// <summary>
    /// Maximum number of rotating promo messages the idle customer-display can
    /// cycle through. Higher tiers get more variety; Starter gets one to keep
    /// the screen branded but not crowded.
    /// </summary>
    public required int MaxPromoMessages { get; init; }

    public required HashSet<string> Features { get; init; }
}
