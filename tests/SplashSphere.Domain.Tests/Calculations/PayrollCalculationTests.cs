using SplashSphere.Domain.Calculations;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Tests.Calculations;

public sealed class PayrollCalculationTests
{
    // ── CalculateBaseSalary ─────────────────────────────────────────────────

    [Theory]
    [InlineData(EmployeeType.Daily, 500.00, 6, 3000.00)]    // Daily: 500 × 6 days = 3000
    [InlineData(EmployeeType.Daily, 600.00, 7, 4200.00)]    // Full week
    [InlineData(EmployeeType.Daily, 450.00, 0, 0.00)]       // Zero days worked
    [InlineData(EmployeeType.Hybrid, 500.00, 5, 2500.00)]   // Hybrid gets daily rate too
    [InlineData(EmployeeType.Commission, null, 6, 0.00)]    // Commission: always zero base
    [InlineData(EmployeeType.Commission, 500.00, 6, 0.00)]  // Commission: zero even if rate set
    public void CalculateBaseSalary_ReturnsCorrectAmount(
        EmployeeType type, double? rate, int days, double expected)
    {
        var dailyRate = rate.HasValue ? (decimal?)Convert.ToDecimal(rate.Value) : null;

        var result = PayrollCalculator.CalculateBaseSalary(type, dailyRate, days);

        result.Should().Be(Convert.ToDecimal(expected));
    }

    // ── CalculateNetPay ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(3000, 1500, 200, 500, 4200)]     // 3000 + 1500 + 200 - 500 = 4200
    [InlineData(0, 2500, 0, 0, 2500)]             // Commission-only employee
    [InlineData(3500, 0, 100, 0, 3600)]            // Daily-only with bonus
    [InlineData(2500, 1000, 0, 1500, 2000)]        // Heavy deductions
    [InlineData(0, 0, 0, 0, 0)]                    // Zero everything
    public void CalculateNetPay_ReturnsCorrectAmount(
        decimal baseSalary, decimal commissions, decimal bonuses, decimal deductions, decimal expected)
    {
        var result = PayrollCalculator.CalculateNetPay(baseSalary, commissions, bonuses, deductions);

        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateNetPay_DeductionsExceedEarnings_ReturnsNegative()
    {
        // Net pay CAN go negative (the system allows it — admin's responsibility)
        var result = PayrollCalculator.CalculateNetPay(1000m, 500m, 0m, 2000m);

        result.Should().Be(-500m);
    }

    // ── CalculateCashAdvanceDeduction ────────────────────────────────────────

    [Theory]
    [InlineData(500, 2000, 500)]   // Normal: deduct full period amount
    [InlineData(500, 300, 300)]    // Remaining < deduction: deduct remaining only
    [InlineData(500, 500, 500)]    // Exactly matches: fully paid
    [InlineData(500, 0, 0)]        // Already fully paid
    [InlineData(0, 1000, 0)]       // Zero deduction per period
    public void CalculateCashAdvanceDeduction_ReturnsMinOfDeductionAndRemaining(
        decimal deductionPerPeriod, decimal remainingBalance, decimal expected)
    {
        var result = PayrollCalculator.CalculateCashAdvanceDeduction(deductionPerPeriod, remainingBalance);

        result.Should().Be(expected);
    }

    [Fact]
    public void CashAdvanceDeduction_NeverExceedsRemainingBalance()
    {
        // Simulate FIFO deduction across multiple advances
        var advances = new[]
        {
            (DeductionPerPeriod: 500m, RemainingBalance: 200m),  // Advance 1: almost done
            (DeductionPerPeriod: 500m, RemainingBalance: 1500m), // Advance 2: still active
            (DeductionPerPeriod: 300m, RemainingBalance: 300m),  // Advance 3: will finish
        };

        var totalDeducted = 0m;
        foreach (var advance in advances)
        {
            var deduction = PayrollCalculator.CalculateCashAdvanceDeduction(
                advance.DeductionPerPeriod, advance.RemainingBalance);

            deduction.Should().BeLessThanOrEqualTo(advance.RemainingBalance);
            deduction.Should().BeLessThanOrEqualTo(advance.DeductionPerPeriod);

            totalDeducted += deduction;
        }

        totalDeducted.Should().Be(200m + 500m + 300m); // 1000 total
    }

    // ── CalculateTipShare ───────────────────────────────────────────────────

    [Theory]
    [InlineData(100, 2, 50.00)]     // Even split
    [InlineData(100, 3, 33.33)]     // Thirds
    [InlineData(50, 1, 50.00)]      // Single employee
    [InlineData(0, 3, 0.00)]        // No tips
    public void CalculateTipShare_SplitsEvenly(
        decimal tipAmount, int employeeCount, decimal expected)
    {
        var result = PayrollCalculator.CalculateTipShare(tipAmount, employeeCount);

        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateTipShare_WithZeroEmployees_ThrowsArgumentException()
    {
        var act = () => PayrollCalculator.CalculateTipShare(100m, 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one employee*");
    }

    // ── Full payroll scenario ───────────────────────────────────────────────

    [Fact]
    public void FullPayrollScenario_HybridEmployee()
    {
        // Hybrid employee: 6 days × ₱500/day + ₱1,200 commissions + ₱300 bonus - ₱500 cash advance
        var baseSalary = PayrollCalculator.CalculateBaseSalary(EmployeeType.Hybrid, 500m, 6);
        baseSalary.Should().Be(3000m);

        var deduction = PayrollCalculator.CalculateCashAdvanceDeduction(500m, 2000m);
        deduction.Should().Be(500m);

        var netPay = PayrollCalculator.CalculateNetPay(baseSalary, 1200m, 300m, deduction);
        netPay.Should().Be(4000m); // 3000 + 1200 + 300 - 500
    }

    [Fact]
    public void FullPayrollScenario_CommissionOnlyEmployee()
    {
        // Commission-only: no daily rate, ₱2,500 commissions, no adjustments
        var baseSalary = PayrollCalculator.CalculateBaseSalary(EmployeeType.Commission, null, 6);
        baseSalary.Should().Be(0m);

        var netPay = PayrollCalculator.CalculateNetPay(baseSalary, 2500m, 0m, 0m);
        netPay.Should().Be(2500m);
    }
}
