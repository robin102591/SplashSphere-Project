# SplashSphere — Phase-by-Phase Build Prompts for Claude Code

> Copy each prompt into Claude Code one at a time, in order. Wait for each to finish before moving to the next. CLAUDE.md must be at the project root.

---

## PHASE 1: Project Foundation & Backend Scaffold

### Prompt 1.1 — .NET Solution Structure
```
Create the .NET 9 solution structure for SplashSphere following the Clean Architecture layout in CLAUDE.md. Create these projects with proper references: SharedKernel, Domain, Application, Infrastructure, API. Install all NuGet packages. Create folder structure, .editorconfig, docker-compose.yml for PostgreSQL 16, and .env.example. DO NOT create entities yet — just the skeleton.
```

### Prompt 1.2 — SharedKernel Foundation
```
In SharedKernel, create: Result<T> monad, PagedResult<T>, IAuditableEntity interface, IDomainEvent marker interface, and custom exceptions (SplashSphereException, TenantNotFoundException, InsufficientStockException, InvalidTransactionStateException, PayrollPeriodLockedException). Plus GlobalUsings.cs.
```

### Prompt 1.3 — Domain Enums
```
In Domain/Enums/, create all enums matching the Prisma schema: EmployeeType, CommissionType, PaymentMethod, TransactionStatus, PayrollStatus, ModifierType, QueueStatus (Waiting, Called, InService, Completed, Cancelled, NoShow), QueuePriority (Regular, Vip, Express). Use PascalCase. Add XML doc comments.
```

---

## PHASE 2: Domain Entities

### Prompt 2.1 — Core Reference Entities
```
In Domain/Entities/, create: Tenant, Branch, User (with ClerkUserId), VehicleType, Size, Make, Model, ServiceCategory, MerchandiseCategory. All implement IAuditableEntity. String IDs. Sealed classes. Include navigation collections. Match Prisma schema exactly.
```

### Prompt 2.2 — Service & Pricing Entities
```
Create: Service, ServicePricing, ServiceCommission (with CommissionType, nullable fixedAmount/percentageRate), ServicePackage, PackageService, PackagePricing, PackageCommission, PricingModifier (with ModifierType). All navigation properties.
```

### Prompt 2.3 — Customer & Vehicle Entities
```
Create: Customer, Car (unique plateNumber, composite unique [plateNumber, tenantId]), Merchandise (composite unique [sku, tenantId]).
```

### Prompt 2.4 — Employee & Payroll Entities
```
Create: Employee (with EmployeeType, dailyRate, branchId), Attendance (DateOnly date, unique [employeeId, date]), PayrollPeriod (PayrollStatus, unique [tenantId, year, cutOffWeek]), PayrollEntry (unique [payrollPeriodId, employeeId]).
```

### Prompt 2.5 — Transaction Aggregate
```
Create the full Transaction aggregate from Prisma schema: Transaction (ULID ID, all financial fields), TransactionService (with commission details), TransactionPackage, TransactionMerchandise, TransactionEmployee (unique [transactionId, employeeId]), Payment, ServiceEmployeeAssignment (unique [transactionServiceId, employeeId]), PackageEmployeeAssignment. Include cascade deletes.
```

### Prompt 2.5b — Queue Entity
```
Create QueueEntry entity: queueNumber, status (QueueStatus), priority (QueuePriority), estimatedWaitMinutes, preferredServices (string JSON), notes, calledAt, startedAt, completedAt, cancelledAt, noShowAt. Optional link to Transaction (one-to-one via transactionId), optional Customer and Car. Update Tenant, Branch, Customer, Car entities to add List<QueueEntry> QueueEntries navigation. Update Transaction to add optional QueueEntry navigation.
```

### Prompt 2.6 — Domain Events
```
Create domain event records in Domain/Events/: TransactionCreatedEvent, TransactionStatusChangedEvent, TransactionCompletedEvent, PayrollPeriodClosedEvent, PayrollProcessedEvent, LowStockAlertEvent, AttendanceRecordedEvent, QueueEntryCreatedEvent, QueueEntryCalledEvent, QueueEntryNoShowEvent, TenantOnboardedEvent. All sealed records implementing IDomainEvent.
```

---

## PHASE 3: Infrastructure — Database Layer

