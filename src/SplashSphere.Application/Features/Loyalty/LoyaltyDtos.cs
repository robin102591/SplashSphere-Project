using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Loyalty;

public sealed record LoyaltyProgramSettingsDto(
    string Id,
    decimal PointsPerCurrencyUnit,
    decimal CurrencyUnitAmount,
    bool IsActive,
    int? PointsExpirationMonths,
    bool AutoEnroll,
    IReadOnlyList<LoyaltyTierConfigDto> Tiers);

public sealed record LoyaltyTierConfigDto(
    string Id,
    LoyaltyTier Tier,
    string Name,
    int MinimumLifetimePoints,
    decimal PointsMultiplier);

public sealed record LoyaltyRewardDto(
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
    bool IsActive,
    DateTime CreatedAt);

public sealed record MembershipCardDto(
    string Id,
    string CustomerId,
    string CustomerName,
    string? CustomerEmail,
    string? CustomerPhone,
    string CardNumber,
    LoyaltyTier CurrentTier,
    string TierName,
    int PointsBalance,
    int LifetimePointsEarned,
    int LifetimePointsRedeemed,
    bool IsActive,
    DateTime CreatedAt);

public sealed record PointTransactionDto(
    string Id,
    PointTransactionType Type,
    int Points,
    int BalanceAfter,
    string Description,
    string? TransactionId,
    string? RewardName,
    DateTime CreatedAt);

public sealed record CustomerLoyaltySummaryDto(
    string MembershipCardId,
    string CardNumber,
    LoyaltyTier CurrentTier,
    string TierName,
    int PointsBalance,
    int LifetimePointsEarned,
    int? PointsToNextTier,
    string? NextTierName,
    IReadOnlyList<AvailableRewardDto> AvailableRewards);

public sealed record AvailableRewardDto(
    string Id,
    string Name,
    RewardType RewardType,
    int PointsCost,
    decimal? DiscountAmount,
    decimal? DiscountPercent);

public sealed record LoyaltyDashboardDto(
    int TotalMembers,
    int TotalPointsEarnedInPeriod,
    int TotalPointsRedeemedInPeriod,
    int TotalRedemptionsInPeriod,
    IReadOnlyList<TierDistributionDto> TierDistribution,
    IReadOnlyList<TopLoyalCustomerDto> TopCustomers);

public sealed record TierDistributionDto(
    LoyaltyTier Tier,
    string TierName,
    int Count);

public sealed record TopLoyalCustomerDto(
    string CustomerId,
    string CustomerName,
    string CardNumber,
    LoyaltyTier CurrentTier,
    int LifetimePointsEarned,
    int PointsBalance);
