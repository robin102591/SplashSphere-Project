# SplashSphere — Negosyo AI Data Metrics Reference

> **Purpose:** Every metric Negosyo AI needs to function, mapped to its source entity, 
> calculation formula, and the phase that provides the underlying data. Use this as a 
> checklist — if a metric's source phase isn't built yet, the AI can't answer questions 
> about it.

---

## 1. Revenue & Sales Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 1.1 | **Total Revenue** | `SUM(Transaction.finalAmount) WHERE status = COMPLETED` | Transaction | Core |
| 1.2 | **Gross Revenue** | `SUM(Transaction.totalAmount) WHERE status = COMPLETED` (before discounts) | Transaction | Core |
| 1.3 | **Total Discounts** | `SUM(Transaction.discountAmount)` | Transaction | Core |
| 1.4 | **Net Revenue** | `totalAmount - discountAmount + taxAmount` = `finalAmount` | Transaction | Core |
| 1.5 | **Transaction Count** | `COUNT(Transaction) WHERE status = COMPLETED` | Transaction | Core |
| 1.6 | **Average Transaction Value** | `Total Revenue ÷ Transaction Count` | Calculated | Core |
| 1.7 | **Revenue by Service** | `SUM(TransactionService.totalPrice) GROUP BY serviceId` | TransactionService | Core |
| 1.8 | **Revenue by Package** | `SUM(TransactionPackage.totalPrice) GROUP BY packageId` | TransactionPackage | Core |
| 1.9 | **Revenue by Payment Method** | `SUM(Payment.amount) GROUP BY method` | Payment | Core |
| 1.10 | **Revenue by Branch** | `SUM(Transaction.finalAmount) GROUP BY branchId` | Transaction | Core |
| 1.11 | **Revenue by Hour of Day** | `SUM(Transaction.finalAmount) GROUP BY EXTRACT(HOUR FROM transactionDate)` | Transaction | Core |
| 1.12 | **Revenue by Day of Week** | `SUM(Transaction.finalAmount) GROUP BY EXTRACT(DOW FROM transactionDate)` | Transaction | Core |
| 1.13 | **Merchandise Revenue** | `SUM(TransactionMerchandise.totalPrice)` | TransactionMerchandise | Core |
| 1.14 | **Service Revenue** | Total Revenue - Merchandise Revenue | Calculated | Core |
| 1.15 | **Revenue Growth (Period over Period)** | `(Current Period Revenue - Previous Period Revenue) ÷ Previous Period Revenue × 100` | Calculated | Core |
| 1.16 | **Revenue per Employee** | `Total Revenue ÷ Active Employee Count` | Calculated | Core |
| 1.17 | **Revenue per Branch** | `Total Revenue ÷ Active Branch Count` | Calculated | Core |
| 1.18 | **Cancelled Transaction Value** | `SUM(Transaction.finalAmount) WHERE status = CANCELLED` | Transaction | Core |
| 1.19 | **Refunded Amount** | `SUM(Transaction.finalAmount) WHERE status = REFUNDED` | Transaction | Core |

---

## 2. Profitability Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 2.1 | **Merchandise COGS** | `SUM(StockMovement.totalCost) WHERE type = SaleOut` | StockMovement | 18 (Inventory) |
| 2.2 | **Supply Usage Cost** | `SUM(StockMovement.totalCost) WHERE type = UsageOut` | StockMovement | 18 (Inventory) |
| 2.3 | **Total COGS** | Merchandise COGS + Supply Usage Cost | Calculated | 18 |
| 2.4 | **Gross Profit** | Net Revenue - Total COGS | Calculated | 18 |
| 2.5 | **Gross Profit Margin** | `Gross Profit ÷ Net Revenue × 100` | Calculated | 18 |
| 2.6 | **Total Expenses** | `SUM(Expense.amount)` grouped by category | Expense | 15 (Expenses) |
| 2.7 | **Expense by Category** | `SUM(Expense.amount) GROUP BY categoryId` | Expense | 15 |
| 2.8 | **Total Commission Paid** | `SUM(Transaction.totalCommissionAmount) WHERE status = COMPLETED` | Transaction | Core |
| 2.9 | **Total Payroll Cost** | `SUM(PayrollEntry.totalAmount)` for a period | PayrollEntry | Core |
| 2.10 | **Operating Expenses** | Total Expenses + Total Payroll Cost | Calculated | 15 |
| 2.11 | **Net Profit** | Gross Profit - Total Expenses | Calculated | 15 + 18 |
| 2.12 | **Net Profit Margin** | `Net Profit ÷ Net Revenue × 100` | Calculated | 15 + 18 |
| 2.13 | **Cost per Wash (by service × size)** | `SUM(ServiceSupplyUsage.quantityPerUse × SupplyItem.averageUnitCost) WHERE serviceId AND sizeId` | ServiceSupplyUsage | 18 |
| 2.14 | **Service Margin (by service × size)** | `(ServicePricing.price - CostPerWash - CommissionPerEmployee × AvgEmployeeCount) ÷ ServicePricing.price × 100` | Calculated | 18 |
| 2.15 | **Merchandise Markup** | `(Merchandise.price - Merchandise.cost) ÷ Merchandise.cost × 100` | Merchandise | Core |
| 2.16 | **Equipment Maintenance Cost** | `SUM(MaintenanceLog.cost)` for a period | MaintenanceLog | 18 |
| 2.17 | **Break-even Transaction Count** | `Total Fixed Costs (rent + utilities + daily employee salaries) ÷ Average Profit per Transaction` | Calculated | 15 + 18 |

