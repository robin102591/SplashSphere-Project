# SplashSphere — Company Profile & Receipt Designer

> **Phase:** Core feature (should be part of Phase 1-2, before transactions go live)
> **Route:** `/settings/company` (profile), `/settings/receipt` (receipt designer)
> **Scope:** Per-tenant settings. Each tenant configures their own profile and receipt layout.
> **Branch-level override:** Receipt settings can optionally differ per branch (e.g., different address/contact on receipt for each branch).

---

## Why This Matters

Every receipt, every digital receipt email, every printed ticket, and every report that leaves SplashSphere carries the tenant's branding. If the business name is wrong, the address is outdated, or there's no logo — it looks unprofessional and erodes trust with customers.

Additionally, car wash owners want control over what appears on their receipts. Some want a promo message at the bottom. Some want their GCash QR code printed. Some want to show the customer's name, others don't. This needs to be configurable without touching code.

---

## Part 1: Company Profile Settings

### What the Tenant Configures

These fields are filled during onboarding (from `TENANT_ONBOARDING.md`) but editable anytime from `/settings/company`.

```
┌── Company Profile ─────────────────────────────────────────────┐
│                                                                 │
│  ── Business Identity ────────────────────────────────────────│
│                                                                 │
│  Logo                                                           │
│  ┌──────────────┐                                              │
│  │              │  [Upload Logo]  [Remove]                     │
│  │   (logo)     │  Max 2MB. PNG, JPG, or SVG.                 │
│  │              │  Recommended: 500×500px square.              │
│  └──────────────┘  Used on receipts, reports, and Connect app.│
│                                                                 │
│  Business Name *                                                │
│  [AquaShine Car Wash                                       ]   │
│                                                                 │
│  Tagline / Description                                         │
│  [Premium car care since 2020                              ]   │
│                                                                 │
│  ── Contact Information ──────────────────────────────────── │
│                                                                 │
│  Email *                                                        │
│  [info@aquashine.ph                                        ]   │
│                                                                 │
│  Phone *                                                        │
│  [0917-123-4567                                            ]   │
│                                                                 │
│  Website                                                        │
│  [https://aquashine.ph                                     ]   │
│                                                                 │
│  ── Business Address ────────────────────────────────────── │
│                                                                 │
│  Street Address *                                               │
│  [123 Makati Avenue                                        ]   │
│                                                                 │
│  Barangay                                                       │
│  [Brgy. San Lorenzo                                        ]   │
│                                                                 │
│  City / Municipality *                                          │
│  [Makati City                                              ]   │
│                                                                 │
│  Province                                                       │
│  [Metro Manila                                             ]   │
│                                                                 │
│  Zip Code                                                       │
│  [1223                                                     ]   │
│                                                                 │
│  ── Tax & Registration ──────────────────────────────────── │
│                                                                 │
│  TIN (Tax Identification Number)                                │
│  [123-456-789-000                                          ]   │
│                                                                 │
│  Business Registration (DTI / SEC / CDA)                       │
│  [DTI-NCR-2020-12345                                       ]   │
│                                                                 │
│  VAT Registered?                                                │
│  ○ Yes (VAT-registered)   ● No (Non-VAT)                      │
│                                                                 │
│  ── Social & Payment ────────────────────────────────────── │
│                                                                 │
│  Facebook Page URL                                              │
│  [https://facebook.com/aquashinecarwash                    ]   │
│                                                                 │
│  Instagram Handle                                               │
│  [@aquashinecarwash                                        ]   │
│                                                                 │
│  GCash Number (for receipt display)                             │
│  [0917-123-4567                                            ]   │
│                                                                 │
│  GCash QR Code Image                                            │
│  [Upload QR]  Used on printed receipts for easy payment.       │
│                                                                 │
│                                          [Save Changes]        │
└─────────────────────────────────────────────────────────────────┘
```

### Domain Model Update

Extend the existing `Tenant` entity with additional fields:

```csharp
// Add to existing Tenant entity
public string? Tagline { get; set; }
public string? LogoUrl { get; set; }                   // Stored in cloud storage (S3/R2)
public string? LogoThumbnailUrl { get; set; }          // Resized version for receipts

// Address (structured — replaces the single "address" string)
public string? StreetAddress { get; set; }
public string? Barangay { get; set; }
public string? City { get; set; }
public string? Province { get; set; }
public string? ZipCode { get; set; }

// Tax & Registration
public string? TIN { get; set; }
public string? BusinessRegistration { get; set; }      // DTI/SEC/CDA number
public bool IsVatRegistered { get; set; }

// Social & Payment display
public string? FacebookUrl { get; set; }
public string? InstagramHandle { get; set; }
public string? GCashNumber { get; set; }
public string? GCashQrUrl { get; set; }                // QR code image
```

### Logo Upload

**Storage:** Use Cloudflare R2 (S3-compatible, free egress) or Supabase Storage.

**Process:**
1. Tenant uploads image from admin dashboard
2. Frontend validates: max 2MB, PNG/JPG/SVG only
3. API receives the file, processes it:
   - Resize to 500×500px (main logo)
   - Create 200×200px thumbnail (for receipts)
   - Create 80×80px icon (for Connect app listing)
4. Upload all three versions to cloud storage
5. Store URLs in Tenant entity
6. Old logo files are deleted when replaced

```csharp
// Application/Features/Tenants/Commands/UploadLogoCommand.cs
public sealed record UploadLogoCommand(
    string TenantId,
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<LogoUploadResult>;

public sealed record LogoUploadResult(
    string LogoUrl,
    string ThumbnailUrl,
    string IconUrl
);
```

**Image processing:** Use `ImageSharp` (cross-platform .NET image library):

```xml
<PackageReference Include="SixLabors.ImageSharp" Version="3.*" />
```

---

## Part 2: Receipt Designer

### Receipt Structure

A receipt has 5 configurable zones:

```
┌─────────────────────────────────┐
│          HEADER ZONE            │  ← Logo, business name, branch address
│         (configurable)          │
├─────────────────────────────────┤
│       TRANSACTION BODY          │  ← Fixed: services, items, prices, subtotals
│        (not configurable)       │     This is standardized — tenants can't
│                                 │     rearrange line items or hide prices
├─────────────────────────────────┤
│       PAYMENT DETAILS           │  ← Fixed: payment method, amount, change
│        (not configurable)       │
├─────────────────────────────────┤
│        CUSTOMER ZONE            │  ← Configurable: show/hide customer info,
│      (toggle on/off)            │     vehicle info, loyalty points
├─────────────────────────────────┤
│         FOOTER ZONE             │  ← Configurable: thank you message,
│        (configurable)           │     promo text, social media, QR code
└─────────────────────────────────┘
```

### Receipt Settings UI

```
┌── Receipt Settings ────────────────────── [Preview] ────────┐
│                                                               │
│  Branch: [All Branches ▾] (or configure per branch)          │
│                                                               │
│  ══ HEADER ══════════════════════════════════════════════════│
│                                                               │
│  Show Logo                    [✓]                            │
│  Logo Size                    [Medium ▾]  (Small/Medium/Large)│
│  Logo Position                [Center ▾]  (Left/Center)      │
│                                                               │
│  Show Business Name           [✓]                            │
│  Show Tagline                 [✓]                            │
│  Show Branch Name             [✓]                            │
│  Show Branch Address          [✓]                            │
│  Show Branch Contact          [✓]                            │
│  Show TIN                     [✓]                            │
│                                                               │
│  Custom Header Text (below address)                          │
│  [VAT-Exempt | Serve with Excellence                     ]   │
│                                                               │
│  ══ BODY ════════════════════════════════════════════════════│
│                                                               │
│  Show Service Duration        [✓]                            │
│  Show Employee Names          [✓]  (who performed the service)│
│  Show Vehicle Info            [✓]  (plate, make/model, type)  │
│  Show Discount Breakdown      [✓]                            │
│  Show Tax Line                [✓]                            │
│  Show Transaction Number      [✓]                            │
│  Show Date & Time             [✓]                            │
│  Show Cashier Name            [✓]                            │
│                                                               │
│  ══ CUSTOMER INFO ═══════════════════════════════════════════│
│                                                               │
│  Show Customer Name           [✓]                            │
│  Show Customer Phone          [☐]  (privacy-sensitive)       │
│  Show Loyalty Points Earned   [✓]                            │
│  Show Loyalty Balance         [✓]                            │
│  Show Loyalty Tier            [✓]                            │
│                                                               │
│  ══ FOOTER ══════════════════════════════════════════════════│
│                                                               │
│  Thank You Message *                                          │
│  [Thank you for choosing AquaShine! 🚗✨                  ]  │
│                                                               │
│  Promo / Marketing Text                                       │
│  [Next wash 10% off! Show this receipt. Valid until Apr 30.]  │
│                                                               │
│  Show Social Media Links      [✓]                            │
│  Show GCash QR Code           [✓]  (from company profile)    │
│  Show GCash Number            [✓]                            │
│                                                               │
│  Custom Footer Text (legal, notes)                            │
│  [This serves as your Official Receipt. Items sold are    ]  │
│  [non-refundable. For concerns: 0917-123-4567             ]  │
│                                                               │
│  ══ FORMAT ══════════════════════════════════════════════════│
│                                                               │
│  Receipt Width                [58mm ▾]  (58mm / 80mm thermal) │
│  Font Size                    [Normal ▾]  (Small/Normal/Large)│
│  Paper Cut After Print        [✓]  (auto-cut if printer supports)│
│                                                               │
│                                          [Save Settings]     │
│                                                               │
└───────────────────────────────────────────────────────────────┘
```

