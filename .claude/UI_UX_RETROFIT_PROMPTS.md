# SplashSphere — UI/UX Retrofit Prompts

> **Context:** Phases 9-11 are already built. These prompts apply the UI/UX improvements
> from UI_UX_ADDENDUM.md on top of the existing codebase. Run them in order.
> Each prompt is a targeted refactor — not a rebuild.

---

## RETROFIT 1: Design System Foundation

```
Read the UI_UX_ADDENDUM.md file in the project root. Apply the design system foundation 
to BOTH apps (admin and pos):

1. Update globals.css in both apps with the complete CSS custom properties defined in 
   Section 1 of UI_UX_ADDENDUM.md — the full splash/aqua color system, semantic colors, 
   and shadcn variable mappings for both :root and .dark.

2. Create a shared utility file lib/format.ts in both apps with:
   - formatPeso(amount: number) using Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' })
   - formatDate, formatTime, formatDateTime helpers using date-fns with Asia/Manila timezone

3. Add the font-mono tabular-nums utility class combination as a Tailwind @utility 
   called "money" so we can use className="money" for all currency values.

4. Audit every hardcoded color in both apps. Replace any raw Tailwind colors 
   (blue-500, green-500, etc.) with the semantic design tokens:
   - blue-500 → splash-500 (use hsl(var(--splash-500)) or the primary alias)
   - green/emerald → success semantic
   - red → destructive/error semantic
   - amber/yellow → warning semantic
   
   Do NOT change shadcn component internals — only our custom code.

5. Replace every raw ₱ string formatting with the formatPeso() helper.
   Search for patterns like: `₱${amount}`, `₱` + amount, toFixed(2), etc.

Only touch styling and formatting — do not change any component logic or data flow.
```

---

## RETROFIT 2: Status Badges Consistency

```
Create a reusable StatusBadge component and apply it everywhere.

1. Create components/ui/status-badge.tsx in both apps (or in a shared location):

   interface StatusBadgeProps {
     status: string
     type?: 'transaction' | 'payroll' | 'employee' | 'queue' | 'stock'
   }

   The component maps status strings to the exact badge styles from UI_UX_ADDENDUM.md:
   - PENDING → bg-amber-100 text-amber-800 border-amber-200
   - IN_PROGRESS → bg-blue-100 text-blue-800 border-blue-200
   - COMPLETED → bg-emerald-100 text-emerald-800 border-emerald-200
   - CANCELLED → bg-red-100 text-red-800 border-red-200
   - REFUNDED → bg-gray-100 text-gray-800 border-gray-200
   - OPEN → bg-amber-100 text-amber-800
   - CLOSED → bg-blue-100 text-blue-800
   - PROCESSED → bg-emerald-100 text-emerald-800
   - ACTIVE → bg-emerald-100 text-emerald-800
   - INACTIVE → bg-gray-100 text-gray-500
   - COMMISSION → bg-purple-100 text-purple-800
   - DAILY → bg-sky-100 text-sky-800
   - WAITING → bg-blue-100 text-blue-800
   - CALLED → bg-amber-100 text-amber-800
   - IN_SERVICE → bg-emerald-100 text-emerald-800
   - NO_SHOW → bg-red-100 text-red-800
   - VIP → bg-purple-100 text-purple-800
   - LOW_STOCK → bg-red-100 text-red-800 (add animate-pulse on the dot)

2. Find every place in both apps where status is displayed — data tables, detail pages, 
   badges, cards — and replace with <StatusBadge status={item.status} />.

3. Remove all inline badge color logic that was scattered across pages.
```

---

## RETROFIT 3: Shared UI Components

```
Create these reusable components referenced in UI_UX_ADDENDUM.md Section 4. 
Place them in the admin app's components/ui/ (or a shared package if you prefer):

1. components/ui/money-display.tsx:
   - <MoneyDisplay amount={1234.56} /> renders "₱1,234.56" with font-mono tabular-nums
   - Optional size prop: "sm" | "md" | "lg" | "xl" controlling text size
   - Optional trend prop: { value: number, label: string } showing ▲/▼ with color

2. components/ui/stat-card.tsx:
   - <StatCard title="Revenue Today" value="₱32,450" icon={TrendingUp} trend={{ value: 12, label: "vs yesterday" }} />
   - Card with: icon in a colored circle (top-left), title (muted, small), 
     value (large, bold), trend line (green up / red down + percentage)
   - Matches the KPI card design from UI_UX_ADDENDUM.md Section 2

3. components/ui/page-header.tsx:
   - <PageHeader title="Basic Wash" back="/services" actions={<Button>Edit</Button>} badge={<StatusBadge status="ACTIVE" />} />
   - Consistent header with optional back arrow, title, status badge, and action buttons

4. components/ui/empty-state.tsx:
   - <EmptyState icon={Package} title="No services found" description="Create your first service" action={{ label: "Create Service", href: "/services/new" }} />
   - Centered layout with large muted icon, title, description, action button

5. components/ui/confirm-dialog.tsx:
   - <ConfirmDialog title="Close Payroll?" description="This will lock all entries." onConfirm={handleClose} variant="warning" />
   - Uses shadcn AlertDialog under the hood
   - variant: "danger" (red confirm button) | "warning" (amber) | "default" (primary)

6. components/ui/connection-status.tsx:
   - Small dot indicator: green (connected), yellow (reconnecting), red (disconnected)
   - Reads from the SignalR hook connection state
   - Place in the header of both apps

Now go through every existing page and replace inline implementations with these 
components where applicable:
- Replace manual KPI card markup on dashboard with <StatCard>
- Replace manual page headers with <PageHeader>
- Add <EmptyState> to all list pages when data is empty
- Replace all confirmation window.confirm() calls with <ConfirmDialog>
```

