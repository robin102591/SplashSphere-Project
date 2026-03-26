# SplashSphere — Payroll Module Specification

## 1. Overview

This document is the single source of truth for the SplashSphere payroll module. It covers the full lifecycle from configuration through computation, adjustment, finalization, and payslip generation — tailored for Philippine carwash operations.

**Business context:** Philippine carwash businesses are labor-intensive. Most employees are commission-based (paid per service performed), some are daily-rate workers (cashiers, security, maintenance), and some are hybrid (base pay + commission). Tips are common (cash and GCash). Government-mandated deductions (SSS, PhilHealth, Pag-IBIG) apply to all employees.

---

## 2. Current State (What Exists)

### 2.1 Entities

| Entity | Table | Purpose |
|--------|-------|---------|
| `PayrollPeriod` | `PayrollPeriods` | Weekly period with state machine (Open → Closed → Processed) |
| `PayrollEntry` | `PayrollEntries` | Per-employee row within a period — base salary, commissions, bonuses, deductions |
| `PayrollSettings` | `PayrollSettings` | Per-tenant config — `CutOffStartDay` (DayOfWeek) |
| `PayrollAdjustmentTemplate` | `PayrollAdjustmentTemplates` | Reusable bonus/deduction presets (e.g., SSS, PhilHealth) |

### 2.2 Enums

| Enum | Values |
|------|--------|
| `PayrollStatus` | Open (1), Closed (2), Processed (3) |
| `EmployeeType` | Commission (1), Daily (2) |
| `AdjustmentType` | Bonus (1), Deduction (2) |
| `CommissionType` | Percentage (1), FixedAmount (2), Hybrid (3) |
| `PaymentMethod` | Cash (1), GCash (2), CreditCard (3), DebitCard (4), BankTransfer (5) |

### 2.3 PayrollEntry Fields (Current)

```
Id, TenantId, PayrollPeriodId, EmployeeId
EmployeeTypeSnapshot (enum)    — frozen at close time
DaysWorked (int)               — attendance count for period
DailyRateSnapshot (decimal?)   — frozen daily rate (null for Commission type)
BaseSalary (decimal)           — DailyRate × DaysWorked (0 for Commission)
TotalCommissions (decimal)     — sum of all transaction commission splits
Bonuses (decimal)              — admin-adjustable (generic)
Deductions (decimal)           — admin-adjustable (generic)
Notes (string?)
NetPay = BaseSalary + TotalCommissions + Bonuses - Deductions  (computed, not stored)
```

### 2.4 Working Features

- **Commission matrix:** 3 types (Percentage, FixedAmount, Hybrid) per (Service, VehicleType, Size). Equal split among assigned employees.
- **Package commission:** Percentage-only per (Package, VehicleType, Size). Equal split.
- **Payroll close:** Aggregates commissions from `TransactionEmployee` (Completed, by `CompletedAt` UTC) + attendance from `Attendance` (Manila DateOnly). Creates `PayrollEntry` rows.
- **Inline editing:** Bonuses/Deductions/Notes editable while period is Closed.
- **Bulk adjustments:** Select entries → apply template or custom amount. Additive (not replacement).
- **Employee detail sheet:** Drill-down with commission line items + attendance records.
- **Adjustment templates:** CRUD with soft-delete. Used in bulk apply dialog.
- **Configurable cut-off day:** Per-tenant `CutOffStartDay`. Daily Hangfire job creates/closes periods.
- **Manual period creation:** Admin can create period with custom date range.
- **Tips on transaction:** `Transaction.TipAmount` tracked but NOT aggregated into payroll.

### 2.5 Hangfire Jobs

| Job | Schedule | Behavior |
|-----|----------|----------|
| `RunDailyPayrollJobAsync` | Daily 00:05 PHT | Auto-close expired Open periods; create new period for tenants whose `CutOffStartDay` matches today. Period covers previous 7 days. |

### 2.6 API Endpoints (Current)

