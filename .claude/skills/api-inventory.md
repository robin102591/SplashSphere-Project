---
name: api-inventory
description: Current API endpoint inventory — updated by agents after each task
---

# API Endpoints — Current Inventory

All prefixed with `/api/v1`. All require auth except webhooks and queue display.

## Auth, Onboarding & Webhooks
| Method | Route | Description |
|---|---|---|
| POST | /webhooks/clerk | Clerk webhook receiver (no auth) |
| GET | /auth/me | Current user profile + tenant info (includes hasPin) |
| POST | /auth/verify-pin | Verify current user's POS lock PIN |
| PATCH | /auth/users/{id}/pin | Set/reset a user's PIN (admin only) |
| GET | /onboarding/status | Check if user needs onboarding |
| POST | /onboarding | Create tenant + first branch + link user |

## Queue Management
| Method | Route | Description |
|---|---|---|
| POST | /queue | Add vehicle to queue |
| GET | /queue | Current queue for branch |
| GET | /queue/{id} | Queue entry details |
| PATCH | /queue/{id}/call | Call next customer (WAITING → CALLED) |
| PATCH | /queue/{id}/start | Start service — creates transaction, links queue |
| PATCH | /queue/{id}/cancel | Cancel queue entry |
| PATCH | /queue/{id}/no-show | Mark as no-show |
| PATCH | /queue/{id}/requeue | Re-queue NO_SHOW back to WAITING |
| GET | /queue/next | Next entry to be called |
| GET | /queue/display | Public (no auth) queue display data |
| GET | /queue/stats | Queue stats: waiting count, avg wait, served today |

## Branches
| Method | Route | Description |
|---|---|---|
| GET | /branches | List branches |
| GET/POST/PUT | /branches/{id} | CRUD |
| PATCH | /branches/{id}/status | Activate/deactivate |

## Services
| Method | Route | Description |
|---|---|---|
| GET/POST | /services | List/create |
| GET/PUT | /services/{id} | Get/update |
| PUT | /services/{id}/pricing | Bulk upsert pricing matrix |
| PUT | /services/{id}/commissions | Bulk upsert commission matrix |

## Packages, Service Categories, Vehicle Types, Sizes, Makes, Models
Standard CRUD endpoints.

## Customers
List (search), Get (with cars + history), Create, Update.

## Cars
List, Get, Create, Update, `GET /cars/lookup/{plateNumber}` (POS fast lookup).

## Employees
| Method | Route | Description |
|---|---|---|
| CRUD | /employees | Standard CRUD + status toggle |
| POST | /employees/{id}/invite | Send Clerk org invitation |
| GET | /employees/{id}/cash-advances | Advances for employee |
| GET | /employees/{id}/payroll-history | Paginated payroll entry history |

## Transactions (POS)
| Method | Route | Description |
|---|---|---|
| POST | /transactions | Create transaction (core POS operation) |
| GET | /transactions | List (filter by branch, date, status) |
| GET | /transactions/{id} | Full detail |
| PATCH | /transactions/{id}/status | Update status |
| PATCH | /transactions/{id}/discount-tip | Update discount and/or tip |
| POST | /transactions/{id}/payments | Add payment |
| GET | /transactions/daily-summary | Daily branch summary |

## Merchandise
Standard CRUD + stock adjustment + `GET /merchandise/low-stock`.

## Payroll
| Method | Route | Description |
|---|---|---|
| GET | /payroll/periods | List periods (filter by status, year, branch, paginated) |
| POST | /payroll/periods | Create a payroll period |
| GET | /payroll/periods/{id} | Period detail with all entries |
| POST | /payroll/periods/{id}/close | Close period (generates entries) |
| POST | /payroll/periods/{id}/process | Process/finalise period |
| POST | /payroll/periods/{id}/release | Release pay |
| GET | /payroll/periods/{id}/export/csv | Export period entries as CSV |
| PATCH | /payroll/entries/{id} | Update entry (notes only) |
| GET | /payroll/entries/{id}/detail | Entry detail with commission breakdown |
| POST | /payroll/entries/{id}/adjustments | Add adjustment to entry |
| PUT | /payroll/adjustments/{id} | Update adjustment |
| DELETE | /payroll/adjustments/{id} | Remove adjustment |
| POST | /payroll/entries/bulk-adjust | Bulk apply bonus/deduction |
| GET | /payroll/templates | List adjustment templates |
| POST | /payroll/templates | Create adjustment template |
| PUT | /payroll/templates/{id} | Update adjustment template |
| DELETE | /payroll/templates/{id} | Soft-delete adjustment template |
| GET | /payroll/entries/{id}/payslip | Payslip data (JSON) |
| GET | /payroll/entries/{id}/payslip/pdf | Download payslip as PDF |

