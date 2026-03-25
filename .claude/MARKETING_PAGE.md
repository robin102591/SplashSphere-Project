# SplashSphere — Marketing Landing Page Specification

> **What this is:** Complete spec for the public-facing marketing website at `splashsphere.ph`.
> This is a separate Next.js 16 project (not part of the admin or POS apps) that serves as the product homepage, pricing page, and lead capture funnel.

---

## Purpose & Goals

The marketing site converts car wash owners who found SplashSphere (via Google, Facebook ads, word of mouth, or industry events) into trial signups or demo requests. It must:

1. Explain what SplashSphere does in under 10 seconds
2. Build credibility with Philippine car wash owners
3. Show pricing transparently
4. Capture leads (email/phone for demo booking)
5. Drive signups to the admin app's onboarding flow

**Target audience:** Filipino car wash business owners, ages 28–55, managing 1–5 branches, currently tracking operations manually (notebooks, Excel, or basic POS). Many browse on mobile. They care about: saving time, accurate commission tracking, knowing their profit, and controlling multiple branches remotely.

---

## Tech Stack

- **Framework:** Next.js 16 (separate project, App Router, static export for most pages)
- **Styling:** Tailwind CSS 4 with the SplashSphere theme
- **Animations:** Framer Motion for scroll reveals and section transitions
- **Forms:** React Hook Form + Zod for the contact/demo form
- **Analytics:** Google Analytics 4 + Meta Pixel (for Facebook ad tracking)
- **CMS (optional):** MDX for blog posts and case studies
- **Deployment:** Vercel (optimal for Next.js) or static export to any CDN
- **Domain:** `splashsphere.ph` (main), `app.splashsphere.ph` (admin), `pos.splashsphere.ph` (POS)

---

## Site Map

```
splashsphere.ph/
├── / .......................... Landing page (hero, features, pricing, CTA)
├── /features ................. Detailed feature breakdown
├── /pricing .................. Pricing plans with comparison table
├── /about .................... About LezanobTech, the story, the team
├── /contact .................. Contact form + demo request
├── /blog ..................... Blog/articles (SEO content)
│   └── /blog/[slug] ......... Individual blog post
├── /case-studies ............. Customer success stories
│   └── /case-studies/[slug] .. Individual case study
├── /privacy .................. Privacy Policy (RA 10173 compliant)
├── /terms .................... Terms of Service
└── /sign-up .................. Redirect to app.splashsphere.ph/sign-up
```

---

## Page 1: Landing Page (`/`)

This is the most important page. A single long-scroll page with these sections in order:

### Section 1: Hero

**Layout:** Full viewport height. Split: left text (55%), right mockup image (45%).

**Headline (H1):**
> Manage your car wash business the smart way

**Subheadline:**
> SplashSphere handles commissions, payroll, queueing, and daily reporting — so you can focus on growing your business, not chasing paper receipts.

**CTA Buttons:**
- Primary: "Start Free Trial" → links to `app.splashsphere.ph/sign-up`
- Secondary: "Book a Demo" → scrolls to contact form or links to `/contact`

**Trust Bar** below the CTA:
> "Trusted by 50+ car wash businesses across the Philippines"
> Logos/icons: "GCash Ready", "Multi-Branch", "Weekly Payroll", "Real-Time Dashboard"

**Hero Image:** Screenshot or mockup of the admin dashboard on a laptop and the POS on a tablet, showing the SplashSphere UI with Philippine data (peso amounts, Filipino names).

### Section 2: Pain Points

**Heading:** "Running a car wash shouldn't mean drowning in spreadsheets"

Three pain point cards:

| Icon | Pain | Description |
|---|---|---|
| 📓 | Manual commission tracking | "Still using notebooks? One miscalculation and your best washer is unhappy." |
| 💸 | No visibility into profit | "Revenue looks great, but after expenses and commissions — are you actually making money?" |
| 🏃 | Can't be everywhere at once | "You have 3 branches. Which one are you not watching right now?" |

### Section 3: Solution Overview

**Heading:** "One platform for everything"

