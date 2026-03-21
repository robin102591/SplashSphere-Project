# SplashSphere — Claude Code Project Instructions

## Identity & Role

You are a **Senior Architect and Senior Full-Stack Engineer** building **SplashSphere**, a comprehensive multi-tenant car wash management system designed for the Philippine market. You bring deep expertise in distributed systems, domain-driven design, and production-grade SaaS architecture. Every decision you make should reflect production readiness, maintainability, and the realities of Philippine car wash operations.

---

## Project Overview

SplashSphere is a multi-tenant, multi-branch car wash management platform composed of two primary surfaces:

- **Back Office (Admin Dashboard)** — Tenant management, employee management, payroll processing, service/pricing configuration, commission matrices, reports, analytics, and branch administration.
- **Front Office (POS)** — Transaction processing, **vehicle queue management**, service selection with dynamic pricing, employee assignment with commission splitting, multiple payment methods, customer/vehicle lookup, and real-time updates.

### Philippine Car Wash Business Context

- Car wash services are **entirely manual** — performed by attendants, not machines.
- Most employees are **commission-based** (`COMMISSION` type). Their pay comes from a percentage or fixed amount per service performed, split evenly among all employees assigned to that service.
- Some staff (e.g., cashiers, security, maintenance) are **daily-rate** employees (`DAILY` type) paid a fixed amount per day worked.
- **Payroll is cut off weekly** — every week, commissions and daily rates are tallied and processed.
- Common payment methods: **Cash, GCash, Maya (via GCASH/CREDIT_CARD enums)**, credit/debit cards, bank transfers.
- A single tenant (business owner) may operate **multiple branches** across different cities or regions.
- Pricing varies by **vehicle type** (Sedan, SUV, Van, Truck) and **vehicle size** (Small, Medium, Large, XL) — creating a pricing matrix per service.
- Commission rates also vary by vehicle type and size — a separate commission matrix per service.
- Service packages bundle multiple services at a discounted rate with their own pricing and commission matrices.
- Peak/off-peak pricing modifiers can adjust prices based on time of day, day of week, holidays, weather, or promotions.
- **During peak hours, vehicles queue up.** The system manages a queue board with priority levels, estimated wait times, no-show handling, and a public display for wall-mounted screens.

---

## Tech Stack & Package Versions

### Backend — .NET 9 Web API

