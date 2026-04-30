'use client'

import { motion } from 'framer-motion'
import {
  Monitor,
  Users,
  ListOrdered,
  Settings,
  Heart,
  CheckCircle,
  ArrowRight,
  Package,
  Wrench,
  Building2,
  ShieldCheck,
  Receipt,
  Smartphone,
  MonitorPlay,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import Link from 'next/link'
import type { LucideIcon } from 'lucide-react'

/* ═══════════════════════════════════════════════════════════════════════════
   Animation Variants
   ═══════════════════════════════════════════════════════════════════════ */

const fadeUp = {
  hidden: { opacity: 0, y: 32 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.55, ease: 'easeOut' } },
}

const staggerContainer = {
  hidden: {},
  visible: {
    transition: { staggerChildren: 0.1 },
  },
}

/* ═══════════════════════════════════════════════════════════════════════════
   Feature Category Data
   ═══════════════════════════════════════════════════════════════════════ */

interface FeatureCategory {
  icon: LucideIcon
  title: string
  description: string
  features: string[]
  imagePlaceholder: string
}

const categories: FeatureCategory[] = [
  {
    icon: Monitor,
    title: 'POS & Transactions',
    description:
      'A point-of-sale system designed for car wash speed. Handle walk-ins and queued customers with dynamic pricing that adjusts for vehicle type, size, and time of day.',
    features: [
      'Dynamic pricing matrix (vehicle type x size)',
      'Service packages with bundle discounts',
      'Multiple payment methods — cash, card, bank transfer, and manual e-wallet (GCash / Maya) tagging with reference number',
      'Split payment support across multiple methods',
      'Branded receipts — 80mm thermal print and PDF',
      'Email receipt sent automatically when customer has email on file',
      'Auto-lock POS with 4–6 digit PIN unlock',
      'Daily transaction summary',
    ],
    imagePlaceholder: 'POS Transaction Screen',
  },
  {
    icon: Users,
    title: 'Employee & Payroll',
    description:
      'Commission-based pay is the backbone of Philippine car wash operations. SplashSphere calculates every split automatically and cuts payroll weekly — no spreadsheets needed.',
    features: [
      'Commission tracking (percentage, fixed, hybrid)',
      'Equal commission splitting among assigned employees',
      'Weekly payroll with automatic calculation',
      'Cash advance management with auto-deduction',
      'Attendance tracking (clock in/out)',
      'Employee performance reports',
    ],
    imagePlaceholder: 'Payroll Dashboard',
  },
  {
    icon: ListOrdered,
    title: 'Queue & Online Booking',
    description:
      'Keep your bays full and your customers informed. A digital queue board with priority levels, estimated wait times, a public TV display for your waiting area — and online bookings that flow directly into the queue at their reserved slot.',
    features: [
      'Digital queue with priority order (VIP > Booked > Express > Regular)',
      'Public queue display for wall-mounted TV (no auth required)',
      'Online booking surfaces in the customer Connect app',
      'Bookings auto-enqueue at their slot with Booked priority',
      '5-minute no-show auto-handling',
      'Real-time updates across all terminals via SignalR',
      'Estimated wait time calculation',
    ],
    imagePlaceholder: 'Queue Board View',
  },
  {
    icon: Settings,
    title: 'Operations',
    description:
      'Run one branch or ten. Manage shifts, track expenses, monitor inventory, and get profit-and-loss + cost-per-wash reports — all from a single dashboard.',
    features: [
      'Multi-branch management with branch switcher',
      'Cashier shift management (opening fund, cash movements, denomination count)',
      'End-of-day reporting with printable summary',
      'Expense tracking with profit & loss dashboard',
      'Cost-per-wash reports — driven by per-service supply usage',
      'Merchandise inventory with low-stock alerts',
      'Pricing modifiers (peak hours, weekends, holidays, promotions, weather)',
    ],
    imagePlaceholder: 'Operations Dashboard',
  },
  {
    icon: Receipt,
    title: 'Receipt Designer & Branding',
    description:
      'Make every receipt look like yours. Upload a logo, set your tagline, and toggle exactly what appears on the printed receipt and the email — from line-item employees to thank-you messages to GCash payment info.',
    features: [
      'Company profile with structured address, TIN, business registration, social handles',
      'Logo upload — auto-resized to 500/200/80 px PNG variants for different surfaces',
      'Receipt designer with ~30 toggles across header, body, customer, footer, format',
      'Custom thank-you message and promo text per tenant',
      '58mm and 80mm thermal paper widths with adjustable font size',
      'Live thermal-receipt preview that updates as you toggle',
      'Digital email receipts auto-sent on completion (Growth+)',
      'Per-branch receipt overrides for franchises and multi-branch operators (Enterprise)',
    ],
    imagePlaceholder: 'Receipt Designer',
  },
  {
    icon: MonitorPlay,
    title: 'Customer-Facing Display',
    description:
      'A second screen that builds trust at the counter. Pair any tablet, monitor, or smart TV to a POS station and customers watch their bill build in real time as the cashier rings them up — no surprises, no disputes, just clean transparency.',
    features: [
      'Three live states: Idle (branding + rotating promos), Building (live cart), Complete (payment + loyalty)',
      'SignalR-driven — every line item, discount, and total updates instantly',
      'Three themes (Dark, Light, Brand) with per-tenant primary color',
      'Up to 20 rotating promo messages on the idle screen',
      'Loyalty points earned + new balance shown on completion',
      'Auto-reverts to Idle after payment so the next customer sees a fresh screen',
      'Multiple POS stations per branch with one display each (Growth: 3, Enterprise: unlimited)',
      'Per-branch display overrides for multi-branch operators (Enterprise)',
    ],
    imagePlaceholder: 'Customer Display',
  },
  {
    icon: Heart,
    title: 'Customer Management & Loyalty',
    description:
      'Build lasting relationships with your customers. Track their vehicles, reward their loyalty, and bring them back with tier-based perks, point rewards, and a referral program that pays both the referrer and the new customer.',
    features: [
      'Customer database with vehicle registry',
      'Plate number quick lookup at the POS',
      'Loyalty points auto-awarded on transaction completion',
      'Tier membership (Bronze → Silver → Gold → Platinum) with per-tier multipliers',
      'Membership cards with optional auto-enrollment',
      'Referral program — both the referrer and referee earn points on the referee\'s first wash',
      'Service history per customer / vehicle',
      'SMS notifications on transaction completion',
    ],
    imagePlaceholder: 'Customer Profile',
  },
  {
    icon: Smartphone,
    title: 'Customer Connect App',
    description:
      'A customer-facing companion app where your customers discover nearby car washes, book a slot, track their queue position live, and check their loyalty balance — across every SplashSphere-powered shop they visit.',
    features: [
      'One customer profile across every SplashSphere wash they\'ve been to',
      'Phone OTP sign-in — no passwords',
      'Discover nearby branches with auto-nearest detection',
      'Book a slot online; vehicle classification confirmed by the cashier on arrival',
      'Live queue tracking once the booking is checked in',
      'Wash history across all branches the customer has visited',
      'Loyalty points + tier membership per car wash',
      'Apply a referral code to share rewards with whoever invited them',
    ],
    imagePlaceholder: 'Customer Connect App',
  },
  {
    icon: Package,
    title: 'Supplies & Inventory',
    description:
      'Track every bottle of shampoo, wax, and microfiber cloth. Auto-deduct supplies when transactions complete, monitor stock movements, and reorder through purchase orders with weighted-average costing.',
    features: [
      'Supply catalog with categories and suppliers',
      'Per-service supply usage by vehicle size (auto-deduct on completion)',
      'Full stock movement audit trail',
      'Purchase orders: Draft → Sent → Received lifecycle',
      'Weighted-average unit cost updated on receiving',
      'Low-stock warnings and negative-stock alerts',
      'Merchandise inventory (retail add-ons) with low-stock alerts',
    ],
    imagePlaceholder: 'Supplies & Purchase Orders',
  },
  {
    icon: Wrench,
    title: 'Equipment Maintenance',
    description:
      'Never lose a bay to a surprise breakdown. Register your pressure washers, vacuums, and blowers, schedule maintenance, and get flagged when service is overdue.',
    features: [
      'Equipment register per branch',
      'Status lifecycle: Operational → Needs Maintenance → Under Repair',
      'Maintenance log history',
      'Daily overdue-maintenance flagging (automatic)',
      'Last-serviced and next-due tracking',
    ],
    imagePlaceholder: 'Equipment Register & Maintenance Log',
  },
  {
    icon: Building2,
    title: 'Franchise Management',
    description:
      'Built for multi-franchise operators. Onboard franchisees with invitation links, push service templates, track royalties, and let each franchisee run their own independent subscription.',
    features: [
      'Franchisee onboarding via invitation link',
      'Service templates pushed from franchisor to franchisees',
      'Royalty period tracking and reporting',
      'Each franchisee tenant pays an independent subscription',
      'Enterprise plan feature',
    ],
    imagePlaceholder: 'Franchise Dashboard',
  },
  {
    icon: ShieldCheck,
    title: 'Security & Access',
    description:
      'Keep your terminals safe on a busy floor. PIN-protected POS lock screen, BCrypt-hashed credentials, and full audit logging across every sensitive action.',
    features: [
      'POS auto-lock after configurable inactivity',
      'Manual lock button for quick hand-off',
      '4–6 digit PIN unlock with attempt throttling',
      'Admin-only PIN provisioning (BCrypt-hashed)',
      'Tenant-isolated data with per-branch access control',
      'Audit log of sensitive actions',
    ],
    imagePlaceholder: 'POS Lock Screen',
  },
]

