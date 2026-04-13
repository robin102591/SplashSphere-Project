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
