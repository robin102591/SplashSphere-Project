# SplashSphere — Tenant Onboarding Wizard

> **Phase:** Integrated into Phase 7 (Auth & Onboarding) from PROMPTS.md. This spec expands the brief 4-step outline in CLAUDE.md into a full feature specification.
> **Route:** `/onboarding` (admin app only)
> **Access:** Users with no `org_id` claim are redirected here. All other routes return 403 until onboarding is complete.

---

## The Problem

A car wash owner signs up for SplashSphere. They land on an empty dashboard with zero data — no services, no employees, no branches, no pricing. They don't know where to start.

If the first 5 minutes feel overwhelming, they abandon. If the first 5 minutes feel guided and productive, they stay. The onboarding wizard bridges sign-up to first-value by:

1. Collecting business info (tenant creation)
2. Setting up the first branch
3. Seeding Philippine car wash defaults (services, sizes, vehicle types, supply categories)
4. Optionally adding first employees and configuring pricing
5. Activating the 14-day trial with Growth features
6. Dropping them into a dashboard that already has useful structure

---

## Wizard Steps

### Step 1: Welcome (No form — just context)

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│          💧 Welcome to SplashSphere!                         │
│                                                              │
│   Let's set up your car wash business in just a few          │
│   minutes. We'll create your account, set up your first      │
│   branch, and get you ready to process transactions.         │
│                                                              │
│   ✓ 14-day free trial with all Growth features               │
│   ✓ No credit card required                                  │
│   ✓ Takes about 3 minutes                                    │
│                                                              │
│                                        [Get Started →]       │
│                                                              │
│   Already have an invitation code?                           │
│   [Enter Franchise Invitation Code]                          │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

**Franchise path:** If the user clicks "Enter Franchise Invitation Code," they enter a token received from a franchisor. The wizard switches to the **Franchisee Onboarding Flow** (Step 1F below) — different steps, pre-filled data from the franchise template.

### Step 2: Business Details

```
┌──────────────────────────────────────────────────────────────┐
│  Step 2 of 5 — Tell us about your business                   │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│                                                              │
│  Business Name *              [AquaShine Car Wash          ] │
│  Description                  [Premium car wash services   ] │
│  Email *                      [info@aquashine.ph           ] │
│  Contact Number *             [09171234567                 ] │
│  Address                      [123 Rizal Ave, Makati City  ] │
│  Website                      [https://aquashine.ph        ] │
│                                                              │
│  Business Type                                               │
│  ○ Independent Car Wash (most common)                        │
│  ○ Multi-Branch Chain                                        │
│  ○ Franchise Network (I'm a franchisor)                      │
│                                                              │
│                              [← Back]  [Next: Branch Setup →]│
└──────────────────────────────────────────────────────────────┘
```

**Validation:**
- Business Name: required, 2-100 characters
- Email: required, valid email format, unique across tenants
- Contact: required, Philippine phone format (09XX-XXX-XXXX)
- Business Type: required (defaults to "Independent")

**Business Type affects:**
- Independent: standard onboarding, Starter/Growth plans available
- Multi-Branch Chain: same as Independent but prompts for Growth/Enterprise plan later
- Franchise Network: enables franchisor features, requires Enterprise plan, shows franchise setup steps after core onboarding

### Step 3: First Branch Setup

```
┌──────────────────────────────────────────────────────────────┐
│  Step 3 of 5 — Set up your first branch                     │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│                                                              │
│  Branch Name *                [AquaShine - Makati           ] │
│  Address *                    [123 Rizal Ave, Makati City   ] │
│  Contact Number *             [09171234567                  ] │
│  Description                  [Main branch                  ] │
│                                                              │
│  💡 You can add more branches later from the admin dashboard. │
│     Your plan determines how many branches you can have.      │
│                                                              │
│                          [← Back]  [Next: Services Setup →]  │
└──────────────────────────────────────────────────────────────┘
```

