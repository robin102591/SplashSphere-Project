# SplashSphere — Super Admin (SaaS Provider Dashboard)

> **What this is:** The platform-level admin dashboard for LezanobTech — the company behind SplashSphere. This is where YOU (Rob) manage all tenants, subscriptions, billing, feature toggles, franchise networks, and platform health. Your tenants never see this app.
> **Tech:** Blazor Server (.NET 9) — already scaffolded as `SplashSphere.SaasAdmin`.
> **Database:** Separate `SaasDbContext` that reads from both the SaaS admin tables AND the main SplashSphere database (read-only cross-queries for analytics).
> **Access:** Only LezanobTech staff. Protected by its own Clerk instance or simple admin auth.

---

## Why Separate From the Main App

The super admin is NOT a page inside the tenant admin dashboard. It's a completely separate application because:

1. **Different audience** — you and your team, not car wash owners
2. **Different data scope** — sees ALL tenants, not one tenant's data
3. **Different security model** — platform-level access, not tenant-scoped
4. **Different database** — SaaS management tables (subscriptions, invoices, feature overrides) plus read-only access to aggregate tenant data
5. **Different deployment** — runs on a private subdomain (`ops.splashsphere.ph`), not publicly accessible

---

## Pages & Features

### Page 1: Dashboard (`/`)

The nerve center. Shows LezanobTech's business health at a glance.

**KPI Cards Row:**

```
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ 💰 MRR            │ │ 🏢 Total Tenants  │ │ 📊 Active         │ │ ⏰ Trial          │
│ ₱187,461          │ │ 68                │ │ 52                │ │ 11               │
│ ▲ 12% vs last mo  │ │ ▲ 5 new this mo   │ │ 76% of total      │ │ 3 expiring soon  │
└──────────────────┘ └──────────────────┘ └──────────────────┘ └──────────────────┘

┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ 🔴 Suspended      │ │ 💸 Overdue        │ │ 📈 Collection     │ │ 🏪 Total Branches │
│ 3                 │ │ ₱14,497          │ │ 91%               │ │ 142              │
│ 2 past due >7d    │ │ 5 invoices        │ │ ▲ 3% vs last mo   │ │ across all tenants│
└──────────────────┘ └──────────────────┘ └──────────────────┘ └──────────────────┘
```

**MRR Calculation:**
```
MRR = SUM of all active subscriptions' monthly plan price
    = (Starter tenants × ₱1,499) + (Growth tenants × ₱2,999) + (Enterprise tenants × ₱4,999)
    + franchise network contributions
```

**Charts Row:**
- Left (60%): **MRR Trend** — line chart, last 12 months. Shows MRR growth over time.
- Right (40%): **Plan Distribution** — donut chart. Starter/Growth/Enterprise/Trial breakdown.

**Recent Activity Feed:**
```
🟢 AquaShine Car Wash — Payment received ₱2,999 (Growth plan)          2 min ago
🔵 CleanDrive Auto Spa — Upgraded from Starter to Growth                15 min ago
🟡 WashPro Express — Trial expires in 2 days                            1 hour ago
🔴 SuperWash Davao — Payment failed, entering grace period              3 hours ago
🟢 SpeedyWash PH — New franchise network registered (Enterprise)        Yesterday
🔵 AquaShine Cebu — New branch added (branch 3 of 3 on Growth plan)    Yesterday
```

**Tenant Leaderboard (by transaction volume this month):**
```
#1  AquaShine Car Wash      3,420 txns  ████████████████████ 100%
#2  SpeedyWash Makati       2,810 txns  ████████████████░░░░  82%
#3  CleanDrive Auto Spa     2,150 txns  ████████████░░░░░░░░  63%
#4  WashPro Express         1,890 txns  ██████████░░░░░░░░░░  55%
#5  SuperWash Cebu          1,540 txns  █████████░░░░░░░░░░░  45%
```

---

### Page 2: Tenant Management (`/tenants`)

**Full CRUD for all tenants on the platform.**

**Filter Tabs:** All | Active | Trial | Past Due | Suspended | Cancelled

