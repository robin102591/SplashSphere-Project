# SplashSphere — Franchise Readiness Plan

> **Purpose:** This document defines how to extend SplashSphere from a multi-tenant SaaS for independent car wash owners into a platform that also supports franchise networks — where a franchisor manages multiple franchisees, enforces standards, collects royalties, and monitors network performance. This is Phase 17 work, to be built after the subscription system (Phase 16) is in place.

---

## Why Franchising Matters

The Philippines has a growing franchise culture — car wash chains like Speedy1, Nice Day!, and Clean Fuel Carwash operate dozens of locations. Today, each franchisee likely tracks operations manually or with disconnected systems. SplashSphere can offer:

1. **To the Franchisor:** Centralized visibility across all franchisees, automated royalty collection, brand compliance enforcement, and consolidated reporting. This is a high-value Enterprise subscription.

2. **To the Franchisee:** A pre-configured system with the franchisor's services, pricing, and branding already loaded — faster setup, less configuration, and compliance built in.

3. **To LezanobTech:** Higher revenue per network (1 franchisor + 20 franchisees = 21 subscriptions), lower churn (switching costs are high when the whole network uses the platform), and a strong competitive moat.

---

## Tenant Hierarchy Model

The current system has flat tenants — each tenant is independent. Franchising introduces a parent-child relationship:

```
┌────────────────────────────────────────────────────────────┐
│  LEZANOBTECH (SaaS Provider — YOU)                         │
│  Manages: all tenants, subscriptions, billing, platform    │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  ┌──────────────────┐    ┌──────────────────┐              │
│  │ INDEPENDENT      │    │ FRANCHISOR       │              │
│  │ "Juan's Car Wash"│    │ "SpeedyWash PH"  │              │
│  │                  │    │ (parent tenant)   │              │
│  │ Branch: Makati   │    │                  │              │
│  │ Branch: BGC      │    │  ┌─────────────┐ │              │
│  │                  │    │  │ FRANCHISEE   │ │              │
│  │ (self-managed,   │    │  │ "SpeedyWash  │ │              │
│  │  normal tenant)  │    │  │  Makati"     │ │              │
│  └──────────────────┘    │  │ (child tent) │ │              │
│                          │  └─────────────┘ │              │
│                          │  ┌─────────────┐ │              │
│                          │  │ FRANCHISEE   │ │              │
│                          │  │ "SpeedyWash  │ │              │
│                          │  │  Cebu"       │ │              │
│                          │  └─────────────┘ │              │
│                          │  ┌─────────────┐ │              │
│                          │  │ FRANCHISEE   │ │              │
│                          │  │ "SpeedyWash  │ │              │
│                          │  │  Davao"      │ │              │
│                          │  └─────────────┘ │              │
│                          └──────────────────┘              │
│                                                            │
│  ┌──────────────────┐                                      │
│  │ CORPORATE CHAIN  │                                      │
│  │ "WashPro Corp"   │                                      │
│  │ (single tenant,  │                                      │
│  │  many branches)  │                                      │
│  └──────────────────┘                                      │
└────────────────────────────────────────────────────────────┘
```

### Tenant Types

```csharp
public enum TenantType
{
    Independent,      // Single owner, self-managed (current default)
    CorporateChain,   // Multiple branches, single management (current multi-branch)
    Franchisor,       // Brand owner, manages franchisees
    Franchisee        // Licensed operator under a franchisor
}
```

### Key Differences

| Aspect | Independent / Corporate | Franchisor | Franchisee |
|---|---|---|---|
| Services | Self-managed | Defines templates for franchisees | Inherits from franchisor (can customize within limits) |
| Pricing | Self-managed | Sets base pricing templates | Uses franchisor templates, limited overrides |
| Dashboard | Own data only | Network-wide view + own | Own data + network benchmarks |
| Reporting | Standard reports | Consolidated + per-franchisee | Standard + reports sent to franchisor |
| Branding | Own branding | Defines brand guidelines | Uses franchisor branding |
| Payroll | Self-managed | Self-managed | Self-managed (franchisor can view) |
| Royalties | None | Collects from franchisees | Pays to franchisor |
| Plan | Any plan | Enterprise required | Starter or Growth (franchisor chooses) |