```
GET    /payroll/periods                  — list (filter by status, year, paginated)
POST   /payroll/periods                  — manually create period (startDate, endDate)
GET    /payroll/periods/{id}             — detail with entries
POST   /payroll/periods/{id}/close       — close period (generates entries)
POST   /payroll/periods/{id}/process     — finalize period (immutable)
PATCH  /payroll/entries/{id}             — update bonuses/deductions/notes
POST   /payroll/entries/bulk-adjust      — bulk apply adjustment
GET    /payroll/entries/{id}/detail       — commission + attendance breakdown
GET    /payroll/templates                — list templates
POST   /payroll/templates                — create template
PUT    /payroll/templates/{id}           — update template
DELETE /payroll/templates/{id}           — soft-delete template
GET    /settings/payroll-config          — get payroll settings
PUT    /settings/payroll-config          — update payroll settings
```

---

## 3. Target State (What to Build)

### 3.1 Payroll Frequency Support

**Current:** Weekly only.
**Target:** Weekly AND Semi-monthly.

#### Semi-Monthly Periods

Philippine standard: two cutoffs per month.
- **First half:** 1st–15th of the month
- **Second half:** 16th–last day of the month (28/29/30/31)

#### PayrollSettings Changes

Add to `PayrollSettings`:

```
PayrollFrequency (enum)     — Weekly (1), SemiMonthly (2)
                              Default: Weekly (backward compatible)
```

New enum:

```csharp
public enum PayrollFrequency
{
    Weekly = 1,
    SemiMonthly = 2
}
```

#### Behavior by Frequency

| Setting | Weekly | Semi-Monthly |
|---------|--------|--------------|
| `CutOffStartDay` | Used (day of week) | Ignored |
| Period boundaries | `CutOffStartDay - 7d` to `CutOffStartDay - 1d` | 1–15 or 16–{lastDay} |
| `CutOffWeek` meaning | Sequential week number | 1 (first half) or 2 (second half) |
| `Year` | Calendar year of start date | Calendar year + month |
| Hangfire trigger | Daily, checks `CutOffStartDay` | Daily, checks if today is 1st or 16th |
| Unique constraint | `(TenantId, StartDate)` | Same — still unique per start date |

#### PayrollPeriod Changes

Add to `PayrollPeriod`:

```
Month (int?)     — 1–12, populated for SemiMonthly periods (null for Weekly)
Half (int?)      — 1 or 2, populated for SemiMonthly periods (null for Weekly)
```

Display format:
- Weekly: "2026 — Week 12"
- Semi-monthly: "March 2026 — 1st Half" or "March 2026 — 2nd Half"

#### Hangfire Job Changes

`RunDailyPayrollJobAsync` logic per tenant:

```
if frequency == Weekly:
    (existing logic — check CutOffStartDay)
elif frequency == SemiMonthly:
    if today is 1st of month:
        close previous period (16th–lastDay of previous month) if Open
        create new period (1st–15th of current month)
    elif today is 16th of month:
        close previous period (1st–15th of current month) if Open
        create new period (16th–lastDay of current month)
```

---

### 3.2 Employee Type: Mixed (Base + Commission)

**Current:** Commission (1) = commissions only, Daily (2) = daily rate only.
**Target:** Add Mixed (3) = base salary + commission earnings.

#### EmployeeType Enum Change

```csharp
public enum EmployeeType
{
    Commission = 1,   // Commission splits only, no base
    Daily = 2,        // Fixed daily rate × days worked, no commission
    Mixed = 3         // Daily rate × days worked + commission splits
}
```

#### Close Logic Change

Current logic in `ClosePayrollPeriodCommandHandler`:

```
if employee.Type == Daily && dailyRate.HasValue:
    baseSalary = dailyRate × daysWorked
else:
    baseSalary = 0
```

New logic:

```
if employee.Type is Daily or Mixed && dailyRate.HasValue:
    baseSalary = dailyRate × daysWorked
else:
    baseSalary = 0

// Commissions are already aggregated for ALL employee types
// (TransactionEmployee records exist for any assigned employee)
```

Mixed employees appear in commission assignments AND attendance. Their `PayrollEntry` will have both `BaseSalary > 0` AND `TotalCommissions > 0`.

