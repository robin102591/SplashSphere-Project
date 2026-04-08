# SplashSphere — Support Ticket System

> **Phase:** Integrated into Phase 20 (Super Admin) and the tenant Admin app.
> **Two sides:** Tenants file tickets from their admin dashboard. LezanobTech manages them from the Super Admin.
> **Priority:** Build after launch once you have 10+ tenants. Before that, support via email/chat is sufficient.

---

## Why Not Just Email

At 5 tenants, email works. At 50 tenants, you're drowning in unthreaded Gmail conversations with no way to track which tenant asked what, what's resolved, what's pending, and what keeps recurring. A ticket system gives you:

- **Accountability** — every request has an owner and a status
- **History** — full conversation thread per issue, not scattered emails
- **Metrics** — how long does it take to resolve? What's the most common issue?
- **Self-service** — knowledge base reduces repeat questions
- **Professionalism** — tenants see a proper support experience, not "text mo na lang ako"

---

## Tenant Side (Admin Dashboard)

### Where It Lives

Add a "Help & Support" section to the tenant's admin sidebar:

```
Settings
Help & Support        ← NEW
├── Submit a Ticket
├── My Tickets
└── Help Center (Knowledge Base)
```

### Submit a Ticket (`/support/new`)

```
┌── Submit a Support Ticket ────────────────────────────┐
│                                                        │
│  Category *                                            │
│  [Billing & Payments          ▾]                      │
│                                                        │
│  Subject *                                             │
│  [I was charged twice for March                     ]  │
│                                                        │
│  Description *                                         │
│  ┌──────────────────────────────────────────────────┐ │
│  │ I see two charges of ₱2,999 on my GCash for     │ │
│  │ March. My invoice is INV-2026-0312. Can you      │ │
│  │ please check and refund the duplicate?           │ │
│  │                                                  │ │
│  └──────────────────────────────────────────────────┘ │
│                                                        │
│  Priority                                              │
│  ○ Low — General question, no urgency                  │
│  ● Medium — Issue affecting operations (default)       │
│  ○ High — Critical issue, can't process transactions   │
│                                                        │
│  Attachments (optional)                                │
│  [📎 Upload screenshot or file]  (max 5 MB, images/PDF)│
│                                                        │
│                              [Submit Ticket]           │
└────────────────────────────────────────────────────────┘
```

**Categories:**
- Billing & Payments
- Technical Issue (Bug)
- Feature Request
- Account & Subscription
- POS & Transactions
- Employee & Payroll
- Inventory
- General Question

### My Tickets (`/support`)

```
┌── My Support Tickets ─────────────────────────────────┐
│                                                        │
│  [All] [Open] [Waiting on Us] [Resolved]              │
│                                                        │
│  #TKT-0045  Charged twice for March                   │
│  Billing & Payments • Medium • ⏳ Waiting on Support   │
│  Submitted 2 hours ago • Last reply: 30 min ago        │
│                                                        │
│  #TKT-0042  Can't add 4th branch                      │
│  Account & Subscription • Low • ✅ Resolved             │
│  Submitted 3 days ago • Resolved 2 days ago            │
│                                                        │
│  #TKT-0038  POS freezes on large transactions         │
│  Technical Issue • High • 🔄 In Progress               │
│  Submitted 5 days ago • Last reply: 1 day ago          │
│                                                        │
│                                  [+ New Ticket]        │
└────────────────────────────────────────────────────────┘
```

### Ticket Detail (`/support/{ticketId}`)

Conversation thread — like a chat, not email:

