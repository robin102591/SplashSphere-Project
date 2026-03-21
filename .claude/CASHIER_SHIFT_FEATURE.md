# SplashSphere — Cashier Shift & End-of-Day Reporting

> **Replaces:** The simple CashReconciliation feature from PHASE15_FEATURES.md Prompt 15.9.
> **What this covers:** Full shift lifecycle (open → operate → close), cash fund tracking,
> cash movement logging, denomination counting, end-of-day summary report (printable),
> admin review, and per-cashier variance analysis.

---

## Why This Matters

In a Philippine car wash, the owner is often not physically present at the branch. Cash is the dominant payment method (~40-60% of transactions). Without shift accountability:
- No way to know if ₱500 went missing from the drawer
- No audit trail for petty cash purchases during the day
- No visibility into how much change fund was given and whether it came back
- No per-cashier tracking to identify patterns of shortages
- No printable report for the owner to review at home

This feature turns the cash drawer from a black box into a fully transparent, auditable system.

---

## Domain Models

### CashierShift (The Core Entity)

```csharp
public sealed class CashierShift : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string CashierId { get; set; } = string.Empty;       // UserId who owns this shift

    // Shift timing
    public DateTime ShiftDate { get; set; }                      // The business date
    public DateTime OpenedAt { get; set; }                       // When shift was opened
    public DateTime? ClosedAt { get; set; }                      // When shift was closed
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;

    // Cash fund
    public decimal OpeningCashFund { get; set; }                 // Starting cash given to cashier
    
    // System-calculated totals (computed on close, stored for fast queries)
    public decimal TotalCashPayments { get; set; }               // Cash received from transactions
    public decimal TotalNonCashPayments { get; set; }            // GCash + Card + Bank total
    public decimal TotalCashIn { get; set; }                     // Manual cash-in entries
    public decimal TotalCashOut { get; set; }                    // Manual cash-out entries (petty cash, refunds, etc.)
    public decimal ExpectedCashInDrawer { get; set; }            // = OpeningFund + CashPayments + CashIn - CashOut
    public decimal ActualCashInDrawer { get; set; }              // From denomination count
    public decimal Variance { get; set; }                        // = Actual - Expected (negative = short)

    // Transaction summary
    public int TotalTransactionCount { get; set; }
    public decimal TotalRevenue { get; set; }                    // All payments (cash + non-cash)
    public decimal TotalCommissions { get; set; }
    public decimal TotalDiscounts { get; set; }

    // Review
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Pending;
    public string? ReviewedById { get; set; }                    // Manager who reviewed
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public User Cashier { get; set; } = null!;
    public User? ReviewedBy { get; set; }
    public List<CashMovement> CashMovements { get; set; } = [];
    public List<ShiftDenomination> Denominations { get; set; } = [];
    public List<ShiftPaymentSummary> PaymentSummaries { get; set; } = [];
}
```

### CashMovement (Cash In/Out During Shift)

```csharp
public sealed class CashMovement : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string CashierShiftId { get; set; } = string.Empty;
    public CashMovementType Type { get; set; }                   // CashIn or CashOut
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;           // e.g., "Soap purchase", "Employee vale", "Change from owner"
    public string? Reference { get; set; }                       // Receipt number, employee name, etc.
    public DateTime MovementTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public CashierShift CashierShift { get; set; } = null!;
}
```

### ShiftDenomination (Physical Cash Count at Close)

```csharp
public sealed class ShiftDenomination
{
    public string Id { get; set; } = string.Empty;
    public string CashierShiftId { get; set; } = string.Empty;
    public decimal DenominationValue { get; set; }               // 1000, 500, 200, 100, 50, 20, 10, 5, 1, 0.25
    public int Count { get; set; }
    public decimal Subtotal { get; set; }                        // = DenominationValue × Count

    public CashierShift CashierShift { get; set; } = null!;
}
```

### ShiftPaymentSummary (Breakdown by Payment Method)

```csharp
public sealed class ShiftPaymentSummary
{
    public string Id { get; set; } = string.Empty;
    public string CashierShiftId { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }                    // Cash, GCash, CreditCard, DebitCard, BankTransfer
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }

    public CashierShift CashierShift { get; set; } = null!;
}
```