#### Employee Entity Change

No schema change needed. `DailyRate` is already nullable. For Mixed type, `DailyRate` is required (same validation as Daily). Commission assignments work regardless of type.

#### Frontend Change

Employee form: add "Mixed" option to type dropdown. Show daily rate input for Daily AND Mixed types. Commission assignment dropdowns already include all employee types.

---

### 3.3 Tips in Payroll

**Current:** `Transaction.TipAmount` exists but is never aggregated into payroll.
**Target:** Tips are distributed among service employees and included in `PayrollEntry`.

#### Tip Distribution Strategy

Per-tenant configurable via `PayrollSettings`:

```
TipDistributionMethod (enum)  — EqualSplit (1), ProportionalToCommission (2)
                                Default: EqualSplit
```

New enum:

```csharp
public enum TipDistributionMethod
{
    EqualSplit = 1,               // Tip ÷ number of employees on the transaction
    ProportionalToCommission = 2  // Tip × (employee commission / total commission)
}
```

#### PayrollEntry Change

Add field:

```
TotalTips (decimal, Precision 10,2, default 0)
```

Update computed property:

```
NetPay = BaseSalary + TotalCommissions + TotalTips + Bonuses - Deductions
```

#### Close Logic Change

After existing commission aggregation, add tip aggregation:

```sql
-- Equal split
SELECT te.EmployeeId,
       SUM(t.TipAmount / (SELECT COUNT(*) FROM TransactionEmployees te2
                           WHERE te2.TransactionId = t.Id)) AS TotalTips
FROM TransactionEmployees te
JOIN Transactions t ON te.TransactionId = t.Id
WHERE t.Status = Completed
  AND t.CompletedAt >= @periodFromUtc
  AND t.CompletedAt < @periodToUtc
  AND t.TipAmount > 0
GROUP BY te.EmployeeId

-- Proportional
SELECT te.EmployeeId,
       SUM(t.TipAmount * (te.TotalCommission /
           NULLIF((SELECT SUM(te2.TotalCommission) FROM TransactionEmployees te2
                   WHERE te2.TransactionId = t.Id), 0))) AS TotalTips
FROM TransactionEmployees te
JOIN Transactions t ON te.TransactionId = t.Id
WHERE t.Status = Completed
  AND t.CompletedAt >= @periodFromUtc
  AND t.CompletedAt < @periodToUtc
  AND t.TipAmount > 0
GROUP BY te.EmployeeId
```

For equal split: `tipPerEmployee = transaction.TipAmount / employeeCount` with `Math.Round(value, 2, MidpointRounding.AwayFromZero)`.

For proportional: if all employees have 0 commission on a tipped transaction, fall back to equal split.

#### Employee Detail Sheet Change

Add "Tips" column to commission line items table. Add tips total to summary cards.

---

### 3.4 Deduction Itemization

**Current:** Single `Deductions` field on `PayrollEntry`. Templates provide names but amounts are merged.
**Target:** Separate line items per deduction/bonus category.

#### New Entity: PayrollAdjustment

```csharp
public sealed class PayrollAdjustment : IAuditableEntity
{
    public string Id { get; set; }
    public string TenantId { get; set; }
    public string PayrollEntryId { get; set; }       // FK
    public AdjustmentType Type { get; set; }          // Bonus or Deduction
    public string Category { get; set; }              // "SSS", "PhilHealth", "PagIBIG", "Cash Advance", "Overtime", custom
    public decimal Amount { get; set; }               // Always positive
    public string? Notes { get; set; }
    public string? TemplateId { get; set; }           // FK to template that created this (nullable for manual)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public PayrollEntry Entry { get; set; }
    public PayrollAdjustmentTemplate? Template { get; set; }
}
```

Table: `PayrollAdjustments`
Indexes: `(PayrollEntryId)`, `(TenantId)`

#### PayrollEntry Changes

Keep `Bonuses` and `Deductions` as **computed aggregates** (sum of line items), not direct editable fields. Or keep them as denormalized totals updated whenever adjustments change.