---

## RETROFIT 4: Admin Sidebar & Header Polish

```
Refactor the admin sidebar and header to match UI_UX_ADDENDUM.md Section 2:

SIDEBAR:
1. Width: 256px expanded, 72px collapsed (icon-only with tooltips).
   - Store collapsed state in localStorage so it persists across page loads.
   - On mobile (<768px): sidebar becomes a sheet/drawer overlay triggered by hamburger.

2. Navigation groups with dividers and group labels:
   - Overview: Dashboard
   - Operations: Branches, Services, Packages, Transactions
   - People: Employees, Payroll, Customers, Vehicles
   - Inventory: Merchandise
   - Analytics: Reports
   - System: Settings
   Group labels: text-xs uppercase text-muted-foreground tracking-wider, only visible when expanded.

3. Active state: bg-splash-50 text-splash-700 with a left border accent (border-l-2 border-splash-500).
   Hover: bg-muted/50.

4. Every nav item must have a Lucide icon. When collapsed, show icon-only with a tooltip 
   for the label.

5. Logo area: SplashSphere wordmark when expanded, "SS" or a water drop icon when collapsed.
   Height matches header (h-16).

HEADER:
1. Global search: w-96 max-w-full input in the center. On mobile, show a search icon 
   that expands to full-width input.

2. Branch switcher: dropdown showing current branch name with a chevron. Lists all 
   branches + "All Branches" option. Changing branch filters all data on the current page.

3. User profile: Replace Clerk's <UserButton> (if used) with a custom dropdown:
   - Shows avatar + name (truncated if long)
   - Dropdown: user name, email, role badge, divider, "Sign Out" button
   - Sign out calls clerk.signOut()

4. Dark mode toggle: Sun/Moon icon button, stores preference in localStorage, 
   toggles "dark" class on <html>.

5. Notification bell: icon button with unread count badge (even if notifications 
   aren't fully implemented yet, wire the UI).
```

---

## RETROFIT 5: Admin Data Tables Polish

```
Audit all data table pages in the admin app and ensure they follow the pattern 
from UI_UX_ADDENDUM.md Section 2:

1. Above every table, ensure this toolbar layout:
   [Search input (debounced)] [Filter dropdowns] ........... [+ Create button]
   - Search: left-aligned, placeholder "Search {resource}...", debounced 300ms
   - Filters: shadcn Select components, relevant to the page
   - Create button: right-aligned, primary color, with plus icon

2. Table header row: bg-muted/50, text-xs uppercase tracking-wider text-muted-foreground
   
3. Table rows:
   - hover:bg-muted/30 transition-colors
   - Entire row clickable (navigates to detail) — add cursor-pointer
   - Status columns use <StatusBadge>
   - Money columns: right-aligned, use <MoneyDisplay>
   - Action column: three-dot menu using shadcn DropdownMenu (not inline buttons)

4. Below table: "Showing 1-20 of X results" left-aligned, pagination right-aligned.

5. Empty state: when no results, show <EmptyState> instead of empty table.

6. Loading state: replace any loading spinners with skeleton rows 
   (5 rows of <Skeleton> matching column widths).

Apply these changes to: branches, services, packages, employees, payroll, customers, 
vehicles, merchandise, transactions list pages.
```

---

## RETROFIT 6: Admin Forms & Detail Pages Polish