### Prompt 3.1 — EF Core Configurations (Reference Data)
```
Create IEntityTypeConfiguration for: Tenant, Branch, User, VehicleType, Size, Make, Model, ServiceCategory, MerchandiseCategory. Follow the Prisma-to-EF Core Mapping Guide exactly: gen_random_uuid() for PKs, default values, all indexes, all relationships. One file per entity.
```

### Prompt 3.2 — EF Core Configurations (Services & Pricing)
```
Configurations for: Service, ServicePricing, ServiceCommission, ServicePackage, PackageService, PackagePricing, PackageCommission, PricingModifier. Unique constraints, cascade deletes, decimal precision.
```

### Prompt 3.3 — EF Core Configurations (Customers, Vehicles, Merchandise)
```
Configurations for: Customer, Car (unique plateNumber, composite unique), Merchandise (composite unique [sku, tenantId]).
```

### Prompt 3.4 — EF Core Configurations (Employees & Payroll)
```
Configurations for: Employee, Attendance, PayrollPeriod, PayrollEntry. All composite uniques and precision.
```

### Prompt 3.5 — EF Core Configurations (Transaction Aggregate)
```
Configurations for: Transaction (ULID, no default PK), TransactionService, TransactionPackage, TransactionMerchandise, TransactionEmployee, Payment, ServiceEmployeeAssignment, PackageEmployeeAssignment. All cascade deletes, named relations for Cashier and Employee assignments.
```

### Prompt 3.5b — Queue EF Core Configuration
```
Create QueueEntryConfiguration: table "QueueEntries", UUID PK, unique [branchId, queueNumber, tenantId], indexes on status/createdAt, one-to-one with Transaction (optional, unique transactionId), relationships to Tenant/Branch/Customer/Car. Add DbSet<QueueEntry> to DbContext. Add global tenant filter. Generate migration "AddQueueManagement".
```

### Prompt 3.6 — DbContext, Interceptor, TenantContext
```
Create SplashSphereDbContext with all DbSets, global query filters for tenant isolation, configuration assembly scan. Create AuditableEntityInterceptor. Create TenantContext scoped service. Create DependencyInjection.cs extension method.
```

### Prompt 3.7 — Repositories
```
Create interfaces in Domain/Interfaces/: IRepository<T>, ITenantAwareRepository<T>, IUnitOfWork, ITransactionRepository, IServicePricingRepository, IServiceCommissionRepository. Implement in Infrastructure: TenantAwareRepository<T>, concrete repos, UnitOfWork. AsNoTracking on reads.
```

### Prompt 3.8 — Migration and Seed Data
```
Create initial EF Core migration. Create DataSeeder with all seed data from CLAUDE.md: SparkleWash Philippines tenant, 2 branches, vehicle types/sizes, makes/models, services with pricing+commission matrices, employees, merchandise, 10-20 sample transactions. Idempotent. Called in Development only.
```

---

## PHASE 4: Application Layer — CQRS Pipeline

### Prompt 4.1 — MediatR Pipeline Behaviors
```
Create: ValidationBehavior (FluentValidation), LoggingBehavior, UnitOfWorkBehavior (save on success for commands only). Register in AddApplication() extension. Register MediatR + validators from assembly.
```

### Prompt 4.2 — Branch Feature (Template Pattern)
```
Build complete Branch CRUD as the reference pattern: Commands (Create, Update, ToggleStatus), Queries (List paginated, GetById), DTOs, validators, BranchEndpoints.cs Minimal API. All subsequent features follow this exact pattern.
```

### Prompt 4.3 — Reference Data Features
```
Following Branch pattern: Vehicle Types, Sizes, Makes, Models, Service Categories, Merchandise Categories. Simple CRUD each with endpoints.
```

### Prompt 4.4 — Service Feature (with Matrices)
```
Service CRUD + UpsertServicePricing (bulk upsert pricing matrix) + UpsertServiceCommission (bulk upsert commission matrix). Queries return full matrices. Matrix editors delete-then-insert pattern.
```

### Prompt 4.5 — Package Feature
```
Package CRUD with included services, pricing matrix, commission matrix. Same pattern as services.
```

### Prompt 4.6 — Customer & Vehicle Features
```
Customer CRUD with search. Car CRUD with plate lookup (GET /cars/lookup/{plateNumber} — fast POS path).
```