### Enums

```csharp
public enum ShiftStatus
{
    Open,       // Shift is active, cashier is operating
    Closed,     // Cashier has counted the drawer and submitted
    Voided      // Shift was voided (opened by mistake, etc.)
}

public enum ReviewStatus
{
    Pending,    // Not yet reviewed by manager
    Approved,   // Manager approved (variance acceptable)
    Flagged     // Manager flagged for investigation (variance too large)
}

public enum CashMovementType
{
    CashIn,     // Money added to drawer (e.g., additional change fund from owner)
    CashOut     // Money removed from drawer (e.g., petty cash purchase, employee vale, refund)
}
```

---

## Business Rules

### Opening a Shift

1. A cashier can have **only one open shift** at a time per branch. Attempting to open a second returns an error.
2. `OpeningCashFund` is required — this is the change fund (typically ₱1,000–₱3,000). The cashier enters the amount they received.
3. `ShiftDate` defaults to today (Asia/Manila timezone). If it's past midnight but before 6 AM, the system still considers it the previous business date (handles late-night operations).
4. `OpenedAt` = current timestamp.
5. Status = `Open`.

### During the Shift

6. While a shift is `Open`, the cashier can record **CashMovement** entries:
   - **CashIn:** Additional change fund received, or any cash added to the drawer that isn't a customer payment.
   - **CashOut:** Petty cash purchases (soap, supplies, meals), cash given as employee vale/advance, cash refunds to customers outside the normal transaction refund flow.
7. Each movement requires: Type, Amount, Reason. Optional: Reference.
8. CashMovements are **append-only** during the shift. To correct a mistake, add a reversing entry (e.g., CashIn ₱500 to fix an erroneous CashOut ₱500).
9. Normal customer payments (from completed transactions) are NOT recorded as CashMovements — they're automatically captured from the `Payment` table.

### Closing a Shift (End of Day)

10. The cashier initiates the close process. The system:
    a. **Queries all completed transactions** during this shift's time window (from `OpenedAt` to now) for the branch where `cashierId` matches.
    b. **Groups payments by method**, creates `ShiftPaymentSummary` records:
       - Cash: count and total
       - GCash: count and total
       - Credit Card: count and total
       - Debit Card: count and total
       - Bank Transfer: count and total
    c. Sets computed totals:
       - `TotalCashPayments` = sum of all Cash payments
       - `TotalNonCashPayments` = sum of all non-Cash payments
       - `TotalRevenue` = TotalCashPayments + TotalNonCashPayments
       - `TotalTransactionCount` = count of distinct transactions
       - `TotalCommissions` = sum of all TransactionEmployee.totalCommissionAmount
       - `TotalDiscounts` = sum of all Transaction.discountAmount
       - `TotalCashIn` = sum of CashMovements where type = CashIn
       - `TotalCashOut` = sum of CashMovements where type = CashOut
       - `ExpectedCashInDrawer` = OpeningCashFund + TotalCashPayments + TotalCashIn - TotalCashOut

11. The cashier enters the **denomination count** — how many of each bill/coin is physically in the drawer.
12. System calculates:
    - `ActualCashInDrawer` = sum of (DenominationValue × Count) for all denominations
    - `Variance` = ActualCashInDrawer - ExpectedCashInDrawer
    - Positive variance = over (more cash than expected)
    - Negative variance = short (less cash than expected)

13. Status → `Closed`, `ClosedAt` = now.

### Variance Thresholds (Tenant-Configurable)

14. After closing, auto-set `ReviewStatus` based on variance:
    - `|Variance|` ≤ ₱50 → `ReviewStatus = Approved` (auto-approved, minor discrepancy)
    - `|Variance|` > ₱50 and ≤ ₱200 → `ReviewStatus = Pending` (needs manager review)
    - `|Variance|` > ₱200 → `ReviewStatus = Flagged` (requires investigation)

    These thresholds (50 and 200) should be configurable per tenant via settings.

### Manager Review

15. Manager can view any closed shift and:
    - **Approve** — accept the variance with optional notes
    - **Flag** — mark for investigation with required notes
    - **Reopen** — set Status back to Open (only if shift was closed by mistake and no new shift has been opened)

