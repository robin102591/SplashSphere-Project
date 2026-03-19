# SplashSphere — Phase 15: Value-Add Features Addendum

> **Purpose:** This document defines 5 new features that directly generate or protect revenue for the car wash owner. Each feature includes domain models, business rules, API endpoints, frontend pages, and Claude Code prompts. These are executed as **Phase 15** after the core system is complete.

---

## Feature 1: Cash Advance & Loan Tracking

### Why It Matters

In Philippine car wash operations, employees regularly request salary advances ("vale"). Without tracking, owners lose money to unrecorded advances and disputed deductions. This plugs directly into the existing PayrollEntry.deductionAmount field.

### Domain Models

```csharp
public sealed class CashAdvance : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string? ApprovedById { get; set; }  // UserId who approved
    public decimal Amount { get; set; }
    public decimal TotalDeducted { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? Reason { get; set; }
    public CashAdvanceStatus Status { get; set; } = CashAdvanceStatus.Pending;
    public bool IsFullyPaid { get; set; }
    public decimal? CustomDeductionAmount { get; set; }  // Fixed amount per payroll
    public decimal? MinDeductionAmount { get; set; }
    public decimal? MaxDeductionAmount { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Navigation
    public Employee Employee { get; set; } = null!;
    public User? ApprovedBy { get; set; }
    public List<CashAdvanceDeduction> Deductions { get; set; } = [];
}

public sealed class CashAdvanceDeduction : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string CashAdvanceId { get; set; } = string.Empty;
    public string PayrollEntryId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? OverrideAmount { get; set; }  // Manager override
    public string? OverrideReason { get; set; }
    public DateTime DeductionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Navigation
    public CashAdvance CashAdvance { get; set; } = null!;
    public PayrollEntry PayrollEntry { get; set; } = null!;
}

public enum CashAdvanceStatus { Pending, Approved, Rejected, FullyPaid }
```

### Business Rules

1. Employee can request a cash advance. Default status = PENDING.
2. Manager approves or rejects. On approval, `RemainingBalance = Amount`.
3. During payroll close (OPEN → CLOSED), for each employee with active advances:
   - Default deduction = 25% of remaining balance (configurable per advance).
   - If `CustomDeductionAmount` is set, use that instead.
   - Deduction cannot exceed employee's gross pay or remaining balance.
   - Deduction cannot make net pay negative.
   - Create `CashAdvanceDeduction` record linked to `PayrollEntry`.
   - Update `CashAdvance.TotalDeducted` and `RemainingBalance`.
   - If `RemainingBalance <= 0`, set `IsFullyPaid = true`, `Status = FullyPaid`.
4. Manager can override deduction amount during CLOSED period (before PROCESSED).
5. Employee cannot have active advances exceeding a configurable limit (e.g., ₱5,000 total).

### API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/cash-advances` | Request a new cash advance |
| `GET` | `/cash-advances` | List advances (filter by employee, status, paid) |
| `GET` | `/cash-advances/{id}` | Advance details with deduction history |
| `PATCH` | `/cash-advances/{id}/approve` | Approve (manager) |
| `PATCH` | `/cash-advances/{id}/reject` | Reject (manager) |
| `PUT` | `/cash-advances/{id}/deduction-settings` | Set custom deduction amount/limits |
| `GET` | `/employees/{id}/cash-advances` | All advances for an employee |

### Frontend Pages (Admin)

| Route | Page |
|---|---|
| `/cash-advances` | List with filters: employee, status, branch. Shows amount, remaining, status badge. |
| `/cash-advances/[id]` | Detail: advance info, deduction history timeline, adjustment controls |
| `/employees/[id]` → new tab | "Advances" tab on employee detail showing all advances + total outstanding |
| `/payroll/[id]` | Update payroll detail to show deduction breakdown per employee |

### Payroll Integration

Update the `ClosePayrollPeriodCommand` to automatically calculate and apply cash advance deductions before creating `PayrollEntry` records. The deduction feeds into `PayrollEntry.deductionAmount`.

---

## Feature 2: Expense Tracking & Profit Dashboard

### Why It Matters

The current dashboard shows revenue but not costs. The owner sees "₱32,450 today" but doesn't know if they profited ₱15,000 or lost ₱5,000. Expense tracking turns the dashboard from a revenue counter into a profit monitor.

### Domain Models