**Data Table:**

| Business Name | Owner | Plan | Status | Branches | Employees | MRR | Last Payment | Actions |
|---|---|---|---|---|---|---|---|---|
| AquaShine Car Wash | Juan Reyes | Growth | Active | 3 | 12 | ₱2,999 | Mar 21 | ••• |
| CleanDrive Auto Spa | Maria Santos | Starter | Active | 1 | 4 | ₱1,499 | Mar 18 | ••• |
| SpeedyWash PH | Carlos Rivera | Enterprise | Active | — | — | ₱4,999 | Mar 25 | ••• |
| WashPro Express | Ana Reyes | Growth | Trial | 1 | 5 | ₱0 | — | ••• |
| SuperWash Davao | Pedro Garcia | Growth | Past Due | 2 | 8 | ₱2,999 | Feb 28 | ••• |

**Actions menu (•••):**
- View Details → `/tenants/{id}`
- Change Plan
- Suspend / Reactivate
- Extend Trial
- Override Features
- Impersonate (log into their admin as a support agent)
- Send Email
- View Billing History

---

### Page 3: Tenant Detail (`/tenants/{id}`)

**Everything about one tenant, from LezanobTech's perspective.**

**Header:**
```
← Back to Tenants
AquaShine Car Wash                                    [Growth Plan]  [Active]
Owner: Juan Reyes • juan@aquashine.ph • +639171234567
Registered: Jan 15, 2026 • Makati City
```

**Tabs:**

#### Tab 1: Overview
- Subscription info: plan, status, billing dates, payment method
- Usage summary: branches (3 of 3 limit), employees (12 of 15), SMS (23 of 50)
- Last 30-day metrics: transaction count, total revenue, avg transaction value
- Feature overrides (if any)

#### Tab 2: Subscription & Billing
- Current plan with upgrade/downgrade buttons
- Billing history table (all invoices with status)
- Payment method on file
- Manual actions: "Mark Invoice as Paid", "Generate Credit Note", "Extend Trial"
- Plan change log (history of upgrades/downgrades)

#### Tab 3: Feature Overrides
- Full feature toggle matrix:
  ```
  Feature                  Plan Default    Override    Status
  POS                      ✓ (core)        —           Enabled
  Queue Management         ✓ (Growth)      —           Enabled
  Customer Loyalty         ✓ (Growth)      —           Enabled
  Cash Advance Tracking    ✓ (Growth)      —           Enabled
  SMS Notifications        ✓ (Growth)      —           Enabled (23/50 used)
  API Access               ✗ (Enterprise)  ✓ Granted   Enabled (override)
  Custom Integrations      ✗ (Enterprise)  —           Disabled
  ```
- Toggle switches for overrides
- Override notes field (e.g., "Granted API access for 3 months as part of partnership deal")

#### Tab 4: Branches & Usage
- List of all branches with metrics
- Employee count per branch
- Transaction volume per branch (chart)
- Storage/data usage

#### Tab 5: Franchise (if applicable)
- If Franchisor: list of franchisees, network MRR
- If Franchisee: parent franchisor, territory, royalty status
- If Independent: "Not part of a franchise network"

#### Tab 6: Activity Log
- Chronological log of all platform actions for this tenant:
  - Sign-ups, plan changes, feature overrides, payments, suspensions
  - Populated from the audit trail

#### Tab 7: Support Notes
- Internal notes field for LezanobTech team
- "This tenant requested custom pricing for Q3"
- "Demo call scheduled for April 5"
- Not visible to the tenant

---

### Page 4: Billing & Revenue (`/billing`)

**Platform-wide billing management.**

**KPI Cards:**
```
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ MRR              │ │ Collected (MTD)  │ │ Outstanding      │ │ Overdue          │
│ ₱187,461         │ │ ₱156,880        │ │ ₱30,581          │ │ ₱14,497          │
│                  │ │ 84% collected    │ │ 5 invoices       │ │ 3 invoices >7d   │
└─────────────────┘ └─────────────────┘ └─────────────────┘ └─────────────────┘
```

