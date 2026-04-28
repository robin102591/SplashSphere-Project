# API Endpoint Inventory

All prefixed with `/api/v1`. All require auth except webhooks and queue display.

## Auth, Onboarding & Webhooks

| Method | Route | Description |
|---|---|---|
| `POST` | `/webhooks/clerk` | Clerk webhook receiver (no auth) |
| `GET` | `/auth/me` | Current user profile + tenant info (includes `hasPin`) |
| `POST` | `/auth/verify-pin` | Verify current user's POS lock PIN |
| `PATCH` | `/auth/users/{id}/pin` | Set/reset a user's PIN (admin only) |
| `GET` | `/onboarding/status` | Check if user needs onboarding |
| `POST` | `/onboarding` | Create tenant + first branch + link user |

## Queue Management

| Method | Route | Description |
|---|---|---|
| `POST` | `/queue` | Add vehicle to queue |
| `GET` | `/queue` | Current queue for branch |
| `GET` | `/queue/{id}` | Queue entry details |
| `PATCH` | `/queue/{id}/call` | Call next customer (WAITING -> CALLED) |
| `PATCH` | `/queue/{id}/start` | Start service -- creates transaction, links queue |
| `PATCH` | `/queue/{id}/cancel` | Cancel queue entry |
| `PATCH` | `/queue/{id}/no-show` | Mark as no-show |
| `PATCH` | `/queue/{id}/requeue` | Re-queue NO_SHOW back to WAITING |
| `GET` | `/queue/next` | Next entry to be called |
| `GET` | `/queue/display` | **Public (no auth)** queue display data |
| `GET` | `/queue/stats` | Queue stats: waiting count, avg wait, served today |

## Branches

| Method | Route | Description |
|---|---|---|
| `GET` | `/branches` | List branches |
| `GET/POST/PUT` | `/branches/{id}` | CRUD |
| `PATCH` | `/branches/{id}/status` | Activate/deactivate |

## Services

| Method | Route | Description |
|---|---|---|
| `GET/POST` | `/services` | List/create |
| `GET/PUT` | `/services/{id}` | Get/update |
| `PUT` | `/services/{id}/pricing` | Bulk upsert pricing matrix |
| `PUT` | `/services/{id}/commissions` | Bulk upsert commission matrix |

## Packages, Service Categories, Vehicle Types, Sizes, Makes, Models -- Standard CRUD

## Customers -- List (search), Get (with cars + history), Create, Update

## Cars -- List, Get, Create, Update, `GET /cars/lookup/{plateNumber}` (POS fast lookup)

## Employees -- CRUD + attendance clock-in/out + commission history + invite

| Method | Route | Description |
|---|---|---|
| `POST` | `/employees/{id}/invite` | Send Clerk org invitation to employee's email |

## Transactions (POS)

| Method | Route | Description |
|---|---|---|
| `POST` | `/transactions` | Create transaction (core POS operation) |
| `GET` | `/transactions` | List (filter by branch, date, status) |
| `GET` | `/transactions/{id}` | Full detail |
| `PATCH` | `/transactions/{id}/status` | Update status |
| `PATCH` | `/transactions/{id}/discount-tip` | Update discount and/or tip on Pending/InProgress transaction |
| `POST` | `/transactions/{id}/payments` | Add payment |
| `GET` | `/transactions/{id}/receipt` | Receipt-formatted transaction data (JSON) |
| `GET` | `/transactions/{id}/receipt/pdf` | Download receipt as PDF (QuestPDF, 80mm thermal) |
| `GET` | `/transactions/daily-summary` | Daily branch summary |

## Merchandise -- CRUD + stock adjustment

| Method | Route | Description |
|---|---|---|
| `GET` | `/merchandise/low-stock` | List active items below low-stock threshold |

## Payroll