16. Once a shift is `Approved` or `Flagged`, it cannot be reopened.

### Constraints

17. Cannot close a shift that has no denomination count submitted.
18. Cannot open a new shift if the previous shift for the same branch and cashier is still `Open`.
19. Multiple cashiers CAN have overlapping shifts at the same branch (e.g., two registers). Each is independent.
20. Voiding a shift is only allowed by a manager and only if no transactions were processed during it.

---

## Philippine Denominations

```
Bills:  ₱1,000  ₱500  ₱200  ₱100  ₱50  ₱20
Coins:  ₱10  ₱5  ₱1  ₱0.25 (25 centavos)
```

The denomination grid in the UI should list these in descending order. Most car washes won't deal with ₱0.25 coins, but include it for completeness.

---

## Expected Cash Calculation Formula

```
Expected Cash in Drawer =
    Opening Cash Fund
  + Total Cash Payments Received (from completed transactions)
  + Total Manual Cash-In (additional funds added during shift)
  - Total Manual Cash-Out (petty cash, vale, refunds removed during shift)

Variance =
    Actual Cash Counted (from denomination breakdown)
  - Expected Cash in Drawer

If Variance > 0 → OVER (more cash than expected — possible uncounted cash-in)
If Variance < 0 → SHORT (less cash than expected — possible theft, error, or unrecorded cash-out)
If Variance = 0 → BALANCED (perfect match)
```

---

## End-of-Day Report (Printable)

This is the report the cashier prints or the owner reviews remotely. It should work both as an on-screen view and as a `@media print` layout.

