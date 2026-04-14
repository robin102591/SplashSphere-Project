# Algorithms, Integrations & Reference

## Custom Auth UI -- Clerk Integration (Headless Approach)

SplashSphere builds its **own sign-in, sign-up, and onboarding UI** while using Clerk's SDK under the hood. This gives full control over branding and flow.

### Clerk Hooks Used (NO prebuilt components)

```typescript
// Sign In -- custom form using useSignIn() hook
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

// Sign Up -- custom form using useSignUp() hook
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

After sign-up, new users land on `/onboarding` -- a multi-step wizard.

**Steps:** 1) Welcome -> 2) Business details (name, email, contact, address) -> 3) First branch setup -> 4) Confirm + Submit

**What happens on submit:**
1. Frontend calls `POST /api/v1/onboarding`.
2. Backend creates a Clerk Organization via Clerk Backend API.
3. Creates Tenant record with `id` = Clerk org ID.
4. Creates first Branch record.
5. Links current User to Tenant.
6. Frontend redirects to `/dashboard`.

**TenantResolutionMiddleware** handles users with no tenant:
- If user has no `org_id` claim -> allow access ONLY to `/auth/me`, `/onboarding/*`, `/webhooks/*`.
- All other endpoints -> return 403 "Complete onboarding first".

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

**Workflow A -- Direct Transaction (Walk-in, Pay Now):** Customer -> Create Transaction -> Service -> Pay -> Done.

**Workflow B -- Queue First (Busy Hours):** Customer -> Add to Queue (WAITING) -> Wait -> Called to bay -> Create Transaction (linked to queue) -> Service -> Pay -> Done.

### Queue Lifecycle

```
WAITING -> CALLED -> IN_SERVICE -> COMPLETED
   |         |
CANCELLED  NO_SHOW -> (back to WAITING or CANCELLED)
```

### Queue Entry Algorithm

1. Customer arrives -> cashier adds to queue. Generate "Q-{DailySequence}", set priority (REGULAR/VIP/EXPRESS), status = WAITING.
2. Estimate wait time: count entries ahead x average service duration.
3. Bay opens -> cashier calls next: highest priority WAITING, earliest createdAt. Status = CALLED. Start 5-min no-show timer (Hangfire).
4a. Customer arrives -> Start Service: create Transaction (normal flow), link QueueEntry.transactionId, status = IN_SERVICE. Cancel no-show timer.
4b. Customer doesn't arrive in 5 min -> status = NO_SHOW. Auto-call next person.
5. Transaction COMPLETED -> QueueEntry COMPLETED.

### Queue Display (Public)

Route: `/queue-display?branchId=xxx` -- NO auth. For wall-mounted TV. Auto-refreshes via SignalR. Shows queue number, masked plate, status, estimated wait.

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
**Step 3:** For each service, look up `ServiceCommission`. Calculate by type: PERCENTAGE (price x rate), FIXED_AMOUNT, or HYBRID (fixed + percentage). Split equally among employees: `commissionPerEmployee = totalCommission / employeeCount` with `Math.Round(value, 2, MidpointRounding.AwayFromZero)`.
**Step 4:** Same for packages (PackagePricing, PackageCommission -- always percentage).
**Step 5:** Process merchandise -- decrement inventory, check stock.
**Step 6:** Aggregate: `finalAmount = totalAmount - discountAmount + taxAmount`.
**Step 7:** Create TransactionEmployee summary records.
**Step 8:** Generate transaction number: `"{BranchCode}-{YYYYMMDD}-{Sequence}"`.
**Step 9:** Save in single DB transaction. Publish `TransactionCreatedEvent`. If linked to queue, set queue status = IN_SERVICE.

---

## Payroll Processing Algorithm

**Weekly Period:** `OPEN -> CLOSED -> PROCESSED -> RELEASED`. Cannot skip states.

**Closing:** For each employee: sum commissions from completed transactions in period, count attendance days, calculate baseSalary (DAILY type: `dailyRate x daysWorked`), create PayrollEntry.

**Processing:** Admin reviews, adjusts bonuses/deductions, confirms. No modifications after PROCESSED.

---

## Hangfire Background Jobs

| Job | Schedule | Description |
|---|---|---|
| `RunDailyPayrollJob` | Daily 00:05 PHT | Per-tenant: auto-close expired periods + create new periods based on tenant's CutOffStartDay |
| `CheckLowStockAlerts` | Every 6 hours | Check supplies and merchandise for low stock |
| `CleanupStaleTransactions` | Hourly | Cancel PENDING transactions older than 4h |
| `GenerateRecurringExpenses` | Daily 00:30 PHT | Auto-generate expense records for recurring expenses (Daily/Weekly/Monthly) |
| `CalculateMonthlyRoyaltiesJob` | Monthly 1st 02:00 PHT | Per-franchise-network: sum franchisee revenue, calculate royalty/marketing/tech fees |
| `SendRoyaltyRemindersJob` | Monthly 5th 09:00 PHT | Mark unpaid royalties as overdue |
| `CheckEquipmentMaintenance` | Daily 08:00 PHT | Set overdue equipment to NeedsMaintenance status |

**Queue No-Show Timer:** Fire-and-forget, triggered when customer is CALLED. `BackgroundJob.Schedule` 5-minute delay. Only marks NO_SHOW if status still CALLED.

---

## SignalR Real-Time

Groups: `tenant:{tenantId}`, `tenant:{tenantId}:branch:{branchId}`, `queue-display:{branchId}` (public)

Events: `TransactionUpdated`, `DashboardMetricsUpdated`, `AttendanceUpdated`, `QueueUpdated`, `QueueDisplayUpdated`

---

## Seed Data

Tenant: "SparkleWash Philippines". Branches: Makati + BGC. Vehicle Types: Sedan/SUV/Van/Truck/Motorcycle. Sizes: S/M/L/XL. Makes: Toyota/Honda/Mitsubishi/Nissan/Suzuki with models. 3 service categories, 10 services. Full pricing + commission matrices for Basic Wash. 8 employees across branches. 4 merchandise items. 10-20 sample transactions.

---

## Environment Variables

```env
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=splashsphere;Username=postgres;Password=postgres
Clerk__Authority=https://<instance>.clerk.accounts.dev
Clerk__SecretKey=sk_test_xxxxx
PayMongo__SecretKey=sk_test_xxxxx          # Optional -- omit to use mock gateway
PayMongo__PublicKey=pk_test_xxxxx
PayMongo__WebhookSecret=whsec_xxxxx
Resend__ApiKey=re_xxxxx                      # Optional -- omit to use mock email service
Resend__FromEmail=SplashSphere <noreply@splashsphere.ph>
Semaphore__ApiKey=xxxxx                      # Optional -- omit to use mock SMS service
Semaphore__SenderName=SplashSphere
NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY=pk_test_xxxxx
CLERK_SECRET_KEY=sk_test_xxxxx
NEXT_PUBLIC_API_URL=http://localhost:5000
```