**Subheading:** "SplashSphere replaces your notebook, calculator, and Excel files with a system built for Philippine car wash operations."

Feature grid (2×3 on desktop, 1 column on mobile):

| Feature | Description | Icon |
|---|---|---|
| POS Terminal | Process transactions in seconds. Dynamic pricing by vehicle type and size. Cash, GCash, card. | 🖥️ |
| Commission Tracking | Automatic split calculation across employees. Percentage, fixed, or hybrid. No more arguments. | 💰 |
| Weekly Payroll | One-click payroll with commission + daily rate + cash advance deductions. Every Saturday, done in 5 minutes. | 📋 |
| Queue Management | Customers get a number. You control the flow. Wall TV display keeps everyone informed. | 📋 |
| Multi-Branch Dashboard | See all branches in one screen. Revenue, transactions, employees — real-time. | 📊 |
| End-of-Day Reporting | Cash count, denomination breakdown, variance tracking. Know exactly where every peso went. | 📄 |

### Section 4: Product Screenshots

**Heading:** "See it in action"

Tabbed carousel showing 4 screenshots with descriptions:

1. **Dashboard** — "Your entire business at a glance. Revenue, commissions, top services, payment breakdown — all in real-time."
2. **POS Transaction** — "Plate lookup, service selection, employee assignment, payment — one screen, no page switching."
3. **Queue Board** — "Kanban-style queue. Call the next customer, track service progress, handle no-shows automatically."
4. **Payroll** — "Close the week, review entries, process payroll. Commission employees and daily staff in one place."

Use actual screenshots from the admin and POS apps (or high-fidelity mockups from the HTML reference page).

### Section 5: How It Works

**Heading:** "Up and running in 15 minutes"

Three steps in a horizontal timeline:

```
Step 1                    Step 2                    Step 3
[Sign Up]  ──────────>  [Set Up]  ──────────>  [Start Washing]
                                     
Create your account.     Add your services,        Open the POS, tap
Free trial, no           pricing, employees,       services, assign
credit card needed.      and branches.             employees — done.
```

### Section 6: Pricing

**Heading:** "Simple, honest pricing"
**Subheading:** "No hidden fees. No per-transaction charges. Cancel anytime."

Three pricing cards:

**Starter — ₱1,499/month**
- 1 branch
- Up to 5 employees
- POS & transactions
- Commission tracking
- Weekly payroll
- Basic reports
- Email support
- CTA: "Start Free Trial"

**Growth — ₱2,999/month** ⭐ Most Popular
- Up to 3 branches
- Up to 15 employees
- Everything in Starter
- Queue management
- Customer loyalty
- Cash advance tracking
- Expense tracking & P&L reports
- Cash reconciliation / shift reports
- SMS notifications (50/month)
- Priority support
- CTA: "Start Free Trial"

**Enterprise — ₱4,999/month**
- Unlimited branches
- Unlimited employees
- Everything in Growth
- API access
- Custom integrations
- Dedicated support
- SLA & onboarding
- SMS notifications (200/month)
- CTA: "Contact Sales"

**Below pricing:** "All plans include a 14-day free trial. No credit card required."

**FAQ toggle below pricing:**
- "Can I switch plans later?" → Yes, upgrade or downgrade anytime.
- "Is there a setup fee?" → No. Setup is free and takes 15 minutes.
- "What payment methods do you accept?" → GCash, credit/debit card, bank transfer.
- "Do you charge per transaction?" → No. Unlimited transactions on all plans.
- "Can I cancel anytime?" → Yes. No lock-in contracts.
- "Is my data safe?" → Yes. Data is encrypted, backed up daily, and hosted on secure cloud infrastructure.

### Section 7: Testimonials

**Heading:** "Loved by car wash owners"

Three testimonial cards with star ratings:

**Testimonial 1:**
> "Before SplashSphere, I was tracking commissions in a notebook. Now payroll takes me 5 minutes every Saturday. My washers love the transparency."
> — **Juan Reyes**, Owner, AquaShine Car Wash, Quezon City

