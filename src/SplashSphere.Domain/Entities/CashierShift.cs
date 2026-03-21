using SplashSphere.Domain.Enums;

namespace SplashSphere.Domain.Entities;

/// <summary>
/// Represents a single cashier shift at a branch, from opening to closing.
/// <para>
/// <b>Lifecycle:</b> Open → Closed (or Voided if no transactions were processed).
/// </para>
/// <para>
/// <b>Business date rule:</b> <see cref="ShiftDate"/> is the Manila local calendar date.
/// If it is past midnight but before 06:00, the previous calendar date is used
/// (handles late-night operations). Computed by the Application layer on open.
/// </para>
/// <para>
/// <b>Cash formula:</b>
/// <c>ExpectedCashInDrawer = OpeningCashFund + TotalCashPayments + TotalCashIn - TotalCashOut</c><br/>
/// <c>Variance = ActualCashInDrawer - ExpectedCashInDrawer</c> (negative = short)
/// </para>
/// <para>
/// <b>Auto-review thresholds</b> (tenant-configurable, defaults 50 / 200):<br/>
/// |Variance| ≤ ₱50 → Approved; ≤ ₱200 → Pending; > ₱200 → Flagged.
/// </para>
/// </summary>
public sealed class CashierShift : IAuditableEntity
{
    private CashierShift() { } // EF Core

    public CashierShift(
        string tenantId,
        string branchId,
        string cashierId,
        DateOnly shiftDate,
        DateTime openedAt,
        decimal openingCashFund)
    {
        Id             = Guid.NewGuid().ToString();
        TenantId       = tenantId;
        BranchId       = branchId;
        CashierId      = cashierId;
        ShiftDate      = shiftDate;
        OpenedAt       = openedAt;
        OpeningCashFund = openingCashFund;
        Status         = ShiftStatus.Open;
        ReviewStatus   = ReviewStatus.Pending;
    }

    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;

    /// <summary>Internal <see cref="User.Id"/> of the cashier who owns this shift.</summary>
    public string CashierId { get; set; } = string.Empty;

    // ── Shift timing ──────────────────────────────────────────────────────────

    /// <summary>
    /// The Manila-local business date. Stored as a PostgreSQL <c>date</c> column.
    /// If opened after midnight but before 06:00, this is set to the previous date.
    /// </summary>
    public DateOnly ShiftDate { get; set; }

    /// <summary>UTC timestamp when the shift was opened.</summary>
    public DateTime OpenedAt { get; set; }

    /// <summary>UTC timestamp when the shift was closed or voided. Null while still open.</summary>
    public DateTime? ClosedAt { get; set; }

    public ShiftStatus Status { get; set; } = ShiftStatus.Open;

    // ── Cash fund ─────────────────────────────────────────────────────────────

    /// <summary>Starting change fund given to the cashier at the start of the shift (₱).</summary>
    public decimal OpeningCashFund { get; set; }

    // ── System-calculated totals (populated on close) ─────────────────────────

    /// <summary>Total cash received from completed transactions during this shift.</summary>
    public decimal TotalCashPayments { get; set; }

    /// <summary>Total non-cash payments (GCash + Card + Bank) during this shift.</summary>
    public decimal TotalNonCashPayments { get; set; }

    /// <summary>Sum of manual CashIn movements recorded during the shift.</summary>
    public decimal TotalCashIn { get; set; }

    /// <summary>Sum of manual CashOut movements (petty cash, vale, refunds) during the shift.</summary>
    public decimal TotalCashOut { get; set; }

    /// <summary>
    /// OpeningCashFund + TotalCashPayments + TotalCashIn − TotalCashOut.
    /// How much cash should theoretically be in the drawer at close.
    /// </summary>
    public decimal ExpectedCashInDrawer { get; set; }

    /// <summary>Physical cash total from the denomination count entered by the cashier.</summary>
    public decimal ActualCashInDrawer { get; set; }

    /// <summary>
    /// ActualCashInDrawer − ExpectedCashInDrawer.
    /// Positive = over; Negative = short; Zero = balanced.
    /// </summary>
    public decimal Variance { get; set; }

    // ── Transaction summary (populated on close) ───────────────────────────────

    public int TotalTransactionCount { get; set; }

    /// <summary>TotalCashPayments + TotalNonCashPayments.</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>Sum of all TransactionEmployee commission amounts during this shift.</summary>
    public decimal TotalCommissions { get; set; }

    /// <summary>Sum of all Transaction.DiscountAmount values during this shift.</summary>
    public decimal TotalDiscounts { get; set; }

    // ── Manager review ────────────────────────────────────────────────────────

    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Pending;

    /// <summary>Internal <see cref="User.Id"/> of the manager who reviewed. Null if unreviewed.</summary>
    public string? ReviewedById { get; set; }

    /// <summary>UTC timestamp of the review action.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Notes added by the manager during review (required when flagging).</summary>
    public string? ReviewNotes { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────────

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Navigations ──────────────────────────────────────────────────────────

    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public User Cashier { get; set; } = null!;
    public User? ReviewedBy { get; set; }
    public List<CashMovement> CashMovements { get; set; } = [];
    public List<ShiftDenomination> Denominations { get; set; } = [];
    public List<ShiftPaymentSummary> PaymentSummaries { get; set; } = [];
}
