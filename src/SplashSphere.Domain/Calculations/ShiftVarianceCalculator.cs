namespace SplashSphere.Domain.Calculations;

/// <summary>
/// Pure calculation methods for cashier shift variance.
/// Extracted from handler logic to enable direct unit testing.
/// </summary>
public static class ShiftVarianceCalculator
{
    /// <summary>
    /// Calculates the expected cash in the drawer at shift close.
    /// Formula: OpeningCash + CashPayments + CashIn - CashOut.
    /// </summary>
    public static decimal CalculateExpectedCash(
        decimal openingCashFund,
        decimal totalCashPayments,
        decimal totalCashIn,
        decimal totalCashOut)
    {
        return openingCashFund + totalCashPayments + totalCashIn - totalCashOut;
    }

    /// <summary>
    /// Calculates the actual cash from denomination counts.
    /// </summary>
    public static decimal CalculateActualCash(IEnumerable<(decimal DenominationValue, int Count)> denominations)
    {
        return denominations.Sum(d => d.DenominationValue * d.Count);
    }

    /// <summary>
    /// Calculates the cash variance (positive = overage, negative = shortage).
    /// </summary>
    public static decimal CalculateVariance(decimal actualCash, decimal expectedCash)
    {
        return actualCash - expectedCash;
    }
}
