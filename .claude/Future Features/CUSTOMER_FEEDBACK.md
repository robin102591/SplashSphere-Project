# SplashSphere — Customer Feedback & Ratings

> **Phase:** 23.3 (Value-Add). Requires core transaction system + optional Customer App (Phase 22).
> **Plan gating:** Available on all plans (basic rating). Detailed analytics on Growth+.

---

## What It Does

After a car wash is completed, the customer rates their experience (1-5 stars) and optionally leaves a comment. The rating is tied to the transaction, the employees who worked on the car, and the branch. This creates a feedback loop: owners see which employees deliver great service, which services get complaints, and what their overall satisfaction score is.

---

## How Ratings Are Collected

### Channel 1: SMS (Primary — No App Needed)

After transaction completes, if the customer has a phone number:

```
[SplashSphere] Hi Maria! How was your wash at AquaShine today?
Rate 1-5 by replying with a number. 5=Excellent 1=Poor
```

Customer replies `5` → rating saved. Simple, works on any phone.

For detailed feedback, include a link:
```
[SplashSphere] Thanks for visiting AquaShine! Rate your experience:
https://rate.splashsphere.ph/r/TXN0312ABC
```

Link opens a mobile-friendly single-page form (no login required).

### Channel 2: Customer App (If Phase 22 is built)

After service completion notification, show rating prompt:
```
⭐ Rate your experience
☆ ☆ ☆ ☆ ☆
[Optional: Leave a comment]
[Submit]
```

### Channel 3: QR Code Receipt

Print a QR code on the receipt that links to the rating page.

---

## Rating Page (Public, No Login)

```
┌── Rate Your Experience ───────────────────────────────┐
│                                                        │
│  AquaShine Car Wash — Makati                          │
│  March 25, 2026 • Basic Wash + Tire Shine             │
│                                                        │
│  How was your experience?                              │
│  ⭐ ⭐ ⭐ ⭐ ☆                                        │
│                                                        │
│  What went well? (optional)                            │
│  ☐ Fast service  ☐ Friendly staff  ☐ Great results    │
│  ☐ Clean facility  ☐ Good value                       │
│                                                        │
│  Any comments? (optional)                              │
│  ┌──────────────────────────────────────────────────┐ │
│  │ Very quick and the car looks spotless!           │ │
│  └──────────────────────────────────────────────────┘ │
│                                                        │
│                              [Submit Rating]           │
│                                                        │
│  ⭐⭐⭐⭐⭐ experience? Share on Google Maps!          │
│  [Leave a Google Review →]                             │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**Google Review prompt:** If the customer gives 4-5 stars, show a link to the car wash's Google Maps listing. This turns happy customers into public reviews. Only show for positive ratings — never prompt 1-2 star customers to post publicly.

---

## Domain Models

```csharp
public sealed class CustomerRating
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string BranchId { get; set; } = string.Empty;
    public int Stars { get; set; }                              // 1-5
    public string? Comment { get; set; }
    public string? PositiveTags { get; set; }                   // JSON: ["fast_service", "friendly_staff"]
    public string RatingToken { get; set; } = string.Empty;     // Unique token for the rating URL
    public bool IsPublic { get; set; } = true;                  // Owner can hide inappropriate comments
    public DateTime CreatedAt { get; set; }

    // Links ratings to the employees who worked on this transaction
    public List<string>? EmployeeIds { get; set; }              // Denormalized from TransactionEmployee
}
```

---

## Admin Dashboard Integration

### Satisfaction Widget (Dashboard Home)

```
┌── Customer Satisfaction ──────────────────────────────┐
│  ⭐ 4.6 / 5.0  (last 30 days, 142 ratings)           │
│  ██████████████████████████░░░░  92% positive (4-5★)  │
│                                                        │
│  5★ ████████████████████  68%                          │
│  4★ ████████░░░░░░░░░░░░  24%                          │
│  3★ ██░░░░░░░░░░░░░░░░░░   5%                          │
│  2★ █░░░░░░░░░░░░░░░░░░░   2%                          │
│  1★ ░░░░░░░░░░░░░░░░░░░░   1%                          │
└────────────────────────────────────────────────────────┘
```

### Ratings Page (`/ratings`)

Full list of ratings with filters (branch, stars, date range). Owner can reply to feedback or hide inappropriate comments.

### Employee Performance Integration

Add "Avg Rating" column to employee performance views:
```
Juan D.C. — ₱12,480 commission — 142 services — ⭐ 4.8 avg
Pedro S.  — ₱10,920 commission — 128 services — ⭐ 4.5 avg
Maria G.  — ₱9,640 commission  — 118 services — ⭐ 4.2 avg
```

Feeds into Negosyo AI: "Pedro has the highest efficiency but Maria's satisfaction score dropped from 4.5 to 4.2 this month — check if she needs support."

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/ratings` | List ratings (filter by branch, stars, period) |
| `GET` | `/ratings/summary` | Aggregate: avg rating, distribution, trend |
| `POST` | `/ratings/{token}` | Submit a rating (public, no auth — token-based) |
| `PATCH` | `/ratings/{id}/hide` | Hide inappropriate rating |
| `GET` | `/ratings/employees` | Avg rating per employee |

---

## Hangfire Job

| Job | Schedule | Description |
|---|---|---|
| `SendRatingRequests` | After each transaction (delayed 2 hours) | Send SMS with rating link to customers with phone numbers |

---

## Claude Code Prompt

```
Build Customer Feedback & Ratings:

Domain: CustomerRating entity with rating token for public URL
Application: SubmitRatingCommand (token-based, no auth), GetRatingsQuery,
  GetRatingSummaryQuery, HideRatingCommand, GetEmployeeRatingsQuery
  
Public rating page: /rate/{token} — standalone page, no login required,
  mobile-optimized, star picker + tags + comment. Google Review prompt on 4-5★.

Admin: /ratings list page, satisfaction widget on dashboard,
  add avg rating to employee performance views.

SMS integration: send rating request 2 hours after transaction completion.
Negosyo AI: add customer satisfaction metrics to data functions.

Plan gating: All plans get basic rating collection. Growth+ gets analytics and employee ratings.
```
