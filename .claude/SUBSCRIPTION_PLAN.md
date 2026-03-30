# SplashSphere — Subscription & Plan Enforcement Implementation

> **Purpose:** This document defines how to implement the three pricing tiers (Starter, Growth, Enterprise) in code — from domain models and feature gating middleware to billing integration and trial management. This is a new Phase 16 to be executed after Phase 15.

---

## Architecture Overview

The subscription system has three layers:

```
┌─────────────────────────────────────────────────────────────┐
│  LAYER 1: Plan Definition (what each tier includes)         │
│  Static config — defines features, limits, and pricing      │
├─────────────────────────────────────────────────────────────┤
│  LAYER 2: Plan Enforcement (runtime gating)                 │
│  Middleware + service that checks tenant's plan before       │
│  allowing access to features or enforcing limits            │
├─────────────────────────────────────────────────────────────┤
│  LAYER 3: Billing & Lifecycle (payment + trial)             │
│  Subscription records, trial tracking, payment gateway,     │
│  invoicing, plan changes, suspension on non-payment         │
└─────────────────────────────────────────────────────────────┘
```

**Key principle:** The tenant's plan is stored in the database. Every API request checks the tenant's active plan and enforces limits. The admin and POS frontends read the plan and hide/show features accordingly. The SaaS super-admin (Blazor app) manages plans and billing.

---

## Layer 1: Plan Definition

### Feature Catalog

Every feature in SplashSphere has a unique string key. The plan defines which keys are enabled.

```csharp
public static class FeatureKeys
{
    // Core (all plans)
    public const string Pos = "pos";
    public const string CommissionTracking = "commission_tracking";
    public const string WeeklyPayroll = "weekly_payroll";
    public const string BasicReports = "basic_reports";
    public const string CustomerManagement = "customer_management";
    public const string VehicleManagement = "vehicle_management";
    public const string EmployeeManagement = "employee_management";
    public const string MerchandiseManagement = "merchandise_management";

    // Growth (Growth + Enterprise)
    public const string QueueManagement = "queue_management";
    public const string CustomerLoyalty = "customer_loyalty";
    public const string CashAdvanceTracking = "cash_advance_tracking";
    public const string ExpenseTracking = "expense_tracking";
    public const string ShiftManagement = "shift_management";
    public const string ProfitLossReports = "profit_loss_reports";
    public const string SmsNotifications = "sms_notifications";
    public const string PricingModifiers = "pricing_modifiers";

    // Enterprise only
    public const string ApiAccess = "api_access";
    public const string CustomIntegrations = "custom_integrations";
}
```

### Plan Definitions