**Approach:** Keep `Bonuses` and `Deductions` fields for fast reads. When an adjustment is added/removed/updated, recalculate:

```
Bonuses = SUM(adjustments WHERE type = Bonus)
Deductions = SUM(adjustments WHERE type = Deduction)
```

#### API Changes

```
GET    /payroll/entries/{id}/adjustments     — list adjustments for entry
POST   /payroll/entries/{id}/adjustments     — add single adjustment
PUT    /payroll/adjustments/{id}             — update adjustment
DELETE /payroll/adjustments/{id}             — remove adjustment
POST   /payroll/entries/bulk-adjust          — (existing) now also creates PayrollAdjustment rows
```

#### Frontend Changes

Employee detail sheet: add "Adjustments" tab showing itemized table (Category, Type badge, Amount, Notes, Template name). Inline edit/delete per row. "Add Adjustment" button.

Payroll detail page: keep existing inline edit for quick changes. Changes create/update underlying `PayrollAdjustment` rows.

#### Pre-seeded Government Deduction Categories

When a tenant is created (onboarding), auto-create these templates:

| Name | Type | Default Amount | Notes |
|------|------|---------------|-------|
| SSS | Deduction | 0 | Amount depends on salary bracket |
| PhilHealth | Deduction | 0 | Amount depends on salary bracket |
| Pag-IBIG | Deduction | 100 | Standard ₱100/month for most brackets |
| Tax (BIR Withholding) | Deduction | 0 | Manual for now |

**Future enhancement:** Auto-calculate SSS/PhilHealth based on 2024+ contribution tables. For MVP, amounts are manually set via templates.

---

### 3.5 Cash Advance System

#### New Entity: CashAdvance

```csharp
public sealed class CashAdvance : IAuditableEntity
{
    public string Id { get; set; }
    public string TenantId { get; set; }
    public string EmployeeId { get; set; }
    public decimal Amount { get; set; }                // Original advance amount
    public decimal RemainingBalance { get; set; }      // Outstanding balance
    public CashAdvanceStatus Status { get; set; }      // Pending, Approved, Active, FullyPaid, Cancelled
    public string? Reason { get; set; }
    public string? ApprovedById { get; set; }          // FK to User who approved
    public DateTime? ApprovedAt { get; set; }
    public decimal DeductionPerPeriod { get; set; }    // Amount to deduct each payroll period
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Employee Employee { get; set; }
}
```

New enum:

```csharp
public enum CashAdvanceStatus
{
    Pending = 1,      // Requested by employee/admin
    Approved = 2,     // Manager approved, ready to disburse
    Active = 3,       // Disbursed, being deducted from payroll
    FullyPaid = 4,    // Fully settled
    Cancelled = 5     // Rejected or withdrawn
}
```

Table: `CashAdvances`
Indexes: `(TenantId, EmployeeId)`, `(TenantId, Status)`

#### Auto-Deduction on Payroll Close

When `ClosePayrollPeriodCommandHandler` runs, after creating entries:

```
for each entry:
    activeAdvances = CashAdvances
        .Where(ca => ca.EmployeeId == entry.EmployeeId
                   && ca.Status == Active
                   && ca.RemainingBalance > 0)
        .OrderBy(ca => ca.CreatedAt)  // FIFO

    for each advance:
        deductAmount = Min(advance.DeductionPerPeriod, advance.RemainingBalance)
        advance.RemainingBalance -= deductAmount

        create PayrollAdjustment(
            entryId, type: Deduction,
            category: "Cash Advance",
            amount: deductAmount,
            notes: $"CA #{advance.Id} — ₱{deductAmount} of ₱{advance.Amount}")

        if advance.RemainingBalance == 0:
            advance.Status = FullyPaid
```

#### API Endpoints

```
GET    /cash-advances                    — list (filter by employee, status, paginated)
GET    /cash-advances/{id}               — detail
POST   /cash-advances                    — create (Pending or Approved based on role)
PATCH  /cash-advances/{id}/approve       — approve (Pending → Approved)
PATCH  /cash-advances/{id}/disburse      — mark as disbursed (Approved → Active)
PATCH  /cash-advances/{id}/cancel        — cancel
GET    /employees/{id}/cash-advances     — advances for specific employee
```