```
╔══════════════════════════════════════════════════════╗
║            SPLASHSPHERE — END OF DAY REPORT          ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Branch:    SparkleWash - Makati                     ║
║  Cashier:   Ana Reyes                                ║
║  Date:      March 21, 2026                           ║
║  Shift:     8:02 AM — 5:47 PM (9h 45m)              ║
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  TRANSACTION SUMMARY                                 ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Total Transactions:              47                 ║
║  Total Revenue:               ₱32,450.00             ║
║  Total Discounts:              -₱1,200.00            ║
║  Net Revenue:                 ₱31,250.00             ║
║  Total Commissions:            ₱6,420.00             ║
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  PAYMENT METHOD BREAKDOWN                            ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Cash              28 txns        ₱18,650.00         ║
║  GCash             12 txns         ₱8,400.00         ║
║  Credit Card        5 txns         ₱3,200.00         ║
║  Debit Card         2 txns         ₱1,200.00         ║
║  ─────────────────────────────────────────           ║
║  Total             47 txns        ₱31,450.00         ║
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  CASH FLOW                                           ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Opening Cash Fund:                ₱2,000.00         ║
║  (+) Cash Payments Received:      ₱18,650.00         ║
║  (+) Manual Cash-In:                 ₱500.00         ║
║      • Additional change from owner (10:30 AM)       ║
║  (-) Manual Cash-Out:             -₱1,350.00         ║
║      • Soap & chemicals purchase    -₱800.00         ║
║      • Employee meals              -₱350.00          ║
║      • Juan D.C. vale              -₱200.00          ║
║  ─────────────────────────────────────────           ║
║  Expected Cash in Drawer:         ₱19,800.00         ║
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  CASH COUNT (DENOMINATION BREAKDOWN)                 ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  ₱1,000  ×  15  =              ₱15,000.00           ║
║  ₱500    ×   6  =               ₱3,000.00           ║
║  ₱200    ×   3  =                 ₱600.00           ║
║  ₱100    ×   8  =                 ₱800.00           ║
║  ₱50     ×   5  =                 ₱250.00           ║
║  ₱20     ×   4  =                  ₱80.00           ║
║  ₱10     ×   3  =                  ₱30.00           ║
║  ₱5      ×   6  =                  ₱30.00           ║
║  ₱1      ×  10  =                  ₱10.00           ║
║  ─────────────────────────────────────────           ║
║  Actual Cash Counted:             ₱19,800.00         ║
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  VARIANCE                                            ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Expected:    ₱19,800.00                             ║
║  Actual:      ₱19,800.00                             ║
║  Variance:        ₱0.00  ✓ BALANCED                  ║
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  TOP SERVICES TODAY                                  ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  1. Basic Wash           22 txns       ₱5,280.00     ║
║  2. Premium Wash         11 txns       ₱4,620.00     ║
║  3. Interior Vacuum       9 txns       ₱1,800.00     ║
║  4. Complete Care Pkg     5 txns       ₱3,750.00     ║
║                                                      ║
╠══════════════════════════════════════════════════════╣
║  TOP EMPLOYEES BY COMMISSION                         ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  1. Juan Dela Cruz    18 services      ₱2,160.00     ║
║  2. Pedro Santos      15 services      ₱1,890.00     ║
║  3. Maria Garcia      14 services      ₱1,680.00     ║
║                                                      ║
╠══════════════════════════════════════════════════════╣
║                                                      ║
║  Cashier Signature: _______________                  ║
║                                                      ║
║  Manager Signature: _______________                  ║
║                                                      ║
║  Report generated: Mar 21, 2026 5:48 PM              ║
║                                                      ║
╚══════════════════════════════════════════════════════╝
```

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/shifts/open` | Open a new cashier shift (opening fund amount required) |
| `GET` | `/shifts/current` | Get the current open shift for the authenticated cashier + branch |
| `GET` | `/shifts/{id}` | Get shift details (includes movements, denominations, payment summary) |
| `GET` | `/shifts` | List shifts (filter by branch, cashier, date range, status, review status) |
| `POST` | `/shifts/{id}/cash-movement` | Record a cash-in or cash-out during the shift |
| `GET` | `/shifts/{id}/cash-movements` | List all cash movements for a shift |
| `POST` | `/shifts/{id}/close` | Close the shift — submit denomination count, system calculates everything |
| `GET` | `/shifts/{id}/report` | Get the full end-of-day report data (for display and print) |
| `PATCH` | `/shifts/{id}/review` | Manager review: approve or flag (with notes) |
| `PATCH` | `/shifts/{id}/reopen` | Manager reopen a closed shift (if allowed) |
| `PATCH` | `/shifts/{id}/void` | Manager void a shift (only if no transactions) |
| `GET` | `/reports/shift-variance` | Variance report: per-cashier trends over time |
| `GET` | `/settings/shift-config` | Get shift settings (variance thresholds, etc.) |
| `PUT` | `/settings/shift-config` | Update shift settings |

---

## Frontend Pages

### POS App

| Route | Page | Description |
|---|---|---|
| `/shift/open` | **Open Shift** | Simple form: "Enter your opening cash fund: [₱ ______]" with large input. Big "Start Shift" button. Only shown if no open shift exists. |
| `/shift` | **Active Shift Panel** | Compact shift info bar (or modal): shows shift start time, opening fund, current cash-in/out totals, transaction count. "Record Cash In/Out" button. Always accessible from POS nav. |
| `/shift/cash-movement` | **Cash Movement Form** | Type toggle (Cash In / Cash Out), Amount (large input), Reason (text), Reference (optional). Quick presets: "Supplies Purchase", "Employee Meals", "Additional Change", "Employee Vale". Submit adds to the shift log. |
| `/shift/close` | **Close Shift (End of Day)** | Multi-step flow: Step 1: Review summary (transactions, payments, cash movements). Step 2: Denomination count grid. Step 3: Variance display + confirm. |
| `/shift/report` | **Shift Report** | Full end-of-day report matching the printable format above. "Print" button. "Done" navigates to POS home. |

### POS Flow — How it works in practice

```
Cashier arrives → Sign in → /shift/open (enter ₱2,000 fund) → Start Shift
    ↓
POS Home shows: "Shift active since 8:02 AM | Fund: ₱2,000"
    ↓
Throughout the day:
  - Process transactions normally (no change to existing flow)
  - When buying supplies with drawer cash → POS nav → "Cash Out" → record ₱800 for soap
  - When owner drops off extra change → "Cash In" → record ₱500
    ↓
End of day → POS nav → "Close Shift"
    ↓