```csharp
public enum PlanTier { Trial, Starter, Growth, Enterprise }

public sealed class PlanDefinition
{
    public PlanTier Tier { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public int MaxBranches { get; init; }
    public int MaxEmployees { get; init; }
    public int SmsPerMonth { get; init; }
    public HashSet<string> Features { get; init; } = [];
}

public static class PlanCatalog
{
    public static readonly PlanDefinition Trial = new()
    {
        Tier = PlanTier.Trial,
        Name = "Free Trial",
        MonthlyPrice = 0,
        MaxBranches = 1,
        MaxEmployees = 5,
        SmsPerMonth = 10,
        Features = [
            // Trial gets Growth features for 14 days so they experience the full product
            FeatureKeys.Pos, FeatureKeys.CommissionTracking, FeatureKeys.WeeklyPayroll,
            FeatureKeys.BasicReports, FeatureKeys.CustomerManagement, FeatureKeys.VehicleManagement,
            FeatureKeys.EmployeeManagement, FeatureKeys.MerchandiseManagement,
            FeatureKeys.QueueManagement, FeatureKeys.CustomerLoyalty, FeatureKeys.CashAdvanceTracking,
            FeatureKeys.ExpenseTracking, FeatureKeys.ShiftManagement, FeatureKeys.ProfitLossReports,
            FeatureKeys.SmsNotifications, FeatureKeys.PricingModifiers
        ]
    };

    public static readonly PlanDefinition Starter = new()
    {
        Tier = PlanTier.Starter,
        Name = "Starter",
        MonthlyPrice = 1499m,
        MaxBranches = 1,
        MaxEmployees = 5,
        SmsPerMonth = 0,
        Features = [
            FeatureKeys.Pos, FeatureKeys.CommissionTracking, FeatureKeys.WeeklyPayroll,
            FeatureKeys.BasicReports, FeatureKeys.CustomerManagement, FeatureKeys.VehicleManagement,
            FeatureKeys.EmployeeManagement, FeatureKeys.MerchandiseManagement
        ]
    };

    public static readonly PlanDefinition Growth = new()
    {
        Tier = PlanTier.Growth,
        Name = "Growth",
        MonthlyPrice = 2999m,
        MaxBranches = 3,
        MaxEmployees = 15,
        SmsPerMonth = 50,
        Features = [
            // Everything in Starter
            FeatureKeys.Pos, FeatureKeys.CommissionTracking, FeatureKeys.WeeklyPayroll,
            FeatureKeys.BasicReports, FeatureKeys.CustomerManagement, FeatureKeys.VehicleManagement,
            FeatureKeys.EmployeeManagement, FeatureKeys.MerchandiseManagement,
            // Growth additions
            FeatureKeys.QueueManagement, FeatureKeys.CustomerLoyalty, FeatureKeys.CashAdvanceTracking,
            FeatureKeys.ExpenseTracking, FeatureKeys.ShiftManagement, FeatureKeys.ProfitLossReports,
            FeatureKeys.SmsNotifications, FeatureKeys.PricingModifiers
        ]
    };

    public static readonly PlanDefinition Enterprise = new()
    {
        Tier = PlanTier.Enterprise,
        Name = "Enterprise",
        MonthlyPrice = 4999m,
        MaxBranches = int.MaxValue,  // Unlimited
        MaxEmployees = int.MaxValue,
        SmsPerMonth = 200,
        Features = [
            // Everything in Growth
            FeatureKeys.Pos, FeatureKeys.CommissionTracking, FeatureKeys.WeeklyPayroll,
            FeatureKeys.BasicReports, FeatureKeys.CustomerManagement, FeatureKeys.VehicleManagement,
            FeatureKeys.EmployeeManagement, FeatureKeys.MerchandiseManagement,
            FeatureKeys.QueueManagement, FeatureKeys.CustomerLoyalty, FeatureKeys.CashAdvanceTracking,
            FeatureKeys.ExpenseTracking, FeatureKeys.ShiftManagement, FeatureKeys.ProfitLossReports,
            FeatureKeys.SmsNotifications, FeatureKeys.PricingModifiers,
            // Enterprise additions
            FeatureKeys.ApiAccess, FeatureKeys.CustomIntegrations
        ]
    };

    public static PlanDefinition GetPlan(PlanTier tier) => tier switch
    {
        PlanTier.Trial => Trial,
        PlanTier.Starter => Starter,
        PlanTier.Growth => Growth,
        PlanTier.Enterprise => Enterprise,
        _ => Starter
    };
}
```

---

## Layer 2: Plan Enforcement

### Domain Models

```csharp
public sealed class TenantSubscription : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public PlanTier PlanTier { get; set; } = PlanTier.Trial;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;

    // Trial tracking
    public DateTime TrialStartDate { get; set; }
    public DateTime TrialEndDate { get; set; }  // TrialStart + 14 days
    public bool TrialExpired => Status == SubscriptionStatus.Trial && DateTime.UtcNow > TrialEndDate;

    // Billing cycle
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? NextBillingDate { get; set; }

    // Overrides (SaaS admin can grant exceptions)
    public int? MaxBranchesOverride { get; set; }
    public int? MaxEmployeesOverride { get; set; }
    public int? SmsPerMonthOverride { get; set; }
    public string? FeatureOverrides { get; set; }  // JSON: additional features enabled/disabled

    // Usage tracking
    public int SmsUsedThisMonth { get; set; }
    public DateTime SmsCountResetDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}

public enum SubscriptionStatus
{
    Trial,          // In free trial period
    Active,         // Paid and current
    PastDue,        // Payment failed, grace period (7 days)
    Suspended,      // Non-payment, features restricted to read-only
    Cancelled       // Tenant cancelled, data retained for 30 days
}
```

### Plan Enforcement Service

