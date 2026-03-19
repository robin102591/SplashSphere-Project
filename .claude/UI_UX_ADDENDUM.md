# SplashSphere — UI/UX Improvements Addendum

> **Purpose:** This addendum defines the visual design system, component patterns, interaction design, and UX improvements for both the Admin Dashboard and POS applications. Apply these specifications when building frontend pages in Phases 9–11 of PROMPTS.md.

---

## 1. Design Identity — SplashSphere Brand

### Design Philosophy

SplashSphere's visual identity is **clean, aquatic, and professional** — inspired by water, motion, and clarity. The aesthetic sits between "refined SaaS dashboard" and "friendly service business tool." It should feel modern enough for a tech-savvy owner managing multiple branches, yet simple enough for a cashier on a tablet during a busy Saturday.

**Core principles:**
- **Clarity over decoration** — every pixel earns its place
- **Speed over beauty** — POS interactions must feel instant
- **Information density without clutter** — dashboard shows a lot, but nothing feels cramped
- **Philippine-friendly** — peso formatting, Filipino names in examples, local vehicle models

### Color System

Use the SplashSphere aquatic theme consistently across both apps. Both apps share the same Tailwind config and CSS variables, but the admin uses a light-dominant scheme while POS uses a slightly darker, higher-contrast variant for glare resistance.

```css
/* globals.css — CSS custom properties for shadcn/ui integration */
:root {
  /* Brand */
  --splash-50: 204 100% 97%;
  --splash-100: 204 94% 94%;
  --splash-200: 201 94% 86%;
  --splash-300: 199 95% 74%;
  --splash-400: 198 93% 60%;
  --splash-500: 199 89% 48%;   /* Primary brand */
  --splash-600: 200 98% 39%;
  --splash-700: 201 96% 32%;
  --splash-800: 201 90% 27%;
  --splash-900: 202 80% 24%;

  /* Aqua accent */
  --aqua-500: 173 80% 40%;

  /* Semantic */
  --success: 160 84% 39%;
  --warning: 38 92% 50%;
  --error: 0 84% 60%;
  --info: 199 89% 48%;

  /* shadcn mappings */
  --background: 0 0% 100%;
  --foreground: 222 84% 5%;
  --card: 0 0% 100%;
  --card-foreground: 222 84% 5%;
  --primary: 199 89% 48%;
  --primary-foreground: 0 0% 100%;
  --secondary: 220 14% 96%;
  --secondary-foreground: 220 9% 46%;
  --muted: 220 14% 96%;
  --muted-foreground: 220 9% 46%;
  --accent: 173 80% 40%;
  --accent-foreground: 0 0% 100%;
  --destructive: 0 84% 60%;
  --destructive-foreground: 0 0% 100%;
  --border: 220 13% 91%;
  --input: 220 13% 91%;
  --ring: 199 89% 48%;
  --radius: 0.5rem;
}

.dark {
  --background: 222 84% 5%;
  --foreground: 210 40% 98%;
  --card: 217 33% 10%;
  --card-foreground: 210 40% 98%;
  --primary: 199 89% 48%;
  --primary-foreground: 0 0% 100%;
  --border: 217 33% 17%;
  --input: 217 33% 17%;
}
```

### Semantic Color Usage

