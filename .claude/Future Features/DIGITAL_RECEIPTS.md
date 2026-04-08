# SplashSphere — Digital Receipts (SMS/Email)

> **Phase:** 23.4 (Value-Add). Requires core transaction system.
> **Plan gating:** Available on all plans. SMS receipts count toward SMS quota.

---

## What It Does

After a transaction is completed, the customer receives a digital receipt via SMS or email instead of (or in addition to) a thermal paper receipt. Reduces paper costs, creates a permanent digital record, and gives the car wash another customer touchpoint.

---

## Receipt Delivery Options

The cashier chooses at payment time (or it's automatic based on customer preference):

| Method | When to Use | Cost |
|---|---|---|
| **Print only** | Walk-in, no phone on file | ₱0 (paper cost) |
| **SMS** | Customer has phone, prefers text | Counts toward SMS quota |
| **Email** | Customer has email on file | Free (via Resend) |
| **SMS + Print** | Customer wants both | SMS quota + paper |
| **No receipt** | Customer declines | ₱0 |

---

## SMS Receipt Format

```
[SplashSphere] AquaShine Makati
Mar 25, 2026 11:45 AM
TXN-MAK-0325-0047

Basic Wash (Sedan/M) ₱200
Tire & Rim Shine     ₱120
─────────────────────
Total:               ₱320
Cash:                ₱500
Change:              ₱180

Points earned: +32
Balance: 1,272 pts (Gold)

Thank you, Maria! 🚗✨
```

Keep under 3 SMS segments (480 chars) to minimize cost. If too long, send a short SMS with a link:

```
[SplashSphere] Receipt for ₱320 at AquaShine Makati.
View: https://r.splashsphere.ph/TXN0312ABC
Thank you, Maria! 🚗✨
```

---

## Email Receipt (Resend Template)

HTML email matching SplashSphere brand — similar to billing templates but for transaction receipts. Includes full line-item detail, payment method, points earned, and a "Book Again" CTA.

---

## Domain Changes

Add to `Transaction`:
```csharp
public ReceiptDelivery ReceiptDelivery { get; set; } = ReceiptDelivery.Print;
public bool ReceiptSent { get; set; }
public DateTime? ReceiptSentAt { get; set; }
```

```csharp
public enum ReceiptDelivery { Print, Sms, Email, SmsAndPrint, None }
```

---

## Web Receipt Page

A public receipt page for the SMS link (no login required):

```
https://r.splashsphere.ph/TXN0312ABC

┌── Digital Receipt ────────────────────────────────────┐
│  AquaShine Car Wash — Makati                          │
│  Transaction #TXN-MAK-0325-0047                       │
│  March 25, 2026 11:45 AM                              │
│                                                        │
│  Customer: Maria Santos                               │
│  Vehicle: Toyota Vios (ABC-1234) • Sedan / Medium     │
│                                                        │
│  ── Services ─────────────────────────────────────── │
│  Basic Wash ............................ ₱200.00       │
│  Tire & Rim Shine ...................... ₱120.00       │
│                                                        │
│  Subtotal: ₱320.00                                    │
│  Discount: ₱0.00                                      │
│  Total:    ₱320.00                                    │
│                                                        │
│  Payment: Cash ₱500.00 | Change: ₱180.00             │
│                                                        │
│  Loyalty: +32 points earned                           │
│  Balance: 1,272 points (Gold tier)                    │
│                                                        │
│  Served by: Juan D., Pedro S., Maria G.              │
│                                                        │
│  ─────────────────────────────────────────────────── │
│  Thank you for choosing AquaShine! 🚗✨               │
│  [Book Your Next Wash →]                              │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## POS Integration

On the payment section of the transaction screen, add receipt preference:

```
Receipt: [Print ▾] ← dropdown: Print / SMS / Email / SMS+Print / None
```

If customer has a saved preference (from their Customer record), auto-select it. If sending SMS/email, show a confirmation: "Receipt sent to 0917-123-4567 ✓"

---

## Tenant Settings

```
/settings → Receipts:
  Default receipt method: [Print ▾]
  Auto-send SMS receipt for known customers: [✓]
  Include loyalty points on receipt: [✓]
  Include "Book Again" link on digital receipt: [✓]
  Google Maps review link on receipt: [https://g.page/aquashine-makati]
```

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/transactions/{id}/send-receipt` | Send/resend receipt via SMS or email |
| `GET` | `/receipts/{token}` | Public receipt page (no auth, token-based) |

---

## Claude Code Prompt

```
Build Digital Receipts:

Domain: Add ReceiptDelivery enum and fields to Transaction entity.
Application: SendReceiptCommand (SMS via Semaphore, email via Resend)
  Generate receipt token (short, URL-safe) for web receipt link.
  SMS format: compact, under 480 chars. Include link for full receipt if too long.

Public page: /receipts/{token} — standalone, no auth, mobile-optimized receipt view.
  Include line items, payment, loyalty points, "Book Again" CTA.

POS: Add receipt method dropdown to payment section. Auto-select customer preference.
Email template: HTML receipt matching SplashSphere brand.

Tenant settings: default receipt method, auto-send toggle.
Plan gating: All plans. SMS receipts count toward SMS quota.
```
