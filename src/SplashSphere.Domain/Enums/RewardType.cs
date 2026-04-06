namespace SplashSphere.Domain.Enums;

/// <summary>
/// The kind of benefit a loyalty reward provides when redeemed.
/// </summary>
public enum RewardType
{
    /// <summary>A specific service at no charge (linked via ServiceId).</summary>
    FreeService = 0,

    /// <summary>A specific package at no charge (linked via PackageId).</summary>
    FreePackage = 1,

    /// <summary>A fixed peso discount off the transaction total.</summary>
    DiscountAmount = 2,

    /// <summary>A percentage discount off the transaction total.</summary>
    DiscountPercent = 3,
}