---

## 3. Employee Performance Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 3.1 | **Total Commission Earned** | `SUM(TransactionEmployee.totalCommissionAmount)` per employee per period | TransactionEmployee | Core |
| 3.2 | **Services Completed** | `SUM(TransactionEmployee.servicesWorkedOn)` per employee | TransactionEmployee | Core |
| 3.3 | **Average Commission per Service** | `Total Commission ÷ Services Completed` | Calculated | Core |
| 3.4 | **Days Worked** | `COUNT(Attendance) WHERE isPresent = true` per employee per period | Attendance | Core |
| 3.5 | **Hours Worked** | `SUM(Attendance.hoursWorked)` per employee per period | Attendance | Core |
| 3.6 | **Commission per Hour** | `Total Commission ÷ Hours Worked` | Calculated | Core |
| 3.7 | **Daily Rate Total** | `Employee.dailyRate × Days Worked` (DAILY employees only) | Employee + Attendance | Core |
| 3.8 | **Base Salary** | `PayrollEntry.baseSalary` | PayrollEntry | Core |
| 3.9 | **Gross Pay** | `PayrollEntry.baseSalary + commissionAmount + bonusAmount` | PayrollEntry | Core |
| 3.10 | **Total Deductions** | `PayrollEntry.deductionAmount` (includes cash advance deductions) | PayrollEntry | Core + 15 |
| 3.11 | **Net Pay** | `PayrollEntry.totalAmount` (gross - deductions) | PayrollEntry | Core |
| 3.12 | **Attendance Rate** | `Days Present ÷ Total Working Days in Period × 100` | Attendance | Core |
| 3.13 | **Revenue Contribution** | `SUM(TransactionService.totalPrice + TransactionPackage.totalPrice)` for services this employee worked on | TransactionService + TransactionPackage | Core |
| 3.14 | **Employee Efficiency Score** | `Commission per Hour × Attendance Rate` (composite) | Calculated | Core |
| 3.15 | **Employee Count (Active)** | `COUNT(Employee) WHERE isActive = true AND branchId = X` | Employee | Core |
| 3.16 | **Commission vs Daily Ratio** | `COUNT(COMMISSION employees) ÷ COUNT(ALL employees)` | Employee | Core |
| 3.17 | **Cash Advance Outstanding** | `SUM(CashAdvance.remainingBalance) WHERE isFullyPaid = false` per employee | CashAdvance | 15 |
| 3.18 | **Avg Services per Day per Employee** | `Services Completed ÷ Days Worked` | Calculated | Core |

---

