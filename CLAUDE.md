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
24. **Connect Customer Identity**: `ConnectUser` is **global** (not tenant-scoped) — phone number is the globally unique primary identifier. `GlobalMake`/`GlobalModel` vehicle catalogues are also global. Per-tenant links live in `ConnectUserTenantLink`, which maps a `ConnectUser` to a tenant's `Customer` row. Customers keep a single profile across every SplashSphere-powered wash.
25. **Connect Auth (Phone OTP)**: The Customer Connect app uses its own JWT scheme (`ConnectJwt`) distinct from Clerk. Flow: OTP request → SMS code via Semaphore → OTP verify → issue access+refresh pair. Access tokens last 30 minutes, refresh tokens 30 days and **rotate on every use** (SHA-256 hash stored in `ConnectRefreshTokens`). Rate limits: 1 send / 60s, 5 sends / day per phone. Platform absorbs OTP SMS cost — it does NOT decrement tenant `SmsQuotaMonthly`. Dev-mode bypass: set `Otp:FixedCode` to a canned value.
26. **Queue Priority (with Bookings)**: Order is `Vip (4) > Booked (3) > Express (2) > Regular (1)`. Bookings arriving at their slot are enqueued at `Booked` priority, sitting below VIPs but above express/regular walk-ins. The `Vip` enum value was promoted from 3 to 4 in the `AddCustomerConnect` migration; existing data is migrated in-place.
27. **Booking Price Semantics**: At booking time, `BookingService.Price` is set **only** if the customer's vehicle is already classified (`ConnectVehicle.IsVehicleClassified = true`) — otherwise the booking shows a `PriceMin`/`PriceMax` range sourced from the `ServicePricing` matrix for the selected `VehicleTypeId`. The cashier confirms final classification on arrival (Arrived → InService), at which point the exact price is locked.
28. **Booking-to-Transaction Handoff**: When a queue entry has a linked booking, (a) the cashier must classify the vehicle before starting service if `IsVehicleClassified = false` — `StartQueueServiceCommandHandler` enforces this guard; (b) `CreateTransactionCommandHandler` auto-populates services from `BookingService` rows and overrides line prices with `BookingService.Price`; (c) completing the transaction sets `Booking.TransactionId` and advances `Booking.Status` to `InService`.
29. **Referral Reward Payout**: On the referred customer's first completed transaction at the tenant, `TransactionCompletedReferralHandler` awards `ReferrerRewardPoints` + `ReferredRewardPoints` (defaults 100 / 50, configurable on `LoyaltyProgramSettings`) to each party's active `MembershipCard`. Missing cards are skipped, but the `Referral` still flips to `Completed` so codes don't linger.
30. **Connect App Token Handling**: The Customer Connect app stores access + refresh tokens in `localStorage` under `splashsphere.connect.*` keys, retrieved via `useSyncExternalStore` so SSR stays safe. The shared `apiClient` auto-attaches the bearer and auto-refreshes via `POST /api/v1/connect/auth/refresh` on 401, with **module-level request coalescing** — at most one refresh is in flight per burst of expired calls. Refresh tokens rotate on every successful use (backend side — see rule 25). A client-side `<AuthGuard>` enforces sign-in for every authed route; there is no Next middleware (`proxy.ts`) because tokens are client-side only.
31. **Receipt Setting Resolution**: Each tenant has at least one `ReceiptSetting` row (seeded during onboarding with `BranchId = null` — the tenant default). `GetReceiptSettingQueryHandler` and `GetReceiptQueryHandler` both resolve in this order: (a) row matching the requested `BranchId`, (b) tenant default (`BranchId IS NULL`), (c) in-memory defaults if no row exists (legacy tenants from before slice 2). The unique partial indexes on `(TenantId)` filtered by `BranchId IS NULL` and `(TenantId, BranchId)` filtered by `BranchId IS NOT NULL` enforce one row per slot. **Branch overrides (slice 4)** are gated on the Enterprise-only `FeatureKeys.BranchReceiptOverrides` feature: the gate is enforced inside `UpdateReceiptSettingCommandHandler` (returns `Error.Forbidden` when `BranchId != null` and the plan lacks the feature) rather than via `RequiresFeatureAttribute`, because the same route serves both the tenant-default and branch-override paths and the gate is conditional on the query parameter. Reads (GET) are intentionally NOT gated — non-Enterprise tenants see the resolved-with-fallback setting but can't save changes. The `DeleteReceiptSettingCommand` removes a branch override only — the tenant default is permanent and the validator rejects empty `BranchId`.
32. **Receipt PDF Toggles**: `ReceiptPdfDocument` honors every `Show*` flag on `ReceiptSettingsDto`. Toggles tied to data not yet wired (loyalty info, service duration) are no-ops — the flags stay in the form so they take effect when the data lands. The renderer reads tenant branding from `ReceiptCompanyDto` (joined via `t.Tenant` in `GetReceiptQueryHandler`) so updates to the company profile flow into receipts on the next request without a separate cache.
33. **File Storage (Cloudflare R2)**: `IFileStorageService` (Application) + `R2FileStorageService` (Infrastructure) target Cloudflare R2 via the AWS S3 SDK. Bucket credentials live under `Cloudflare:R2:*` in `appsettings`; until real values are populated, uploads fail at runtime with the SDK's invalid-credentials error — DI / build / handler pipeline all work. Object keys are deterministic (`tenants/{tenantId}/{logo|thumbnail|icon}.png`) so re-uploads overwrite cleanly and there's no orphaned-blob accounting. The upload handler appends `?v={unix-seconds}` to returned URLs so browsers and CDN caches refetch after re-uploads. Logo upload uses the abstraction in tandem with `IImageProcessor` (ImageSharp impl) which produces 3 PNG variants (500/200/80 px). The receipt PDF handler prefetches the thumbnail bytes via `IHttpClientFactory` before invoking QuestPDF (whose `Compose` is synchronous); fetch failures are logged and fall back to the text-only header — a missing logo never breaks the receipt.
34. **Digital Receipt Email (auto-send + manual resend)**: `TransactionCompletedDigitalReceiptHandler` fires on every `TransactionCompletedEvent` and emails the customer when (a) the tenant's plan includes `FeatureKeys.DigitalReceipts` (Growth + Enterprise + Trial), (b) the transaction has a linked customer, and (c) that customer has an email on file. Email send failures are **swallowed** in the auto-send path so a Resend hiccup never tears down the rest of the completion pipeline (loyalty, SMS, dashboard, payroll commission accumulation). The cashier surface for retry is `POST /transactions/{id}/receipt/send` (`SendDigitalReceiptCommand`) — it shares the same plan gate, render path, and email service, but **does** propagate failures so the cashier knows whether the resend worked. The manual command also accepts an optional `OverrideEmail` for cases where the customer needs the receipt at a different address (typo, secondary email) without updating their profile. Both paths render via `ReceiptHtmlRenderer.Render(ReceiptDto)` — pure C# string-builder with all-inline styles (email clients strip external CSS), 600 px width, honors the same `ReceiptSettingsDto` toggles as `ReceiptPdfDocument`.

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

## Branching & Release

`staging` is the active development branch. `main` mirrors production. Full rules are in **[CONTRIBUTING.md](CONTRIBUTING.md)**, but the load-bearing one is:

> **Never squash-merge `staging` → `main`.** Always use `git merge --no-ff staging`. Squashing severs the commit graph and causes massive false conflicts on the next release. After releasing, fast-forward `staging` from `main` to keep them in sync.

`feature/*` → `staging` may squash or rebase. Branch protection on `main` should disable the "Squash and merge" button and require PRs.

## Living Documentation Rules

1. **Changelog:** Append an entry to `CHANGELOG.md` after every task (not in this file).
2. **API Inventory:** When adding new endpoints, add them to `docs/API_ENDPOINTS.md`.
3. **Page Inventory:** When adding new frontend pages, add them to `docs/PAGE_INVENTORY.md`.
4. **Business Rules:** When implementing new business logic, add the rule to the Key Business Rules section above.
5. **Commit Message:** At the end of every session with code changes, suggest a commit message summarizing all work done. Use a summary first line, then bullet points for each change in imperative mood.