```csharp
public interface IPlanEnforcementService
{
    Task<bool> HasFeatureAsync(string tenantId, string featureKey, CancellationToken ct);
    Task<PlanLimitResult> CheckLimitAsync(string tenantId, LimitType limitType, CancellationToken ct);
    Task<PlanDefinition> GetActivePlanAsync(string tenantId, CancellationToken ct);
    Task<int> GetSmsBudgetRemainingAsync(string tenantId, CancellationToken ct);
}

public enum LimitType { Branches, Employees, SmsPerMonth }

public sealed record PlanLimitResult(bool Allowed, int CurrentCount, int MaxAllowed, string Message);

public sealed class PlanEnforcementService : IPlanEnforcementService
{
    private readonly SplashSphereDbContext _db;
    private readonly IHybridCache _cache;

    public async Task<bool> HasFeatureAsync(string tenantId, string featureKey, CancellationToken ct)
    {
        var sub = await GetSubscriptionAsync(tenantId, ct);

        // Suspended tenants get read-only access to core features only
        if (sub.Status == SubscriptionStatus.Suspended)
            return false;

        // Trial expired → block everything
        if (sub.TrialExpired)
            return false;

        var plan = PlanCatalog.GetPlan(sub.PlanTier);

        // Check feature overrides first (SaaS admin can grant/revoke)
        if (!string.IsNullOrEmpty(sub.FeatureOverrides))
        {
            var overrides = JsonSerializer.Deserialize<Dictionary<string, bool>>(sub.FeatureOverrides);
            if (overrides?.TryGetValue(featureKey, out var enabled) == true)
                return enabled;
        }

        return plan.Features.Contains(featureKey);
    }

    public async Task<PlanLimitResult> CheckLimitAsync(string tenantId, LimitType limitType, CancellationToken ct)
    {
        var sub = await GetSubscriptionAsync(tenantId, ct);
        var plan = PlanCatalog.GetPlan(sub.PlanTier);

        return limitType switch
        {
            LimitType.Branches => await CheckBranchLimit(tenantId, plan, sub, ct),
            LimitType.Employees => await CheckEmployeeLimit(tenantId, plan, sub, ct),
            LimitType.SmsPerMonth => CheckSmsLimit(plan, sub),
            _ => new PlanLimitResult(true, 0, int.MaxValue, "")
        };
    }

    private async Task<PlanLimitResult> CheckBranchLimit(
        string tenantId, PlanDefinition plan, TenantSubscription sub, CancellationToken ct)
    {
        var currentCount = await _db.Branches
            .Where(b => b.TenantId == tenantId && b.IsActive)
            .CountAsync(ct);
        var max = sub.MaxBranchesOverride ?? plan.MaxBranches;
        return new PlanLimitResult(
            currentCount < max, currentCount, max,
            currentCount >= max ? $"Your {plan.Name} plan allows {max} branch(es). Upgrade to add more." : "");
    }

    // ... similar for employees and SMS
}
```

### Feature Gate Middleware

This middleware intercepts API requests and checks if the tenant's plan allows the requested feature.

```csharp
// Attribute to mark endpoints with required features
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresFeatureAttribute(string featureKey) : Attribute
{
    public string FeatureKey { get; } = featureKey;
}

// Middleware that enforces feature gates
public sealed class PlanEnforcementMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IPlanEnforcementService planService, TenantContext tenantContext)
    {
        var endpoint = context.GetEndpoint();
        var featureAttr = endpoint?.Metadata.GetMetadata<RequiresFeatureAttribute>();

        if (featureAttr != null && !string.IsNullOrEmpty(tenantContext.TenantId))
        {
            var allowed = await planService.HasFeatureAsync(
                tenantContext.TenantId, featureAttr.FeatureKey, context.RequestAborted);

            if (!allowed)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Feature not available",
                    Detail = $"The '{featureAttr.FeatureKey}' feature is not included in your current plan. Please upgrade to access this feature.",
                    Status = 403,
                    Extensions = { ["featureKey"] = featureAttr.FeatureKey }
                });
                return;
            }
        }

        await next(context);
    }
}
```

### Applying Feature Gates to Endpoints