| Method | Route | Description |
|---|---|---|
| `GET` | `/payroll/periods` | List periods (filter by status, year, paginated) |
| `POST` | `/payroll/periods` | Manually create a 7-day payroll period |
| `GET` | `/payroll/periods/{id}` | Period detail with all entries |
| `POST` | `/payroll/periods/{id}/close` | Close period (generates entries) |
| `POST` | `/payroll/periods/{id}/process` | Process/finalise period (immutable) |
| `POST` | `/payroll/periods/{id}/release` | Release pay (Processed -> Released) |
| `GET` | `/payroll/periods/{id}/export/csv` | Export period entries as CSV download |
| `PATCH` | `/payroll/entries/{id}` | Update entry (bonuses, deductions, notes) |
| `GET` | `/payroll/entries/{id}/detail` | Entry detail with commission breakdown + attendance + adjustments |
| `POST` | `/payroll/entries/{id}/adjustments` | Add itemised adjustment to entry |
| `PUT` | `/payroll/adjustments/{id}` | Update adjustment amount/notes |
| `DELETE` | `/payroll/adjustments/{id}` | Remove adjustment |
| `POST` | `/payroll/entries/bulk-adjust` | Bulk apply bonus/deduction to selected entries |
| `GET` | `/payroll/templates` | List adjustment templates |
| `POST` | `/payroll/templates` | Create adjustment template |
| `PUT` | `/payroll/templates/{id}` | Update adjustment template |
| `DELETE` | `/payroll/templates/{id}` | Soft-delete (toggle active) adjustment template |
| `GET` | `/payroll/entries/{id}/payslip` | Payslip data (JSON) for print-friendly rendering |

## Cash Advances

| Method | Route | Description |
|---|---|---|
| `GET` | `/cash-advances` | List advances (filter by employee, status, paginated) |
| `GET` | `/cash-advances/{id}` | Advance detail |
| `POST` | `/cash-advances` | Create cash advance (Pending) |
| `PATCH` | `/cash-advances/{id}/approve` | Approve (Pending -> Approved) |
| `PATCH` | `/cash-advances/{id}/disburse` | Disburse (Approved -> Active) |
| `PATCH` | `/cash-advances/{id}/cancel` | Cancel (Pending/Approved only) |
| `GET` | `/employees/{id}/cash-advances` | Advances for specific employee |
| `GET` | `/employees/{id}/payroll-history` | Paginated payroll entry history for employee |

## Pricing Modifiers -- CRUD

## Cashier Shifts

| Method | Route | Description |
|---|---|---|
| `POST` | `/shifts/open` | Open a new cashier shift |
| `POST` | `/shifts/{id}/cash-movement` | Record cash-in or cash-out |
| `POST` | `/shifts/{id}/close` | Close shift with denomination count |
| `PATCH` | `/shifts/{id}/review` | Manager approves or flags a closed shift |
| `PATCH` | `/shifts/{id}/reopen` | Reopen a Pending closed shift |
| `PATCH` | `/shifts/{id}/void` | Void a shift with no completed transactions |
| `GET` | `/shifts/current` | Get current open shift for cashier |
| `GET` | `/shifts` | List shifts (paginated, filterable) |
| `GET` | `/shifts/{id}` | Shift detail |
| `GET` | `/shifts/{id}/report` | End-of-day report with top services & employees |
| `GET` | `/shifts/variance-report` | Variance report by cashier |
| `GET` | `/settings/shift-config` | Get shift settings |
| `PUT` | `/settings/shift-config` | Update shift settings |
| `GET` | `/settings/payroll-config` | Get payroll settings (cut-off start day) |
| `PUT` | `/settings/payroll-config` | Update payroll settings |

## Global Search

| Method | Route | Description |
|---|---|---|
| `GET` | `/search?q=term&limit=5` | Global search across customers, employees, transactions, vehicles, services, merchandise |

## Notifications