### Prompt 4.7 — Employee & Attendance Features
```
Employee CRUD with branch/type filters. ClockIn/ClockOut commands with validation. Commission history query. Attendance query by branch/date range.
```

### Prompt 4.8 — Merchandise Feature
```
Merchandise CRUD + AdjustStock command (validates non-negative). IsLowStock computed flag.
```

### Prompt 4.9 — Queue Feature
```
Build Queue Management: AddToQueueCommand (generate queue number, estimate wait, publish QueueEntryCreatedEvent), CallNextInQueueCommand (pick highest priority WAITING, schedule 5-min no-show timer), StartQueueServiceCommand (set IN_SERVICE, create Transaction via CreateTransactionCommandHandler, link queue entry, cancel no-show timer), CancelQueueEntryCommand, MarkNoShowCommand (only if still CALLED, auto-call next), RequeueEntryCommand (NO_SHOW → WAITING). Queries: GetQueue (ordered by priority/time), GetNextInQueue, GetQueueDisplay (public, masked plates), GetQueueStats. QueueEndpoints.cs — display endpoint has NO auth.
```

---

## PHASE 5: Transaction Engine

### Prompt 5.1 — Transaction Creation Command
```
Build CreateTransactionCommandHandler following the Transaction Creation Algorithm in CLAUDE.md EXACTLY — all 9 steps. Pricing matrix lookup with fallback, commission calculation per type, employee split with rounding, merchandise inventory, aggregation, transaction number generation, event publishing. If queueEntryId is provided, also link the queue entry. Comprehensive FluentValidation.
```

### Prompt 5.2 — Transaction Status & Payments
```
UpdateTransactionStatusCommand (enforce valid transitions, propagate to queue entry), AddPaymentCommand (auto-complete when paid in full), GetTransactionsQuery, GetTransactionByIdQuery, GetReceiptQuery, GetDailySummaryQuery. TransactionEndpoints.cs.
```

---

## PHASE 6: Payroll System

### Prompt 6.1 — Payroll Commands & Queries
```
ClosePayrollPeriodCommand (verify OPEN, calculate entries per employee, commissions + attendance), ProcessPayrollPeriodCommand (verify CLOSED, set PROCESSED), UpdatePayrollEntryCommand (only when CLOSED). Queries: periods list, period detail with entries. PayrollEndpoints.cs.
```

---

## PHASE 7: Authentication & API Wiring

### Prompt 7.1 — Clerk Auth + Onboarding + Program.cs
```
Set up authentication with CUSTOM UI support:

Infrastructure/Authentication/: ClerkJwtSetup (JWT Bearer with Clerk Authority, OnTokenValidated populates TenantContext). TenantResolutionMiddleware — resolves internal UserId from ClerkUserId. IMPORTANT: handles users with NO tenant: allow /auth/me, /onboarding/*, /webhooks/* only. All other routes → 403.

API/Endpoints/:
- AuthEndpoints: GET /auth/me, GET /onboarding/status (returns needsOnboarding boolean)
- OnboardingEndpoints: POST /onboarding — creates Clerk Organization via Backend API, creates Tenant (id = org.id), creates Branch, links User. Requires auth but NOT tenant context.
- WebhookEndpoints: POST /webhooks/clerk — handles org/user events.

Program.cs: auth middleware, TenantResolutionMiddleware (skip for onboarding/webhooks), CORS, SignalR hub, Serilog, OpenAPI + Scalar, Hangfire, seed data.
```

### Prompt 7.2 — Pricing Modifiers & Dashboard
```
Pricing Modifier CRUD. Dashboard summary (tenant-wide + branch KPIs). Reports: revenue, commissions, service popularity.
```

---

## PHASE 8: Background Jobs & Real-Time

### Prompt 8.1 — Hangfire Jobs
```
PayrollJobService (weekly create + auto-close), InventoryJobService (low stock alerts), TransactionJobService (stale cleanup), QueueJobService (no-show marking — only if status still CALLED). Register recurring jobs with Asia/Manila timezone.
```

### Prompt 8.2 — SignalR Hub & Broadcasting
```
SplashSphereHub with JoinBranch, LeaveBranch, JoinQueueDisplay methods. Require auth on hub EXCEPT for queue display group. MediatR notification handlers broadcast: TransactionUpdated, DashboardMetricsUpdated, AttendanceUpdated, QueueUpdated, QueueDisplayUpdated.
```