```csharp
// In endpoint definitions — annotate with RequiresFeature

// Queue endpoints — requires Growth or Enterprise
app.MapPost("/api/v1/queue", [RequiresFeature(FeatureKeys.QueueManagement)] 
    async (AddToQueueCommand cmd, ISender sender) => ...);

// Loyalty endpoints — requires Growth or Enterprise
app.MapGet("/api/v1/customers/{id}/loyalty", [RequiresFeature(FeatureKeys.CustomerLoyalty)]
    async (string id, ISender sender) => ...);

// Cash advance endpoints — requires Growth or Enterprise
app.MapPost("/api/v1/cash-advances", [RequiresFeature(FeatureKeys.CashAdvanceTracking)]
    async (RequestCashAdvanceCommand cmd, ISender sender) => ...);

// Expense endpoints — requires Growth or Enterprise
app.MapPost("/api/v1/expenses", [RequiresFeature(FeatureKeys.ExpenseTracking)]
    async (RecordExpenseCommand cmd, ISender sender) => ...);

// Shift endpoints — requires Growth or Enterprise
app.MapPost("/api/v1/shifts/open", [RequiresFeature(FeatureKeys.ShiftManagement)]
    async (OpenShiftCommand cmd, ISender sender) => ...);

// SMS — requires Growth or Enterprise
app.MapPost("/api/v1/sms/send", [RequiresFeature(FeatureKeys.SmsNotifications)]
    async (SendSmsCommand cmd, ISender sender) => ...);

// API access — Enterprise only
app.MapGet("/api/v1/external/export", [RequiresFeature(FeatureKeys.ApiAccess)]
    async (ISender sender) => ...);

// Core endpoints — no feature gate needed (available on all plans)
app.MapPost("/api/v1/transactions", async (CreateTransactionCommand cmd, ISender sender) => ...);
app.MapGet("/api/v1/services", async (ISender sender) => ...);
```

### Limit Enforcement in Command Handlers

```csharp
// In CreateBranchCommandHandler — check branch limit before creating
public sealed class CreateBranchCommandHandler(
    IPlanEnforcementService planService,
    TenantContext tenantContext,
    // ... other deps
) : IRequestHandler<CreateBranchCommand, Result<BranchResponse>>
{
    public async Task<Result<BranchResponse>> Handle(
        CreateBranchCommand request, CancellationToken ct)
    {
        var limitCheck = await planService.CheckLimitAsync(
            tenantContext.TenantId, LimitType.Branches, ct);

        if (!limitCheck.Allowed)
            return Result<BranchResponse>.Failure(limitCheck.Message);

        // ... continue with branch creation
    }
}

// Same pattern for CreateEmployeeCommandHandler → check employee limit
// Same pattern for SendSmsCommandHandler → check SMS budget
```

---

## Layer 3: Billing & Lifecycle

### Trial Flow

```
User signs up → Tenant created → TenantSubscription created:
  PlanTier = Trial
  Status = Trial
  TrialStartDate = now
  TrialEndDate = now + 14 days
  Features = Growth-level (full experience)

Day 1-14: Full access to Growth features with Trial limits (1 branch, 5 employees)

Day 13: Hangfire sends reminder email: "Your trial ends tomorrow — choose a plan to continue"

Day 14: Trial expires.
  Status stays Trial, but TrialExpired = true.
  All API requests for gated features return 403.
  Frontend shows upgrade banner.
  READ access to existing data is preserved (they can still view, not create).

User selects a plan → Creates payment via gateway → On success:
  PlanTier = selected tier
  Status = Active
  CurrentPeriodStart = now
  CurrentPeriodEnd = now + 30 days
  NextBillingDate = now + 30 days
```

### Payment Flow (PayMongo for Philippines)

```
                    ┌─ Admin App ──────────────┐
                    │ "Upgrade to Growth"       │
                    │ [Select Plan] [Pay Now]   │
                    └──────────┬───────────────┘
                               │
                    POST /api/v1/billing/checkout
                               │
                    ┌──────────▼───────────────┐
                    │ .NET API creates a        │
                    │ PayMongo checkout session  │
                    │ with plan amount + tenant  │
                    └──────────┬───────────────┘
                               │
                    ┌──────────▼───────────────┐
                    │ User redirected to        │
                    │ PayMongo payment page      │
                    │ (GCash, Card, Bank)        │
                    └──────────┬───────────────┘
                               │
                    ┌──────────▼───────────────┐
                    │ Payment succeeds →         │
                    │ PayMongo webhook fires     │
                    │ POST /api/v1/webhooks/pay  │
                    └──────────┬───────────────┘
                               │
                    ┌──────────▼───────────────┐
                    │ Webhook handler:           │
                    │ 1. Verify signature        │
                    │ 2. Update subscription:    │
                    │    Status = Active          │
                    │    PlanTier = selected      │
                    │    Set billing dates        │
                    │ 3. Create payment record    │
                    │ 4. Send confirmation email  │
                    └─────────────────────────────┘
```