| Method | Route | Description |
|---|---|---|
| `GET` | `/notifications?page=1&pageSize=20&unreadOnly=false&category=1` | Paginated notification list (optional category filter) |
| `GET` | `/notifications/unread-count` | Unread notification count (for badge) |
| `PATCH` | `/notifications/{id}/read` | Mark one notification as read |
| `POST` | `/notifications/mark-all-read` | Mark all notifications as read |
| `GET` | `/notifications/preferences` | Get user notification channel preferences |
| `PUT` | `/notifications/preferences` | Update user notification channel preferences |

## Attendance Reports

| Method | Route | Description |
|---|---|---|
| `GET` | `/attendance/report` | Attendance summary report with per-employee stats (days, late, hours) |
| `GET` | `/attendance/export/csv` | Export attendance records as CSV download |

## Billing & Subscription

| Method | Route | Description |
|---|---|---|
| `GET` | `/billing/plan` | Get current tenant's plan, features, limits, trial status |
| `POST` | `/billing/checkout` | Create payment gateway checkout session for plan upgrade |
| `POST` | `/billing/change-plan` | Request plan change (upgrade/downgrade with limit validation) |
| `POST` | `/billing/cancel` | Cancel subscription (effective immediately) |
| `GET` | `/billing/history` | Paginated payment/billing history |
| `GET` | `/billing/invoices/{id}/pdf` | Download invoice as PDF |
| `POST` | `/billing/invoices/{id}/pay` | Create checkout session to pay a pending invoice |
| `POST` | `/webhooks/payment` | Payment gateway webhook (no auth) |

## Report Exports

| Method | Route | Description |
|---|---|---|
| `GET` | `/reports/revenue/export/csv` | Export revenue report as CSV |
| `GET` | `/reports/commissions/export/csv` | Export commissions report as CSV |
| `GET` | `/reports/service-popularity/export/csv` | Export service popularity report as CSV |
| `GET` | `/payroll/entries/{id}/payslip/pdf` | Download payslip as PDF (QuestPDF) |

## Audit Logs

| Method | Route | Description |
|---|---|---|
| `GET` | `/audit-logs` | Paginated audit log list (filter by entityType, entityId, userId, from, to) |

## Settings

| Method | Route | Description |
|---|---|---|
| `GET` | `/settings/company` | Get the current tenant's company profile (identity, contact, structured address, tax/registration, social, GCash) |
| `PUT` | `/settings/company` | Update the current tenant's company profile. Server re-derives the legacy single-string `Address` from the structured fields |
| `GET` | `/settings/receipt[?branchId={id}]` | Get receipt-design settings. Resolution: branch-specific row → tenant default → in-memory default. With `branchId`, returns the override row for that branch (or the tenant default falling through if no override exists) |
| `PUT` | `/settings/receipt[?branchId={id}]` | Upsert receipt-design settings. With no `branchId`, upserts the tenant default. With `branchId`, upserts a per-branch override (Enterprise only — handler returns 403 `Error.Forbidden` if the tenant lacks the `branch_receipt_overrides` feature) |
| `DELETE` | `/settings/receipt?branchId={id}` | Remove a per-branch override; the branch falls back to the tenant default. `branchId` is required — the tenant default cannot be deleted (always exists). Idempotent (deleting a missing override succeeds) |
| `POST` | `/settings/company/logo` | Upload a logo (multipart/form-data, field name `file`). Server resizes to 500/200/80px PNG variants and stores in Cloudflare R2. Returns `{logoUrl, logoThumbnailUrl, logoIconUrl}` with cache-busting `?v=` suffixes |
| `DELETE` | `/settings/company/logo` | Remove the current tenant's logo. Best-effort R2 delete (orphan blobs are not a correctness issue — next upload overwrites) |

## Expenses

| Method | Route | Description |
|---|---|---|
| `POST` | `/expenses` | Record an expense |
| `GET` | `/expenses` | List expenses (filter by branch, category, date range, paginated) |
| `PUT` | `/expenses/{id}` | Update expense |
| `DELETE` | `/expenses/{id}` | Soft delete expense |
| `GET` | `/expense-categories` | List expense categories |
| `POST` | `/expense-categories` | Create expense category |
| `GET` | `/reports/profit-loss` | P&L report (revenue, COGS, expenses by category, net profit) |