## 4. Customer Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 4.1 | **Total Customers** | `COUNT(Customer) WHERE isActive = true` | Customer | Core |
| 4.2 | **New Customers (period)** | `COUNT(Customer) WHERE createdAt within period` | Customer | Core |
| 4.3 | **Returning Customers (period)** | `COUNT(DISTINCT customerId) WHERE has >1 transaction` | Transaction | Core |
| 4.4 | **Return Rate** | `Returning Customers ÷ Total Customers with transactions × 100` | Calculated | Core |
| 4.5 | **Average Visit Frequency** | `AVG(transaction count per customer per month)` | Transaction | Core |
| 4.6 | **Average Spend per Customer** | `Total Revenue ÷ Unique Customers with transactions` | Calculated | Core |
| 4.7 | **Customer Lifetime Value (CLV)** | `Customer.totalSpent` (cumulative) | Customer | 15 (Loyalty) |
| 4.8 | **Loyalty Points Outstanding** | `SUM(Customer.loyaltyPoints)` across all customers | Customer | 15 (Loyalty) |
| 4.9 | **Tier Distribution** | `COUNT(Customer) GROUP BY membershipTier` (Bronze/Silver/Gold/Platinum) | Customer | 15 (Loyalty) |
| 4.10 | **Tier Upgrade Rate** | `COUNT(customers who upgraded tier this period) ÷ Eligible Customers × 100` | LoyaltyTransaction | 15 (Loyalty) |
| 4.11 | **Points Redeemed** | `SUM(LoyaltyTransaction.points) WHERE type = RedeemedDiscount OR RedeemedService` | LoyaltyTransaction | 15 (Loyalty) |
| 4.12 | **Referral Count** | `COUNT(Customer) WHERE referredById IS NOT NULL` in period | Customer | 15 (Loyalty) |
| 4.13 | **Churn Risk Customers** | `COUNT(Customer) WHERE lastVisitDate < NOW() - 30 days AND membershipTier IN (Gold, Platinum)` | Customer | 15 (Loyalty) |
| 4.14 | **Days Since Last Visit** | `NOW() - Customer.lastVisitDate` per customer | Customer | 15 (Loyalty) |
| 4.15 | **Top Customers by Revenue** | `ORDER BY Customer.totalSpent DESC LIMIT N` | Customer | 15 (Loyalty) |
| 4.16 | **Customer with Most Vehicles** | `COUNT(Car) GROUP BY customerId ORDER BY DESC` | Car | Core |
| 4.17 | **Walk-in Rate** | `COUNT(Transaction WHERE customerId IS NULL) ÷ Total Transaction Count × 100` | Transaction | Core |

---

## 5. Vehicle & Service Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 5.1 | **Vehicles Registered** | `COUNT(Car) WHERE isActive = true` | Car | Core |
| 5.2 | **Vehicles by Type** | `COUNT(Car) GROUP BY vehicleTypeId` | Car | Core |
| 5.3 | **Vehicles by Size** | `COUNT(Car) GROUP BY sizeId` | Car | Core |
| 5.4 | **Most Common Vehicle** | `COUNT(Car) GROUP BY makeId, modelId ORDER BY DESC LIMIT 1` | Car | Core |
| 5.5 | **Top Services by Volume** | `COUNT(TransactionService) GROUP BY serviceId ORDER BY DESC` | TransactionService | Core |
| 5.6 | **Top Services by Revenue** | `SUM(TransactionService.totalPrice) GROUP BY serviceId ORDER BY DESC` | TransactionService | Core |
| 5.7 | **Top Packages by Volume** | `COUNT(TransactionPackage) GROUP BY packageId ORDER BY DESC` | TransactionPackage | Core |
| 5.8 | **Service Price (matrix)** | `ServicePricing.price WHERE serviceId AND vehicleTypeId AND sizeId` | ServicePricing | Core |
| 5.9 | **Commission Rate (matrix)** | `ServiceCommission rate/fixedAmount WHERE serviceId AND vehicleTypeId AND sizeId` | ServiceCommission | Core |
| 5.10 | **Service Duration (avg actual)** | `AVG(actual duration)` if tracked, else `Service.durationMinutes` | Service | Core |
| 5.11 | **Revenue per Vehicle Size** | `SUM(TransactionService.totalPrice) GROUP BY Car.sizeId` | TransactionService + Car | Core |
| 5.12 | **Avg Transaction Value by Vehicle Size** | `AVG(Transaction.finalAmount) grouped by vehicle size` | Transaction + Car | Core |

---