### Monthly Billing Cycle

```
Hangfire recurring job: every day at 9 AM PHT

FOR each TenantSubscription WHERE Status = Active AND NextBillingDate <= today:
  1. Create a PayMongo recurring payment (if card on file) or invoice
  2. IF payment succeeds:
     - CurrentPeriodStart = NextBillingDate
     - CurrentPeriodEnd = NextBillingDate + 30 days
     - NextBillingDate = NextBillingDate + 30 days
     - LastPaymentDate = now
     - Create billing record
  3. IF payment fails:
     - Status = PastDue
     - Send email: "Payment failed — please update your payment method"
     - Grace period: 7 days
  4. IF PastDue for > 7 days:
     - Status = Suspended
     - Send email: "Account suspended — pay now to reactivate"
     - All gated features return 403 (core features remain read-only)
```

### Domain Models for Billing

```csharp
public sealed class BillingRecord : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PHP";
    public BillingType Type { get; set; }
    public BillingStatus Status { get; set; }
    public string? PaymentGatewayId { get; set; }     // PayMongo payment ID
    public string? PaymentMethod { get; set; }         // gcash, card, bank_transfer
    public string? InvoiceNumber { get; set; }         // INV-2026-0001
    public DateTime BillingDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public TenantSubscription Subscription { get; set; } = null!;
}

public enum BillingType { Subscription, Upgrade, Downgrade, Manual }
public enum BillingStatus { Pending, Paid, Failed, Refunded, Voided }

public sealed class PlanChangeLog : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public PlanTier FromPlan { get; set; }
    public PlanTier ToPlan { get; set; }
    public string ChangedBy { get; set; } = string.Empty;  // UserId or "system"
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## Frontend Implementation

### Plan-Aware API Client

```typescript
// types/subscription.ts
export interface TenantPlan {
  tier: 'trial' | 'starter' | 'growth' | 'enterprise'
  status: 'trial' | 'active' | 'past_due' | 'suspended' | 'cancelled'
  features: string[]
  limits: {
    maxBranches: number
    maxEmployees: number
    smsPerMonth: number
  }
  trial?: {
    startDate: string
    endDate: string
    daysRemaining: number
    expired: boolean
  }
  billing?: {
    nextBillingDate: string
    lastPaymentDate: string
  }
}

// hooks/use-plan.ts
export function usePlan() {
  return useQuery({
    queryKey: ['plan'],
    queryFn: () => apiClient<TenantPlan>('/api/v1/billing/plan'),
    staleTime: 60_000, // Refresh every minute
  })
}

export function useHasFeature(featureKey: string): boolean {
  const { data: plan } = usePlan()
  if (!plan) return false
  if (plan.status === 'suspended') return false
  if (plan.trial?.expired) return false
  return plan.features.includes(featureKey)
}
```

### Feature Gate Component

```typescript
// components/feature-gate.tsx
'use client'
import { useHasFeature, usePlan } from '@/hooks/use-plan'

interface FeatureGateProps {
  feature: string
  children: React.ReactNode
  fallback?: React.ReactNode  // What to show when feature is locked
}

export function FeatureGate({ feature, children, fallback }: FeatureGateProps) {
  const hasFeature = useHasFeature(feature)
  const { data: plan } = usePlan()

  if (hasFeature) return <>{children}</>

  // Default fallback: upgrade prompt
  return fallback ?? (
    <div className="border-2 border-dashed border-amber-200 bg-amber-50 rounded-xl p-6 text-center">
      <p className="text-amber-800 font-semibold mb-1">
        Upgrade to unlock this feature
      </p>
      <p className="text-amber-600 text-sm mb-3">
        This feature is available on the Growth plan and above.
      </p>
      <a href="/settings/billing" className="text-sm font-semibold text-splash-600 hover:underline">
        View Plans →
      </a>
    </div>
  )
}

// Usage in admin pages:
<FeatureGate feature="queue_management">
  <QueueBoard />
</FeatureGate>