**Auto-fill:** Branch contact and address default to the business details from Step 2. Owner can change them.

### Step 4: Services & Pricing Quick Setup

This is the most important step — it determines what appears on the POS.

```
┌──────────────────────────────────────────────────────────────┐
│  Step 4 of 5 — Configure your services                       │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│                                                              │
│  We've set up common car wash services for you.              │
│  Toggle the ones you offer and adjust prices if needed.      │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ ✓  Basic Wash ..................... ₱200    [Edit]     │  │
│  │ ✓  Premium Wash .................. ₱350    [Edit]     │  │
│  │ ✓  Wax & Polish .................. ₱450    [Edit]     │  │
│  │ ✓  Interior Vacuum ............... ₱150    [Edit]     │  │
│  │ ✓  Full Interior Clean ........... ₱400    [Edit]     │  │
│  │ ✓  Undercarriage Wash ............ ₱180    [Edit]     │  │
│  │ ✓  Tire & Rim Shine .............. ₱120    [Edit]     │  │
│  │ ✗  Engine Wash ................... ₱350    [Edit]     │  │
│  │ ✗  Exterior Detailing ............ ₱800    [Edit]     │  │
│  │ ✗  Full Detailing ................ ₱1,500  [Edit]     │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  [+ Add Custom Service]                                      │
│                                                              │
│  ⚙️ Prices shown are base prices (Medium Sedan).              │
│  You can configure the full pricing matrix by vehicle size    │
│  after setup.                                                 │
│                                                              │
│                        [← Back]  [Next: Add Employees →]     │
└──────────────────────────────────────────────────────────────┘
```

**What happens behind the scenes:**
- Toggled-on services are created with the displayed base prices
- Default vehicle types are seeded: Sedan, SUV, Van, Truck, Hatchback, Pickup
- Default sizes are seeded: Small, Medium, Large, XL
- Default service categories are seeded: Basic Services, Premium Services, Add-Ons, Detailing
- Default supply categories are seeded: Cleaning Chemicals, Wax & Polish, Tire & Trim, Towels & Cloths, Brushes & Tools, Water & Utilities, Packaging & Miscellaneous
- A simple pricing matrix is auto-generated using multipliers:
  - Small: base × 0.85
  - Medium: base × 1.00 (the displayed price)
  - Large: base × 1.30
  - XL: base × 1.60

**Edit dialog** (inline expansion or modal):
- Service name, description, base price, duration (minutes), category

### Step 5: Add First Employees (Optional — Can Skip)

```
┌──────────────────────────────────────────────────────────────┐
│  Step 5 of 5 — Add your team (optional)                      │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│                                                              │
│  Add your employees so they appear on the POS when           │
│  assigning services. You can always add more later.          │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ Name              Type         Branch      Date Hired  │  │
│  │ [Juan Dela Cruz ] [Commission▾] [Makati ▾] [03/01/26] │  │
│  │ [Pedro Santos   ] [Commission▾] [Makati ▾] [03/01/26] │  │
│  │ [Maria Garcia   ] [Commission▾] [Makati ▾] [03/15/26] │  │
│  │ [Ana Reyes      ] [Daily     ▾] [Makati ▾] [02/15/26] │  │
│  │                                    [+ Add Another]     │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  Commission employees earn per service performed.            │
│  Daily employees earn a fixed rate per day worked.           │
│                                                              │
│                [← Back]  [Skip for Now]  [Complete Setup →]  │
└──────────────────────────────────────────────────────────────┘
```

**Daily Rate field** appears when type = Daily:
- Daily Rate: ₱ input (e.g., ₱500)

**Minimum:** 0 employees (can skip entirely). No maximum during onboarding.

---

## What Happens On "Complete Setup"

A single API call: `POST /api/v1/onboarding`