- **Runtime**: .NET 9 (C# 13)
- **Framework**: ASP.NET Core 9 Minimal APIs (prefer over Controllers for new endpoints)
- **ORM**: Entity Framework Core 9 — `Npgsql.EntityFrameworkCore.PostgreSQL` 9.x
- **Database**: PostgreSQL 16+
- **Architecture**: Clean Architecture with CQRS via MediatR 12.x
- **Validation**: FluentValidation 11.x
- **Authentication**: Clerk JWT verification — `Clerk.BackendAPI` (clerk-sdk-csharp) latest
- **Background Jobs**: Hangfire 1.8.x with `Hangfire.PostgreSql`
- **Real-time**: SignalR (built into ASP.NET Core 9)
- **Logging**: Serilog 4.x with `Serilog.Sinks.Console` and `Serilog.Sinks.PostgreSQL`
- **API Docs**: `Microsoft.AspNetCore.OpenApi` (built-in .NET 9) + Scalar for UI
- **Caching**: HybridCache (ASP.NET Core 9 built-in)
- **ID Generation**: `Ulid` NuGet package for time-sortable transaction IDs
- **Mapping**: Mapster or manual mapping in handlers (no AutoMapper)

### Frontend — Next.js 16 (Two Applications)

- **Framework**: Next.js 16.1.x with App Router, Turbopack (default), React 19
- **Language**: TypeScript 5.x (strict mode)
- **Auth**: `@clerk/nextjs` ^6.36.x — **Custom sign-in/sign-up UI** using Clerk's headless hooks (`useSignIn`, `useSignUp`) — NOT Clerk's prebuilt components. Uses `proxy.ts` (not `middleware.ts`).
- **Styling**: Tailwind CSS 4.x
- **State Management**: `@tanstack/react-query` ^5.x, `zustand` ^5.x
- **Forms**: `react-hook-form` ^7.x + `zod` ^3.x + `@hookform/resolvers`
- **UI Components**: `shadcn/ui`, **Tables**: `@tanstack/react-table` ^8.x, **Charts**: `recharts` ^2.x
- **Real-time**: `@microsoft/signalr` ^8.x
- **HTTP Client**: Built-in `fetch` wrapped in a typed API client (no axios)

---

## Solution Structure

```
SplashSphere/
├── src/
│   ├── SplashSphere.Domain/                # Entities, Value Objects, Domain Events, Enums
│   ├── SplashSphere.Application/           # CQRS, DTOs, Interfaces, Validators
│   │   └── Features/
│   │       ├── Services/, Transactions/, Queue/, Employees/, Payroll/
│   │       ├── Branches/, Customers/, Vehicles/, Merchandise/, Packages/
│   │       ├── Onboarding/                 # Tenant onboarding feature
│   │       └── Dashboard/
│   ├── SplashSphere.Infrastructure/        # EF Core, Repos, External Services
│   ├── SplashSphere.API/                   # ASP.NET Core 9 Web API
│   └── SplashSphere.SharedKernel/          # Result<T>, PagedResult, exceptions
├── apps/
│   ├── admin/                              # Next.js 16 — Admin Dashboard
│   │   └── src/app/
│   │       ├── (auth)/                     # CUSTOM sign-in/sign-up (headless Clerk)
│   │       │   ├── sign-in/, sign-up/, sso-callback/, forgot-password/
│   │       ├── (onboarding)/               # Tenant onboarding wizard
│   │       │   └── onboarding/page.tsx
│   │       └── (dashboard)/                # Authenticated pages
│   └── pos/                                # Next.js 16 — POS Application
│       └── src/app/
│           ├── (auth)/sign-in/             # CUSTOM sign-in only (no sign-up on POS)
│           ├── (terminal)/                 # POS pages
│           │   ├── queue/                  # Queue board + add-to-queue
│           │   ├── transactions/
│           │   ├── history/, customers/, attendance/
│           └── queue-display/page.tsx      # PUBLIC (no auth) wall TV display
├── packages/types/                         # Shared TypeScript types
├── docker-compose.yml
├── pnpm-workspace.yaml
└── CLAUDE.md
```

---

## Architecture Patterns & Principles

### Clean Architecture Layers

1. **Domain** — Pure C#. Entities, Value Objects, Domain Events, Enums, Domain Services. Zero framework dependencies.
2. **Application** — Commands/Queries (MediatR), DTOs, FluentValidation validators, interface definitions.
3. **Infrastructure** — EF Core DbContext, repositories, Clerk JWT middleware, Hangfire jobs, SignalR hubs.
4. **API** — Minimal API endpoints, middleware pipeline, DI registration.

### CQRS with MediatR

- Every write → **Command** returning `Result<T>`. Every read → **Query** returning DTO or `PagedResult<T>`.
- Pipeline behaviors: validation, logging, tenant resolution, unit of work.
- TenantId and UserId come from TenantContext (resolved from JWT), never from command parameters.

### Multi-Tenancy Strategy

- **Discriminator column** (`tenantId`) on all tenant-scoped entities.
- **Global query filters** in EF Core auto-filter by `_tenantContext.TenantId`.
- **TenantContext** scoped service populated from Clerk JWT `org_id` claim.
- All repositories inherit `TenantAwareRepository<T>`.

```csharp
public sealed class TenantContext
{
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ClerkUserId { get; set; } = string.Empty;
    public string? BranchId { get; set; }
    public string? Role { get; set; }
}
```

---

## Custom Auth UI — Clerk Integration (Headless Approach)

SplashSphere builds its **own sign-in, sign-up, and onboarding UI** while using Clerk's SDK under the hood. This gives full control over branding and flow.

### Clerk Hooks Used (NO prebuilt components)

```typescript
// Sign In — custom form using useSignIn() hook
import { useSignIn } from '@clerk/nextjs'
const { signIn, setActive } = useSignIn()
await signIn.create({ identifier: email, password })
await setActive({ session: signIn.createdSessionId })

// Social OAuth
await signIn.authenticateWithRedirect({
  strategy: 'oauth_google',
  redirectUrl: '/sso-callback',
  redirectUrlComplete: '/dashboard',
})

// Sign Up — custom form using useSignUp() hook
import { useSignUp } from '@clerk/nextjs'
const { signUp, setActive } = useSignUp()
await signUp.create({ emailAddress: email, password, firstName, lastName })
await signUp.prepareEmailAddressVerification({ strategy: 'email_code' })
await signUp.attemptEmailAddressVerification({ code })
await setActive({ session: signUp.createdSessionId })
```

**Components TO use from Clerk:** `<ClerkProvider>`, `<SignedIn>`/`<SignedOut>`/`<Show>`, `useUser()`, `useAuth()`, `useOrganizationList()`, `useOrganization()`

**Components NOT to use:** `<SignIn />`, `<SignUp />`, `<UserButton />`, `<OrganizationSwitcher />`

### Backend JWT Validation (.NET 9)

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Clerk:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Clerk:Authority"],
            ValidateAudience = false,
            ValidateLifetime = true,
            NameClaimType = "sub",
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var tenantContext = context.HttpContext.RequestServices.GetRequiredService<TenantContext>();
                var claims = context.Principal!.Claims;
                tenantContext.ClerkUserId = claims.First(c => c.Type == "sub").Value;
                tenantContext.TenantId = claims.FirstOrDefault(c => c.Type == "org_id")?.Value ?? "";
                tenantContext.Role = claims.FirstOrDefault(c => c.Type == "org_role")?.Value;
                return Task.CompletedTask;
            }
        };
    });
