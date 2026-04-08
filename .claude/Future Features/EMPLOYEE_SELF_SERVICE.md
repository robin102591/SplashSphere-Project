# SplashSphere — Employee Self-Service Portal

> **Phase:** 23.1 (Value-Add). Can build after core Phases 1-14 are complete.
> **App:** Separate route group within the admin app: `/employee-portal` — or a lightweight standalone page.
> **Access:** Employees log in via Clerk (they already have User accounts linked to Employee records). Portal shows only their own data.
> **Plan gating:** Growth+ feature.

---

## Why This Matters

In a Philippine commission-based car wash, employees ask their manager the same questions every day: "Magkano na commission ko?" (How much is my commission?), "Ilan na service ko today?" (How many services today?), "Tama ba payroll ko?" (Is my payroll correct?). The manager checks the system, reads out numbers, and this eats 30-60 minutes daily across a team of 8-10 employees.

The self-service portal puts this info directly on the employee's phone. They check it themselves. No interruptions, no disputes, full transparency.

---

## What Employees See

### Dashboard (Home)

```
┌── Hi Juan! ───────────────────────────────────────────┐
│                                                        │
│  TODAY — March 25, 2026                                │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐              │
│  │ Services │ │Commission│ │ Hours    │              │
│  │    6     │ │  ₱480    │ │ 6.5 hrs │              │
│  └──────────┘ └──────────┘ └──────────┘              │
│                                                        │
│  THIS WEEK (Mar 18-24)                                 │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐              │
│  │ Services │ │Commission│ │ Days    │              │
│  │   32     │ │  ₱2,640  │ │  5/6    │              │
│  └──────────┘ └──────────┘ └──────────┘              │
│                                                        │
│  ── Recent Services ──────────────────────────────── │
│  11:30 AM  Premium Wash (SUV/Large) ......... ₱94    │
│  10:45 AM  Basic Wash (Sedan/Medium) ........ ₱33    │
│  10:15 AM  Wax & Polish (Sedan/Medium) ...... ₱68    │
│  9:30 AM   Basic Wash + Tire (SUV/Large) .... ₱57    │
│                                                        │
│  ── Leaderboard Today ──────────────────────────── │
│  🥇 Pedro S. — ₱620 (8 services)                     │
│  🥈 You — ₱480 (6 services)                          │
│  🥉 Maria G. — ₱390 (5 services)                     │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### My Commission (Detail View)

```
┌── Commission History ─────────────────────────────────┐
│                                                        │
│  [Today] [This Week] [Last Week] [This Month]         │
│                                                        │
│  This Week: ₱2,640 from 32 services                   │
│                                                        │
│  Mon Mar 18 — 7 services ................. ₱560        │
│  Tue Mar 19 — 6 services ................. ₱495        │
│  Wed Mar 20 — Day Off                                  │
│  Thu Mar 21 — 5 services ................. ₱410        │
│  Fri Mar 22 — 8 services ................. ₱680        │
│  Sat Mar 23 — 6 services ................. ₱495        │
│                                                        │
│  ── Breakdown by Service ────────────────────────── │
│  Basic Wash ........... 14 × avg ₱33 ....... ₱462     │
│  Premium Wash ......... 8 × avg ₱57 ........ ₱456     │
│  Wax & Polish ......... 6 × avg ₱68 ........ ₱408     │
│  Tire & Rim ........... 4 × avg ₱18 ........  ₱72     │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### My Attendance

