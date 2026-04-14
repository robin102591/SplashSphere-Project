# SplashSphere -- Claude Code Project Instructions

## Identity & Role

You are a **Senior Architect and Senior Full-Stack Engineer** building **SplashSphere**, a comprehensive multi-tenant car wash management system designed for the Philippine market. You bring deep expertise in distributed systems, domain-driven design, and production-grade SaaS architecture. Every decision you make should reflect production readiness, maintainability, and the realities of Philippine car wash operations.

---

## Project Overview

SplashSphere is a multi-tenant, multi-branch car wash management platform composed of two primary surfaces:

- **Back Office (Admin Dashboard)** -- Tenant management, employee management, payroll processing, service/pricing configuration, commission matrices, reports, analytics, and branch administration.
- **Front Office (POS)** -- Transaction processing, **vehicle queue management**, service selection with dynamic pricing, employee assignment with commission splitting, multiple payment methods, customer/vehicle lookup, and real-time updates.

### Philippine Car Wash Business Context

- Car wash services are **entirely manual** -- performed by attendants, not machines.
- Most employees are **commission-based** (`COMMISSION` type). Their pay comes from a percentage or fixed amount per service performed, split evenly among all employees assigned to that service.
- Some staff (e.g., cashiers, security, maintenance) are **daily-rate** employees (`DAILY` type) paid a fixed amount per day worked.
- **Payroll is cut off weekly** -- every week, commissions and daily rates are tallied and processed.
- Common payment methods: **Cash, GCash, Maya (via GCASH/CREDIT_CARD enums)**, credit/debit cards, bank transfers.
- A single tenant (business owner) may operate **multiple branches** across different cities or regions.
- Pricing varies by **vehicle type** (Sedan, SUV, Van, Truck) and **vehicle size** (Small, Medium, Large, XL) -- creating a pricing matrix per service.
- Commission rates also vary by vehicle type and size -- a separate commission matrix per service.
- Service packages bundle multiple services at a discounted rate with their own pricing and commission matrices.
- Peak/off-peak pricing modifiers can adjust prices based on time of day, day of week, holidays, weather, or promotions.
- **During peak hours, vehicles queue up.** The system manages a queue board with priority levels, estimated wait times, no-show handling, and a public display for wall-mounted screens.

---

## Tech Stack & Package Versions

### Backend -- .NET 9 Web API

