namespace SplashSphere.Domain.Enums;

/// <summary>
/// Defines how a service commission amount is calculated per transaction line item.
/// </summary>
public enum CommissionType
{
    /// <summary>
    /// Commission is a fixed percentage of the service's final price.
    /// Formula: <c>price × rate / 100</c>.
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// Commission is a fixed peso amount regardless of the service price.
    /// </summary>
    FixedAmount = 2,

    /// <summary>
    /// Commission combines a fixed base amount plus a percentage of the price.
    /// Formula: <c>fixedAmount + (price × rate / 100)</c>.
    /// </summary>
    Hybrid = 3,
}