Step 1: System shows summary:
  "47 transactions | ₱31,250 revenue | ₱18,650 cash | 3 cash movements"
  [Continue]
    ↓
Step 2: Denomination count grid:
  ₱1,000 × [  15  ] = ₱15,000
  ₱500   × [   6  ] = ₱3,000
  ₱200   × [   3  ] = ₱600
  ...
  Running total: ₱19,800
  Expected: ₱19,800
  Variance: ₱0.00 ✓
  [Submit Count]
    ↓
Step 3: Report generated → Print or view
```

### Admin Dashboard

| Route | Page | Description |
|---|---|---|
| `/shifts` | **Shift List** | Data table: Date, Branch, Cashier, Opened/Closed time, Revenue, Variance (color-coded), Review Status badge. Filters: branch, cashier, date range, review status. |
| `/shifts/[id]` | **Shift Detail** | Full report view (same as POS report) + review actions. Approve/Flag buttons with notes textarea. Shows denomination breakdown, cash movement log, payment summary, top services, top employees. |
| `/reports/shift-variance` | **Variance Analysis** | Date range picker + branch/cashier filter. Variance trend line chart over time (per cashier). Table: cashier name, shift count, total variance, avg variance, largest shortage. Highlights cashiers with consistently negative variance in red. |

### POS Integration Points

1. **POS Home / Nav:** Show shift status indicator:
   - No active shift → Yellow banner: "No active shift. Open a shift to start." with link
   - Active shift → Green dot in nav: "Shift active | 8:02 AM | ₱2,000 fund"
   - After 8 PM with open shift → Amber banner: "End of day? Close your shift."

2. **Transaction Screen:** When completing a transaction, if no shift is open → block with message: "Open a shift before processing transactions."

3. **POS Nav:** Add a "Shift" pill/tab alongside New Transaction, Queue, History, Attendance. Shows cash movement count badge if there are entries today.

---

## Denomination Count UI (POS)

The denomination grid is the most interaction-heavy part. Design for speed:

```
┌─ Cash Count ──────────────────────────────────────────┐
│                                                        │
│  Enter the count for each denomination in your drawer  │
│                                                        │
│  BILLS                                                 │
│  ┌──────────┬──────────┬──────────────────────┐        │
│  │ ₱1,000   │ × [ 15 ] │ = ₱15,000.00        │        │
│  ├──────────┼──────────┼──────────────────────┤        │
│  │ ₱500     │ × [  6 ] │ = ₱3,000.00         │        │
│  ├──────────┼──────────┼──────────────────────┤        │
│  │ ₱200     │ × [  3 ] │ = ₱600.00           │        │
│  ├──────────┼──────────┼──────────────────────┤        │
│  │ ₱100     │ × [  8 ] │ = ₱800.00           │        │
│  ├──────────┼──────────┼──────────────────────┤        │
│  │ ₱50      │ × [  5 ] │ = ₱250.00           │        │
│  ├──────────┼──────────┼──────────────────────┤        │
│  │ ₱20      │ × [  4 ] │ = ₱80.00            │        │
│  └──────────┴──────────┴──────────────────────┘        │
│                                                        │
│  COINS                                                 │
│  ┌──────────┬──────────┬──────────────────────┐        │
│  │ ₱10      │ × [  3 ] │ = ₱30.00            │        │
│  ├──────────┼──────────┼──────────────────────┤        │
│  │ ₱5       │ × [  6 ] │ = ₱30.00            │        │
│  ├──────────┼──────────┼──────────────────────┤        │
│  │ ₱1       │ × [ 10 ] │ = ₱10.00            │        │
│  └──────────┴──────────┴──────────────────────┘        │
│                                                        │
│  ┌──────────────────────────────────────────────┐      │
│  │  Total Counted:          ₱19,800.00          │      │
│  │  Expected in Drawer:     ₱19,800.00          │      │
│  │  Variance:                   ₱0.00  ✓        │      │
│  └──────────────────────────────────────────────┘      │
│                                                        │
│              [ Submit Count & Close Shift ]             │
└────────────────────────────────────────────────────────┘
```

- Count inputs: `type="number"`, min=0, step=1, `w-20`, centered text, `text-lg font-mono`
- Subtotal updates in real-time as counts change
- Variance line: green text + ✓ if balanced, amber if within threshold, red + ✗ if flagged
- Denomination labels: `font-mono font-bold`, right-aligned
- Tab key moves between count inputs (natural keyboard flow for fast entry)
- Touch: each count input has +/- stepper buttons on mobile

---

## Cash Movement Quick Presets

To speed up cash-out recording, provide preset reason buttons:

```
Cash Out Presets:
[🧴 Supplies] [🍚 Meals] [👤 Employee Vale] [🔧 Equipment] [💵 Refund] [📝 Other]