- **Runtime**: .NET 9 (C# 13)
- **Framework**: ASP.NET Core 9 Minimal APIs (prefer over Controllers for new endpoints)
- **ORM**: Entity Framework Core 9 -- `Npgsql.EntityFrameworkCore.PostgreSQL` 9.x
- **Database**: PostgreSQL 16+
- **Architecture**: Clean Architecture with CQRS via MediatR 12.x
- **Validation**: FluentValidation 11.x
- **Authentication**: Clerk JWT verification -- `Clerk.BackendAPI` (clerk-sdk-csharp) latest
- **Background Jobs**: Hangfire 1.8.x with `Hangfire.PostgreSql`
- **Real-time**: SignalR (built into ASP.NET Core 9)
- **Logging**: Serilog 4.x with `Serilog.Sinks.Console` and `Serilog.Sinks.PostgreSQL`
- **API Docs**: `Microsoft.AspNetCore.OpenApi` (built-in .NET 9) + Scalar for UI
- **Caching**: HybridCache (ASP.NET Core 9 built-in)
- **ID Generation**: `Ulid` NuGet package for time-sortable transaction IDs
- **Mapping**: Mapster or manual mapping in handlers (no AutoMapper)

### Frontend -- Next.js 16 (Two Applications)

- **Framework**: Next.js 16.1.x with App Router, Turbopack (default), React 19
- **Language**: TypeScript 5.x (strict mode)
- **Auth**: `@clerk/nextjs` ^6.36.x -- **Custom sign-in/sign-up UI** using Clerk's headless hooks (`useSignIn`, `useSignUp`) -- NOT Clerk's prebuilt components. Uses `proxy.ts` (not `middleware.ts`).
- **Styling**: Tailwind CSS 4.x
- **State Management**: `@tanstack/react-query` ^5.x, `zustand` ^5.x
- **Forms**: `react-hook-form` ^7.x + `zod` ^3.x + `@hookform/resolvers`
- **UI Components**: `shadcn/ui`, **Tables**: `@tanstack/react-table` ^8.x, **Charts**: `recharts` ^2.x
- **Real-time**: `@microsoft/signalr` ^8.x
- **i18n**: `next-intl` ^4.x -- cookie-based locale detection (`NEXT_LOCALE` cookie), no URL prefix. Supports English (`en`) and Filipino (`fil`). Translation files in `apps/{app}/messages/{locale}.json`. Use `useTranslations()` hook in components.
- **PWA**: `@serwist/next` ^9.x -- service worker with precaching, runtime caching, offline fallback. Manifests in `public/manifest.json`. Install prompt via `usePwaInstall` hook.
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
│   ├── admin/                              # Next.js 16 -- Admin Dashboard
│   └── pos/                                # Next.js 16 -- POS Application
├── packages/types/                         # Shared TypeScript types
├── docs/                                   # Reference documentation (see below)
├── docker-compose.yml
├── pnpm-workspace.yaml
└── CLAUDE.md
```

---

## Architecture Patterns & Principles

### Clean Architecture Layers

1. **Domain** -- Pure C#. Entities, Value Objects, Domain Events, Enums, Domain Services. Zero framework dependencies.
2. **Application** -- Commands/Queries (MediatR), DTOs, FluentValidation validators, interface definitions.
3. **Infrastructure** -- EF Core DbContext, repositories, Clerk JWT middleware, Hangfire jobs, SignalR hubs.
4. **API** -- Minimal API endpoints, middleware pipeline, DI registration.

### CQRS with MediatR

- Every write -> **Command** returning `Result<T>`. Every read -> **Query** returning DTO or `PagedResult<T>`.
- Pipeline behaviors: validation, logging, tenant resolution, unit of work.
- TenantId and UserId come from TenantContext (resolved from JWT), never from command parameters.

#### ICommand / IQuery Type Rules

The project uses custom wrapper interfaces that **auto-wrap** the return type in `Result<T>`. Never double-wrap.

```
ICommand                      -> IRequest<Result>            -- use for commands with no return value
ICommand<T>                   -> IRequest<Result<T>>         -- use for commands returning a value (T = the inner value, NOT Result)
IQuery<T>                     -> IRequest<T>                 -- use for queries (T is typically a DTO or PagedResult<DTO>)
```

**Common mistake -- NEVER do this:**
```csharp
// WRONG -- produces IRequest<Result<Result>>, handler won't resolve
public record MyCommand() : ICommand<Result>;

// CORRECT -- produces IRequest<Result>
public record MyCommand() : ICommand;
```
```csharp
// WRONG -- produces IRequest<Result<Result<MyDto>>>
public record MyCommand() : ICommand<Result<MyDto>>;

// CORRECT -- produces IRequest<Result<MyDto>>
public record MyCommand() : ICommand<MyDto>;
```

**Handler must match the expanded type:**
| Command declares | Handler implements |
|---|---|
| `: ICommand` | `IRequestHandler<MyCommand, Result>` |
| `: ICommand<MyDto>` | `IRequestHandler<MyCommand, Result<MyDto>>` |
| `: IQuery<MyDto>` | `IRequestHandler<MyQuery, MyDto>` |

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

## Reference Documentation

Detailed algorithms, integration code samples, API/page inventories, and other reference material have been extracted to keep this file focused on architecture and rules:

- **[docs/API_ENDPOINTS.md](docs/API_ENDPOINTS.md)** -- Full API endpoint inventory (all `/api/v1` routes with methods and descriptions)
- **[docs/PAGE_INVENTORY.md](docs/PAGE_INVENTORY.md)** -- Frontend page inventory (Admin Dashboard + POS routes) and POS UX requirements
- **[docs/ALGORITHMS.md](docs/ALGORITHMS.md)** -- Clerk auth integration (headless hooks + JWT validation), tenant onboarding flow, Prisma-to-EF Core mapping guide, queue management system (lifecycle + algorithm), transaction creation algorithm, payroll processing algorithm, Hangfire background jobs, SignalR real-time events, seed data, environment variables

Consult these files when working on the relevant domain. They are the authoritative source for endpoint routes, page routes, and implementation algorithms.

---

## Key Business Rules

1. **Commission Split**: Equal split, `Math.Round(value, 2, MidpointRounding.AwayFromZero)`.
2. **Dynamic Pricing**: ServicePricing matrix -> fallback to basePrice.
3. **Commission Matrix**: PERCENTAGE / FIXED_AMOUNT / HYBRID. No matrix entry = P0.
4. **Package Pricing**: Own matrix. Commission always percentage.
5. **Transaction Lifecycle**: PENDING -> IN_PROGRESS -> COMPLETED. Cancel from PENDING/IN_PROGRESS. Refund from COMPLETED. When linked to queue, status changes propagate.
6. **Payroll**: OPEN -> CLOSED -> PROCESSED -> RELEASED. Cannot skip or modify after PROCESSED. Released is terminal -- pay has been disbursed.
7. **Employee Types**: COMMISSION = commissions only. DAILY = dailyRate x daysWorked. HYBRID = dailyRate x daysWorked + commissions.
8. **Multi-Payment**: Sum of payments must equal finalAmount before COMPLETED.
9. **Tenant Isolation**: EF Core global query filters. No cross-tenant access.
10. **Queue Priority**: VIP > EXPRESS > REGULAR. Within same priority, FIFO.
11. **Inventory Tracking**: Decrement on completion. Block sale if insufficient stock.
12. **Attendance**: One record per employee per day. TimeIn before TimeOut.
13. **Shift Gate**: Cashier must have an open shift before adding to queue or creating a transaction. Enforced in both backend (handler validation) and frontend (page-level gate).
14. **POS Lock Screen**: POS auto-locks after configurable inactivity (default 5 min) or manual lock. Requires 4-6 digit PIN to unlock. PINs are BCrypt-hashed, stored on User entity, set by admins only. Max PIN attempts before 30s cooldown (configurable via ShiftSettings).
15. **Cash Advance FIFO Deduction**: Active cash advances are automatically deducted during payroll close, oldest first. Each deduction creates a `PayrollAdjustment` row with category "Cash Advance". Deduction amount = `min(DeductionPerPeriod, RemainingBalance)`. Advance marked `FullyPaid` when balance reaches zero.
16. **Per-Branch Payroll**: Payroll periods and settings are branch-scoped. Each branch can have its own cut-off day and frequency (overrides tenant default). The Hangfire job creates one period per branch per cycle. `ClosePayrollPeriod` scopes employees to the period's branch. Settings resolve: branch override -> tenant default -> hardcoded (Monday/Weekly).
17. **Loyalty Points**: Points are whole integers. Earned via `floor(FinalAmount / CurrencyUnitAmount) * PointsPerCurrencyUnit * tierMultiplier`. Auto-awarded on transaction completion via `TransactionCompletedLoyaltyHandler`. MembershipCard is separate from Customer (requires feature gate). Tier progression is one-directional (upgrades only). Auto-enrollment when `AutoEnroll` is enabled.
18. **Loyalty Feature Gate**: All loyalty endpoints require `FeatureKeys.CustomerLoyalty` (Growth + Enterprise + Trial plans). The `RequiresFeatureAttribute` middleware enforces this.
19. **Franchise Feature Gate**: All franchise endpoints (except public invitation validate/accept) require `FeatureKeys.FranchiseManagement` (Enterprise plan only). Franchisees pay their own independent subscriptions -- each franchisee tenant has its own `TenantSubscription` starting with a 14-day Trial, then upgrades to any plan independently.
20. **Stock Movement Audit Trail**: Every stock quantity change creates a `StockMovement` record. `CurrentStock` is always derivable from the sum of movements. Negative stock is allowed but triggers warnings.
21. **Supply Usage Auto-Deduction**: On transaction completion, if `ServiceSupplyUsage` records exist, supplies are auto-deducted based on vehicle size. Falls back to default (null SizeId) entries. Optional per-service -- unconfigured services skip deduction.
22. **Purchase Order Lifecycle**: Draft -> Sent -> PartiallyReceived/Received. Only Draft POs can be edited. Receiving creates PurchaseIn movements and updates stock + weighted average unit cost.
23. **Equipment Maintenance Scheduling**: Equipment status cycles: Operational -> NeedsMaintenance -> UnderRepair -> Operational. Daily Hangfire job flags overdue equipment. Logging maintenance resets status to Operational.

---

## Coding Standards

**C#:** file-scoped namespaces, primary constructors, sealed classes, records for DTOs/commands, Result<T> pattern, CancellationToken everywhere, no AutoMapper.

**TypeScript:** strict mode, no `any`, `@splashsphere/types` workspace package, `proxy.ts` for Clerk, kebab-case files, `pnpm`, `reactCompiler: true`.

**API:** RESTful `/api/v1/{resource}`, ProblemDetails errors, TypedResults, tenant from JWT only.

---

## Multi-Agent System

Specialized subagents and slash commands are available in `.claude/`:

- **Agents** (`.claude/agents/`): `backend`, `frontend`, `database`, `devops`, `qa`, `uiux`, `docs` -- each carries focused context and skills for its domain. Use via the Agent tool for delegated tasks.
- **Skills** (`.claude/skills/`): Shared knowledge packages loaded by agents -- project context, .NET patterns, Next.js patterns, EF Core patterns, Philippine car wash domain, and living API/page inventories.
- **Commands** (`.claude/commands/`): `/implement-feature`, `/fix-bug`, `/add-endpoint`, `/add-page`, `/review` -- workflow shortcuts that orchestrate multi-agent tasks.

This system complements CLAUDE.md (which remains the authoritative project spec). Agents load their own skill subsets instead of the full CLAUDE.md.

---

## Important Reminders

- **Always scope data by tenant.** TenantId from JWT `org_id` claim.
- **Commission logic is the heart of the system.** Follow the algorithm exactly.
- **Custom auth UI** -- use Clerk hooks, NOT prebuilt components.
- **Onboarding creates the Clerk Organization** via backend API, then creates Tenant + Branch.
- **Queue supports two workflows** -- direct transaction OR queue-first.
- **Next.js 16 uses `proxy.ts`** not `middleware.ts`. Turbopack default. React Compiler stable.
- **All times in Asia/Manila (UTC+8).** Store as UTC, convert for display.
- **Currency: Philippine Peso (PHP).** Decimal with 2 places.
- **i18n: English + Filipino.** Use `useTranslations()` from `next-intl` for all user-facing strings. Translation files in `messages/en.json` and `messages/fil.json`. Cookie-based locale (`NEXT_LOCALE`), no URL prefix. Navigation strings are fully extracted; remaining pages should follow the same pattern incrementally.

## Living Documentation Rules

1. **Changelog:** Append an entry to `CHANGELOG.md` after every task (not in this file).
2. **API Inventory:** When adding new endpoints, add them to `docs/API_ENDPOINTS.md`.
3. **Page Inventory:** When adding new frontend pages, add them to `docs/PAGE_INVENTORY.md`.
4. **Business Rules:** When implementing new business logic, add the rule to the Key Business Rules section above.
5. **Commit Message:** At the end of every session with code changes, suggest a commit message summarizing all work done. Use a summary first line, then bullet points for each change in imperative mood.
