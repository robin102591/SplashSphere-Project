# SplashSphere — Customer Display

> **Phase:** Core feature (should ship alongside POS — Phase 3-4)
> **Route:** `pos.splashsphere.com/display`
> **Purpose:** A customer-facing second screen (tablet or monitor) that shows the transaction being built in real time.
> **Tech:** Next.js page in the POS app, SignalR for real-time updates, fullscreen kiosk mode.
> **Plan gating:** Available on all plans (Starter, Growth, Enterprise).

---

## Why This Matters

Trust and transparency. In a Philippine car wash, the customer hands over their car and waits. When the cashier says "₱320 po" — the customer has no way to verify what they're being charged for. Did the cashier add a service they didn't ask for? Was the discount applied? Is the loyalty points calculation correct?

A customer-facing display solves this:
- Customer watches their bill being built in real time → no surprises at payment
- Reduces disputes ("Hindi ko po inorder yang Wax & Polish")
- Shows loyalty points earned → reinforces the loyalty program
- Displays promos during idle time → upsells without the cashier needing to pitch
- Looks professional → customers perceive the business as modern and trustworthy

This is standard in retail (grocery POS displays, fast food order screens) but rare in Philippine car washes. It's a small feature with outsized trust impact.

---

## Hardware Requirements

**Minimum:** Any device with a browser. That's it.