## 6. Inventory & Supply Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 6.1 | **Current Stock Level** | `SupplyItem.currentStock` per item per branch | SupplyItem | 18 |
| 6.2 | **Stock Value** | `SUM(SupplyItem.currentStock × SupplyItem.averageUnitCost)` | SupplyItem | 18 |
| 6.3 | **Items Below Reorder Level** | `COUNT(SupplyItem) WHERE currentStock < reorderLevel` | SupplyItem | 18 |
| 6.4 | **Items Out of Stock** | `COUNT(SupplyItem) WHERE currentStock <= 0` | SupplyItem | 18 |
| 6.5 | **Daily Usage Rate** | `SUM(StockMovement.quantity WHERE type = UsageOut AND last 7 days) ÷ 7` per item | StockMovement | 18 |
| 6.6 | **Days Until Depletion** | `currentStock ÷ Daily Usage Rate` | Calculated | 18 |
| 6.7 | **Supply Cost per Day** | `SUM(StockMovement.totalCost WHERE type = UsageOut)` per day | StockMovement | 18 |
| 6.8 | **Unit Cost Trend** | `StockMovement.unitCost WHERE type = PurchaseIn ORDER BY date` per item | StockMovement | 18 |
| 6.9 | **Supply Usage by Service** | `SUM(ServiceSupplyUsage.quantityPerUse × transaction count)` per service | ServiceSupplyUsage + Transactions | 18 |
| 6.10 | **Supply Usage by Size** | `SUM(quantities) GROUP BY sizeId` | ServiceSupplyUsage | 18 |
| 6.11 | **Merchandise Stock Level** | `Merchandise.inventoryCount` per item | Merchandise | Core |
| 6.12 | **Merchandise Items Low Stock** | `COUNT(Merchandise) WHERE inventoryCount < lowStockThreshold` | Merchandise | Core |
| 6.13 | **Merchandise Sold (period)** | `SUM(TransactionMerchandise.quantity) GROUP BY merchandiseId` | TransactionMerchandise | Core |
| 6.14 | **Merchandise Margin** | `SUM((price - cost) × quantity sold)` | TransactionMerchandise + Merchandise | Core |
| 6.15 | **Purchase Order Count** | `COUNT(PurchaseOrder) GROUP BY status` | PurchaseOrder | 18 |
| 6.16 | **Pending Deliveries** | `COUNT(PurchaseOrder) WHERE status IN (Sent, PartiallyReceived)` | PurchaseOrder | 18 |
| 6.17 | **Total Purchase Spend (period)** | `SUM(PurchaseOrder.totalAmount) WHERE status = Received AND within period` | PurchaseOrder | 18 |
| 6.18 | **Top Supplier by Spend** | `SUM(PurchaseOrder.totalAmount) GROUP BY supplierId ORDER BY DESC` | PurchaseOrder | 18 |

---

## 7. Cash & Shift Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 7.1 | **Opening Cash Fund** | `CashierShift.openingCashFund` | CashierShift | 15.9 |
| 7.2 | **Total Cash Received** | `CashierShift.totalCashPayments` | CashierShift | 15.9 |
| 7.3 | **Total Non-Cash Received** | `CashierShift.totalNonCashPayments` | CashierShift | 15.9 |
| 7.4 | **Cash In (manual)** | `CashierShift.totalCashIn` (additional funds, change from owner) | CashierShift | 15.9 |
| 7.5 | **Cash Out (manual)** | `CashierShift.totalCashOut` (supplies, meals, vale) | CashierShift | 15.9 |
| 7.6 | **Expected Cash in Drawer** | `OpeningFund + CashPayments + CashIn - CashOut` | Calculated | 15.9 |
| 7.7 | **Actual Cash Counted** | `CashierShift.actualCashInDrawer` (from denomination count) | CashierShift | 15.9 |
| 7.8 | **Cash Variance** | `Actual - Expected` (positive = over, negative = short) | CashierShift | 15.9 |
| 7.9 | **Variance per Cashier (trend)** | `CashierShift.variance` over time per cashierId | CashierShift | 15.9 |
| 7.10 | **Avg Variance per Cashier** | `AVG(CashierShift.variance) GROUP BY cashierId` over period | CashierShift | 15.9 |
| 7.11 | **Consecutive Short Count** | Count of sequential shifts with negative variance per cashier | CashierShift | 15.9 |
| 7.12 | **Shift Duration** | `ClosedAt - OpenedAt` per shift | CashierShift | 15.9 |
| 7.13 | **Transactions per Shift** | `CashierShift.totalTransactionCount` | CashierShift | 15.9 |
| 7.14 | **Revenue per Shift** | `CashierShift.totalRevenue` | CashierShift | 15.9 |
| 7.15 | **Cash Movement Log** | List of CashMovement records per shift (type, amount, reason, time) | CashMovement | 15.9 |
| 7.16 | **Denomination Breakdown** | List of ShiftDenomination records (value × count = subtotal) | ShiftDenomination | 15.9 |
| 7.17 | **Shifts Pending Review** | `COUNT(CashierShift) WHERE reviewStatus = Pending` | CashierShift | 15.9 |
| 7.18 | **Flagged Shifts** | `COUNT(CashierShift) WHERE reviewStatus = Flagged` | CashierShift | 15.9 |

