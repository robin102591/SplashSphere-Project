# Frontend Page Inventory

## Auth Pages (both apps)

| Route | Page |
|---|---|
| `/sign-in` | Custom sign-in (email/password + social via Clerk headless hooks) |
| `/sign-up` | Custom sign-up + email verification (admin only, NOT on POS) |
| `/sso-callback` | OAuth redirect handler |
| `/onboarding` | Tenant onboarding wizard (admin only) |

## Admin Dashboard

30+ routes for branches, services, packages, employees, payroll, customers, vehicles, merchandise, transactions, reports, settings.

| Route | Page |
|---|---|
| `/dashboard/shifts` | Shift list -- paginated, filterable by branch/date/status/review |
| `/dashboard/shifts/[id]` | Shift detail -- EOD report + manager review actions |
| `/dashboard/reports/shift-variance` | Variance report -- per-cashier trend chart with thresholds |
| `/dashboard/cash-advances` | Cash Advances -- list with status actions (approve/disburse/cancel) + create dialog |
| `/dashboard/attendance` | Attendance Report -- per-employee stats (days, tardiness, hours) with filters + CSV export |
| `/dashboard/audit-logs` | Audit Logs -- paginated log viewer with entity type/ID/user/date filters + expandable JSON changes |
| `/dashboard/subscription` | Subscription -- current plan card with usage meters, plan comparison grid with upgrade buttons |
| `/dashboard/billing` | Billing -- next billing date, payment history with PDF download + Pay Now, cancel subscription |
| `/dashboard/expenses` | Expenses -- list with filters, record expense dialog, category/branch/date filters |
| `/dashboard/reports/profit-loss` | P&L Dashboard -- revenue/expenses/net profit KPI cards, trend chart, category breakdown, daily table |
| `/dashboard/loyalty` | Loyalty Program -- dashboard (members, points, tiers), rewards catalogue CRUD, program settings + tier config |
| `/dashboard/reports/customer-analytics` | Customer Analytics -- retention rate, new vs returning trend, visit frequency distribution, top 20 customers table |
| `/dashboard/reports/peak-hours` | Peak Hours Heatmap -- 7x24 day-of-week x hour grid, transaction/revenue toggle, color intensity legend |
| `/dashboard/reports/employee-performance` | Employee Performance -- leaderboard rankings, top 10 chart, sortable by revenue/services/commissions/attendance |
| `/dashboard/franchise` | Franchise Network Overview -- KPI cards + franchisee performance table |
| `/dashboard/franchise/franchisees` | Franchisees list with invite button |
| `/dashboard/franchise/franchisees/[id]` | Franchisee detail -- agreement, royalties, actions |
| `/dashboard/franchise/royalties` | Royalty periods -- paginated, filterable, mark-paid |
| `/dashboard/franchise/templates` | Service template CRUD + push to franchisees |
| `/dashboard/franchise/compliance` | Compliance report -- color-coded matrix |
| `/dashboard/franchise/settings` | Franchise settings form |
| `/dashboard/franchise/my-agreement` | Franchisee: read-only agreement view |
| `/dashboard/franchise/my-royalties` | Franchisee: paginated royalty statements |
| `/dashboard/franchise/benchmarks` | Franchisee: performance vs network benchmarks |
| `/franchise/accept` | **Public** -- franchise invitation acceptance (token validation + onboarding form) |
| `/dashboard/settings/company` | Company Profile -- business identity, contact, structured address, tax/registration (TIN, DTI/SEC, VAT flag), social URLs, GCash number. Drives branding on receipts, reports, and Connect listing. |
| `/dashboard/settings/receipt` | Receipt Designer -- per-section toggles (Header / Body / Customer / Footer / Format), custom header/footer text, thank-you message, paper width + font size. Live thermal-receipt preview on the right that updates as toggles change. Slice 2: tenant default only; per-branch override comes in slice 4. |
| `/dashboard/settings/import` | Data Import Wizard -- 4-step CSV/Excel import (upload, column mapping, validation, execute) |
| `/dashboard/settings/notifications` | Notification Preferences -- per-type SMS/email channel toggles with mandatory indicators |
| `/dashboard/supplies` | Supply list with category/branch/stock filters, quick actions |
| `/dashboard/supplies/[id]` | Supply detail: stock gauge, movements timeline, usage/restock dialogs |
| `/dashboard/equipment` | Equipment list with status badges and maintenance indicators |
| `/dashboard/equipment/[id]` | Equipment detail with maintenance log timeline |
| `/dashboard/purchase-orders` | PO list with status lifecycle badges |
| `/dashboard/purchase-orders/new` | Create PO form with line items |
| `/dashboard/purchase-orders/[id]` | PO detail with receive items workflow |
| `/dashboard/suppliers` | Supplier CRUD list |
| `/dashboard/bookings` | Bookings -- list + week calendar view with branch/status/date filters + detail dialog (gated on `online_booking`) |
| `/dashboard/settings/booking` | Per-branch online-booking configuration -- hours, slot interval, lead time, availability toggles (gated on `online_booking`) |

