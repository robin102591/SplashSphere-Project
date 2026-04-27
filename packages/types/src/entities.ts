/**
 * TypeScript interfaces mirroring the C# DTO records returned by the SplashSphere API.
 *
 * Conventions:
 *  - `string`  for IDs, text, enums serialised as strings.
 *  - `number`  for decimals (PHP amounts), integers, and enums serialised as numbers.
 *  - `string`  for DateTime / DateOnly / TimeOnly — all arrive as ISO-8601 strings.
 *  - `T | null` for nullable fields (C# `T?`).
 *  - `readonly T[]` for IReadOnlyList<T>.
 */

import type {
  AdjustmentType,
  BookingStatus,
  CashAdvanceStatus,
  CashMovementType,
  CommissionType,
  EmployeeType,
  EquipmentStatus,
  ExpenseFrequency,
  LoyaltyTier,
  MaintenanceType,
  ModifierType,
  MovementType,
  PaymentMethod,
  PayrollStatus,
  PointTransactionType,
  PurchaseOrderStatus,
  QueuePriority,
  QueueStatus,
  ReceiptLineType,
  ReferralStatus,
  ReviewStatus,
  RewardType,
  ShiftStatus,
  TransactionStatus,
  TenantType,
} from './enums';

// ── Auth / User ───────────────────────────────────────────────────────────────

export interface CurrentUserTenant {
  id: string;
  name: string;
  email: string;
  contactNumber: string;
  address: string;
  isActive: boolean;
  tenantType: number;
  parentTenantId: string | null;
  franchiseCode: string | null;
}

export interface CurrentUser {
  id: string;
  clerkUserId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: string | null;
  isActive: boolean;
  hasPin: boolean;
  tenant: CurrentUserTenant | null;
}

// ── Branch ────────────────────────────────────────────────────────────────────

