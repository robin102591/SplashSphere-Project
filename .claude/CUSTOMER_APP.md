# SplashSphere — Customer App (SplashSphere Connect)

> **Phase:** Future roadmap (Phase 22). Build after core platform is stable with 20+ active tenants.
> **App name:** SplashSphere Connect — the customer-facing companion app.
> **Tech:** Next.js 16 (separate app in the monorepo: `apps/customer/`). PWA-installable.
> **This is NOT a native mobile app.** It's a responsive web app optimized for phones. Consider TWA (Trusted Web Activity) for Google Play listing later.
> **Audience:** End customers of car wash businesses that use SplashSphere.
> **Multi-tenant aware:** A customer can belong to multiple car wash organizations (tenants).

---

## Why This Matters

Right now, the car wash owner's customers have no digital touchpoint. They drive up, wait in line, pay, and leave. No relationship beyond the physical visit. SplashSphere Connect changes that:

- **The customer books online** — arrives at their scheduled time, no waiting
- **The car wash sees the booking** — it appears in the queue automatically with priority
- **The customer manages their own vehicles** — no cashier data entry, fewer errors
- **Loyalty happens passively** — points accumulate, tiers upgrade, referrals earn rewards
- **The owner gets a marketing channel** — push notifications, promos, re-engagement

This also gives SplashSphere a defensible moat: customers download the app for *one* car wash, but it works across *all* SplashSphere-powered car washes. Network effect.

---

## How Multi-Tenancy Works for Customers

A single customer can visit multiple car wash businesses that use SplashSphere. Their experience:

```
Maria opens SplashSphere Connect:

┌───────────────────────────────────┐
│ My Car Washes                     │
│                                   │
│ 🏢 AquaShine - Makati   [Book]   │
│    Gold member • 1,240 pts        │
│    Last visit: 3 days ago         │
│                                   │
│ 🏢 SpeedyWash - Cebu    [Book]   │
│    Silver member • 620 pts        │
│    Last visit: 2 weeks ago        │
│                                   │
│ [+ Find a Car Wash]              │
└───────────────────────────────────┘
```