```csharp
public sealed class Expense : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string RecordedById { get; set; } = string.Empty;  // UserId
    public string CategoryId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public string? ReceiptReference { get; set; }  // Receipt number or image ref
    public DateTime ExpenseDate { get; set; }
    public ExpenseFrequency Frequency { get; set; } = ExpenseFrequency.OneTime;
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public User RecordedBy { get; set; } = null!;
    public ExpenseCategory Category { get; set; } = null!;
}

public sealed class ExpenseCategory : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }  // Lucide icon name
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Expense> Expenses { get; set; } = [];
}

public enum ExpenseFrequency { OneTime, Daily, Weekly, Monthly }
```

### Default Expense Categories (Seeded)

```
Water Bill, Electricity, Rent, Soap & Chemicals, Equipment Maintenance,
Employee Meals/Snacks, Transportation, Supplies (towels, sponges),
Miscellaneous, Insurance, Taxes & Permits
```

### Business Rules

1. Any authenticated user in the branch can record an expense.
2. Expense is always tied to a branch and a date.
3. Categories are tenant-configurable. Seed defaults on onboarding.
4. Recurring expenses are tracked by frequency but must still be recorded (no auto-creation — just a reminder flag).
5. Expenses are never deleted — only soft-deleted or marked void.

### API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/expenses` | Record an expense |
| `GET` | `/expenses` | List expenses (filter by branch, category, date range) |
| `GET` | `/expenses/{id}` | Expense detail |
| `PUT` | `/expenses/{id}` | Update expense |
| `DELETE` | `/expenses/{id}` | Soft delete |
| `GET` | `/expense-categories` | List categories |
| `POST` | `/expense-categories` | Create category |
| `GET` | `/reports/profit-loss` | **P&L report**: revenue - COGS - expenses by period |

### Profit & Loss Report Calculation

```
FOR a given date range and optional branch filter:

Revenue     = SUM(Transaction.finalAmount WHERE status = COMPLETED)
COGS        = SUM(TransactionMerchandise.quantity × Merchandise.cost)  // cost, not price
Gross Profit = Revenue - COGS

Expenses    = SUM(Expense.amount) grouped by category
Total Expenses = SUM(all expenses)

Net Profit  = Gross Profit - Total Expenses
Margin      = (Net Profit / Revenue) × 100
```

### Frontend Pages (Admin)

| Route | Page |
|---|---|
| `/expenses` | List with date range picker, branch filter, category filter. Daily/weekly/monthly grouping toggle. |
| `/expenses/new` | Quick expense entry form: amount, category (dropdown), description, date, vendor |
| `/reports/profit-loss` | **P&L Dashboard**: Revenue card, Expenses card, Net Profit card. Expense breakdown by category (bar chart). Revenue vs Expenses trend (dual-axis line chart). Date range + branch filter. |

### Dashboard Integration

Add to the main dashboard:
- New KPI card: "Today's Expenses: ₱X,XXX" with category tooltip
- New KPI card: "Est. Net Profit: ₱X,XXX" (revenue minus expenses for today)

---

## Feature 3: Customer Loyalty & Rewards

### Why It Matters

Repeat customers are the lifeblood of a car wash. A points system increases visit frequency by 20-30%. The tier system (Bronze → Platinum) creates aspiration and status — Filipinos respond strongly to recognition and tier-based rewards.

### Domain Model Additions

Add to existing `Customer` entity:
```csharp
// Add these properties to the existing Customer entity
public int LoyaltyPoints { get; set; }
public MembershipTier MembershipTier { get; set; } = MembershipTier.Bronze;
public decimal TotalSpent { get; set; }
public int TotalVisits { get; set; }
public DateTime? LastVisitDate { get; set; }
public string? ReferralCode { get; set; }  // Unique, auto-generated
public string? ReferredById { get; set; }  // Customer who referred them
```

New entities:
```csharp
public sealed class LoyaltyTransaction : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? TransactionId { get; set; }  // Source transaction if earned from purchase
    public int Points { get; set; }  // Positive = earned, Negative = redeemed
    public LoyaltyPointType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Navigation
    public Customer Customer { get; set; } = null!;
    public Transaction? Transaction { get; set; }
}

public sealed class MembershipTierConfig : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public MembershipTier Tier { get; set; }
    public int MinPoints { get; set; }
    public decimal MinSpent { get; set; }
    public int MinVisits { get; set; }
    public decimal DiscountRate { get; set; }  // e.g., 0.05 for 5%
    public string? Benefits { get; set; }  // JSON description of benefits
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum MembershipTier { Bronze, Silver, Gold, Platinum }
public enum LoyaltyPointType
{
    EarnedPurchase, EarnedReferral, EarnedBonus, EarnedBirthday,
    RedeemedDiscount, RedeemedService, Expired, Adjusted
}
```

