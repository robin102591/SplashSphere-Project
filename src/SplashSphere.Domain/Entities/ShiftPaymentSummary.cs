using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// A per-payment-method breakdown computed and stored when a <see cref="CashierShift"/>
/// is closed. One row per <see cref="PaymentMethod"/> that had at least one transaction
/// during the shift.
/// <para>
/// Data is sourced from the <c>Payment</c> table at close time and stored here for
/// fast retrieval on the end-of-day report without re-querying all payments.
/// </para>
/// </summary>
public sealed class ShiftPaymentSummary
{
    private ShiftPaymentSummary() { } // EF Core

    public ShiftPaymentSummary(
        string tenantId,
        string cashierShiftId,
        PaymentMethod method,
        int transactionCount,
        decimal totalAmount)
    {
        Id               = Guid.NewGuid().ToString();
        TenantId         = tenantId;
        CashierShiftId   = cashierShiftId;
        Method           = method;
        TransactionCount = transactionCount;
        TotalAmount      = totalAmount;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CashierShiftId { get; set; } = string.Empty;

    /// <summary>The payment method this summary row covers.</summary>
    public PaymentMethod Method { get; set; }

    /// <summary>Number of distinct transactions that used this payment method.</summary>
    public int TransactionCount { get; set; }

    /// <summary>Total amount received via this payment method (₱). Precision (10, 2).</summary>
    public decimal TotalAmount { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public CashierShift CashierShift { get; set; } = null!;
}