| Device | Cost | Best For |
|---|---|---|
| Cheap Android tablet (8-10") | ₱3,000-5,000 | Countertop display next to POS |
| Old iPad / tablet (recycled) | Free | Reuse existing hardware |
| Small monitor + Raspberry Pi | ₱4,000-6,000 | Wall-mounted or pole-mounted display |
| Smart TV (HDMI + any device) | Varies | Large format behind the counter |

The display page is just a fullscreen web page — any device that runs a modern browser works. No app installation needed.

---

## Display States

The customer display cycles through three states:

### State 1: Idle (No Active Transaction)

Shown when the cashier has no open transaction. The display rotates between the tenant's branding and promo messages.

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│                                                         │
│                    [LOGO]                                │
│                                                         │
│              AquaShine Car Wash                          │
│          Premium car care since 2020                     │
│                                                         │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│            🚗 Welcome! Mabuhay! 🚗                      │
│                                                         │
│         "Next wash 10% off for Gold members!"           │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│                                                         │
│                Saturday, April 26, 2026                  │
│                      2:41 PM                             │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Idle screen features:**
- Logo + business name + tagline (from Company Profile)
- Rotating promo messages (from Receipt Settings `PromoText` or dedicated display messages)
- Current date and time
- Optional: social media handles, GCash QR for tips
- Rotates between messages every 8-10 seconds with fade transition

### State 2: Building (Transaction In Progress)

Updates in real time via SignalR as the cashier adds/removes services, merchandise, and packages.

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│  [Logo]  AquaShine Car Wash                             │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│  Vehicle: Toyota Vios 2020 • ABC-1234 • Sedan/Medium   │
│  Customer: Maria Santos • ⭐ Gold Member                │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  SERVICE / ITEM              QTY         AMOUNT         │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  Basic Wash                   1       ₱200.00           │
│  Tire & Rim Shine             1       ₱120.00           │
│  Air Freshener (Lavender)     1        ₱85.00           │
│                                                         │
│                                                         │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│  Subtotal                             ₱405.00           │
│  Gold Member Discount (10%)           -₱32.00           │
│  ═══════════════════════════════════════════════════    │
│                                                         │
│              TOTAL          ₱373.00                     │
│                                                         │
│  ═══════════════════════════════════════════════════    │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Building state behavior:**
- Each line item appears with a subtle slide-in animation when added
- Removed items fade out
- Total recalculates instantly on every change
- Discount line appears only if a discount is applied
- Customer info appears if a customer is linked (optional — controlled by display settings)
- Vehicle info appears if a vehicle is selected

### State 3: Complete (Payment Received)

Shown for a configurable duration (default 10 seconds) after payment is processed.

```
┌─────────────────────────────────────────────────────────┐
│                                                         │
│  [Logo]  AquaShine Car Wash                             │
│                                                         │
│  ═══════════════════════════════════════════════════    │
│                                                         │
│               ✅ PAYMENT COMPLETE                       │
│                                                         │
│  ═══════════════════════════════════════════════════    │
│                                                         │
│  Basic Wash                   1       ₱200.00           │
│  Tire & Rim Shine             1       ₱120.00           │
│  Air Freshener                1        ₱85.00           │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│  Subtotal                             ₱405.00           │
│  Gold Discount (10%)                  -₱32.00           │
│  TOTAL                                ₱373.00           │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│  Paid: Cash                           ₱400.00           │
│  Change                                ₱27.00           │
│                                                         │
│  ─────────────────────────────────────────────────────  │
│  ⭐ Points Earned: +37 pts                              │
│  ⭐ New Balance: 1,277 pts                              │
│  ─────────────────────────────────────────────────────  │
│                                                         │
│        Thank you for choosing AquaShine! 🚗✨           │
│                                                         │
│        Next wash 10% off! Show this receipt.            │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

**Complete state behavior:**
- Shows full summary with payment details
- Displays loyalty points earned and new balance (if customer is linked)
- Shows thank-you message (from Receipt Settings)
- Shows promo text (from Receipt Settings)
- Holds for configurable duration (default: 10 seconds)
- Automatically transitions back to Idle state

### State Transitions

```
         ┌──────────┐
         │   IDLE   │ ◄────────── timer (10 sec)
         └────┬─────┘                    ▲
              │                          │
     cashier starts               payment done
     new transaction              + hold timer
              │                          │
              ▼                          │
         ┌──────────┐           ┌──────────────┐
         │ BUILDING │ ────────► │   COMPLETE   │
         └──────────┘           └──────────────┘
              ▲    │
              │    │
         add/remove items
         (SignalR updates)
```

---

## SignalR Integration

### Hub Method — POS Side (Sends Updates)

The POS already uses SignalR for queue updates. Extend it with display events:

```csharp
// Events broadcast by POS to display
public interface IDisplayHub
{
    // Transaction started — display enters Building state
    Task TransactionStarted(DisplayTransactionDto transaction);

    // Line item added/removed/updated — display refreshes items
    Task TransactionUpdated(DisplayTransactionDto transaction);

    // Payment completed — display enters Complete state
    Task TransactionCompleted(DisplayCompletionDto completion);

    // Transaction voided/cancelled — display returns to Idle
    Task TransactionCancelled();
}
```

### Group Pattern

```csharp
// Display joins a specific POS station group
// Group format: display:{branchId}:{stationId}
await Groups.AddToGroupAsync(connectionId, $"display:{branchId}:{stationId}");

// POS broadcasts to its station's display group
await _hub.Clients
    .Group($"display:{branchId}:{stationId}")
    .TransactionUpdated(transactionDto);
```

**Why station-specific groups?** A branch might have 2 POS stations (2 cashiers). Each has its own customer display. Station 1's transaction shouldn't appear on Station 2's display.

### DTOs — What Gets Sent to the Display

```csharp
// Sent on TransactionStarted and TransactionUpdated
public sealed record DisplayTransactionDto
{
    public string TransactionId { get; init; } = string.Empty;

    // Vehicle (shown if available)
    public string? VehiclePlate { get; init; }
    public string? VehicleMakeModel { get; init; }
    public string? VehicleTypeSize { get; init; }

    // Customer (shown if linked and display setting allows)
    public string? CustomerName { get; init; }
    public string? LoyaltyTier { get; init; }

    // Line items
    public List<DisplayLineItem> Items { get; init; } = [];

    // Totals
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public string? DiscountLabel { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
}

public sealed record DisplayLineItem(
    string Id,
    string Name,              // "Basic Wash" or "Air Freshener (Lavender)"
    string Type,              // "service", "package", "merchandise"
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

// Sent on TransactionCompleted
public sealed record DisplayCompletionDto
{
    public DisplayTransactionDto Transaction { get; init; } = null!;

    // Payment
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal AmountPaid { get; init; }
    public decimal ChangeAmount { get; init; }

    // Loyalty
    public int? PointsEarned { get; init; }
    public int? PointsBalance { get; init; }

    // Footer
    public string? ThankYouMessage { get; init; }
    public string? PromoText { get; init; }
}
```

### What is NOT Sent to the Display

Security — the display never receives:
- Employee names or commission data
- Cost prices or profit margins
- Other customers' data
- Payroll or business financial information
- Internal transaction notes

The DTO is purpose-built for customer-facing display only.

---

## Display Settings

Configurable per branch from admin dashboard: `/settings/display`

```
┌── Customer Display Settings ──────────────────────────┐
│                                                        │
│  Branch: [Makati Branch ▾]                             │
│                                                        │
│  ══ IDLE SCREEN ═════════════════════════════════════ │
│                                                        │
│  Show Logo                      [✓]                    │
│  Show Business Name             [✓]                    │
│  Show Tagline                   [✓]                    │
│  Show Date & Time               [✓]                    │
│  Show GCash QR Code             [☐]                    │
│  Show Social Media              [☐]                    │
│                                                        │
│  Promo Messages (rotate every 8 sec)                   │
│  ┌──────────────────────────────────────────────────┐ │
│  │ 1. Next wash 10% off for Gold members!           │ │
│  │ 2. Refer a friend, earn 100 pts! 🎁              │ │
│  │ 3. Try our new Premium Detailing service!        │ │
│  │                                  [+ Add Message] │ │
│  └──────────────────────────────────────────────────┘ │
│                                                        │
│  ══ TRANSACTION SCREEN ══════════════════════════════ │
│                                                        │
│  Show Vehicle Info              [✓]                    │
│  Show Customer Name             [✓]                    │
│  Show Loyalty Tier Badge        [✓]                    │
│  Show Discount Breakdown        [✓]                    │
│  Show Tax Line                  [☐]                    │
│                                                        │
│  ══ COMPLETION SCREEN ═══════════════════════════════ │
│                                                        │
│  Show Payment Method            [✓]                    │
│  Show Change Amount             [✓]                    │
│  Show Loyalty Points Earned     [✓]                    │
│  Show Points Balance            [✓]                    │
│  Show Thank You Message         [✓]                    │
│  Show Promo Text                [✓]                    │
│  Completion Hold Duration       [10 seconds ▾]         │
│                                                        │
│  ══ APPEARANCE ══════════════════════════════════════ │
│                                                        │
│  Theme                          [Dark ▾]               │
│     (Dark / Light / Brand — uses company brand colors) │
│  Font Size                      [Large ▾]              │
│     (Normal / Large / Extra Large)                     │
│  Orientation                    [Landscape ▾]          │
│     (Landscape / Portrait)                             │
│                                                        │
│                                    [Save Settings]     │
└────────────────────────────────────────────────────────┘
```

### Domain Model — Display Settings

```csharp
public sealed class DisplaySetting
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? BranchId { get; set; }              // null = tenant default

    // Idle screen
    public bool ShowLogo { get; set; } = true;
    public bool ShowBusinessName { get; set; } = true;
    public bool ShowTagline { get; set; } = true;
    public bool ShowDateTime { get; set; } = true;
    public bool ShowGCashQr { get; set; } = false;
    public bool ShowSocialMedia { get; set; } = false;
    public List<string> PromoMessages { get; set; } = [];
    public int PromoRotationSeconds { get; set; } = 8;

    // Transaction screen
    public bool ShowVehicleInfo { get; set; } = true;
    public bool ShowCustomerName { get; set; } = true;
    public bool ShowLoyaltyTier { get; set; } = true;
    public bool ShowDiscountBreakdown { get; set; } = true;
    public bool ShowTaxLine { get; set; } = false;

    // Completion screen
    public bool ShowPaymentMethod { get; set; } = true;
    public bool ShowChangeAmount { get; set; } = true;
    public bool ShowPointsEarned { get; set; } = true;
    public bool ShowPointsBalance { get; set; } = true;
    public bool ShowThankYouMessage { get; set; } = true;
    public bool ShowPromoText { get; set; } = true;
    public int CompletionHoldSeconds { get; set; } = 10;

    // Appearance
    public DisplayTheme Theme { get; set; } = DisplayTheme.Dark;
    public DisplayFontSize FontSize { get; set; } = DisplayFontSize.Large;
    public DisplayOrientation Orientation { get; set; } = DisplayOrientation.Landscape;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum DisplayTheme { Dark, Light, Brand }
public enum DisplayFontSize { Normal, Large, ExtraLarge }
public enum DisplayOrientation { Landscape, Portrait }
```

---

## POS Station Concept

Each POS station is a logical unit: one cashier + one POS device + one optional customer display. Branches with multiple cashiers need multiple stations.

```csharp
// New entity
public sealed class PosStation
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;       // "Station 1", "Counter A"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Admin setup:**

```
/settings/pos-stations

┌── POS Stations — Makati Branch ───────────────────────┐
│                                                        │
│  Station 1 (Counter A)              [Edit] [Delete]   │
│  Station 2 (Counter B)              [Edit] [Delete]   │
│                                                        │
│                              [+ Add Station]           │
└────────────────────────────────────────────────────────┘
```

Most car washes have 1 station. Larger operations might have 2-3. The cashier selects their station when logging into the POS. The display page selects which station to mirror.

---

## Display Page — Setup Flow

When someone opens `pos.splashsphere.com/display`:

```
┌── Customer Display Setup ─────────────────────────────┐
│                                                        │
│       [SS Logo]                                        │
│    SplashSphere Customer Display                       │
│                                                        │
│  Branch *                                              │
│  [Makati Branch ▾]                                     │
│                                                        │
│  POS Station *                                         │
│  [Station 1 (Counter A) ▾]                             │
│                                                        │
│             [Start Display]                            │
│                                                        │
│  This will open a fullscreen display.                  │
│  Tip: Press F11 for fullscreen mode.                   │
│                                                        │
└────────────────────────────────────────────────────────┘
```

After clicking "Start Display":
- Page enters fullscreen mode (using Fullscreen API)
- Hides all browser UI (address bar, tabs)
- Connects to SignalR hub
- Joins the station-specific display group
- Shows Idle state with company branding
- Listens for transaction events

**Authentication:** The display page requires Clerk authentication (same as POS). The person setting up the display logs in once, selects the station, and leaves the device running. The session stays active.

**Reconnection:** If SignalR disconnects (Wi-Fi drops), the display auto-reconnects with exponential backoff and shows a subtle "Reconnecting..." indicator. When reconnected, it requests the current transaction state (if any) to sync back up.

---

## POS Integration — When to Broadcast

The POS broadcasts display events at these moments:

| POS Action | Display Event | Display State |
|---|---|---|
| Start new transaction | `TransactionStarted` | → Building |
| Add service to transaction | `TransactionUpdated` | Building (refresh) |
| Remove service from transaction | `TransactionUpdated` | Building (refresh) |
| Add merchandise | `TransactionUpdated` | Building (refresh) |
| Add package | `TransactionUpdated` | Building (refresh) |
| Change quantity | `TransactionUpdated` | Building (refresh) |
| Apply discount | `TransactionUpdated` | Building (refresh) |
| Link customer | `TransactionUpdated` | Building (refresh) |
| Select vehicle | `TransactionUpdated` | Building (refresh) |
| Complete payment | `TransactionCompleted` | → Complete |
| Void transaction | `TransactionCancelled` | → Idle |
| Cancel transaction | `TransactionCancelled` | → Idle |

The POS dispatches these events as part of its existing transaction flow — each `TransactionUpdated` call builds the DTO from the current in-memory transaction state and broadcasts it.

---

## API Endpoints

### Display Settings (Admin)
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/settings/display` | Get display settings (default) |
| `GET` | `/api/v1/settings/display?branchId={id}` | Get display settings for branch |
| `PUT` | `/api/v1/settings/display` | Update display settings |
| `PUT` | `/api/v1/settings/display?branchId={id}` | Update for specific branch |

### POS Stations (Admin)
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/branches/{branchId}/stations` | List stations for a branch |
| `POST` | `/api/v1/branches/{branchId}/stations` | Create station |
| `PUT` | `/api/v1/branches/{branchId}/stations/{id}` | Update station name |
| `DELETE` | `/api/v1/branches/{branchId}/stations/{id}` | Delete station |

### Display Page (POS App)
| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/display/settings?branchId={id}` | Get resolved display settings + company profile for rendering |
| `GET` | `/api/v1/display/current?stationId={id}` | Get current transaction state (for reconnection sync) |

### SignalR Hub Methods
| Method | Direction | Description |
|---|---|---|
| `JoinDisplayGroup` | Client → Server | Display joins `display:{branchId}:{stationId}` group |
| `TransactionStarted` | Server → Client | New transaction started |
| `TransactionUpdated` | Server → Client | Line items changed |
| `TransactionCompleted` | Server → Client | Payment processed |
| `TransactionCancelled` | Server → Client | Transaction voided/cancelled |

---

## Offline Behavior

If the display loses connection to SignalR:

1. Show subtle "Reconnecting..." bar at the bottom (don't alarm the customer)
2. Auto-reconnect with exponential backoff (1s, 2s, 4s, 8s, max 30s)
3. On reconnect: call `GET /display/current?stationId={id}` to sync current state
4. If no transaction is active: return to Idle
5. If transaction is in progress: rebuild the Building state from current data

The display never shows stale data — it either shows the current state or shows Idle.

---

## Plan Gating

| Feature | Starter | Growth | Enterprise |
|---|---|---|---|
| Customer Display | ✓ (1 station) | ✓ (3 stations per branch) | ✓ (unlimited) |
| Display Settings | Basic (theme only) | Full | Full + branch overrides |
| Promo Messages | 1 message | 5 messages | Unlimited |

Customer display is available on all plans because it costs nothing to support and adds perceived value to Starter tenants.

---

## Claude Code Prompts

### Prompt — Customer Display Backend

```
Build the Customer Display backend:

Entities:
- PosStation (tenantId, branchId, name, isActive)
- DisplaySetting (tenantId, branchId nullable, all toggle fields,
  promoMessages as JSON array, theme/fontSize/orientation enums)

Application (CQRS):
- CRUD for PosStation
- CRUD for DisplaySetting (with branch-level override + fallback)
- GetDisplayConfigQuery: resolves display settings + company profile
  (logo, name, tagline, thankYouMessage, promoText, gcashQr) into
  a single DisplayConfigDto for the display page
- GetCurrentTransactionQuery: returns current transaction state for
  a specific station (for reconnection sync)

SignalR:
- Extend existing CarWashHub (or create DisplayHub):
  - JoinDisplayGroup(branchId, stationId): joins display:{branchId}:{stationId}
  - Server-to-client: TransactionStarted, TransactionUpdated,
    TransactionCompleted, TransactionCancelled
- DTOs: DisplayTransactionDto (items, totals, vehicle, customer, discount)
  and DisplayCompletionDto (payment, change, loyalty points, footer messages)
- NEVER include employee names, commissions, cost prices, or internal data

POS integration:
- In the existing transaction flow, after each state change (add item, remove item,
  apply discount, link customer, select vehicle, complete payment, void),
  broadcast the appropriate display event to the station's display group.
- Build DisplayTransactionDto from current transaction in-memory state.

Admin API:
- CRUD endpoints for /api/v1/branches/{branchId}/stations
- CRUD endpoints for /api/v1/settings/display (with branchId query param)
- GET /api/v1/display/settings?branchId={id} (display page config)
- GET /api/v1/display/current?stationId={id} (reconnection sync)
```

### Prompt — Customer Display Frontend

```
Build the Customer Display page in the POS app:

Route: /display (setup page) and /display/live (fullscreen display)

Setup page (/display):
- Clerk auth required (same as POS login)
- Branch selector dropdown
- POS Station selector dropdown
- "Start Display" button → navigates to /display/live in fullscreen mode

Display page (/display/live):
- Fullscreen (Fullscreen API on mount, F11 hint on setup page)
- Three states: Idle, Building, Complete
- SignalR connection to display:{branchId}:{stationId} group
- Auto-reconnect with exponential backoff on disconnect
- On reconnect: fetch current state from /display/current?stationId={id}

Idle state:
- Company logo (centered, from display config)
- Business name + tagline
- Rotating promo messages with fade transition (every N seconds)
- Current date and time (updates every minute)
- Optional: GCash QR code, social media handles

Building state:
- Header: logo (small) + business name
- Vehicle info bar (plate, make/model, type/size) if available
- Customer info (name + loyalty tier badge) if linked
- Line items table: name, quantity, unit price, total price
- Items slide-in when added, fade-out when removed
- Running totals: subtotal, discount (with label), tax (if enabled), total
- Total highlighted prominently (large font)

Complete state:
- Green checkmark + "PAYMENT COMPLETE" header
- All line items + totals (same as building)
- Payment method + amount paid + change
- Loyalty: points earned + new balance (if customer linked)
- Thank you message + promo text
- Auto-transition to Idle after configurable seconds (default 10)

Three themes:
- Dark: navy background (#0F172A), white text, splash blue accents
- Light: white background, dark text, splash blue accents
- Brand: uses tenant's primary brand color as accent

Font sizes: Normal (14-16px body), Large (18-20px), Extra Large (22-24px)
Orientation: Landscape (default) and Portrait layouts

Design: Clean, high-contrast, readable from 2 meters away.
Large amounts and totals. No clutter. Subtle animations (slide-in items,
fade transitions, pulse on total change). SplashSphere watermark in corner.

Admin settings page:
- /settings/display: all toggles (idle, transaction, completion, appearance)
- Promo messages editor (add/remove/reorder)
- Branch selector for branch-level overrides
- Live preview panel showing how the display looks with current settings
```
