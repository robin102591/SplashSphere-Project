using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Connect.Loyalty;

/// <summary>
/// Customer-facing membership summary for a single tenant. Returned by
/// <c>GET /api/v1/connect/carwashes/{tenantId}/loyalty</c>.
/// <para>
/// <see cref="IsEnrolled"/> is false when the tenant does not offer loyalty
/// (feature not on their plan) or when the customer has not yet been issued
/// a membership card — all other fields are then zero / null.
/// </para>
/// </summary>
public sealed record ConnectMembershipDto(
    bool IsEnrolled,
    string? MembershipCardId,
    string? CardNumber,
    LoyaltyTier CurrentTier,
    string TierName,
    int PointsBalance,
    int LifetimePointsEarned,
    int LifetimePointsRedeemed,
    int? PointsToNextTier,
    string? NextTierName,
    decimal TierMultiplier);

/// <summary>
/// A reward a customer could redeem at a tenant. Mirrors
/// <see cref="Application.Features.Loyalty.LoyaltyRewardDto"/> but trimmed to
/// what the Connect app actually displays plus an <see cref="IsAffordable"/>
/// flag based on the caller's current balance.
/// </summary>
public sealed record ConnectRewardDto(
    string Id,
    string Name,
    string? Description,
    RewardType RewardType,
    int PointsCost,
    string? ServiceId,
    string? ServiceName,
    string? PackageId,
    string? PackageName,
    decimal? DiscountAmount,
    decimal? DiscountPercent,
    bool IsAffordable);

/// <summary>A single row in the Connect "Points history" screen.</summary>
public sealed record ConnectPointTransactionDto(
    string Id,
    PointTransactionType Type,
    int Points,
    int BalanceAfter,
    string Description,
    string? RewardName,
    DateTime CreatedAt);

/// <summary>
/// Result returned after a successful reward redemption.
/// The customer presents <see cref="PointTransactionId"/> (or a derived QR) to the cashier.
/// </summary>
public sealed record ConnectRedemptionResultDto(
    string PointTransactionId,
    string RewardId,
    string RewardName,
    int PointsDeducted,
    int NewBalance);