#### Frontend

Admin: Cash Advances page (list + create dialog). Employee detail: outstanding advances section.
Payroll detail: cash advance deductions appear as itemized adjustments.

---

### 3.6 Payslip Generation

#### Payslip Data Structure

```csharp
public sealed record PayslipDto(
    // Header
    string TenantName,
    string BranchName,
    string PeriodLabel,          // "March 2026 — 1st Half" or "2026 — Week 12"
    DateOnly PeriodStart,
    DateOnly PeriodEnd,

    // Employee
    string EmployeeName,
    string EmployeeType,         // "Commission", "Daily", "Mixed"
    string? EmployeeId,

    // Earnings
    decimal BaseSalary,
    decimal TotalCommissions,
    decimal TotalTips,
    decimal GrossEarnings,       // Base + Commission + Tips

    // Adjustments (itemized)
    IReadOnlyList<PayslipAdjustmentLine> Bonuses,
    IReadOnlyList<PayslipAdjustmentLine> Deductions,
    decimal TotalBonuses,
    decimal TotalDeductions,

    // Summary
    decimal NetPay,

    // Metadata
    int DaysWorked,
    int CommissionTransactions,  // count of transactions with commissions
    DateTime GeneratedAt
);

public sealed record PayslipAdjustmentLine(
    string Category,
    decimal Amount,
    string? Notes
);
```

#### API Endpoint

```
GET  /payroll/entries/{id}/payslip       — returns PayslipDto (JSON)
GET  /payroll/entries/{id}/payslip/pdf   — returns PDF binary (future)
GET  /payroll/periods/{id}/payslips/pdf  — batch PDF for all entries in period (future)
```

For MVP, generate the payslip as structured JSON. The frontend renders it in a print-friendly layout. PDF generation (via a library like QuestPDF or Puppeteer) is a future enhancement.

#### Frontend

Employee detail sheet: "View Payslip" button → opens print-friendly modal/page.
Payroll detail page: "Download Payslips" button (future, batch PDF).

---

### 3.7 Pay Release Date

#### PayrollPeriod Changes

Add fields:

```
ScheduledReleaseDate (DateOnly?)   — when pay is expected to be released
ReleasedAt (DateTime?)             — actual UTC timestamp when marked as released
```

#### PayrollSettings Changes

Add field:

```
PayReleaseDayOffset (int)          — days after period end to release pay
                                     Default: 3 (e.g., period ends Sunday, pay released Wednesday)
                                     Used to auto-populate ScheduledReleaseDate on period creation
```

#### Status Machine Extension

Current: `Open → Closed → Processed`
Extended: `Open → Closed → Processed → Released`

New status:

```csharp
Released = 4    // Pay has been disbursed to employees
```

`Processed` = admin approved the numbers.
`Released` = money has been paid out.

For tenants that don't need this distinction, they can skip directly from Processed to Released via a single action, or configure auto-release on process.

---

### 3.8 Payroll Notification

Wire the existing `PayrollPeriodClosedEvent` to the notification system.

#### New Handler: PayrollClosedNotificationHandler

When a period is auto-closed by Hangfire:

```
Create Notification(
    type: PayrollReady,
    category: Finance,
    title: "Payroll period ready for review",
    message: "Week 12 (Mar 19–25) has been closed with {entryCount} entries. Total net pay: ₱{totalNetPay}.",
    link: "/dashboard/payroll/{periodId}"
)
```

Add to `NotificationType` enum: `PayrollReady = 5`

---

## 4. Payroll Computation Flow

### 4.1 Period Lifecycle (Complete)

