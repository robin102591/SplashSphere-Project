# Loyalty Program Feature Specification

> Customer loyalty / membership cards with points earning, tier progression, rewards redemption, and auto-enrollment.

---

## Overview

The loyalty program allows car wash tenants to reward repeat customers with points on every transaction. Points accumulate toward tier upgrades that unlock better earning multipliers, and can be redeemed for rewards from a configurable catalogue. The feature is gated behind `FeatureKeys.CustomerLoyalty` (available on Growth, Enterprise, and Trial plans).

### Key Principles

- **Points are whole integers** (no fractional points).
- **Tier progression is one-directional** (upgrades only, never demoted).
- **MembershipCard is separate from Customer** (not all customers need cards; respects feature gate).
- **PointTransaction is an append-only ledger** with a running `BalanceAfter` for auditable reconciliation.
- **Auto-enrollment** on first transaction completion when enabled in settings.
- **Points auto-awarded** on transaction completion via domain event handler (no manual API call needed).

---

## Domain Model

### Enums

```
src/SplashSphere.Domain/Enums/
  LoyaltyTier.cs            Standard=0, Silver=1, Gold=2, Platinum=3
  PointTransactionType.cs   Earned=0, Redeemed=1, Expired=2, Adjustment=3
  RewardType.cs             FreeService=0, FreePackage=1, DiscountAmount=2, DiscountPercent=3
```

### Entities

#### LoyaltyProgramSettings
Per-tenant singleton (one record per tenant, upsert pattern). Controls the earning formula, program on/off, and expiration policy.

| Field | Type | Description |
|---|---|---|
| `Id` | string | PK |
| `TenantId` | string | FK, unique index (singleton) |
| `PointsPerCurrencyUnit` | decimal | Points awarded per currency unit spent (default: 1) |
| `CurrencyUnitAmount` | decimal | Peso threshold per point award (default: 100) |
| `IsActive` | bool | Master on/off switch |
| `PointsExpirationMonths` | int? | Months before points expire (null = never) |
| `AutoEnroll` | bool | Auto-create membership card on first transaction (default: true) |

**Navigation:** `Tiers` (collection of `LoyaltyTierConfig`)

#### LoyaltyTierConfig
Child of `LoyaltyProgramSettings`. Defines tier thresholds and point earning multipliers.

| Field | Type | Description |
|---|---|---|
| `Id` | string | PK |
| `TenantId` | string | FK |
| `LoyaltyProgramSettingsId` | string | FK (cascade delete) |
| `Tier` | LoyaltyTier | Enum value |
| `Name` | string | Customisable display name (e.g. "Gold Member") |
| `MinimumLifetimePoints` | int | Lifetime points threshold to reach this tier |
| `PointsMultiplier` | decimal | Multiplier applied to base points (e.g. 1.5 = 50% bonus) |

**Unique constraint:** `(LoyaltyProgramSettingsId, Tier)`

#### LoyaltyReward
Rewards catalogue. Each reward has a type and point cost.

| Field | Type | Description |
|---|---|---|
| `Id` | string | PK |
| `TenantId` | string | FK |
| `Name` | string | Display name |
| `Description` | string? | Optional description |
| `RewardType` | RewardType | What benefit this grants |
| `PointsCost` | int | Points required to redeem |
| `ServiceId` | string? | FK to Service (FreeService type) |
| `PackageId` | string? | FK to ServicePackage (FreePackage type) |
| `DiscountAmount` | decimal? | Fixed peso discount (DiscountAmount type) |
| `DiscountPercent` | decimal? | Percentage discount 0-100 (DiscountPercent type) |
| `IsActive` | bool | Whether currently available for redemption |

#### MembershipCard
One per customer per tenant. Virtual loyalty card with sequential numbering.

| Field | Type | Description |
|---|---|---|
| `Id` | string | PK |
| `TenantId` | string | FK |
| `CustomerId` | string | FK to Customer (unique per tenant) |
| `CardNumber` | string | Sequential "SS-XXXXX" format (globally unique) |
| `CurrentTier` | LoyaltyTier | Current tier level |
| `PointsBalance` | int | Redeemable points available now |
| `LifetimePointsEarned` | int | Total points ever earned (drives tier progression) |
| `LifetimePointsRedeemed` | int | Total points ever redeemed |
| `IsActive` | bool | Card active flag |

