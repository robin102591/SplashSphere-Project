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
  CommissionType,
  EmployeeType,
  ModifierType,
  PaymentMethod,
  PayrollStatus,
  QueuePriority,
  QueueStatus,
  ReceiptLineType,
  TransactionStatus,
} from './enums';

// ── Auth / User ───────────────────────────────────────────────────────────────

export interface CurrentUserTenant {
  id: string;
  name: string;
  email: string;
  contactNumber: string;
  address: string;
  isActive: boolean;
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
  cashierName: string;
  queueEntryId: string | null;
  createdAt: string;
}

export interface TransactionDetail extends TransactionSummary {
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
  entryCount: number;
  totalNetPay: number;
  createdAt: string;
  updatedAt: string;
}

export interface PayrollPeriodDetail extends Omit<PayrollPeriodSummary, 'entryCount' | 'totalNetPay'> {
  entries: readonly PayrollEntry[];
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