/* ═══════════════════════════════════════════════════════════════════════════
   Hero Section
   ═══════════════════════════════════════════════════════════════════════ */

function FeaturesHero() {
  return (
    <section className="relative overflow-hidden bg-gradient-to-br from-splash-900 to-splash-800 py-20 md:py-28">
      {/* Decorative blobs */}
      <div className="pointer-events-none absolute -left-32 -top-32 h-[400px] w-[400px] rounded-full bg-splash-500/10 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-24 -right-24 h-[350px] w-[350px] rounded-full bg-aqua-400/10 blur-3xl" />

      <div className="relative mx-auto max-w-6xl px-6 text-center">
        <motion.h1
          variants={fadeUp}
          initial="hidden"
          animate="visible"
          className="text-4xl font-extrabold leading-tight tracking-tight text-white md:text-5xl lg:text-6xl"
        >
          Everything you need to run{' '}
          <span className="bg-gradient-to-r from-aqua-400 to-splash-300 bg-clip-text text-transparent">
            your car wash
          </span>
        </motion.h1>
        <motion.p
          variants={fadeUp}
          initial="hidden"
          animate="visible"
          className="mx-auto mt-6 max-w-2xl text-lg leading-relaxed text-white/70 md:text-xl"
        >
          From POS to payroll, SplashSphere covers every aspect of Philippine car wash operations.
        </motion.p>
      </div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   Feature Category Section
   ═══════════════════════════════════════════════════════════════════════ */

function FeatureCategorySection({
  category,
  index,
}: {
  category: FeatureCategory
  index: number
}) {
  const isEven = index % 2 === 0
  const Icon = category.icon

  return (
    <section className={cn('py-20', isEven ? 'bg-white' : 'bg-gray-50')}>
      <div className="mx-auto max-w-6xl px-6">
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-80px' }}
          className={cn(
            'grid items-center gap-12 md:grid-cols-2 md:gap-16',
            !isEven && 'md:[&>*:first-child]:order-2'
          )}
        >
          {/* Text column */}
          <motion.div variants={fadeUp}>
            <div className="mb-4 inline-flex items-center justify-center rounded-xl bg-splash-100 p-3">
              <Icon className="h-6 w-6 text-splash-600" />
            </div>
            <h2 className="text-3xl font-extrabold tracking-tight text-foreground md:text-4xl">
              {category.title}
            </h2>
            <p className="mt-4 text-lg leading-relaxed text-muted-foreground">
              {category.description}
            </p>
            <ul className="mt-8 space-y-3">
              {category.features.map((feature) => (
                <li key={feature} className="flex items-start gap-3">
                  <CheckCircle className="mt-0.5 h-5 w-5 shrink-0 text-aqua-500" />
                  <span className="text-foreground">{feature}</span>
                </li>
              ))}
            </ul>
          </motion.div>

          {/* Placeholder image column */}
          <motion.div variants={fadeUp}>
            <div className="flex aspect-[4/3] items-center justify-center rounded-2xl bg-gradient-to-br from-splash-100 via-splash-50 to-aqua-400/10 shadow-lg ring-1 ring-splash-200/50">
              <div className="text-center">
                <div className="mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-xl bg-splash-200/60">
                  <Icon className="h-7 w-7 text-splash-700" />
                </div>
                <p className="text-sm font-medium text-splash-700">
                  {category.imagePlaceholder}
                </p>
                <p className="mt-1 text-xs text-splash-500">Screenshot coming soon</p>
              </div>
            </div>
          </motion.div>
        </motion.div>
      </div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   Bottom CTA Section
   ═══════════════════════════════════════════════════════════════════════ */

function BottomCTA() {
  return (
    <section className="relative overflow-hidden bg-splash-900 py-20 md:py-24">
      <div className="pointer-events-none absolute -right-32 -top-32 h-[400px] w-[400px] rounded-full bg-splash-500/10 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-24 -left-24 h-[300px] w-[300px] rounded-full bg-aqua-400/10 blur-3xl" />

      <motion.div
        variants={staggerContainer}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        className="relative mx-auto max-w-6xl px-6 text-center"
      >
        <motion.h2
          variants={fadeUp}
          className="text-3xl font-extrabold tracking-tight text-white md:text-4xl"
        >
          Ready to see it in action?
        </motion.h2>
        <motion.p
          variants={fadeUp}
          className="mx-auto mt-4 max-w-xl text-lg text-white/70"
        >
          Join car wash businesses across the Philippines already using SplashSphere to streamline their operations.
        </motion.p>
        <motion.div
          variants={fadeUp}
          className="mt-10 flex flex-col items-center justify-center gap-4 sm:flex-row"
        >
          <Link
            href="/sign-up"
            className="inline-flex h-12 items-center gap-2 rounded-xl bg-aqua-400 px-8 text-base font-semibold text-splash-950 shadow-lg transition hover:bg-aqua-500 hover:shadow-xl"
          >
            Start Free Trial
            <ArrowRight className="h-4 w-4" />
          </Link>
          <Link
            href="/contact"
            className="inline-flex h-12 items-center gap-2 rounded-xl border border-white/20 bg-white/5 px-8 text-base font-semibold text-white backdrop-blur transition hover:bg-white/10"
          >
            Book a Demo
          </Link>
        </motion.div>
      </motion.div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   Page Component
   ═══════════════════════════════════════════════════════════════════════ */

export default function FeaturesPage() {
  return (
    <>
      <FeaturesHero />
      {categories.map((category, index) => (
        <FeatureCategorySection key={category.title} category={category} index={index} />
      ))}
      <BottomCTA />
    </>
  )
}