**Unique constraints:** `(TenantId, CustomerId)`, `(CardNumber)`

#### PointTransaction
Append-only ledger for all point movements.

| Field | Type | Description |
|---|---|---|
| `Id` | string | PK |
| `TenantId` | string | FK |
| `MembershipCardId` | string | FK to MembershipCard |
| `Type` | PointTransactionType | Earned, Redeemed, Expired, Adjustment |
| `Points` | int | Positive for earn, negative for redeem/expire |
| `BalanceAfter` | int | Running balance snapshot after this entry |
| `Description` | string | Human-readable reason |
| `TransactionId` | string? | FK to Transaction (if earned from a wash) |
| `RewardId` | string? | FK to LoyaltyReward (if redeemed) |
| `ExpiresAt` | DateTime? | When these points expire |
| `CreatedAt` | DateTime | Immutable timestamp |

**Indexes:** `MembershipCardId`, `TransactionId`, `(MembershipCardId, CreatedAt)`

### Domain Events

| Event | Trigger | Payload |
|---|---|---|
| `PointsEarnedEvent` | Transaction completed + points awarded | MembershipCardId, TenantId, BranchId, CustomerId, PointsEarned, NewBalance, TransactionId |
| `PointsRedeemedEvent` | Customer redeems reward | MembershipCardId, TenantId, BranchId, PointsRedeemed, NewBalance, RewardId, TransactionId |
| `TierUpgradedEvent` | Lifetime points cross tier threshold | MembershipCardId, TenantId, CustomerId, PreviousTier, NewTier |

### Transaction Entity Changes

Added to the existing `Transaction` entity:
- `int PointsEarned` (default 0) — populated by the loyalty handler after completion
- `string? LoyaltyRedemptionId` — future use for linking redemptions to transactions

---

## Points Earning Algorithm

Implemented in `TransactionCompletedLoyaltyHandler` (runs as a MediatR notification handler for `TransactionCompletedEvent`).

```
1. Gate: tenant must have CustomerLoyalty feature enabled
2. Load transaction → get CustomerId, FinalAmount, BranchId
3. Skip if no CustomerId
4. Load LoyaltyProgramSettings → skip if null or !IsActive
5. Get MembershipCard for customer:
   a. If exists → use it
   b. If !exists && AutoEnroll → create new card with next sequential number
   c. If !exists && !AutoEnroll → skip (no points)
6. Calculate base points:
   basePoints = floor(FinalAmount / CurrencyUnitAmount) * PointsPerCurrencyUnit
7. Skip if basePoints <= 0
8. Apply tier multiplier:
   - Load LoyaltyTierConfigs ordered by MinimumLifetimePoints DESC
   - Find first tier where card.LifetimePointsEarned >= MinimumLifetimePoints
   - multiplier = tierConfig.PointsMultiplier ?? 1.0
   - finalPoints = floor(basePoints * multiplier)
9. Update card: PointsBalance += finalPoints, LifetimePointsEarned += finalPoints
10. Create PointTransaction ledger entry (Type=Earned, with optional ExpiresAt)
11. Update Transaction.PointsEarned = finalPoints
12. Check tier upgrade:
    - Find highest tier where LifetimePointsEarned >= MinimumLifetimePoints
    - If newTier > card.CurrentTier → upgrade, publish TierUpgradedEvent
13. SaveChangesAsync
14. Publish PointsEarnedEvent
```

### Example Calculation

Settings: 1 point per P100, Gold tier multiplier = 1.5x

| Transaction Amount | Base Points | Tier | Multiplier | Final Points |
|---|---|---|---|---|
| P500 | 5 | Standard | 1.0x | 5 |
| P500 | 5 | Gold | 1.5x | 7 |
| P1,250 | 12 | Platinum | 2.0x | 24 |
| P80 | 0 | Any | - | 0 (below threshold) |

---

## Points Redemption Flow

Implemented in `RedeemPointsCommandHandler`.

```
1. Load MembershipCard by ID → validate exists and active
2. Load LoyaltyReward by RewardId → validate exists and active
3. Check card.PointsBalance >= reward.PointsCost
4. Deduct: card.PointsBalance -= reward.PointsCost
5. Increment: card.LifetimePointsRedeemed += reward.PointsCost
6. Create PointTransaction (Type=Redeemed, Points=-PointsCost)
7. Return RedemptionResultDto with redemptionId, pointsDeducted, newBalance
```