**Testimonial 2:**
> "We expanded from 1 branch to 3. The dashboard lets me compare Makati vs Cebu revenue in real-time. It's like having eyes everywhere."
> — **Maria Santos**, Owner, CleanDrive Auto Spa, Makati

**Testimonial 3:**
> "The queue system changed everything during weekends. No more arguing about who's next. Customers see the screen, they know their turn."
> — **Carlos Rivera**, Manager, SuperWash PH, Cebu

*(Use placeholder testimonials for launch. Replace with real ones as customers onboard.)*

### Section 8: Final CTA

**Heading:** "Ready to modernize your car wash?"
**Subheading:** "Join 50+ Philippine car wash businesses already using SplashSphere."

Two buttons:
- "Start Free Trial" (primary, large)
- "Book a Demo" (secondary)

Trust badges: "14-day free trial", "No credit card required", "Cancel anytime", "GCash ready"

### Section 9: Footer

```
SplashSphere                           Product                Company
The smart car wash                     Features               About
management platform                    Pricing                Contact
by LezanobTech                         Blog                   Careers

                                       Resources              Legal
                                       Case Studies           Privacy Policy
                                       Help Center            Terms of Service

© 2026 LezanobTech. All rights reserved.
Made with 💧 in the Philippines

[Facebook] [Instagram] [LinkedIn] [TikTok]
```

---

## Page 2: Features (`/features`)

Detailed breakdown of every feature with screenshots. Organized by category:

**POS & Transactions**
- Dynamic pricing matrix (vehicle type × size)
- Service packages with bundle discounts
- Multiple payment methods (Cash, GCash, Card)
- Split payment support
- Transaction receipt (printable)
- Daily transaction summary

**Employee & Payroll**
- Commission tracking (percentage, fixed, hybrid)
- Equal commission splitting among assigned employees
- Weekly payroll with automatic calculation
- Cash advance management with auto-deduction
- Attendance tracking (clock in/out)
- Employee performance reports

**Queue Management**
- Digital queue with priority levels (Regular, VIP, Express)
- Public queue display for wall-mounted TV
- 5-minute no-show auto-handling
- Real-time updates across all terminals
- Estimated wait time calculation

**Operations**
- Multi-branch management with branch switcher
- Cashier shift management (opening fund, cash movements, denomination count)
- End-of-day reporting with printable summary
- Expense tracking with profit & loss dashboard
- Merchandise inventory with low-stock alerts
- Pricing modifiers (peak hours, weekends, promotions)

**Customer Management**
- Customer database with vehicle registry
- Plate number quick lookup
- Loyalty points & tier membership (Bronze → Platinum)
- Service history per customer/vehicle
- SMS notifications

Each feature section: heading + 2-3 sentence description + screenshot or illustration.

---

## Page 3: Pricing (`/pricing`)

Same pricing cards as the landing page, but with a full **feature comparison table** below:

| Feature | Starter | Growth | Enterprise |
|---|---|---|---|
| Branches | 1 | 3 | Unlimited |
| Employees | 5 | 15 | Unlimited |
| Transactions | Unlimited | Unlimited | Unlimited |
| POS Terminal | ✓ | ✓ | ✓ |
| Commission Tracking | ✓ | ✓ | ✓ |
| Weekly Payroll | ✓ | ✓ | ✓ |
| Basic Reports | ✓ | ✓ | ✓ |
| Queue Management | — | ✓ | ✓ |
| Customer Loyalty | — | ✓ | ✓ |
| Cash Advance Tracking | — | ✓ | ✓ |
| Expense Tracking | — | ✓ | ✓ |
| Shift / Cash Reconciliation | — | ✓ | ✓ |
| P&L Reports | — | ✓ | ✓ |
| SMS Notifications | — | 50/mo | 200/mo |
| API Access | — | — | ✓ |
| Custom Integrations | — | — | ✓ |
| Dedicated Support | — | — | ✓ |
| SLA | — | — | ✓ |
| Support | Email | Priority | Dedicated |

---

## Page 4: About (`/about`)

**Company:** LezanobTech
**Product:** SplashSphere
**Location:** Philippines

