using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Calculations;

/// <summary>
/// Pure calculation methods for service and package commissions.
/// Extracted from handler logic to enable direct unit testing.
/// </summary>
public static class CommissionCalculator
{
    /// <summary>
    /// Calculates the total commission for a service line item using the
    /// Percentage / FixedAmount / Hybrid formula.
    /// </summary>
    /// <param name="price">The final service price after modifiers.</param>
    /// <param name="type">The commission calculation type.</param>
    /// <param name="percentageRate">The percentage rate (e.g. 15.0 means 15%). Required for Percentage and Hybrid.</param>
    /// <param name="fixedAmount">The fixed peso amount. Required for FixedAmount and Hybrid.</param>
    /// <returns>The total commission amount (before employee split).</returns>
    public static decimal CalculateTotal(
        decimal price,
        CommissionType type,
        decimal? percentageRate,
        decimal? fixedAmount)
    {
        return type switch
        {
            CommissionType.Percentage =>
                Math.Round(price * percentageRate!.Value / 100, 2, MidpointRounding.AwayFromZero),

            CommissionType.FixedAmount =>
                fixedAmount!.Value,

            CommissionType.Hybrid =>
                Math.Round(
                    fixedAmount!.Value + (price * percentageRate!.Value / 100),
                    2, MidpointRounding.AwayFromZero),

            _ => 0m,
        };
    }

    /// <summary>
    /// Splits a total commission equally among N employees with banker's rounding.
    /// </summary>
    /// <param name="totalCommission">The total commission to split.</param>
    /// <param name="employeeCount">Number of employees to split among. Must be &gt; 0.</param>
    /// <returns>The per-employee commission amount.</returns>
    /// <exception cref="ArgumentException">When <paramref name="employeeCount"/> is less than 1.</exception>
    public static decimal SplitAmongEmployees(decimal totalCommission, int employeeCount)
    {
        if (employeeCount < 1)
            throw new ArgumentException("Commission split requires at least one employee.", nameof(employeeCount));

        return Math.Round(totalCommission / employeeCount, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calculates a package commission (always percentage-based).
    /// </summary>
    public static decimal CalculatePackageCommission(decimal packagePrice, decimal percentageRate)
    {
        return Math.Round(packagePrice * percentageRate / 100, 2, MidpointRounding.AwayFromZero);
    }
}
