namespace SplashSphere.Domain.Enums;

/// <summary>
/// Classifies a movement in a membership card's point ledger.
/// </summary>
public enum PointTransactionType
{
    /// <summary>Points awarded from a completed car wash transaction.</summary>
    Earned = 0,

    /// <summary>Points spent to claim a loyalty reward.</summary>
    Redeemed = 1,

    /// <summary>Points removed due to time-based expiry policy.</summary>
    Expired = 2,

    /// <summary>Manual admin correction (positive or negative).</summary>
    Adjustment = 3,

    /// <summary>Bonus points awarded via the referral program.</summary>
    Referral = 4,
}