**Revenue by Plan (this month):**
```
Starter:     18 tenants × ₱1,499  = ₱26,982  (14%)
Growth:      28 tenants × ₱2,999  = ₱83,972  (45%)
Enterprise:   6 tenants × ₱4,999  = ₱29,994  (16%)
Trial:       11 tenants × ₱0      = ₱0
Franchise:   5 networks × ₱4,999  = ₱24,995 + 20 franchisees × ₱2,999 = ₱59,980 (25%)
```

**Invoice Table:**

| Invoice # | Tenant | Period | Amount | Status | Due Date | Actions |
|---|---|---|---|---|---|---|
| INV-2026-0312 | AquaShine | Mar 2026 | ₱2,999 | Paid | Mar 21 | View |
| INV-2026-0311 | CleanDrive | Mar 2026 | ₱1,499 | Paid | Mar 18 | View |
| INV-2026-0298 | SuperWash | Mar 2026 | ₱2,999 | Overdue | Mar 5 | Send Reminder / Mark Paid |
| INV-2026-0285 | WashPro | Mar 2026 | ₱2,999 | Pending | Apr 1 | — |

**Actions:**
- "Generate Monthly Invoices" button — batch-creates invoices for all active subscriptions
- "Send Reminders" — batch-sends payment reminders to all overdue tenants
- "Export to CSV" — download invoice data for accounting
- Per-invoice: View, Send, Mark as Paid, Void

**MRR Trend Chart:** Line chart showing MRR over the last 12 months with growth annotations.

**Churn Analysis:**
```
This month:
  New tenants: +5 (₱12,496 MRR added)
  Upgrades: +2 (₱3,000 MRR added)
  Downgrades: -1 (-₱1,500 MRR lost)
  Cancelled: -1 (-₱2,999 MRR lost)
  Net MRR change: +₱10,997 (+6.2%)
```

---

### Page 5: Feature Control (`/features`)

**Platform-wide feature flag management.**

**Two views:**

#### View 1: Feature Matrix (default)
Shows which features each tenant has, with the ability to override:

```
                         AquaShine  CleanDrive  SpeedyWash  WashPro  SuperWash
                         (Growth)   (Starter)   (Enterpr.)  (Trial)  (Growth)
POS                      ✓          ✓           ✓           ✓        ✓
Commission Tracking      ✓          ✓           ✓           ✓        ✓
Weekly Payroll           ✓          ✓           ✓           ✓        ✓
Queue Management         ✓          ✗           ✓           ✓        ✓
Customer Loyalty         ✓          ✗           ✓           ✓        ✓
Cash Advance Tracking    ✓          ✗           ✓           ✓        ✓
Expense Tracking         ✓          ✗           ✓           ✓        ✓
Shift Management         ✓          ✗           ✓           ✓        ✓
SMS Notifications        ✓ (23/50)  ✗           ✓ (45/200)  ✓ (3/10) ✓ (12/50)
API Access               ✓*         ✗           ✓           ✗        ✗
Custom Integrations      ✗          ✗           ✓           ✗        ✗

* = override (granted outside normal plan)
```

Click on any cell to toggle an override.

#### View 2: Per-Tenant Feature Config
Select a tenant from a dropdown → see their full feature config → toggle individual features on/off as overrides.

**Feature categories:**
- Core (always on): POS, Commission, Payroll, Basic Reports, Customer/Vehicle/Employee Management
- Operations (Growth+): Queue, Cash Advance, Expense, Shift, Pricing Modifiers
- Analytics (Growth+): P&L Reports, Cost-per-Wash, Supply Usage
- Growth (Growth+): Customer Loyalty, SMS
- Enterprise: API Access, Custom Integrations
- Inventory: Basic Supplies (all plans), POs + Equipment + Cost Analytics (Growth+)

---

### Page 6: Franchise Networks (`/franchise`)

**Manage all franchise networks on the platform.**

**Network Table:**