### Live Receipt Preview

The right side of the settings page shows a live preview that updates as the tenant toggles settings:

```
┌─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┐
╎                                 ╎
╎       [LOGO]                    ╎
╎    AquaShine Car Wash           ╎
╎  Premium car care since 2020    ╎
╎       Makati Branch             ╎
╎  123 Makati Ave, Brgy. San     ╎
╎  Lorenzo, Makati City 1223     ╎
╎  Tel: 0917-123-4567            ╎
╎  TIN: 123-456-789-000          ╎
╎  VAT-Exempt | Serve with       ╎
╎  Excellence                     ╎
╎─────────────────────────────────╎
╎  TXN: TXN-MAK-0426-0012       ╎
╎  Date: Apr 26, 2026 2:41 PM   ╎
╎  Cashier: Ana                   ╎
╎─────────────────────────────────╎
╎  Vehicle: Toyota Vios          ╎
╎  Plate: ABC-1234               ╎
╎  Type: Sedan / Medium          ╎
╎─────────────────────────────────╎
╎  Basic Wash         1  ₱200.00 ╎
╎    (30 min, Juan & Pedro)      ╎
╎  Tire & Rim Shine   1  ₱120.00 ╎
╎    (10 min, Juan)              ╎
╎─────────────────────────────────╎
╎  Subtotal              ₱320.00 ╎
╎  Discount (Gold 10%)   -₱32.00 ╎
╎  VAT (if applicable)     ₱0.00 ╎
╎  ═══════════════════════════════╎
╎  TOTAL                 ₱288.00 ╎
╎  ═══════════════════════════════╎
╎  Cash                  ₱300.00 ╎
╎  Change                 ₱12.00 ╎
╎─────────────────────────────────╎
╎  Customer: Maria Santos        ╎
╎  Gold Member ⭐                ╎
╎  Points Earned: +29 pts        ╎
╎  Balance: 1,269 pts            ╎
╎─────────────────────────────────╎
╎                                 ╎
╎  Thank you for choosing        ╎
╎  AquaShine! 🚗✨               ╎
╎                                 ╎
╎  Next wash 10% off! Show this  ╎
╎  receipt. Valid until Apr 30.   ╎
╎                                 ╎
╎  FB: /aquashinecarwash         ╎
╎  IG: @aquashinecarwash         ╎
╎                                 ╎
╎  ┌───────────────┐             ╎
╎  │   GCash QR    │             ╎
╎  │   (QR Code)   │             ╎
╎  └───────────────┘             ╎
╎  GCash: 0917-123-4567         ╎
╎                                 ╎
╎  This serves as your Official  ╎
╎  Receipt. Items sold are       ╎
╎  non-refundable.               ╎
╎  For concerns: 0917-123-4567   ╎
╎                                 ╎
└─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┘
```

### Domain Model — Receipt Settings