---

## API Endpoints

All under `/api/v1/loyalty`. All require authentication + `FeatureKeys.CustomerLoyalty`.

### Settings & Configuration

| Method | Route | Description |
|---|---|---|
| `GET` | `/loyalty/settings` | Get loyalty program settings + tier configs |
| `PUT` | `/loyalty/settings` | Upsert settings (create or update singleton) |
| `PUT` | `/loyalty/tiers` | Upsert tier configurations (atomic replace) |

### Rewards Catalogue

| Method | Route | Description |
|---|---|---|
| `GET` | `/loyalty/rewards` | List rewards (paginated, optional `activeOnly` filter) |
| `POST` | `/loyalty/rewards` | Create reward |
| `PUT` | `/loyalty/rewards/{id}` | Update reward |
| `PATCH` | `/loyalty/rewards/{id}/status` | Toggle reward active/inactive |

### Dashboard

| Method | Route | Description |
|---|---|---|
| `GET` | `/loyalty/dashboard?from=&to=` | Dashboard: total members, period points, tier distribution, top 10 |

### Members

| Method | Route | Description |
|---|---|---|
| `POST` | `/loyalty/members` | Manually enroll customer (body: `{ customerId }`) |
| `GET` | `/loyalty/members/by-customer/{customerId}` | Get membership card by customer ID |
| `GET` | `/loyalty/members/by-card/{cardNumber}` | Get membership card by card number (QR scan) |
| `GET` | `/loyalty/members/{membershipCardId}/points` | Point history (paginated) |
| `POST` | `/loyalty/members/{membershipCardId}/redeem` | Redeem points for reward |
| `POST` | `/loyalty/members/{membershipCardId}/adjust` | Admin manual point adjustment |
| `GET` | `/loyalty/members/by-customer/{customerId}/summary` | Lightweight summary for POS |

---

## CQRS Structure

```
src/SplashSphere.Application/Features/Loyalty/
  LoyaltyDtos.cs                                    # 10 DTO records
  Commands/
    UpsertLoyaltySettings/                           # Upsert singleton settings
    UpsertLoyaltyTiers/                              # Atomic replace all tiers
    CreateLoyaltyReward/                             # Create with type-specific validation
    UpdateLoyaltyReward/                             # Update all reward fields
    ToggleLoyaltyRewardStatus/                       # Flip IsActive
    EnrollCustomer/                                  # Create MembershipCard (SS-XXXXX)
    RedeemPoints/                                    # Validate balance, deduct, ledger entry
    AdjustPoints/                                    # Manual admin correction (+/-)
  Queries/
    GetLoyaltySettings/                              # Settings + tiers via Include
    GetLoyaltyRewards/                               # Paged, optional active filter
    GetMembershipCard/                               # By CustomerId
    GetMembershipCardByNumber/                       # By CardNumber (IgnoreQueryFilters for QR)
    GetPointHistory/                                 # Paged, ordered by CreatedAt DESC
    GetLoyaltyDashboard/                             # Aggregated stats for date range
    GetCustomerLoyaltySummary/                       # Lightweight: card, tier, next tier, rewards
```

### DTOs

| DTO | Used By |
|---|---|
| `LoyaltyProgramSettingsDto` | GetSettings |
| `LoyaltyTierConfigDto` | Nested in Settings |
| `LoyaltyRewardDto` | GetRewards, includes Service/Package names |
| `MembershipCardDto` | GetMembershipCard, includes customer info |
| `PointTransactionDto` | GetPointHistory |
| `CustomerLoyaltySummaryDto` | GetCustomerLoyaltySummary (POS) |
| `AvailableRewardDto` | Nested in Summary |
| `LoyaltyDashboardDto` | GetLoyaltyDashboard |
| `TierDistributionDto` | Nested in Dashboard |
| `TopLoyalCustomerDto` | Nested in Dashboard |

---

## EF Core Configuration

```
src/SplashSphere.Infrastructure/Persistence/Configurations/
  LoyaltyProgramSettingsConfiguration.cs    # Unique index on TenantId (singleton)
  LoyaltyTierConfigConfiguration.cs         # Unique (SettingsId, Tier), cascade from Settings
  LoyaltyRewardConfiguration.cs             # FK to Service/Package with SetNull delete
  MembershipCardConfiguration.cs            # Unique (TenantId, CustomerId), unique CardNumber
  PointTransactionConfiguration.cs          # Indexes on CardId, TransactionId, (CardId, CreatedAt)
```

