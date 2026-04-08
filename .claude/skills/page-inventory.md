---
name: page-inventory
description: Current frontend page inventory — updated by agents after each task
---

# Frontend Pages — Current Inventory

## Auth Pages (both apps)
| Route | Page |
|---|---|
| /sign-in | Custom sign-in (email/password + social via Clerk headless hooks) |
| /sign-up | Custom sign-up + email verification (admin only, NOT on POS) |
| /sso-callback | OAuth redirect handler |
| /onboarding | Tenant onboarding wizard (admin only) |

## Admin Dashboard
| Route | Page |
|---|---|
| /dashboard | Dashboard home — KPI cards, charts |
| /dashboard/branches | Branch list |
| /dashboard/branches/[id] | Branch detail |
| /dashboard/services | Service list |
| /dashboard/services/new | Create service |
| /dashboard/services/[id] | Service detail (pricing + commission matrices) |
| /dashboard/packages | Package list |
| /dashboard/packages/new | Create package |
| /dashboard/packages/[id] | Package detail |
| /dashboard/merchandise | Merchandise list + stock management |
| /dashboard/pricing-modifiers | Pricing rules (gated) |
| /dashboard/employees | Employee list |
| /dashboard/employees/[id] | Employee detail (payroll, security, invitations) |
| /dashboard/attendance | Attendance report with stats + CSV export |
| /dashboard/customers | Customer list |
| /dashboard/customers/[id] | Customer detail (vehicles + transaction history) |
| /dashboard/vehicles | Vehicle list |
| /dashboard/transactions | Transaction list |
| /dashboard/transactions/[id] | Transaction detail |
| /dashboard/payroll | Payroll period list |
| /dashboard/payroll/[id] | Payroll period detail (entries, adjustments, bulk) |
| /dashboard/cash-advances | Cash advance list + actions (gated) |
| /dashboard/expenses | Expense list + record/edit/delete + category management (gated) |
| /dashboard/shifts | Shift list (gated) |
| /dashboard/shifts/[id] | Shift detail + manager review (gated) |
| /dashboard/reports | Reports hub (revenue, commissions, service popularity) |
| /dashboard/reports/shift-variance | Shift variance analysis (gated) |
| /dashboard/reports/profit-loss | P&L dashboard (gated) |
| /dashboard/subscription | Plan details + usage meters + plan comparison |
| /dashboard/billing | Payment history + invoice download + cancel |
| /dashboard/settings | Settings (vehicle types, sizes, makes, categories, shifts, payroll) |
| /dashboard/audit-logs | Audit log viewer |
| /dashboard/loyalty | Loyalty program — dashboard, rewards CRUD, settings + tier config |
| /dashboard/franchise | Franchise network overview — KPI stat cards + franchisee table |
| /dashboard/franchise/franchisees | Franchisees list — paginated, invite button (stub) |
| /dashboard/franchise/franchisees/[id] | Franchisee detail — business info, agreement, actions, recent royalties |
| /dashboard/franchise/royalties | Royalties — paginated list with filters, mark-paid action |
| /dashboard/franchise/my-agreement | Franchisee: my agreement details (read-only) |
| /dashboard/franchise/my-royalties | Franchisee: paginated royalty periods list |
| /dashboard/franchise/benchmarks | Franchisee: network performance benchmarks |
| /dashboard/franchise/templates | Service templates CRUD + push-to-franchisees |
| /dashboard/franchise/compliance | Compliance report — color-coded matrix per franchisee |
| /dashboard/franchise/settings | Franchise settings form — royalties, standards, branding |

## POS App
| Route | Page |
|---|---|
| / | POS Home — quick actions |
| /queue | Queue Board — Kanban (WAITING / CALLED / IN_SERVICE) |
| /queue/add | Add to Queue — plate lookup, priority, preferred services |
| /transactions/new | New Transaction (supports direct OR from-queue via ?queueEntryId) |
| /transactions/[id] | Transaction detail + receipt |
| /history | Today's transactions |
| /customers/lookup | Plate/customer search |
| /attendance | Clock in/out |
| /queue-display | PUBLIC (no auth) full-screen queue for wall TV |
| /shift/open | Open Shift — opening cash fund entry |
| /shift | Active Shift Panel — stats, cash movements, payment breakdown |
| /shift/cash-movement | Cash Movement Form — Cash In / Cash Out |
| /shift/close | Close Shift — 3-step wizard |
| /shift/report | Shift Report — printable EOD report |