```csharp
public sealed class ReceiptSetting
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? BranchId { get; set; }              // null = all branches default

    // Header
    public bool ShowLogo { get; set; } = true;
    public LogoSize LogoSize { get; set; } = LogoSize.Medium;
    public LogoPosition LogoPosition { get; set; } = LogoPosition.Center;
    public bool ShowBusinessName { get; set; } = true;
    public bool ShowTagline { get; set; } = true;
    public bool ShowBranchName { get; set; } = true;
    public bool ShowBranchAddress { get; set; } = true;
    public bool ShowBranchContact { get; set; } = true;
    public bool ShowTIN { get; set; } = false;
    public string? CustomHeaderText { get; set; }

    // Body
    public bool ShowServiceDuration { get; set; } = true;
    public bool ShowEmployeeNames { get; set; } = true;
    public bool ShowVehicleInfo { get; set; } = true;
    public bool ShowDiscountBreakdown { get; set; } = true;
    public bool ShowTaxLine { get; set; } = true;
    public bool ShowTransactionNumber { get; set; } = true;
    public bool ShowDateTime { get; set; } = true;
    public bool ShowCashierName { get; set; } = true;

    // Customer
    public bool ShowCustomerName { get; set; } = true;
    public bool ShowCustomerPhone { get; set; } = false;
    public bool ShowLoyaltyPointsEarned { get; set; } = true;
    public bool ShowLoyaltyBalance { get; set; } = true;
    public bool ShowLoyaltyTier { get; set; } = true;

    // Footer
    public string ThankYouMessage { get; set; } = "Thank you for your patronage!";
    public string? PromoText { get; set; }
    public bool ShowSocialMedia { get; set; } = true;
    public bool ShowGCashQr { get; set; } = false;
    public bool ShowGCashNumber { get; set; } = false;
    public string? CustomFooterText { get; set; }

    // Format
    public ReceiptWidth ReceiptWidth { get; set; } = ReceiptWidth.Mm58;
    public ReceiptFontSize FontSize { get; set; } = ReceiptFontSize.Normal;
    public bool AutoCutPaper { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum LogoSize { Small, Medium, Large }
public enum LogoPosition { Left, Center }
public enum ReceiptWidth { Mm58, Mm80 }
public enum ReceiptFontSize { Small, Normal, Large }
```

### Resolution: Branch-Level vs Tenant-Level

Receipt settings follow this priority:
1. **Branch-specific setting** (if exists for this branch) → use it
2. **Tenant default setting** (BranchId = null) → fallback

```csharp
// Query to resolve receipt settings for a branch
public async Task<ReceiptSetting> GetReceiptSettingAsync(string tenantId, string branchId)
{
    // Try branch-specific first
    var branchSetting = await _context.ReceiptSettings
        .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.BranchId == branchId);

    if (branchSetting != null) return branchSetting;

    // Fall back to tenant default
    return await _context.ReceiptSettings
        .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.BranchId == null)
        ?? CreateDefaultReceiptSetting(tenantId);
}
```

This way, a franchise with 3 branches can have different addresses and promo text on each branch's receipts, while sharing the same logo and thank-you message.

---

## Part 3: Receipt Rendering

### Three Receipt Outputs

The same receipt settings power three outputs:

| Output | Where | Format |
|---|---|---|
| **Printed receipt** | POS terminal → thermal printer | ESC/POS commands (raw) |
| **Digital receipt (email)** | Sent via Resend after payment | HTML email template |
| **Digital receipt (in-app)** | Customer Connect → History | HTML rendered in app |

### Receipt Data DTO

When generating any receipt, the API builds a `ReceiptData` object that contains everything:

```csharp
public sealed record ReceiptData
{
    // Company (from Tenant + Branch)
    public string? LogoUrl { get; init; }
    public string BusinessName { get; init; } = string.Empty;
    public string? Tagline { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public string? BranchAddress { get; init; }
    public string? BranchContact { get; init; }
    public string? TIN { get; init; }
    public string? CustomHeaderText { get; init; }

    // Transaction
    public string TransactionNo { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public string CashierName { get; init; } = string.Empty;

    // Vehicle
    public string? VehiclePlate { get; init; }
    public string? VehicleMakeModel { get; init; }
    public string? VehicleTypeSize { get; init; }

    // Line items
    public List<ReceiptLineItem> Services { get; init; } = [];
    public List<ReceiptLineItem> Merchandises { get; init; } = [];
    public List<ReceiptLineItem> Packages { get; init; } = [];

    // Totals
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public string? DiscountLabel { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal FinalAmount { get; init; }

    // Payment
    public string PaymentMethod { get; init; } = string.Empty;
    public decimal AmountPaid { get; init; }
    public decimal ChangeAmount { get; init; }

    // Customer & Loyalty
    public string? CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public string? LoyaltyTier { get; init; }
    public int? PointsEarned { get; init; }
    public int? PointsBalance { get; init; }

    // Footer
    public string? ThankYouMessage { get; init; }
    public string? PromoText { get; init; }
    public string? FacebookUrl { get; init; }
    public string? InstagramHandle { get; init; }
    public string? GCashQrUrl { get; init; }
    public string? GCashNumber { get; init; }
    public string? CustomFooterText { get; init; }

    // Settings (controls what's shown)
    public ReceiptSetting Settings { get; init; } = null!;
}

public sealed record ReceiptLineItem(
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    int? DurationMinutes,
    List<string>? EmployeeNames
);
```