---

## 8. Payroll & Compensation Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 8.1 | **Total Payroll (period)** | `SUM(PayrollEntry.totalAmount)` for a PayrollPeriod | PayrollEntry | Core |
| 8.2 | **Total Base Salaries** | `SUM(PayrollEntry.baseSalary)` | PayrollEntry | Core |
| 8.3 | **Total Commissions** | `SUM(PayrollEntry.commissionAmount)` | PayrollEntry | Core |
| 8.4 | **Total Bonuses** | `SUM(PayrollEntry.bonusAmount)` | PayrollEntry | Core |
| 8.5 | **Total Deductions** | `SUM(PayrollEntry.deductionAmount)` | PayrollEntry | Core |
| 8.6 | **Payroll Period Status** | `PayrollPeriod.status` (Open, Closed, Processed) | PayrollPeriod | Core |
| 8.7 | **Payroll as % of Revenue** | `Total Payroll ÷ Total Revenue × 100` for same period | Calculated | Core |
| 8.8 | **Avg Pay per Employee** | `Total Payroll ÷ Employee Count` | Calculated | Core |
| 8.9 | **Cash Advance Balance (total)** | `SUM(CashAdvance.remainingBalance) WHERE isFullyPaid = false` | CashAdvance | 15 |
| 8.10 | **Cash Advances This Period** | `SUM(CashAdvance.amount) WHERE requestDate within period AND status = Approved` | CashAdvance | 15 |
| 8.11 | **Cash Advance Deductions Applied** | `SUM(CashAdvanceDeduction.amount)` for a PayrollPeriod | CashAdvanceDeduction | 15 |
| 8.12 | **Payroll Week-over-Week Change** | `(This Week Payroll - Last Week Payroll) ÷ Last Week × 100` | Calculated | Core |
| 8.13 | **Commission-to-Revenue Ratio** | `Total Commissions ÷ Service Revenue × 100` | Calculated | Core |

---

## 9. Queue & Operations Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 9.1 | **Queue Entries Today** | `COUNT(QueueEntry) WHERE date = today` | QueueEntry | Core |
| 9.2 | **Currently Waiting** | `COUNT(QueueEntry) WHERE status = WAITING` | QueueEntry | Core |
| 9.3 | **Currently In Service** | `COUNT(QueueEntry) WHERE status = IN_SERVICE` | QueueEntry | Core |
| 9.4 | **Served Today** | `COUNT(QueueEntry) WHERE status = COMPLETED AND date = today` | QueueEntry | Core |
| 9.5 | **No-Shows Today** | `COUNT(QueueEntry) WHERE status = NO_SHOW AND date = today` | QueueEntry | Core |
| 9.6 | **No-Show Rate** | `No-Shows ÷ Total Queue Entries × 100` | Calculated | Core |
| 9.7 | **Average Wait Time** | `AVG(calledAt - createdAt)` for queue entries | QueueEntry | Core |
| 9.8 | **Average Service Time** | `AVG(completedAt - startedAt)` for queue entries | QueueEntry | Core |
| 9.9 | **Peak Queue Depth** | `MAX(concurrent WAITING entries)` by hour | QueueEntry | Core |
| 9.10 | **VIP/Express Entries** | `COUNT(QueueEntry) WHERE priority IN (VIP, EXPRESS)` | QueueEntry | Core |
| 9.11 | **Queue Throughput per Hour** | `COUNT(completed entries) GROUP BY hour` | QueueEntry | Core |

---

