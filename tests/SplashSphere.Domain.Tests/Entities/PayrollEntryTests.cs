using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Tests.Entities;

/// <summary>
/// Tests PayrollEntry entity — NetPay computed property and RecalculateTotals method.
/// </summary>
public sealed class PayrollEntryTests
{
    // ── NetPay computed property ─────────────────────────────────────────────

    [Fact]
    public void NetPay_CommissionOnlyEmployee_ReturnsTotalCommissions()
    {
        var entry = CreateEntry(EmployeeType.Commission, baseSalary: 0, commissions: 2500);

        entry.NetPay.Should().Be(2500m);
    }

    [Fact]
    public void NetPay_DailyEmployee_ReturnsBaseSalary()
    {
        var entry = CreateEntry(EmployeeType.Daily, baseSalary: 3000, commissions: 0);

        entry.NetPay.Should().Be(3000m);
    }

    [Fact]
    public void NetPay_HybridEmployee_CombinesBaseSalaryAndCommissions()
    {
        var entry = CreateEntry(EmployeeType.Hybrid, baseSalary: 3000, commissions: 1500);

        entry.NetPay.Should().Be(4500m);
    }

    [Fact]
    public void NetPay_WithBonusesAndDeductions_CalculatesCorrectly()
    {
        var entry = CreateEntry(EmployeeType.Hybrid, baseSalary: 3000, commissions: 1500);
        entry.Bonuses = 200m;
        entry.Deductions = 500m;

        entry.NetPay.Should().Be(4200m); // 3000 + 1500 + 200 - 500
    }

    [Fact]
    public void NetPay_HighDeductions_CanGoNegative()
    {
        var entry = CreateEntry(EmployeeType.Commission, baseSalary: 0, commissions: 500);
        entry.Deductions = 1000m;

        entry.NetPay.Should().Be(-500m);
    }

    // ── RecalculateTotals ───────────────────────────────────────────────────

    [Fact]
    public void RecalculateTotals_WithAdjustments_UpdatesBonusesAndDeductions()
    {
        var entry = CreateEntry(EmployeeType.Daily, baseSalary: 3000, commissions: 0);
        entry.Adjustments.Add(CreateAdjustment(entry.Id, AdjustmentType.Bonus, 200m));
        entry.Adjustments.Add(CreateAdjustment(entry.Id, AdjustmentType.Bonus, 100m));
        entry.Adjustments.Add(CreateAdjustment(entry.Id, AdjustmentType.Deduction, 500m));

        entry.RecalculateTotals();

        entry.Bonuses.Should().Be(300m);
        entry.Deductions.Should().Be(500m);
        entry.NetPay.Should().Be(2800m); // 3000 + 0 + 300 - 500
    }

    [Fact]
    public void RecalculateTotals_WithNoAdjustments_PreservesExistingValues()
    {
        var entry = CreateEntry(EmployeeType.Daily, baseSalary: 3000, commissions: 0);
        entry.Bonuses = 200m;
        entry.Deductions = 100m;

        entry.RecalculateTotals(); // No adjustments loaded — should preserve

        entry.Bonuses.Should().Be(200m);
        entry.Deductions.Should().Be(100m);
    }

    [Fact]
    public void RecalculateTotals_OnlyBonuses_DeductionsBecomesZero()
    {
        var entry = CreateEntry(EmployeeType.Commission, baseSalary: 0, commissions: 1500);
        entry.Deductions = 999m; // Legacy value
        entry.Adjustments.Add(CreateAdjustment(entry.Id, AdjustmentType.Bonus, 300m));

        entry.RecalculateTotals();

        entry.Bonuses.Should().Be(300m);
        entry.Deductions.Should().Be(0m); // Overwritten since adjustments exist
    }

    [Fact]
    public void RecalculateTotals_CashAdvanceAdjustments_AppliedAsDeductions()
    {
        var entry = CreateEntry(EmployeeType.Hybrid, baseSalary: 2500, commissions: 1000);
        entry.Adjustments.Add(CreateAdjustment(entry.Id, AdjustmentType.Deduction, 500m, "Cash Advance"));
        entry.Adjustments.Add(CreateAdjustment(entry.Id, AdjustmentType.Deduction, 300m, "Cash Advance"));

        entry.RecalculateTotals();

        entry.Deductions.Should().Be(800m);
        entry.NetPay.Should().Be(2700m); // 2500 + 1000 + 0 - 800
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static PayrollEntry CreateEntry(
        EmployeeType type,
        decimal baseSalary,
        decimal commissions,
        decimal tips = 0m)
    {
        return new PayrollEntry(
            tenantId: "test-tenant",
            payrollPeriodId: "period-1",
            employeeId: "emp-1",
            employeeTypeSnapshot: type,
            daysWorked: type is EmployeeType.Daily or EmployeeType.Hybrid ? 6 : 0,
            dailyRateSnapshot: type is EmployeeType.Daily or EmployeeType.Hybrid ? 500m : null,
            baseSalary: baseSalary,
            totalCommissions: commissions,
            totalTips: tips);
    }

    private static PayrollAdjustment CreateAdjustment(
        string entryId,
        AdjustmentType type,
        decimal amount,
        string category = "Test")
    {
        return new PayrollAdjustment(
            tenantId: "test-tenant",
            payrollEntryId: entryId,
            type: type,
            category: category,
            amount: amount,
            notes: null);
    }
}