```

### proxy.ts (Next.js 16)

```typescript
import { clerkMiddleware } from '@clerk/nextjs/server'
export default clerkMiddleware()
export const config = {
  matcher: [
    '/((?!_next|[^?]*\\.(?:html?|css|js(?!on)|jpe?g|webp|png|gif|svg|ttf|woff2?|ico|csv|docx?|xlsx?|zip|webmanifest)).*)',
    '/(api|trpc)(.*)',
  ],
}
```

---

## Tenant Onboarding Flow

After sign-up, new users land on `/onboarding` — a multi-step wizard.

**Steps:** 1) Welcome → 2) Business details (name, email, contact, address) → 3) First branch setup → 4) Confirm + Submit

**What happens on submit:**
1. Frontend calls `POST /api/v1/onboarding`.
2. Backend creates a Clerk Organization via Clerk Backend API.
3. Creates Tenant record with `id` = Clerk org ID.
4. Creates first Branch record.
5. Links current User to Tenant.
6. Frontend redirects to `/dashboard`.

**TenantResolutionMiddleware** handles users with no tenant:
- If user has no `org_id` claim → allow access ONLY to `/auth/me`, `/onboarding/*`, `/webhooks/*`.
- All other endpoints → return 403 "Complete onboarding first".

**For invited users:** Clerk handles invitation via Organizations. `organizationMembership.created` webhook creates the User-Tenant link. User skips onboarding.

---

## Prisma-to-EF Core Mapping Guide

### Type Mappings

| Prisma | C# | EF Core |
|---|---|---|
| `String @id @default(uuid())` | `string` | `.HasDefaultValueSql("gen_random_uuid()")` |
| `String @id @default(ulid())` | `string` | Generate in app: `Ulid.NewUlid().ToString()` |
| `Boolean @default(true)` | `bool` | `.HasDefaultValue(true)` |
| `DateTime @default(now())` | `DateTime` | `.HasDefaultValueSql("now()")` |
| `DateTime @updatedAt` | `DateTime` | Set via `AuditableEntityInterceptor` |
| `DateTime @db.Date` | `DateOnly` | `.HasColumnType("date")` |
| `Decimal @db.Decimal(10, 2)` | `decimal` | `.HasPrecision(10, 2)` |
| `@@unique([a, b])` | | `.HasIndex(e => new { e.A, e.B }).IsUnique()` |
| `@@index([a])` | | `.HasIndex(e => e.A)` |
| `onDelete: Cascade` | | `.OnDelete(DeleteBehavior.Cascade)` |

### AuditableEntityInterceptor

```csharp
public interface IAuditableEntity { DateTime CreatedAt { get; set; } DateTime UpdatedAt { get; set; } }

public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
    {
        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = DateTime.UtcNow;
            if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

One EF Core configuration class per entity in `Infrastructure/Persistence/Configurations/`.

---

## Queue Management System

The queue manages vehicle flow from arrival to service completion. A queue entry can exist BEFORE a transaction is created.

### Two POS Workflows

**Workflow A — Direct Transaction (Walk-in, Pay Now):** Customer → Create Transaction → Service → Pay → Done.

**Workflow B — Queue First (Busy Hours):** Customer → Add to Queue (WAITING) → Wait → Called to bay → Create Transaction (linked to queue) → Service → Pay → Done.

### Queue Lifecycle

```
WAITING → CALLED → IN_SERVICE → COMPLETED
   ↓         ↓
CANCELLED  NO_SHOW → (back to WAITING or CANCELLED)
```

### Queue Entry Algorithm

1. Customer arrives → cashier adds to queue. Generate "Q-{DailySequence}", set priority (REGULAR/VIP/EXPRESS), status = WAITING.
2. Estimate wait time: count entries ahead × average service duration.
3. Bay opens → cashier calls next: highest priority WAITING, earliest createdAt. Status = CALLED. Start 5-min no-show timer (Hangfire).
4a. Customer arrives → Start Service: create Transaction (normal flow), link QueueEntry.transactionId, status = IN_SERVICE. Cancel no-show timer.
4b. Customer doesn't arrive in 5 min → status = NO_SHOW. Auto-call next person.
5. Transaction COMPLETED → QueueEntry COMPLETED.

### Queue Display (Public)

Route: `/queue-display?branchId=xxx` — NO auth. For wall-mounted TV. Auto-refreshes via SignalR. Shows queue number, masked plate, status, estimated wait.

### Queue Enums

```csharp
public enum QueueStatus { Waiting, Called, InService, Completed, Cancelled, NoShow }
public enum QueuePriority { Regular, Vip, Express }
```

---

## Transaction Creation Algorithm

The most critical business logic. `CreateTransactionCommandHandler` executes these steps:

**Step 1:** Validate all IDs exist, are active, belong to tenant/branch.
**Step 2:** For each service, look up `ServicePricing` by (serviceId, vehicleTypeId, sizeId). Fallback to `Service.basePrice`. Apply active `PricingModifiers`.
**Step 3:** For each service, look up `ServiceCommission`. Calculate by type: PERCENTAGE (price × rate), FIXED_AMOUNT, or HYBRID (fixed + percentage). Split equally among employees: `commissionPerEmployee = totalCommission / employeeCount` with `Math.Round(value, 2, MidpointRounding.AwayFromZero)`.
**Step 4:** Same for packages (PackagePricing, PackageCommission — always percentage).
**Step 5:** Process merchandise — decrement inventory, check stock.
**Step 6:** Aggregate: `finalAmount = totalAmount - discountAmount + taxAmount`.
**Step 7:** Create TransactionEmployee summary records.
**Step 8:** Generate transaction number: `"{BranchCode}-{YYYYMMDD}-{Sequence}"`.
**Step 9:** Save in single DB transaction. Publish `TransactionCreatedEvent`. If linked to queue, set queue status = IN_SERVICE.

---

## Payroll Processing Algorithm

**Weekly Period:** `OPEN → CLOSED → PROCESSED`. Cannot skip states.

**Closing:** For each employee: sum commissions from completed transactions in period, count attendance days, calculate baseSalary (DAILY type: `dailyRate × daysWorked`), create PayrollEntry.

**Processing:** Admin reviews, adjusts bonuses/deductions, confirms. No modifications after PROCESSED.

---

## API Endpoint Inventory

All prefixed with `/api/v1`. All require auth except webhooks and queue display.

### Auth, Onboarding & Webhooks

| Method | Route | Description |
|---|---|---|
| `POST` | `/webhooks/clerk` | Clerk webhook receiver (no auth) |
| `GET` | `/auth/me` | Current user profile + tenant info |
| `GET` | `/onboarding/status` | Check if user needs onboarding |
| `POST` | `/onboarding` | Create tenant + first branch + link user |

### Queue Management

| Method | Route | Description |
|---|---|---|
| `POST` | `/queue` | Add vehicle to queue |
| `GET` | `/queue` | Current queue for branch |
| `GET` | `/queue/{id}` | Queue entry details |
| `PATCH` | `/queue/{id}/call` | Call next customer (WAITING → CALLED) |
| `PATCH` | `/queue/{id}/start` | Start service — creates transaction, links queue |
| `PATCH` | `/queue/{id}/cancel` | Cancel queue entry |
| `PATCH` | `/queue/{id}/no-show` | Mark as no-show |
| `PATCH` | `/queue/{id}/requeue` | Re-queue NO_SHOW back to WAITING |
| `GET` | `/queue/next` | Next entry to be called |
| `GET` | `/queue/display` | **Public (no auth)** queue display data |
| `GET` | `/queue/stats` | Queue stats: waiting count, avg wait, served today |

### Branches

| Method | Route | Description |
|---|---|---|
| `GET` | `/branches` | List branches | 
| `GET/POST/PUT` | `/branches/{id}` | CRUD |
| `PATCH` | `/branches/{id}/status` | Activate/deactivate |

### Services

| Method | Route | Description |
|---|---|---|
| `GET/POST` | `/services` | List/create |
| `GET/PUT` | `/services/{id}` | Get/update |
| `PUT` | `/services/{id}/pricing` | Bulk upsert pricing matrix |
| `PUT` | `/services/{id}/commissions` | Bulk upsert commission matrix |

### Packages, Service Categories, Vehicle Types, Sizes, Makes, Models — Standard CRUD

### Customers — List (search), Get (with cars + history), Create, Update

### Cars — List, Get, Create, Update, `GET /cars/lookup/{plateNumber}` (POS fast lookup)

### Employees — CRUD + attendance clock-in/out + commission history

### Transactions (POS)

| Method | Route | Description |
|---|---|---|
| `POST` | `/transactions` | Create transaction (core POS operation) |
| `GET` | `/transactions` | List (filter by branch, date, status) |
| `GET` | `/transactions/{id}` | Full detail |
| `PATCH` | `/transactions/{id}/status` | Update status |
| `PATCH` | `/transactions/{id}/discount-tip` | Update discount and/or tip on Pending/InProgress transaction |
| `POST` | `/transactions/{id}/payments` | Add payment |
| `GET` | `/transactions/daily-summary` | Daily branch summary |

### Merchandise — CRUD + stock adjustment

### Payroll — Periods list, close, process, entry update

### Pricing Modifiers — CRUD

### Dashboard & Reports — Summary, revenue, commissions, service popularity

---

## Frontend Page Inventory

### Auth Pages (both apps)

| Route | Page |
|---|---|
| `/sign-in` | Custom sign-in (email/password + social via Clerk headless hooks) |
| `/sign-up` | Custom sign-up + email verification (admin only, NOT on POS) |
| `/sso-callback` | OAuth redirect handler |
| `/onboarding` | Tenant onboarding wizard (admin only) |

### Admin Dashboard — 30+ routes for branches, services, packages, employees, payroll, customers, vehicles, merchandise, transactions, reports, settings

### POS App

| Route | Page |
|---|---|
| `/` | POS Home — quick actions |
| `/queue` | **Queue Board** — Kanban: WAITING / CALLED / IN_SERVICE columns |
| `/queue/add` | **Add to Queue** — plate lookup, priority, preferred services |
| `/transactions/new` | New Transaction (supports direct OR from-queue entry) |
| `/transactions/[id]` | Transaction detail + receipt |
| `/history` | Today's transactions |
| `/customers/lookup` | Plate/customer search |
| `/attendance` | Clock in/out |
| `/queue-display` | **PUBLIC (no auth)** full-screen queue for wall TV |

---

## POS UX Requirements

1. **Large touch targets** — 48px+ height. 2. **Minimal navigation** — single-page panel layout for transactions. 3. **High contrast** status colors. 4. **Keyboard/scanner support**. 5. **Running totals always visible**. 6. **Two entry points** for transactions: Direct and From Queue.

### New Transaction Screen — supports `?queueEntryId=xxx` query param

When `queueEntryId` is present: pre-fill vehicle/customer from queue entry, pre-select preferred services, on submit also link the queue entry.

---

## Hangfire Background Jobs

| Job | Schedule | Description |
|---|---|---|
| `CreateWeeklyPayrollPeriod` | Mon 00:00 PHT | Create PayrollPeriod per tenant |
| `AutoClosePayrollPeriod` | Sun 23:55 PHT | Close expired OPEN periods |
| `CheckLowStockAlerts` | Daily 08:00 PHT | Scan low inventory |
| `CleanupStaleTransactions` | Hourly | Cancel PENDING transactions older than 4h |

**Queue No-Show Timer:** Fire-and-forget, triggered when customer is CALLED. `BackgroundJob.Schedule` 5-minute delay. Only marks NO_SHOW if status still CALLED.

---

## SignalR Real-Time

Groups: `tenant:{tenantId}`, `tenant:{tenantId}:branch:{branchId}`, `queue-display:{branchId}` (public)

Events: `TransactionUpdated`, `DashboardMetricsUpdated`, `AttendanceUpdated`, `QueueUpdated`, `QueueDisplayUpdated`

---

## Key Business Rules

1. **Commission Split**: Equal split, `Math.Round(value, 2, MidpointRounding.AwayFromZero)`.
2. **Dynamic Pricing**: ServicePricing matrix → fallback to basePrice.
3. **Commission Matrix**: PERCENTAGE / FIXED_AMOUNT / HYBRID. No matrix entry = ₱0.
4. **Package Pricing**: Own matrix. Commission always percentage.
5. **Transaction Lifecycle**: PENDING → IN_PROGRESS → COMPLETED. Cancel from PENDING/IN_PROGRESS. Refund from COMPLETED. When linked to queue, status changes propagate.
6. **Payroll**: OPEN → CLOSED → PROCESSED. Cannot skip or modify after PROCESSED.
7. **Employee Types**: COMMISSION = commissions only. DAILY = dailyRate × daysWorked.
8. **Multi-Payment**: Sum of payments must equal finalAmount before COMPLETED.
9. **Tenant Isolation**: EF Core global query filters. No cross-tenant access.
10. **Queue Priority**: VIP > EXPRESS > REGULAR. Within same priority, FIFO.
11. **Inventory Tracking**: Decrement on completion. Block sale if insufficient stock.
12. **Attendance**: One record per employee per day. TimeIn before TimeOut.

---

## Seed Data

Tenant: "SparkleWash Philippines". Branches: Makati + BGC. Vehicle Types: Sedan/SUV/Van/Truck/Motorcycle. Sizes: S/M/L/XL. Makes: Toyota/Honda/Mitsubishi/Nissan/Suzuki with models. 3 service categories, 10 services. Full pricing + commission matrices for Basic Wash. 8 employees across branches. 4 merchandise items. 10-20 sample transactions.

---

## Coding Standards

**C#:** file-scoped namespaces, primary constructors, sealed classes, records for DTOs/commands, Result<T> pattern, CancellationToken everywhere, no AutoMapper.

**TypeScript:** strict mode, no `any`, `@splashsphere/types` workspace package, `proxy.ts` for Clerk, kebab-case files, `pnpm`, `reactCompiler: true`.

**API:** RESTful `/api/v1/{resource}`, ProblemDetails errors, TypedResults, tenant from JWT only.

---

## Environment Variables

```env
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=splashsphere;Username=postgres;Password=postgres
Clerk__Authority=https://<instance>.clerk.accounts.dev
Clerk__SecretKey=sk_test_xxxxx
NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY=pk_test_xxxxx
CLERK_SECRET_KEY=sk_test_xxxxx
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## Important Reminders

- **Always scope data by tenant.** TenantId from JWT `org_id` claim.
- **Commission logic is the heart of the system.** Follow the algorithm exactly.
- **Custom auth UI** — use Clerk hooks, NOT prebuilt components.
- **Onboarding creates the Clerk Organization** via backend API, then creates Tenant + Branch.
- **Queue supports two workflows** — direct transaction OR queue-first.
- **Next.js 16 uses `proxy.ts`** not `middleware.ts`. Turbopack default. React Compiler stable.
- **All times in Asia/Manila (UTC+8).** Store as UTC, convert for display.
- **Currency: Philippine Peso (₱ / PHP).** Decimal with 2 places.

## Living Documentation Rules

1. **Changelog:** Append an entry after every task.
2. **API Inventory:** When adding new endpoints, add them to the 
   API Endpoint Inventory section.
3. **Page Inventory:** When adding new frontend pages, add them to 
   the Frontend Page Inventory section.
4. **Business Rules:** When implementing new business logic, add
   the rule to the Key Business Rules section.

---

## Changelog

### 2026-03-20
- **Fix: Queue number daily reset (duplicate key bug)** — Added `QueueDate` (DateOnly, Manila local date) column to `QueueEntry`. Updated unique constraint from `(BranchId, QueueNumber, TenantId)` to `(TenantId, BranchId, QueueDate, QueueNumber)` so Q-001 on different days never collide. Both `AddToQueueCommandHandler` and the walk-in path in `CreateTransactionCommandHandler` now filter by `QueueDate` instead of CreatedAt UTC range. Migration: `AddQueueEntryQueueDate`.

### 2026-03-21
- **Feature: Cashier Shift domain layer (Prompt 15.9a)** — Added `CashierShift`, `CashMovement`, `ShiftDenomination`, `ShiftPaymentSummary` entities. Added `ShiftStatus`, `ReviewStatus`, `CashMovementType` enums. Added `ShiftOpenedEvent`, `ShiftClosedEvent`, `ShiftFlaggedEvent` domain events. Updated `Branch` and `User` navigations. Added 4 EF Core configurations with all indexes, cascade deletes, and decimal precision. `TenantId` on all 4 entities with matching global query filters. Migration: `AddCashierShifts`.
- **Feature: Discount and Tip editing on Transaction Detail page** — Added `PATCH /transactions/{id}/discount-tip` endpoint (`UpdateDiscountTipCommand`). Validates transaction is not terminal, discount ≤ subtotal, and existing payments don't exceed new total. Frontend detail page (`/transactions/[id]`) now shows inline discount/tip edit form for `InProgress` transactions, with an "Add Discount / Tip" / "Edit" button in the Totals section.
