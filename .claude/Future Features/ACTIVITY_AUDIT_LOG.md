# SplashSphere — Activity & Audit Log (Per-Tenant)

> **Phase:** 23.7 (Value-Add). Can build alongside any phase — it's infrastructure.
> **Plan gating:** Available on all plans (last 7 days). Growth+: 90 days. Enterprise: 1 year.

---

## What It Does

Tracks every significant action in the tenant's system: who did what, when, and what changed. The owner opens `/activity-log` and sees a chronological record of everything that happened across their business. This answers questions like "who changed the price?", "who voided that transaction?", "when was this employee added?", and "who approved that cash advance?"

This is NOT the same as the Super Admin audit log (which tracks LezanobTech platform actions). This is the tenant's own activity history.

---

## What Gets Logged

| Category | Actions Logged |
|---|---|
| **Transactions** | Created, completed, voided, refunded |
| **Services** | Created, updated (name, price), deactivated |
| **Pricing** | Service pricing matrix changed, commission matrix changed |
| **Employees** | Added, updated, deactivated, type changed |
| **Payroll** | Period closed, processed, entry modified |
| **Cash Advances** | Requested, approved, rejected, deduction applied |
| **Shifts** | Opened, closed, flagged, reviewed |
| **Cash Movements** | Cash in/out recorded |
| **Inventory** | Stock adjusted, PO received, supply usage recorded |
| **Customers** | Created, updated, merged |
| **Vehicles** | Added, updated, plate changed |
| **Expenses** | Created, approved, rejected |
| **Settings** | Plan changed, feature toggled, branch added/updated |
| **Users** | Invited, role changed, removed |
| **Promos** | Created, deactivated, redeemed |
| **Queue** | Customer called, no-show marked |

---

## Domain Model

```csharp
public sealed class ActivityLog
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;        // Denormalized for display
    public string Action { get; set; } = string.Empty;          // "service.price_updated"
    public string Category { get; set; } = string.Empty;        // "services", "transactions", etc.
    public string Description { get; set; } = string.Empty;     // Human-readable: "Updated Basic Wash price from ₱200 to ₱220"
    public string? EntityType { get; set; }                     // "Service", "Transaction", "Employee"
    public string? EntityId { get; set; }                       // The affected record's ID
    public string? OldValues { get; set; }                      // JSON: { "basePrice": 200 }
    public string? NewValues { get; set; }                      // JSON: { "basePrice": 220 }
    public string? IpAddress { get; set; }
    public DateTime PerformedAt { get; set; }
}
```

---

## Implementation: EF Core Interceptor

Instead of manually logging in every command handler, use an EF Core `SaveChangesInterceptor` that automatically detects changes:

```csharp
public class AuditLogInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, 
        CancellationToken ct)
    {
        var context = eventData.Context;
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => IsAuditable(e.Entity));

        foreach (var entry in entries)
        {
            var log = new ActivityLog
            {
                TenantId = _tenantContext.TenantId,
                UserId = _userContext.UserId,
                UserName = _userContext.UserName,
                Action = ResolveAction(entry),
                Category = ResolveCategory(entry.Entity),
                Description = BuildDescription(entry),
                EntityType = entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                OldValues = entry.State == EntityState.Modified 
                    ? JsonSerializer.Serialize(GetOriginalValues(entry)) : null,
                NewValues = JsonSerializer.Serialize(GetCurrentValues(entry)),
                PerformedAt = DateTime.UtcNow
            };
            context.Set<ActivityLog>().Add(log);
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
```

For critical actions that need custom descriptions (like "voided transaction #TXN-0312 — reason: customer complaint"), log explicitly in the command handler using `IAuditLogger.Log()`.

---

## Admin UI

### Activity Log Page (`/activity-log`)

```
┌── Activity Log ───────────────────────────────────────┐
│                                                        │
│  Filter: [All categories ▾] [All users ▾] [Last 7d ▾]│
│  🔍 [Search...                                      ] │
│                                                        │
│  TODAY                                                 │
│  11:45 AM  Ana R. voided transaction #TXN-0312        │
│            Reason: "Customer complaint — service redo" │
│            [View Transaction]                          │
│                                                        │
│  10:30 AM  Juan R. updated Basic Wash price            │
│            ₱200 → ₱220 (Sedan/Medium)                 │
│            [View Service]                              │
│                                                        │
│  9:15 AM   Juan R. approved cash advance ₱2,000       │
│            For: Pedro S.                               │
│            [View Cash Advance]                         │
│                                                        │
│  8:02 AM   System opened shift for Ana R.              │
│            Opening fund: ₱5,000                        │
│                                                        │
│  YESTERDAY                                             │
│  5:45 PM   Juan R. processed payroll Week 12           │
│            Total: ₱48,200 for 8 employees              │
│            [View Payroll]                              │
│                                                        │
│  [Load More]                                           │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**Filters:**
- Category dropdown: Transactions, Services, Employees, Payroll, Inventory, etc.
- User dropdown: all users who've performed actions
- Date range: Last 7 days, Last 30 days, Custom range
- Search: free-text search in description

**Each entry shows:** timestamp, user name, human-readable description, and a link to the affected entity.

**Old/New values:** Expandable row showing what changed (for updates):
```
▼ Updated Basic Wash price
  basePrice:  ₱200 → ₱220
  Modified by: Juan R. at 10:30 AM
```

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/activity-log` | List activity entries (paginated, filterable by category, user, date range, search) |
| `GET` | `/activity-log/entity/{type}/{id}` | All activity for a specific entity (e.g., all changes to Service X) |

**Read-only.** Activity logs cannot be modified or deleted by tenants. Retention managed by Hangfire cleanup job.

---

## Retention by Plan

| Plan | Retention |
|---|---|
| Starter | 7 days |
| Growth | 90 days |
| Enterprise | 1 year |

Hangfire job `CleanupActivityLogs` runs weekly, deletes entries older than the tenant's plan retention.

---

## Claude Code Prompt

```
Build the Activity & Audit Log:

Domain: ActivityLog entity
Infrastructure: AuditLogInterceptor (EF Core SaveChangesInterceptor)
  Auto-logs: Added, Modified, Deleted entities that implement IAuditableEntity.
  Captures: old values, new values, user context, timestamp.

Application: IAuditLogger interface for explicit logging in command handlers.
  Use for: transaction voids, payroll processing, cash advance approvals,
  shift flags — anything needing a custom description.

  GetActivityLogQuery: paginated, filter by category/user/dateRange/search.
  GetEntityActivityQuery: all logs for a specific entity.

Admin: /activity-log page with filterable timeline.
  Expandable rows showing old→new value diffs.
  Entity links to navigate to the affected record.

Hangfire: CleanupActivityLogs (weekly) — delete entries beyond plan retention.
Plan gating: All plans (7d Starter, 90d Growth, 1yr Enterprise).
```