## POS App

| Route | Page |
|---|---|
| `/` | POS Home -- quick actions |
| `/queue` | **Queue Board** -- Kanban: WAITING / CALLED / IN_SERVICE columns |
| `/queue/add` | **Add to Queue** -- plate lookup, priority, preferred services |
| `/transactions/new` | New Transaction (supports direct OR from-queue entry) |
| `/transactions/[id]` | Transaction detail + receipt + auto-print prompt |
| `/receipt/[id]` | Standalone receipt page -- screen preview, print/PDF download, auto-print via `?print=1` |
| `/history` | Today's transactions |
| `/customers/lookup` | Plate/customer search |
| `/attendance` | Clock in/out |
| `/queue-display` | **PUBLIC (no auth)** full-screen queue for wall TV |
| `/shift/open` | Open Shift -- opening cash fund entry with quick presets |
| `/shift` | Active Shift Panel -- stats, cash movements log, payment breakdown, actions |
| `/shift/cash-movement` | Cash Movement Form -- Cash In / Cash Out with presets |
| `/shift/close` | Close Shift -- 3-step wizard: summary -> denomination count -> confirm |
| `/shift/report` | Shift Report -- printable EOD report with top services & employees |

## POS UX Requirements

1. **Large touch targets** -- 48px+ height.
2. **Minimal navigation** -- single-page panel layout for transactions.
3. **High contrast** status colors.
4. **Keyboard/scanner support**.
5. **Running totals always visible**.
6. **Two entry points** for transactions: Direct and From Queue.

### New Transaction Screen -- supports `?queueEntryId=xxx` query param

When `queueEntryId` is present: pre-fill vehicle/customer from queue entry, pre-select preferred services, on submit also link the queue entry.

### Booking-aware POS behavior

- **Queue Board (`/queue`)**: queue entries linked to a booking display a 📅 badge with the slot time (Manila TZ). `Confirmed` bookings show a **Check In** action that flips Confirmed → Arrived and ensures a queue entry exists.
- **Start Service**: when a `Booked` queue entry has an unclassified vehicle (`IsVehicleClassified = false`), a classification modal is required before service can start — cashier picks VehicleType + Size, which locks exact service prices on the linked booking.
- **New Transaction (`/transactions/new`)**: when navigated from a booked queue entry, a "From booking" banner is shown and the service list is auto-populated from `BookingService` rows with prices pre-locked; cashier still adjusts quantities and adds ad-hoc items as needed.

## Customer Connect App (`apps/customer/`)

End-customer PWA on port **3002**. Mobile-first, bottom-tab shell (Home / Book / History / Profile). Phone-OTP auth via the `ConnectJwt` scheme — **no Clerk, no SignalR** (polling for live data). All authed routes are wrapped in a client-side `<AuthGuard>`; there is no Next middleware because tokens live in `localStorage`. i18n via `next-intl` (en/fil, cookie-based). Serwist-backed PWA with offline fallback.

| Route | Page |
|---|---|
| `/auth` | **Public** — Phone OTP sign-in (2 steps: phone → 6-digit code) |
| `/` | Home — greeting + "My Car Washes" cards + quick actions (AuthGuard) |
| `/discover` | Car-wash search — debounced, geolocation-aware tenant directory (AuthGuard) |
| `/carwash/[tenantId]` | Car-wash detail — branches, services, Join / Book / View-rewards CTAs (AuthGuard) |
| `/carwash/[tenantId]/book` | 4-step booking wizard — Vehicle → Services → Branch/Date/Time → Confirm (AuthGuard) |
| `/carwash/[tenantId]/membership` | Loyalty — tier/points, rewards redemption, points history, referral code share (AuthGuard) |
| `/bookings` | My bookings — Upcoming / Past tabs via `?tab=` URL param (AuthGuard) |
| `/bookings/[id]` | Booking detail + **live queue panel** polling `/queue/active` every 10s, paused when tab hidden; cancel action (AuthGuard) |
| `/history` | Cross-tenant service history grouped by month (Manila TZ, sticky headers) (AuthGuard) |
| `/profile` | Profile edit, vehicles list + add/edit/delete, language toggle, sign out (AuthGuard) |
| `/offline` | **Public** — Serwist offline fallback page |

### Connect app UX notes

- Bottom-tab nav sits on all `(tabs)/*` routes; `carwash/*` and `bookings/*` swap it for a back-button AppBar (`src/components/layout/app-bar.tsx`).
- Classified vehicle → exact service prices; unclassified → `PriceMin`/`PriceMax` range with a "final price confirmed on first visit" banner (rule 27).
- Booking wizard uses a reducer with cascading invalidation — vehicle change clears services + slot; branch/date change clears slot.
- Live queue panel on booking detail pauses polling via `visibilitychange` + `refetchIntervalInBackground: false`.
- Elapsed-time ticker renders for `InService` bookings.
- Referral code sharing prefers the Web Share API with a clipboard fallback.