### Building the Receipt

```csharp
// Application/Services/IReceiptBuilder.cs
public interface IReceiptBuilder
{
    Task<ReceiptData> BuildAsync(string transactionId);
    string RenderHtml(ReceiptData data);         // For email and in-app
    byte[] RenderEscPos(ReceiptData data);        // For thermal printer
}
```

The `ReceiptBuilder` pulls data from the transaction, resolves the receipt settings (branch-specific or tenant default), and merges company profile info. The `Settings` object controls which fields are included — the renderer checks each `Show*` flag before adding a section.

### Digital Receipt Email (via Resend)

```csharp
// Triggered after transaction is completed
public class SendDigitalReceiptHandler
{
    public async Task Handle(TransactionCompletedEvent @event)
    {
        var receipt = await _receiptBuilder.BuildAsync(@event.TransactionId);
        var html = _receiptBuilder.RenderHtml(receipt);

        if (receipt.CustomerPhone != null || receipt.Settings.ShowCustomerPhone)
        {
            // Could also send via SMS as a link to view receipt online
        }

        // Send via email if customer has email
        await _emailService.SendAsync(
            to: receipt.CustomerEmail,
            subject: $"Receipt from {receipt.BusinessName} — {receipt.TransactionNo}",
            html: html
        );
    }
}
```

---

## Part 4: Where Company Profile Appears

The tenant's company profile data is used in multiple places:

| Location | Data Used |
|---|---|
| **Printed receipt** | Logo, name, branch address, contact, TIN, GCash QR |
| **Email receipt** | Logo, name, branch address, contact, social links |
| **Customer Connect app** | Logo, name, tagline (in car wash listing and detail) |
| **End-of-Day Report** | Logo, name, branch name, TIN |
| **Payroll Summary** | Business name, TIN |
| **Booking confirmation SMS** | Business name, branch address |
| **Invoice (if applicable)** | Full business details + TIN + registration |

This is why the company profile must be complete and accurate — it cascades everywhere.

---

## API Endpoints

### Company Profile

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/settings/company` | Get tenant company profile |
| `PUT` | `/api/v1/settings/company` | Update company profile fields |
| `POST` | `/api/v1/settings/company/logo` | Upload logo (multipart form) |
| `DELETE` | `/api/v1/settings/company/logo` | Remove logo |
| `POST` | `/api/v1/settings/company/gcash-qr` | Upload GCash QR image |
| `DELETE` | `/api/v1/settings/company/gcash-qr` | Remove GCash QR |

### Receipt Settings

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/settings/receipt` | Get receipt settings (default) |
| `GET` | `/api/v1/settings/receipt?branchId={id}` | Get receipt settings for specific branch |
| `PUT` | `/api/v1/settings/receipt` | Update receipt settings (default) |
| `PUT` | `/api/v1/settings/receipt?branchId={id}` | Update receipt settings for specific branch |
| `DELETE` | `/api/v1/settings/receipt?branchId={id}` | Remove branch override (fall back to default) |
| `GET` | `/api/v1/settings/receipt/preview` | Get rendered HTML receipt preview with sample data |