## 10. Branch Comparison Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 10.1 | **Revenue by Branch** | `SUM(Transaction.finalAmount) GROUP BY branchId` | Transaction | Core |
| 10.2 | **Transaction Count by Branch** | `COUNT(Transaction) GROUP BY branchId` | Transaction | Core |
| 10.3 | **Avg Ticket by Branch** | `AVG(Transaction.finalAmount) GROUP BY branchId` | Transaction | Core |
| 10.4 | **Employee Count by Branch** | `COUNT(Employee WHERE isActive) GROUP BY branchId` | Employee | Core |
| 10.5 | **Commission Total by Branch** | `SUM(Transaction.totalCommissionAmount) GROUP BY branchId` | Transaction | Core |
| 10.6 | **Expense Total by Branch** | `SUM(Expense.amount) GROUP BY branchId` | Expense | 15 |
| 10.7 | **Net Profit by Branch** | Revenue - COGS - Expenses - Commissions per branch | Calculated | 15 + 18 |
| 10.8 | **Customer Count by Branch** | `COUNT(Customer) GROUP BY branchId` | Customer | Core |
| 10.9 | **Queue Wait Time by Branch** | `AVG(wait time) GROUP BY branchId` | QueueEntry | Core |
| 10.10 | **Shift Variance by Branch** | `AVG(CashierShift.variance) GROUP BY branchId` | CashierShift | 15.9 |
| 10.11 | **Supply Cost by Branch** | `SUM(StockMovement.totalCost WHERE UsageOut) GROUP BY branchId` | StockMovement | 18 |

---

## 11. Trend & Forecasting Metrics

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 11.1 | **Revenue Trend (daily)** | `SUM(Transaction.finalAmount) GROUP BY DATE(transactionDate)` over 30-90 days | Transaction | Core |
| 11.2 | **Revenue Trend (weekly)** | `SUM per week` over 12-52 weeks | Transaction | Core |
| 11.3 | **Revenue Trend (monthly)** | `SUM per month` over 6-12 months | Transaction | Core |
| 11.4 | **Transaction Volume Trend** | `COUNT per day/week/month` | Transaction | Core |
| 11.5 | **Commission Trend** | `SUM(totalCommissionAmount) per week` | Transaction | Core |
| 11.6 | **Expense Trend** | `SUM(Expense.amount) per week/month` | Expense | 15 |
| 11.7 | **Profit Margin Trend** | `Net Profit Margin per week/month` | Calculated | 15 + 18 |
| 11.8 | **Customer Growth Trend** | `COUNT(new customers) per month` | Customer | Core |
| 11.9 | **Supply Usage Trend** | `SUM(UsageOut quantity) per week` per supply item | StockMovement | 18 |
| 11.10 | **Unit Cost Trend** | `PurchaseIn unitCost over time` per supply item | StockMovement | 18 |
| 11.11 | **Payroll Trend** | `Total payroll per week` over 12 weeks | PayrollEntry | Core |
| 11.12 | **Avg Transaction Value Trend** | `AVG(finalAmount) per day` over 30 days | Transaction | Core |
| 11.13 | **Day-of-Week Revenue Pattern** | `AVG revenue per day of week` over 8+ weeks | Transaction | Core |
| 11.14 | **Hour-of-Day Revenue Pattern** | `AVG revenue per hour` over 30 days | Transaction | Core |
| 11.15 | **Seasonal Patterns** | Month-over-month revenue comparison across 12+ months | Transaction | Core |

---

## 12. Anomaly Detection Inputs

| # | What to Detect | Comparison | Threshold | Source | Phase |
|---|---|---|---|---|---|
| 12.1 | **Revenue anomaly** | Today vs 30-day rolling avg (same day of week) | > 20% deviation | Transaction | Core |
| 12.2 | **Cash variance pattern** | Cashier's last 5 shifts | 3+ consecutive negative | CashierShift | 15.9 |
| 12.3 | **Employee performance drop** | This week vs 4-week avg commission | > 30% drop | TransactionEmployee | Core |
| 12.4 | **Stock depletion warning** | Current stock ÷ daily usage rate | < 5 days remaining | SupplyItem + StockMovement | 18 |
| 12.5 | **Customer churn risk** | Last visit date for Gold/Platinum | > 30 days inactive | Customer | 15 (Loyalty) |
| 12.6 | **Pricing opportunity** | Service margin vs tenant avg margin | > 10% below average | ServicePricing + ServiceSupplyUsage | Core + 18 |
| 12.7 | **Peak hour opportunity** | Revenue by hour distribution | Bottom 20% hours | Transaction | Core |
| 12.8 | **Payroll anomaly** | This week's total vs 4-week avg | > 20% deviation | PayrollEntry | Core |
| 12.9 | **Expense spike** | Category expense vs 4-week avg | > 50% increase | Expense | 15 |
| 12.10 | **Supply cost increase** | Latest purchase unit cost vs 3-month avg | > 15% increase | StockMovement | 18 |
| 12.11 | **No-show rate spike** | Today's no-show rate vs 30-day avg | > 2× average | QueueEntry | Core |
| 12.12 | **Merchandise underperformer** | Items with zero sales in 30 days | 0 sales, stock > 0 | TransactionMerchandise + Merchandise | Core |