## Loyalty Program

| Method | Route | Description |
|---|---|---|
| `GET` | `/loyalty/settings` | Get loyalty program settings + tier configs |
| `PUT` | `/loyalty/settings` | Upsert loyalty program settings |
| `PUT` | `/loyalty/tiers` | Upsert tier configurations |
| `GET` | `/loyalty/rewards` | List rewards (paginated, optional activeOnly filter) |
| `POST` | `/loyalty/rewards` | Create reward |
| `PUT` | `/loyalty/rewards/{id}` | Update reward |
| `PATCH` | `/loyalty/rewards/{id}/status` | Toggle reward active/inactive |
| `GET` | `/loyalty/dashboard` | Loyalty dashboard (members, points, tiers, top customers) |
| `POST` | `/loyalty/members` | Enroll customer in loyalty program |
| `GET` | `/loyalty/members/by-customer/{customerId}` | Get membership card by customer |
| `GET` | `/loyalty/members/by-card/{cardNumber}` | Get membership card by card number |
| `GET` | `/loyalty/members/{membershipCardId}/points` | Point history (paginated) |
| `POST` | `/loyalty/members/{membershipCardId}/redeem` | Redeem points for reward |
| `POST` | `/loyalty/members/{membershipCardId}/adjust` | Admin manual point adjustment |
| `GET` | `/loyalty/members/by-customer/{customerId}/summary` | Lightweight loyalty summary for POS |

## Franchise

| Method | Route | Description |
|---|---|---|
| `GET` | `/franchise/settings` | Get franchise network settings |
| `PUT` | `/franchise/settings` | Update franchise settings |
| `GET` | `/franchise/franchisees` | List all franchisees (paginated) |
| `GET` | `/franchise/franchisees/{id}` | Franchisee detail with agreement + royalties |
| `POST` | `/franchise/franchisees/{id}/suspend` | Suspend a franchisee |
| `POST` | `/franchise/franchisees/{id}/reactivate` | Reactivate a suspended franchisee |
| `POST` | `/franchise/franchisees/{id}/push-templates` | Push service templates to franchisee |
| `POST` | `/franchise/agreements` | Create franchise agreement |
| `GET` | `/franchise/royalties` | List royalty periods (paginated, filterable) |
| `POST` | `/franchise/royalties/calculate` | Calculate royalties for a period |
| `PATCH` | `/franchise/royalties/{id}/paid` | Mark royalty as paid |
| `GET` | `/franchise/network-summary` | Network KPIs |
| `GET` | `/franchise/compliance` | Compliance report per franchisee |
| `GET` | `/franchise/templates` | List service templates |
| `POST` | `/franchise/templates` | Create service template |
| `PUT` | `/franchise/templates/{id}` | Update service template |
| `POST` | `/franchise/invite` | Send franchise invitation |
| `GET` | `/franchise/my-agreement` | Franchisee: get my agreement |
| `GET` | `/franchise/my-royalties` | Franchisee: my royalty statements |
| `GET` | `/franchise/benchmarks` | Franchisee: network benchmarks |
| `GET` | `/franchise/invitations/{token}/validate` | **Public** -- validate invitation token |
| `POST` | `/franchise/invitations/{token}/accept` | Accept invitation and create franchisee |

## Data Import

| Method | Route | Description |
|---|---|---|
| `GET` | `/import/templates/{type}` | Download CSV template for import type (Customers/Vehicles/Employees/Services) |
| `POST` | `/import/detect` | Upload file + detect columns, return preview rows (multipart) |
| `POST` | `/import/validate` | Validate file with column mappings (multipart) |
| `POST` | `/import/execute` | Execute import after validation (multipart, transactional) |

## Dashboard & Reports -- Summary, revenue, commissions, service popularity

## Analytics

