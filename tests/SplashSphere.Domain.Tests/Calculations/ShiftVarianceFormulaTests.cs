using SplashSphere.Domain.Calculations;

namespace SplashSphere.Domain.Tests.Calculations;

public sealed class ShiftVarianceFormulaTests
{
    // ── CalculateExpectedCash ────────────────────────────────────────────────

    [Theory]
    [InlineData(5000, 12500, 2000, 1500, 18000)]  // 5000 + 12500 + 2000 - 1500 = 18000
    [InlineData(3000, 8000, 0, 0, 11000)]          // No cash movements
    [InlineData(5000, 0, 0, 0, 5000)]              // No sales at all — just opening fund
    [InlineData(1000, 5000, 500, 3000, 3500)]      // Large cash out
    [InlineData(0, 0, 0, 0, 0)]                    // Zero everything
    public void CalculateExpectedCash_ReturnsCorrectTotal(
        decimal opening, decimal cashPayments, decimal cashIn, decimal cashOut, decimal expected)
    {
        var result = ShiftVarianceCalculator.CalculateExpectedCash(opening, cashPayments, cashIn, cashOut);

        result.Should().Be(expected);
    }

    // ── CalculateActualCash ─────────────────────────────────────────────────

    [Fact]
    public void CalculateActualCash_PhilippineDenominations_ReturnsCorrectTotal()
    {
        var denominations = new (decimal Value, int Count)[]
        {
            (1000m, 10),  // 10,000
            (500m, 5),    // 2,500
            (200m, 3),    // 600
            (100m, 8),    // 800
            (50m, 4),     // 200
            (20m, 10),    // 200
            (10m, 5),     // 50
            (5m, 6),      // 30
            (1m, 20),     // 20
        };

        var result = ShiftVarianceCalculator.CalculateActualCash(denominations);

        result.Should().Be(14400m);
    }

    [Fact]
    public void CalculateActualCash_EmptyDenominations_ReturnsZero()
    {
        var result = ShiftVarianceCalculator.CalculateActualCash([]);

        result.Should().Be(0m);
    }

    [Fact]
    public void CalculateActualCash_ZeroCounts_ReturnsZero()
    {
        var denominations = new (decimal Value, int Count)[]
        {
            (1000m, 0),
            (500m, 0),
            (100m, 0),
        };

        var result = ShiftVarianceCalculator.CalculateActualCash(denominations);

        result.Should().Be(0m);
    }

    // ── CalculateVariance ───────────────────────────────────────────────────

    [Theory]
    [InlineData(18000, 18000, 0)]         // Perfect — no variance
    [InlineData(18100, 18000, 100)]       // Overage (positive)
    [InlineData(17800, 18000, -200)]      // Shortage (negative)
    [InlineData(0, 5000, -5000)]          // Drawer empty — missing everything
    public void CalculateVariance_ReturnsActualMinusExpected(
        decimal actual, decimal expected, decimal expectedVariance)
    {
        var result = ShiftVarianceCalculator.CalculateVariance(actual, expected);

        result.Should().Be(expectedVariance);
    }

    // ── End-to-end shift scenario ───────────────────────────────────────────

    [Fact]
    public void FullShiftScenario_CalculatesCorrectVariance()
    {
        // Cashier opens with ₱5,000
        // Receives ₱12,500 in cash payments
        // Cash in: ₱2,000 (change fund top-up)
        // Cash out: ₱1,500 (GCash cashout withdrawal)
        var expectedCash = ShiftVarianceCalculator.CalculateExpectedCash(5000m, 12500m, 2000m, 1500m);
        expectedCash.Should().Be(18000m);

        // Cashier counts: 10×1000 + 5×500 + 3×200 + 8×100 + 4×50 + 10×20 + 5×10 + 6×5 + 20×1
        var denominations = new (decimal, int)[]
        {
            (1000m, 10), (500m, 5), (200m, 3), (100m, 8),
            (50m, 4), (20m, 10), (10m, 5), (5m, 6), (1m, 20),
        };
        var actualCash = ShiftVarianceCalculator.CalculateActualCash(denominations);
        actualCash.Should().Be(14400m);

        // Variance: 14400 - 18000 = -3600 (shortage)
        var variance = ShiftVarianceCalculator.CalculateVariance(actualCash, expectedCash);
        variance.Should().Be(-3600m);
    }
}