### DbContext Registration

5 DbSet properties on `IApplicationDbContext` and `ApplicationDbContext`:
- `LoyaltyProgramSettings`
- `LoyaltyTierConfigs`
- `LoyaltyRewards`
- `MembershipCards`
- `PointTransactions`

5 global query filters (all filtered by `TenantId`).

### Migration

`20260402105538_AddLoyaltyProgram` — creates all 5 tables with indexes and foreign keys.

---

## Event Handler: TransactionCompletedLoyaltyHandler

**File:** `src/SplashSphere.Infrastructure/Hubs/Handlers/TransactionCompletedLoyaltyHandler.cs`

Subscribes to `DomainEventNotification<TransactionCompletedEvent>`. Runs after the `UnitOfWorkBehavior` pipeline completes the main transaction save.

**Key behaviors:**
- Checks feature gate via `IPlanEnforcementService`
- Manages own `SaveChangesAsync` (runs after the UoW pipeline)
- Auto-enrolls customers when `AutoEnroll` is enabled
- Calculates points with tier multiplier
- Creates PointTransaction ledger entry with optional `ExpiresAt`
- Checks and applies tier upgrades (one-directional)
- Publishes `PointsEarnedEvent` and `TierUpgradedEvent`
- Sequential card number generation: `SS-{next:D5}`

---

## Frontend Integration

### Admin Dashboard

**Page:** `apps/admin/src/app/(dashboard)/dashboard/loyalty/page.tsx`

Three tabs:
1. **Dashboard** — 4 KPI stat cards (Total Members, Points Earned 30d, Points Redeemed 30d, Redemptions 30d), tier distribution bar chart, top 10 loyal customers
2. **Rewards** — CRUD table with create/edit Dialog, toggle active/inactive, pagination
3. **Settings** — Program settings form (earning rate, active toggle, expiration, auto-enroll), tier configuration grid with add/remove rows

**Hooks:** `apps/admin/src/hooks/use-loyalty.ts`
- `useLoyaltySettings()` / `useUpsertLoyaltySettings()` / `useUpsertLoyaltyTiers()`
- `useLoyaltyRewards()` / `useCreateLoyaltyReward()` / `useUpdateLoyaltyReward()` / `useToggleLoyaltyRewardStatus()`
- `useLoyaltyDashboard(from, to)`
- `useMembershipCard(customerId)` / `useEnrollMember()` / `usePointHistory()` / `useRedeemPoints()` / `useAdjustPoints()`
- `useCustomerLoyaltySummary(customerId)`

**Sidebar:** "Loyalty" under People section, feature-gated with `FeatureKeys.CustomerLoyalty`, uses `Award` icon from lucide-react.

### POS Integration

**Hook:** `apps/pos/src/lib/use-loyalty.ts`
- `useCustomerLoyalty(customerId)` — fetches `CustomerLoyaltySummaryDto`, enabled only when customerId is set, 30s staleTime

**Transaction Creation (`/transactions/new`):**
- When a vehicle is looked up and `customerId` resolves, the loyalty summary is fetched
- Vehicle info section shows: tier badge (amber) + points balance alongside "Linked customer"
- Totals section shows estimated points to be earned (based on `customerPayable / 100`)

**Transaction Detail (`/transactions/[id]`):**
- Customer section shows `+{pointsEarned} loyalty points earned` when `pointsEarned > 0`

### Shared TypeScript Types

**Enums** (`packages/types/src/enums.ts`):
- `LoyaltyTier` — Standard=0, Silver=1, Gold=2, Platinum=3
- `PointTransactionType` — Earned=0, Redeemed=1, Expired=2, Adjustment=3
- `RewardType` — FreeService=0, FreePackage=1, DiscountAmount=2, DiscountPercent=3

**Interfaces** (`packages/types/src/entities.ts`):
- `LoyaltyProgramSettingsDto`, `LoyaltyTierConfigDto`, `LoyaltyRewardDto`
- `MembershipCardDto`, `PointTransactionDto`
- `CustomerLoyaltySummaryDto`, `AvailableRewardDto`
- `LoyaltyDashboardDto`, `TierDistributionDto`, `TopLoyalCustomerDto`