| Method | Route | Description |
|---|---|---|
| `GET` | `/reports/customer-analytics` | Customer analytics (retention, visit frequency, top customers, trend) |
| `GET` | `/reports/peak-hours` | Peak hours heatmap (7x24 grid, transaction count + revenue per slot) |
| `GET` | `/reports/employee-performance` | Employee performance rankings (revenue, services, commissions, attendance) |

## Supplies

| Method | Route | Description |
|---|---|---|
| `GET` | `/supplies` | List supply items (filter by category, branch, stock status) |
| `POST` | `/supplies` | Create supply item |
| `GET` | `/supplies/{id}` | Supply item detail with movement history |
| `PUT` | `/supplies/{id}` | Update supply item |
| `DELETE` | `/supplies/{id}` | Soft delete supply item |
| `GET` | `/supplies/categories` | List supply categories |
| `POST` | `/supplies/categories` | Create supply category |

## Stock Movements

| Method | Route | Description |
|---|---|---|
| `POST` | `/stock-movements` | Record a stock movement |
| `GET` | `/stock-movements` | List movements (filter by item, type, branch, date) |
| `POST` | `/stock-movements/bulk-usage` | Record daily usage for multiple supplies |

## Service Supply Usage

| Method | Route | Description |
|---|---|---|
| `GET` | `/services/{id}/supply-usage` | Get supply usage matrix for a service |
| `PUT` | `/services/{id}/supply-usage` | Set/update supply usage matrix |
| `GET` | `/services/{id}/cost-breakdown` | Cost-per-wash breakdown by vehicle size |

## Purchase Orders

| Method | Route | Description |
|---|---|---|
| `GET` | `/purchase-orders` | List purchase orders (filter by status, supplier, branch) |
| `POST` | `/purchase-orders` | Create purchase order |
| `GET` | `/purchase-orders/{id}` | PO detail with lines |
| `PUT` | `/purchase-orders/{id}` | Update PO (Draft only) |
| `PATCH` | `/purchase-orders/{id}/send` | Mark PO as Sent |
| `POST` | `/purchase-orders/{id}/receive` | Receive items (partial or full) |
| `PATCH` | `/purchase-orders/{id}/cancel` | Cancel PO |

## Suppliers

| Method | Route | Description |
|---|---|---|
| `GET` | `/suppliers` | List suppliers |
| `POST` | `/suppliers` | Create supplier |
| `PUT` | `/suppliers/{id}` | Update supplier |

## Equipment

| Method | Route | Description |
|---|---|---|
| `GET` | `/equipment` | List equipment (filter by branch, status) |
| `POST` | `/equipment` | Register equipment |
| `GET` | `/equipment/{id}` | Equipment detail with maintenance history |
| `PUT` | `/equipment/{id}` | Update equipment |
| `POST` | `/equipment/{id}/maintenance` | Log maintenance activity |
| `PATCH` | `/equipment/{id}/status` | Update equipment status |

## Inventory Reports

| Method | Route | Description |
|---|---|---|
| `GET` | `/reports/inventory-summary` | Stock levels, value, low stock alerts |
| `GET` | `/reports/supply-usage` | Supply consumption over time |
| `GET` | `/reports/equipment-maintenance` | Upcoming and overdue maintenance |
| `GET` | `/reports/purchase-history` | Spending by supplier, category, period |

## Booking Settings

Per-branch online-booking configuration. Feature-gated behind `online_booking`.

| Method | Route | Description |
|---|---|---|
| `GET` | `/booking-settings?branchId={id}` | Get booking settings for a branch (falls back to tenant defaults) |
| `PUT` | `/booking-settings?branchId={id}` | Upsert booking settings (hours, slot interval, capacity, lead/grace, toggles) |

## Bookings (Admin / POS)

Tenant-scoped booking management — consumed by the admin dashboard and the POS for cashier check-in + classification. Feature-gated behind `online_booking`.