---

## Domain Model Extensions

### Changes to Existing Tenant Entity

```csharp
// Add to the existing Tenant entity
public TenantType TenantType { get; set; } = TenantType.Independent;
public string? ParentTenantId { get; set; }       // Franchisee → Franchisor link
public string? FranchiseCode { get; set; }         // e.g., "SPEEDY-MKT-001"
public string? TaxId { get; set; }                 // BIR TIN
public string? BusinessPermitNo { get; set; }

// Navigation
public Tenant? ParentTenant { get; set; }
public List<Tenant> ChildTenants { get; set; } = [];  // Franchisor's franchisees
public FranchiseAgreement? FranchiseAgreement { get; set; }
public FranchiseSettings? FranchiseSettings { get; set; }
```

### New Entities

```csharp
// Franchise-specific settings (owned by the Franchisor)
public sealed class FranchiseSettings : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;  // Franchisor's tenantId

    // Royalty configuration
    public decimal RoyaltyRate { get; set; }               // e.g., 0.05 = 5%
    public decimal MarketingFeeRate { get; set; }          // e.g., 0.02 = 2%
    public decimal TechnologyFeeRate { get; set; }         // e.g., 0.01 = 1%
    public RoyaltyBasis RoyaltyBasis { get; set; } = RoyaltyBasis.GrossRevenue;
    public RoyaltyFrequency RoyaltyFrequency { get; set; } = RoyaltyFrequency.Monthly;

    // Standardization controls
    public bool EnforceStandardServices { get; set; }      // Franchisees must use franchisor's service list
    public bool EnforceStandardPricing { get; set; }       // Franchisees use franchisor's pricing
    public bool AllowLocalServices { get; set; }           // Franchisees can add local-only services
    public decimal? MaxPriceVariance { get; set; }         // e.g., 0.10 = ±10% from standard pricing
    public bool EnforceBranding { get; set; }              // Franchisees use franchisor's receipt/display branding
    
    // Network defaults
    public PlanTier DefaultFranchiseePlan { get; set; } = PlanTier.Growth;
    public int MaxBranchesPerFranchisee { get; set; } = 3;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Tenant Tenant { get; set; } = null!;
}

// Agreement between franchisor and each franchisee
public sealed class FranchiseAgreement : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string FranchisorTenantId { get; set; } = string.Empty;
    public string FranchiseeTenantId { get; set; } = string.Empty;
    public string AgreementNumber { get; set; } = string.Empty;   // e.g., "FA-2026-0012"

    // Territory
    public string TerritoryName { get; set; } = string.Empty;     // e.g., "Makati City"
    public string? TerritoryDescription { get; set; }              // Boundaries, zip codes, etc.
    public bool ExclusiveTerritory { get; set; }                   // No other franchisee in this area

    // Contract terms
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }                          // Contract expiry
    public decimal InitialFranchiseFee { get; set; }               // One-time fee paid at signing
    public AgreementStatus Status { get; set; } = AgreementStatus.Active;

    // Customized rates (override FranchiseSettings if needed)
    public decimal? CustomRoyaltyRate { get; set; }
    public decimal? CustomMarketingFeeRate { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant FranchisorTenant { get; set; } = null!;
    public Tenant FranchiseeTenant { get; set; } = null!;
}

// Monthly royalty calculations
public sealed class RoyaltyPeriod : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string FranchisorTenantId { get; set; } = string.Empty;
    public string FranchiseeTenantId { get; set; } = string.Empty;
    public string AgreementId { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal GrossRevenue { get; set; }                  // Franchisee's revenue in period
    public decimal RoyaltyRate { get; set; }                   // Rate applied
    public decimal RoyaltyAmount { get; set; }                 // Revenue × rate
    public decimal MarketingFeeRate { get; set; }
    public decimal MarketingFeeAmount { get; set; }
    public decimal TechnologyFeeRate { get; set; }
    public decimal TechnologyFeeAmount { get; set; }
    public decimal TotalDue { get; set; }                      // Sum of all fees

    public RoyaltyStatus Status { get; set; } = RoyaltyStatus.Pending;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant FranchisorTenant { get; set; } = null!;
    public Tenant FranchiseeTenant { get; set; } = null!;
    public FranchiseAgreement Agreement { get; set; } = null!;
}

// Template services that franchisor pushes to franchisees
public sealed class FranchiseServiceTemplate : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string FranchisorTenantId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public decimal BasePrice { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsRequired { get; set; }            // Franchisee MUST offer this service
    public string? PricingMatrixJson { get; set; }   // Standard pricing matrix as JSON
    public string? CommissionMatrixJson { get; set; } // Standard commission matrix as JSON
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### New Enums

```csharp
public enum RoyaltyBasis { GrossRevenue, NetRevenue, ServiceRevenueOnly }
public enum RoyaltyFrequency { Weekly, Monthly }
public enum AgreementStatus { Draft, Active, Expired, Terminated, Suspended }
public enum RoyaltyStatus { Pending, Invoiced, Paid, Overdue }
```

---

## Franchise Onboarding Flow

### Franchisor Signs Up

```
1. Franchisor signs up normally (admin app sign-up)
2. During onboarding, selects TenantType = "Franchisor"
3. Onboarding wizard has additional steps:
   Step 1: Business details (same as independent)
   Step 2: Franchise network details:
     - Brand name
     - Number of existing locations (if migrating)
     - Tax ID / SEC registration
   Step 3: Franchise settings:
     - Royalty rate (e.g., 5%)
     - Marketing fee rate (e.g., 2%)
     - Technology fee rate (e.g., 1%)
     - Standardization preferences (enforce services? pricing? branding?)
   Step 4: Service templates:
     - Define the standard service list that franchisees will use
     - Set standard pricing and commission matrices
   Step 5: Confirmation