```
┌── #TKT-0045 — Charged twice for March ────────────────┐
│  Category: Billing • Priority: Medium • Status: Open   │
│  Submitted: April 3, 2026 10:15 AM                     │
│                                                        │
│  ── Conversation ─────────────────────────────────── │
│                                                        │
│  👤 You (Apr 3, 10:15 AM)                              │
│  I see two charges of ₱2,999 on my GCash for March.   │
│  My invoice is INV-2026-0312. Can you please check     │
│  and refund the duplicate?                             │
│  📎 gcash-screenshot.jpg                               │
│                                                        │
│  🛡️ SplashSphere Support (Apr 3, 10:45 AM)            │
│  Hi Juan! Thanks for reaching out. I can see the       │
│  duplicate charge in our records. I've initiated a     │
│  refund of ₱2,999 to your GCash. It should reflect    │
│  within 3-5 business days.                             │
│                                                        │
│  👤 You (Apr 3, 11:00 AM)                              │
│  Thank you! That was fast. 👍                          │
│                                                        │
│  ── Reply ────────────────────────────────────────── │
│  ┌──────────────────────────────────────────────────┐ │
│  │ Type your reply...                               │ │
│  └──────────────────────────────────────────────────┘ │
│  [📎 Attach]                          [Send Reply]    │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## Super Admin Side (LezanobTech)

### Support Dashboard (`/support` in Super Admin)

```
┌── Support Dashboard ──────────────────────────────────┐
│                                                        │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐    │
│  │ Open    │ │ In Prog │ │ Waiting │ │ Avg Time│    │
│  │   12    │ │    5    │ │    3    │ │  4.2 hr │    │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘    │
│                                                        │
│  [All] [Open] [In Progress] [Waiting] [Resolved]      │
│                                                        │
│  ⚡ HIGH PRIORITY                                      │
│  #TKT-0048  POS down, can't process transactions      │
│  SparkleWash Makati • Technical • 🔴 High • 15 min ago│
│  [Assign to me]                                        │
│                                                        │
│  📋 MEDIUM PRIORITY                                    │
│  #TKT-0045  Charged twice for March                   │
│  AquaShine Makati • Billing • 🟡 Medium • 2 hrs ago   │
│  Assigned: Rob                                         │
│                                                        │
│  #TKT-0044  How to set up commission matrix?           │
│  CleanDrive BGC • General • 🟡 Medium • 5 hrs ago     │
│  Unassigned                                            │
│                                                        │
│  📌 LOW PRIORITY                                       │
│  #TKT-0042  Feature request: export to Excel           │
│  WashPro QC • Feature Request • 🟢 Low • 2 days ago   │
│  Assigned: Rob                                         │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### Ticket Detail (Super Admin View)

Same conversation thread as the tenant sees, PLUS internal tools:

```
┌── #TKT-0045 — Charged twice for March ────────────────┐
│                                                        │
│  Tenant: AquaShine Car Wash (Growth plan)              │
│  Submitted by: Juan Reyes (Owner)                      │
│  Category: Billing • Priority: Medium                  │
│  Status: [Open ▾]  Assigned: [Rob ▾]                   │
│                                                        │
│  ── Quick Actions ────────────────────────────────── │
│  [View Tenant] [View Billing] [Impersonate] [Escalate] │
│                                                        │
│  ── Conversation ─────────────────────────────────── │
│  (same thread as tenant sees)                          │
│                                                        │
│  ── Internal Notes (tenant cannot see) ──────────── │
│  ┌──────────────────────────────────────────────────┐ │
│  │ Rob (Apr 3, 10:30 AM):                           │ │
│  │ Confirmed duplicate PayMongo webhook fired twice  │ │
│  │ for the same checkout session. Refund initiated    │ │
│  │ via PayMongo dashboard. Need to add idempotency   │ │
│  │ check to webhook handler — logged as bug #BUG-012 │ │
│  └──────────────────────────────────────────────────┘ │
│  [+ Add Internal Note]                                 │
│                                                        │
│  ── Reply to Tenant ─────────────────────────────── │
│  ┌──────────────────────────────────────────────────┐ │
│  │ Type your reply...                               │ │
│  └──────────────────────────────────────────────────┘ │
│  [📎 Attach]  ☐ Use template   [Send Reply]          │
│                                                        │
│  ── Ticket Info ─────────────────────────────────── │
│  Created: Apr 3, 10:15 AM                              │
│  First response: Apr 3, 10:45 AM (30 min)              │
│  Resolution time: —                                    │
│  Replies: 3 (2 support, 1 tenant)                      │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### Reply Templates (Canned Responses)

Pre-written responses for common questions:

```
Templates:
├── Billing
│   ├── "Refund initiated — 3-5 business days"
│   ├── "Payment method update instructions"
│   └── "Plan upgrade/downgrade confirmation"
├── Technical
│   ├── "Clear browser cache and retry"
│   ├── "Known issue — fix in progress"
│   └── "Please provide screenshot/steps to reproduce"
├── Account
│   ├── "Branch limit reached — upgrade to Growth"
│   ├── "Trial extended by 7 days"
│   └── "Password reset via Clerk"
└── General
    ├── "Feature request logged — thank you!"
    └── "Here's a link to our guide: ..."
```

The admin clicks "Use template" → selects a template → it fills the reply box → admin can edit before sending.

---

## Domain Models

```csharp
public sealed class SupportTicket
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string SubmittedByUserId { get; set; } = string.Empty;
    public string TicketNumber { get; set; } = string.Empty;     // TKT-0001
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public string? AssignedToAdminId { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ResolutionSummary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<TicketMessage> Messages { get; set; } = [];
    public List<TicketAttachment> Attachments { get; set; } = [];
}

