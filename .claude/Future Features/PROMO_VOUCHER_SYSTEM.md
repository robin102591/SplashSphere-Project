# SplashSphere — Promo Code & Voucher System

> **Phase:** 23.2 (Value-Add). Requires core transaction system.
> **Plan gating:** Growth+ feature. Starter tenants can apply codes manually (flat discount), Growth+ gets the full engine.

---

## What It Does

The owner creates promotional codes that customers present at the POS for discounts. Codes can be percentage-based, fixed amount, or free service. Supports: marketing campaigns, seasonal promos, birthday vouchers, loyalty rewards, referral incentives, and partnership deals.

---

## Promo Code Types

| Type | Example | Behavior |
|---|---|---|
| **Percentage Off** | `SUMMER20` → 20% off total | Applies percentage to finalAmount |
| **Fixed Amount Off** | `SAVE50` → ₱50 off | Subtracts fixed amount from finalAmount |
| **Free Service** | `FREEWAX` → free Wax & Polish | Sets specific service price to ₱0 |
| **Buy X Get Y** | `BOGO` → Buy Premium, get Tire Shine free | Adds free service when condition met |
| **First Wash** | `WELCOME` → 30% off first transaction | One-time use per customer |
| **Birthday** | `BDAY-MARIA-0425` → Free Basic Wash | Auto-generated, valid during birthday month |

---

## Domain Models

```csharp
public sealed class PromoCode
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;           // "SUMMER20" — uppercase, alphanumeric
    public string Name { get; set; } = string.Empty;           // "Summer 2026 Promo"
    public string? Description { get; set; }
    public PromoType Type { get; set; }
    public decimal? DiscountPercent { get; set; }               // For percentage type
    public decimal? DiscountAmount { get; set; }                // For fixed amount type
    public string? FreeServiceId { get; set; }                  // For free service type
    public decimal? MinimumSpend { get; set; }                  // Minimum transaction amount to qualify
    public decimal? MaxDiscount { get; set; }                   // Cap on percentage discounts
    public int? MaxUsesTotal { get; set; }                      // Total redemptions allowed (null = unlimited)
    public int? MaxUsesPerCustomer { get; set; }                // Per-customer limit (default 1)
    public int UsedCount { get; set; }                          // Current total redemptions
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? TargetCustomerFilter { get; set; }           // JSON: tier, new_customer, specific IDs
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<PromoRedemption> Redemptions { get; set; } = [];
}

public sealed class PromoRedemption
{
    public string Id { get; set; } = string.Empty;
    public string PromoCodeId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public decimal DiscountApplied { get; set; }                // Actual discount amount
    public DateTime RedeemedAt { get; set; }
}

public enum PromoType
{
    PercentageOff,
    FixedAmountOff,
    FreeService,
    BuyXGetY,
    FirstWash
}
```

---

## POS Integration

Cashier enters promo code during transaction:

```
┌── Apply Promo Code ───────────────────────────────────┐
│                                                        │
│  Code: [SUMMER20        ] [Apply]                     │
│                                                        │
│  ✅ "Summer 2026 Promo" — 20% off                     │
│  Discount: -₱64.00                                    │
│  Valid until: April 30, 2026                           │
│                                                        │
│  Subtotal:  ₱320.00                                   │
│  Discount:  -₱64.00                                   │
│  Total:     ₱256.00                                   │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**Validation on apply:**
- Code exists and belongs to this tenant
- Code is active and within date range
- Usage limits not exceeded (total and per-customer)
- Minimum spend met
- Customer eligibility (if targeted)
- Max discount cap applied

---

## Admin Pages

| Route | Page |
|---|---|
| `/promos` | List all promo codes with status, usage count, date range |
| `/promos/new` | Create promo: code, type, discount, limits, dates, targeting |
| `/promos/{id}` | Detail: usage stats, redemption list, edit (if not yet used) |

**Dashboard widget:** "Active Promos: 3 running, ₱12,480 total discounts this month"

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/promos` | List promo codes (filter by active, expired) |
| `POST` | `/promos` | Create promo code |
| `GET` | `/promos/{id}` | Promo detail with redemption stats |
| `PUT` | `/promos/{id}` | Update (only if zero redemptions) |
| `PATCH` | `/promos/{id}/deactivate` | Deactivate early |
| `POST` | `/promos/validate` | Validate a code (used by POS before applying) |
| `POST` | `/promos/apply` | Apply to a transaction (called during transaction creation) |

---

## Claude Code Prompt

```
Build the Promo Code system:

Domain: PromoCode, PromoRedemption, PromoType enum
Application: CreatePromoCommand, ValidatePromoQuery, ApplyPromoCommand,
  GetPromosQuery, DeactivatePromoCommand
  
Validation logic: date range, usage limits, min spend, customer eligibility, max discount cap.
Update CreateTransactionCommandHandler: accept optional promoCode, validate and apply discount.
PromoRedemption created on transaction completion.

Admin pages: /promos (list), /promos/new (create), /promos/{id} (detail + stats)
POS: "Apply Promo Code" input field on transaction screen with validation feedback.

Plan gating: Growth+ (Starter gets manual flat discounts only, no promo engine)
```