| Color | Usage |
|---|---|
| `splash-500` (primary blue) | Primary buttons, active nav items, links, focus rings, header accents |
| `aqua-500` (teal) | Secondary actions, commission/money highlights, chart accents |
| `success` (emerald) | Completed transactions, active employees, positive metrics, "Complete" buttons |
| `warning` (amber) | Pending status, low stock alerts, queue CALLED state, payroll OPEN |
| `error` (red) | Cancelled, errors, destructive actions, overdue items, NO_SHOW |
| `info` (blue) | In-progress, informational badges, queue WAITING |
| `purple` (#8b5cf6) | VIP badge, premium services, package highlights |

### Typography

```css
/* Use system font stack for performance — no external font loading on POS */
--font-sans: 'Inter var', 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
--font-mono: 'JetBrains Mono', 'Fira Code', monospace;
```

**Type scale (used across both apps):**
- Page title: `text-2xl font-bold` (admin), `text-xl font-bold` (POS — save space)
- Section heading: `text-lg font-semibold`
- Card title: `text-sm font-medium text-muted-foreground` (label), `text-2xl font-bold` (value)
- Body: `text-sm` (admin), `text-base` (POS — readability)
- Caption/helper: `text-xs text-muted-foreground`
- Money values: `font-mono tabular-nums` — always monospaced for column alignment

### Currency Formatting

Always display Philippine Peso with the ₱ symbol. Use `Intl.NumberFormat`:
```typescript
export const formatPeso = (amount: number) =>
  new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' }).format(amount)
// Output: ₱1,234.56
```

Money values should always use `font-mono tabular-nums` for alignment in tables and summaries.

---

## 2. Admin Dashboard — UI Specifications

### Layout Structure

```
┌─────────────────────────────────────────────────────────────────┐
│ HEADER (h-16, fixed top, z-30)                                  │
│ [☰ Toggle] [🔍 Global Search...              ] [🔔] [👤 User] │
├──────────┬──────────────────────────────────────────────────────┤
│ SIDEBAR  │ MAIN CONTENT (scrollable)                            │
│ (w-64,   │                                                      │
│  fixed,  │ ┌─ Breadcrumb ──────────────────────────────────┐    │
│  z-20)   │ │ Dashboard > Services > Basic Wash             │    │
│          │ └────────────────────────────────────────────────┘    │
│ Logo     │                                                      │
│ ───────  │ ┌─ Page Header ─────────────────────────────────┐    │
│ Dashboard│ │ Basic Wash              [Edit] [Delete]       │    │
│ Branches │ └────────────────────────────────────────────────┘    │
│ Services │                                                      │
│ Packages │ ┌─ Content ─────────────────────────────────────┐    │
│ ──────── │ │                                               │    │
│ Employees│ │  (Page-specific content)                      │    │
│ Payroll  │ │                                               │    │
│ ──────── │ └────────────────────────────────────────────────┘    │
│ Customers│                                                      │
│ Vehicles │                                                      │
│ Merch    │                                                      │
│ ──────── │                                                      │
│ Transact │                                                      │
│ Reports  │                                                      │
│ Settings │                                                      │
└──────────┴──────────────────────────────────────────────────────┘
```

### Sidebar

- **Width:** 256px expanded, 72px collapsed (icon-only)
- **Toggle:** Hamburger button in header. On mobile (<768px), sidebar is a sheet/drawer overlay.
- **Logo:** SplashSphere wordmark when expanded, "SS" icon when collapsed. Top section, `h-16` to align with header.
- **Navigation groups:** Separated by thin dividers with group labels (`text-xs uppercase text-muted-foreground tracking-wider`)
  - **Overview:** Dashboard
  - **Operations:** Branches, Services, Packages, Transactions
  - **People:** Employees, Payroll, Customers, Vehicles
  - **Inventory:** Merchandise
  - **Analytics:** Reports
  - **System:** Settings
- **Active state:** `bg-splash-50 text-splash-700 border-l-2 border-splash-500` (left accent bar)
- **Hover:** `bg-muted/50`
- **Icons:** Lucide icons, 20px, consistent stroke width. Every nav item has an icon.
- **Badge counts:** Show on nav items when relevant — e.g., "Payroll" shows a yellow badge when a period is OPEN and due for closing.

### Header

- **Height:** 64px, `bg-background border-b`
- **Left:** Sidebar toggle (hamburger), then breadcrumb on desktop
- **Center:** Global search input (`w-96 max-w-full`). Searches across transactions (by number), customers (by name/plate), employees (by name). Results in a dropdown with categories.
- **Right:** Notification bell (with unread count badge), Branch switcher dropdown (shows current branch name, dropdown lists all branches + "All Branches"), User profile avatar + name dropdown (custom, NOT Clerk's UserButton) with: user name, email, role, "Sign Out"

### Dashboard Home Page

**KPI Cards Row (4 cards):**
```
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ 📈 Revenue    │ │ 🧾 Transact. │ │ 👥 Active    │ │ 💰 Commission│
│ Today         │ │ Today         │ │ Employees     │ │ This Week    │
│ ₱32,450      │ │ 47            │ │ 12            │ │ ₱8,920      │
│ ▲ 12% vs yes │ │ ▲ 5 vs yes   │ │ 2 branches    │ │ ▲ ₱1,200    │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
```

- Cards use `bg-card rounded-xl border shadow-sm p-6`
- Trend indicator: green arrow + percentage for positive, red for negative
- Icon: splash-500 background circle with white icon

**Charts Row (2 charts, 60/40 split):**
- Left: Revenue trend — 7-day line chart (Recharts `<LineChart>`). Splash-500 line, aqua-500 for commission overlay. Tooltip shows date, revenue, commission.
- Right: Payment methods — pie/donut chart (Recharts `<PieChart>`). Colors: Cash=emerald, GCash=blue, Card=purple, Bank=amber.

**Bottom Row (2 sections, 60/40):**
- Left: Recent transactions table — last 10. Columns: Transaction #, Customer, Vehicle, Services, Amount, Status badge, Time. Click row → navigate to detail.
- Right: Top employees — ranked by commission this week. Avatar, name, commission amount, services count. Show EmployeeType badge (COMMISSION/DAILY).

### Data Tables (Reusable Pattern)

All list pages use the same data table pattern:

**Above table:** 
```
[🔍 Search...          ] [Filter ▼] [Filter ▼]  [+ Create New]
```

- Search: debounced 300ms, searches server-side
- Filters: shadcn Select dropdowns, each fires a query parameter change
- Create button: `bg-primary text-primary-foreground` with plus icon

**Table styling:**
- Header: `bg-muted/50 text-xs uppercase tracking-wider text-muted-foreground font-medium`
- Rows: `hover:bg-muted/30 transition-colors`, alternate row coloring via `even:bg-muted/10`
- Borders: horizontal only (`border-b`), no vertical borders
- Row click: navigates to detail page (entire row is clickable, cursor-pointer)
- Status column: colored badges using shadcn Badge variants
- Money columns: right-aligned, `font-mono tabular-nums`
- Action column: icon buttons (edit, delete) in a dropdown menu `<DropdownMenu>`

**Below table:**
```
Showing 1-20 of 147 results          [< Previous] [1] [2] [3] [Next >]
```

- Pagination: shadcn Pagination component
- Page size selector: 10, 20, 50

### Status Badges

Consistent badge styling across the entire admin:

| Status | Style |
|---|---|
| PENDING | `bg-amber-100 text-amber-800 border-amber-200` |
| IN_PROGRESS | `bg-blue-100 text-blue-800 border-blue-200` |
| COMPLETED | `bg-emerald-100 text-emerald-800 border-emerald-200` |
| CANCELLED | `bg-red-100 text-red-800 border-red-200` |
| REFUNDED | `bg-gray-100 text-gray-800 border-gray-200` |
| OPEN (payroll) | `bg-amber-100 text-amber-800` |
| CLOSED (payroll) | `bg-blue-100 text-blue-800` |
| PROCESSED (payroll) | `bg-emerald-100 text-emerald-800` |
| ACTIVE | `bg-emerald-100 text-emerald-800` |
| INACTIVE | `bg-gray-100 text-gray-500` |
| COMMISSION (employee type) | `bg-purple-100 text-purple-800` |
| DAILY (employee type) | `bg-sky-100 text-sky-800` |
| VIP (queue) | `bg-purple-100 text-purple-800` |
| LOW STOCK | `bg-red-100 text-red-800` with pulse animation |

### Form Pages

All create/edit pages follow this layout:

```
┌─ Page Header ──────────────────────────────────────────────┐
│ ← Back to List    Create New Service          [Cancel] [Save] │
└────────────────────────────────────────────────────────────────┘

┌─ Form Card ────────────────────────────────────────────────┐
│ Section Label (text-sm font-medium text-muted-foreground) │
│                                                            │
│ [Label]                    [Label]                        │
│ [Input ________________]  [Input ________________]       │
│                                                            │
│ [Label]                    [Label]                        │
│ [Select ▼_______________] [Input ________________]       │
│                                                            │
│ [Label]                                                   │
│ [Textarea ___________________________________________]    │
└────────────────────────────────────────────────────────────┘
```

- Forms use `max-w-2xl mx-auto` for readability (not full-width)
- Two-column grid on desktop (`grid grid-cols-2 gap-4`), single column on mobile
- Labels always above inputs, never inline
- Required fields marked with red asterisk
- Validation errors shown below input in red text
- Save button: loading state with spinner, disabled during submission
- Cancel: navigates back with confirmation if form is dirty
- Success: toast notification + redirect to list or detail page

### Pricing Matrix Editor

The most complex UI component. Used for service pricing and commission configuration.

```
┌─ Pricing Matrix ───────────────────────────────────────────┐
│                                                            │
│              Small      Medium     Large      XL          │
│ ┌──────────┬──────────┬──────────┬──────────┬──────────┐  │
│ │          │          │          │          │          │  │
│ │ Sedan    │ [₱120  ] │ [₱150  ] │ [₱180  ] │ [  —   ] │  │
│ │          │          │          │          │          │  │
│ ├──────────┼──────────┼──────────┼──────────┼──────────┤  │
│ │          │          │          │          │          │  │
│ │ SUV      │ [₱180  ] │ [₱220  ] │ [₱280  ] │ [₱350  ] │  │
│ │          │          │          │          │          │  │
│ ├──────────┼──────────┼──────────┼──────────┼──────────┤  │
│ │ Van      │ [₱200  ] │ [₱250  ] │ [₱320  ] │ [₱400  ] │  │
│ ├──────────┼──────────┼──────────┼──────────┼──────────┤  │
│ │ Truck    │ [₱220  ] │ [₱280  ] │ [₱350  ] │ [₱450  ] │  │
│ ├──────────┼──────────┼──────────┼──────────┼──────────┤  │
│ │ Motor    │ [₱80   ] │ [  —   ] │ [  —   ] │ [  —   ] │  │
│ └──────────┴──────────┴──────────┴──────────┴──────────┘  │
│                                                            │
│ Base Price: ₱150 (fallback when matrix cell is empty)     │
│                                     [Reset All] [Save Matrix] │
└────────────────────────────────────────────────────────────┘
```

- Empty cells show "—" and are treated as "use base price"
- Cells with values different from base price get a subtle `bg-splash-50` highlight
- Number inputs: right-aligned, `w-20`, step=10
- Row header (vehicle type): `font-medium bg-muted/30`
- Column header (size): `text-center font-medium bg-muted/30`
- Save button sends the entire matrix as a bulk upsert
- Dirty state indicator: show "Unsaved changes" warning with dot

### Detail Pages with Tabs

Service detail, employee detail, package detail, etc. all use this pattern:

```
┌─ Page Header ──────────────────────────────────────────────┐
│ ← Back    Basic Wash    [Active ●]          [Edit] [Delete] │
└────────────────────────────────────────────────────────────────┘

┌─ Tabs ─────────────────────────────────────────────────────┐
│ [Details] [Pricing Matrix] [Commission Matrix]             │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  (Tab content)                                             │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

- Use shadcn `<Tabs>` component
- Tab underline style (not boxed): active tab has `border-b-2 border-primary`
- Tab content has `pt-6` padding

### Loading States

- **Page load:** Full-page skeleton using shadcn `<Skeleton>`. Match the layout of the actual page.
- **Table load:** Skeleton rows (5 rows with pulsing rectangles matching column widths)
- **Card load:** Skeleton rectangle matching card dimensions
- **Button submit:** Replace text with spinner (`<Loader2 className="animate-spin" />`), disable button
- **Optimistic updates:** For toggles (active/inactive), update UI immediately, revert on error

### Empty States

When a list has no data, show a centered illustration with:
```
┌─────────────────────────────────────────────┐
│                                             │
│          (illustration or icon)             │
│                                             │
│        No services found                    │
│   Create your first service to get started  │
│                                             │
│           [+ Create Service]                │
│                                             │
└─────────────────────────────────────────────┘
```

Use Lucide icons at 48px with `text-muted-foreground/30` for the illustration.

### Toast Notifications

Use shadcn `<Toaster>` from sonner. Position: bottom-right. Variants:
- Success: green left border, check icon
- Error: red left border, X icon
- Warning: amber left border, alert icon
- Duration: 4 seconds, dismissible

### Responsive Breakpoints

| Breakpoint | Behavior |
|---|---|
| `<768px` (mobile) | Sidebar hidden (sheet overlay), single-column forms, stacked KPI cards, table scrolls horizontally |
| `768-1024px` (tablet) | Sidebar collapsed (icons only), 2-column KPI cards, tables show essential columns |
| `>1024px` (desktop) | Full sidebar, 4-column KPI cards, full tables |

---

## 3. POS Application — UI Specifications

### Design Differences from Admin

The POS app shares the same color system but has critical differences:

| Aspect | Admin | POS |
|---|---|---|
| Font size | `text-sm` body | `text-base` body (larger for readability) |
| Touch targets | 40px minimum | **56px minimum** |
| Information density | High (tables, charts) | Low (big buttons, clear totals) |
| Navigation | Full sidebar | Bottom nav bar (mobile) or slim sidebar (tablet/desktop) |
| Animations | Subtle transitions | Minimal (speed over beauty) |
| Color contrast | Standard | Higher (may be used in bright/glare conditions) |
| Layout | Flexible | **Fixed panels** (no scroll on main transaction screen) |

### POS Layout

**Desktop/Tablet (≥768px):**
```
┌─ Top Bar (h-14) ─────────────────────────────────────────────┐
│ [🏪 Makati Branch] [👤 Ana Reyes, Cashier] [⏰ 2:34 PM]  │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─ Nav Pills ────────────────────────────────────────────┐  │
│  │ [🛒 New Transaction] [📋 Queue] [📜 History] [⏱ Att] │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  (Page content fills remaining space, NO scroll on tx page)  │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

- Top bar: compact, shows essential info only
- Navigation: horizontal pill buttons (not a sidebar) — POS doesn't need deep navigation
- No breadcrumbs — POS is flat, not deep

**Mobile (<768px):**
```
┌─ Top Bar ──────────────────────┐
│ 🏪 Makati   👤 Ana    ⏰ 2:34 │
├────────────────────────────────┤
│                                │
│  (Full-screen page content)    │
│                                │
├────────────────────────────────┤
│ [🛒] [📋] [📜] [⏱]          │
│  New  Queue  Hist  Att         │
└────────────────────────────────┘
```

Bottom tab bar on mobile, like a native app.

### POS Transaction Screen — Detailed Layout

**Two-panel layout (NO scroll, content fits viewport):**

```
┌──────────────────────────────┬───────────────────────────────┐
│ LEFT PANEL (55%)             │ RIGHT PANEL (45%)             │
│                              │                               │
│ ┌──────────────────────────┐ │ ┌─ ORDER SUMMARY ───────────┐ │
│ │ 🔍 ABC-1234       [Go]  │ │ │                           │ │
│ │ Toyota Fortuner          │ │ │ Basic Wash (SUV/L)  ₱280  │ │
│ │ SUV • Large • Juan DC    │ │ │ Interior Vac.       ₱200  │ │
│ └──────────────────────────┘ │ │ Air Fresh. (×2)     ₱120  │ │
│                              │ │ ─────────────────────────  │ │
│ ┌─ SERVICES ───────────────┐ │ │ Subtotal          ₱600    │ │
│ │┌──────┐┌──────┐┌──────┐ │ │ │ Discount            -₱0   │ │
│ ││Basic ││Prem. ││Wax & │ │ │ │                           │ │
│ ││Wash  ││Wash  ││Polish│ │ │ │ ══════════════════════════ │ │
│ ││ ₱280 ││ ₱420 ││ ₱650 │ │ │ │ TOTAL             ₱600   │ │
│ │└──────┘└──────┘└──────┘ │ │ │ Commission         ₱120   │ │
│ │┌──────┐┌──────┐┌──────┐ │ │ └───────────────────────────┘ │
│ ││Under ││Inter.││Full  │ │ │                               │
│ ││wash  ││Vac   ││Inter.│ │ │ ┌─ EMPLOYEES ───────────────┐ │
│ ││ ₱250 ││ ₱200 ││ ₱500 │ │ │ │ ☑ Juan    ☑ Pedro        │ │
│ │└──────┘└──────┘└──────┘ │ │ │ ☑ Maria   ☐ Jose          │ │
│ └──────────────────────────┘ │ │ 3 assigned • ₱40 each     │ │
│                              │ └───────────────────────────┘ │
│ ┌─ PACKAGES ───────────────┐ │                               │
│ │┌──────────┐┌──────────┐  │ │ ┌─ PAYMENT ─────────────────┐ │
│ ││Complete  ││Premium   │  │ │ │                           │ │
│ ││Care ₱750 ││Pack ₱900 │  │ │ │ [💵 Cash] [📱GCash][💳] │ │
│ │└──────────┘└──────────┘  │ │ │                           │ │
│ └──────────────────────────┘ │ │ Remaining: ₱600           │ │
│                              │ │ [₱600              ]      │ │
│ ┌─ MERCHANDISE ────────────┐ │ │                           │ │
│ │ Air Freshener  ₱60 [+]  │ │ │ ┌─────────────────────┐   │ │
│ │ Towel          ₱120 [+] │ │ │ │  ✓ COMPLETE (₱600)  │   │ │
│ │ Dash Cleaner   ₱180 [+] │ │ │ └─────────────────────┘   │ │
│ └──────────────────────────┘ │ └───────────────────────────┘ │
└──────────────────────────────┴───────────────────────────────┘
```

### Service Buttons

- Grid: 3 columns on desktop, 2 on tablet
- Each button: `min-h-[72px]`, rounded-xl, border
- Content: service name (top, font-medium), price (bottom, font-mono, text-lg)
- **Unselected:** `bg-card border-border hover:border-splash-300 hover:bg-splash-50`
- **Selected:** `bg-splash-50 border-splash-500 ring-2 ring-splash-500/20` with a checkmark badge
- **Tapping a selected service removes it** (toggle behavior)
- When vehicle is set: show the **matrix price** (not base price). If no matrix price, show base price with a "base" label

### Employee Picker

- Checkbox grid: 2 columns
- Each item: `min-h-[48px]` with employee name and type badge
- Show commission-per-employee below the grid: "3 assigned • ₱40.00 each"
- Only show COMMISSION-type employees (DAILY employees don't do services)
- Pre-select employees who are clocked in today

### Payment Section

- Method buttons: large, toggle style (only one active at a time for each payment line)
- Cash button: green tint. GCash: blue. Card: purple.
- Amount input: large `text-2xl font-mono`, auto-filled with remaining balance
- For split payment: "+ Add Payment" link below, creates another payment line
- Show running total of payments vs. remaining

### Complete Button

- Full width, `min-h-[56px]`, `bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-lg`
- **Disabled states** (with tooltip explaining why):
  - No services/packages selected → "Add at least one service"
  - No employees assigned → "Assign employees"
  - Payment doesn't cover total → "Payment insufficient"
- When clicked: brief loading spinner, then show receipt dialog
- On success: toast "Transaction completed!", reset the form, optionally print receipt

### Receipt Dialog

After completing a transaction, show a dialog with the receipt:

```
┌─ Receipt ──────────────────────────────┐
│                                        │
│       ✦ SparkleWash Philippines ✦      │
│         SparkleWash - Makati           │
│      123 Ayala Ave, Makati City        │
│                                        │
│  Transaction: MNL01-20260318-0042      │
│  Date: Mar 18, 2026 2:34 PM           │
│  Cashier: Ana Reyes                    │
│  ────────────────────────────────────  │
│  Customer: Juan Dela Cruz              │
│  Vehicle: ABC-1234 Toyota Fortuner     │
│  ────────────────────────────────────  │
│  Basic Wash (SUV/Large)     ₱280.00   │
│  Interior Vacuum            ₱200.00   │
│  Air Freshener (×2)         ₱120.00   │
│  ────────────────────────────────────  │
│  Subtotal                   ₱600.00   │
│  Discount                     -₱0.00  │
│  TOTAL                      ₱600.00   │
│  ────────────────────────────────────  │
│  Paid (Cash)                ₱600.00   │
│  ────────────────────────────────────  │
│  Employees: Juan, Pedro, Maria         │
│                                        │
│       Thank you for your visit!        │
│                                        │
│     [🖨 Print]  [📱 New Transaction]   │
└────────────────────────────────────────┘
```

- Print button: triggers `window.print()` with `@media print` CSS that hides everything except the receipt
- "New Transaction" button: closes dialog, resets form

### Queue Board (Kanban)

```
┌─ Queue Board ──────────────────────────────────────────────────┐
│ [+ Add to Queue]    Waiting: 5 | Avg Wait: 12 min | Served: 23│
├────────────────────┬──────────────────┬────────────────────────┤
│ 📋 WAITING (5)     │ 📢 CALLED (1)   │ 🔧 IN SERVICE (3)     │
├────────────────────┼──────────────────┼────────────────────────┤
│ ┌────────────────┐ │ ┌──────────────┐ │ ┌────────────────────┐ │
│ │ Q-006          │ │ │ Q-003  ⚡VIP │ │ │ Q-001              │ │
│ │ ABC-1234       │ │ │ XYZ-5678     │ │ │ DEF-9012           │ │
│ │ SUV • Fortuner │ │ │ Sedan • Vios │ │ │ Van • Innova       │ │
│ │ Basic, Wax     │ │ │ Premium Wash │ │ │ Full Detail        │ │
│ │ ~18 min wait   │ │ │              │ │ │ 23 min elapsed     │ │
│ │         [Call] │ │ │ ⏱ 3:42 left │ │ │       [View Tx]    │ │
│ └────────────────┘ │ │              │ │ └────────────────────┘ │
│ ┌────────────────┐ │ │ [Start] [NS] │ │ ┌────────────────────┐ │
│ │ Q-007          │ │ └──────────────┘ │ │ Q-002              │ │
│ │ GHI-3456       │ │                  │ │ JKL-7890           │ │
│ │ Sedan • City   │ │                  │ │ SUV • CR-V         │ │
│ │ Basic Wash     │ │                  │ │ Basic + Interior   │ │
│ │ ~24 min wait   │ │                  │ │ 11 min elapsed     │ │
│ │         [Call] │ │                  │ │       [View Tx]    │ │
│ └────────────────┘ │                  │ └────────────────────┘ │
└────────────────────┴──────────────────┴────────────────────────┘
```

- Three scrollable columns
- Cards: `rounded-lg border p-4 bg-card`
- CALLED column: cards have `border-warning bg-warning/5` with subtle pulse
- VIP badge: purple `<Badge>`
- Countdown timer in CALLED cards: shows remaining time from 5 minutes, turns red under 1 minute
- Elapsed timer in IN_SERVICE cards: `text-muted-foreground`
- [NS] = No-Show button, red text
- Cards are NOT draggable — use action buttons only (touch reliability)

### Public Queue Display (Wall TV)

Full-screen, no chrome, auto-scales to any resolution:

```
┌──────────────────────────────────────────────────────────────┐
│  💧 SparkleWash - Makati                       ⏰ 2:34 PM   │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Queue #    Vehicle         Status          Est. Wait        │
│  ─────────────────────────────────────────────────────────   │
│  Q-003      XY*-****8       🟡 CALLED        NOW            │
│  Q-001      DE*-****2       🟢 IN SERVICE     —             │
│  Q-002      JK*-****0       🟢 IN SERVICE     —             │
│  Q-004      MN*-****5       🟢 IN SERVICE     —             │
│  Q-006      AB*-****4       ⬜ WAITING        ~18 min       │
│  Q-007      GH*-****6       ⬜ WAITING        ~24 min       │
│  Q-008      PQ*-****1       ⬜ WAITING        ~30 min       │
│                                                              │
│                                                              │
│  Today: 23 served | Average wait: 12 min                     │
└──────────────────────────────────────────────────────────────┘
```

- Dark background (`bg-gray-950 text-white`) for TV readability
- Font: `text-2xl` for table rows, `text-4xl` for header
- CALLED row: yellow background flash animation (CSS keyframes, 2s interval)
- Plate numbers masked for privacy: show first 2 chars + last char only
- Auto-scrolls if >8 entries (marquee-style, slow)
- No user interaction — pure display
- Reconnection: if SignalR disconnects, show "Reconnecting..." banner

---

## 4. Shared Component Library

### Components to Build in `components/ui/` (beyond shadcn defaults)

| Component | Description |
|---|---|
| `<StatusBadge status={} />` | Maps any status enum to the correct badge color |
| `<MoneyDisplay amount={} />` | Formatted ₱ with `font-mono tabular-nums` |
| `<DataTable />` | Wrapper around TanStack Table with search, filters, pagination built in |
| `<PricingMatrixEditor />` | The vehicle type × size editable grid |
| `<CommissionMatrixEditor />` | Same grid but with commission type dropdown per cell |
| `<PageHeader title back? actions? />` | Consistent page header with back button and action buttons |
| `<EmptyState icon title description action? />` | Empty list illustration |
| `<ConfirmDialog title description onConfirm />` | Reusable confirmation with destructive variant |
| `<BranchSwitcher />` | Dropdown to switch active branch context |
| `<UserProfileMenu />` | Custom profile dropdown (replaces Clerk UserButton) |
| `<ConnectionStatus />` | SignalR connection indicator (green/yellow/red dot) |
| `<QueueCard entry onAction />` | Queue entry card for the Kanban board |
| `<CountdownTimer target />` | Live countdown for queue no-show timer |
| `<ElapsedTimer start />` | Live elapsed time for in-service entries |
| `<StatCard title value trend? icon? />` | Dashboard KPI card |
| `<ReceiptView transaction />` | Printable receipt layout |

---

## 5. Animation & Micro-interactions

### Admin Dashboard
- **Page transitions:** None (instant, server-rendered)
- **Table row hover:** `transition-colors duration-150`
- **Card hover:** `transition-shadow duration-200 hover:shadow-md`
- **Status toggle:** Optimistic UI with 200ms color transition
- **Chart load:** Recharts default line draw animation (1s)
- **Toast appear:** Slide in from right, 300ms

### POS Application
- **Service button tap:** Immediate visual feedback (no delay). Scale down to 0.97 on press (`active:scale-[0.97] transition-transform duration-75`)
- **Order summary item add:** Slide in from left, 150ms
- **Order summary item remove:** Fade out + height collapse, 200ms
- **Complete button:** Pulse animation when enabled and total > 0
- **Queue card CALLED state:** Subtle border pulse (`animate-pulse` on border color)
- **Queue display CALLED row:** Full row background flash (yellow → transparent, 2s loop)
- **Avoid:** No spring animations, no bouncing, no parallax. Speed is king on POS.

---

## 6. Accessibility Requirements

- All interactive elements have visible focus rings (`ring-2 ring-ring ring-offset-2`)
- Color is never the only indicator — status badges include text labels, not just colors
- Form inputs have associated labels (not placeholder-only)
- Tables have proper `<th scope>` attributes
- Touch targets minimum 48px (admin) / 56px (POS)
- Contrast ratios: minimum 4.5:1 for body text, 3:1 for large text
- Keyboard navigation: Tab through all interactive elements, Enter to activate
- Screen reader: `aria-label` on icon-only buttons, `aria-live` on real-time updates

---

## 7. Dark Mode (Admin Only)

The admin dashboard supports dark mode (system preference or manual toggle). The POS does NOT — it uses a fixed light theme optimized for bright car wash environments.

Dark mode simply swaps the CSS custom properties in `:root` vs `.dark` (defined in globals.css above). All shadcn components automatically adapt. Charts use lighter line colors on dark backgrounds.

Toggle: Sun/moon icon button in the header, stores preference in localStorage.

---

## 8. Mobile Responsiveness — Key Breakpoint Behaviors

### Admin on Mobile (<768px)
- Sidebar becomes a slide-in sheet (hamburger menu)
- Dashboard KPI cards: 2×2 grid instead of 4-column
- Charts: stacked vertically, full width
- Tables: horizontal scroll with sticky first column
- Forms: single column
- Matrix editors: horizontal scroll with sticky vehicle type column

### POS on Tablet (768-1024px)
- Transaction screen: two panels side by side (55/45)
- Service grid: 2 columns
- Queue board: three columns with horizontal scroll

### POS on Mobile (<768px)
- Transaction screen: **single panel with tab switching** between:
  - Tab 1: Vehicle + Services selection
  - Tab 2: Order summary + employees + payment
- Service grid: 2 columns, full width
- Queue board: single column, swipe between WAITING/CALLED/IN_SERVICE
- Bottom tab navigation (native app feel)

---

## 9. Prompt Modifications for UI/UX

When executing Phases 9-11 of PROMPTS.md, apply these additional instructions to the relevant prompts:

**Prompt 9.2 (Admin App Scaffold):** Include the globals.css with the full color system defined above. Set up dark mode toggle in the header. Build the sidebar with grouped navigation, collapsible behavior, and badges. Build the header with global search, notification bell, branch switcher, and custom user profile menu.

**Prompt 9.3 (POS App Scaffold):** Use the POS-specific layout (top bar + pill nav, not sidebar). Set up the bottom tab bar for mobile. Apply larger font sizes and touch targets. NO dark mode on POS.

**Prompt 10.1-10.7 (Admin Pages):** Use the DataTable pattern with search/filters/pagination defined above. Use the form page pattern for all create/edit pages. Use the status badge mapping consistently. Build all empty states. Add skeleton loading for every page.

**Prompt 11.0 (Queue Board):** Follow the Kanban layout defined above with countdown/elapsed timers. Build the public queue display with dark theme and auto-refresh.

**Prompt 11.1 (POS Transaction):** Follow the two-panel layout exactly. Build service buttons as toggle grid with matrix prices. Build the employee picker with commission-per-employee calculation. Build split payment support. Build the receipt dialog with print functionality.