public sealed class TicketMessage
{
    public string Id { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;       // UserId or AdminId
    public TicketMessageSender SenderType { get; set; }         // Tenant or Support
    public string Content { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; }                    // Hidden from tenant
    public DateTime CreatedAt { get; set; }

    public List<TicketAttachment> Attachments { get; set; } = [];
}

public sealed class TicketAttachment
{
    public string Id { get; set; } = string.Empty;
    public string? TicketId { get; set; }
    public string? MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;         // Azure Blob Storage URL
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public sealed class ReplyTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;           // "Refund initiated"
    public string Content { get; set; } = string.Empty;         // Full reply text with {{variables}}
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum TicketPriority { Low, Medium, High }
public enum TicketStatus { Open, InProgress, WaitingOnTenant, Resolved, Closed }
public enum TicketMessageSender { Tenant, Support }
```

---

## Ticket Lifecycle

```
OPEN
  │ Tenant submits ticket
  │ Notification to super admin: "New ticket from AquaShine"
  ▼
IN PROGRESS
  │ Admin assigns to self, starts working
  │ Admin replies → status stays InProgress
  ▼
WAITING ON TENANT
  │ Admin asks for more info → status changes
  │ Tenant replies → status reverts to InProgress
  ▼
RESOLVED
  │ Admin marks as resolved with summary
  │ Tenant receives: "Your ticket has been resolved"
  │ Tenant can reopen within 7 days if not satisfied
  ▼
CLOSED
  │ Auto-closed 7 days after resolution if not reopened
  │ Or manually closed by admin
```

**Auto-close rule:** Tickets in "Waiting on Tenant" for 7 days with no reply → auto-close with message "This ticket was automatically closed due to inactivity."

---

## SLA Tracking

Track response and resolution times for internal performance monitoring:

| Metric | Target | Tracking |
|---|---|---|
| First response time | < 4 hours (business hours) | `FirstResponseAt - CreatedAt` |
| Resolution time (Low) | < 72 hours | `ResolvedAt - CreatedAt` |
| Resolution time (Medium) | < 24 hours | `ResolvedAt - CreatedAt` |
| Resolution time (High) | < 4 hours | `ResolvedAt - CreatedAt` |
| Customer satisfaction | > 4.0 / 5.0 | Post-resolution rating |

**SLA Dashboard** (Super Admin → Support page):

```
┌── SLA Performance (Last 30 Days) ─────────────────────┐
│                                                        │
│  Avg First Response:  2.4 hours  ✅ (target: < 4 hr)  │
│  Avg Resolution (H):  3.1 hours  ✅ (target: < 4 hr)  │
│  Avg Resolution (M):  18 hours   ✅ (target: < 24 hr) │
│  Avg Resolution (L):  52 hours   ✅ (target: < 72 hr) │
│  Satisfaction:         4.3 / 5.0  ✅ (target: > 4.0)  │
│                                                        │
│  Tickets by Category:                                  │
│  Billing: 12 (28%) | Technical: 9 (21%) |             │
│  Feature Req: 8 (19%) | Account: 6 (14%) |            │
│  POS: 4 (9%) | General: 4 (9%)                        │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## Knowledge Base (Help Center)

Before a tenant submits a ticket, they should be able to find answers themselves.

### Tenant View (`/support/help`)

```
┌── Help Center ────────────────────────────────────────┐
│                                                        │
│  🔍 [Search help articles...                        ]  │
│                                                        │
│  📖 Getting Started                                    │
│     How to set up your first branch                    │
│     Adding employees and setting commission rates      │
│     Creating your first transaction on POS             │
│     Setting up the pricing matrix                      │
│                                                        │
│  💳 Billing & Payments                                 │
│     Understanding your subscription plan               │
│     How to upgrade or downgrade your plan               │
│     Payment methods (GCash, Card, Bank Transfer)       │
│     How to view your invoices                          │
│                                                        │
│  📊 Reports & Analytics                                │
│     Understanding the P&L report                       │
│     Generating payroll reports                         │
│     Viewing employee performance                       │
│                                                        │
│  🔧 Troubleshooting                                    │
│     POS not loading — what to do                       │
│     Sync issues with offline transactions              │
│     Password reset                                     │
│                                                        │
│  Can't find what you need?  [Submit a Ticket]          │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### Super Admin — Article Management (`/support/articles`)

Simple CMS for help articles:

```csharp
public sealed class HelpArticle
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;          // "setting-up-pricing-matrix"
    public string Category { get; set; } = string.Empty;      // "Getting Started"
    public string Content { get; set; } = string.Empty;       // Markdown
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

Articles are written in Markdown, rendered in the tenant's Help Center. The super admin manages them from `/support/articles` with a CRUD interface. Track view counts to identify what tenants read most (and what's missing).

---

## Notifications

| Event | Who Gets Notified | Channel |
|---|---|---|
| Ticket submitted | Super admin team | In-app + email |
| High priority ticket | Super admin team | In-app + SMS |
| Support replies | Tenant (submitter) | In-app + email |
| Tenant replies | Assigned admin | In-app |
| Ticket resolved | Tenant (submitter) | In-app + email |
| Ticket auto-closed | Tenant (submitter) | In-app |
| SLA breach (first response overdue) | Super admin team | In-app + SMS |

---

## API Endpoints

### Tenant-Side

| Method | Route | Description |
|---|---|---|
| `GET` | `/support/tickets` | List my tickets (filter by status) |
| `POST` | `/support/tickets` | Submit a new ticket |
| `GET` | `/support/tickets/{id}` | Ticket detail with messages |
| `POST` | `/support/tickets/{id}/reply` | Reply to a ticket |
| `PATCH` | `/support/tickets/{id}/reopen` | Reopen a resolved ticket |
| `POST` | `/support/tickets/{id}/attachments` | Upload attachment |
| `GET` | `/support/help` | List help article categories |
| `GET` | `/support/help/{slug}` | Get article by slug |
| `GET` | `/support/help/search?q=` | Search articles |

### Super Admin

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/saas/tickets` | List all tickets (filter by status, priority, tenant, assigned) |
| `GET` | `/api/saas/tickets/{id}` | Ticket detail with messages + internal notes |
| `PATCH` | `/api/saas/tickets/{id}/assign` | Assign ticket to admin |
| `PATCH` | `/api/saas/tickets/{id}/status` | Change ticket status |
| `POST` | `/api/saas/tickets/{id}/reply` | Reply to tenant (public) |
| `POST` | `/api/saas/tickets/{id}/note` | Add internal note (hidden from tenant) |
| `PATCH` | `/api/saas/tickets/{id}/resolve` | Resolve with summary |
| `GET` | `/api/saas/tickets/stats` | SLA metrics, category breakdown |
| `GET` | `/api/saas/reply-templates` | List canned responses |
| `POST` | `/api/saas/reply-templates` | Create template |
| `GET` | `/api/saas/help-articles` | List articles (admin view) |
| `POST` | `/api/saas/help-articles` | Create article |
| `PUT` | `/api/saas/help-articles/{id}` | Update article |

---

## Claude Code Prompts

### Prompt 20.5 — Support Ticket Backend

```
Add the support ticket system:

Domain/Entities (in SaasDbContext):
- SupportTicket, TicketMessage, TicketAttachment, ReplyTemplate, HelpArticle
- Enums: TicketPriority, TicketStatus, TicketMessageSender

EF configs with indexes on [tenantId, status], [priority], [assignedToAdminId]
Auto-generate ticket numbers: TKT-{Sequence}
Migration: "AddSupportTickets"

Application layer:
- Tenant-side: CreateTicketCommand, ReplyToTicketCommand, GetMyTicketsQuery,
  GetTicketDetailQuery, ReopenTicketCommand
- Admin-side: GetAllTicketsQuery (with filters), AssignTicketCommand,
  ChangeTicketStatusCommand, AdminReplyCommand, AddInternalNoteCommand,
  ResolveTicketCommand, GetSlaStatsQuery
- Knowledge base: CRUD for HelpArticle, SearchArticlesQuery
- Reply templates: CRUD for ReplyTemplate

File upload: save attachments to Azure Blob Storage, store URL in TicketAttachment

Hangfire jobs:
- AutoCloseInactiveTickets: daily, close tickets in WaitingOnTenant > 7 days
- SlaBreachCheck: hourly, flag tickets exceeding SLA targets

Notifications: integrate with INotificationService
- ticket.submitted → super admin (in-app + email)
- ticket.high_priority → super admin (in-app + SMS)
- ticket.reply_from_support → tenant (in-app + email)
- ticket.resolved → tenant (in-app + email)
- ticket.sla_breach → super admin (in-app + SMS)
```

### Prompt 20.6 — Support Ticket Frontend

```
Build both sides of the support UI:

Tenant Admin App:
1. Add "Help & Support" sidebar nav group
2. /support — My tickets list with status tabs
3. /support/new — Submit ticket form (category, subject, description, priority, attachments)
4. /support/{id} — Ticket conversation thread, reply box, attachment viewer
5. /support/help — Knowledge base with category accordion and search
6. /support/help/{slug} — Article detail page (markdown rendered)

Super Admin (Blazor):
1. Update /support page from placeholder to full ticket management
2. Ticket list with filters (status, priority, tenant, assigned to)
3. Ticket detail: conversation + internal notes tab + quick actions
   (View Tenant, Impersonate, View Billing)
4. Reply with template selector
5. SLA dashboard card on support page header
6. /support/articles — Help article CRUD (markdown editor)
7. /support/templates — Reply template management
```

---

## Phase Summary

| Prompt | What | Layer |
|---|---|---|
| 20.5 | Ticket entities, CQRS, file upload, Hangfire auto-close, SLA, notifications | Backend |
| 20.6 | Tenant ticket UI, super admin ticket management, knowledge base, templates | Frontend (both apps) |

**Total: 2 prompts added to Phase 20.**
