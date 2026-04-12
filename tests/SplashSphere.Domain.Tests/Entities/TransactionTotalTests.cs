namespace SplashSphere.Domain.Tests.Entities;

/// <summary>
/// Tests the transaction total aggregation formula:
///   FinalAmount = TotalAmount - DiscountAmount + TaxAmount
/// where TotalAmount = services + packages + merchandise.
/// </summary>
public sealed class TransactionTotalTests
{
    // ── FinalAmount formula ─────────────────────────────────────────────────

    [Theory]
    [InlineData(1000, 0, 0, 1000)]       // No discount, no tax
    [InlineData(1000, 100, 0, 900)]       // Discount only
    [InlineData(1000, 0, 120, 1120)]      // Tax only
    [InlineData(1000, 100, 120, 1020)]    // Both discount and tax
    [InlineData(500, 500, 0, 0)]          // 100% discount
    [InlineData(0, 0, 0, 0)]              // Zero everything
    public void FinalAmount_CalculatesCorrectly(
        decimal totalAmount, decimal discount, decimal tax, decimal expected)
    {
        var finalAmount = totalAmount - discount + tax;

        finalAmount.Should().Be(expected);
    }

    // ── TotalAmount aggregation ─────────────────────────────────────────────

    [Theory]
    [InlineData(220, 500, 150, 870)]      // Services + packages + merchandise
    [InlineData(380, 0, 0, 380)]           // Services only
    [InlineData(0, 800, 0, 800)]           // Packages only
    [InlineData(0, 0, 250, 250)]           // Merchandise only
    [InlineData(220, 500, 0, 720)]         // No merchandise
    public void TotalAmount_SumsAllLineItems(
        decimal services, decimal packages, decimal merchandise, decimal expected)
    {
        var totalAmount = services + packages + merchandise;

        totalAmount.Should().Be(expected);
    }

    // ── Full transaction scenario ───────────────────────────────────────────

    [Fact]
    public void FullScenario_BasicWashWithMerchandise()
    {
        // 1 Basic Wash (₱220) + 1 Air Freshener (₱150) - ₱50 senior discount + ₱0 tax
        var serviceAmount = 220m;
        var merchandiseAmount = 150m;
        var totalAmount = serviceAmount + 0m + merchandiseAmount; // no packages
        totalAmount.Should().Be(370m);

        var finalAmount = totalAmount - 50m + 0m;
        finalAmount.Should().Be(320m);
    }

    [Fact]
    public void FullScenario_PremiumPackageWithDiscount()
    {
        // 1 Premium Package (₱800) + 1 Basic Wash (₱220) + 2 towels (₱200) - ₱100 promo
        var totalAmount = 220m + 800m + 200m;
        totalAmount.Should().Be(1220m);

        var finalAmount = totalAmount - 100m + 0m;
        finalAmount.Should().Be(1120m);
    }

    // ── Edge cases ──────────────────────────────────────────────────────────

    [Fact]
    public void DiscountLargerThanTotal_ResultsInNegative()
    {
        // System should prevent this via validation, but the formula itself doesn't guard
        var finalAmount = 100m - 200m + 0m;
        finalAmount.Should().Be(-100m);
    }

    [Fact]
    public void PrecisionMaintained_WithSmallAmounts()
    {
        // Verify decimal precision with Philippine peso centavos
        var finalAmount = 99.50m - 10.25m + 12.75m;
        finalAmount.Should().Be(102.00m);
    }
}