---

## PHASE 9: Frontend — Shared Foundation

### Prompt 9.1 — Monorepo & Shared Types
```
pnpm-workspace.yaml. packages/types/ with TypeScript enums (including QueueStatus, QueuePriority), entity interfaces (including QueueEntryDto), API types. Barrel export.
```

### Prompt 9.2 — Admin App with Custom Auth + Onboarding
```
Create admin app (Next.js 16). Install deps + shadcn/ui.

CUSTOM AUTH PAGES (NOT Clerk prebuilt):
- (auth)/sign-in/page.tsx: "use client", email/password form + Google/Facebook/Microsoft social buttons via signIn.authenticateWithRedirect. Uses useSignIn() hook.
- (auth)/sign-up/page.tsx: name/email/password form + social. Uses useSignUp() hook. Email verification code step. Redirects to /onboarding.
- (auth)/sso-callback/page.tsx: OAuth redirect handler.
- (auth)/forgot-password/page.tsx: Reset via email code.

ONBOARDING:
- (onboarding)/onboarding/page.tsx: Multi-step wizard (Welcome → Business details → First branch → Confirm). On submit: POST /api/v1/onboarding. On success: redirect to /dashboard.

DASHBOARD: Custom profile dropdown (NOT UserButton), custom tenant display (NOT OrganizationSwitcher). Sign out via clerk.signOut(). Sidebar + header layout. Data table wrapper. All route placeholders.
```

### Prompt 9.3 — POS App with Custom Auth + Queue Display
```
Create POS app (Next.js 16). Install deps.

CUSTOM AUTH — POS-optimized:
- sign-in/page.tsx: Larger inputs (56px min), email/password ONLY (no social on POS), NO sign-up link. Uses useSignIn().

POS TERMINAL: layout + all POS routes including queue/page.tsx and queue/add/page.tsx.

PUBLIC QUEUE DISPLAY:
- app/queue-display/page.tsx: NO AUTH, full-screen for wall TV. Shows queue entries with large text, color-coded (WAITING=white, CALLED=yellow flash, IN_SERVICE=green). Auto-refreshes via SignalR "queue-display:{branchId}" group. Clock in corner. Business branding header. URL: /queue-display?branchId=xxx
```

---

## PHASE 10: Admin Dashboard — Feature Pages

### Prompt 10.1 — Branch Management
```
Complete Branch CRUD UI: data table, create form, detail page with tabs (employees, transactions).
```

### Prompt 10.2 — Service Management (with Matrix Editors)
```
Service list + create + detail page with THREE TABS: Details, Pricing Matrix (editable grid: vehicle type × size), Commission Matrix (editable grid with type dropdown). Build reusable pricing-matrix-editor.tsx and commission-matrix-editor.tsx components.
```

### Prompt 10.3 — Package Management
```
Package CRUD reusing matrix editor components.
```

### Prompt 10.4 — Employee Management
```
Employee list (filter by branch/type), create form, detail page with tabs: Details, Commission History, Attendance.
```

### Prompt 10.5 — Payroll Management
```
Payroll periods list with status badges. Detail page: entries table, inline-editable bonus/deductions when CLOSED, Close/Process action buttons with confirmations.
```

### Prompt 10.6 — Customer, Vehicle, Merchandise, Transaction History
```
Standard CRUD pages for Customers, Vehicles, Merchandise (with low stock highlighting), Transaction History (with full detail breakdown).
```

### Prompt 10.7 — Dashboard & Reports
```
Dashboard: KPI cards, revenue chart (Recharts), payment pie chart, top services bar chart, recent transactions. Reports: revenue, commissions, service popularity. Settings: vehicle types, sizes, makes/models, pricing modifiers, categories.
```

---

## PHASE 11: POS Application

### Prompt 11.0 — POS Queue Board
```
Build POS Queue Management:

queue/page.tsx — Kanban board: WAITING column, CALLED column (with 5-min countdown), IN_SERVICE column (with elapsed time). Cards show: Queue #, Plate, Vehicle Type, Priority badge, Customer name, Preferred services, Wait time. Actions: Call (WAITING), Start Service (CALLED → opens transaction), No-Show (CALLED), View Transaction (IN_SERVICE). Real-time via SignalR.

queue/add/page.tsx — Plate lookup + Priority selector (large toggles) + Preferred services multi-select + Notes. Submit → POST /api/v1/queue.

queue-display/page.tsx — Public, no auth. Full-screen wall TV display. Large text table, color-coded statuses, auto-scroll, clock, SignalR auto-refresh.
```

