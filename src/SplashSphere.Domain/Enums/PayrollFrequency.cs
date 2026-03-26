namespace SplashSphere.Domain.Enums;

/// <summary>
/// Determines how often payroll periods are created.
/// </summary>
public enum PayrollFrequency
{
    /// <summary>
    /// 7-day periods starting on the tenant's configured <c>CutOffStartDay</c>.
    /// </summary>
    Weekly = 1,

    /// <summary>
    /// Two periods per month: 1st–15th and 16th–last day.
    /// Philippine standard semi-monthly cutoff.
    /// </summary>
    SemiMonthly = 2,
}