**Story section:**
> "We built SplashSphere because we saw car wash owners struggling with the same problems every week — lost notebooks, commission arguments, no idea if they're actually profitable. We knew there had to be a better way."

**Mission:** Empowering car wash entrepreneurs with affordable, locally-relevant business management tools.

**Values:**
- Built for the Philippines — peso, GCash, local names, weekly payroll
- Simple enough for a cashier, powerful enough for an owner
- Transparent pricing, no hidden fees
- Customer data is private and secure

---

## Page 5: Contact / Demo (`/contact`)

**Heading:** "Let's talk about your car wash"
**Subheading:** "Book a free 30-minute demo, or just ask us anything."

**Form fields:**
- Full Name (required)
- Phone Number (required — PH format: 09XX-XXX-XXXX)
- Email (required)
- Business Name (required)
- Number of Branches (dropdown: 1, 2-3, 4-5, 6+)
- Current System (dropdown: Notebook/Paper, Excel/Spreadsheet, Other Software, Nothing)
- Message (textarea, optional)
- Submit button: "Book My Demo"

**On submit:** Send data to a backend endpoint or email service (Resend, SendGrid). Show success message: "Thanks! We'll call you within 24 hours."

**Alternative contact info:**
- Phone: +63 XXX XXX XXXX
- Email: hello@splashsphere.ph
- Facebook: facebook.com/splashsphere
- Office hours: Mon-Sat, 9 AM - 6 PM (PHT)

---

## Page 6: Blog (`/blog`)

SEO-driven content targeting Philippine car wash owners searching for solutions.

**Initial blog post ideas:**

1. "How to Calculate Car Wash Employee Commissions (Without Losing Your Mind)"
2. "The Real Cost of Running a Car Wash in the Philippines — 2026 Breakdown"
3. "GCash for Car Wash: How to Accept Digital Payments at Your Shop"
4. "5 Signs Your Car Wash Needs a POS System"
5. "Weekly Payroll vs Monthly: Why Philippine Car Washes Do It Differently"
6. "How to Manage Multiple Car Wash Branches Without Cloning Yourself"
7. "End-of-Day Cash Count: The Process Every Car Wash Should Follow"
8. "Customer Loyalty Programs That Actually Work for Car Wash Businesses"

Each post: 800-1,200 words, conversational tone, includes SplashSphere CTA at the end.
Use MDX for blog posts — allows embedding React components (pricing tables, CTAs) in content.

---

## SEO Strategy

### Target Keywords

| Keyword | Volume | Intent | Page |
|---|---|---|---|
| car wash POS system Philippines | Low-Med | Transactional | Landing page |
| car wash management software | Medium | Transactional | Landing page |
| car wash commission calculator | Low | Informational | Blog post |
| car wash payroll system | Low | Transactional | Features |
| car wash business Philippines | Medium | Informational | Blog post |
| POS system for car wash | Medium | Transactional | Landing page |
| GCash car wash payment | Low | Informational | Blog post |
| car wash queue management | Low | Transactional | Features |

### Meta Tags (Landing Page)

```html
<title>SplashSphere — Car Wash Management Software for the Philippines</title>
<meta name="description" content="POS, commission tracking, weekly payroll, and multi-branch management built for Philippine car wash businesses. Start your free trial today." />
<meta property="og:title" content="SplashSphere — Smart Car Wash Management" />
<meta property="og:description" content="Manage commissions, payroll, queues, and daily cash — all in one platform designed for Philippine car wash operations." />
<meta property="og:image" content="https://splashsphere.ph/og-image.png" />
<meta property="og:url" content="https://splashsphere.ph" />
<meta name="twitter:card" content="summary_large_image" />
```

### Technical SEO

- Sitemap.xml auto-generated by Next.js
- robots.txt allowing all crawlers
- Structured data (JSON-LD) for: Organization, SoftwareApplication, FAQ, BreadcrumbList
- All images with alt text
- Heading hierarchy: one H1 per page, proper H2/H3 nesting
- Canonical URLs on all pages
- Open Graph images for social sharing (1200×630px)

---

