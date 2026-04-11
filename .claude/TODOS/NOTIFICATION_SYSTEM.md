# SplashSphere — Unified Notification System

> **Phase:** Cross-cutting — integrates with Phases 12 (SignalR), 15 (SMS), 16 (Billing), 18 (Inventory), and 21 (AI Alerts).
> **Scope:** All notification channels: in-app (real-time via SignalR), SMS (Semaphore), and email (Resend).

---

## Why Unified

Notifications are scattered across multiple specs: SMS in Phase 15, billing emails in Phase 16, inventory alerts in Phase 18, AI alerts in Phase 21, and real-time updates via SignalR in Phase 12. Without a unified system, each feature implements its own notification logic — different patterns, duplicate code, no central preferences, and no way for the owner to control what they receive.

One `INotificationService.Send()` call — the system figures out which channels to use based on notification type, severity, and user preferences.

---

## Three Channels

**In-App (SignalR)** — Real-time. Bell icon badge in admin header. Toast notifications. Always delivered (can't disable). Free, no quota.

**SMS (Semaphore)** — For critical alerts and customer messages. Counts toward plan SMS quota (Starter: 0, Growth: 50/mo, Enterprise: 200/mo). Philippine phone format. 160-char single SMS.

**Email (Resend)** — For billing lifecycle (mandatory, can't disable), payroll summaries, and platform announcements. Uses the HTML templates already built.

---

## Notification Types

### Operations

| Type | Severity | In-App | SMS | Email |
|---|---|---|---|---|
| `transaction.completed` | info | Always | — | — |
| `transaction.voided` | warning | Always | Owner (opt) | — |
| `shift.closed` | info | Always | Owner (opt) | — |
| `shift.flagged` | critical | Always | Owner | — |
| `queue.customer_called` | info | POS only | — | — |
| `employee.clocked_in` | info | Always | — | — |

### Inventory

| Type | Severity | In-App | SMS | Email |
|---|---|---|---|---|
| `inventory.low_stock` | warning | Always | Owner (opt) | — |
| `inventory.out_of_stock` | critical | Always | Owner | — |
| `inventory.po_received` | info | Always | — | — |
| `equipment.maintenance_due` | warning | Always | — | — |

### Financial

| Type | Severity | In-App | SMS | Email |
|---|---|---|---|---|
| `payroll.processed` | info | Always | — | Owner |
| `payroll.ready_for_review` | info | Always | Owner (opt) | — |
| `cash_advance.requested` | info | Always | Owner (opt) | — |
| `cash_advance.approved` | info | Always | Employee | — |

### Billing (Email Always Mandatory)

| Type | Severity | In-App | SMS | Email |
|---|---|---|---|---|
| `billing.invoice_created` | info | Always | — | Mandatory |
| `billing.payment_reminder` | warning | Always | Always | Mandatory |
| `billing.payment_received` | info | Always | — | Mandatory |
| `billing.payment_failed` | critical | Always | Always | Mandatory |
| `billing.overdue` | critical | Always | Always | Mandatory |
| `billing.suspended` | critical | Always | Always | Mandatory |
| `billing.trial_expiring` | warning | Always | Always | Mandatory |

### Customer (SMS to Customer Phone, Not Tenant User)

| Type | Severity | In-App | SMS | Email |
|---|---|---|---|---|
| `customer.car_ready` | info | — | Customer | — |
| `customer.loyalty_tier_up` | info | — | Customer | — |
| `customer.promo` | info | — | Customer | — |
| `customer.birthday` | info | — | Customer | — |

### AI (Negosyo AI)

| Type | Severity | In-App | SMS | Email |
|---|---|---|---|---|
| `ai.daily_brief` | info | Always | Optional | — |
| `ai.smart_alert` | warning | Always | Critical only | — |

### Platform (From Super Admin)

| Type | Severity | In-App | SMS | Email |
|---|---|---|---|---|
| `platform.announcement` | info | Always | — | Optional |
| `platform.feature_update` | info | Always | — | Yes |

---

## Domain Models

```csharp
public sealed class Notification : IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string? RecipientUserId { get; set; }        // Null = broadcast to all tenant users
    public string? RecipientPhone { get; set; }          // For customer SMS
    public string? RecipientEmail { get; set; }
    public string Type { get; set; } = string.Empty;     // "inventory.low_stock"
    public string Category { get; set; } = string.Empty;  // operations, inventory, financial, billing, customer, ai, platform
    public string Severity { get; set; } = string.Empty;  // info, warning, critical
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }                // Deep link: "/inventory/supplies/soap-123"
    public string? ActionLabel { get; set; }              // "View Supply"
    public string? Metadata { get; set; }                 // JSON: { transactionId, amount, etc. }

    // Channel delivery status
    public bool InAppDelivered { get; set; }
    public bool SmsDelivered { get; set; }
    public bool EmailDelivered { get; set; }
    public bool SmsSkipped { get; set; }
    public bool EmailSkipped { get; set; }

    // Read tracking
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class NotificationPreference
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool InAppEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool EmailEnabled { get; set; } = false;
    public DateTime UpdatedAt { get; set; }
}
```

---

## Routing Logic

```
NotificationService.Send(request):

1. Look up NotificationTypeConfig for request.Type
   → gets: category, severity, default channels, mandatory flags

2. Create Notification record in database

3. IN-APP: Almost always. Send via SignalR to tenant group.
   → Client receives "NotificationReceived" event
   → Bell badge increments, toast appears

4. SMS: Check in this order:
   a. Is SMS mandatory for this type? (billing) → send
   b. Is severity = critical? → send (unless user explicitly blocked)
   c. Check user's NotificationPreference → smsEnabled?
   d. Check plan SMS quota remaining → skip if exhausted
   e. Send via Semaphore, increment quota counter

5. EMAIL: Check in this order:
   a. Is email mandatory for this type? (billing) → send always
   b. Check user's NotificationPreference → emailEnabled?
   c. Resolve email template ID → render with merge variables
   d. Send via Resend

6. Save delivery status flags on the Notification record
```

---

## SMS Integration (Semaphore)

```csharp
public class SemaphoreSmsService : ISmsService
{
    public async Task<SmsResult> SendAsync(string phone, string message)
    {
        var normalized = PhoneHelper.Normalize(phone); // 09XX → +639XX

        var response = await _http.PostAsync(
            "https://api.semaphore.co/api/v4/messages",
            JsonContent.Create(new {
                apikey = _apiKey,
                number = normalized,
                message = message,
                sendername = _senderName  // "SplashSphere"
            }));

        await _planEnforcement.IncrementSmsUsage();

        return new SmsResult { Success = response.IsSuccessStatusCode };
    }
}
```

**SMS format (≤160 chars):**
```
[SplashSphere] Soap at 12L - reorder in 4 days. app.splashsphere.ph

[SplashSphere] Shift flagged: Cash short P280 at Makati. Review now.

[SplashSphere] Hi Maria! Your Vios (ABC-1234) is ready at AquaShine.
```

---

## Email Integration (Resend)

**Templates** (from `splashsphere-email-templates.html`):

| Template ID | Notification Type | Key Merge Variables |
|---|---|---|
| `invoice_new` | billing.invoice_created | `{{owner_name}}`, `{{invoice_number}}`, `{{amount}}`, `{{due_date}}`, `{{payment_url}}` |
| `payment_reminder` | billing.payment_reminder | Same + `{{days_remaining}}` |
| `payment_received` | billing.payment_received | `{{payment_method}}`, `{{payment_date}}`, `{{next_billing_date}}`, `{{receipt_url}}` |
| `overdue_warning` | billing.overdue | `{{days_overdue}}`, `{{days_until_suspension}}` |
| `trial_expiring` | billing.trial_expiring | `{{days_remaining}}`, `{{transaction_count}}`, `{{total_revenue}}`, `{{employee_count}}` |
| `account_suspended` | billing.suspended | `{{invoice_number}}`, `{{amount}}`, `{{payment_url}}` |
| `payroll_summary` | payroll.processed | `{{period}}`, `{{total_amount}}`, `{{employee_count}}`, `{{dashboard_url}}` |

---

## Frontend — Notification Bell

### Header Bell Icon

```
🔔(5) ← unread count badge (red dot if critical, amber if warning)

Click → dropdown panel:
┌─────────────────────────────────────┐
│ Notifications           [Mark all ✓]│
│                                     │
│ 🔴 Shift flagged — Cash short ₱280 │
│    Makati branch · 2 min ago        │
│                                     │
│ 🟡 Soap at 12L — reorder needed    │
│    15 min ago                       │
│                                     │
│ 🟢 TXN-0312 completed — ₱690      │
│    22 min ago                       │
│                                     │
│ 🤖 Daily brief ready               │
│    6:00 AM                          │
│                                     │
│ 📢 Scheduled maintenance April 5   │
│    Yesterday                        │
│                                     │
│              [View All →]           │
└─────────────────────────────────────┘
```

### Toast Notifications

When SignalR delivers a notification:
- Toast slides in from top-right
- Auto-dismisses after 5 seconds (info), 10 seconds (warning), sticky (critical)
- Click toast → navigates to ActionUrl
- Critical: toast has red left border and subtle shake animation

### Preferences Page (`/settings/notifications`)

Toggle matrix: rows = notification types (grouped by category), columns = In-App / SMS / Email. Billing email column is locked (🔒). SMS column shows quota: "23 of 50 used this month."

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/notifications` | List notifications (paginated, filter by category/unread) |
| `GET` | `/notifications/unread-count` | Unread count for bell badge |
| `PATCH` | `/notifications/{id}/read` | Mark as read |
| `PATCH` | `/notifications/read-all` | Mark all as read |
| `DELETE` | `/notifications/{id}` | Dismiss |
| `GET` | `/notifications/preferences` | Get user preferences |
| `PUT` | `/notifications/preferences` | Update preferences |

---

## Claude Code Prompt

```
Build the unified notification system:

Domain/Entities: Notification, NotificationPreference
Application/Interfaces: INotificationService, ISmsService, IEmailService
Infrastructure/Services:
- NotificationService (routing logic: in-app + SMS + email)
- SemaphoreSmsService (Semaphore API, phone normalization, quota tracking)
- ResendEmailService (Resend API, HTML template rendering, merge variables)
- NotificationTypeConfig (static config for all types)

SignalR: Add "NotificationReceived" event to CarWashHub
EF config + migration: "AddNotifications"

Endpoints: NotificationEndpoints.cs
Frontend: Bell icon component, dropdown panel, toast system, preferences page

Wire into existing handlers:
- CreateTransactionCommandHandler → transaction.completed
- CloseShiftCommandHandler → shift.closed / shift.flagged
- CheckLowStockJob → inventory.low_stock
- All billing Hangfire jobs → billing.* types
- DetectSmartAlertsJob → ai.smart_alert
```