**Data model:** The existing `Customer` entity is tenant-scoped — a customer can have separate records in different tenants. The Connect app links them via phone number or email (the customer's global identity). A `ConnectUser` entity bridges the customer's app account to their per-tenant Customer records.

```csharp
public sealed class ConnectUser
{
    public string Id { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;         // Primary identifier (PH phone)
    public string? Email { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Links to per-tenant Customer records
    public List<ConnectUserTenantLink> TenantLinks { get; set; } = [];
    // Global vehicle registry
    public List<ConnectVehicle> Vehicles { get; set; } = [];
}

public sealed class ConnectUserTenantLink
{
    public string Id { get; set; } = string.Empty;
    public string ConnectUserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;     // The tenant's Customer record
    public bool IsActive { get; set; } = true;
    public DateTime LinkedAt { get; set; }
}

// Customer's vehicle — lightweight, NO vehicle type or size.
// Type/size is assigned by the car wash cashier on first visit (stored in tenant's Car entity).
public sealed class ConnectVehicle
{
    public string Id { get; set; } = string.Empty;
    public string ConnectUserId { get; set; } = string.Empty;
    public string MakeId { get; set; } = string.Empty;        // Toyota, Honda, etc.
    public string ModelId { get; set; } = string.Empty;        // Vios, City, etc.
    public string PlateNumber { get; set; } = string.Empty;    // PH format: ABC-1234
    public string? Color { get; set; }
    public int? Year { get; set; }
    // NO VehicleTypeId — assigned by cashier on arrival
    // NO SizeId — assigned by cashier on arrival
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## Core Features

### 1. Customer Profile & Vehicle Management

**Self-service profile** — the customer creates and maintains their own info, reducing cashier data entry.

```
┌── My Profile ─────────────────────────────────────────┐
│                                                        │
│  👤 Maria Santos                          [Edit]      │
│  📱 0917-123-4567                                     │
│  ✉️  maria.santos@gmail.com                           │
│                                                        │
│  ── My Vehicles ────────────────────────── [+ Add] ── │
│                                                        │
│  🚗 Toyota Vios 2020 • White                          │
│     ABC-1234                                           │
│     Last wash: Mar 22, 2026 at AquaShine              │
│                                                        │
│  🚙 Mitsubishi Xpander 2023 • Black                   │
│     DEF-5678                                           │
│     Last wash: Mar 15, 2026 at SpeedyWash             │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**Add Vehicle flow:**
1. Enter plate number (PH format validation)
2. Select make → model (searchable dropdown, pre-populated from global make/model list)
3. Optional: color, year
4. Vehicle saved to `ConnectVehicle` (global)
5. **No vehicle type or size selection** — the customer does NOT classify their vehicle. This prevents gaming (e.g., Fortuner owner selecting "Sedan/Small" to get cheaper prices). Vehicle type and size are assigned by the cashier/attendant when the car physically arrives at the car wash.

**Vehicle sync:** When a customer books at a specific car wash, the API checks if that tenant already has a `Car` record matching the plate number:
- **Car exists in tenant (return visit):** Link to existing record. Vehicle type/size already assigned → exact pricing shown.
- **Car doesn't exist (first visit):** No `Car` record created yet. Vehicle type/size unknown → price ranges shown. The `Car` entity (with type/size) is created by the cashier when the customer arrives and the vehicle is assessed.

**Why the cashier assigns type/size, not the customer:**
- Prevents pricing fraud (customers selecting smaller classification for cheaper services)
- Matches real-world operations (attendants look at the car and decide classification)
- After first visit, all future bookings at that car wash show exact prices automatically

### 2. Find & Join a Car Wash

**Discovery page** — find car washes powered by SplashSphere.

```
┌── Find a Car Wash ────────────────────────────────────┐
│                                                        │
│  🔍 [Search by name or location...            ]       │
│                                                        │
│  📍 Near You                                           │
│                                                        │
│  🏢 AquaShine Car Wash — Makati                       │
│     ⭐ 4.8 • 2.3 km away • Open until 6 PM           │
│     Services: Basic Wash, Premium, Wax, Detailing     │
│     [View] [Join & Book]                               │
│                                                        │
│  🏢 CleanDrive Auto Spa — BGC                         │
│     ⭐ 4.5 • 5.1 km away • Open until 7 PM           │
│     Services: Basic Wash, Premium, Interior            │
│     [View] [Join & Book]                               │
│                                                        │
│  🏢 SpeedyWash — Quezon City                          │
│     ⭐ 4.6 • 8.7 km away • Open until 5 PM           │
│     Services: Basic, Premium, Full Detail              │
│     [View] [Join & Book]                               │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**"Join & Book"** creates a `ConnectUserTenantLink` — the customer is now linked to that tenant. A `Customer` record is auto-created in the tenant's database (or linked if phone/email matches an existing record the cashier created).

**Tenant listing criteria:** Only tenants with `isActive = true` and a plan that supports the Connect app (Growth+ plan feature). Tenants can opt out of public listing.

### 3. Online Booking (Appointment Scheduling)

The core feature. A customer books a time slot, which automatically creates a queue entry with priority.

```
┌── Book at AquaShine - Makati ─────────────────────────┐
│                                                        │
│  Select Vehicle                                        │
│  ┌─────────────────────────────────────────────────┐  │
│  │ 🚗 Toyota Vios 2020 (ABC-1234) • White          │  │
│  │     ✅ Classified: Sedan/Medium                  │  │
│  │ 🚙 Mitsubishi Xpander 2023 (DEF-5678) • Black   │  │
│  │     ⏳ Not yet classified at this car wash       │  │
│  └─────────────────────────────────────────────────┘  │
│                                                        │
│  ── If vehicle IS classified (return visit): ──       │
│  Select Services                                       │
│  ┌─────────────────────────────────────────────────┐  │
│  │ ✓ Basic Wash .......................... ₱200     │  │
│  │ ✓ Tire & Rim Shine ................... ₱120     │  │
│  │ ○ Wax & Polish ....................... ₱450     │  │
│  │ ○ Interior Vacuum .................... ₱150     │  │
│  └─────────────────────────────────────────────────┘  │
│  Prices for Sedan/Medium ✓                            │
│                                                        │
│  ── If vehicle NOT classified (first visit): ──       │
│  Select Services                                       │
│  ┌─────────────────────────────────────────────────┐  │
│  │ ✓ Basic Wash .................. ₱180 – ₱350     │  │
│  │ ✓ Tire & Rim Shine ........... ₱100 – ₱180     │  │
│  │ ○ Wax & Polish ............... ₱350 – ₱600     │  │
│  │ ○ Interior Vacuum ............ ₱120 – ₱250     │  │
│  └─────────────────────────────────────────────────┘  │
│  ⚠️ Final price confirmed at vehicle assessment       │
│                                                        │
│  Select Date & Time                                    │
│  ┌─────────────────────────────────────────────────┐  │
│  │  ← March 2026 →                                 │  │
│  │  Su Mo Tu We Th Fr Sa                           │  │
│  │           1  2  3  4  [5]                       │  │
│  │  6  7  8  9  10 11 12                           │  │
│  └─────────────────────────────────────────────────┘  │
│                                                        │
│  Available Slots — Saturday, March 5                   │
│  [8:00 AM] [8:30 AM] [9:00 AM] [9:30 AM]             │
│  [10:00]   [10:30]   [11:00]   [11:30]                │
│  [1:00 PM] [1:30 PM] [2:00 PM] [2:30 PM]             │
│                                                        │
│  ── Summary (classified vehicle) ────────────────    │
│  Toyota Vios • Basic Wash + Tire Shine                │
│  Saturday, Mar 5 at 9:00 AM                           │
│  Estimated duration: 40 min                           │
│  Total: ₱320                                          │
│  Points earned: 32 pts                                │
│                                                        │
│  ── Summary (unclassified vehicle) ──────────────    │
│  Mitsubishi Xpander • Basic Wash + Tire Shine         │
│  Saturday, Mar 5 at 9:00 AM                           │
│  Estimated duration: 40 min                           │
│  Estimated Total: ₱280 – ₱530                        │
│  *Final price confirmed when vehicle is assessed      │
│                                                        │
│              [Book Appointment]                        │
└────────────────────────────────────────────────────────┘
```

**How slots work:**

The tenant configures booking settings in their admin dashboard:
- Operating hours (e.g., 8 AM - 6 PM)
- Slot interval (30 min default)
- Max bookings per slot (based on number of bays/capacity, e.g., 3 concurrent)
- Advance booking window (1-7 days ahead)
- Minimum lead time (e.g., 2 hours before slot)
- Auto-cancel if no-show after 15 minutes

```csharp
public sealed class BookingSetting
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public TimeOnly OpenTime { get; set; }                 // 08:00
    public TimeOnly CloseTime { get; set; }                // 18:00
    public int SlotIntervalMinutes { get; set; } = 30;
    public int MaxBookingsPerSlot { get; set; } = 3;
    public int AdvanceBookingDays { get; set; } = 7;
    public int MinLeadTimeMinutes { get; set; } = 120;     // Book at least 2 hours ahead
    public int NoShowGraceMinutes { get; set; } = 15;
    public bool IsBookingEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class Booking
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string ConnectUserId { get; set; } = string.Empty;
    public string ConnectVehicleId { get; set; } = string.Empty;  // Always set (customer's vehicle)
    public string? CarId { get; set; }                            // Set only if tenant has classified this vehicle
    public DateTime SlotStart { get; set; }
    public DateTime SlotEnd { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public bool IsVehicleClassified { get; set; }                 // true = exact prices, false = estimated
    public decimal EstimatedTotal { get; set; }                   // Exact if classified, midpoint estimate if not
    public decimal? EstimatedTotalMin { get; set; }               // Only set if not classified (price range low)
    public decimal? EstimatedTotalMax { get; set; }               // Only set if not classified (price range high)
    public int EstimatedDurationMinutes { get; set; }
    public string? QueueEntryId { get; set; }                     // Created when customer arrives or at slot time
    public string? TransactionId { get; set; }                    // Created when service starts
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<BookingService> Services { get; set; } = [];
}

public sealed class BookingService
{
    public string Id { get; set; } = string.Empty;
    public string BookingId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public decimal? Price { get; set; }                           // Exact price (if vehicle classified)
    public decimal? PriceMin { get; set; }                        // Range low (if not classified)
    public decimal? PriceMax { get; set; }                        // Range high (if not classified)
}

public enum BookingStatus
{
    Confirmed,       // Booking accepted
    Arrived,         // Customer checked in (auto or manual)
    InService,       // Service started (linked to transaction)
    Completed,       // Service done
    Cancelled,       // Customer or tenant cancelled
    NoShow           // Customer didn't arrive within grace period
}
```

### 4. Booking → Queue Integration (Automatic Priority)

**This is the key integration.** When a customer books online, the system automatically handles the queue:

```
BOOKING FLOW → QUEUE INTEGRATION

1. Customer books: "Basic Wash, Saturday 9:00 AM"
   → Booking record created (status: Confirmed)
   → SMS confirmation sent: "Your booking at AquaShine for 9:00 AM is confirmed!"

2. 15 minutes before slot (8:45 AM):
   → Hangfire job creates QueueEntry automatically
   → Priority: BOOKED (higher than walk-in NORMAL)
   → Status: WAITING
   → POS queue board shows: "ABC-1234 — BOOKED 9:00 AM" with a 📅 badge

3. Customer arrives:
   Option A: Customer taps "I'm here" in the app → status = ARRIVED
   Option B: Cashier sees them and checks them in on POS → status = ARRIVED
   
4. Queue board calls the booked customer at their slot time:
   → They get priority over walk-ins
   → If earlier slot is available, they can be called sooner
   
5. Service begins:
   → Transaction created from booking data (pre-filled services, vehicle, customer)
   → Booking status = InService
   
6. No-show handling:
   → If customer doesn't arrive within 15 min of slot → BookingStatus = NoShow
   → QueueEntry status = NO_SHOW
   → Slot freed for walk-ins
   → Customer SMS: "You missed your 9:00 AM booking at AquaShine. Book again?"
```

**Queue priority hierarchy:**
1. **VIP** — Platinum/Gold loyalty tier customers (highest)
2. **BOOKED** — Online booking customers (second)
3. **EXPRESS** — Paid express queue (if offered)
4. **NORMAL** — Walk-in customers (standard)

### 5. Loyalty Membership

The app surfaces the existing loyalty system (Phase 15) with a customer-facing experience.

```
┌── AquaShine — My Membership ──────────────────────────┐
│                                                        │
│  ┌──────────────────────────────────────────────────┐ │
│  │        🥇 GOLD MEMBER                            │ │
│  │                                                  │ │
│  │   1,240 points                                   │ │
│  │   ████████████████████░░░░░░░  62% to Platinum   │ │
│  │   760 more points to Platinum                    │ │
│  │                                                  │ │
│  │   Member since: Jan 2026 • 28 visits             │ │
│  └──────────────────────────────────────────────────┘ │
│                                                        │
│  ── Your Benefits ────────────────────────────────── │
│  ✓ 10% discount on all services                      │
│  ✓ Priority queue (VIP)                              │
│  ✓ Free birthday wash                                │
│  ✓ Double points on weekdays                         │
│                                                        │
│  ── Recent Points Activity ──────────────────────── │
│  +32 pts   Basic Wash + Tire Shine       Mar 22      │
│  +48 pts   Premium Wash                  Mar 15      │
│  -200 pts  Redeemed: ₱100 discount       Mar 10      │
│  +100 pts  Referral: Pedro joined         Mar 8       │
│                                                        │
│  ── Available Rewards ──────────── [Redeem] ──────── │
│  🎁 ₱50 discount ............. 500 pts               │
│  🎁 ₱100 discount ............ 1,000 pts             │
│  🎁 Free Basic Wash .......... 800 pts               │
│  🎁 Free Interior Vacuum ..... 600 pts               │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**Tier progression (per tenant — each car wash has its own tiers):**

| Tier | Points Required | Benefits |
|---|---|---|
| Bronze | 0 | 5% discount, standard queue |
| Silver | 500 | 7% discount, earn 1.5× points |
| Gold | 1,000 | 10% discount, VIP queue priority, birthday wash |
| Platinum | 2,000 | 15% discount, VIP queue, birthday wash, double points on weekdays, free monthly basic wash |

**Points earning:** Configured by each tenant. Default: 1 point per ₱10 spent.

**Redemption:** Customer taps "Redeem" in the app → generates a redemption code → shows it to cashier at POS → cashier applies discount.

### 6. Referral System

```
┌── Refer a Friend ─────────────────────────────────────┐
│                                                        │
│  Share your referral code and earn 100 points          │
│  when they complete their first wash!                  │
│                                                        │
│  Your Code: MARIA-AQUA-7X3F                           │
│                                                        │
│  [📋 Copy Code]  [📱 Share via SMS]  [📲 Share Link]  │
│                                                        │
│  ── Your Referrals ──────────────────────────────── │
│  ✅ Pedro Garcia — joined Mar 8 — 100 pts earned      │
│  ✅ Ana Reyes — joined Mar 12 — 100 pts earned        │
│  ⏳ Carlos Rivera — signed up, hasn't washed yet      │
│                                                        │
│  Total earned from referrals: 200 pts                  │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**How referrals work:**
1. Maria shares her referral code/link
2. Pedro signs up on the Connect app and enters Maria's code (or uses the referral link which auto-fills)
3. Pedro books and completes his first wash at AquaShine
4. Both Maria and Pedro earn referral points:
   - Maria (referrer): 100 points
   - Pedro (referred): 50 bonus points on first wash
5. The referral is tracked per tenant — Maria's code works for AquaShine, not for SpeedyWash

```csharp
public sealed class Referral
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ReferrerCustomerId { get; set; } = string.Empty;
    public string ReferredCustomerId { get; set; } = string.Empty;
    public string ReferralCode { get; set; } = string.Empty;
    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;
    public int ReferrerPointsEarned { get; set; }
    public int ReferredPointsEarned { get; set; }
    public DateTime? CompletedAt { get; set; }           // When referred customer completed first wash
    public DateTime CreatedAt { get; set; }
}