---

## 13. SMS Metrics (for AI context)

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 13.1 | **SMS Sent (period)** | `COUNT(SmsNotification) WHERE status = Sent` | SmsNotification | 15 (SMS) |
| 13.2 | **SMS Failed** | `COUNT(SmsNotification) WHERE status = Failed` | SmsNotification | 15 |
| 13.3 | **SMS Budget Used** | `TenantSubscription.smsUsedThisMonth` | TenantSubscription | 16 |
| 13.4 | **SMS Budget Remaining** | `Plan.smsPerMonth - smsUsedThisMonth` | Calculated | 16 |
| 13.5 | **SMS by Type** | `COUNT GROUP BY SmsType` | SmsNotification | 15 |

---

## 14. Subscription & Plan Metrics (for AI context)

| # | Metric | Formula / Source | Entity | Phase |
|---|---|---|---|---|
| 14.1 | **Current Plan** | `TenantSubscription.planTier` | TenantSubscription | 16 |
| 14.2 | **Subscription Status** | `TenantSubscription.status` | TenantSubscription | 16 |
| 14.3 | **Trial Days Remaining** | `TrialEndDate - NOW()` | TenantSubscription | 16 |
| 14.4 | **Branch Limit Used** | `Active Branches ÷ Plan.maxBranches` | Calculated | 16 |
| 14.5 | **Employee Limit Used** | `Active Employees ÷ Plan.maxEmployees` | Calculated | 16 |
| 14.6 | **Features Available** | `PlanCatalog.GetPlan(tier).Features` + overrides | PlanCatalog + TenantSubscription | 16 |

---

## Phase Dependency Map

This shows which phases must be complete for each AI capability to work:

| AI Capability | Required Phases | Minimum Data |
|---|---|---|
| Revenue summary & trends | Core (Phases 1-14) | 7+ days of transactions |
| Employee performance ranking | Core | 7+ days of transactions with employee assignments |
| Branch comparison | Core | 2+ branches with 7+ days each |
| Payment method breakdown | Core | Any completed transactions |
| Vehicle/service analytics | Core | 50+ transactions |
| **Profit & Loss report** | **Core + Phase 15 (Expenses) + Phase 18 (Inventory)** | 30+ days of expenses + supply usage configured |
| **Cost per wash** | **Phase 18 (Inventory)** | ServiceSupplyUsage configured for services |
| **Supply forecasting** | **Phase 18 (Inventory)** | 14+ days of stock movement data |
| **Cash variance analysis** | **Phase 15.9 (Shifts)** | 5+ closed shifts per cashier |
| **Customer retention/churn** | **Phase 15 (Loyalty)** | 30+ days of loyalty data, 50+ customers |
| **Cash advance impact** | **Phase 15 (Cash Advances)** | Active cash advances with deductions |
| **Anomaly detection** | **Core + Phase 15 + Phase 18** | 30+ days of comprehensive data |
| **Full daily brief** | **All of the above** | 30+ days of complete data across all modules |

---

## Metric Count Summary

| Category | Count |
|---|---|
| Revenue & Sales | 19 |
| Profitability | 17 |
| Employee Performance | 18 |
| Customer | 17 |
| Vehicle & Service | 12 |
| Inventory & Supply | 18 |
| Cash & Shift | 18 |
| Payroll & Compensation | 13 |
| Queue & Operations | 11 |
| Branch Comparison | 11 |
| Trend & Forecasting | 15 |
| Anomaly Detection Inputs | 12 |
| SMS | 5 |
| Subscription & Plan | 6 |
| **Total** | **192** |

**192 distinct metrics** feeding Negosyo AI. The core system (Phases 1-14) provides 108 of these. Phases 15, 15.9, and 18 provide the remaining 84 that unlock profitability analysis, supply forecasting, and anomaly detection.