```
┌─────────────────────────────────────────────────────────┐
│  PERIOD CREATION                                        │
│  Trigger: Hangfire daily job OR manual admin action      │
│  Creates: PayrollPeriod (status = Open)                 │
│  For weekly: startDate = cutOffDay - 7, endDate = -1    │
│  For semi-monthly: 1–15 or 16–lastDay                   │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│  ACCUMULATION (Open)                                    │
│  Transactions complete → commissions recorded           │
│  Employees clock in/out → attendance recorded           │
│  Tips recorded on transactions                          │
│  Cash advances disbursed                                │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│  CLOSE (Open → Closed)                                  │
│  Trigger: Admin clicks "Close" OR Hangfire auto-close   │
│                                                         │
│  Step 1: Load all active employees                      │
│  Step 2: Aggregate commissions per employee             │
│          (TransactionEmployee.TotalCommission            │
│           WHERE Transaction.Status = Completed           │
│           AND CompletedAt in period UTC bounds)          │
│  Step 3: Aggregate tips per employee                    │
│          (Transaction.TipAmount split by method)         │
│  Step 4: Count attendance days per employee             │
│          (Attendance.Date in period Manila dates)        │
│  Step 5: Create PayrollEntry per employee               │
│          - Snapshot EmployeeType, DailyRate              │
│          - BaseSalary = rate × days (Daily/Mixed)        │
│          - TotalCommissions = aggregated                 │
│          - TotalTips = aggregated                        │
│  Step 6: Auto-deduct active cash advances               │
│          - Create PayrollAdjustment rows                 │
│          - Update advance RemainingBalance               │
│  Step 7: Set period.Status = Closed                     │
│  Step 8: Publish PayrollPeriodClosedEvent                │
│          → Notification to admin                        │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│  REVIEW & ADJUST (Closed)                               │
│  Admin reviews entries in dashboard                     │
│  - Inline edit adjustments                              │
│  - Bulk apply templates (SSS, PhilHealth, etc.)         │
│  - Add/remove individual adjustments                    │
│  - View commission + attendance + tip breakdowns        │
│  - View payslips                                        │
│  All changes create PayrollAdjustment line items         │
│  Bonuses/Deductions fields auto-recalculate             │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│  PROCESS (Closed → Processed)                           │
│  Trigger: Admin clicks "Process Payroll"                │
│  Validation: Period must be Closed                      │
│  Effect: No further adjustments allowed                 │
│  Publishes: PayrollProcessedEvent                       │
└───────────────────────┬─────────────────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│  RELEASE (Processed → Released)  [Future]               │
│  Trigger: Admin confirms pay disbursement               │
│  Records: ReleasedAt timestamp                          │
│  Effect: Fully immutable, audit complete                │
└─────────────────────────────────────────────────────────┘
```

### 4.2 Net Pay Formula

```
GrossEarnings = BaseSalary + TotalCommissions + TotalTips
TotalBonuses  = SUM(adjustments WHERE type = Bonus)
TotalDeductions = SUM(adjustments WHERE type = Deduction)
NetPay = GrossEarnings + TotalBonuses - TotalDeductions
```

---

## 5. Sample Computation

### Scenario: Semi-monthly, March 1–15, 2026

**Tenant:** SparkleWash Philippines (Makati branch)
**Frequency:** Semi-monthly
**Employees:**

| Employee | Type | Daily Rate | Services in Period |
|----------|------|-----------|-------------------|
| Juan (Washer) | Commission | — | 45 services |
| Pedro (Washer) | Mixed | ₱500/day | 30 services |
| Maria (Cashier) | Daily | ₱600/day | — |

**Transaction data for period (completed):**

- 120 total transactions
- Average service price: ₱350
- Juan assigned to 45 services, earned ₱8,750 in commissions
- Pedro assigned to 30 services, earned ₱5,200 in commissions
- Total tips in period: ₱3,600 across 80 tipped transactions
- Juan was on 35 of those tipped transactions, Pedro on 25, Maria on 20

**Attendance:**

- Juan: 13 days (missed 2)
- Pedro: 15 days (perfect)
- Maria: 14 days (missed 1)

**Computation:**