Cash In Presets:
[💵 Additional Change] [📦 Owner Deposit] [📝 Other]
```

Tapping a preset fills the Reason field. Cashier only needs to type the Amount.

---

## SignalR Events

```csharp
// When a shift is opened:
await _hubContext.Clients
    .Group($"tenant:{tenantId}:branch:{branchId}")
    .SendAsync("ShiftUpdated", new { shiftId, status = "Open", cashierName });

// When a shift is closed:
await _hubContext.Clients
    .Group($"tenant:{tenantId}:branch:{branchId}")
    .SendAsync("ShiftUpdated", new { shiftId, status = "Closed", variance });

// Admin gets notified of flagged shifts:
await _hubContext.Clients
    .Group($"tenant:{tenantId}")
    .SendAsync("ShiftFlagged", new { shiftId, branchName, cashierName, variance });
```

---

## Domain Events

```csharp
public sealed record ShiftOpenedEvent(string ShiftId, string CashierId, 
    string BranchId, string TenantId, decimal OpeningFund) : IDomainEvent;

public sealed record ShiftClosedEvent(string ShiftId, string CashierId, 
    string BranchId, string TenantId, decimal Variance, 
    ReviewStatus AutoReviewStatus) : IDomainEvent;

public sealed record ShiftFlaggedEvent(string ShiftId, string CashierId,
    string BranchId, string TenantId, decimal Variance,
    string FlaggedByUserId, string Notes) : IDomainEvent;
```

---

## Settings (Tenant-Configurable)

```csharp
public sealed class ShiftSettings
{
    public decimal DefaultOpeningFund { get; set; } = 2000m;     // Suggested default
    public decimal AutoApproveThreshold { get; set; } = 50m;     // ±₱50 auto-approved
    public decimal FlagThreshold { get; set; } = 200m;           // ±₱200+ flagged
    public bool RequireShiftForTransactions { get; set; } = true; // Block POS if no shift
    public TimeOnly EndOfDayReminderTime { get; set; } = new(20, 0); // 8 PM reminder
}
```

Stored as JSON in a tenant settings table or as individual fields.

---

## Claude Code Prompts

### Prompt 15.9a — Cashier Shift Domain + Infrastructure

```
Replace the simple CashReconciliation feature with a full Cashier Shift system.

Domain/Entities/:
- CashierShift (all fields from the spec: shift timing, cash fund, computed totals,
  transaction summary, review fields)
- CashMovement (type, amount, reason, reference, movementTime)
- ShiftDenomination (denominationValue, count, subtotal)
- ShiftPaymentSummary (paymentMethod, transactionCount, totalAmount)

Domain/Enums/:
- ShiftStatus (Open, Closed, Voided)
- ReviewStatus (Pending, Approved, Flagged)
- CashMovementType (CashIn, CashOut)

Domain/Events/:
- ShiftOpenedEvent, ShiftClosedEvent, ShiftFlaggedEvent

Update Branch entity: add List<CashierShift> navigation.
Update User entity: add cashier shift and reviewer navigations.

Infrastructure/Persistence/Configurations/:
- CashierShiftConfiguration: indexes on [tenantId, branchId, shiftDate],
  [tenantId, cashierId, status]. Decimal precision on all money fields (10,2).
- CashMovementConfiguration: index on cashierShiftId. Cascade delete from shift.
- ShiftDenomination and ShiftPaymentSummary as owned entities or dependent
  with cascade delete from CashierShift.

Add DbSets. Apply tenant global filter on CashierShift.
Migration: "AddCashierShifts"