```
Audit all form pages (create/edit) and detail pages:

FORMS:
1. All form pages: max-w-2xl mx-auto (not full-width sprawl)
2. Two-column grid on desktop (grid grid-cols-2 gap-4), single on mobile
3. Labels always above inputs, required fields marked with red asterisk
4. Validation errors: red text below the input, not as toasts
5. Submit button: show loading spinner during submission, disable the button
6. Cancel button: if form is dirty, show <ConfirmDialog> before navigating away
7. Success: toast notification (bottom-right, shadcn sonner) + redirect

DETAIL PAGES:
1. Use <PageHeader> component with back button, title, status badge, action buttons
2. Tabs use shadcn <Tabs> with underline style (border-b-2 on active tab, not boxed)
3. Tab content has pt-6 top padding

MATRIX EDITORS (services/[id] and packages/[id]):
1. If not already built as a dedicated component, extract the pricing matrix and 
   commission matrix into:
   - components/forms/pricing-matrix-editor.tsx
   - components/forms/commission-matrix-editor.tsx
2. Apply the visual spec from UI_UX_ADDENDUM.md:
   - Row headers (vehicle type): font-medium bg-muted/30
   - Column headers (size): text-center font-medium bg-muted/30
   - Cells with values different from base price: bg-splash-50 highlight
   - Empty cells show "—"
   - Number inputs: right-aligned, w-20
   - "Unsaved changes" dot indicator when dirty
3. Ensure these are reused identically on both service and package detail pages.
```

---

## RETROFIT 7: POS Layout & Touch Optimization

```
Refactor the POS app layout and sizing to match UI_UX_ADDENDUM.md Section 3:

1. POS LAYOUT:
   - Replace any sidebar with a horizontal pill navigation bar below the top bar
   - Top bar (h-14): [Branch name] [Cashier name] [Clock showing current time]
   - Nav pills: [New Transaction] [Queue] [History] [Attendance] — large, tappable
   - On mobile (<768px): convert nav pills to a bottom tab bar (fixed bottom, h-16)

2. TOUCH TARGETS:
   - Audit every button, link, and interactive element in the POS app
   - Ensure minimum height of 56px (not 48px — POS needs bigger targets)
   - Ensure minimum tap area of 56×56px even if the visual element is smaller
   - Service selection buttons: min-h-[72px]
   - Complete Transaction button: min-h-[56px], full width

3. FONT SIZES:
   - Body text: text-base (not text-sm)
   - Headings: text-xl (not text-2xl — save space)
   - Money totals on transaction screen: text-2xl font-bold font-mono
   - Service button prices: text-lg font-mono

4. Remove any decorative animations from POS. Keep only:
   - active:scale-[0.97] on buttons (tactile press feedback)
   - 150ms transitions on color changes
   
5. Ensure the transaction screen panels are FIXED (no scroll on the main view).
   If content overflows, individual panels (service list, order summary) scroll internally.
```

---

## RETROFIT 8: POS Transaction Screen Polish

```
Refactor the POS transaction screen (/transactions/new) to match the wireframe 
in UI_UX_ADDENDUM.md Section 3:

1. SERVICE BUTTONS:
   - Grid: 3 columns desktop, 2 tablet
   - Each button: min-h-[72px], rounded-xl, border
   - Show service name (font-medium) and MATRIX PRICE for current vehicle (font-mono text-lg)
   - Unselected: bg-card border-border hover:border-splash-300
   - Selected: bg-splash-50 border-splash-500 ring-2 ring-splash-500/20 with checkmark
   - Tapping selected service REMOVES it (toggle behavior)

2. EMPLOYEE PICKER:
   - Checkbox grid, 2 columns
   - Show only COMMISSION-type employees who are clocked in today
   - Below the grid: "3 assigned • ₱40.00 each" commission split preview
   - Update this text in real-time as employees are toggled

3. PAYMENT SECTION:
   - Large method buttons: Cash (emerald tint), GCash (blue), Card (purple)
   - Amount input: text-2xl font-mono, auto-fills with remaining balance
   - "+ Add Payment" for split payments
   - Show remaining balance that updates as payments are added

4. COMPLETE BUTTON:
   - Full width, min-h-[56px], bg-emerald-600 text-white font-bold text-lg
   - Disabled with tooltip when: no services, no employees, insufficient payment
   - Show spinner during submission

5. RECEIPT DIALOG:
   - After completion, show the receipt in a shadcn Dialog
   - Receipt content matches the format in UI_UX_ADDENDUM.md
   - [Print] button triggers window.print() with @media print CSS
   - [New Transaction] button resets the form
   - Add @media print styles that hide everything except the receipt dialog content

6. On mobile (<768px): convert to single-panel with TWO TABS:
   - Tab 1: Vehicle + Services
   - Tab 2: Summary + Employees + Payment
```

---

## RETROFIT 9: Queue Board & Display Polish