export interface Branch {
  id: string;
  name: string;
  code: string;
  address: string;
  contactNumber: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// ── Vehicle master data ───────────────────────────────────────────────────────

export interface VehicleType {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Size {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Make {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface VehicleModel {
  id: string;
  makeId: string;
  makeName: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// ── Service catalogue ─────────────────────────────────────────────────────────

export interface ServiceCategory {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ServiceSummary {
  id: string;
  name: string;
  description: string | null;
  basePrice: number;
  categoryId: string;
  categoryName: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ServicePricingRow {
  id: string;
  vehicleTypeId: string;
  vehicleTypeName: string;
  sizeId: string;
  sizeName: string;
  price: number;
}

export interface ServiceCommissionRow {
  id: string;
  vehicleTypeId: string;
  vehicleTypeName: string;
  sizeId: string;
  sizeName: string;
  type: CommissionType;
  fixedAmount: number | null;
  percentageRate: number | null;
}

export interface ServiceDetail extends ServiceSummary {
  pricing: readonly ServicePricingRow[];
  commissions: readonly ServiceCommissionRow[];
}

// ── Package catalogue ─────────────────────────────────────────────────────────

export interface PackageSummary {
  id: string;
  name: string;
  description: string | null;
  serviceCount: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface PackageService {
  serviceId: string;
  serviceName: string;
  categoryName: string;
}

export interface PackagePricingRow {
  id: string;
  vehicleTypeId: string;
  vehicleTypeName: string;
  sizeId: string;
  sizeName: string;
  price: number;
}

export interface PackageCommissionRow {
  id: string;
  vehicleTypeId: string;
  vehicleTypeName: string;
  sizeId: string;
  sizeName: string;
  percentageRate: number;
}

export interface PackageDetail extends PackageSummary {
  services: readonly PackageService[];
  pricing: readonly PackagePricingRow[];
  commissions: readonly PackageCommissionRow[];
}

// ── Pricing modifiers ─────────────────────────────────────────────────────────

export interface PricingModifier {
  id: string;
  name: string;
  type: ModifierType;
  typeLabel: string;
  value: number;
  branchId: string | null;
  branchName: string | null;
  /** HH:MM:SS — PeakHour only */
  startTime: string | null;
  /** HH:MM:SS — PeakHour only */
  endTime: string | null;
  /** 0 (Sunday) – 6 (Saturday) — DayOfWeek only */
  activeDayOfWeek: number | null;
  /** YYYY-MM-DD — Holiday only */
  holidayDate: string | null;
  /** Holiday only */
  holidayName: string | null;
  /** YYYY-MM-DD — Promotion only */
  startDate: string | null;
  /** YYYY-MM-DD — Promotion only */
  endDate: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// ── Customers & vehicles ──────────────────────────────────────────────────────

export interface Customer {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string | null;
  contactNumber: string | null;
  notes: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CustomerCar {
  id: string;
  plateNumber: string;
  vehicleTypeName: string;
  sizeName: string;
  makeName: string | null;
  modelName: string | null;
  color: string | null;
  year: number | null;
}

export interface CustomerDetail extends Customer {
  cars: readonly CustomerCar[];
}

export interface Car {
  id: string;
  plateNumber: string;
  vehicleTypeId: string;
  vehicleTypeName: string;
  sizeId: string;
  sizeName: string;
  makeId: string | null;
  makeName: string | null;
  modelId: string | null;
  modelName: string | null;
  customerId: string | null;
  customerFullName: string | null;
  color: string | null;
  year: number | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
}

// ── Merchandise ───────────────────────────────────────────────────────────────

export interface MerchandiseCategory {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Merchandise {
  id: string;
  name: string;
  sku: string;
  description: string | null;
  categoryId: string | null;
  categoryName: string | null;
  price: number;
  costPrice: number | null;
  stockQuantity: number;
  lowStockThreshold: number;
  isLowStock: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// ── Employees ─────────────────────────────────────────────────────────────────

export interface Employee {
  id: string;
  branchId: string;
  branchName: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string | null;
  contactNumber: string | null;
  employeeType: EmployeeType;
  dailyRate: number | null;
  /** YYYY-MM-DD */
  hiredDate: string | null;
  isActive: boolean;
  userId: string | null;
  /** ISO-8601 DateTime — null if never invited */
  invitedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface EmployeeCommissionDto {
  transactionId: string;
  transactionNumber: string;
  /** YYYY-MM-DD */
  transactionDate: string;
  branchName: string;
  totalCommission: number;
}

export interface AttendanceDto {
  id: string;
  employeeId: string;
  employeeFullName: string;
  branchName: string;
  /** YYYY-MM-DD */
  date: string;
  timeIn: string;
  timeOut: string | null;
  notes: string | null;
}

// ── Queue ─────────────────────────────────────────────────────────────────────

export interface QueueEntry {
  id: string;
  branchId: string;
  branchName: string;
  queueNumber: string;
  plateNumber: string;
  status: QueueStatus;
  priority: QueuePriority;
  customerId: string | null;
  customerFullName: string | null;
  carId: string | null;
  transactionId: string | null;
  estimatedWaitMinutes: number | null;
  /** JSON array of service IDs, e.g. '["id1","id2"]' */
  preferredServices: string | null;
  notes: string | null;
  calledAt: string | null;
  startedAt: string | null;
  completedAt: string | null;
  cancelledAt: string | null;
  noShowAt: string | null;
  createdAt: string;
  // ── Booking context (null when queue entry is a walk-in) ─────────────────
  bookingId: string | null;
  /** ISO-8601 UTC slot start time. Display in Asia/Manila. */
  bookingSlotStart: string | null;
  /** True once the cashier has locked the vehicle type + size for this booking's first visit. */
  isVehicleClassified: boolean | null;
  bookingStatus: BookingStatus | null;
}

/** Slimmed-down projection for the public wall-display. Plate is masked. */
export interface QueueDisplayEntry {
  queueNumber: string;
  /** e.g. "AB***3" */
  maskedPlate: string;
  status: QueueStatus;
  priority: QueuePriority;
  estimatedWaitMinutes: number | null;
}

export interface QueueStats {
  waitingCount: number;
  calledCount: number;
  inServiceCount: number;
  servedToday: number;
  avgWaitMinutes: number | null;
}

// ── Transactions ──────────────────────────────────────────────────────────────

export interface Payment {
  id: string;
  method: PaymentMethod;
  amount: number;
  reference: string | null;
  createdAt: string;
}

export interface ServiceAssignment {
  id: string;
  employeeId: string;
  employeeName: string;
  commissionAmount: number;
}

export interface PackageAssignment {
  id: string;
  employeeId: string;
  employeeName: string;
  commissionAmount: number;
}

export interface TransactionServiceLine {
  id: string;
  serviceId: string;
  serviceName: string;
  categoryName: string;
  vehicleTypeName: string;
  sizeName: string;
  unitPrice: number;
  totalCommission: number;
  notes: string | null;
  employeeAssignments: readonly ServiceAssignment[];
}

export interface TransactionPackageLine {
  id: string;
  packageId: string;
  packageName: string;
  vehicleTypeName: string;
  sizeName: string;
  unitPrice: number;
  totalCommission: number;
  notes: string | null;
  employeeAssignments: readonly PackageAssignment[];
}

export interface TransactionMerchandiseLine {
  id: string;
  merchandiseId: string;
  merchandiseName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface TransactionEmployeeSummary {
  id: string;
  employeeId: string;
  employeeName: string;
  totalCommission: number;
}

export interface TransactionSummary {
  id: string;
  transactionNumber: string;
  branchId: string;
  branchName: string;
  carId: string;
  plateNumber: string;
  vehicleTypeName: string;
  sizeName: string;
  customerId: string | null;
  customerName: string | null;
  status: TransactionStatus;
  totalAmount: number;
  discountAmount: number;
  taxAmount: number;
  finalAmount: number;
  tipAmount: number;
  cashierName: string;
  queueEntryId: string | null;
  pointsEarned: number;
  createdAt: string;
}

export interface TransactionDetail extends TransactionSummary {
  branchAddress: string;
  branchContactNumber: string;
  cashierId: string;
  vehicleTypeId: string;
  sizeId: string;
  notes: string | null;
  completedAt: string | null;
  cancelledAt: string | null;
  refundedAt: string | null;
  refundReason: string | null;
  services: readonly TransactionServiceLine[];
  packages: readonly TransactionPackageLine[];
  merchandise: readonly TransactionMerchandiseLine[];
  employees: readonly TransactionEmployeeSummary[];
  payments: readonly Payment[];
}

// ── Receipt ───────────────────────────────────────────────────────────────────

export interface ReceiptBranch {
  id: string;
  name: string;
  address: string;
  contactNumber: string;
}

export interface ReceiptVehicle {
  plateNumber: string;
  vehicleTypeName: string;
  sizeName: string;
  makeName: string | null;
  modelName: string | null;
  color: string | null;
  year: number | null;
}

export interface ReceiptCustomer {
  id: string;
  name: string;
  contactNumber: string | null;
}

export interface ReceiptLineItem {
  type: ReceiptLineType;
  name: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  assignedEmployees: readonly string[];
}

export interface ReceiptPayment {
  method: PaymentMethod;
  amount: number;
  reference: string | null;
  paidAt: string;
}

export interface Receipt {
  transactionId: string;
  transactionNumber: string;
  issuedAt: string;
  branch: ReceiptBranch;
  vehicle: ReceiptVehicle;
  customer: ReceiptCustomer | null;
  cashierName: string;
  lineItems: readonly ReceiptLineItem[];
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  payments: readonly ReceiptPayment[];
  notes: string | null;
}

// ── Daily summary ─────────────────────────────────────────────────────────────

export interface PaymentBreakdown {
  method: PaymentMethod;
  count: number;
  totalAmount: number;
}

export interface TopService {
  serviceId: string;
  serviceName: string;
  count: number;
  totalRevenue: number;
}

export interface DailySummary {
  /** YYYY-MM-DD */
  date: string;
  branchId: string;
  branchName: string;
  totalTransactions: number;
  completedTransactions: number;
  cancelledTransactions: number;
  pendingTransactions: number;
  totalRevenue: number;
  totalDiscounts: number;
  totalTax: number;
  paymentBreakdown: readonly PaymentBreakdown[];
  topServices: readonly TopService[];
}

// ── Payroll ───────────────────────────────────────────────────────────────────

export interface PayrollEntry {
  id: string;
  employeeId: string;
  employeeName: string;
  branchName: string;
  employeeTypeSnapshot: EmployeeType;
  daysWorked: number;
  dailyRateSnapshot: number | null;
  baseSalary: number;
  totalCommissions: number;
  /** Informational only — tips are paid out immediately. NOT included in netPay. */
  totalTips: number;
  bonuses: number;
  deductions: number;
  netPay: number;
  notes: string | null;
}

export interface PayrollPeriodSummary {
  id: string;
  status: PayrollStatus;
  year: number;
  cutOffWeek: number;
  /** YYYY-MM-DD */
  startDate: string;
  /** YYYY-MM-DD */
  endDate: string;
  branchId: string | null;
  branchName: string | null;
  entryCount: number;
  totalNetPay: number;
  scheduledReleaseDate: string | null;
  releasedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PayrollPeriodDetail extends Omit<PayrollPeriodSummary, 'entryCount' | 'totalNetPay'> {
  entries: readonly PayrollEntry[];
}

export interface PayrollAdjustmentTemplate {
  id: string;
  name: string;
  type: AdjustmentType;
  defaultAmount: number;
  isActive: boolean;
  sortOrder: number;
  isSystemDefault: boolean;
}

export interface PayrollAdjustment {
  id: string;
  type: AdjustmentType;
  category: string;
  amount: number;
  notes: string | null;
  templateId: string | null;
  templateName: string | null;
  createdAt: string;
}

export interface PayrollEntryDetail {
  entry: PayrollEntry;
  commissionLineItems: readonly CommissionLineItem[];
  attendanceRecords: readonly AttendanceLineItem[];
  adjustments: readonly PayrollAdjustment[];
}

export interface CommissionLineItem {
  transactionNumber: string;
  serviceName: string;
  commissionAmount: number;
  completedAt: string;
}

export interface AttendanceLineItem {
  date: string;
  timeIn: string;
  timeOut: string | null;
}

// ── Cash Advances ─────────────────────────────────────────────────────────────

export interface CashAdvance {
  id: string;
  employeeId: string;
  employeeName: string;
  amount: number;
  remainingBalance: number;
  status: CashAdvanceStatus;
  reason: string | null;
  approvedByName: string | null;
  approvedAt: string | null;
  deductionPerPeriod: number;
  createdAt: string;
  updatedAt: string;
}

// ── Payslip ───────────────────────────────────────────────────────────────────

export interface Payslip {
  tenantName: string;
  branchName: string;
  periodLabel: string;
  periodStart: string;
  periodEnd: string;
  employeeName: string;
  employeeType: string;
  employeeId: string | null;
  baseSalary: number;
  totalCommissions: number;
  totalTips: number;
  grossEarnings: number;
  bonuses: readonly PayslipAdjustmentLine[];
  deductions: readonly PayslipAdjustmentLine[];
  totalBonuses: number;
  totalDeductions: number;
  netPay: number;
  daysWorked: number;
  commissionTransactions: number;
  generatedAt: string;
}

export interface PayslipAdjustmentLine {
  category: string;
  amount: number;
  notes: string | null;
}

// ── Dashboard ─────────────────────────────────────────────────────────────────

export interface BranchKpi {
  branchId: string;
  branchName: string;
  revenueToday: number;
  transactionsToday: number;
  queueWaiting: number;
  queueInService: number;
}

export interface DashboardSummary {
  revenueToday: number;
  revenueThisWeek: number;
  revenueThisMonth: number;
  transactionsToday: number;
  transactionsThisWeek: number;
  transactionsThisMonth: number;
  queueWaiting: number;
  queueInService: number;
  activeEmployees: number;
  clockedInToday: number;
  /** Null when the query was scoped to a single branch. */
  branches: readonly BranchKpi[] | null;
  /** Week-over-week revenue % change. Null if no prior data. */
  revenueWeekChange: number | null;
  /** Month-over-month revenue % change. Null if no prior data. */
  revenueMonthChange: number | null;
  /** Week-over-week transaction count % change. */
  transactionsWeekChange: number | null;
  /** Month-over-month transaction count % change. */
  transactionsMonthChange: number | null;
}

// ── Reports ───────────────────────────────────────────────────────────────────

export interface RevenueDayBreakdown {
  /** YYYY-MM-DD */
  date: string;
  revenue: number;
  discount: number;
  tax: number;
  transactionCount: number;
}

export interface RevenueByPaymentMethod {
  paymentMethod: string;
  amount: number;
  paymentCount: number;
}

export interface RevenueReport {
  /** YYYY-MM-DD */
  from: string;
  /** YYYY-MM-DD */
  to: string;
  branchId: string | null;
  branchName: string | null;
  grandTotal: number;
  totalDiscount: number;
  totalTax: number;
  transactionCount: number;
  dailyBreakdown: readonly RevenueDayBreakdown[];
  byPaymentMethod: readonly RevenueByPaymentMethod[];
}

export interface EmployeeCommission {
  employeeId: string;
  employeeName: string;
  branchName: string;
  employeeType: string;
  totalCommissions: number;
  transactionCount: number;
}

export interface CommissionsReport {
  /** YYYY-MM-DD */
  from: string;
  /** YYYY-MM-DD */
  to: string;
  branchId: string | null;
  employeeId: string | null;
  grandTotalCommissions: number;
  transactionCount: number;
  employees: readonly EmployeeCommission[];
}

export interface ServicePopularityItem {
  serviceId: string;
  serviceName: string;
  categoryName: string | null;
  timesPerformed: number;
  totalRevenue: number;
  averageRevenue: number;
}

export interface PackagePopularityItem {
  packageId: string;
  packageName: string;
  timesPerformed: number;
  totalRevenue: number;
  averageRevenue: number;
}

export interface ServicePopularityReport {
  /** YYYY-MM-DD */
  from: string;
  /** YYYY-MM-DD */
  to: string;
  branchId: string | null;
  branchName: string | null;
  services: readonly ServicePopularityItem[];
  packages: readonly PackagePopularityItem[];
}

// ── Expenses ─────────────────────────────────────────────────────────────────

export interface ExpenseDto {
  id: string;
  branchName: string;
  categoryName: string;
  categoryIcon: string | null;
  amount: number;
  description: string;
  vendor: string | null;
  receiptReference: string | null;
  expenseDate: string;
  frequency: ExpenseFrequency;
  isRecurring: boolean;
  recordedByName: string;
  createdAt: string;
}

export interface ExpenseCategoryDto {
  id: string;
  name: string;
  icon: string | null;
  isActive: boolean;
}

export interface ProfitLossReport {
  from: string;
  to: string;
  branchId: string | null;
  branchName: string | null;
  revenue: number;
  cogs: number;
  grossProfit: number;
  totalExpenses: number;
  netProfit: number;
  marginPercent: number;
  expensesByCategory: readonly ExpenseByCategoryDto[];
  dailyBreakdown: readonly ProfitLossDayDto[];
}

export interface ExpenseByCategoryDto {
  categoryName: string;
  amount: number;
}

export interface ProfitLossDayDto {
  date: string;
  revenue: number;
  expenses: number;
  netProfit: number;
}

// ── Audit Logs ───────────────────────────────────────────────────────────────

export interface AuditLogDto {
  id: string;
  userId: string | null;
  action: string;
  entityType: string;
  entityId: string;
  changes: string | null;
  timestamp: string;
}

// ── Subscription & Billing ────────────────────────────────────────────────────

export interface TenantPlan {
  tier: 'trial' | 'starter' | 'growth' | 'enterprise';
  status: 'trial' | 'active' | 'past_due' | 'suspended' | 'cancelled';
  planName: string;
  monthlyPrice: number;
  features: string[];
  limits: PlanLimits;
  trial: TrialInfo | null;
  billing: BillingInfo | null;
}

export interface PlanLimits {
  maxBranches: number;
  currentBranches: number;
  maxEmployees: number;
  currentEmployees: number;
  smsPerMonth: number;
  smsUsedThisMonth: number;
}

export interface TrialInfo {
  startDate: string;
  endDate: string;
  daysRemaining: number;
  expired: boolean;
}

export interface BillingInfo {
  nextBillingDate: string | null;
  lastPaymentDate: string | null;
  currentPeriodStart: string | null;
  currentPeriodEnd: string | null;
}

export interface BillingRecord {
  id: string;
  amount: number;
  currency: string;
  type: number;
  status: number;
  paymentMethod: string | null;
  invoiceNumber: string | null;
  billingDate: string;
  paidDate: string | null;
  notes: string | null;
}

export interface CheckoutResult {
  checkoutUrl: string;
  sessionId: string;
}

// ── Feature Keys (must match backend FeatureKeys.cs) ─────────────────────────

export const FeatureKeys = {
  // Core (all plans)
  Pos: 'pos',
  CommissionTracking: 'commission_tracking',
  WeeklyPayroll: 'weekly_payroll',
  BasicReports: 'basic_reports',
  CustomerManagement: 'customer_management',
  VehicleManagement: 'vehicle_management',
  EmployeeManagement: 'employee_management',
  MerchandiseManagement: 'merchandise_management',
  // Growth
  QueueManagement: 'queue_management',
  CustomerLoyalty: 'customer_loyalty',
  CashAdvanceTracking: 'cash_advance_tracking',
  ExpenseTracking: 'expense_tracking',
  ShiftManagement: 'shift_management',
  ProfitLossReports: 'profit_loss_reports',
  SmsNotifications: 'sms_notifications',
  PricingModifiers: 'pricing_modifiers',
  // Enterprise
  ApiAccess: 'api_access',
  CustomIntegrations: 'custom_integrations',
  FranchiseManagement: 'franchise_management',
  // Inventory (Growth+)
  SupplyTracking: 'supply_tracking',
  PurchaseOrders: 'purchase_orders',
  EquipmentManagement: 'equipment_management',
  SupplyUsageAutoDeduction: 'supply_usage_auto_deduction',
  CostPerWashReports: 'cost_per_wash_reports',
  // Customer Connect / Online Booking (Growth+)
  OnlineBooking: 'online_booking',
} as const;

// ── Attendance Reports ────────────────────────────────────────────────────────

export interface AttendanceReportSummary {
  totalEmployees: number;
  averageAttendanceRate: number;
  totalLateArrivals: number;
  averageHoursPerDay: number;
}

export interface EmployeeAttendanceRow {
  employeeId: string;
  employeeName: string;
  branchName: string;
  employeeType: string;
  daysPresent: number;
  daysAbsent: number;
  lateCount: number;
  earlyOutCount: number;
  totalHours: number;
  averageHoursPerDay: number;
}

export interface AttendanceReport {
  /** YYYY-MM-DD */
  from: string;
  /** YYYY-MM-DD */
  to: string;
  branchId: string | null;
  employeeId: string | null;
  summary: AttendanceReportSummary;
  employees: readonly EmployeeAttendanceRow[];
}

// ── Cashier Shifts ────────────────────────────────────────────────────────────

export interface CashMovementDto {
  id: string;
  type: CashMovementType;
  amount: number;
  reason: string;
  reference: string | null;
  movementTime: string;
}

export interface ShiftDenominationDto {
  denominationValue: number;
  count: number;
  subtotal: number;
}

export interface ShiftPaymentSummaryDto {
  method: PaymentMethod;
  transactionCount: number;
  totalAmount: number;
}

export interface ShiftDetailDto {
  id: string;
  branchId: string;
  branchName: string;
  cashierId: string;
  cashierName: string;
  shiftDate: string;
  openedAt: string;
  closedAt: string | null;
  status: ShiftStatus;
  openingCashFund: number;
  totalCashPayments: number;
  totalNonCashPayments: number;
  totalCashIn: number;
  totalCashOut: number;
  expectedCashInDrawer: number;
  actualCashInDrawer: number;
  variance: number;
  totalTransactionCount: number;
  totalRevenue: number;
  totalCommissions: number;
  totalDiscounts: number;
  reviewStatus: ReviewStatus;
  reviewedById: string | null;
  reviewedByName: string | null;
  reviewedAt: string | null;
  reviewNotes: string | null;
  cashMovements: readonly CashMovementDto[];
  denominations: readonly ShiftDenominationDto[];
  paymentSummaries: readonly ShiftPaymentSummaryDto[];
}

export interface ShiftSummaryDto {
  id: string;
  branchId: string;
  branchName: string;
  cashierId: string;
  cashierName: string;
  shiftDate: string;
  openedAt: string;
  closedAt: string | null;
  status: ShiftStatus;
  openingCashFund: number;
  totalRevenue: number;
  variance: number;
  reviewStatus: ReviewStatus;
  reviewedByName: string | null;
  reviewedAt: string | null;
}

export interface TopServiceDto {
  serviceName: string;
  transactionCount: number;
  totalAmount: number;
}

export interface TopEmployeeDto {
  employeeId: string;
  employeeName: string;
  serviceCount: number;
  totalCommission: number;
}

export interface ShiftReportDto {
  shift: ShiftDetailDto;
  topServices: readonly TopServiceDto[];
  topEmployees: readonly TopEmployeeDto[];
  generatedAt: string;
}

export interface PayrollSettingsDto {
  cutOffStartDay: number; // DayOfWeek: 0=Sunday, 1=Monday, ..., 6=Saturday
  frequency: number;      // PayrollFrequency: 1=Weekly, 2=SemiMonthly
  payReleaseDayOffset: number;
  autoCalcGovernmentDeductions: boolean;
  branchId: string | null;
  branchName: string | null;
  isInherited: boolean;
}

export interface ShiftSettingsDto {
  defaultOpeningFund: number;
  autoApproveThreshold: number;
  flagThreshold: number;
  requireShiftForTransactions: boolean;
  endOfDayReminderTime: string;
  lockTimeoutMinutes: number;
  maxPinAttempts: number;
}

export interface ShiftVarianceCashierDto {
  cashierId: string;
  cashierName: string;
  shiftCount: number;
  totalVariance: number;
  averageVariance: number;
  largestShortage: number;
}

export interface VarianceTrendPointDto {
  /** YYYY-MM-DD */
  shiftDate: string;
  variance: number;
  reviewStatus: ReviewStatus;
}

// ── Notifications ───────────────────────────────────────────────────────────

export interface NotificationDto {
  id: string;
  type: number;
  category: number;
  severity: number;
  title: string;
  message: string;
  referenceId: string | null;
  referenceType: string | null;
  actionUrl: string | null;
  actionLabel: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface UnreadCountDto {
  count: number;
}

export interface NotificationPreferenceDto {
  notificationType: number;
  typeName: string;
  category: number;
  smsAvailable: boolean;
  smsMandatory: boolean;
  emailAvailable: boolean;
  emailMandatory: boolean;
  smsEnabled: boolean;
  emailEnabled: boolean;
}

export interface UpdateNotificationPreferencesRequest {
  preferences: { notificationType: number; smsEnabled: boolean; emailEnabled: boolean }[];
}

export interface LowStockItem {
  id: string;
  name: string;
  sku: string;
  stockQuantity: number;
  lowStockThreshold: number;
}

export interface EmployeePayrollHistory {
  entryId: string;
  periodId: string;
  periodStart: string;
  periodEnd: string;
  periodStatus: number;
  daysWorked: number;
  baseSalary: number;
  totalCommissions: number;
  totalTips: number;
  bonuses: number;
  deductions: number;
  netPay: number;
}

// ── Loyalty ──────────────────────────────────────────────────────────────────

export interface LoyaltyTierConfigDto {
  id: string;
  tier: LoyaltyTier;
  name: string;
  minimumLifetimePoints: number;
  pointsMultiplier: number;
}

export interface LoyaltyProgramSettingsDto {
  id: string;
  pointsPerCurrencyUnit: number;
  currencyUnitAmount: number;
  isActive: boolean;
  pointsExpirationMonths: number | null;
  autoEnroll: boolean;
  tiers: readonly LoyaltyTierConfigDto[];
}

export interface LoyaltyRewardDto {
  id: string;
  name: string;
  description: string | null;
  rewardType: RewardType;
  pointsCost: number;
  serviceId: string | null;
  serviceName: string | null;
  packageId: string | null;
  packageName: string | null;
  discountAmount: number | null;
  discountPercent: number | null;
  isActive: boolean;
  createdAt: string;
}

export interface MembershipCardDto {
  id: string;
  customerId: string;
  customerName: string;
  customerEmail: string | null;
  customerPhone: string | null;
  cardNumber: string;
  currentTier: LoyaltyTier;
  tierName: string;
  pointsBalance: number;
  lifetimePointsEarned: number;
  lifetimePointsRedeemed: number;
  isActive: boolean;
  createdAt: string;
}

export interface PointTransactionDto {
  id: string;
  type: PointTransactionType;
  points: number;
  balanceAfter: number;
  description: string;
  transactionId: string | null;
  rewardName: string | null;
  createdAt: string;
}

export interface AvailableRewardDto {
  id: string;
  name: string;
  rewardType: RewardType;
  pointsCost: number;
  discountAmount: number | null;
  discountPercent: number | null;
}

export interface CustomerLoyaltySummaryDto {
  membershipCardId: string;
  cardNumber: string;
  currentTier: LoyaltyTier;
  tierName: string;
  pointsBalance: number;
  lifetimePointsEarned: number;
  pointsToNextTier: number | null;
  nextTierName: string | null;
  availableRewards: readonly AvailableRewardDto[];
}

export interface TierDistributionDto {
  tier: LoyaltyTier;
  tierName: string;
  count: number;
}

export interface TopLoyalCustomerDto {
  customerId: string;
  customerName: string;
  cardNumber: string;
  currentTier: LoyaltyTier;
  lifetimePointsEarned: number;
  pointsBalance: number;
}

export interface LoyaltyDashboardDto {
  totalMembers: number;
  totalPointsEarnedInPeriod: number;
  totalPointsRedeemedInPeriod: number;
  totalRedemptionsInPeriod: number;
  tierDistribution: readonly TierDistributionDto[];
  topCustomers: readonly TopLoyalCustomerDto[];
}

// ── Analytics Reports ────────────────────────────────────────────────────────

export interface CustomerAnalytics {
  from: string;
  to: string;
  branchId: string | null;
  totalCustomers: number;
  newCustomers: number;
  returningCustomers: number;
  retentionRate: number;
  averageVisitsPerCustomer: number;
  averageSpendPerVisit: number;
  topCustomers: readonly TopAnalyticsCustomer[];
  visitFrequencyDistribution: readonly VisitFrequencyBucket[];
  dailyTrend: readonly CustomerTrendDay[];
}

export interface TopAnalyticsCustomer {
  customerId: string;
  customerName: string;
  plateNumber: string | null;
  visitCount: number;
  totalSpent: number;
  averageSpend: number;
  lastVisit: string;
}

export interface VisitFrequencyBucket {
  bucket: string;
  customerCount: number;
}

export interface CustomerTrendDay {
  date: string;
  newCustomers: number;
  returningCustomers: number;
  totalTransactions: number;
}

export interface PeakHoursReport {
  from: string;
  to: string;
  branchId: string | null;
  totalTransactions: number;
  peakDay: string;
  peakHour: number;
  slots: readonly HourlySlot[];
}

export interface HourlySlot {
  /** 0 = Sunday … 6 = Saturday */
  dayOfWeek: number;
  /** 0–23 (Manila time) */
  hour: number;
  transactionCount: number;
  revenue: number;
}

export interface EmployeePerformanceReport {
  from: string;
  to: string;
  branchId: string | null;
  totalEmployees: number;
  totalCommissions: number;
  totalServicesPerformed: number;
  rankings: readonly EmployeeRanking[];
}

export interface EmployeeRanking {
  employeeId: string;
  employeeName: string;
  branchName: string;
  employeeType: string;
  servicesPerformed: number;
  revenueGenerated: number;
  commissionsEarned: number;
  daysWorked: number;
  daysLate: number;
  averageRevenuePerService: number;
  attendanceRate: number;
}

// ── Franchise ───────────────────────────────────────────────────────────────

export interface FranchiseSettingsDto {
  id: string;
  tenantId: string;
  royaltyRate: number;
  marketingFeeRate: number;
  technologyFeeRate: number;
  royaltyBasis: number;
  royaltyFrequency: number;
  enforceStandardServices: boolean;
  enforceStandardPricing: boolean;
  allowLocalServices: boolean;
  maxPriceVariance: number | null;
  enforceBranding: boolean;
  defaultFranchiseePlan: number;
  maxBranchesPerFranchisee: number;
}

export interface FranchiseeListItem {
  tenantId: string;
  name: string;
  franchiseCode: string | null;
  territoryName: string;
  branchCount: number;
  isActive: boolean;
  agreementStatus: number;
  revenueThisMonth: number;
  royaltyDue: number;
}

export interface FranchiseeDetail extends FranchiseeListItem {
  email: string;
  contactNumber: string;
  address: string;
  agreement: FranchiseAgreementDto | null;
  recentRoyalties: readonly RoyaltyPeriodDto[];
}

export interface FranchiseAgreementDto {
  id: string;
  franchisorTenantId: string;
  franchiseeTenantId: string;
  agreementNumber: string;
  territoryName: string;
  territoryDescription: string | null;
  exclusiveTerritory: boolean;
  startDate: string;
  endDate: string | null;
  initialFranchiseFee: number;
  status: number;
  customRoyaltyRate: number | null;
  customMarketingFeeRate: number | null;
  notes: string | null;
  createdAt: string;
}

export interface RoyaltyPeriodDto {
  id: string;
  franchiseeTenantId: string;
  franchiseeName: string;
  agreementId: string;
  periodStart: string;
  periodEnd: string;
  grossRevenue: number;
  royaltyRate: number;
  royaltyAmount: number;
  marketingFeeRate: number;
  marketingFeeAmount: number;
  technologyFeeRate: number;
  technologyFeeAmount: number;
  totalDue: number;
  status: number;
  paidDate: string | null;
  paymentReference: string | null;
}

export interface NetworkSummaryDto {
  totalFranchisees: number;
  activeFranchisees: number;
  suspendedFranchisees: number;
  pendingFranchisees: number;
  networkRevenueThisMonth: number;
  totalRoyaltiesCollected: number;
  pendingRoyalties: number;
  overdueRoyalties: number;
  averageRevenuePerFranchisee: number;
}

export interface FranchiseComplianceItem {
  tenantId: string;
  name: string;
  territoryName: string;
  usingStandardServices: boolean;
  pricingCompliant: boolean;
  royaltiesCurrent: boolean;
  agreementExpiringSoon: boolean;
  complianceScore: number;
}

export interface FranchiseBenchmarkDto {
  metric: string;
  yourValue: number;
  networkAverage: number;
  rank: number;
  totalInNetwork: number;
}

export interface FranchiseServiceTemplateDto {
  id: string;
  serviceName: string;
  description: string | null;
  categoryName: string | null;
  basePrice: number;
  durationMinutes: number;
  isRequired: boolean;
  isActive: boolean;
}

export interface FranchiseInvitationDto {
  id: string;
  email: string;
  businessName: string;
  ownerName: string | null;
  franchiseCode: string | null;
  territoryName: string | null;
  expiresAt: string;
  isUsed: boolean;
  createdAt: string;
}

export interface InvitationDetailsDto {
  franchisorName: string;
  businessName: string;
  email: string;
  franchiseCode: string | null;
  territoryName: string | null;
  expiresAt: string;
}

// ── Inventory ────────────────────────────────────────────────────────────────

export interface SupplyCategoryDto {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
}

export interface SupplyItemDto {
  id: string;
  branchId: string;
  branchName: string;
  categoryId: string | null;
  categoryName: string | null;
  name: string;
  description: string | null;
  unit: string;
  currentStock: number;
  reorderLevel: number | null;
  averageUnitCost: number;
  isActive: boolean;
  isLowStock: boolean;
  createdAt: string;
}

export interface SupplyItemDetailDto extends SupplyItemDto {
  recentMovements: readonly StockMovementDto[];
}

export interface StockMovementDto {
  id: string;
  branchName: string;
  itemName: string;
  type: string;
  quantity: number;
  unitCost: number | null;
  totalCost: number | null;
  reference: string | null;
  notes: string | null;
  performedBy: string | null;
  movementDate: string;
}

export interface ServiceSupplyUsageDto {
  supplyItemId: string;
  supplyItemName: string;
  unit: string;
  sizeUsages: readonly SizeUsageDto[];
}

export interface SizeUsageDto {
  sizeId: string | null;
  sizeName: string | null;
  quantityPerUse: number;
}

export interface ServiceCostBreakdownDto {
  serviceName: string;
  basePrice: number;
  sizeCosts: readonly SizeCostDto[];
}

export interface SizeCostDto {
  sizeId: string;
  sizeName: string;
  servicePrice: number;
  supplyCost: number;
  estimatedCommission: number;
  grossMargin: number;
  marginPercent: number;
  supplyCostLines: readonly SupplyCostLineDto[];
}

export interface SupplyCostLineDto {
  supplyName: string;
  unit: string;
  quantityPerUse: number;
  unitCost: number;
  lineCost: number;
}

export interface SupplierDto {
  id: string;
  name: string;
  contactPerson: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  isActive: boolean;
}

export interface PurchaseOrderDto {
  id: string;
  poNumber: string;
  supplierName: string;
  branchName: string;
  status: string;
  totalAmount: number;
  orderDate: string | null;
  expectedDeliveryDate: string | null;
  createdAt: string;
}

export interface PurchaseOrderDetailDto extends PurchaseOrderDto {
  supplierId: string;
  branchId: string;
  notes: string | null;
  lines: readonly PurchaseOrderLineDto[];
}

export interface PurchaseOrderLineDto {
  id: string;
  itemName: string;
  supplyItemId: string | null;
  merchandiseId: string | null;
  quantity: number;
  receivedQuantity: number;
  unitCost: number;
  totalCost: number;
}

export interface EquipmentDto {
  id: string;
  branchName: string;
  name: string;
  brand: string | null;
  model: string | null;
  serialNumber: string | null;
  status: string;
  location: string | null;
  isActive: boolean;
  lastMaintenanceDate: string | null;
  nextMaintenanceDue: string | null;
  createdAt: string;
}

export interface EquipmentDetailDto extends EquipmentDto {
  branchId: string;
  purchaseDate: string | null;
  purchaseCost: number | null;
  warrantyExpiry: string | null;
  notes: string | null;
  maintenanceLogs: readonly MaintenanceLogDto[];
}

export interface MaintenanceLogDto {
  id: string;
  type: string;
  description: string;
  cost: number | null;
  performedBy: string | null;
  performedDate: string;
  nextDueDate: string | null;
  nextDueHours: number | null;
  notes: string | null;
}

export interface InventorySummaryDto {
  totalSupplyItems: number;
  lowStockCount: number;
  outOfStockCount: number;
  totalStockValue: number;
  lowStockItems: readonly LowStockItemInventoryDto[];
}

export interface LowStockItemInventoryDto {
  id: string;
  name: string;
  unit: string;
  branchName: string;
  currentStock: number;
  reorderLevel: number | null;
  averageUnitCost: number;
}

export interface EquipmentMaintenanceReportDto {
  totalEquipment: number;
  needsMaintenanceCount: number;
  underRepairCount: number;
  totalMaintenanceCostThisMonth: number;
  upcomingMaintenance: readonly MaintenanceDueItemDto[];
  overdueMaintenance: readonly MaintenanceDueItemDto[];
}

export interface MaintenanceDueItemDto {
  equipmentId: string;
  equipmentName: string;
  branchName: string;
  lastMaintenanceDescription: string | null;
  nextDueDate: string | null;
  daysUntilDue: number;
}

// ── Bookings ─────────────────────────────────────────────────────────────────

/** A single service line on a booking, admin view. */
export interface BookingAdminServiceDto {
  serviceId: string;
  name: string;
  /** Exact price once the booking's vehicle has been classified, else null. */
  price: number | null;
  priceMin: number | null;
  priceMax: number | null;
}

/** Full detail for a single booking — POS uses this to auto-fill transactions. */
export interface BookingAdminDetailDto {
  id: string;
  branchId: string;
  branchName: string;
  customerId: string;
  customerName: string;
  customerPhone: string | null;
  vehicleId: string;
  plateNumber: string;
  makeName: string | null;
  modelName: string | null;
  /** ISO-8601 UTC. Display in Asia/Manila. */
  slotStartUtc: string;
  slotEndUtc: string;
  estimatedDurationMinutes: number;
  /** Stringified BookingStatus (e.g., "Confirmed", "Arrived"). */
  status: string;
  isVehicleClassified: boolean;
  estimatedTotal: number;
  estimatedTotalMin: number | null;
  estimatedTotalMax: number | null;
  cancellationReason: string | null;
  queueEntryId: string | null;
  transactionId: string | null;
  createdAtUtc: string;
  services: readonly BookingAdminServiceDto[];
}

/** Result returned by POST /bookings/{id}/classify-vehicle. */
export interface BookingClassificationResultDto {
  bookingId: string;
  carId: string;
  total: number;
  services: readonly {
    serviceId: string;
    serviceName: string;
    price: number;
  }[];
}

/** Result returned by PATCH /bookings/{id}/check-in. */
export interface BookingCheckInDto {
  bookingId: string;
  queueEntryId: string | null;
  queueNumber: string | null;
  /** Enum value (number) matching BookingStatus. */
  status: BookingStatus;
}

/** Summary row used by the admin bookings list. */
export interface BookingListItemDto {
  id: string;
  branchId: string;
  branchName: string;
  customerId: string;
  customerName: string;
  vehicleId: string;
  plateNumber: string;
  /** Comma-joined service names. */
  serviceSummary: string;
  /** ISO-8601 UTC slot start — display in Asia/Manila. */
  slotStartUtc: string;
  slotEndUtc: string;
  /** Stringified BookingStatus (e.g., "Confirmed", "Arrived"). */
  status: string;
  isVehicleClassified: boolean;
  estimatedTotal: number;
  estimatedTotalMin: number | null;
  estimatedTotalMax: number | null;
  queueEntryId: string | null;
  transactionId: string | null;
}

/** Per-branch online-booking configuration. */
export interface BookingSettingDto {
  branchId: string;
  /** "HH:mm" (TimeOnly on the wire). */
  openTime: string;
  /** "HH:mm" (TimeOnly on the wire). */
  closeTime: string;
  slotIntervalMinutes: number;
  maxBookingsPerSlot: number;
  advanceBookingDays: number;
  minLeadTimeMinutes: number;
  noShowGraceMinutes: number;
  isBookingEnabled: boolean;
  showInPublicDirectory: boolean;
}

// ── Customer Connect auth ─────────────────────────────────────────────────────

/**
 * Response payload from `POST /api/v1/connect/auth/otp/send`.
 * TTL is the server-side countdown until the code expires (seconds).
 */
export interface SendOtpResponse {
  ttlSeconds: number;
}

/**
 * Public profile for the signed-in Connect user.
 * Mirrors the C# `ConnectUserDto` record returned by the verify endpoint.
 */
export interface ConnectUserDto {
  id: string;
  phone: string;
  name: string;
  email: string | null;
  avatarUrl: string | null;
  /** True on the first successful verification for a phone number. */
  isNew: boolean;
}

/**
 * Response payload from `POST /api/v1/connect/auth/otp/verify`.
 * Access token lives ~30 minutes, refresh token ~30 days (and rotates on use).
 */
export interface VerifyOtpResponse {
  accessToken: string;
  /** ISO-8601 UTC instant. */
  accessTokenExpiresAt: string;
  refreshToken: string;
  /** ISO-8601 UTC instant. */
  refreshTokenExpiresAt: string;
  user: ConnectUserDto;
}

/**
 * Response payload from `POST /api/v1/connect/auth/refresh`.
 * Shape mirrors verify but without the user block — the stored user is still valid.
 */
export interface RefreshTokenResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

/**
 * One row in the Connect customer's cross-tenant service history — a
 * completed transaction at any car wash branch they've joined. Mirrors
 * the C# `ConnectServiceHistoryItemDto` record. Returned by
 * `GET /api/v1/connect/history` (flat array, newest first, no pagination).
 */
export interface ConnectHistoryItemDto {
  transactionId: string;
  transactionNumber: string;
  tenantId: string;
  tenantName: string;
  branchId: string;
  branchName: string;
  plateNumber: string;
  /** Decimal PHP amount from the server (already a number via System.Text.Json). */
  finalAmount: number;
  pointsEarned: number;
  /** ISO-8601 UTC instant. Convert to Asia/Manila for display. */
  completedAt: string;
  serviceNames: string[];
}

// ── Customer Connect profile ──────────────────────────────────────────────────

/**
 * Vehicle registered on a Connect user's global profile.
 *
 * Deliberately lacks type/size — those get assigned per-tenant by cashiers on
 * the customer's first physical visit to each car wash. Mirrors the C#
 * `ConnectVehicleDto` record.
 */
export interface ConnectVehicleDto {
  id: string;
  makeId: string;
  makeName: string;
  modelId: string;
  modelName: string;
  plateNumber: string;
  color: string | null;
  year: number | null;
}

/**
 * Full profile for the signed-in Connect user, returned by
 * `GET /api/v1/connect/profile`. Includes the user's vehicle list inline.
 */
export interface ConnectProfileDto {
  id: string;
  phone: string;
  name: string;
  email: string | null;
  avatarUrl: string | null;
  /** ISO-8601 UTC instant. */
  createdAt: string;
  vehicles: readonly ConnectVehicleDto[];
}

/** Request body for `PATCH /api/v1/connect/profile`. */
export interface UpdateConnectProfileRequest {
  name: string;
  email: string | null;
  avatarUrl: string | null;
}

/** Request body for `POST`/`PATCH /api/v1/connect/profile/vehicles[/{id}]`. */
export interface ConnectVehicleUpsertRequest {
  makeId: string;
  modelId: string;
  plateNumber: string;
  color: string | null;
  year: number | null;
}

/**
 * A tenant the authenticated Connect user has joined. Returned by
 * `GET /api/v1/connect/my-carwashes`. Loyalty tier/points are not included
 * here — fetch `/carwashes/{tenantId}/loyalty` separately for that data.
 */
export interface ConnectTenantSummaryDto {
  tenantId: string;
  tenantName: string;
  address: string;
  /** ISO-8601 UTC instant. */
  linkedAt: string;
}

// ── Customer Connect vehicle catalogue ────────────────────────────────────────

/** Global vehicle make for the Connect app's vehicle picker. */
export interface GlobalMakeDto {
  id: string;
  name: string;
  displayOrder: number;
}

/** Global vehicle model, scoped to a make. */
export interface GlobalModelDto {
  id: string;
  makeId: string;
  name: string;
  displayOrder: number;
}

// ── Customer Connect discovery ────────────────────────────────────────────────

/**
 * One row in the public car-wash directory returned by
 * `GET /api/v1/connect/carwashes`. NOTE: the endpoint emits one row per
 * *branch*, so tenants with multiple public branches appear multiple times —
 * the UI groups by `tenantId` for the tenant-level card.
 *
 * Mirrors the C# `CarWashListItemDto` record.
 */
export interface ConnectDiscoveryResultDto {
  tenantId: string;
  tenantName: string;
  branchId: string;
  branchName: string;
  address: string;
  contactNumber: string;
  /** Branch latitude in WGS84. `null` when the branch has no geolocation set. */
  latitude: number | null;
  /** Branch longitude in WGS84. `null` when the branch has no geolocation set. */
  longitude: number | null;
  /** Straight-line km from the caller's coords when supplied; otherwise `null`. */
  distanceKm: number | null;
  /** Local Manila `HH:mm`, or `null` when booking is disabled for the branch. */
  openTime: string | null;
  /** Local Manila `HH:mm`, or `null` when booking is disabled for the branch. */
  closeTime: string | null;
  isBookingEnabled: boolean;
  isJoined: boolean;
}

// ── Customer Connect car-wash detail ──────────────────────────────────────────

/** A publicly listed branch on a tenant's detail page. */
export interface ConnectBranchSummaryDto {
  id: string;
  name: string;
  address: string;
  contactNumber: string;
  latitude: number | null;
  longitude: number | null;
  /** Local Manila `HH:mm`, or `null` when booking is disabled. */
  openTime: string | null;
  /** Local Manila `HH:mm`, or `null` when booking is disabled. */
  closeTime: string | null;
  isBookingEnabled: boolean;
}

/**
 * A service offered by a tenant, as shown on the detail page (pre-vehicle
 * selection). Only the base price is returned here — per-vehicle pricing
 * ranges come from `GET /carwashes/{tenantId}/services?vehicleId=...`.
 *
 * Mirrors the C# `CarWashServiceDto` record.
 */
export interface ConnectServiceSummaryDto {
  id: string;
  name: string;
  description: string | null;
  basePrice: number;
}

/**
 * Full detail for a single car wash tenant returned by
 * `GET /api/v1/connect/carwashes/{tenantId}`.
 *
 * Mirrors the C# `CarWashDetailDto` record.
 */
export interface ConnectCarwashDetailDto {
  tenantId: string;
  tenantName: string;
  email: string;
  contactNumber: string;
  address: string;
  isJoined: boolean;
  branches: readonly ConnectBranchSummaryDto[];
  services: readonly ConnectServiceSummaryDto[];
}

// ── Customer Connect bookings ────────────────────────────────────────────────

/**
 * A single service line on a Connect booking. When `price` is set the
 * booking's vehicle was already classified; otherwise the
 * `priceMin`/`priceMax` range is shown. Mirrors C# `BookingServiceDto`.
 */
export interface ConnectBookingServiceDto {
  serviceId: string;
  name: string;
  price: number | null;
  priceMin: number | null;
  priceMax: number | null;
}

/**
 * Row returned by `GET /api/v1/connect/bookings` — summary fields for
 * the customer's "My Bookings" list. Mirrors C# `BookingListItemDto`
 * (the Connect variant; the admin DTO has a different shape).
 *
 * `status` is the stringified `BookingStatus` (e.g., "Confirmed",
 * "Arrived", "InService", "Completed", "Cancelled", "NoShow").
 */
export interface ConnectBookingListItemDto {
  id: string;
  tenantId: string;
  tenantName: string;
  branchId: string;
  branchName: string;
  /** ISO-8601 UTC. Display in Asia/Manila. */
  slotStartUtc: string;
  slotEndUtc: string;
  status: string;
  isVehicleClassified: boolean;
  estimatedTotal: number;
  estimatedTotalMin: number | null;
  estimatedTotalMax: number | null;
  vehicleId: string;
  plateNumber: string;
}

/**
 * Full detail for a single Connect booking. Returned by
 * `GET /api/v1/connect/bookings/{id}`. Mirrors C# `BookingDetailDto`.
 */
export interface ConnectBookingDetailDto {
  id: string;
  tenantId: string;
  tenantName: string;
  branchId: string;
  branchName: string;
  /** ISO-8601 UTC. Display in Asia/Manila. */
  slotStartUtc: string;
  slotEndUtc: string;
  /** Stringified BookingStatus. */
  status: string;
  isVehicleClassified: boolean;
  estimatedTotal: number;
  estimatedTotalMin: number | null;
  estimatedTotalMax: number | null;
  estimatedDurationMinutes: number;
  vehicleId: string;
  plateNumber: string;
  queueEntryId: string | null;
  queueNumber: string | null;
  /** Stringified QueueStatus when an entry exists, else null. */
  queueStatus: string | null;
  transactionId: string | null;
  services: readonly ConnectBookingServiceDto[];
}

/**
 * The caller's currently active queue entry across every tenant, or
 * `null` if none. Returned by `GET /api/v1/connect/queue/active`.
 *
 * `status` and `priority` arrive as numeric enum values (matches the
 * backend which does NOT register `JsonStringEnumConverter`). Compare
 * against `QueueStatus`/`QueuePriority` from `./enums`.
 */
export interface ConnectActiveQueueDto {
  queueEntryId: string;
  tenantId: string;
  tenantName: string;
  branchId: string;
  branchName: string;
  queueNumber: string;
  status: QueueStatus;
  priority: QueuePriority;
  /** Number of entries ahead of this one; null while not Waiting/Called. */
  aheadCount: number | null;
  estimatedWaitMinutes: number | null;
  /** ISO-8601 UTC when the entry was called. */
  calledAt: string | null;
  /** ISO-8601 UTC when service started. */
  startedAt: string | null;
  bookingId: string | null;
}

// ── Customer Connect booking wizard ──────────────────────────────────────────

/**
 * One tenant service with pricing resolved for the caller's selected vehicle.
 *
 * `priceMode` is the server's decision about how to present the price:
 *  - `"exact"` — the vehicle is classified at this tenant, so only `price`
 *    is populated.
 *  - `"estimate"` — the vehicle is unclassified; `priceMin`/`priceMax`
 *    span the ServicePricing matrix (with the base price as a fallback)
 *    and `price` is null.
 *
 * Mirrors C# `ConnectServicePriceDto`.
 */
export interface ConnectServicePriceDto {
  serviceId: string;
  name: string;
  description: string | null;
  /** `"exact"` or `"estimate"`. */
  priceMode: string;
  price: number | null;
  priceMin: number | null;
  priceMax: number | null;
}

/**
 * Response wrapper for
 * `GET /api/v1/connect/carwashes/{tenantId}/services?vehicleId={id}`.
 * Mirrors C# `ConnectServicesWithPricingDto`.
 */
export interface ConnectServicesWithPricingDto {
  tenantId: string;
  vehicleId: string;
  /** `"exact"` when the caller's vehicle is already classified, else `"estimate"`. */
  priceMode: string;
  services: readonly ConnectServicePriceDto[];
}

/**
 * One available booking slot on a branch+date, returned by
 * `GET /api/v1/connect/carwashes/{tenantId}/slots?branchId={id}&date=YYYY-MM-DD`.
 *
 * The backend already filters out slots with zero remaining capacity and
 * those that violate `minLeadTimeMinutes`, so the client can render every
 * returned slot as tappable. Mirrors C# `BookingSlotDto`.
 */
export interface ConnectBookingSlotDto {
  /** ISO-8601 UTC instant — pass straight back on `POST /bookings`. */
  slotStartUtc: string;
  slotEndUtc: string;
  /** Manila-local `HH:mm`, ready to render. */
  localTime: string;
  remainingCapacity: number;
}

/** Alias — the customer app uses the plural; admin uses a different shape. */
export type ConnectAvailabilityDto = ConnectBookingSlotDto;

/**
 * Request body for `POST /api/v1/connect/bookings`. Sends the slot back as
 * an ISO-8601 UTC instant — the server validates it against the branch's
 * `BookingSetting` and rejects with 400/409 on mismatch.
 *
 * `notes` is accepted by the UI but not yet persisted by the current
 * `CreateBookingCommand` — the field is kept so the shape matches once the
 * backend starts storing it.
 */
export interface CreateBookingRequest {
  tenantId: string;
  branchId: string;
  vehicleId: string;
  /** ISO-8601 UTC instant. */
  slotStartUtc: string;
  serviceIds: string[];
  notes?: string;
}

/**
 * Response payload from `POST /api/v1/connect/bookings`. The backend
 * returns the full booking detail on create so the confirmation redirect
 * can warm the detail cache immediately. Alias of
 * `ConnectBookingDetailDto` — kept as a separate name so call sites read
 * semantically.
 */
export type ConnectBookingCreatedDto = ConnectBookingDetailDto;

// ── Customer Connect loyalty / membership ────────────────────────────────────

/**
 * Customer-facing loyalty card summary for a single tenant. Returned by
 * `GET /api/v1/connect/carwashes/{tenantId}/loyalty`.
 *
 * `isEnrolled` is `false` when the tenant doesn't offer loyalty (not on
 * their plan) or the customer has not yet earned a card — all numeric
 * fields are then zero and `pointsToNextTier`/`nextTierName` are null.
 *
 * Mirrors the C# `ConnectMembershipDto` record.
 */
export interface ConnectLoyaltyCardDto {
  isEnrolled: boolean;
  membershipCardId: string | null;
  cardNumber: string | null;
  currentTier: LoyaltyTier;
  tierName: string;
  pointsBalance: number;
  lifetimePointsEarned: number;
  lifetimePointsRedeemed: number;
  /** Points required to reach the next tier, or `null` at the top tier. */
  pointsToNextTier: number | null;
  nextTierName: string | null;
  tierMultiplier: number;
}

/**
 * A loyalty reward a customer may redeem at a tenant. Returned in bulk by
 * `GET /api/v1/connect/carwashes/{tenantId}/rewards`. `isAffordable`
 * reflects the caller's current points balance at this tenant — falsy
 * cards should be rendered disabled with a "need X more points" hint.
 *
 * Mirrors the C# `ConnectRewardDto` record.
 */
export interface ConnectRewardDto {
  id: string;
  name: string;
  description: string | null;
  rewardType: RewardType;
  pointsCost: number;
  serviceId: string | null;
  serviceName: string | null;
  packageId: string | null;
  packageName: string | null;
  discountAmount: number | null;
  discountPercent: number | null;
  isAffordable: boolean;
}

/**
 * Payload returned by `POST /api/v1/connect/carwashes/{tenantId}/rewards/redeem`.
 * The customer presents `pointTransactionId` (or a QR derived from it) at
 * the POS to claim the reward.
 *
 * Mirrors the C# `ConnectRedemptionResultDto` record.
 */
export interface RedeemRewardResponseDto {
  pointTransactionId: string;
  rewardId: string;
  rewardName: string;
  pointsDeducted: number;
  newBalance: number;
}

/**
 * A single row in the Connect "Points history" feed. Negative `points`
 * indicate a redemption or expiry; positive values indicate points earned.
 *
 * Mirrors the C# `ConnectPointTransactionDto` record.
 */
export interface ConnectPointTransactionDto {
  id: string;
  type: PointTransactionType;
  points: number;
  balanceAfter: number;
  description: string;
  rewardName: string | null;
  /** ISO-8601 UTC. */
  createdAt: string;
}

// ── Customer Connect referrals ───────────────────────────────────────────────

/**
 * The caller's referral code at a tenant plus aggregate share stats.
 * Returned by `GET /api/v1/connect/carwashes/{tenantId}/referral-code` —
 * the handler lazily issues a code on first read so a joined customer
 * always has something to share.
 *
 * Mirrors the C# `ConnectReferralCodeDto` record.
 */
export interface ConnectReferralCodeDto {
  code: string;
  /** Points the referrer earns when a referral completes. */
  referrerPointsReward: number;
  /** Points the new customer earns on their first wash. */
  referredPointsReward: number;
  totalReferrals: number;
  completedReferrals: number;
  pendingReferrals: number;
  /** Lifetime points this customer has earned from completed referrals. */
  pointsEarned: number;
}

/**
 * A single row on the "My referrals" list at a tenant. Only referrals
 * where someone actually used the code are returned — pending-but-unused
 * codes are the caller's own code, not referrals.
 *
 * Mirrors the C# `ConnectReferralListItemDto` record.
 */
export interface ConnectReferralHistoryItemDto {
  id: string;
  /** Full name of the referred customer, or `null` if no longer resolvable. */
  referredName: string | null;
  status: ReferralStatus;
  /** Points the referrer earned once the referral completed. */
  referrerPointsEarned: number;
  /** ISO-8601 UTC when the referral was completed, or `null` if still pending. */
  completedAt: string | null;
  /** ISO-8601 UTC. */
  createdAt: string;
}