```
┌── Attendance — March 2026 ────────────────────────────┐
│                                                        │
│  Days Worked: 22 / 26 working days                     │
│  Attendance Rate: 85%                                  │
│                                                        │
│  Su Mo Tu We Th Fr Sa                                  │
│      ✅ ✅ ✅ ✅ ✅ ✅                                  │
│  —  ✅ ✅ ❌ ✅ ✅ ✅                                  │
│  —  ✅ ✅ ✅ ✅ ✅ ✅                                  │
│  —  ✅ ✅ ✅ 📍 •  •                                  │
│                                                        │
│  ✅ Present  ❌ Absent  📍 Today  — Day Off            │
│                                                        │
│  Today: Time in 8:02 AM | Time out: —                  │
│  Hours today: 6.5 hours (still active)                 │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### My Payroll

```
┌── Payroll History ────────────────────────────────────┐
│                                                        │
│  ── Week 12 (Mar 18-24) — PROCESSED ──────────────── │
│  Commission:          ₱2,640                          │
│  Bonus:               ₱200                            │
│  Gross:               ₱2,840                          │
│  Deductions:          -₱500 (cash advance)            │
│  Net Pay:             ₱2,340                          │
│                                                        │
│  ── Week 11 (Mar 11-17) — PROCESSED ──────────────── │
│  Commission:          ₱3,120                          │
│  Bonus:               ₱0                              │
│  Gross:               ₱3,120                          │
│  Deductions:          -₱500 (cash advance)            │
│  Net Pay:             ₱2,620                          │
│                                                        │
│  ── Cash Advance Balance ────────────────────────── │
│  Original: ₱3,000 | Paid: ₱1,500 | Remaining: ₱1,500│
│  Weekly deduction: ₱500                               │
│                                                        │
└────────────────────────────────────────────────────────┘
```

---

## Security

- Employee sees ONLY their own data — never other employees' payroll, commission details, or personal info
- Leaderboard shows names + totals only (no payroll details of others)
- Read-only — employees cannot modify any records
- Tenant owner can enable/disable the portal in settings
- The portal reuses existing Clerk auth — employee logs in with their linked User account

---

## Domain Models

No new entities needed — the portal reads from existing:
- `TransactionEmployee` → commission per service
- `ServiceEmployeeAssignment` + `PackageEmployeeAssignment` → service details
- `Attendance` → time in/out, hours, presence
- `PayrollEntry` → weekly payroll breakdown
- `CashAdvance` → outstanding balance
- `Employee` → profile info

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/employee-portal/dashboard` | Today's stats + this week summary + recent services |
| `GET` | `/employee-portal/commission` | Commission history (filter by period) |
| `GET` | `/employee-portal/commission/breakdown` | Commission grouped by service type |
| `GET` | `/employee-portal/attendance` | Attendance calendar for current month |
| `GET` | `/employee-portal/payroll` | Payroll history (last 12 weeks) |
| `GET` | `/employee-portal/cash-advance` | Outstanding cash advance balance |
| `GET` | `/employee-portal/leaderboard` | Today's commission leaderboard (names + totals only) |

All endpoints automatically filter by the authenticated user's `employeeId`. No `employeeId` parameter accepted — prevents snooping.

---

## Frontend

### Route: `/employee-portal` (within admin app)

Mobile-first layout — most employees access this on their phone:
- Bottom tab navigation: Dashboard | Commission | Attendance | Payroll
- No sidebar (employees don't need admin nav)
- 56px touch targets (same as POS)
- Pull-to-refresh on all pages
- Auto-refresh every 60 seconds on dashboard (see commission update in real-time after a service)

### Tenant Settings

Add to `/settings`:
```
Employee Self-Service Portal: [✓ Enabled]
Show commission leaderboard:  [✓ Enabled]
```

---

## Claude Code Prompt

```
Build the Employee Self-Service Portal:

Backend:
- Application/Features/EmployeePortal/:
  GetDashboardQuery, GetCommissionHistoryQuery, GetCommissionBreakdownQuery,
  GetAttendanceCalendarQuery, GetPayrollHistoryQuery, GetCashAdvanceBalanceQuery,
  GetLeaderboardQuery
- All queries resolve employeeId from the authenticated user's User→Employee link
- Leaderboard returns names + commission totals only (no sensitive data)
- Endpoints: EmployeePortalEndpoints.cs under /api/v1/employee-portal

Frontend:
- Route group: /employee-portal with minimal layout (no sidebar, bottom tabs)
- 4 tabs: Dashboard, Commission, Attendance, Payroll
- Mobile-first, 56px touch targets, pull-to-refresh
- Real-time commission update (poll every 60 seconds or SignalR)
- Leaderboard with rank badges (🥇🥈🥉)
- Settings toggle to enable/disable portal per tenant

Plan gating: Growth+ feature. Starter tenants see "Upgrade to enable Employee Portal."
```