4. System creates Tenant (type=Franchisor) + FranchiseSettings
5. Franchisor must be on Enterprise plan
```

### Franchisee Invited by Franchisor

```
1. Franchisor admin dashboard → "Franchisees" → "Invite New Franchisee"
2. Franchisor enters:
   - Franchisee's business name, owner name, email, phone
   - Territory assignment
   - Agreement details (start/end date, initial fee)
   - Optional: custom royalty rate override
3. System sends invitation email with a unique link
4. Franchisee clicks link → lands on special sign-up page:
   - Pre-filled: franchisor name, territory, agreement terms
   - Franchisee enters: password, branch details
   - Accepts terms and franchise agreement
5. System creates:
   - Tenant (type=Franchisee, parentTenantId = franchisor's ID)
   - FranchiseAgreement record
   - Branch (pre-configured from invitation)
   - Subscription at the plan the franchisor configured (default: Growth)
   - Services, pricing, and commission matrices CLONED from franchisor's templates
6. Franchisee lands on their admin dashboard — pre-configured and ready to use
```

### Independent Owner Converts to Franchisee

```
An existing independent tenant can join a franchise network:
1. Franchisor sends invitation to existing tenant's email
2. Tenant owner accepts from their settings page
3. System updates:
   - TenantType → Franchisee
   - ParentTenantId → franchisor's ID
   - Creates FranchiseAgreement
   - Optionally syncs services from franchisor templates (merge or replace)
4. Existing data (transactions, employees, customers) is preserved
```

---

## Royalty Calculation Engine

### Monthly Royalty Flow

```
Hangfire recurring job: 1st of each month, 6 AM PHT

FOR each Franchisor tenant:
  FOR each active Franchisee (parentTenantId = franchisor):
    1. Get the FranchiseAgreement (for custom rates)
    2. Query COMPLETED transactions for the previous month
    3. Calculate:
       GrossRevenue = SUM(transaction.finalAmount)
       
       royaltyRate = agreement.CustomRoyaltyRate ?? franchisorSettings.RoyaltyRate
       RoyaltyAmount = GrossRevenue × royaltyRate
       
       marketingFeeRate = agreement.CustomMarketingFeeRate ?? franchisorSettings.MarketingFeeRate
       MarketingFeeAmount = GrossRevenue × marketingFeeRate
       
       TechnologyFeeAmount = GrossRevenue × franchisorSettings.TechnologyFeeRate
       
       TotalDue = RoyaltyAmount + MarketingFeeAmount + TechnologyFeeAmount
       
    4. Create RoyaltyPeriod record
    5. If auto-invoicing enabled → generate invoice and send to franchisee
    6. Publish RoyaltyCalculatedEvent
```

### Example Calculation

```
SpeedyWash Makati (Franchisee) — March 2026:
  Gross Revenue:        ₱287,500
  Royalty (5%):          ₱14,375
  Marketing Fee (2%):     ₱5,750
  Technology Fee (1%):    ₱2,875
  ─────────────────────────────
  Total Due:             ₱23,000
  
  Due date: April 7, 2026 (7 days after period end)
  Payment: Franchisee pays via bank transfer or in-app payment
```

---

## Franchisor Dashboard — Additional Views

The franchisor sees everything a normal tenant sees for their own operations, PLUS these franchise-specific views:

### Network Overview Page (`/franchise/network`)

```
KPI Cards:
  Total Franchisees: 23 (20 active, 2 pending, 1 suspended)
  Network Revenue (this month): ₱4.2M
  Royalties Due: ₱336,000
  Avg Revenue per Franchisee: ₱210,000

Franchisee Performance Table:
  Franchisee | Territory | Revenue | Royalty Due | Status | Compliance
  SpeedyWash Makati | Makati | ₱287,500 | ₱23,000 | Active | ✓
  SpeedyWash Cebu | Cebu City | ₱198,000 | ₱15,840 | Active | ✓
  SpeedyWash Davao | Davao | ₱152,000 | ₱12,160 | Active | ⚠ Overdue
  ...

Map View (optional):
  Philippines map with pins showing each franchisee location
```

### Franchisee Management Page (`/franchise/franchisees`)

```
List of all franchisees with:
  - Business name, territory, owner name
  - Agreement status (Active/Expired/Suspended)
  - Subscription plan
  - Revenue this month
  - Royalty payment status
  - Actions: View Details, Suspend, Edit Agreement

"Invite New Franchisee" button
```

### Royalty Management Page (`/franchise/royalties`)

```
Period selector (month/year)

Royalty summary table:
  Franchisee | Gross Revenue | Royalty | Marketing | Tech | Total Due | Status
  
Actions: Generate all invoices, Mark as paid, Send reminder

Total collected vs outstanding bar chart
```

### Service Templates Page (`/franchise/templates`)

```
Standard services list with:
  - Service name, category, base price, duration
  - "Required" flag (franchisees must offer)
  - Edit service → changes propagate to all franchisees
  
"Push Updates to Franchisees" button:
  Syncs service definitions to all active franchisees
```

### Compliance Dashboard (`/franchise/compliance`)

```
Per-franchisee compliance check:
  ✓ Using standard services
  ✓ Pricing within variance limits
  ✓ Royalties current
  ⚠ Missing this week's report
  ✗ Non-standard services detected

Overall network compliance score: 94%
```

---

## Franchisee Experience

Franchisees use the same admin dashboard as independent tenants, with these additions:

### Franchisor Banner

At the top of the dashboard: "Operating under SpeedyWash PH franchise. Territory: Makati City."

### Restricted Settings

If `EnforceStandardServices = true`:
- Service list is read-only (synced from franchisor)
- Pricing matrix is read-only (or editable within variance limit)
- "These services are managed by your franchisor" notice

If `AllowLocalServices = true`:
- Franchisee can add additional services marked as "Local Only"
- These don't appear in the franchisor's standard list

### Royalty Statement Page (`/franchise/royalties`)

Franchisee sees their own royalty statements:
```
March 2026:
  Gross Revenue: ₱287,500
  Royalty (5%): ₱14,375
  Marketing Fee (2%): ₱5,750
  Tech Fee (1%): ₱2,875
  Total Due: ₱23,000
  Status: Paid (Mar 28, 2026)
  Reference: BT-20260328-001
```

### Network Benchmarks (Anonymized)

```
Your Revenue vs Network Average:
  You: ₱287,500 | Network Avg: ₱210,000 | Rank: #3 of 23

Your Top Services vs Network:
  1. Premium Wash — you: 35% | network: 28%
  2. Basic Wash — you: 30% | network: 42%
```

---

## API Endpoints — Franchise

### Franchisor Endpoints (requires TenantType = Franchisor)

| Method | Route | Description |
|---|---|---|
| `GET` | `/franchise/settings` | Get franchise network settings |
| `PUT` | `/franchise/settings` | Update franchise settings (royalty rates, standardization) |
| `GET` | `/franchise/franchisees` | List all franchisees |
| `POST` | `/franchise/franchisees/invite` | Send invitation to new franchisee |
| `GET` | `/franchise/franchisees/{id}` | Franchisee detail (revenue, compliance, agreement) |
| `PATCH` | `/franchise/franchisees/{id}/suspend` | Suspend a franchisee |
| `GET` | `/franchise/agreements` | List all franchise agreements |
| `POST` | `/franchise/agreements` | Create agreement |
| `PUT` | `/franchise/agreements/{id}` | Update agreement |
| `GET` | `/franchise/royalties` | List royalty periods across all franchisees |
| `POST` | `/franchise/royalties/calculate` | Trigger royalty calculation for a period |
| `PATCH` | `/franchise/royalties/{id}/mark-paid` | Mark royalty as paid |
| `GET` | `/franchise/templates` | List service templates |
| `POST` | `/franchise/templates` | Create service template |
| `PUT` | `/franchise/templates/{id}` | Update template |
| `POST` | `/franchise/templates/push` | Push templates to all franchisees |
| `GET` | `/franchise/network/summary` | Network KPIs |
| `GET` | `/franchise/compliance` | Compliance report per franchisee |

### Franchisee Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/franchise/my-agreement` | Get my franchise agreement details |
| `GET` | `/franchise/my-royalties` | My royalty statements |
| `GET` | `/franchise/benchmarks` | Network benchmarks (anonymized) |

### Invitation Endpoint (Public)

| Method | Route | Description |
|---|---|---|
| `GET` | `/franchise/invitation/{token}` | Validate invitation token |
| `POST` | `/franchise/invitation/{token}/accept` | Accept and create franchisee tenant |

---

## Data Isolation & Visibility Rules

| Data | Independent Sees | Franchisee Sees | Franchisor Sees |
|---|---|---|---|
| Own transactions | ✓ | ✓ | ✗ (summary only) |
| Own employees | ✓ | ✓ | ✗ (count only) |
| Own customers | ✓ | ✓ | ✗ |
| Revenue totals | Own | Own + network rank | All franchisees |
| Service list | Own | Franchisor's + local | Template list |
| Pricing | Own | Franchisor's ±variance | Standard pricing |
| Payroll | Own | Own | ✗ |
| Royalties | N/A | Own statements | All franchisees |
| Compliance | N/A | Own status | All franchisees |

**Critical rule:** A franchisor NEVER sees individual transaction details, employee salaries, or customer personal data of franchisees. They only see aggregated revenue, transaction counts, and compliance metrics. This protects franchisee privacy while giving the franchisor the oversight they need.

---

## Implementation — When to Build This

### Prerequisites (Must Exist First)

- [x] Multi-tenant architecture with tenant isolation (Phase 3)
- [x] Service and pricing management (Phase 4)
- [x] Transaction engine with commission tracking (Phase 5)
- [x] Subscription and plan enforcement (Phase 16)
- [ ] Role-based access control (recommended before franchise)

### Phase 17 Prompt Sequence

#### Prompt 17.1 — Franchise Domain + Infrastructure

```
Extend the SplashSphere domain for franchise support:

Domain:
- Add TenantType enum (Independent, CorporateChain, Franchisor, Franchisee)
- Add fields to Tenant entity: TenantType, ParentTenantId, FranchiseCode, TaxId, 
  BusinessPermitNo. Add self-referencing navigation (ParentTenant, ChildTenants).
- New entities: FranchiseSettings, FranchiseAgreement, RoyaltyPeriod, 
  FranchiseServiceTemplate
- New enums: RoyaltyBasis, RoyaltyFrequency, AgreementStatus, RoyaltyStatus

Infrastructure:
- EF configurations for all new entities
- FranchiseAgreement: unique on [franchisorTenantId, franchiseeTenantId]
- RoyaltyPeriod: unique on [franchiseeTenantId, periodStart, periodEnd]
- Self-referencing Tenant relationship: 
  HasOne(t => t.ParentTenant).WithMany(t => t.ChildTenants).HasForeignKey(t => t.ParentTenantId)
- NO global tenant filter on FranchiseAgreement or RoyaltyPeriod 
  (franchisor needs cross-tenant visibility)
- Migration: "AddFranchiseSupport"
```

#### Prompt 17.2 — Franchise Application Layer

```
Build the Franchise CQRS feature:

Application/Features/Franchise/:

Franchisor Commands:
- UpdateFranchiseSettingsCommand (royalty rates, standardization flags)
- InviteFranchiseeCommand (email, businessName, territory, agreement details)
  → generates unique invitation token, sends email
- CreateFranchiseAgreementCommand
- SuspendFranchiseeCommand / ReactivateFranchiseeCommand
- PushServiceTemplatesToFranchiseesCommand
  → clones FranchiseServiceTemplate records into Service/ServicePricing/ServiceCommission 
    for each active franchisee, respecting existing local services
- CalculateRoyaltiesCommand(DateTime periodStart, DateTime periodEnd)
  → for each franchisee: query completed transactions, calculate fees, create RoyaltyPeriod
- MarkRoyaltyPaidCommand(string royaltyPeriodId, string paymentReference)

Franchisor Queries:
- GetFranchiseSettingsQuery
- GetFranchiseesQuery (with revenue summary, compliance status)
- GetFranchiseeDetailQuery (revenue, royalties, agreement, compliance)
- GetRoyaltyPeriodsQuery (filter by franchisee, period, status)
- GetNetworkSummaryQuery (KPIs: total revenue, total royalties, franchisee count)
- GetComplianceReportQuery (per-franchisee: services match?, pricing within variance?)
- GetServiceTemplatesQuery

Franchisee Queries:
- GetMyAgreementQuery
- GetMyRoyaltiesQuery
- GetNetworkBenchmarksQuery (anonymized: rank, avg revenue, top services)

Invitation:
- ValidateInvitationQuery(string token) → returns franchisor name, territory, terms
- AcceptInvitationCommand(string token, string password, string branchName, string branchAddress)
  → creates Tenant (type=Franchisee), Clerk Organization, Branch, 
    clones service templates, creates subscription

FranchiseEndpoints.cs — all routes.

Hangfire:
- CalculateMonthlyRoyaltiesJob: 1st of each month, calculates for all franchise networks
- SendRoyaltyRemindersJob: 5 days after period end, remind unpaid
```

#### Prompt 17.3 — Franchise Frontend (Admin)

```
Build franchise pages in the admin app:

FRANCHISOR VIEWS (only visible when tenant.tenantType === 'franchisor'):

1. Add "Franchise" nav group in sidebar with sub-items:
   Network, Franchisees, Royalties, Templates, Compliance

2. /franchise/network — Network Overview:
   KPI cards (total franchisees, network revenue, royalties due, avg revenue)
   Franchisee performance table (name, territory, revenue, royalty, status)

3. /franchise/franchisees — Franchisee List:
   Data table with filters. "Invite New Franchisee" button opens dialog form.

4. /franchise/franchisees/[id] — Franchisee Detail:
   Agreement info, revenue chart, royalty history, compliance status.
   Suspend/reactivate button.

5. /franchise/royalties — Royalty Management:
   Month/year picker. Summary table. "Calculate Royalties" button.
   "Mark as Paid" per row. Outstanding total card.

6. /franchise/templates — Service Templates:
   CRUD for template services with pricing/commission matrices.
   "Push to All Franchisees" button with confirmation.

7. /franchise/settings — Franchise Settings:
   Royalty rate, marketing fee rate, tech fee rate.
   Standardization toggles. Default franchisee plan.

FRANCHISEE VIEWS (only visible when tenant.tenantType === 'franchisee'):

1. Banner at top of dashboard: "SpeedyWash PH Franchise — Makati Territory"

2. /franchise/my-royalties — Royalty Statements:
   Table of monthly statements. Status badges (Pending/Paid/Overdue).

3. /franchise/benchmarks — Network Benchmarks:
   Your revenue vs network average. Rank (anonymized).

4. If services are franchisor-enforced:
   Show read-only badge on service list page: "Managed by SpeedyWash PH"
   Disable edit/delete on franchisor services.
   Allow "Add Local Service" button if AllowLocalServices = true.
```

#### Prompt 17.4 — Franchise Invitation Flow

```
Build the franchise invitation and acceptance flow:

1. Admin app — Invite form (franchisor):
   Dialog with: email, business name, owner name, territory, start date, 
   end date, initial fee, custom royalty rate (optional).
   Submit → POST /franchise/franchisees/invite
   
2. Backend:
   - Generate unique token (UUID + HMAC signature)
   - Create FranchiseAgreement with status = Draft
   - Send email with link: https://app.splashsphere.ph/franchise/join/{token}

3. Admin app — Invitation acceptance page:
   /franchise/join/[token] — public page (no auth required):
   - Shows: franchisor name, territory, agreement terms, royalty rates
   - Form: owner name, email, password, branch name, branch address
   - "Accept & Create Account" button
   - On submit: creates Clerk user + organization, creates Tenant (Franchisee),
     clones service templates, sets up subscription, redirects to dashboard.
   
4. After acceptance:
   - FranchiseAgreement.Status → Active
   - Franchisor's franchisee list updates
   - Notification to franchisor: "SpeedyWash Makati has joined the network"
```

---

## Subscription Integration

Franchise networks work with the subscription system (Phase 16):

- **Franchisor** must be on **Enterprise** plan (₱4,999/month). Includes API access, unlimited branches, all features.
- **Franchisees** are auto-assigned the plan specified in `FranchiseSettings.DefaultFranchiseePlan` (typically Growth).
- **Billing:** Each franchisee has their own subscription and pays LezanobTech directly. The franchisor does NOT pay for franchisee subscriptions — each is an independent paying customer.
- **Royalties** are separate from SaaS subscriptions. Royalties are paid franchisee → franchisor (not to LezanobTech). SplashSphere just calculates and tracks them.

```
Money flows:
  Franchisee → LezanobTech: monthly SaaS subscription (₱2,999)
  Franchisee → Franchisor: monthly royalties (calculated in SplashSphere)
  Franchisor → LezanobTech: monthly SaaS subscription (₱4,999)
```

---

## Revenue Impact

A single franchise network signing up means:

```
1 Franchisor × ₱4,999/month  = ₱4,999
20 Franchisees × ₱2,999/month = ₱59,980
─────────────────────────────────────────
Total MRR from one network    = ₱64,979/month
                              = ₱779,748/year
```

Compared to 21 independent owners on Starter: 21 × ₱1,499 = ₱31,479/month. The franchise model generates **2× more revenue** from the same number of tenants, with lower churn because switching costs are network-wide.

---

## Phase Summary

| Prompt | What | Dependency |
|---|---|---|
| 17.1 | Domain models, enums, EF configs, migration | Phase 16 complete |
| 17.2 | CQRS: invitations, royalties, templates, compliance | 17.1 |
| 17.3 | Franchisor + franchisee frontend pages | 17.2 |
| 17.4 | Invitation flow (send, accept, onboard) | 17.2 |

**Total: 4 prompts in Phase 17.**

### What NOT to Build Yet

- **Franchise marketplace** (public directory of franchise opportunities) — future
- **Automated royalty payment** (payment gateway integration for royalty collection) — future, manual tracking first
- **Territory map visualization** — future, use text-based territory descriptions initially
- **Franchisor mobile app** — future, desktop admin is sufficient
- **Multi-currency support** — future, PHP only for now
- **White-label branding** (franchisor's logo on franchisee's POS) — future, text branding first
