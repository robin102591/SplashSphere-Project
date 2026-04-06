using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Events;

/// <summary>Raised after loyalty points are awarded from a completed transaction.</summary>
public sealed record PointsEarnedEvent(
    string MembershipCardId,
    string TenantId,
    string BranchId,
    string CustomerId,
    int PointsEarned,
    int NewBalance,
    string TransactionId
) : DomainEventBase;

/// <summary>Raised after a customer redeems loyalty points for a reward.</summary>
public sealed record PointsRedeemedEvent(
    string MembershipCardId,
    string TenantId,
    string BranchId,
    int PointsRedeemed,
    int NewBalance,
    string RewardId,
    string TransactionId
) : DomainEventBase;

/// <summary>Raised when a membership card's tier upgrades due to lifetime point accumulation.</summary>
public sealed record TierUpgradedEvent(
    string MembershipCardId,
    string TenantId,
    string CustomerId,
    LoyaltyTier PreviousTier,
    LoyaltyTier NewTier
) : DomainEventBase;