<FeatureGate feature="expense_tracking">
  <ExpenseDashboard />
</FeatureGate>
```

### Sidebar Navigation Gating

```typescript
// In the admin sidebar — hide or badge locked features
const navItems = [
  { label: 'Dashboard', href: '/', icon: LayoutDashboard, feature: null },  // always visible
  { label: 'Branches', href: '/branches', icon: Building, feature: null },
  { label: 'Services', href: '/services', icon: Wrench, feature: null },
  { label: 'Employees', href: '/employees', icon: Users, feature: null },
  { label: 'Payroll', href: '/payroll', icon: CreditCard, feature: null },
  { label: 'Transactions', href: '/transactions', icon: Receipt, feature: null },
  // Growth features — show but with lock icon if not available
  { label: 'Queue', href: '/queue', icon: ListOrdered, feature: 'queue_management' },
  { label: 'Expenses', href: '/expenses', icon: Coins, feature: 'expense_tracking' },
  { label: 'Loyalty', href: '/loyalty', icon: Heart, feature: 'customer_loyalty' },
  { label: 'Cash Advances', href: '/cash-advances', icon: Banknote, feature: 'cash_advance_tracking' },
  { label: 'Reports', href: '/reports', icon: BarChart, feature: null },
]

// Render:
{navItems.map(item => {
  const locked = item.feature && !hasFeature(item.feature)
  return (
    <NavLink href={locked ? '#' : item.href} disabled={locked}>
      <item.icon />
      {item.label}
      {locked && <LockIcon className="ml-auto text-gray-400 w-3.5" />}
    </NavLink>
  )
})}
```

### Trial Banner

```typescript
// components/trial-banner.tsx — shown at the top of all pages during trial
export function TrialBanner() {
  const { data: plan } = usePlan()
  if (!plan || plan.status !== 'trial') return null

  const daysLeft = plan.trial?.daysRemaining ?? 0
  const expired = plan.trial?.expired

  if (expired) {
    return (
      <div className="bg-red-600 text-white text-center py-2 text-sm font-medium">
        Your free trial has ended. <a href="/settings/billing" className="underline font-bold">Choose a plan</a> to continue using SplashSphere.
      </div>
    )
  }

  return (
    <div className="bg-amber-500 text-white text-center py-2 text-sm font-medium">
      Free trial: {daysLeft} day{daysLeft !== 1 ? 's' : ''} remaining.
      <a href="/settings/billing" className="underline font-bold ml-2">Upgrade now</a>
    </div>
  )
}
```

### Billing Settings Page

```
/settings/billing — accessible on all plans

Current Plan card:
  [Growth Plan — ₱2,999/month]
  Status: Active
  Next billing: April 21, 2026
  [Change Plan] [Cancel Subscription]

Plan Comparison:
  Three cards (Starter / Growth / Enterprise) — same as marketing page
  Current plan highlighted. Upgrade/downgrade buttons on others.

Payment History:
  Table: Invoice #, Date, Amount, Status, Payment Method
  [Download Invoice] button per row

Payment Method:
  Current: GCash ending in •••1234
  [Update Payment Method] → opens PayMongo portal
```

---

## API Endpoints (Billing)

| Method | Route | Description |
|---|---|---|
| `GET` | `/billing/plan` | Get current tenant's plan, features, limits, trial status |
| `POST` | `/billing/checkout` | Create PayMongo checkout session for plan upgrade |
| `POST` | `/billing/change-plan` | Request plan change (upgrade/downgrade) |
| `POST` | `/billing/cancel` | Cancel subscription (effective at period end) |
| `GET` | `/billing/history` | Payment history for the tenant |
| `GET` | `/billing/invoices/{id}` | Download invoice PDF |
| `POST` | `/webhooks/payment` | PayMongo webhook (payment success/failure) |

---

## SaaS Super-Admin (Blazor) Integration

The existing Blazor SaaS admin dashboard manages subscriptions from the LezanobTech side:

| Page | Functionality |
|---|---|
| `/tenants` | View all tenants with plan, status, MRR |
| `/tenants/{id}` | Override plan, features, limits. Manually activate/suspend. |
| `/billing` | Generate invoices, mark as paid, track MRR |
| `/features` | Toggle individual features per tenant (overrides) |

The SaaS admin writes to the same `TenantSubscription` and `BillingRecord` tables. Changes take effect immediately because the `PlanEnforcementService` reads from the database (cached for 60 seconds via HybridCache).

---

## Hangfire Jobs for Billing

| Job | Schedule | Description |
|---|---|---|
| `ProcessDailyBilling` | Daily 9 AM PHT | Check for subscriptions due, attempt payment, handle failures |
| `SendTrialExpiryReminder` | Daily 9 AM PHT | Send email to tenants with trial expiring in 1-3 days |
| `SuspendOverdueAccounts` | Daily 9 AM PHT | Suspend accounts that are PastDue for > 7 days |
| `ResetMonthlySmsCount` | 1st of each month | Reset `SmsUsedThisMonth` to 0 for all subscriptions |
| `GenerateMonthlyInvoices` | 1st of each month | Generate invoice records for the previous month |

---

## Claude Code Prompts — Phase 16

### Prompt 16.1 — Subscription Domain + Infrastructure

```
Add the Subscription system to SplashSphere:

Domain:
- FeatureKeys static class with all feature key constants
- PlanTier enum (Trial, Starter, Growth, Enterprise)
- PlanCatalog static class with PlanDefinition records for each tier
  (exact features, limits, and pricing from the marketing page)
- TenantSubscription entity with all fields from the spec
- BillingRecord entity
- PlanChangeLog entity
- SubscriptionStatus enum, BillingType enum, BillingStatus enum

Infrastructure:
- TenantSubscriptionConfiguration (one-to-one with Tenant)
- BillingRecordConfiguration
- PlanChangeLogConfiguration
- Migration: "AddSubscriptions"

Update the DataSeeder: when creating the seed tenant, also create a
TenantSubscription with PlanTier = Growth, Status = Active for development.

Update Tenant entity: add TenantSubscription navigation property.
```

### Prompt 16.2 — Plan Enforcement Service + Middleware

```
Build the plan enforcement layer:

Application/Interfaces/:
- IPlanEnforcementService with HasFeatureAsync, CheckLimitAsync, 
  GetActivePlanAsync, GetSmsBudgetRemainingAsync

Infrastructure/Authentication/:
- PlanEnforcementService implementation:
  - Reads TenantSubscription from DB (cached 60 seconds via HybridCache)
  - Checks features against PlanCatalog
  - Handles FeatureOverrides JSON
  - Handles Suspended/TrialExpired states
  - Limit checks for branches, employees, SMS

API/Middleware/:
- RequiresFeatureAttribute
- PlanEnforcementMiddleware — reads attribute from endpoint metadata,
  checks via IPlanEnforcementService, returns 403 ProblemDetails if blocked

Register middleware in Program.cs AFTER TenantResolutionMiddleware.

Apply [RequiresFeature(...)] to ALL gated endpoints:
- Queue endpoints → FeatureKeys.QueueManagement
- Loyalty endpoints → FeatureKeys.CustomerLoyalty
- Cash advance endpoints → FeatureKeys.CashAdvanceTracking
- Expense endpoints → FeatureKeys.ExpenseTracking
- Shift endpoints → FeatureKeys.ShiftManagement
- P&L report endpoint → FeatureKeys.ProfitLossReports
- SMS endpoints → FeatureKeys.SmsNotifications
- API export endpoints → FeatureKeys.ApiAccess

Update CreateBranchCommandHandler to check branch limit.
Update CreateEmployeeCommandHandler to check employee limit.
Update SendSmsCommandHandler to check SMS budget and increment counter.
```

### Prompt 16.3 — Billing Commands + PayMongo Integration

```
Build the billing and subscription management:

Application/Features/Billing/:
- GetCurrentPlanQuery → returns plan details, features, limits, trial info
- CreateCheckoutCommand(PlanTier targetPlan) → creates PayMongo checkout session URL
- ChangePlanCommand(PlanTier newPlan) → handles upgrade/downgrade logic
- CancelSubscriptionCommand → marks for cancellation at period end
- GetBillingHistoryQuery → paginated billing records
- ProcessPaymentWebhookCommand → handles PayMongo webhook payload

Infrastructure/ExternalServices/:
- IPaymentGateway interface
- PayMongoPaymentGateway implementation:
  POST to PayMongo API to create checkout sessions
  Webhook signature verification
  (For development: create a MockPaymentGateway that auto-succeeds)