## Cash Advances
| Method | Route | Description |
|---|---|---|
| GET | /cash-advances | List advances (filter by employee, status) |
| GET | /cash-advances/{id} | Advance detail |
| POST | /cash-advances | Create cash advance (Pending) |
| PATCH | /cash-advances/{id}/approve | Approve |
| PATCH | /cash-advances/{id}/disburse | Disburse |
| PATCH | /cash-advances/{id}/cancel | Cancel |

## Cashier Shifts
| Method | Route | Description |
|---|---|---|
| POST | /shifts/open | Open a new cashier shift |
| POST | /shifts/{id}/cash-movement | Record cash-in or cash-out |
| POST | /shifts/{id}/close | Close shift with denomination count |
| PATCH | /shifts/{id}/review | Manager approves or flags |
| PATCH | /shifts/{id}/reopen | Reopen a Pending closed shift |
| PATCH | /shifts/{id}/void | Void a shift with no completed transactions |
| GET | /shifts/current | Get current open shift for cashier |
| GET | /shifts | List shifts (paginated, filterable) |
| GET | /shifts/{id} | Shift detail |
| GET | /shifts/{id}/report | End-of-day report |
| GET | /shifts/variance-report | Variance report by cashier |

## Settings
| Method | Route | Description |
|---|---|---|
| GET | /settings/shift-config | Get shift settings |
| PUT | /settings/shift-config | Update shift settings |
| GET | /settings/payroll-config | Get payroll settings |
| PUT | /settings/payroll-config | Update payroll settings |

## Pricing Modifiers
Standard CRUD.

## Expenses (Gated: expense_tracking)
| Method | Route | Description |
|---|---|---|
| POST | /expenses | Record an expense |
| GET | /expenses | List expenses (filter by branch, category, date range) |
| PUT | /expenses/{id} | Update expense |
| DELETE | /expenses/{id} | Soft delete expense |
| GET | /expense-categories | List expense categories |
| POST | /expense-categories | Create expense category |

## Reports
| Method | Route | Description |
|---|---|---|
| GET | /reports/profit-loss | P&L report (gated: profit_loss_reports) |
| GET | /reports/revenue/export/csv | Export revenue report as CSV |
| GET | /reports/commissions/export/csv | Export commissions report as CSV |
| GET | /reports/service-popularity/export/csv | Export service popularity report as CSV |

## Dashboard & Reports
Summary, revenue, commissions, service popularity.

## Global Search
| Method | Route | Description |
|---|---|---|
| GET | /search?q=term&limit=5 | Global search across 6 entity types |

## Notifications
| Method | Route | Description |
|---|---|---|
| GET | /notifications | Paginated notification list |
| GET | /notifications/unread-count | Unread notification count |
| PATCH | /notifications/{id}/read | Mark one notification as read |
| POST | /notifications/mark-all-read | Mark all as read |

## Attendance
| Method | Route | Description |
|---|---|---|
| GET | /attendance/report | Attendance summary report |
| GET | /attendance/export/csv | Export attendance records as CSV |

## Billing & Subscription
| Method | Route | Description |
|---|---|---|
| GET | /billing/plan | Current tenant's plan, features, limits |
| POST | /billing/checkout | Create payment gateway checkout session |
| POST | /billing/change-plan | Request plan change |
| POST | /billing/cancel | Cancel subscription |
| GET | /billing/history | Paginated payment/billing history |
| GET | /billing/invoices/{id}/pdf | Download invoice as PDF |
| POST | /billing/invoices/{id}/pay | Create checkout to pay pending invoice |
| POST | /webhooks/payment | Payment gateway webhook (no auth) |

## Audit Logs
| Method | Route | Description |
|---|---|---|
| GET | /audit-logs | Paginated audit log list |