If the old CashReconciliation entity exists, remove it and create a new migration
to drop those tables.
```

### Prompt 15.9b — Cashier Shift Application Layer

```
Build the Cashier Shift CQRS feature in Application/Features/Shifts/:

Commands/:
- OpenShiftCommand(string BranchId, decimal OpeningCashFund)
  Validate: no open shift for this cashier+branch. Create CashierShift with status=Open.
  Publish ShiftOpenedEvent.

- RecordCashMovementCommand(string ShiftId, CashMovementType Type, decimal Amount, 
  string Reason, string? Reference)
  Validate: shift must be Open. Amount > 0. Create CashMovement record.

- CloseShiftCommand(string ShiftId, List<DenominationCountDto> Denominations)
  DenominationCountDto = { DenominationValue, Count }
  This is the critical handler:
  1. Verify shift is Open.
  2. Query all completed Transactions where cashierId matches AND transactionDate 
     falls within shift window (openedAt to now) AND branchId matches.
  3. Query all Payments for those transactions, group by PaymentMethod → create 
     ShiftPaymentSummary records.
  4. Calculate: TotalCashPayments, TotalNonCashPayments, TotalRevenue, 
     TotalTransactionCount, TotalCommissions (from TransactionEmployee), TotalDiscounts.
  5. Sum CashMovements by type → TotalCashIn, TotalCashOut.
  6. ExpectedCashInDrawer = OpeningCashFund + TotalCashPayments + TotalCashIn - TotalCashOut.
  7. Create ShiftDenomination records from input. ActualCashInDrawer = sum of subtotals.
  8. Variance = ActualCashInDrawer - ExpectedCashInDrawer.
  9. Auto-set ReviewStatus based on thresholds from tenant shift settings.
  10. Status = Closed, ClosedAt = now.
  11. Publish ShiftClosedEvent.

- ReviewShiftCommand(string ShiftId, ReviewStatus Status, string? Notes)
  Validate: shift must be Closed. Status must be Approved or Flagged.
  If Flagging, Notes are required. Set ReviewedById, ReviewedAt.
  If Flagged → publish ShiftFlaggedEvent.

- ReopenShiftCommand(string ShiftId)
  Validate: shift is Closed and ReviewStatus is Pending. No newer shift opened.
  Reset status to Open, clear computed totals, delete denominations and payment summaries.

- VoidShiftCommand(string ShiftId)
  Validate: shift has zero transactions. Set status = Voided.

Queries/:
- GetCurrentShiftQuery(string BranchId) → current open shift for authenticated cashier, 
  or null. Include cash movements.
- GetShiftByIdQuery → full detail with movements, denominations, payment summaries.
- GetShiftsQuery(string? BranchId, string? CashierId, DateTime? From, DateTime? To, 
  ShiftStatus? Status, ReviewStatus? ReviewStatus) → paginated list.
- GetShiftReportQuery(string ShiftId) → the complete end-of-day report data structure:
  all summary data, cash flow breakdown, denomination count, top services (top 5), 
  top employees by commission (top 5), cash movement log. This DTO is what the 
  printable report renders from.
- GetShiftVarianceReportQuery(DateTime From, DateTime To, string? BranchId, 
  string? CashierId) → per-cashier variance analysis: shift count, total variance,
  avg variance, largest shortage, trend data points.

ShiftEndpoints.cs — all routes from the API table above.
Add ShiftSettingsEndpoints for reading/updating threshold config.
```

### Prompt 15.9c — Cashier Shift Frontend (POS)

```
Build the POS shift management pages:

1. Shift status in POS layout:
   - If no open shift: show amber banner at top "No active shift — open one to start"
     with "Open Shift" button.
   - If shift is open: show green indicator in the POS top bar:
     "Shift: 8:02 AM | Fund: ₱2,000 | 12 txns | 3 cash moves"
   - Add "Shift" to the POS nav pills (between Queue and History).
   - If RequireShiftForTransactions is true AND no open shift, disable the 
     "New Transaction" button with tooltip "Open a shift first."

