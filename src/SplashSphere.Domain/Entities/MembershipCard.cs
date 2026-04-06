using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// A virtual loyalty membership card issued to a customer within a tenant.
/// One card per customer per tenant. Tracks current points balance, lifetime stats,
/// and tier progression.
/// </summary>
public sealed class MembershipCard : IAuditableEntity
{
    private MembershipCard() { } // EF Core

    public MembershipCard(
        string tenantId,
        string customerId,
        string cardNumber)
    {
        Id = Guid.NewGuid().ToString();
        TenantId = tenantId;
        CustomerId = customerId;
        CardNumber = cardNumber;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>Unique card number for QR-code scanning (e.g. "SS-00001").</summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>Current tier, upgraded automatically when lifetime points cross thresholds.</summary>
    public LoyaltyTier CurrentTier { get; set; } = LoyaltyTier.Standard;

    /// <summary>Redeemable points available now.</summary>
    public int PointsBalance { get; set; }

    /// <summary>Total points ever earned (drives tier progression).</summary>
    public int LifetimePointsEarned { get; set; }

    /// <summary>Total points ever redeemed.</summary>
    public int LifetimePointsRedeemed { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ICollection<PointTransaction> PointTransactions { get; set; } = [];
}
