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
 * Ordering: Vip (4) > Booked (3) > Express (2) > Regular (1).
 * Within the same tier, entries are served FIFO by createdAt.
 */
export enum QueuePriority {
  Regular = 1,
  Express = 2,
  /** Customer pre-booked via the Connect app. Auto-assigned by the pre-slot Hangfire job. */
  Booked  = 3,
  Vip     = 4,
}

// ── Booking ───────────────────────────────────────────────────────────────────

/**
 * Lifecycle state of an online booking created via the Customer Connect app.
 *
 * Transitions:
 *   Confirmed → Arrived → InService → Completed
 *   Confirmed → Cancelled (customer/tenant)
 *   Confirmed → NoShow (grace period elapsed)
 */
export enum BookingStatus {
  Confirmed = 1,
  Arrived   = 2,
  InService = 3,
  Completed = 4,
  Cancelled = 5,
  NoShow    = 6,
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
  /** Earns both a fixed daily rate AND service commissions. */
  Hybrid     = 3,
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

/** Lifecycle states of a payroll period. Open → Closed → Processed → Released. */
export enum PayrollStatus {
  Open      = 1,
  Closed    = 2,
  Processed = 3,
  Released  = 4,
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
  PayrollClosed        = 5,
}

export enum NotificationCategory {
  Operations = 1,
  Inventory  = 2,
  Finance    = 3,
  Queue      = 4,
}

// ── Payroll ───────────────────────────────────────────────────────────────────

export enum PayrollFrequency {
  Weekly      = 1,
  SemiMonthly = 2,
}

export enum AdjustmentType {
  Bonus     = 1,
  Deduction = 2,
}

export enum CashAdvanceStatus {
  Pending   = 1,
  Approved  = 2,
  Active    = 3,
  FullyPaid = 4,
  Cancelled = 5,
}

export enum ExpenseFrequency {
  OneTime = 0,
  Daily   = 1,
  Weekly  = 2,
  Monthly = 3,
}

// ── Loyalty ──────────────────────────────────────────────────────────────────

/** Membership tier levels. Progression is one-directional (upgrades only). */
export enum LoyaltyTier {
  Standard = 0,
  Silver   = 1,
  Gold     = 2,
  Platinum = 3,
}

/** Type of point transaction in the loyalty ledger. */
export enum PointTransactionType {
  Earned     = 0,
  Redeemed   = 1,
  Expired    = 2,
  Adjustment = 3,
}

/** What a loyalty reward grants when redeemed. */
export enum RewardType {
  FreeService     = 0,
  FreePackage     = 1,
  DiscountAmount  = 2,
  DiscountPercent = 3,
}

/** Lifecycle state of a referral — mirrors C# `SplashSphere.Domain.Enums.ReferralStatus`. */
export enum ReferralStatus {
  Pending   = 1,
  Completed = 2,
  Expired   = 3,
}

// ── Franchise ───────────────────────────────────────────────────────────────

/** Classification of a tenant within the platform. */
export enum TenantType {
  Independent    = 0,
  CorporateChain = 1,
  Franchisor     = 2,
  Franchisee     = 3,
}

/** How royalty amounts are calculated from franchisee revenue. */
export enum RoyaltyBasis {
  GrossRevenue       = 0,
  NetRevenue         = 1,
  ServiceRevenueOnly = 2,
}

/** How often royalties are calculated. */
export enum RoyaltyFrequency {
  Weekly  = 0,
  Monthly = 1,
}

/** Lifecycle states of a franchise agreement. */
export enum AgreementStatus {
  Draft      = 0,
  Active     = 1,
  Expired    = 2,
  Terminated = 3,
  Suspended  = 4,
}

/** Lifecycle states of a royalty period payment. */
export enum RoyaltyStatus {
  Pending  = 0,
  Invoiced = 1,
  Paid     = 2,
  Overdue  = 3,
}

// ── Inventory ────────────────────────────────────────────────────────────────

export enum MovementType {
  PurchaseIn    = 1,
  UsageOut      = 2,
  SaleOut       = 3,
  AdjustmentIn  = 4,
  AdjustmentOut = 5,
  TransferIn    = 6,
  TransferOut   = 7,
  ReturnIn      = 8,
  WasteOut      = 9,
}

export enum PurchaseOrderStatus {
  Draft              = 0,
  Sent               = 1,
  PartiallyReceived  = 2,
  Received           = 3,
  Cancelled          = 4,
}

export enum EquipmentStatus {
  Operational      = 0,
  NeedsMaintenance = 1,
  UnderRepair      = 2,
  Retired          = 3,
}

export enum MaintenanceType {
  Preventive      = 0,
  Corrective      = 1,
  Inspection      = 2,
  PartReplacement = 3,
}

// ── Receipt designer ──────────────────────────────────────────────────────────

export enum LogoSize {
  Small  = 0,
  Medium = 1,
  Large  = 2,
}

export enum LogoPosition {
  Left   = 0,
  Center = 1,
}

export enum ReceiptWidth {
  Mm58 = 0,
  Mm80 = 1,
}

export enum ReceiptFontSize {
  Small  = 0,
  Normal = 1,
  Large  = 2,
}