2. /shift/open — Open Shift page:
   - Large centered card: "Start Your Shift"
   - Opening Cash Fund input: ₱ amount, large (text-2xl font-mono), min-h-[56px]
   - Default value from tenant ShiftSettings.DefaultOpeningFund
   - "Start Shift" button (large, primary)
   - On success: redirect to POS home, show toast "Shift started"

3. /shift — Active Shift page (the "Shift" tab):
   - Shift info card: opened time, hours elapsed, opening fund
   - Cash movement log: list of all movements (time, type badge, amount, reason)
   - Two buttons: "Record Cash In" and "Record Cash Out"
   - Tapping either opens a bottom sheet/dialog:
     - Type is pre-selected based on which button was tapped
     - Amount input (large)
     - Reason: preset quick-select buttons + free text input
     - "Record" button
   - "Close Shift" button at the bottom (large, amber color)

4. /shift/close — Close Shift (multi-step):
   Step 1 — Summary Review:
     Cards showing: Transaction count, Total revenue, Cash payments, Non-cash payments,
     Cash-in total, Cash-out total.
     Cash movement log below.
     [Continue to Cash Count]

   Step 2 — Denomination Count:
     Grid as shown in the spec. Bills section, Coins section.
     Each row: denomination label | × | count input (with +/- on mobile) | = subtotal
     Running total at bottom.
     Expected amount displayed.
     Variance calculated live (color-coded).
     [Submit & Close Shift]

   Step 3 — Report:
     Full end-of-day report rendered from GetShiftReportQuery.
     [Print Report] button → window.print() with @media print CSS
     [Done] → redirect to POS home

5. @media print styles:
   - Hide POS chrome (top bar, nav)
   - Report renders as a clean document with the format from the spec
   - Include business name, branch, cashier, date
   - Signature lines at bottom
   - Fits on A4 or thermal receipt width (if using receipt printer)
```

### Prompt 15.9d — Cashier Shift Frontend (Admin)

```
Build the admin shift management pages:

1. /shifts — Shift List page:
   Data table columns: Date, Branch, Cashier, Time (opened–closed), Revenue, 
   Cash Payments, Variance (color-coded: green ≤₱50, amber ≤₱200, red >₱200),
   Review Status badge (Pending/Approved/Flagged).
   Filters: branch dropdown, cashier dropdown, date range picker, review status.

2. /shifts/[id] — Shift Detail page:
   Full end-of-day report (same layout as POS report view).
   Plus: Review panel at the top (only for Closed + Pending shifts):
   - Two buttons: "Approve" (green) and "Flag" (red)
   - Notes textarea (required for Flag)
   - On submit: PATCH /shifts/{id}/review
   - If already reviewed: show review status, reviewer name, date, notes (read-only)
   
   Plus: "Reopen Shift" button (only if Closed + Pending + no newer shift).

3. /reports/shift-variance — Variance Analysis page:
   - Date range picker + branch filter + cashier filter
   - Variance trend line chart (Recharts): X = date, Y = variance amount. 
     One line per selected cashier. Highlight the zero line.
   - Summary table: Cashier | Shifts | Total Variance | Avg Variance | 
     Largest Over | Largest Short
   - Rows with avg variance worse than -₱100 highlighted in red.

4. /settings → add "Shift Settings" section:
   - Default Opening Fund: ₱ input
   - Auto-Approve Threshold: ₱ input (shifts within this variance auto-approve)
   - Flag Threshold: ₱ input (shifts beyond this are auto-flagged)
   - Require Shift for Transactions: toggle switch
   - End-of-Day Reminder Time: time picker
   - Save button
```

---

## Phase Summary

| Prompt | What | Layer |
|---|---|---|
| 15.9a | Domain entities, enums, events, EF configs, migration | Backend |
| 15.9b | CQRS commands/queries, CloseShift calculation engine, endpoints | Backend |
| 15.9c | POS: open shift, cash movements, close shift wizard, denomination count, printable report | Frontend (POS) |
| 15.9d | Admin: shift list, shift detail + review, variance analysis, settings | Frontend (Admin) |

**Total: 4 prompts replacing the original single Prompt 15.9.**

Run in order: 15.9a → 15.9b → 15.9c → 15.9d.