**Transaction update:** `pointsEarned: number` added to `TransactionSummary`

---

## Seed Data

Added as Batch 7 in `DataSeeder.SeedAsync()`.

| Data | Details |
|---|---|
| **Settings** | 1 point per P100, active, auto-enroll, 12-month expiry |
| **Tiers** | Standard (0 pts, 1.0x), Silver (500 pts, 1.25x), Gold (2000 pts, 1.5x), Platinum (5000 pts, 2.0x) |
| **Rewards** | "10% Off Next Wash" (200 pts), "P50 Off" (100 pts), "P200 Off Premium" (500 pts) |
| **Members** | Jose Santos (SS-00001, Silver, 650 balance, 850 lifetime), Maria Cruz (SS-00002, Standard, 120 balance) |
| **Point History** | 3 entries for Jose (2 earned + 1 redeemed), 1 entry for Maria (earned) |

---

## Feature Gate

The loyalty feature is gated by `FeatureKeys.CustomerLoyalty`:
- **Backend:** `RequiresFeatureAttribute` on the route group in `LoyaltyEndpoints.cs`
- **Frontend (Admin):** Sidebar nav item uses `feature: FeatureKeys.CustomerLoyalty` (locked with upgrade tooltip for lower plans)
- **Auto-award handler:** Checks `IPlanEnforcementService.HasFeatureAsync` before processing

Available on: **Growth**, **Enterprise**, and **Trial** plans.

---

## File Index

### Domain
- `src/SplashSphere.Domain/Enums/LoyaltyTier.cs`
- `src/SplashSphere.Domain/Enums/PointTransactionType.cs`
- `src/SplashSphere.Domain/Enums/RewardType.cs`
- `src/SplashSphere.Domain/Entities/LoyaltyProgramSettings.cs`
- `src/SplashSphere.Domain/Entities/LoyaltyTierConfig.cs`
- `src/SplashSphere.Domain/Entities/LoyaltyReward.cs`
- `src/SplashSphere.Domain/Entities/MembershipCard.cs`
- `src/SplashSphere.Domain/Entities/PointTransaction.cs`
- `src/SplashSphere.Domain/Events/LoyaltyEvents.cs`

### Application (CQRS)
- `src/SplashSphere.Application/Features/Loyalty/LoyaltyDtos.cs`
- `src/SplashSphere.Application/Features/Loyalty/Commands/` — 8 command folders
- `src/SplashSphere.Application/Features/Loyalty/Queries/` — 7 query folders

### Infrastructure
- `src/SplashSphere.Infrastructure/Persistence/Configurations/LoyaltyProgramSettingsConfiguration.cs`
- `src/SplashSphere.Infrastructure/Persistence/Configurations/LoyaltyTierConfigConfiguration.cs`
- `src/SplashSphere.Infrastructure/Persistence/Configurations/LoyaltyRewardConfiguration.cs`
- `src/SplashSphere.Infrastructure/Persistence/Configurations/MembershipCardConfiguration.cs`
- `src/SplashSphere.Infrastructure/Persistence/Configurations/PointTransactionConfiguration.cs`
- `src/SplashSphere.Infrastructure/Hubs/Handlers/TransactionCompletedLoyaltyHandler.cs`
- `src/SplashSphere.Infrastructure/Persistence/Migrations/20260402105538_AddLoyaltyProgram.cs`

### API
- `src/SplashSphere.API/Endpoints/LoyaltyEndpoints.cs`

### Frontend (Admin)
- `apps/admin/src/hooks/use-loyalty.ts`
- `apps/admin/src/app/(dashboard)/dashboard/loyalty/page.tsx`
- `apps/admin/src/components/layout/app-sidebar.tsx` (updated)

### Frontend (POS)
- `apps/pos/src/lib/use-loyalty.ts`
- `apps/pos/src/app/(terminal)/transactions/new/page.tsx` (updated)
- `apps/pos/src/app/(terminal)/transactions/[id]/page.tsx` (updated)

### Shared Types
- `packages/types/src/enums.ts` (3 new enums)
- `packages/types/src/entities.ts` (10 new interfaces, `TransactionSummary` updated)

### Seed Data
- `src/SplashSphere.Infrastructure/Persistence/DataSeeder.cs` (`SeedLoyaltyProgram` method)
