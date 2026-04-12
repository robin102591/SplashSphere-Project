using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Calculations;

/// <summary>
/// Pure calculation methods for payroll processing.
/// </summary>
public static class PayrollCalculator
{
    /// <summary>
    /// Calculates the base salary for an employee based on their type.
    /// Daily/Hybrid: dailyRate × daysWorked. Commission-only: 0.
    /// </summary>
    public static decimal CalculateBaseSalary(EmployeeType employeeType, decimal? dailyRate, int daysWorked)
    {
        return employeeType is EmployeeType.Daily or EmployeeType.Hybrid && dailyRate.HasValue
            ? dailyRate.Value * daysWorked
            : 0m;
    }

    /// <summary>
    /// Calculates net pay: BaseSalary + TotalCommissions + Bonuses - Deductions.
    /// </summary>
    public static decimal CalculateNetPay(decimal baseSalary, decimal totalCommissions, decimal bonuses, decimal deductions)
    {
        return baseSalary + totalCommissions + bonuses - deductions;
    }

    /// <summary>
    /// Calculates the cash advance deduction amount for a single advance.
    /// Returns min(DeductionPerPeriod, RemainingBalance) — never exceeds what's owed.
    /// </summary>
    public static decimal CalculateCashAdvanceDeduction(decimal deductionPerPeriod, decimal remainingBalance)
    {
        return Math.Min(deductionPerPeriod, remainingBalance);
    }

    /// <summary>
    /// Calculates tip split: tip amount equally divided among all employees on the transaction.
    /// </summary>
    public static decimal CalculateTipShare(decimal tipAmount, int employeeCount)
    {
        if (employeeCount < 1)
            throw new ArgumentException("Tip split requires at least one employee.", nameof(employeeCount));

        return Math.Round(tipAmount / employeeCount, 2, MidpointRounding.AwayFromZero);
    }
}