### Receipt Generation

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/v1/transactions/{id}/receipt` | Get rendered receipt HTML for a transaction |
| `POST` | `/api/v1/transactions/{id}/receipt/send` | Send digital receipt to customer (email/SMS) |
| `GET` | `/api/v1/transactions/{id}/receipt/print` | Get ESC/POS binary for thermal printing |

---

## Plan Gating

| Feature | Starter | Growth | Enterprise |
|---|---|---|---|
| Company profile editing | ✓ | ✓ | ✓ |
| Logo upload | ✓ | ✓ | ✓ |
| Receipt customization | Basic (thank you + footer text only) | Full (all toggles) | Full + branch overrides |
| Digital receipts (email) | ✗ | ✓ | ✓ |
| GCash QR on receipt | ✓ | ✓ | ✓ |
| Custom header/footer text | ✓ | ✓ | ✓ |

Starter gets basic receipt customization (thank you message, footer text, logo) — enough to look professional. Growth and Enterprise unlock all toggles. Branch-level overrides are Enterprise only (multi-branch operation).

---

## Default Values (Created During Onboarding)

When a tenant completes onboarding, a default `ReceiptSetting` is created:

```csharp
var defaultSetting = new ReceiptSetting
{
    TenantId = tenant.Id,
    BranchId = null,                    // Applies to all branches

    // Header — show everything by default
    ShowLogo = true,
    ShowBusinessName = true,
    ShowTagline = true,
    ShowBranchName = true,
    ShowBranchAddress = true,
    ShowBranchContact = true,
    ShowTIN = false,                     // Off by default (not all are VAT-registered)

    // Body — show everything
    ShowServiceDuration = true,
    ShowEmployeeNames = true,
    ShowVehicleInfo = true,
    ShowDiscountBreakdown = true,
    ShowTaxLine = false,                 // Off by default (most car washes are non-VAT)
    ShowTransactionNumber = true,
    ShowDateTime = true,
    ShowCashierName = true,

    // Customer — show name and loyalty, hide phone
    ShowCustomerName = true,
    ShowCustomerPhone = false,
    ShowLoyaltyPointsEarned = true,
    ShowLoyaltyBalance = true,
    ShowLoyaltyTier = true,

    // Footer
    ThankYouMessage = $"Thank you for choosing {tenant.Name}!",
    ShowSocialMedia = true,
    ShowGCashQr = false,                 // Needs QR upload first

    // Format
    ReceiptWidth = ReceiptWidth.Mm58,
    FontSize = ReceiptFontSize.Normal,
    AutoCutPaper = true,
};
```

---

## Claude Code Prompts

### Prompt — Company Profile

```
Add Company Profile settings to the admin dashboard:

Domain: Extend Tenant entity with structured address (street, barangay, city,
province, zip), TIN, business registration, VAT flag, tagline, social media URLs,
GCash number, and image URLs (logo, logoThumbnail, gcashQr).

Application:
- UpdateCompanyProfileCommand (all text fields)
- UploadLogoCommand (multipart file → resize to 500px, 200px, 80px → upload to
  Cloudflare R2 → store URLs in Tenant)
- DeleteLogoCommand
- UploadGCashQrCommand, DeleteGCashQrCommand
- GetCompanyProfileQuery

Infrastructure:
- IFileStorageService with Cloudflare R2 implementation (S3-compatible SDK)
- ImageSharp for resizing (SixLabors.ImageSharp)

Admin UI: /settings/company page with all fields, logo upload with drag-and-drop,
GCash QR upload, preview of how logo appears. Migration: "ExtendTenantProfile"
```

### Prompt — Receipt Designer

```
Add Receipt Settings and Designer to the admin dashboard:

Domain: ReceiptSetting entity with all toggle fields, LogoSize/LogoPosition/
ReceiptWidth/ReceiptFontSize enums.

Application:
- UpdateReceiptSettingCommand (with optional branchId for branch override)
- DeleteBranchReceiptOverrideCommand
- GetReceiptSettingQuery (resolves branch → tenant fallback)
- GetReceiptPreviewQuery (builds sample receipt with current settings)
- BuildReceiptQuery (builds real receipt from transactionId)
- SendDigitalReceiptCommand (Resend email to customer)

Infrastructure:
- ReceiptBuilder service: assembles ReceiptData from Transaction + Tenant + Branch
  + ReceiptSetting. Renderers: RenderHtml (Razor template) and RenderEscPos
  (ESC/POS binary for thermal printers)
- EF config with branch-level receipt setting support

Admin UI: /settings/receipt with all toggle sections (header, body, customer, footer,
format). Live preview panel on the right that updates as toggles change.
Branch selector dropdown at top. "Reset to Default" button for branch overrides.

POS integration: after transaction completion, "Print Receipt" button calls
the receipt/print endpoint. "Send Receipt" button sends via email.
```