#### Juan (Commission type)
```
BaseSalary       = ₱0 (Commission type, no daily rate)
Commissions      = ₱8,750.00
Tips (equal split)= ₱3,600 × (35 tipped txns / 80 tipped txns employee-slots) ≈ ₱1,575.00
GrossEarnings    = ₱0 + ₱8,750 + ₱1,575 = ₱10,325.00

Deductions:
  SSS            = ₱450.00
  PhilHealth     = ₱250.00
  Pag-IBIG       = ₱100.00
  Cash Advance   = ₱500.00 (installment)
TotalDeductions  = ₱1,300.00

NetPay = ₱10,325.00 - ₱1,300.00 = ₱9,025.00
```

#### Pedro (Mixed type)
```
BaseSalary       = ₱500 × 15 days = ₱7,500.00
Commissions      = ₱5,200.00
Tips (equal split)= ≈ ₱1,125.00
GrossEarnings    = ₱7,500 + ₱5,200 + ₱1,125 = ₱13,825.00

Deductions:
  SSS            = ₱580.00
  PhilHealth     = ₱350.00
  Pag-IBIG       = ₱100.00
TotalDeductions  = ₱1,030.00

NetPay = ₱13,825.00 - ₱1,030.00 = ₱12,795.00
```

#### Maria (Daily type)
```
BaseSalary       = ₱600 × 14 days = ₱8,400.00
Commissions      = ₱0 (Daily type, not assigned to services)
Tips (equal split)= ≈ ₱900.00 (from tipped transactions she cashiered)
GrossEarnings    = ₱8,400 + ₱0 + ₱900 = ₱9,300.00

Deductions:
  SSS            = ₱450.00
  PhilHealth     = ₱250.00
  Pag-IBIG       = ₱100.00
TotalDeductions  = ₱800.00

NetPay = ₱9,300.00 - ₱800.00 = ₱8,500.00
```

**Period totals:** ₱30,320.00 net pay across 3 employees.

---

## 6. Schema Changes Summary

### New Entities

| Entity | Table | Purpose |
|--------|-------|---------|
| `PayrollAdjustment` | `PayrollAdjustments` | Itemized bonus/deduction per entry |
| `CashAdvance` | `CashAdvances` | Employee cash advance lifecycle |

### New Enums

| Enum | Values |
|------|--------|
| `PayrollFrequency` | Weekly (1), SemiMonthly (2) |
| `TipDistributionMethod` | EqualSplit (1), ProportionalToCommission (2) |
| `CashAdvanceStatus` | Pending (1), Approved (2), Active (3), FullyPaid (4), Cancelled (5) |

### Modified Entities

| Entity | Changes |
|--------|---------|
| `PayrollSettings` | + `PayrollFrequency`, `TipDistributionMethod`, `PayReleaseDayOffset` |
| `PayrollPeriod` | + `Month`, `Half`, `ScheduledReleaseDate`, `ReleasedAt` |
| `PayrollEntry` | + `TotalTips` |
| `EmployeeType` enum | + `Mixed = 3` |
| `PayrollStatus` enum | + `Released = 4` |
| `NotificationType` enum | + `PayrollReady = 5` |

---

## 7. API Endpoints (Complete Target)

### Payroll Periods

```
GET    /payroll/periods                      — list (status, year, month, paginated)
POST   /payroll/periods                      — manual create (startDate, endDate)
GET    /payroll/periods/{id}                 — detail with entries
POST   /payroll/periods/{id}/close           — close (generate entries)
POST   /payroll/periods/{id}/process         — finalize
POST   /payroll/periods/{id}/release         — mark as released [future]
```

### Payroll Entries

```
PATCH  /payroll/entries/{id}                 — update notes
GET    /payroll/entries/{id}/detail           — commission + attendance + tips breakdown
GET    /payroll/entries/{id}/payslip          — payslip data (JSON)
GET    /payroll/entries/{id}/adjustments      — list adjustments
POST   /payroll/entries/{id}/adjustments      — add adjustment
POST   /payroll/entries/bulk-adjust           — bulk apply (creates adjustment rows)
```

### Payroll Adjustments

```
PUT    /payroll/adjustments/{id}             — update adjustment
DELETE /payroll/adjustments/{id}             — remove adjustment
```

### Templates & Settings