## Design Direction

### Visual Style

- **Clean, professional, trustworthy** — not flashy or gimmicky
- **Aquatic theme** — use the SplashSphere splash blue and aqua teal palette
- **Dark header/hero area** with gradient (`splash-900` → `splash-800`), white body sections
- **Photography style:** Real Filipino car wash environments if available, otherwise clean illustrations
- **Mockup frames:** Show the product in context — laptop for admin, tablet for POS, TV for queue display
- **No stock photos of generic business people** — use illustrations or real product screenshots

### Typography

- **Headings:** Plus Jakarta Sans 700/800 (same as the app)
- **Body:** Plus Jakarta Sans 400/500
- **Code/money:** JetBrains Mono

### Section Rhythm

Alternate between:
- White background sections (`bg-white`)
- Light gray sections (`bg-gray-50`)
- Dark accent sections for CTAs (`bg-splash-900 text-white`)

Each section has generous padding (`py-20` to `py-24`). Max content width: `max-w-6xl`.

### Mobile

- Hero: stacks vertically (text on top, image below)
- Feature grid: single column
- Pricing cards: horizontal scroll or vertical stack
- Navigation: hamburger menu
- CTAs: full-width buttons, minimum height 56px
- Phone number in footer is tappable (`tel:` link)

---

## Claude Code Prompts

### Prompt M.1 — Marketing Site Scaffold

```
Create a new Next.js 16 project at apps/marketing/ for the SplashSphere marketing website.

Tech: Next.js 16, TypeScript, Tailwind CSS 4, Framer Motion, React Hook Form + Zod.
Install: framer-motion, react-hook-form, zod, @hookform/resolvers, next-mdx-remote (for blog).

Structure:
- app/layout.tsx — root layout with marketing Navbar and Footer
- app/page.tsx — landing page (all sections)
- app/features/page.tsx
- app/pricing/page.tsx
- app/about/page.tsx
- app/contact/page.tsx
- app/blog/page.tsx + app/blog/[slug]/page.tsx
- app/case-studies/page.tsx
- app/privacy/page.tsx
- app/terms/page.tsx
- app/sign-up/page.tsx — redirect to app.splashsphere.ph/sign-up
- components/marketing/ — Navbar, Footer, Hero, FeatureCard, PricingCard, 
  TestimonialCard, StepCard, PainPointCard, CTASection, ContactForm, ScreenshotCarousel

Apply the SplashSphere theme (splash/aqua colors, Plus Jakarta Sans font).
Configure meta tags and Open Graph for the landing page as specified in the spec.
Build the Navbar (sticky, transparent on hero then white on scroll) and Footer first.
```

### Prompt M.2 — Landing Page Sections

```
Build the complete landing page (app/page.tsx) with all 9 sections from the 
marketing spec:

1. Hero: split layout, headline, subheadline, two CTA buttons, trust bar.
   Framer Motion: stagger-reveal text from left, mockup slide-in from right.

2. Pain Points: three cards with icons.
   Framer Motion: scroll-triggered fade-up with stagger.

3. Solution Overview: 2×3 feature grid with icons and descriptions.

4. Product Screenshots: tabbed carousel (4 tabs). Use placeholder images 
   or screenshots from the UI reference HTML. Auto-rotate every 5 seconds.

5. How It Works: three-step horizontal timeline with connecting lines.

6. Pricing: three cards (Starter, Growth with "Most Popular" badge, Enterprise).
   FAQ accordion below using details/summary or shadcn Accordion.

7. Testimonials: three cards with star ratings and Philippine names/locations.

8. Final CTA: dark background section, heading, two buttons, trust badges.

9. Footer: four-column layout, social links, copyright.

All sections use scroll-triggered Framer Motion animations (fade-up, stagger).
Responsive: all sections stack cleanly on mobile.
```

### Prompt M.3 — Features, Pricing, About Pages