**Request Payload:**
```typescript
{
  business: {
    name: string;
    description?: string;
    email: string;
    contact: string;
    address?: string;
    website?: string;
    businessType: 'independent' | 'chain' | 'franchisor';
  },
  branch: {
    name: string;
    address: string;
    contact: string;
    description?: string;
  },
  services: [
    { name: string; basePrice: number; durationMinutes: number; 
      categoryName: string; description?: string; isActive: boolean }
  ],
  employees: [
    { name: string; type: 'COMMISSION' | 'DAILY'; dailyRate?: number; 
      dateHired: string }
  ]
}
```

**Backend Processing (single transaction):**

```
1. Create Clerk Organization via Clerk Backend API
   → orgId = response.id

2. Create Tenant record
   → id = orgId (matches Clerk org)
   → name, email, contact, address, website from payload

3. Create TenantSubscription
   → planTier = Trial
   → trialStartDate = now
   → trialEndDate = now + 14 days
   → status = Active

4. Add user to Clerk Organization as admin
   → Clerk API: addOrganizationMember(orgId, userId, role: 'admin')

5. Link User to Tenant
   → user.tenantId = tenant.id

6. Seed reference data for this tenant:
   a. Vehicle Types: Sedan, SUV, Van, Truck, Hatchback, Pickup
   b. Sizes: Small, Medium, Large, XL
   c. Makes: Toyota, Honda, Mitsubishi, Nissan, Suzuki, Ford, 
      Hyundai, Kia, Isuzu, Mazda (top 10 Philippine brands)
   d. Service Categories: Basic Services, Premium Services, Add-Ons, Detailing
   e. Supply Categories: Cleaning Chemicals, Wax & Polish, Tire & Trim, 
      Towels & Cloths, Brushes & Tools, Water & Utilities, Packaging & Misc
   f. Merchandise Categories: Air Fresheners, Cleaning Products, Accessories

7. Create Branch record
   → linked to tenant

8. Create Service records (only isActive = true ones)
   → linked to tenant
   → assigned to selected categories

9. Auto-generate ServicePricing matrix
   For each service × vehicleType × size:
   → Small: basePrice × 0.85
   → Medium: basePrice × 1.00
   → Large: basePrice × 1.30
   → XL: basePrice × 1.60
   (Owner can fine-tune later in the pricing matrix editor)

10. Auto-generate default ServiceCommission records
    For each service × vehicleType × size:
    → commissionType = PERCENTAGE
    → percentageRate = 0.15 (15% — common Philippine car wash default)
    (Owner adjusts per service later)

11. Create Employee records
    → linked to tenant + branch
    → type and dailyRate from payload

12. Create initial PayrollPeriod
    → current week, status = OPEN

13. Log audit entry: "Tenant onboarded"

14. Return: { tenantId, branchId, redirectUrl: '/dashboard' }
```

**Frontend after success:**
- Redirect to `/dashboard`
- Show a welcome toast: "🎉 Welcome to SplashSphere! Your 14-day trial has started."
- First-time flag in localStorage triggers the **Guided Tour**

---

## Guided Tour (Post-Onboarding)

After the wizard completes, the dashboard loads with a guided tour overlay. Uses a tooltip-based walkthrough (like Shepherd.js or custom implementation).

**Tour Steps (7 stops):**

```
Stop 1: Dashboard
"This is your command center. You'll see today's revenue, 
transactions, and key metrics here."
[Next →]

Stop 2: Sidebar → Services
"Manage your car wash services, pricing, and packages here. 
We've set up defaults — customize them anytime."
[Next →]

Stop 3: Sidebar → Employees
"Add and manage your team. Commission-based employees earn 
per service. Daily employees earn a fixed rate."
[Next →]

Stop 4: Sidebar → POS (link)
"This opens your Point of Sale terminal — where your cashier 
processes transactions. Open it in a separate tab for your 
POS device."
[Next →]

Stop 5: Sidebar → Reports
"Revenue reports, payroll summaries, and business analytics 
live here. More data appears as you process transactions."
[Next →]

Stop 6: Top Bar → Branch Selector
"If you add more branches later, switch between them here 
to see branch-specific data."
[Next →]

Stop 7: Top Bar → Settings
"Configure your account, manage users, update your subscription, 
and customize your business settings here."
[Finish Tour ✓]
```