```
GET    /payroll/templates                    — list
POST   /payroll/templates                    — create
PUT    /payroll/templates/{id}               — update
DELETE /payroll/templates/{id}               — soft-delete
GET    /settings/payroll-config              — get settings
PUT    /settings/payroll-config              — update settings
```

### Cash Advances

```
GET    /cash-advances                        — list (employee, status, paginated)
POST   /cash-advances                        — create
GET    /cash-advances/{id}                   — detail
PATCH  /cash-advances/{id}/approve           — approve
PATCH  /cash-advances/{id}/disburse          — mark disbursed (Active)
PATCH  /cash-advances/{id}/cancel            — cancel
GET    /employees/{id}/cash-advances         — per-employee list
```

---

## 8. Frontend Pages (Target)

### Settings → Payroll Tab

- **Period Configuration:** Frequency dropdown (Weekly/Semi-Monthly), Cut-Off Start Day (for weekly), Pay Release Day Offset, Tip Distribution Method
- **Adjustment Templates:** Existing CRUD table (SSS, PhilHealth, etc.)

### Payroll List Page

- "Create Period" button (existing)
- Filter by frequency display (week number vs month/half)
- Release date column

### Payroll Detail Page

- Existing functionality preserved
- Tips column in entry table
- Adjustments tab in employee detail sheet (itemized)
- "View Payslip" button per entry
- "Release Payroll" button (after Processed)

### Cash Advances Page (New)

- List with filters (employee, status)
- Create dialog (employee select, amount, deduction per period, reason)
- Approve/Disburse/Cancel actions
- Outstanding balance tracking

### Employee Detail → Payroll Tab (New section)

- Payroll history across periods
- Outstanding cash advances
- Commission earnings trend

---

## 9. Implementation Priority

### Phase 1: Core Enhancements (Build Next)
1. **Mixed employee type** — enum + close logic + frontend
2. **Tips in payroll** — `TotalTips` field + aggregation in close + `TipDistributionMethod` setting
3. **Semi-monthly frequency** — `PayrollFrequency` setting + Hangfire logic + period creation
4. **Payroll closed notification** — wire event to notification system

### Phase 2: Deduction Itemization
5. **PayrollAdjustment entity** — line items per entry
6. **Adjust close/edit flows** — create adjustment rows instead of flat field updates
7. **Pre-seed government templates** — SSS, PhilHealth, Pag-IBIG on tenant creation

### Phase 3: Cash Advances & Payslips
8. **CashAdvance entity** — full lifecycle
9. **Auto-deduction on close** — integrate with payroll close
10. **Payslip JSON endpoint** — structured payslip data
11. **Payslip print layout** — frontend print-friendly component

### Phase 4: Polish
12. **Pay release workflow** — Released status + dates
13. **Employee payroll history** — cross-period view
14. **CSV/PDF export** — batch operations
15. **Government deduction auto-calc** — SSS/PhilHealth contribution tables

---

## 10. Edge Cases

| Scenario | Handling |
|----------|----------|
| Employee switches from Daily to Mixed mid-period | Snapshot at close time captures current type. Previous period used old type. |
| Transaction with tip but 0 employees assigned | Tip is not distributed (stays unallocated). Log warning. |
| Cash advance deduction exceeds entry net pay | Deduct only up to `GrossEarnings + Bonuses - OtherDeductions`. Carry over remainder. |
| Semi-monthly: month has 28 days (February) | Second half = 16–28. EndDate is always last day of month. |
| Employee has 0 activity but active cash advance | Create PayrollEntry with 0 earnings but with cash advance deduction. NetPay will be negative — flag for admin review. |
| Bulk adjustment applied after individual adjustments exist | Additive. Creates new PayrollAdjustment rows. Does not overwrite existing. |
| Tenant switches frequency from Weekly to Semi-Monthly | Current open weekly periods continue. Next auto-created period uses new frequency. No retroactive changes. |
| Proportional tip split with all-zero commissions | Fall back to equal split for that transaction. |
| Multiple cash advances active simultaneously | FIFO — deduct from oldest first. Each gets its own PayrollAdjustment row. |