### Tier Configuration (Default)

```
Bronze:   0 points,    ₱0 spent,     0 visits  → 0% discount
Silver:   1,000 pts,   ₱1,000 spent, 5 visits  → 5% discount
Gold:     3,000 pts,   ₱3,000 spent, 12 visits → 10% discount
Platinum: 8,000 pts,   ₱8,000 spent, 25 visits → 15% discount
```

Points earning: **1 point per peso spent**. Referral bonus: 100 points.

### Business Rules

1. Points earned automatically when a transaction completes: `points = floor(finalAmount)`.
2. After earning points, check if customer qualifies for tier upgrade (points + spent + visits).
3. Tier discount applies to the NEXT transaction (not the one earning the points).
4. Points expire after 12 months if unused (Hangfire job checks monthly).
5. Redemption: customer can spend points for discounts at POS. 100 points = ₱1 discount.
6. Referral: when a referred customer completes first transaction, referrer gets 100 bonus points.
7. Tier configs are tenant-customizable.
8. VIP queue priority: Gold and Platinum customers get automatic VIP priority in queue.

### Transaction Integration

Update `CreateTransactionCommandHandler`:
- After Step 9 (save), if customer exists:
  - Award loyalty points based on `finalAmount`
  - Increment `Customer.TotalVisits` and `Customer.TotalSpent`
  - Set `Customer.LastVisitDate`
  - Check for tier upgrade
  - If customer has tier discount and it was applied, record as `LoyaltyTransaction` type `RedeemedDiscount`

### API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/customers/{id}/loyalty` | Points balance, tier, history |
| `POST` | `/customers/{id}/loyalty/redeem` | Redeem points for discount |
| `GET` | `/customers/{id}/loyalty/history` | Points transaction log |
| `GET` | `/loyalty/tier-config` | Get tier configuration |
| `PUT` | `/loyalty/tier-config` | Update tier configuration (admin) |
| `POST` | `/loyalty/points/adjust` | Manual points adjustment (admin) |

### Frontend Pages

**POS Integration:**
- On vehicle/customer lookup: show tier badge, points balance, and available discount
- During payment: option to apply tier discount or redeem points
- After completion: show "You earned X points! Y points to next tier"

**Admin:**
| Route | Page |
|---|---|
| `/customers/[id]` → new tab | "Loyalty" tab: points balance, tier, history, manual adjust |
| `/settings/loyalty` | Tier configuration: edit points/spending/visit thresholds, discount rates |
| `/reports/loyalty` | Loyalty report: members by tier, points issued vs redeemed, retention metrics |

---

## Feature 4: SMS Notifications

### Why It Matters

Filipinos overwhelmingly prefer SMS over email. Three messages drive real business value: "your car is ready" (reduces wait area congestion), "you're next in queue" (reduces no-shows), and weekly payroll summary (builds employee trust).

### Domain Model

```csharp
public sealed class SmsNotification : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string? EmployeeId { get; set; }
    public SmsType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public SmsStatus Status { get; set; } = SmsStatus.Pending;
    public string? ProviderMessageId { get; set; }  // From SMS gateway
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class SmsTemplate : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public SmsType Type { get; set; }
    public string Template { get; set; } = string.Empty;  // e.g., "Hi {customerName}, your {vehiclePlate} is ready!"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum SmsType
{
    ServiceComplete,     // "Your car is ready for pickup"
    QueueCalled,         // "You're next! Please proceed to the bay"
    QueueReminder,       // "You are #3 in line, ~15 min wait"
    PayrollSummary,      // Weekly: "Your pay this week: ₱X,XXX"
    LoyaltyWelcome,      // "Welcome to Silver tier!"
    PromotionalOffer,    // Marketing
    Custom               // Manual one-off
}

public enum SmsStatus { Pending, Sent, Failed, Delivered }
```

### SMS Gateway Integration

Use a Philippine SMS gateway. Recommended: **Semaphore** (semaphore.co) or **Engagespark**. Both have simple REST APIs and support Philippine mobile numbers.