| Method | Route | Description |
|---|---|---|
| `GET` | `/bookings?fromDate=&toDate=&branchId=&status=` | List bookings in a date window |
| `GET` | `/bookings/{id}` | Booking detail with customer, vehicle, services, queue/transaction links |
| `PATCH` | `/bookings/{id}/check-in` | Cashier check-in: flip Confirmed → Arrived and allocate a queue entry when missing |
| `POST` | `/bookings/{id}/classify-vehicle` | Classify vehicle (VehicleType + Size) and lock exact service prices |

---

# Customer Connect

The Customer Connect app authenticates end-customers (not tenant staff) via phone OTP. All routes below live under `/api/v1/connect/*` and use the **`ConnectJwt`** auth scheme instead of the default Clerk Bearer scheme. `ConnectUser`, `ConnectVehicle`, `GlobalMake`, and `GlobalModel` are globally-scoped entities (not tenant-partitioned); handlers use `IgnoreQueryFilters()` so a single customer identity works across every tenant they've joined.

## Connect.Auth

Anonymous endpoints — the caller is establishing identity.

| Method | Route | Description |
|---|---|---|
| `POST` | `/connect/auth/otp/send` | Send a one-time code to a Philippine mobile number |
| `POST` | `/connect/auth/otp/verify` | Verify the OTP and receive access + refresh tokens |
| `POST` | `/connect/auth/refresh` | Rotate refresh token and receive a new access token |
| `POST` | `/connect/auth/sign-out` | Revoke a refresh token |

## Connect.Profile

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/profile` | Read the Connect user's profile with vehicles |
| `PATCH` | `/connect/profile` | Update display name / email / avatar |
| `POST` | `/connect/profile/vehicles` | Register a vehicle on the profile |
| `PATCH` | `/connect/profile/vehicles/{id}` | Edit a vehicle |
| `DELETE` | `/connect/profile/vehicles/{id}` | Remove a vehicle |

## Connect.Catalogue

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/catalogue/makes` | List global vehicle makes |
| `GET` | `/connect/catalogue/makes/{makeId}/models` | List models under a make |

## Connect.Discovery

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes?search=&lat=&lng=&take=` | Search the public car-wash directory |
| `GET` | `/connect/carwashes/{tenantId}` | Public car-wash detail (branches + services) |
| `POST` | `/connect/carwashes/{tenantId}/join` | Link the authenticated user to a car wash |
| `GET` | `/connect/my-carwashes` | List all car washes the user has joined |

## Connect.Services

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes/{tenantId}/services?vehicleId=` | Services with exact or estimated pricing for the selected vehicle |

## Connect.Booking

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes/{tenantId}/slots?branchId=&date=YYYY-MM-DD` | Available booking slots |
| `POST` | `/connect/bookings` | Create a booking |
| `GET` | `/connect/bookings?includePast=` | List the caller's bookings |
| `GET` | `/connect/bookings/{id}` | Booking detail + queue status |
| `PATCH` | `/connect/bookings/{id}/cancel` | Cancel a booking |
| `PATCH` | `/connect/bookings/{id}/arrived` | Self check-in — mark booking as arrived |

## Connect.Loyalty

Per-tenant loyalty endpoints — feature-gated (handler returns degraded DTO when tenant plan lacks loyalty).

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes/{tenantId}/loyalty` | Caller's loyalty membership at a car wash |
| `GET` | `/connect/carwashes/{tenantId}/rewards` | Rewards offered, with affordability flag |
| `POST` | `/connect/carwashes/{tenantId}/rewards/redeem` | Redeem points for a reward |
| `GET` | `/connect/carwashes/{tenantId}/points-history?take=` | Point-movement ledger |

## Connect.Referral

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes/{tenantId}/referral-code` | Get (and lazily issue) the caller's referral code |
| `GET` | `/connect/carwashes/{tenantId}/referrals` | List referrals the caller has made |
| `POST` | `/connect/carwashes/{tenantId}/apply-referral` | Apply a referral code (rewards deferred until first wash) |

## Connect.Queue

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/queue/active` | Caller's currently active queue entry across any tenant, or `204` |

## Connect.History

| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/history?take=` | Cross-tenant completed-transaction history |