```
Build:

1. /features — detailed feature breakdown organized by category (POS, Employee/Payroll,
   Queue, Operations, Customer). Each feature: heading, description, placeholder for 
   screenshot. Alternating layout (text-left/image-right, then text-right/image-left).

2. /pricing — same pricing cards as landing page, PLUS a full feature comparison table
   with checkmarks matching the spec. Sticky header on the table. Mobile: horizontal scroll.

3. /about — company story, mission, values. Clean layout with large typography.
   Include a "Meet the Founder" section placeholder.
```

### Prompt M.4 — Contact Form & Blog Setup

```
Build:

1. /contact — demo request form with all fields from the spec. 
   React Hook Form + Zod validation. Philippine phone number validation (starts with 09, 
   11 digits). On submit: POST to /api/contact (Next.js route handler that sends email 
   via Resend or logs to console in dev). Success state: "Thanks! We'll call you within 
   24 hours." with confetti or checkmark animation.

2. /blog — blog listing page. Use MDX (next-mdx-remote or @next/mdx) for blog posts.
   Create 2-3 sample blog posts as MDX files:
   - "How to Calculate Car Wash Employee Commissions"
   - "5 Signs Your Car Wash Needs a POS System"
   Each with: title, date, author, excerpt, body. Blog listing shows cards with title, 
   excerpt, date. Blog post page: clean reading layout (max-w-prose), 
   CTA banner at bottom ("Try SplashSphere free").

3. /privacy and /terms — placeholder legal pages with professional formatting.

4. Configure sitemap.xml generation, robots.txt, and JSON-LD structured data 
   (Organization + SoftwareApplication) in the root layout.
```

---

## Analytics & Conversion Tracking

### Events to Track

| Event | Trigger | Tool |
|---|---|---|
| `page_view` | Every page load | GA4 |
| `cta_click_trial` | "Start Free Trial" button click | GA4 + Meta Pixel |
| `cta_click_demo` | "Book a Demo" button click | GA4 + Meta Pixel |
| `pricing_view` | Pricing section scrolled into view | GA4 |
| `demo_form_submit` | Contact form submitted | GA4 + Meta Pixel (Lead event) |
| `signup_redirect` | User clicks through to sign-up page | GA4 |
| `blog_read` | Blog post viewed for > 30 seconds | GA4 |
| `feature_tab_click` | Screenshot carousel tab clicked | GA4 |

### Facebook Ads Integration

For running Facebook ads targeting Philippine car wash owners:
- Install Meta Pixel via `next/script` (afterInteractive strategy)
- Fire `Lead` event on demo form submission
- Fire `ViewContent` event on pricing page view
- Set up custom audiences based on landing page visitors

---

## Content Calendar (First 3 Months)

| Week | Blog Post | Social Media |
|---|---|---|
| Launch | "Introducing SplashSphere" | Launch announcement on FB + IG |
| Week 2 | "How to Calculate Commissions" | Commission calculator infographic |
| Week 4 | "The Real Cost of Running a Car Wash" | Cost breakdown carousel post |
| Week 6 | "GCash for Car Wash" | GCash integration demo video |
| Week 8 | "5 Signs You Need a POS" | Pain point carousel post |
| Week 10 | "Weekly Payroll Guide" | Payroll walkthrough reel |
| Week 12 | Customer case study #1 | Customer testimonial video |

---

## Launch Checklist (Marketing-Specific)

- [ ] Domain `splashsphere.ph` is registered and DNS is configured
- [ ] SSL certificate active on all subdomains
- [ ] Google Analytics 4 property created and tracking code installed
- [ ] Meta Pixel installed and verified
- [ ] Google Search Console connected and sitemap submitted
- [ ] Open Graph image (1200×630) created and serving correctly on social shares
- [ ] Contact form sends emails successfully (test with real submission)
- [ ] All internal links work (no 404s)
- [ ] Mobile responsiveness tested on iPhone, Android, and tablet
- [ ] Page speed: Lighthouse score > 90 on Performance
- [ ] Blog has at least 2 published posts for SEO
- [ ] Facebook page created: facebook.com/splashsphere
- [ ] Instagram account created: @splashsphere.ph
- [ ] Google Business Profile created (for local SEO)
- [ ] Privacy Policy includes RA 10173 compliance language