**Tour behavior:**
- Dismissible at any step ("Skip Tour" link)
- Tour state saved to localStorage (`tourCompleted: true`)
- Accessible again from Settings → "Replay Welcome Tour"
- Does NOT show for invited users (they join an existing tenant)

---

## Franchisee Onboarding Flow (Invitation-Based)

When a user enters a franchise invitation code on Step 1:

### Step 1F: Verify Invitation

```
┌──────────────────────────────────────────────────────────────┐
│  Franchise Invitation                                        │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│                                                              │
│  Invitation Code *       [SPEEDYWASH-2026-A3F8            ]  │
│                                                              │
│  ✅ Valid invitation from SpeedyWash PH                       │
│                                                              │
│  You're being invited to join the SpeedyWash franchise       │
│  network as a franchisee. Your SplashSphere account will     │
│  be linked to the SpeedyWash network.                        │
│                                                              │
│  Franchise: SpeedyWash PH                                    │
│  Territory: Cebu City                                        │
│  Plan: Growth (included in franchise agreement)              │
│                                                              │
│                                   [Accept & Continue →]      │
└──────────────────────────────────────────────────────────────┘
```

### Step 2F: Business Details (Pre-filled)

Same as Step 2, but:
- Business Name pre-filled: "SpeedyWash - Cebu"
- Business Type locked to "Franchisee"
- Some fields pre-filled from franchisor's template

### Step 3F: Branch Setup (Pre-filled Territory)

Same as Step 3, but address pre-filled with the territory from the invitation.

### Step 4F: Services (Inherited from Franchisor)

```
┌──────────────────────────────────────────────────────────────┐
│  Step 4F — Services (from SpeedyWash franchise template)     │
│  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━   │
│                                                              │
│  These services are standardized by your franchise network.  │
│  Prices and names are set by SpeedyWash PH.                  │
│                                                              │
│  🔒 SpeedyWash Basic .................. ₱220                 │
│  🔒 SpeedyWash Premium ................ ₱380                 │
│  🔒 SpeedyWash Wax .................... ₱480                 │
│  🔒 SpeedyWash Interior ............... ₱180                 │
│     + Full Detailing .................. ₱1,500 [can edit]    │
│                                                              │
│  🔒 = Set by franchise. You can add your own services below. │
│  [+ Add Custom Service]                                      │
│                                                              │
│                        [← Back]  [Next: Add Employees →]     │
└──────────────────────────────────────────────────────────────┘
```

Franchisee services come from `FranchiseServiceTemplate`. Locked services can't be edited by the franchisee. They can add their own additional services.

### Step 5F: Employees

Same as Step 5 (no franchise restrictions on employees).

**On submit:** Same as standard onboarding, plus:
- Set `tenant.parentTenantId` = franchisor's tenant ID
- Set `tenant.tenantType` = Franchisee
- Create `FranchiseAgreement` record
- Mark invitation as accepted
- Clone `FranchiseServiceTemplate` entries as the franchisee's services
- Auto-assign the plan specified in the franchise agreement

---

## Seed Data Reference

### Default Services (Philippine Car Wash)

| Service | Base Price (Medium) | Duration | Category |
|---|---|---|---|
| Basic Wash | ₱200 | 30 min | Basic Services |
| Premium Wash | ₱350 | 45 min | Premium Services |
| Wax & Polish | ₱450 | 60 min | Premium Services |
| Interior Vacuum | ₱150 | 20 min | Basic Services |
| Full Interior Clean | ₱400 | 45 min | Premium Services |
| Undercarriage Wash | ₱180 | 15 min | Add-Ons |
| Tire & Rim Shine | ₱120 | 10 min | Add-Ons |
| Engine Wash | ₱350 | 30 min | Add-Ons |
| Exterior Detailing | ₱800 | 90 min | Detailing |
| Full Detailing | ₱1,500 | 180 min | Detailing |

