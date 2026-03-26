/**
 * TypeScript enums mirroring SplashSphere.Domain.Enums.
 *
 * Numeric values match the C# enum backing integers that .NET's System.Text.Json
 * serialises by default.  Configure JsonStringEnumConverter on the API to receive
 * string names instead; the enum members are named identically so both modes work.
 */

// ── Queue ─────────────────────────────────────────────────────────────────────

/**
 * Lifecycle states of a vehicle queue entry.
 *
 * Valid transitions:
 *   Waiting → Called → InService → Completed
 *      ↓         ↓
 *   Cancelled  NoShow → (requeue to Waiting, or leave as terminal)
 */
export enum QueueStatus {
  Waiting   = 1,
  Called    = 2,
  InService = 3,
  Completed = 4,
  Cancelled = 5,
  NoShow    = 6,
}

/**
 * Priority level assigned at queue check-in.
 * Ordering: Vip (3) > Express (2) > Regular (1).
 * Within the same tier, entries are served FIFO by createdAt.
 */
export enum QueuePriority {
  Regular = 1,
  Express = 2,
  Vip     = 3,
}

// ── Transaction ───────────────────────────────────────────────────────────────

/**
 * Lifecycle states of a POS transaction.
 *
 * Valid transitions:
 *   Pending → InProgress → Completed
 *      ↓           ↓
 *   Cancelled   Cancelled      Refunded ← (from Completed only)
 */
export enum TransactionStatus {
  Pending    = 1,
  InProgress = 2,
  Completed  = 3,
  Cancelled  = 4,
  Refunded   = 5,
}

/**
 * Accepted payment methods at the POS.
 * A transaction can have multiple Payment records that together sum to FinalAmount.
 */
export enum PaymentMethod {
  Cash         = 1,
  /** GCash mobile wallet. Also covers Maya/PayMaya QR payments. */
  GCash        = 2,
  CreditCard   = 3,
  DebitCard    = 4,
  BankTransfer = 5,
}

// ── Employee & Payroll ────────────────────────────────────────────────────────

/** How an employee is compensated. */
export enum EmployeeType {
  /** Earns through service commissions split equally among assigned staff. */
  Commission = 1,
  /** Fixed daily rate (cashiers, security, maintenance). */
  Daily      = 2,
}

/** How a service commission amount is calculated per transaction line. */
export enum CommissionType {
  /** price × rate / 100 */
  Percentage  = 1,
  /** Fixed peso amount regardless of price. */
  FixedAmount = 2,
  /** fixedAmount + (price × rate / 100) */
  Hybrid      = 3,
}

/** Lifecycle states of a weekly payroll period. Open → Closed → Processed. */
export enum PayrollStatus {
  Open      = 1,
  Closed    = 2,
  Processed = 3,
}

// ── Pricing modifiers ─────────────────────────────────────────────────────────

/**
 * Determines what condition activates a pricing modifier and how its `value`
 * field is interpreted (multiplier vs. absolute peso deduction).
 */
export enum ModifierType {
  /** Active during configured hours. `value` is a multiplier (e.g. 1.20 = +20%). */
  PeakHour  = 1,
  /** Active on a specific day of week. `value` is a multiplier. */
  DayOfWeek = 2,
  /** Active on a holiday. `value` is a multiplier. */
  Holiday   = 3,
  /** Promotional discount. `value` is an absolute peso deduction. */
  Promotion = 4,
  /** Weather-triggered surcharge, activated manually. `value` is a multiplier. */
  Weather   = 5,
}

// ── Receipt ───────────────────────────────────────────────────────────────────

export enum ReceiptLineType {
  Service     = 0,
  Package     = 1,
  Merchandise = 2,
}

// ── Cashier Shifts ────────────────────────────────────────────────────────────

export enum ShiftStatus {
  Open   = 1,
  Closed = 2,
  Voided = 3,
}

export enum ReviewStatus {
  Pending  = 1,
  Approved = 2,
  Flagged  = 3,
}

export enum CashMovementType {
  CashIn  = 1,
  CashOut = 2,
}

// ── Notifications ───────────────────────────────────────────────────────────

export enum NotificationType {
  TransactionCompleted = 1,
  LowStockAlert        = 2,
  ShiftFlagged         = 3,
  QueueNoShow          = 4,
}

export enum NotificationCategory {
  Operations = 1,
  Inventory  = 2,
  Finance    = 3,
  Queue      = 4,
}

// ── Payroll ───────────────────────────────────────────────────────────────────

export enum AdjustmentType {
  Bonus     = 1,
  Deduction = 2,
}
