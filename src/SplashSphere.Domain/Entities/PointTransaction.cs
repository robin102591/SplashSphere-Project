using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// An append-only ledger entry recording a point movement on a <see cref="MembershipCard"/>.
/// Positive <see cref="Points"/> for earning, negative for redemption/expiry/adjustment.
/// <see cref="BalanceAfter"/> enables auditable reconciliation.
/// </summary>
public sealed class PointTransaction
{
    private PointTransaction() { } // EF Core

    public PointTransaction(
        string tenantId,
        string membershipCardId,
        PointTransactionType type,
        int points,
        int balanceAfter,
        string description)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        MembershipCardId = membershipCardId;
        Type = type;
        Points = points;
        BalanceAfter = balanceAfter;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string MembershipCardId { get; set; } = string.Empty;

    public PointTransactionType Type { get; set; }

    /// <summary>Positive for earn/adjustment-add, negative for redeem/expire/adjustment-deduct.</summary>
    public int Points { get; set; }

    /// <summary>Running balance after this entry was applied.</summary>
    public int BalanceAfter { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>The car wash transaction that triggered this point movement, if any.</summary>
    public string? TransactionId { get; set; }

    /// <summary>The reward redeemed, if this is a redemption entry.</summary>
    public string? RewardId { get; set; }

    /// <summary>When these points expire (null = no expiry).</summary>
    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public MembershipCard MembershipCard { get; set; } = null!;
    public Transaction? Transaction { get; set; }
    public LoyaltyReward? Reward { get; set; }
}