### Price Multipliers by Size

| Size | Multiplier | Example (Basic Wash) |
|---|---|---|
| Small | 0.85× | ₱170 |
| Medium | 1.00× | ₱200 |
| Large | 1.30× | ₱260 |
| XL | 1.60× | ₱320 |

### Default Commission Rate

All services: **15% of service price**, divided equally among assigned employees. This is the most common rate in Philippine car washes. Owners can customize per service × vehicle type × size in the commission matrix editor.

### Default Vehicle Makes (Top 10 Philippines)

Toyota, Honda, Mitsubishi, Nissan, Suzuki, Ford, Hyundai, Kia, Isuzu, Mazda

### Default Vehicle Types

Sedan, SUV, Van, Truck, Hatchback, Pickup

### Default Sizes

Small, Medium, Large, XL

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/onboarding/status` | Check if current user needs onboarding (has tenant or not) |
| `POST` | `/onboarding` | Complete the standard onboarding wizard |
| `POST` | `/onboarding/franchise` | Complete the franchisee onboarding (with invitation code) |
| `POST` | `/onboarding/validate-invitation` | Validate a franchise invitation code (returns franchisor info) |
| `GET` | `/onboarding/default-services` | Get the default service list for Step 4 (so frontend doesn't hardcode) |

---

## Frontend Implementation

### Route Structure

```
apps/admin/app/(onboarding)/
├── layout.tsx          # Clean layout — no sidebar, just SplashSphere logo + progress bar
├── onboarding/
│   └── page.tsx        # Main wizard page (all steps rendered as one page with step transitions)
```

### UX Details

- **Progress bar** at the top showing current step (1 of 5)
- **Step transitions:** Slide animation (left-to-right on next, right-to-left on back)
- **Validation:** Inline validation on blur. Next button disabled until required fields are valid.
- **Auto-save draft:** Each step's data saved to `sessionStorage`. If the user refreshes mid-wizard, they resume where they left off.
- **Mobile responsive:** Single-column layout on mobile. Steps stack vertically.
- **Loading state:** "Setting up your business..." with animated splash logo during the API call (takes 2-3 seconds to create everything)
- **Error handling:** If API fails, show error toast and keep the form intact. Retry button.

### Layout (No Sidebar)

The onboarding page uses a **minimal layout** — no sidebar, no header navigation. Just:
- SplashSphere logo (top-left)
- Progress bar (top)
- Step content (center, max-width 640px)
- Navigation buttons (bottom)

This keeps the focus entirely on setup. The full admin layout loads only after onboarding completes.

---

## Onboarding Completion Checklist

After onboarding, show a subtle "Setup Progress" card on the dashboard for the first 7 days. This encourages the owner to complete optional configurations:

```
┌─ 🎯 Complete Your Setup — 4 of 8 done ─────────────────────┐
│  ███████████░░░░░░░░░░░░░░░░░░░░░░░ 50%                    │
│                                                              │
│  ✅ Create your account                                      │
│  ✅ Set up first branch                                      │
│  ✅ Configure services                                       │
│  ✅ Add employees                                            │
│  ☐  Configure pricing matrix (customize by vehicle size)     │
│  ☐  Set up commission rates per service                      │
│  ☐  Create your first transaction on POS                     │
│  ☐  Invite a team member (cashier)                           │
│                                                              │
│  Each completed item links to the relevant settings page.    │
│                                               [Dismiss ×]    │
└──────────────────────────────────────────────────────────────┘
```

**Completion tracking:** Stored as a JSON field on the Tenant or in a separate `OnboardingProgress` entity. Updated by checking the database:
- Has pricing matrix entries beyond defaults? ✓
- Has commission records? ✓
- Has at least 1 completed transaction? ✓
- Has invited at least 1 team member (>1 user)? ✓

---

## Claude Code Prompts

### Prompt 7.3a — Onboarding Backend

```
Build the tenant onboarding backend:

Application/Features/Onboarding/:
- CheckOnboardingStatusQuery → returns { needsOnboarding: bool, tenantId?: string }
- GetDefaultServicesQuery → returns the default Philippine car wash service list
- CompleteOnboardingCommand:
  Accepts: business details, branch, services[], employees[]
  In a single transaction:
  1. Create Clerk Organization (via IClerkService)
  2. Create Tenant (id = Clerk orgId)
  3. Create TenantSubscription (Trial, 14 days, Growth features)
  4. Add user to Clerk org as admin
  5. Link User.tenantId
  6. Seed: VehicleTypes, Sizes, Makes, ServiceCategories, 
     SupplyCategories, MerchandiseCategories
  7. Create Branch
  8. Create Services with auto-generated pricing matrix 
     (Small 0.85×, Medium 1.0×, Large 1.3×, XL 1.6×)
  9. Create default ServiceCommission records (15% for all)
  10. Create Employees
  11. Create initial PayrollPeriod (current week, OPEN)
  Return: { tenantId, branchId }

- CompleteFranchiseOnboardingCommand:
  Accepts: invitation code, business details, branch, employees[]
  Same as above but:
  - Validates invitation code
  - Sets parentTenantId and tenantType = Franchisee
  - Clones FranchiseServiceTemplate as services
  - Creates FranchiseAgreement
  - Marks invitation as accepted
  - Plan from franchise agreement (not Trial)

- ValidateFranchiseInvitationQuery:
  Accepts: invitation code
  Returns: { valid, franchisorName, territory, planTier }

Endpoints: OnboardingEndpoints.cs
Routes: GET /onboarding/status, POST /onboarding, 
        POST /onboarding/franchise, POST /onboarding/validate-invitation,
        GET /onboarding/default-services
```

### Prompt 7.3b — Onboarding Frontend

```
Build the onboarding wizard UI:

1. Create (onboarding) route group with minimal layout:
   - No sidebar, no header nav
   - SplashSphere logo top-left
   - Progress bar (step X of 5)
   - Max-width 640px centered content

2. /onboarding page — multi-step wizard:
   Step 1: Welcome screen with "Get Started" CTA and franchise code option
   Step 2: Business details form (name, email, contact, address, business type)
   Step 3: First branch setup (name, address, contact)
   Step 4: Service selection — default services with toggles and inline price editing.
           "Add Custom Service" button. Base price label: "(Medium Sedan price)"
   Step 5: Employee list — add rows with name, type dropdown, daily rate (if Daily),
           date hired. "Skip for Now" button.

   Step transitions: slide animation with AnimatePresence or CSS transitions
   Validation: inline on blur, Next button disabled until valid
   Draft auto-save to sessionStorage (resume on refresh)

3. Loading screen on submit:
   "Setting up your business..."
   Animated SplashSphere logo
   Progress steps appearing one by one:
   "Creating your account... ✓"
   "Setting up services... ✓"
   "Adding employees... ✓"
   "Almost ready... ✓"
   Then redirect to /dashboard

4. Franchise path:
   If invitation code entered → validate via API → show franchise info →
   Steps 2F-5F with pre-filled data and locked franchise services

5. Post-onboarding:
   - Welcome toast on dashboard
   - Guided tour (7 stops) using tooltip overlay
   - "Setup Progress" card on dashboard (50% complete checklist)
   - Tour state in localStorage, dismissible, replayable from Settings
```

---

## Phase Summary

| Prompt | What | Layer |
|---|---|---|
| 7.3a | Onboarding commands, seed data, Clerk org creation, franchise flow | Backend |
| 7.3b | Wizard UI, step transitions, guided tour, setup checklist | Frontend |

**Total: 2 prompts integrated into Phase 7.**
