namespace SplashSphere.Domain.Entities;

/// <summary>
/// Records the count of a single Philippine denomination in the cash drawer
/// at the time the <see cref="CashierShift"/> is closed.
/// <para>
/// <b>Philippine denominations (descending):</b><br/>
/// Bills: ₱1,000 | ₱500 | ₱200 | ₱100 | ₱50 | ₱20<br/>
/// Coins: ₱10 | ₱5 | ₱1 | ₱0.25
/// </para>
/// <see cref="Subtotal"/> = <see cref="DenominationValue"/> × <see cref="Count"/>.
/// All denomination rows are cascade-deleted when the parent shift is deleted.
/// </summary>
public sealed class ShiftDenomination : ITenantScoped
{
    private ShiftDenomination() { } // EF Core

    public ShiftDenomination(string tenantId, string cashierShiftId, decimal denominationValue, int count)
    {
        Id                 = Guid.NewGuid().ToString();
        TenantId           = tenantId;
        CashierShiftId     = cashierShiftId;
        DenominationValue  = denominationValue;
        Count              = count;
        Subtotal           = denominationValue * count;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CashierShiftId { get; set; } = string.Empty;

    /// <summary>Face value of the denomination (e.g. 1000, 500, 0.25).</summary>
    public decimal DenominationValue { get; set; }

    /// <summary>Physical count of this denomination in the drawer.</summary>
    public int Count { get; set; }

    /// <summary>DenominationValue × Count. Stored for fast aggregation.</summary>
    public decimal Subtotal { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public CashierShift CashierShift { get; set; } = null!;
}