Infrastructure/BackgroundJobs/:
- BillingJobService:
  ProcessDailyBilling, SendTrialExpiryReminder, 
  SuspendOverdueAccounts, ResetMonthlySmsCount

API/Endpoints/BillingEndpoints.cs — all billing routes
API/Endpoints/PaymentWebhookEndpoints.cs — webhook handler (no auth)

Register Hangfire billing jobs in Program.cs.
Env vars: PayMongo__SecretKey, PayMongo__PublicKey, PayMongo__WebhookSecret
```

### Prompt 16.4 — Frontend Plan Awareness (Admin + POS)

```
Add plan awareness to both frontend apps:

Shared types (packages/types/):
- TenantPlan interface with tier, status, features, limits, trial info

Admin App:
1. hooks/use-plan.ts — useQuery for /billing/plan, useHasFeature helper
2. components/feature-gate.tsx — wraps content, shows upgrade prompt if locked
3. components/trial-banner.tsx — top banner during trial with days remaining
4. Update sidebar: show lock icon on gated nav items for Starter plan
5. Wrap all Growth/Enterprise pages with <FeatureGate>:
   Queue, Expenses, Loyalty, Cash Advances, Shift/Cash Reconciliation pages
6. /settings/billing — billing settings page:
   Current plan card, plan comparison cards (from marketing pricing),
   payment history table, change plan button, cancel button

POS App:
1. Same use-plan hook and feature gate
2. Hide "Queue" pill if queue_management not in features
3. Hide "Cash In/Out" shift features if shift_management not in features
4. Show trial banner if in trial
5. Block transactions if subscription is Suspended (show upgrade prompt)
```

---

## Feature Gating Summary

| Feature Key | Starter | Growth | Enterprise | Gated Where |
|---|---|---|---|---|
| `pos` | ✓ | ✓ | ✓ | Not gated |
| `commission_tracking` | ✓ | ✓ | ✓ | Not gated |
| `weekly_payroll` | ✓ | ✓ | ✓ | Not gated |
| `basic_reports` | ✓ | ✓ | ✓ | Not gated |
| `customer_management` | ✓ | ✓ | ✓ | Not gated |
| `vehicle_management` | ✓ | ✓ | ✓ | Not gated |
| `employee_management` | ✓ | ✓ | ✓ | Not gated |
| `merchandise_management` | ✓ | ✓ | ✓ | Not gated |
| `queue_management` | — | ✓ | ✓ | API + sidebar + POS nav |
| `customer_loyalty` | — | ✓ | ✓ | API + sidebar + POS lookup |
| `cash_advance_tracking` | — | ✓ | ✓ | API + sidebar |
| `expense_tracking` | — | ✓ | ✓ | API + sidebar |
| `shift_management` | — | ✓ | ✓ | API + sidebar + POS nav |
| `profit_loss_reports` | — | ✓ | ✓ | API + reports page |
| `sms_notifications` | — | ✓ | ✓ | API + settings |
| `pricing_modifiers` | — | ✓ | ✓ | API + settings |
| `api_access` | — | — | ✓ | API endpoints |
| `custom_integrations` | — | — | ✓ | API + settings |

---

## Key Design Decisions

1. **Trial = Growth features.** Tenants experience the full product during trial so they see the value before choosing a plan. If they pick Starter, Growth features disappear — creating natural upgrade pressure.

2. **Feature gates are on the API, not just the frontend.** Even if someone bypasses the UI, the API rejects requests for gated features. Defense in depth.

3. **Limits are checked in command handlers, not middleware.** Branch/employee limits need database counts, which are better done in the handler context with proper async/await.

4. **SaaS admin can override anything.** Feature overrides per tenant allow granting exceptions (e.g., "give this big client queue management for free during their first month"). This is stored as JSON on the subscription record.

5. **Suspended ≠ deleted.** Suspended tenants can still log in and VIEW their data (read-only). They just can't create new transactions or use gated features. This prevents data loss disputes and gives them a reason to pay (they can see their data is still there).

6. **PayMongo for PH payments.** Supports GCash, Maya, credit/debit cards, and bank transfers. Lower fees than Stripe for local payments. Webhook-based confirmation.

7. **Billing records are separate from payment gateway.** Even if you switch from PayMongo to Stripe later, the billing records and subscription logic don't change.