public enum ReferralStatus { Pending, Completed, Expired }
```

### 7. Live Queue Status

Customers can see their position in real-time without being at the car wash:

```
┌── Your Queue Status ──────────────────────────────────┐
│                                                        │
│  📅 Booked: Today 9:00 AM at AquaShine - Makati      │
│                                                        │
│  ┌──────────────────────────────────────────────────┐ │
│  │  Queue Position: #2                               │ │
│  │  Estimated wait: ~15 minutes                      │ │
│  │  Status: WAITING                                  │ │
│  │                                                   │ │
│  │  █████████████████░░░░░░░░░░░░░░ Almost there!    │ │
│  └──────────────────────────────────────────────────┘ │
│                                                        │
│  [I'm Here ✓]  (tap when you arrive)                  │
│                                                        │
│  Live updates via push notification:                  │
│  "You're next! Head to Bay 2"                         │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**After service completes:**

```
┌── Service Complete! ──────────────────────────────────┐
│                                                        │
│  ✅ Your Toyota Vios is ready!                        │
│                                                        │
│  Services: Basic Wash + Tire Shine                    │
│  Total: ₱320                                          │
│  Points earned: +32 pts                               │
│  New balance: 1,272 pts                               │
│                                                        │
│  ⭐ Rate your experience                              │
│  ☆ ☆ ☆ ☆ ☆                                           │
│                                                        │
│  [Book Again]  [View Receipt]                         │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### 8. Service History

```
┌── My Wash History ────────────────────────────────────┐
│                                                        │
│  Filter: [All ▾]  [AquaShine ▾]  [All vehicles ▾]    │
│                                                        │
│  Mar 22, 2026 — AquaShine Makati                      │
│  🚗 Toyota Vios • Basic Wash + Tire Shine             │
│  ₱320 • +32 pts • ⭐⭐⭐⭐⭐                          │
│                                                        │
│  Mar 15, 2026 — SpeedyWash Cebu                       │
│  🚙 Xpander • Premium Wash                            │
│  ₱450 • +45 pts • ⭐⭐⭐⭐                             │
│                                                        │
│  Mar 10, 2026 — AquaShine Makati                      │
│  🚗 Toyota Vios • Full Detailing                      │
│  ₱1,500 • +150 pts • ⭐⭐⭐⭐⭐                        │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### 9. Push Notifications

| Notification | Trigger | Message |
|---|---|---|
| Booking confirmed | After booking | "Your booking at AquaShine for Sat 9:00 AM is confirmed!" |
| Booking reminder | 2 hours before slot | "Reminder: Your car wash is in 2 hours at AquaShine" |
| Queue update | Position changes | "You're now #2 in line at AquaShine!" |
| Called to bay | POS calls customer | "You're next! Head to Bay 2 at AquaShine" |
| Car ready | Service completed | "Your Toyota Vios is ready for pickup!" |
| Points earned | After transaction | "You earned 32 points! Total: 1,272 pts" |
| Tier upgrade | Points hit threshold | "Congratulations! You've reached Gold tier!" |
| Referral completed | Referred friend completes first wash | "Pedro completed his first wash — you earned 100 pts!" |
| Promo | Tenant sends promotion | "This weekend: Double points on all services!" |
| Inactive reminder | No visit in 30 days | "We miss you! Visit this week for bonus points" |

---

## API Endpoints (Customer-Facing)

Separate route prefix: `/api/v1/connect/` — different auth from the admin/POS JWT.

### Auth
| Method | Route | Description |
|---|---|---|
| `POST` | `/connect/auth/send-otp` | Send OTP to phone number |
| `POST` | `/connect/auth/verify-otp` | Verify OTP → return session token |
| `POST` | `/connect/auth/refresh` | Refresh session token |

### Profile & Vehicles
| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/profile` | Get current user profile |
| `PUT` | `/connect/profile` | Update name, email, avatar |
| `GET` | `/connect/vehicles` | List user's vehicles (make, model, plate, color, year — no type/size) |
| `POST` | `/connect/vehicles` | Add a vehicle (make, model, plate, color, year only) |
| `PUT` | `/connect/vehicles/{id}` | Update vehicle details |
| `DELETE` | `/connect/vehicles/{id}` | Remove vehicle |

### Car Wash Discovery
| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes` | List available car washes (search, location) |
| `GET` | `/connect/carwashes/{tenantId}` | Car wash detail (services, hours, location) |
| `POST` | `/connect/carwashes/{tenantId}/join` | Join/link to a car wash |
| `GET` | `/connect/my-carwashes` | List user's linked car washes |

### Services & Pricing
| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes/{tenantId}/services?vehicleId={id}` | List services with pricing — returns exact prices if vehicle is classified at this tenant, or price ranges (min/max across all vehicle types/sizes) if not yet classified |

Note: The `vehicleId` is the `ConnectVehicle.Id`. The API checks if a matching `Car` record exists in the tenant (matched by plate number). If yes → looks up `ServicePricing` for that vehicle's type/size → returns exact prices. If no → queries all `ServicePricing` rows for the service → returns min/max range.

### Booking
| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes/{tenantId}/slots` | Get available time slots for a date |
| `POST` | `/connect/bookings` | Create a booking |
| `GET` | `/connect/bookings` | List my bookings (upcoming + past) |
| `GET` | `/connect/bookings/{id}` | Booking detail with queue status |
| `PATCH` | `/connect/bookings/{id}/cancel` | Cancel a booking |
| `PATCH` | `/connect/bookings/{id}/arrived` | Mark as arrived ("I'm here") |

### Loyalty
| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes/{tenantId}/loyalty` | Get my membership at this car wash |
| `GET` | `/connect/carwashes/{tenantId}/rewards` | List available rewards to redeem |
| `POST` | `/connect/carwashes/{tenantId}/rewards/redeem` | Redeem points for a reward |
| `GET` | `/connect/carwashes/{tenantId}/points-history` | Points earning/spending history |

### Referrals
| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/carwashes/{tenantId}/referral-code` | Get my referral code |
| `GET` | `/connect/carwashes/{tenantId}/referrals` | List my referrals and their status |
| `POST` | `/connect/auth/apply-referral` | Apply a referral code during/after signup |

### Queue
| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/queue/active` | Get my current queue position (if any) |

### History
| Method | Route | Description |
|---|---|---|
| `GET` | `/connect/history` | Service history across all car washes |

---

## Authentication (OTP-Based, Not Clerk)

Customers don't use Clerk (that's for tenant staff). They authenticate via **phone OTP** — the most natural auth method for Filipino consumers.

```
Flow:
1. Enter phone number: 0917-123-4567
2. Receive SMS OTP via Semaphore: "Your SplashSphere code is 482917"
3. Enter 6-digit code
4. If new user: create ConnectUser, prompt for name
5. If returning user: log in, show dashboard
6. Session: JWT with 30-day expiry + refresh token
```

OTP costs ~₱0.50 per SMS via Semaphore. For 1,000 customer sign-ins/month: ~₱500.

---

## Tenant Admin — Booking Settings

The tenant needs a settings page to configure online booking:

```
/settings/booking:

┌── Online Booking Settings ────────────────────────────┐
│                                                        │
│  Enable online booking:     [✓]                       │
│                                                        │
│  Operating hours:                                      │
│  Open: [8:00 AM ▾]    Close: [6:00 PM ▾]             │
│                                                        │
│  Slot interval:             [30 min ▾]                │
│  Max bookings per slot:     [3      ]                 │
│  Advance booking window:    [7 days ▾]                │
│  Minimum lead time:         [2 hours ▾]               │
│  No-show grace period:      [15 min ▾]                │
│                                                        │
│  Show in public directory:  [✓]                       │
│  (Customers can find you via SplashSphere Connect)     │
│                                                        │
│  Referral reward (referrer): [100] points              │
│  Referral reward (referred): [50] points               │
│                                                        │
│                                  [Save Settings]       │
└────────────────────────────────────────────────────────┘
```

On the POS queue board, booked customers appear with a 📅 badge and their scheduled time. The cashier sees them in the queue and processes them like any other customer — the difference is they were pre-registered and have priority.

---

## Plan Gating

| Feature | Starter | Growth | Enterprise |
|---|---|---|---|
| Listed in Connect app directory | ✗ | ✓ | ✓ |
| Online booking | ✗ | ✓ | ✓ |
| Customer self-registration | ✗ | ✓ | ✓ |
| Loyalty in Connect app | ✗ | ✓ | ✓ |
| Referral program | ✗ | ✓ | ✓ |
| Push notifications to customers | ✗ | ✓ (50/mo) | ✓ (unlimited) |
| Booking customization | ✗ | Standard | Advanced (slot rules, capacity) |

---

## Hangfire Jobs

| Job | Schedule | Description |
|---|---|---|
| `CreateQueueFromBookings` | Every 5 min | For bookings with slot time in the next 15 min, auto-create QueueEntry if not already created |
| `SendBookingReminders` | Hourly | SMS reminders 2 hours before slot |
| `MarkNoShows` | Every 5 min | Mark bookings as NoShow if slot time + grace period has passed and customer didn't arrive |
| `ExpireReferrals` | Daily | Expire referral codes older than 90 days that were never used |

---

## Claude Code Prompts — Phase 22

### Prompt 22.1 — Connect Backend (Domain + Infrastructure)

```
Add the Customer Connect system:

Domain/Entities:
- ConnectUser, ConnectUserTenantLink, ConnectVehicle
- BookingSetting, Booking, BookingService, BookingStatus enum
- Referral, ReferralStatus enum

Infrastructure:
- EF configs with proper indexes
- OTP authentication service (generate, store, verify 6-digit codes)
  OTP stored in Redis with 5-minute expiry
- JWT token generation for Connect users (separate from Clerk JWT)
- Migration: "AddCustomerConnect"
```

### Prompt 22.2 — Connect Backend (Application Layer)

```
CQRS features for Connect:

Auth: SendOtpCommand, VerifyOtpCommand, RefreshTokenCommand
Profile: UpdateProfileCommand, CRUD for ConnectVehicle (make, model, plate, color, year — NO type/size)
Discovery: SearchCarWashesQuery, GetCarWashDetailQuery, JoinCarWashCommand
Services: GetServicesWithPricingQuery — takes tenantId + connectVehicleId:
  - Looks up plate number in tenant's Car table
  - If Car exists (return visit): return exact prices from ServicePricing for that type/size
  - If Car doesn't exist (first visit): return price ranges (min/max across all type/size combos)
  - Response includes priceMode: "exact" | "estimate" and vehicleClassification: {...} | null
Booking: GetAvailableSlotsQuery, CreateBookingCommand, CancelBookingCommand,
         MarkArrivedCommand
Loyalty: GetMembershipQuery, GetRewardsQuery, RedeemRewardCommand,
         GetPointsHistoryQuery
Referral: GetReferralCodeQuery, ApplyReferralCommand, GetMyReferralsQuery
Queue: GetActiveQueuePositionQuery
History: GetServiceHistoryQuery

Booking → Queue integration:
- CreateBookingCommand validates slot availability, creates Booking record
  - Sets IsVehicleClassified based on whether tenant Car exists
  - Stores exact Price or PriceMin/PriceMax on BookingService
- Hangfire job CreateQueueFromBookings: creates QueueEntry 15 min before slot
  with priority = BOOKED
- MarkArrivedCommand updates both Booking and QueueEntry status
- When POS creates Transaction from queue entry, link back to Booking

Vehicle flow:
- Customer registers ConnectVehicle with make/model/plate (NO type/size)
- On first visit: cashier sees vehicle info, assigns VehicleType + Size in POS
  → creates Car entity in tenant's database with the classification
- On return visits: Car already exists → exact pricing shown in Connect app
- ConnectVehicle is NEVER synced to Car automatically — Car is only created by the cashier

API routes under /api/v1/connect/ with OTP auth middleware
```

### Prompt 22.3 — Connect Frontend (Customer App)

```
Create the customer app: apps/customer/ (Next.js 16 in the monorepo)

Pages:
- / — Home: "My Car Washes" list with loyalty summary per tenant
- /auth — Phone OTP sign-in
- /profile — Profile + vehicle management (add vehicle: make, model, plate, color, year ONLY — no type/size)
- /discover — Search/browse car washes, join
- /carwash/[tenantId] — Car wash detail (services, hours, reviews)
- /carwash/[tenantId]/book — Booking wizard (vehicle → services → date/slot → confirm)
  Vehicle selection: show classification status per vehicle ("Classified: Sedan/Medium" or "Not yet classified")
  Services: if vehicle classified → show exact prices. If not → show price ranges (₱180 – ₱350)
  with note: "Final price confirmed when your vehicle is assessed"
  Booking summary: exact total OR estimated range
- /carwash/[tenantId]/membership — Loyalty tier, points, rewards, referrals
- /bookings — My upcoming + past bookings
- /bookings/[id] — Booking detail with live queue status
- /history — Service history across all car washes

Design: Mobile-first (phone-optimized). SplashSphere brand colors.
56px touch targets. Bottom tab navigation: Home, Book, History, Profile.
PWA manifest for home screen installation.

Real-time queue updates via SSE or polling (not SignalR — 
lighter weight for customer app).
```

### Prompt 22.4 — Admin Booking Integration

```
Add booking support to the existing admin + POS apps:

Admin:
- /settings/booking — Booking settings page (operating hours, slots, capacity)
- /bookings — View all bookings (calendar view + list view)

POS:
- Queue board: show 📅 badge on booked customers, display scheduled time
- "Check In" button for when booked customer arrives
- Vehicle classification flow (first-visit customers):
  When a booked customer arrives and their vehicle has NOT been classified at this tenant,
  the cashier sees: "Toyota Vios 2020 • ABC-1234 • White — Assign vehicle type and size"
  Cashier selects VehicleType + Size → creates the tenant's Car record → prices lock in
  → all future Connect app bookings show exact prices automatically
- Auto-fill transaction from booking data (vehicle, services, customer)
- If vehicle was unclassified: recalculate actual prices using the now-assigned type/size
  Show cashier: "Estimated: ₱280-₱530 → Actual: ₱320" for confirmation
- Mark no-show button (after grace period)

Hangfire jobs:
- CreateQueueFromBookings (every 5 min)
- SendBookingReminders (hourly)
- MarkNoShows (every 5 min)
- ExpireReferrals (daily)

Notifications: booking.confirmed, booking.reminder, booking.no_show,
              queue.position_update, service.completed
```

---

## Phase Summary

| Prompt | What | Layer |
|---|---|---|
| 22.1 | Domain entities, OTP auth, EF configs, migration | Backend |
| 22.2 | CQRS features, booking→queue integration, vehicle sync | Backend |
| 22.3 | Customer-facing Next.js app (mobile-first PWA) | Frontend (new app) |
| 22.4 | Admin booking settings, POS queue integration, Hangfire jobs | Full stack |

**Total: 4 prompts in Phase 22.**

---

## Key Design Decisions

1. **Separate app, not embedded in POS/admin.** Customers and tenant staff have fundamentally different needs, auth methods, and UX requirements. A separate `apps/customer/` app keeps concerns clean.

2. **Phone OTP auth, not Clerk.** Filipino consumers authenticate with their phone number, not email. OTP via Semaphore is cheap (~₱0.50/SMS), familiar, and doesn't require the customer to create a password. Clerk is overkill for consumer auth.

3. **Multi-tenant customer identity via phone number.** A customer's phone is their global identity across all SplashSphere car washes. `ConnectUser` holds the global profile; `ConnectUserTenantLink` connects them to each tenant's `Customer` record.

4. **Booking creates queue entry automatically.** The cashier doesn't need to manually add booked customers to the queue. A Hangfire job creates the `QueueEntry` 15 minutes before the slot with `BOOKED` priority, which sits between VIP and NORMAL.

5. **Prices captured at booking time.** If the tenant changes pricing after the customer books, the booked price is honored. For classified vehicles, `BookingService.Price` stores the exact quoted price. For unclassified vehicles (first visit), `BookingService.PriceMin` and `PriceMax` store the range — actual price is determined when the cashier classifies the vehicle at arrival.

6. **Loyalty and referrals are per-tenant.** Maria's Gold status at AquaShine doesn't transfer to SpeedyWash. Each car wash has its own loyalty program. This matches real-world expectations and lets each tenant customize their tiers.

7. **Vehicle type/size assigned by cashier, not customer.** Customers register vehicles with make, model, plate, color, and year only. The car wash cashier assigns vehicle type (Sedan/SUV/Van) and size (Small/Medium/Large/XL) when the car physically arrives. This prevents pricing fraud (customers selecting smaller classification for cheaper services) and matches real-world operations where attendants assess vehicles visually. After the first visit, all future bookings at that car wash show exact pricing automatically.

7. **Growth+ plan feature.** Online booking and the Connect app listing are upgrade incentives for Starter tenants. "Want customers to book online? Upgrade to Growth."

8. **PWA, not native.** Same rationale as the POS discussion — a web app covers 95% of needs, is cheaper to build, and avoids app store overhead. If customers demand a native app, wrap it in a TWA for Google Play.