| Franchisor | Plan | Franchisees | Network MRR | Network Revenue | Royalty Due | Status |
|---|---|---|---|---|---|---|
| SpeedyWash PH | Enterprise | 12 active | ₱40,987 | ₱2.8M | ₱224,000 | Healthy |
| CleanFuel Wash | Enterprise | 8 active | ₱28,991 | ₱1.6M | ₱128,000 | Healthy |
| AquaChain | Enterprise | 3 active | ₱13,996 | ₱580K | ₱46,400 | 1 past due |

**Network Detail (`/franchise/{id}`):**
- Franchisor info + subscription status
- Franchise settings (royalty rates, standardization flags)
- Franchisee list with individual subscription status
- Network MRR breakdown (franchisor + each franchisee)
- Royalty periods: calculated vs paid
- Compliance overview

**Note:** LezanobTech doesn't collect royalties — those flow franchisor → franchisee. But you need visibility to ensure the franchise network is healthy (franchisees paying their SplashSphere subscriptions).

---

### Page 7: Platform Analytics (`/analytics`)

**Cross-tenant aggregate analytics. No individual tenant data exposed — only aggregates.**

**Usage Metrics:**
```
Total Transactions (this month):     42,380 across all tenants
Total Revenue Processed:             ₱12.4M across all tenants
Avg Transactions per Tenant:         623
Avg Revenue per Tenant:              ₱182,353
Total Employees Managed:             847
Total Branches:                      142
Total Vehicles Registered:           28,450
Total Customers:                     15,820
```

**Growth Charts:**
- Tenant growth: new sign-ups per month (line chart, last 12 months)
- MRR growth: monthly MRR trend with net change annotations
- Plan distribution over time: stacked area chart showing Starter/Growth/Enterprise mix

**Engagement Metrics:**
- Daily active tenants (how many logged in today)
- Avg transactions per tenant per day
- Feature adoption: % of Growth tenants using Queue, Loyalty, Expenses, etc.
- Trial conversion rate: % of trials that convert to paid
- Churn rate: % of paid tenants that cancelled this month

**Platform Health:**
- API response time (p50, p95, p99) — from health check data
- Error rate — from Serilog aggregation
- Hangfire job success rate
- SignalR active connections

---

### Page 8: Support & Communications (`/support`)

**Manage tenant support interactions.**

**Support Tickets (future — placeholder for now):**
- Ticket list: tenant, subject, priority, status, assigned to
- Ticket detail: conversation thread, internal notes
- For now: just a table with manually entered notes