```csharp
public interface ISmsService
{
    Task<Result<string>> SendAsync(string phoneNumber, string message, CancellationToken ct);
}

// Implementation calls the gateway API:
// POST https://api.semaphore.co/api/v4/messages
// { apikey, number, message, sendername }
```

### Business Rules

1. SMS is sent as a fire-and-forget Hangfire job (don't block the main flow).
2. Templates are tenant-configurable with placeholders: `{customerName}`, `{vehiclePlate}`, `{queueNumber}`, `{totalAmount}`, `{employeeName}`, `{commission}`.
3. Rate limit: max 5 SMS per customer per day (prevent spam).
4. SMS sending can be toggled on/off per tenant in settings.
5. Log every SMS attempt with status for cost tracking and debugging.

### Trigger Points

| Event | SMS Type | Recipient |
|---|---|---|
| Transaction → COMPLETED | `ServiceComplete` | Customer (if contact exists) |
| QueueEntry → CALLED | `QueueCalled` | Customer (if contact exists) |
| PayrollPeriod → PROCESSED | `PayrollSummary` | Each employee (with their total) |
| Customer tier upgrade | `LoyaltyWelcome` | Customer |

### API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/sms/send` | Send a one-off SMS (admin) |
| `GET` | `/sms/history` | SMS log (filter by type, status, date) |
| `GET` | `/sms/templates` | List templates |
| `PUT` | `/sms/templates/{id}` | Edit template |
| `GET` | `/settings/sms` | SMS settings (enabled, gateway config) |
| `PUT` | `/settings/sms` | Update SMS settings |

### Frontend Pages (Admin)

| Route | Page |
|---|---|
| `/settings/sms` | SMS config: enable/disable, gateway API key, sender name. Template editor with preview. |
| `/settings/sms/history` | SMS log table: recipient, type, status, message preview, sent date. |

---

## Feature 5: Daily Cash Reconciliation

### Why It Matters

Cash is king in Philippine car washes — most transactions are cash. Without reconciliation, the owner has no way to know if ₱500 went missing from the drawer. This feature catches theft and honest mistakes, and builds accountability per cashier. Critical for owners managing multiple branches remotely.

### Domain Model

```csharp
public sealed class CashReconciliation : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string CashierId { get; set; } = string.Empty;  // User who did the count
    public DateTime ReconciliationDate { get; set; }  // Date being reconciled
    public decimal ExpectedCash { get; set; }   // System-calculated from cash payments
    public decimal ActualCash { get; set; }     // Physically counted
    public decimal Variance { get; set; }       // Actual - Expected (negative = short)
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Pending;
    public string? Notes { get; set; }
    public string? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Navigation
    public Branch Branch { get; set; } = null!;
    public User Cashier { get; set; } = null!;
    public User? ApprovedBy { get; set; }
    public List<CashDenomination> Denominations { get; set; } = [];
}

public sealed class CashDenomination
{
    public string Id { get; set; } = string.Empty;
    public string ReconciliationId { get; set; } = string.Empty;
    public int DenominationValue { get; set; }  // 1000, 500, 200, 100, 50, 20, 10, 5, 1
    public int Count { get; set; }
    public decimal Subtotal { get; set; }  // DenominationValue × Count
    public CashReconciliation Reconciliation { get; set; } = null!;
}

public enum ReconciliationStatus { Pending, Approved, Flagged }
```

### Business Rules

1. **Expected cash** = SUM of all `Payment.amount` WHERE `method = CASH` AND transaction is `COMPLETED` AND `transactionDate` = reconciliation date AND `branchId` matches. Minus any starting float/change fund.
2. Cashier physically counts the drawer and enters denomination counts.
3. System calculates `ActualCash` = SUM(denomination × count).
4. `Variance = ActualCash - ExpectedCash`.
5. Variance thresholds (tenant-configurable):
   - Within ±₱50 → auto-approve (minor discrepancy)
   - ±₱51 to ±₱200 → Pending review
   - Over ±₱200 → Flagged (requires manager investigation)
6. One reconciliation per branch per day. Cannot do it twice for the same date (unless manager reopens).
7. Track per-cashier variance history to identify patterns.

### Denomination Counting

Philippine denominations: ₱1000, ₱500, ₱200, ₱100, ₱50, ₱20, ₱10, ₱5, ₱1, ₱0.25

The cashier enters count per denomination. System calculates total.

### API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/cash-reconciliation` | Submit daily cash count |
| `GET` | `/cash-reconciliation` | List reconciliations (filter by branch, date range, status) |
| `GET` | `/cash-reconciliation/{id}` | Detail with denominations |
| `GET` | `/cash-reconciliation/expected` | Get expected cash for branch/date (preview before counting) |
| `PATCH` | `/cash-reconciliation/{id}/approve` | Approve (manager) |
| `PATCH` | `/cash-reconciliation/{id}/flag` | Flag for investigation |
| `GET` | `/reports/cash-variance` | Variance report by cashier/branch over time |

### Frontend Pages

**POS (end-of-day workflow):**
| Route | Page |
|---|---|
| `/cash-count` | Denomination entry form: grid of denomination × count inputs. Shows running total. Auto-calculates variance against expected. Submit button. |

**Admin:**
| Route | Page |
|---|---|
| `/cash-reconciliation` | List: date, branch, cashier, expected, actual, variance (color-coded: green=ok, amber=review, red=flagged), status. |
| `/cash-reconciliation/[id]` | Detail: denomination breakdown, variance, notes, approve/flag actions. |
| `/reports/cash-variance` | Variance trend chart per cashier. Highlights cashiers with consistent shortages. |

### POS Integration

Add to POS Home:
- End-of-day banner: "Time to count your drawer?" with link to `/cash-count`
- Shows after 8 PM or configurable time
- If today hasn't been reconciled, show a reminder badge

---

## Claude Code Prompts — Phase 15

### Prompt 15.1 — Cash Advance Domain + Infrastructure

```
Add the Cash Advance feature to the existing SplashSphere project:

1. Domain/Entities/: CashAdvance, CashAdvanceDeduction entities
2. Domain/Enums/: CashAdvanceStatus enum
3. Update Employee entity: add List<CashAdvance> CashAdvances navigation
4. Update PayrollEntry entity: add List<CashAdvanceDeduction> CashAdvanceDeductions navigation
5. Update User entity: add approver navigation for CashAdvance
6. Infrastructure/Persistence/Configurations/: CashAdvanceConfiguration, CashAdvanceDeductionConfiguration
7. Add DbSets to SplashSphereDbContext
8. Generate migration: "AddCashAdvances"

Business rules: CashAdvance has composite index on [tenantId, employeeId, status].
CashAdvanceDeduction has unique on [cashAdvanceId, payrollEntryId].
Apply global tenant filter on CashAdvance.
```

### Prompt 15.2 — Cash Advance Application Layer + Payroll Integration

```
Build the Cash Advance CQRS feature:

Commands:
- RequestCashAdvanceCommand (employeeId, amount, reason)
- ApproveCashAdvanceCommand (cashAdvanceId) — sets approved, fills RemainingBalance
- RejectCashAdvanceCommand (cashAdvanceId)
- UpdateDeductionSettingsCommand (cashAdvanceId, customDeductionAmount?, min?, max?)

Queries:
- GetCashAdvancesQuery (filter by employee, status, branch, paginated)
- GetCashAdvanceByIdQuery (includes deduction history)
- GetEmployeeCashAdvanceSummaryQuery (total outstanding for an employee)

CRITICAL: Update the existing ClosePayrollPeriodCommand handler:
After calculating base salary and commissions, BEFORE creating PayrollEntry:
1. Get all approved, not-fully-paid advances for the employee
2. For each advance: calculate deduction (custom amount or 25% of remaining, whichever)
3. Ensure total deductions don't make net pay negative
4. Create CashAdvanceDeduction records
5. Update advance.TotalDeducted and advance.RemainingBalance
6. Sum all deductions into PayrollEntry.deductionAmount

CashAdvanceEndpoints.cs — all routes. Add deduction details to PayrollEntry DTOs.
```

### Prompt 15.3 — Cash Advance Frontend (Admin)

```
Build Cash Advance admin pages:

1. /cash-advances — list page with filters (employee dropdown, status, branch)
   Table columns: Employee, Amount, Remaining, Status badge, Request Date, Actions
   
2. /cash-advances/[id] — detail page:
   - Advance info card (amount, remaining, status, dates)
   - Deduction history timeline (date, payroll period, amount, running balance)
   - Deduction settings panel (custom amount, min/max) — editable if not FullyPaid
   - Approve/Reject buttons (if Pending)

3. Update /employees/[id] — add "Advances" tab:
   - List of all advances for this employee
   - Total outstanding amount card
   - "New Advance" button opens form dialog

4. Update /payroll/[id] — show deduction breakdown:
   - In each PayrollEntry row, show a breakdown tooltip: 
     "Commission: ₱X, Advances: ₱Y, Net: ₱Z"
```

### Prompt 15.4 — Expense Tracking Domain + Application

```
Add the Expense Tracking feature:

Domain:
- Expense entity, ExpenseCategory entity, ExpenseFrequency enum
- Add navigation lists to Tenant and Branch

Infrastructure:
- EF configurations for both entities
- Seed default expense categories during DataSeeder
- Migration: "AddExpenseTracking"

Application Features/Expenses/:
- RecordExpenseCommand (branchId, categoryId, amount, description, expenseDate, vendor?)
- UpdateExpenseCommand
- DeleteExpenseCommand (soft delete)
- GetExpensesQuery (filter by branch, category, date range, paginated)
- GetExpenseCategoriesQuery
- CreateExpenseCategoryCommand
- GetProfitLossReportQuery(DateTime from, DateTime to, string? branchId):
  Calculate: Revenue (completed transactions), COGS (merchandise cost), 
  Gross Profit, Expenses by category, Total Expenses, Net Profit, Margin %

ExpenseEndpoints.cs + update ReportEndpoints.cs with P&L route.
```

### Prompt 15.5 — Expense Tracking Frontend + Profit Dashboard

```
Build Expense pages:

1. /expenses — list page with date range picker, branch filter, category filter
   Group by date with daily subtotals. Columns: Date, Category, Description, Amount, Vendor.

2. /expenses/new — quick entry form (optimized for speed, minimal fields):
   Amount (large input), Category (dropdown), Description, Date (defaults today), Vendor

3. /reports/profit-loss — P&L Dashboard:
   - Three big cards: Revenue, Total Expenses, Net Profit (green or red)
   - Expense breakdown by category — horizontal bar chart
   - Revenue vs Expenses trend — dual-axis line chart over selected period
   - Date range picker + branch filter
   - Table below charts with daily breakdown

4. Update main dashboard (/ page):
   - Add "Today's Expenses" KPI card
   - Add "Est. Net Profit" KPI card (revenue - expenses for today)
```

### Prompt 15.6 — Customer Loyalty Domain + Application

```
Add the Customer Loyalty feature:

Domain:
- Add loyalty fields to existing Customer entity: LoyaltyPoints, MembershipTier, 
  TotalSpent, TotalVisits, LastVisitDate, ReferralCode, ReferredById
- New entities: LoyaltyTransaction, MembershipTierConfig
- New enums: MembershipTier, LoyaltyPointType
- Add navigation: Customer → List<LoyaltyTransaction>

Infrastructure:
- EF configurations (add new fields to CustomerConfiguration)
- LoyaltyTransactionConfiguration, MembershipTierConfigConfiguration
- Seed default tier configs during DataSeeder
- Migration: "AddCustomerLoyalty"

Application Features/Loyalty/:
- AwardLoyaltyPointsCommand (customerId, transactionId, amount) — called after transaction
- RedeemPointsCommand (customerId, points, description) — returns discount amount
- CheckTierUpgradeCommand (customerId) — auto-upgrades if qualified
- AdjustPointsCommand (customerId, points, reason) — admin manual adjustment
- GetCustomerLoyaltyQuery (customerId) — points, tier, history
- GetTierConfigQuery (tenantId)
- UpdateTierConfigCommand

CRITICAL: Update CreateTransactionCommandHandler:
After Step 9, if transaction has a customer:
1. Award points: floor(finalAmount) points
2. Increment TotalVisits, TotalSpent, set LastVisitDate
3. Check tier upgrade
4. Create LoyaltyTransaction record

Update POS pricing: if customer has tier discount, apply as transaction-level discount.

LoyaltyEndpoints.cs — all routes.
```

### Prompt 15.7 — Customer Loyalty Frontend

```
Build Loyalty pages:

POS Integration:
1. Update vehicle/customer lookup: show tier badge (Bronze/Silver/Gold/Platinum with color),
   points balance, and "X% tier discount available" below customer name
2. Update transaction screen: auto-apply tier discount if customer has one
   Show line: "Gold Member Discount (10%): -₱60"
3. After transaction complete, in receipt dialog: 
   "You earned 600 points! 400 more to Gold tier"

Admin:
1. /customers/[id] → new "Loyalty" tab:
   - Points balance, current tier, tier progress bar (points to next tier)
   - Points history table: date, type, points (+/-), description
   - Manual adjust button (admin grants/removes points)

2. /settings/loyalty — Tier Configuration:
   - Editable table: Tier | Min Points | Min Spent | Min Visits | Discount Rate
   - Save button

3. /reports/loyalty:
   - Members by tier — pie chart
   - Points issued vs redeemed this month
   - Top customers by points
   - Tier upgrade rate
```

### Prompt 15.8 — SMS Notifications

```
Add SMS Notification feature:

Domain:
- SmsNotification entity, SmsTemplate entity
- SmsType and SmsStatus enums

Infrastructure:
- EF configurations + migration "AddSmsNotifications"
- Infrastructure/ExternalServices/SmsService.cs:
  ISmsService interface in Application/Interfaces/
  SemaphoreSmsService implementation (POST to Semaphore API)
  Uses HttpClient, reads API key from configuration
- Seed default SMS templates per type

Application Features/Sms/:
- SendSmsCommand (recipientPhone, type, templateVariables) — enqueues as Hangfire job
- GetSmsHistoryQuery (filter by type, status, date range)
- GetSmsTemplatesQuery
- UpdateSmsTemplateCommand

Create MediatR notification handlers that trigger SMS:
- TransactionCompletedEventHandler → if customer has phone, send ServiceComplete SMS
- QueueEntryCalledEventHandler → if customer has phone, send QueueCalled SMS
- PayrollProcessedEventHandler → for each employee, send PayrollSummary SMS

SmsEndpoints.cs. Add SMS settings to tenant settings page.
Env var: Sms__ApiKey, Sms__SenderName
```

### Prompt 15.9 — Daily Cash Reconciliation

```
Add Cash Reconciliation feature:

Domain:
- CashReconciliation entity, CashDenomination entity (child)
- ReconciliationStatus enum
- Add navigation to Branch and User

Infrastructure:
- EF configurations. CashDenomination is owned by CashReconciliation (not a separate table)
  or a dependent entity with cascade delete.
- Migration: "AddCashReconciliation"

Application Features/CashReconciliation/:
- GetExpectedCashQuery(branchId, date) — SUM of cash payments for completed transactions
- SubmitCashCountCommand(branchId, date, denominations[], notes?)
  Calculate ActualCash from denominations, Variance = Actual - Expected
  Auto-set status based on threshold: ±50 = Approved, ±200 = Pending, else Flagged
- ApproveCashReconciliationCommand
- FlagCashReconciliationCommand
- GetCashReconciliationsQuery (filter by branch, date range, status)
- GetCashVarianceReportQuery (per cashier, date range) — for trend analysis

CashReconciliationEndpoints.cs

POS: Add /cash-count page:
- Shows expected cash amount (from API)
- Denomination grid: each PH denomination (₱1000 down to ₱1) with count input
- Running total that updates as counts are entered
- Variance display (green/amber/red based on threshold)
- Submit button

Admin:
- /cash-reconciliation — list page with variance color coding
- /cash-reconciliation/[id] — detail with denomination breakdown
- /reports/cash-variance — cashier variance trend chart
```

---

## Phase Summary

| Prompt | Feature | Layer |
|---|---|---|
| 15.1 | Cash Advance — domain + infrastructure | Backend |
| 15.2 | Cash Advance — application + payroll integration | Backend |
| 15.3 | Cash Advance — admin frontend | Frontend |
| 15.4 | Expense Tracking — domain + application | Backend |
| 15.5 | Expense Tracking — frontend + profit dashboard | Frontend |
| 15.6 | Customer Loyalty — domain + application + transaction integration | Backend |
| 15.7 | Customer Loyalty — POS + admin frontend | Frontend |
| 15.8 | SMS Notifications — full stack | Full stack |
| 15.9 | Cash Reconciliation — full stack | Full stack |

**Total: 9 prompts in Phase 15.**

Run order: 15.1 → 15.2 → 15.3 (cash advance complete), then 15.4 → 15.5 (expenses complete), then 15.6 → 15.7 (loyalty complete), then 15.8 (SMS), then 15.9 (cash reconciliation). Each feature is independent — you can reorder if you prefer a different priority.