### Prompt 11.1 — POS Transaction Screen
```
Build main POS transaction screen. SUPPORTS TWO ENTRY POINTS:
1. DIRECT — from POS home "New Transaction" button
2. FROM QUEUE — via ?queueEntryId=xxx (pre-fills vehicle/customer, pre-selects preferred services)

Left panel (60%): Plate lookup, service buttons, package buttons, merchandise buttons.
Right panel (40%): Order summary, running totals, employee picker, payment section (multi-payment), COMPLETE button.
When queueEntryId present, also calls PATCH /queue/{id}/start on completion.
Use Zustand for local state, TanStack Query for API calls.
```

### Prompt 11.2 — POS Supporting Pages
```
POS Home (quick actions + today stats), Transaction detail + receipt (printable), Transaction history, Customer/plate lookup, Attendance clock in/out.
```

---

## PHASE 12: Real-Time Integration

### Prompt 12.1 — SignalR Client Integration
```
use-signalr.ts hook in both apps. Admin: dashboard + transaction list updates. POS: queue board + transaction history updates. Queue display: dedicated "queue-display:{branchId}" group. Connection status indicator (green/yellow/red).
```

---

## PHASE 13: Testing

### Prompt 13.1 — Backend Unit Tests
```
Commission calculations (PERCENTAGE/FIXED/HYBRID), employee split rounding, transaction state machine, payroll calculations.
```

### Prompt 13.1b — Queue Tests
```
Queue status transitions (valid + invalid), priority ordering (VIP before REGULAR), queue-to-transaction linking, no-show timer behavior, re-queue logic.
```

### Prompt 13.2 — Integration Tests
```
Full transaction flow with pricing/commission verification. Payroll close/process flow. Tenant isolation. Queue → transaction linking flow.
```

---

## PHASE 14: Polish & Production Readiness

### Prompt 14.1 — Error Handling & Logging
```
GlobalExceptionHandler with ProblemDetails. Serilog structured logging with tenantId/branchId. Frontend error boundaries, toast notifications, loading skeletons.
```

### Prompt 14.2 — Final Configuration & README
```
docker-compose with API service. README with setup steps. .gitignore. Verify everything compiles: dotnet build, pnpm build for both apps.
```

---

## Phase Summary

| Phase | What | Prompts |
|---|---|---|
| 1 | Skeleton, packages, shared kernel | 1.1–1.3 |
| 2 | Domain entities + queue entity + events | 2.1–2.6, **2.5b** |
| 3 | Database: EF configs, DbContext, repos, migration, seed | 3.1–3.8, **3.5b** |
| 4 | Application: CQRS pipeline, CRUD, **queue feature** | 4.1–4.9 |
| 5 | Transaction engine | 5.1–5.2 |
| 6 | Payroll system | 6.1 |
| 7 | **Custom auth, onboarding**, endpoints, dashboard | 7.1–7.2 |
| 8 | Background jobs **(+ no-show timer)**, SignalR | 8.1–8.2 |
| 9 | Frontend: **custom auth UI, onboarding, queue display** | 9.1–9.3 |
| 10 | Admin dashboard — all pages | 10.1–10.7 |
| 11 | POS: **queue board**, transactions, supporting pages | 11.0–11.2 |
| 12 | Real-time SignalR | 12.1 |
| 13 | Tests **(+ queue tests)** | 13.1–13.2, **13.1b** |
| 14 | Polish & production | 14.1–14.2 |

**Total: 14 phases, 38 prompts.**

---

## Tips

1. Wait for each prompt to finish. Fix compile errors before moving on.
2. Phase 5 (Transaction Engine) is the most complex — review against the algorithm in CLAUDE.md.
3. After Phase 8, the entire backend works. Test with curl/Postman before building frontend.
4. After Phase 9, both apps compile with working custom auth. Test Clerk login before feature pages.
5. If Claude Code hits context limits, say "continue" to finish.
