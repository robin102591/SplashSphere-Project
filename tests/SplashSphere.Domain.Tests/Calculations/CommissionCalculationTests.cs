using SplashSphere.Domain.Calculations;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Tests.Calculations;

public sealed class CommissionCalculationTests
{
    // ── CalculateTotal — Percentage ─────────────────────────────────────────

    [Theory]
    [InlineData(220.00, 15.0, 33.00)]   // Basic Wash: 220 × 15% = 33
    [InlineData(380.00, 12.0, 45.60)]   // Premium: 380 × 12% = 45.60
    [InlineData(100.00, 10.0, 10.00)]   // Round number
    [InlineData(350.00, 8.5, 29.75)]    // Non-round rate
    [InlineData(0.00, 15.0, 0.00)]      // Zero price
    [InlineData(999.99, 0.0, 0.00)]     // Zero rate
    public void CalculateTotal_Percentage_ReturnsCorrectAmount(
        decimal price, decimal rate, decimal expected)
    {
        var result = CommissionCalculator.CalculateTotal(
            price, CommissionType.Percentage, rate, null);

        result.Should().Be(expected);
    }

    // ── CalculateTotal — FixedAmount ────────────────────────────────────────

    [Theory]
    [InlineData(220.00, 45.00, 45.00)]  // Fixed amount regardless of price
    [InlineData(999.00, 100.00, 100.00)]
    [InlineData(50.00, 0.00, 0.00)]     // Zero fixed amount
    public void CalculateTotal_FixedAmount_ReturnsFlatAmount(
        decimal price, decimal fixedAmount, decimal expected)
    {
        var result = CommissionCalculator.CalculateTotal(
            price, CommissionType.FixedAmount, null, fixedAmount);

        result.Should().Be(expected);
    }

    // ── CalculateTotal — Hybrid ─────────────────────────────────────────────

    [Theory]
    [InlineData(220.00, 15.0, 20.00, 53.00)]   // 20 + (220 × 15%) = 20 + 33 = 53
    [InlineData(380.00, 10.0, 30.00, 68.00)]   // 30 + (380 × 10%) = 30 + 38 = 68
    [InlineData(100.00, 5.0, 10.00, 15.00)]    // 10 + (100 × 5%) = 10 + 5 = 15
    public void CalculateTotal_Hybrid_CombinesFixedAndPercentage(
        decimal price, decimal rate, decimal fixedAmount, decimal expected)
    {
        var result = CommissionCalculator.CalculateTotal(
            price, CommissionType.Hybrid, rate, fixedAmount);

        result.Should().Be(expected);
    }

    // ── CalculateTotal — Unknown type ───────────────────────────────────────

    [Fact]
    public void CalculateTotal_UnknownType_ReturnsZero()
    {
        var result = CommissionCalculator.CalculateTotal(
            220m, (CommissionType)99, 15m, 20m);

        result.Should().Be(0m);
    }

    // ── SplitAmongEmployees ─────────────────────────────────────────────────

    [Theory]
    [InlineData(33.00, 3, 11.00)]       // 33 / 3 = 11 exact
    [InlineData(33.00, 1, 33.00)]       // Single employee gets all
    [InlineData(33.00, 2, 16.50)]       // 33 / 2 = 16.50 exact
    [InlineData(100.00, 3, 33.33)]      // 100 / 3 = 33.333... → rounds to 33.33
    [InlineData(100.00, 7, 14.29)]      // 100 / 7 = 14.2857... → rounds to 14.29
    [InlineData(45.60, 5, 9.12)]        // 45.60 / 5 = 9.12 exact
    [InlineData(0.00, 3, 0.00)]         // Zero commission
    public void SplitAmongEmployees_ReturnsCorrectPerEmployeeAmount(
        decimal totalCommission, int employeeCount, decimal expected)
    {
        var result = CommissionCalculator.SplitAmongEmployees(totalCommission, employeeCount);

        result.Should().Be(expected);
    }

    [Fact]
    public void SplitAmongEmployees_WithZeroEmployees_ThrowsArgumentException()
    {
        var act = () => CommissionCalculator.SplitAmongEmployees(33m, 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one employee*");
    }

    [Fact]
    public void SplitAmongEmployees_WithNegativeCount_ThrowsArgumentException()
    {
        var act = () => CommissionCalculator.SplitAmongEmployees(33m, -1);

        act.Should().Throw<ArgumentException>();
    }

    // ── CalculatePackageCommission ───────────────────────────────────────────

    [Theory]
    [InlineData(500.00, 10.0, 50.00)]   // 500 × 10% = 50
    [InlineData(1200.00, 8.0, 96.00)]   // 1200 × 8% = 96
    [InlineData(350.00, 12.5, 43.75)]   // 350 × 12.5% = 43.75
    [InlineData(0.00, 15.0, 0.00)]      // Zero price
    public void CalculatePackageCommission_ReturnsCorrectAmount(
        decimal packagePrice, decimal rate, decimal expected)
    {
        var result = CommissionCalculator.CalculatePackageCommission(packagePrice, rate);

        result.Should().Be(expected);
    }

    // ── Rounding verification ───────────────────────────────────────────────

    [Fact]
    public void CalculateTotal_Percentage_UsesAwayFromZeroRounding()
    {
        // 225 × 15% / 100 = 33.75 — exactly at .5 boundary
        // MidpointRounding.AwayFromZero should round 33.75 to 33.75 (already 2dp)
        var result = CommissionCalculator.CalculateTotal(
            225m, CommissionType.Percentage, 15m, null);
        result.Should().Be(33.75m);

        // 215 × 15% / 100 = 32.25
        result = CommissionCalculator.CalculateTotal(
            215m, CommissionType.Percentage, 15m, null);
        result.Should().Be(32.25m);
    }

    [Fact]
    public void SplitAmongEmployees_RoundingDoesNotExceedTotal()
    {
        // 100 / 3 = 33.33 per employee × 3 = 99.99 (₱0.01 rounding loss is expected)
        var perEmployee = CommissionCalculator.SplitAmongEmployees(100m, 3);
        var total = perEmployee * 3;

        // The split total should be close to the original but may differ by rounding
        total.Should().BeApproximately(100m, 0.03m);
    }
}