```
Refactor the POS queue pages:

1. QUEUE BOARD (/queue):
   - Three-column Kanban layout from UI_UX_ADDENDUM.md Section 3
   - Columns scroll independently, each has a count in the header
   - CALLED column cards: border-warning bg-warning/5 with subtle pulse
   - Add <CountdownTimer> to CALLED cards (5 min countdown, red under 1 min)
   - Add <ElapsedTimer> to IN_SERVICE cards (shows "23 min elapsed")
   - VIP entries show purple badge
   - Action buttons: [Call], [Start Service], [No-Show], [View Transaction]
   - Cards are NOT draggable — buttons only
   - Top stats bar: "Waiting: 5 | Avg Wait: 12 min | Served Today: 23"

2. PUBLIC QUEUE DISPLAY (/queue-display):
   - Full dark theme: bg-gray-950 text-white
   - Large fonts: text-2xl for rows, text-4xl for header
   - Plate numbers MASKED: "AB*-****4" (first 2 + last 1 visible)
   - CALLED row: yellow background flash animation (keyframes, 2s loop)
   - Auto-scroll if >8 entries
   - Bottom stats: "Today: 23 served | Average wait: 12 min"
   - "Reconnecting..." banner if SignalR drops
   - No user interaction elements — pure display
   
3. Create the timer components if they don't exist:
   - components/pos/countdown-timer.tsx: accepts target DateTime, shows MM:SS, 
     turns red text under 60 seconds
   - components/pos/elapsed-timer.tsx: accepts start DateTime, shows "X min elapsed"
```

---

## RETROFIT 10: Loading States & Empty States

```
Go through EVERY page in both apps and ensure:

1. LOADING STATES:
   - Every page that fetches data shows a skeleton while loading
   - Tables: 5 skeleton rows matching column layout
   - Dashboard: skeleton cards + skeleton charts
   - Detail pages: skeleton matching the tab content
   - Do NOT use spinning loaders — only skeleton placeholders
   - Use shadcn <Skeleton> component with appropriate widths/heights

2. EMPTY STATES:
   - Every list/table page: when data returns empty, show <EmptyState>
   - Include relevant icon, title, description, and action button
   - Examples:
     - Services: icon=Sparkles, "No services yet", "Create your first service", action="/services/new"
     - Employees: icon=Users, "No employees", "Add your first team member"
     - Transactions: icon=Receipt, "No transactions today", "Start a new transaction"
     - Queue: icon=Clock, "Queue is empty", "All caught up!"

3. ERROR STATES:
   - Add error.tsx boundary in both (dashboard) and (terminal) route groups
   - Show friendly error with retry button, not raw error messages
   - API errors from the backend should show as toast notifications, not page crashes
```

---

## RETROFIT 11: Dark Mode (Admin Only)

```
Add dark mode support to the admin app ONLY (not POS):

1. Ensure the .dark CSS custom properties in globals.css are defined 
   (from UI_UX_ADDENDUM.md Section 1).

2. Create a ThemeProvider component using next-themes:
   - Install: pnpm add next-themes
   - Wrap the root layout with <ThemeProvider attribute="class" defaultTheme="system">
   - Create a ThemeToggle component: Sun/Moon icon button
   - Place it in the admin header next to the notification bell

3. Verify all pages look correct in dark mode:
   - Cards should use bg-card (maps to dark surface)
   - Borders use border (maps to dark border color)
   - Text uses foreground (maps to light text on dark)
   - Charts: adjust line/bar colors if they become invisible on dark backgrounds
   - Status badges: the bg-{color}-100 classes might need dark variants — 
     if they look washed out, use bg-{color}-900/20 text-{color}-300 for dark mode

4. Do NOT add dark mode to the POS app. The POS stays light theme only.
```

---

## Run Order

| # | Retrofit | What Changes | Risk |
|---|---|---|---|
| 1 | Design System Foundation | globals.css, color tokens, formatPeso | Low — styling only |
| 2 | Status Badges | New component, replace inline badges | Low — visual only |
| 3 | Shared UI Components | New components, replace inline markup | Medium — touches many files |
| 4 | Admin Sidebar & Header | Layout refactor | Medium — structural change |
| 5 | Admin Data Tables | Table styling standardization | Low — styling only |
| 6 | Admin Forms & Detail Pages | Form layout, matrix editors | Medium — component extraction |
| 7 | POS Layout & Touch | Layout refactor, sizing | Medium — structural change |
| 8 | POS Transaction Screen | Transaction page rework | High — critical UX surface |
| 9 | Queue Board & Display | Queue pages rework | Medium — component additions |
| 10 | Loading & Empty States | Add states to all pages | Low — additive only |
| 11 | Dark Mode (Admin) | Theme toggle, dark CSS | Low — additive only |

**Total: 11 retrofit prompts.** Start with 1-3 (low risk, foundational). Then 4-6 (admin polish). Then 7-9 (POS polish). Finish with 10-11 (final touches).

After each retrofit, verify the app compiles and visually check 2-3 pages before moving on.
