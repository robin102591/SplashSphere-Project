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
    public required HashSet<string> Features { get; init; }
}