**Announcements:**
- Create platform-wide announcements ("Scheduled maintenance on April 5, 10 PM - 12 AM")
- Target: all tenants, specific plans, specific tenants
- Delivery: in-app banner (shown in tenant's admin dashboard) + optional email

**Bulk Email:**
- Send email to all tenants, or filtered by plan/status
- Use cases: feature launch announcement, pricing change notice, holiday greetings
- Template with merge fields: `{tenantName}`, `{ownerName}`, `{planName}`

---

### Page 9: Settings (`/settings`)

**Platform-level configuration for LezanobTech.**

**Company Settings:**
- Company name: LezanobTech
- Product name: SplashSphere
- Support email: support@splashsphere.ph
- Support phone: +63 XXX XXX XXXX
- Website: splashsphere.ph

**Plan Pricing (display only — changes require code deploy):**
```
Trial:       ₱0/month (14 days, Growth features)
Starter:     ₱1,499/month
Growth:      ₱2,999/month
Enterprise:  ₱4,999/month
```

**Billing Settings:**
- PayMongo API keys (masked display, update form)
- Invoice number prefix: `INV`
- Invoice due days: 7 days after billing date
- Grace period before suspension: 7 days
- Auto-suspend on non-payment: enabled/disabled

**SMS Gateway Settings:**
- Semaphore API key (masked)
- Sender name
- Platform-level SMS (for announcements, not tenant SMS)

**Shift Variance Thresholds (platform defaults):**
- Auto-approve threshold: ₱50
- Flag threshold: ₱200
- (Tenants can override these in their own settings)

**Admin Users:**
- List of LezanobTech staff with super admin access
- Add/remove admin users
- Activity log per admin user

---

### Page 10: Impersonation (`/impersonate/{tenantId}`)

**Critical support tool.** When a tenant reports a bug or needs help, you need to see what they see.

**How it works:**
1. From `/tenants/{id}`, click "Impersonate"
2. System creates a temporary read-only session in the tenant's admin dashboard
3. A prominent banner shows: "🔴 IMPERSONATING: AquaShine Car Wash — Read Only"
4. You can browse their pages, see their data, but cannot modify anything
5. All impersonation sessions are logged (who, when, which tenant, duration)
6. Click "End Impersonation" to return to super admin

**Impersonation is read-only by default.** If you need to fix something, use the super admin's tenant detail page to make changes (plan, features, billing).

**Audit trail:** Every impersonation is logged with:
- Admin user who impersonated
- Tenant impersonated
- Start/end timestamps
- Pages viewed during session

---

## Domain Models (SaaS Admin Context)

These live in the `SaasDbContext`, separate from the main `SplashSphereDbContext`.

```csharp
// Already partially built — enhance with these additions:

public sealed class PlatformAuditLog
{
    public string Id { get; set; } = string.Empty;
    public string AdminUserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;     // "suspend_tenant", "override_feature", "impersonate"
    public string? TargetTenantId { get; set; }
    public string? Details { get; set; }                    // JSON with action-specific data
    public DateTime PerformedAt { get; set; }
}

public sealed class PlatformAnnouncement
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public AnnouncementTarget Target { get; set; }          // All, ByPlan, ByTenant
    public string? TargetFilter { get; set; }               // JSON: { plans: ["Growth"] } or { tenantIds: [...] }
    public bool IsActive { get; set; } = true;
    public DateTime PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string CreatedById { get; set; } = string.Empty;
}

public sealed class SupportNote
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string AdminUserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class ImpersonationSession
{
    public string Id { get; set; } = string.Empty;
    public string AdminUserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? PagesViewed { get; set; }                // JSON array of routes visited
}

public enum AnnouncementTarget { All, ByPlan, ByTenant }
```

---

## Data Access Pattern

The super admin reads from **two databases**:

```
SaasDbContext (read-write):
  - SaasTenant, Subscription, BillingRecord, Invoice
  - Feature, TenantFeature (overrides)
  - PlatformAuditLog, PlatformAnnouncement, SupportNote
  - ImpersonationSession
  - Admin users

SplashSphereDbContext (READ-ONLY aggregate queries):
  - COUNT transactions per tenant (for analytics)
  - COUNT employees per tenant
  - COUNT branches per tenant
  - SUM revenue per tenant (for leaderboard)
  - No access to individual transaction details, customer PII, or employee salaries
```

**Critical rule:** The super admin NEVER sees individual customer names, employee salaries, or transaction details. Only aggregates (counts, sums, averages). This protects tenant data privacy and builds trust.

---

## API Endpoints (Super Admin API)

These are on a separate route prefix (`/api/saas/`) protected by super admin auth, NOT the tenant JWT.

### Tenants
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/saas/tenants` | List all tenants (filter by status, plan, search) |
| `GET` | `/api/saas/tenants/{id}` | Tenant detail with subscription, usage, feature overrides |
| `PATCH` | `/api/saas/tenants/{id}/status` | Activate / Suspend / Cancel |
| `PATCH` | `/api/saas/tenants/{id}/plan` | Change plan (upgrade/downgrade) |
| `POST` | `/api/saas/tenants/{id}/extend-trial` | Extend trial by N days |

### Subscriptions & Billing
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/saas/billing/summary` | MRR, collected, outstanding, overdue |
| `GET` | `/api/saas/billing/invoices` | All invoices (filter by status, tenant, period) |
| `POST` | `/api/saas/billing/generate` | Generate monthly invoices for all active tenants |
| `PATCH` | `/api/saas/billing/invoices/{id}/mark-paid` | Mark invoice as paid manually |
| `POST` | `/api/saas/billing/invoices/{id}/send-reminder` | Send payment reminder email |
| `GET` | `/api/saas/billing/mrr-trend` | MRR data points for chart (last 12 months) |
| `GET` | `/api/saas/billing/churn` | Churn analysis: new, upgrades, downgrades, cancels |

### Feature Overrides
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/saas/features` | Full feature catalog |
| `GET` | `/api/saas/tenants/{id}/features` | Tenant's feature state (plan + overrides) |
| `PUT` | `/api/saas/tenants/{id}/features` | Set feature overrides for a tenant |

### Franchise
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/saas/franchise/networks` | All franchise networks with metrics |
| `GET` | `/api/saas/franchise/networks/{franchisorId}` | Network detail with franchisees |

### Analytics
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/saas/analytics/overview` | Platform-wide KPIs |
| `GET` | `/api/saas/analytics/tenant-growth` | New sign-ups per month |
| `GET` | `/api/saas/analytics/feature-adoption` | % of eligible tenants using each feature |
| `GET` | `/api/saas/analytics/engagement` | DAU, avg transactions, trial conversion |
| `GET` | `/api/saas/analytics/health` | API response times, error rates |
| `GET` | `/api/saas/analytics/leaderboard` | Top tenants by transaction volume |

### Support
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/saas/tenants/{id}/notes` | Support notes for a tenant |
| `POST` | `/api/saas/tenants/{id}/notes` | Add a support note |
| `GET` | `/api/saas/announcements` | List announcements |
| `POST` | `/api/saas/announcements` | Create announcement |
| `PATCH` | `/api/saas/announcements/{id}` | Update/deactivate announcement |

### Impersonation
| Method | Route | Description |
|---|---|---|
| `POST` | `/api/saas/impersonate/{tenantId}` | Start impersonation session |
| `DELETE` | `/api/saas/impersonate` | End current session |
| `GET` | `/api/saas/impersonate/log` | Impersonation audit log |

### Audit
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/saas/audit-log` | Platform audit log (filter by admin, action, tenant) |

---

## Blazor UI Design

### Layout

```
┌────────────────────────────────────────────────────────────────┐
│ TOPBAR (h-14)                                                  │
│ [☰] SplashSphere Ops    [🔍 Search tenants...]    [👤 Rob]   │
├──────────┬─────────────────────────────────────────────────────┤
│ SIDEBAR  │ MAIN CONTENT                                        │
│ (w-60)   │                                                     │
│          │                                                     │
│ 📊 Dash  │  (page content)                                     │
│ 🏢 Tenants│                                                    │
│ 💳 Billing│                                                    │
│ ⚡ Features│                                                   │
│ 🔗 Franchise│                                                  │
│ 📈 Analytics│                                                  │
│ 📨 Support │                                                   │
│ ⚙️ Settings│                                                   │
│          │                                                     │
│ ─────── │                                                      │
│ 📋 Audit │                                                     │
└──────────┴─────────────────────────────────────────────────────┘
```

### Design System
- Same SplashSphere aquatic theme (splash blue/aqua teal)
- Dark sidebar (`bg-gray-950`) with light main content
- Tailwind CSS v4 (same as main apps)
- Material Icons Outlined for sidebar icons
- Data-dense tables (this is an ops tool, not consumer-facing)
- Color-coded status badges matching the main app (Active=green, Trial=amber, Suspended=red, etc.)

---

## What the Super Admin Controls vs What Tenants Control

| Setting | Super Admin | Tenant Admin |
|---|---|---|
| Plan assignment | ✓ (override) | ✓ (self-service upgrade) |
| Feature overrides | ✓ (grant/revoke beyond plan) | ✗ |
| Billing/invoices | ✓ (generate, mark paid, void) | ✓ (view own, make payment) |
| Suspension | ✓ (manual) | ✗ |
| Trial extension | ✓ | ✗ |
| Tenant data (CRUD) | ✗ (read-only aggregates) | ✓ (full access to own data) |
| Feature config (services, pricing) | ✗ | ✓ |
| Employee/payroll management | ✗ | ✓ |
| Franchise settings | ✗ (view only) | ✓ (franchisor manages) |
| Platform announcements | ✓ | ✗ (receives them) |
| Impersonation | ✓ (read-only) | ✗ |

---

## Hangfire Jobs (Platform-Level)

| Job | Schedule | Description |
|---|---|---|
| `GenerateMonthlyInvoices` | 1st of month, 6 AM | Create invoice records for all active subscriptions |
| `ProcessDailyBilling` | Daily 9 AM | Attempt payment for due invoices via PayMongo |
| `SendPaymentReminders` | Daily 9 AM | Email tenants with invoices due in 3 days |
| `SuspendOverdueAccounts` | Daily 9 AM | Suspend tenants past due > 7 days grace |
| `SendTrialExpiryReminders` | Daily 9 AM | Email tenants with trials expiring in 1-3 days |
| `CalculateMonthlyMRR` | 1st of month | Snapshot MRR for trend chart |
| `ResetMonthlySmsCounters` | 1st of month | Reset SMS usage counters for all tenants |
| `PurgeExpiredAnnouncements` | Weekly | Remove announcements past their expiry date |

---

## Claude Code Prompts — Phase 20

### Prompt 20.1 — Super Admin Domain + Infrastructure

```
Enhance the existing SplashSphere.SaasAdmin Blazor project:

Domain/Models/:
- Update existing SaasTenant, Feature, TenantFeature, Invoice models to align 
  with Phase 16 subscription plans (Starter ₱1,499, Growth ₱2,999, Enterprise ₱4,999)
- Add: PlatformAuditLog, PlatformAnnouncement, SupportNote, ImpersonationSession
- Add AnnouncementTarget enum

Data/SaasDbContext:
- Add DbSets for new entities
- Add configurations with proper indexes
- Migration: "AlignSuperAdminModels"

Services/Interfaces:
- ITenantManagementService: CRUD, status changes, plan changes, trial extensions
- IBillingManagementService: invoice generation, payment tracking, MRR calculation
- IFeatureManagementService: feature matrix, overrides
- IAnalyticsService: aggregate queries across main DB (read-only)
- ISupportService: notes, announcements
- IAuditService: log platform actions, query audit trail

Register a SECOND DbContext (SplashSphereReadOnlyContext) that connects to the 
main SplashSphere database as READ-ONLY for aggregate analytics queries.
Connection string: ConnectionStrings__SplashSphereReadOnly

Set up the global InteractiveServer rendermode on Routes (App.razor).
```

### Prompt 20.2 — Super Admin Dashboard + Tenant Management

```
Build the core super admin pages:

1. / (Dashboard):
   - 8 KPI cards: MRR, total tenants, active, trial, suspended, overdue amount, 
     collection rate, total branches
   - MRR trend chart (last 12 months) using a charting library (Radzen or Chart.js via JS interop)
   - Plan distribution donut chart
   - Recent activity feed (last 10 events from audit log)
   - Tenant leaderboard (top 5 by transaction volume) with progress bars

2. /tenants — Tenant list:
   - Filter tabs: All, Active, Trial, Past Due, Suspended, Cancelled
   - Searchable data table with all columns from the spec
   - Actions dropdown per row

3. /tenants/{id} — Tenant detail:
   - 7 tabs: Overview, Subscription & Billing, Feature Overrides, 
     Branches & Usage, Franchise, Activity Log, Support Notes
   - Overview: subscription card, usage bars (branches X/Y, employees X/Y, SMS X/Y)
   - Feature Overrides: toggle matrix with override notes

Use the SplashSphere aquatic theme. Dark sidebar, light content area.
Shared components: StatCard, StatusBadge, PlanBadge, PageHeader, DataTable.
```

### Prompt 20.3 — Billing, Features, Analytics Pages

```
Build:

1. /billing — Billing & Revenue:
   - KPI cards: MRR, collected MTD, outstanding, overdue
   - Revenue by plan breakdown
   - Invoice table with status filters and actions (mark paid, send reminder, void)
   - "Generate Monthly Invoices" button
   - MRR trend chart
   - Churn analysis card: new, upgrades, downgrades, cancels, net change

2. /features — Feature Control:
   - Matrix view: tenants as columns, features as rows, cells are checkmarks
   - Click cell to toggle override
   - Per-tenant view: dropdown to select tenant, full feature list with toggles
   - Override notes field

3. /analytics — Platform Analytics:
   - Usage KPIs: total transactions, revenue processed, employees, branches, vehicles
   - Tenant growth chart (new sign-ups per month)
   - Feature adoption chart (% of eligible tenants using each Growth feature)
   - Trial conversion rate card
   - Platform health indicators (API response time, error rate, Hangfire status)

4. /franchise — Franchise Networks:
   - Network table: franchisor, plan, franchisee count, network MRR
   - Network detail: franchisee list with subscription status, royalty status
```

### Prompt 20.4 — Support, Settings, Impersonation, Audit

```
Build:

1. /support — Support & Communications:
   - Announcement management: create, edit, deactivate
   - Target selector: All / By Plan (checkboxes) / By Tenant (multi-select)
   - Announcement preview
   - Per-tenant support notes (linked from tenant detail tab 7)

2. /settings — Platform Settings:
   - Company info form
   - Plan pricing display (read-only)
   - Billing settings: grace period, auto-suspend toggle, invoice due days
   - SMS gateway config (masked API key)
   - Admin user management: list, add, remove

3. Impersonation:
   - From tenant detail: "Impersonate" button
   - Opens tenant's admin dashboard URL in new tab with a special impersonation 
     token (short-lived, read-only)
   - Red banner in the tenant app: "IMPERSONATING — Read Only"
   - Session logged in ImpersonationSession table

4. /audit — Audit Log:
   - Searchable, filterable log of all super admin actions
   - Columns: admin user, action, target tenant, timestamp, details
   - Filter by action type, admin user, date range, tenant
```

---

## Phase Summary

| Prompt | What | Focus |
|---|---|---|
| 20.1 | Models, DbContext, service interfaces, read-only cross-DB | Infrastructure |
| 20.2 | Dashboard KPIs, tenant list, tenant detail (7 tabs) | Core pages |
| 20.3 | Billing, feature matrix, analytics, franchise overview | Revenue & control |
| 20.4 | Support, settings, impersonation, audit log | Operations |

**Total: 4 prompts in Phase 20.**

---

## Key Design Decisions

1. **Blazor Server, not Next.js.** The super admin is an internal ops tool — not customer-facing. Blazor Server keeps it in the .NET ecosystem (shared domain models, EF Core), renders on the server (no client bundle to secure), and supports real-time dashboard updates via SignalR built into Blazor.

2. **Separate database.** SaaS management data (subscriptions, invoices, feature overrides, audit logs) lives in `SaasDbContext`. The main `SplashSphereDbContext` is accessed read-only for aggregate analytics. This prevents accidental cross-contamination and keeps the tenant data layer clean.

3. **Read-only aggregates only.** The super admin sees "AquaShine has 3,420 transactions this month" but never sees the individual transaction details, customer names, or employee salaries. This is a trust and privacy boundary.

4. **Impersonation is read-only.** You can see what the tenant sees, but can't modify their data. If a fix is needed, do it from the super admin (plan change, feature override) or guide the tenant through their own dashboard. Every impersonation session is fully logged.

5. **Audit everything.** Every action in the super admin creates an audit log entry. When a tenant asks "who changed my plan?" or "why was I suspended?", you have the answer with timestamps.

6. **Announcements feed into the tenant app.** When you create an announcement, it shows as a banner in the tenant's admin dashboard. The tenant app checks `/api/v1/announcements/active` on load and displays any relevant announcements.

7. **Plan pricing is code-defined, not admin-editable.** Changing plan prices is a business decision that affects all tenants. It requires a code change to `PlanCatalog`, a migration to update any stored amounts, and communication to existing customers. Making it editable in the UI is dangerous — one wrong click changes everyone's billing.
