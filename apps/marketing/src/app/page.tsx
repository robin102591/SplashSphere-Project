'use client'

import { useState, useEffect, useCallback } from 'react'
import { motion } from 'framer-motion'
import {
  NotebookPen,
  TrendingDown,
  MapPin,
  Monitor,
  Coins,
  ClipboardList,
  Users,
  BarChart3,
  FileText,
  Star,
  CheckCircle,
  Smartphone,
  Building2,
  CalendarCheck,
  Activity,
  ArrowRight,
  Sparkles,
  Shield,
  CreditCard,
  ChevronDown,
  Wrench,
} from 'lucide-react'
import { cn } from '@/lib/utils'

/* ═══════════════════════════════════════════════════════════════════════════
   Animation Variants
   ═══════════════════════════════════════════════════════════════════════ */

const fadeUp = {
  hidden: { opacity: 0, y: 24 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.5, ease: 'easeOut' } },
}

const staggerContainer = {
  hidden: {},
  visible: {
    transition: { staggerChildren: 0.12 },
  },
}

const scaleIn = {
  hidden: { opacity: 0, scale: 0.95 },
  visible: { opacity: 1, scale: 1, transition: { duration: 0.5, ease: 'easeOut' } },
}

/* ═══════════════════════════════════════════════════════════════════════════
   Section Wrapper
   ═══════════════════════════════════════════════════════════════════════ */

function Section({
  children,
  className,
  id,
}: {
  children: React.ReactNode
  className?: string
  id?: string
}) {
  return (
    <section id={id} className={cn('py-20 md:py-24', className)}>
      <div className="mx-auto max-w-6xl px-6">{children}</div>
    </section>
  )
}

function SectionHeader({
  title,
  subtitle,
  className,
}: {
  title: string
  subtitle?: string
  className?: string
}) {
  return (
    <motion.div
      variants={fadeUp}
      initial="hidden"
      whileInView="visible"
      viewport={{ once: true }}
      className={cn('mb-14 text-center', className)}
    >
      <h2 className="text-3xl font-extrabold tracking-tight text-foreground md:text-4xl">
        {title}
      </h2>
      {subtitle && (
        <p className="mx-auto mt-4 max-w-2xl text-lg text-muted-foreground">{subtitle}</p>
      )}
    </motion.div>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   1. HERO
   ═══════════════════════════════════════════════════════════════════════ */

const trustBadges = [
  { icon: Smartphone, label: 'Multi-Payment Tracking' },
  { icon: Building2, label: 'Multi-Branch' },
  { icon: CalendarCheck, label: 'Weekly Payroll' },
  { icon: Activity, label: 'Real-Time Dashboard' },
]

function HeroSection() {
  return (
    <section className="relative min-h-screen overflow-hidden bg-gradient-to-br from-splash-900 via-splash-800 to-splash-950">
      {/* Decorative blobs */}
      <div className="pointer-events-none absolute -left-40 -top-40 h-[500px] w-[500px] rounded-full bg-splash-500/10 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-32 -right-32 h-[400px] w-[400px] rounded-full bg-aqua-400/10 blur-3xl" />

      <div className="relative mx-auto flex min-h-screen max-w-6xl flex-col items-center justify-center gap-12 px-6 py-24 lg:flex-row lg:gap-16">
        {/* Left column — text (55%) */}
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          animate="visible"
          className="flex-[55] text-center lg:text-left"
        >
          <motion.div variants={fadeUp} className="mb-4 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/5 px-4 py-1.5 text-sm text-white/70 backdrop-blur">
            <Sparkles className="h-4 w-4 text-aqua-400" />
            Built for Philippine car wash businesses
          </motion.div>

          <motion.h1
            variants={fadeUp}
            className="text-4xl font-extrabold leading-tight tracking-tight text-white md:text-5xl lg:text-6xl"
          >
            Manage your car wash business{' '}
            <span className="bg-gradient-to-r from-aqua-400 to-splash-300 bg-clip-text text-transparent">
              the smart way
            </span>
          </motion.h1>

          <motion.p variants={fadeUp} className="mt-6 max-w-xl text-lg leading-relaxed text-white/70">
            SplashSphere handles commissions, payroll, queueing, and daily reporting — so you can
            focus on growing your business, not chasing paper receipts.
          </motion.p>

          <motion.div variants={fadeUp} className="mt-8 flex flex-col gap-4 sm:flex-row sm:justify-center lg:justify-start">
            <a
              href="#pricing"
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-splash-500 px-6 py-3 text-base font-semibold text-white shadow-lg shadow-splash-500/25 transition hover:bg-splash-400"
            >
              Start Free Trial
              <ArrowRight className="h-4 w-4" />
            </a>
            <a
              href="#demo"
              className="inline-flex items-center justify-center gap-2 rounded-lg border border-white/30 px-6 py-3 text-base font-semibold text-white transition hover:bg-white/10"
            >
              Book a Demo
            </a>
          </motion.div>

          {/* Trust bar */}
          <motion.div variants={fadeUp} className="mt-12">
            <p className="mb-4 text-sm font-medium text-white/50">
              Trusted by 50+ car wash businesses across the Philippines
            </p>
            <div className="flex flex-wrap items-center justify-center gap-6 lg:justify-start">
              {trustBadges.map((badge) => (
                <div key={badge.label} className="flex items-center gap-2 text-sm text-white/60">
                  <badge.icon className="h-4 w-4 text-aqua-400" />
                  {badge.label}
                </div>
              ))}
            </div>
          </motion.div>
        </motion.div>

        {/* Right column — mockup placeholder (45%) */}
        <motion.div
          variants={scaleIn}
          initial="hidden"
          animate="visible"
          className="flex-[45]"
        >
          <div className="relative overflow-hidden rounded-2xl border border-white/10 bg-gradient-to-br from-splash-700/40 to-splash-900/60 p-1 shadow-2xl shadow-splash-950/50 backdrop-blur">
            <div className="rounded-xl bg-splash-950/80 p-8">
              {/* Fake browser chrome */}
              <div className="mb-6 flex items-center gap-2">
                <div className="h-3 w-3 rounded-full bg-red-400/60" />
                <div className="h-3 w-3 rounded-full bg-yellow-400/60" />
                <div className="h-3 w-3 rounded-full bg-green-400/60" />
                <div className="ml-4 h-6 flex-1 rounded-md bg-white/5" />
              </div>
              {/* Placeholder content */}
              <div className="flex aspect-video items-center justify-center rounded-lg bg-gradient-to-br from-splash-600/30 to-aqua-500/20">
                <div className="text-center">
                  <Monitor className="mx-auto mb-3 h-12 w-12 text-aqua-400/60" />
                  <p className="text-lg font-semibold text-white/50">Dashboard Preview</p>
                  <p className="mt-1 text-sm text-white/30">Real-time business insights</p>
                </div>
              </div>
            </div>
          </div>
        </motion.div>
      </div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   2. PAIN POINTS
   ═══════════════════════════════════════════════════════════════════════ */

const painPoints = [
  {
    icon: NotebookPen,
    title: 'Manual commission tracking',
    description:
      'Calculating split commissions by hand every week is slow, error-prone, and leads to arguments. Your top washers deserve accurate pay.',
  },
  {
    icon: TrendingDown,
    title: 'No visibility into daily revenue',
    description:
      'Cash reconciliation at the end of the day is a guessing game. You never know your real numbers until it is too late.',
  },
  {
    icon: MapPin,
    title: 'Multiple branches, zero oversight',
    description:
      'Managing branches across different locations with separate systems means duplicated work, inconsistent pricing, and data you cannot trust.',
  },
]

function PainPointsSection() {
  return (
    <Section id="problems">
      <SectionHeader title="Running a car wash shouldn't mean drowning in spreadsheets" />

      <motion.div
        variants={staggerContainer}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        className="grid gap-8 md:grid-cols-3"
      >
        {painPoints.map((point) => (
          <motion.div
            key={point.title}
            variants={fadeUp}
            className="rounded-xl border bg-white p-8 shadow-sm"
          >
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-lg bg-red-50">
              <point.icon className="h-6 w-6 text-red-500" />
            </div>
            <h3 className="mb-2 text-lg font-bold text-foreground">{point.title}</h3>
            <p className="leading-relaxed text-muted-foreground">{point.description}</p>
          </motion.div>
        ))}
      </motion.div>
    </Section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   3. SOLUTION OVERVIEW
   ═══════════════════════════════════════════════════════════════════════ */

const features = [
  {
    icon: Monitor,
    title: 'POS Terminal',
    description:
      'Touch-friendly point of sale with vehicle lookup, service selection, and multi-payment support. Built for wet hands and fast queues.',
  },
  {
    icon: Coins,
    title: 'Auto Commissions',
    description:
      'Define commission matrices per service, vehicle type, and size. Split calculations happen instantly on every transaction.',
  },
  {
    icon: ClipboardList,
    title: 'Queue Management',
    description:
      'Visual queue board with priority lanes, no-show detection, and a public display for your wall-mounted TV.',
  },
  {
    icon: Users,
    title: 'Employee & Payroll',
    description:
      'Weekly payroll with commission and daily-rate support, attendance tracking, cash advance deductions, and payslip generation.',
  },
  {
    icon: BarChart3,
    title: 'Analytics & Reports',
    description:
      'Revenue trends, peak-hour heatmaps, employee performance rankings, and customer analytics. Export to CSV anytime.',
  },
  {
    icon: FileText,
    title: 'Receipt & Invoicing',
    description:
      'Thermal-printer-ready receipts in 80mm format. PDF downloads, auto-print on transaction complete, and customer SMS option.',
  },
  {
    icon: ClipboardList,
    title: 'Supplies & Purchase Orders',
    description:
      'Track soap, wax, and microfiber stock with auto-deduction per service and vehicle size. Draft purchase orders, receive stock, and update weighted-average cost automatically.',
  },
  {
    icon: Wrench,
    title: 'Equipment & Maintenance',
    description:
      'Register pressure washers, vacuums, and blowers. Log maintenance, get overdue alerts, and track equipment status from Operational to Under Repair.',
  },
  {
    icon: Building2,
    title: 'Franchise Management',
    description:
      'For Enterprise operators — onboard franchisees with invitations, royalty tracking, and per-franchisee subscription billing. Each franchisee runs independently.',
  },
  {
    icon: Shield,
    title: 'POS Lock & PIN',
    description:
      'Auto-lock the POS after inactivity. Cashiers unlock with a 4–6 digit PIN, and admin-only PIN provisioning keeps your terminal secure on a shared floor.',
  },
]

function SolutionSection() {
  return (
    <Section className="bg-gray-50" id="features">
      <SectionHeader
        title="One platform for everything"
        subtitle="From the moment a car arrives to the day you cut payroll, SplashSphere has you covered."
      />

      <motion.div
        variants={staggerContainer}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3"
      >
        {features.map((feature) => (
          <motion.div
            key={feature.title}
            variants={fadeUp}
            className="rounded-xl bg-white p-6 shadow-sm transition hover:shadow-md"
          >
            <div className="mb-4 flex h-10 w-10 items-center justify-center rounded-lg bg-splash-50">
              <feature.icon className="h-5 w-5 text-splash-600" />
            </div>
            <h3 className="mb-2 text-base font-bold text-foreground">{feature.title}</h3>
            <p className="text-sm leading-relaxed text-muted-foreground">{feature.description}</p>
          </motion.div>
        ))}
      </motion.div>
    </Section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   4. PRODUCT SCREENSHOTS (tabbed, auto-rotate)
   ═══════════════════════════════════════════════════════════════════════ */

const screenshotTabs = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    gradient: 'from-splash-600 to-splash-800',
    heading: 'Real-Time Dashboard',
    description: 'Revenue, transactions, and employee performance at a glance.',
  },
  {
    id: 'pos',
    label: 'POS Transaction',
    gradient: 'from-aqua-500 to-splash-700',
    heading: 'POS Transaction Screen',
    description: 'Touch-optimized service selection with live running totals.',
  },
  {
    id: 'queue',
    label: 'Queue Board',
    gradient: 'from-splash-500 to-aqua-400',
    heading: 'Queue Board',
    description: 'Kanban-style queue with waiting, called, and in-service columns.',
  },
  {
    id: 'payroll',
    label: 'Payroll',
    gradient: 'from-splash-800 to-splash-500',
    heading: 'Payroll Processing',
    description: 'Weekly payroll with commission breakdowns and one-click release.',
  },
]

function ScreenshotSection() {
  const [activeTab, setActiveTab] = useState(0)

  const nextTab = useCallback(() => {
    setActiveTab((prev) => (prev + 1) % screenshotTabs.length)
  }, [])

  useEffect(() => {
    const interval = setInterval(nextTab, 5000)
    return () => clearInterval(interval)
  }, [nextTab])

  const current = screenshotTabs[activeTab]

  return (
    <Section id="screenshots">
      <SectionHeader title="See it in action" />

      {/* Tabs */}
      <div className="mb-8 flex flex-wrap justify-center gap-2">
        {screenshotTabs.map((tab, i) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(i)}
            className={cn(
              'rounded-full px-5 py-2 text-sm font-medium transition',
              i === activeTab
                ? 'bg-splash-500 text-white shadow-md'
                : 'bg-gray-100 text-muted-foreground hover:bg-gray-200'
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Screenshot placeholder */}
      <motion.div
        key={current.id}
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4 }}
        className="mx-auto max-w-4xl"
      >
        <div
          className={cn(
            'flex aspect-video items-center justify-center rounded-2xl bg-gradient-to-br shadow-xl',
            current.gradient
          )}
        >
          <div className="text-center">
            <h3 className="text-2xl font-bold text-white">{current.heading}</h3>
            <p className="mt-2 text-base text-white/70">{current.description}</p>
          </div>
        </div>
      </motion.div>

      {/* Progress dots */}
      <div className="mt-6 flex justify-center gap-2">
        {screenshotTabs.map((_, i) => (
          <button
            key={i}
            onClick={() => setActiveTab(i)}
            className={cn(
              'h-2 rounded-full transition-all',
              i === activeTab ? 'w-8 bg-splash-500' : 'w-2 bg-gray-300'
            )}
            aria-label={`Go to tab ${i + 1}`}
          />
        ))}
      </div>
    </Section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   5. HOW IT WORKS
   ═══════════════════════════════════════════════════════════════════════ */

const steps = [
  {
    number: 1,
    title: 'Sign Up',
    description: 'Create your account in under a minute. No credit card required.',
  },
  {
    number: 2,
    title: 'Set Up',
    description:
      'Add your branches, services, pricing, and employees. Our onboarding wizard guides you through everything.',
  },
  {
    number: 3,
    title: 'Start Washing',
    description:
      'Open your POS terminal and start processing cars. Commissions, payroll, and reports happen automatically.',
  },
]

function HowItWorksSection() {
  return (
    <Section className="bg-gray-50" id="how-it-works">
      <SectionHeader title="Up and running in 15 minutes" />

      <motion.div
        variants={staggerContainer}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        className="relative grid gap-12 md:grid-cols-3 md:gap-8"
      >
        {/* Dashed connecting line (desktop only) */}
        <div className="pointer-events-none absolute left-0 right-0 top-10 hidden h-0.5 border-t-2 border-dashed border-splash-200 md:block" />

        {steps.map((step) => (
          <motion.div key={step.number} variants={fadeUp} className="relative text-center">
            <div className="relative z-10 mx-auto mb-5 flex h-20 w-20 items-center justify-center rounded-full bg-splash-500 text-2xl font-extrabold text-white shadow-lg shadow-splash-500/25">
              {step.number}
            </div>
            <h3 className="mb-2 text-lg font-bold text-foreground">{step.title}</h3>
            <p className="mx-auto max-w-xs text-sm leading-relaxed text-muted-foreground">
              {step.description}
            </p>
          </motion.div>
        ))}
      </motion.div>
    </Section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   6. PRICING
   ═══════════════════════════════════════════════════════════════════════ */

const plans = [
  {
    name: 'Starter',
    price: '1,499',
    popular: false,
    features: [
      '1 branch',
      '5 employees',
      'POS terminal',
      'Commission tracking',
      'Weekly payroll',
      'Basic reports',
      'Email support',
    ],
  },
  {
    name: 'Growth',
    price: '2,999',
    popular: true,
    features: [
      '3 branches',
      '15 employees',
      'Everything in Starter',
      'Queue management',
      'Loyalty program',
      'Cash advance tracking',
      'Expense tracking',
      'Shift reports',
      'SMS notifications (50/mo)',
      'Priority support',
    ],
  },
  {
    name: 'Enterprise',
    price: '4,999',
    popular: false,
    features: [
      'Unlimited branches',
      'Unlimited employees',
      'Everything in Growth',
      'API access',
      'Custom integrations',
      'Dedicated account manager',
      'SLA guarantee',
      'SMS notifications (200/mo)',
    ],
  },
]

const faqItems = [
  {
    question: 'Is there a free trial?',
    answer:
      'Yes. Every new account gets a 14-day free trial on the Growth plan with full access to all features. No credit card required.',
  },
  {
    question: 'Can I change plans later?',
    answer:
      'Absolutely. Upgrade or downgrade anytime from your dashboard. Changes take effect on your next billing cycle.',
  },
  {
    question: 'Do you charge per transaction?',
    answer:
      'No. All plans include unlimited transactions. We believe in simple, predictable pricing.',
  },
  {
    question: 'What payment methods do you accept for my subscription?',
    answer:
      'We accept credit/debit cards and bank transfers for SplashSphere subscription billing. Inside your POS, you can record cash, card, e-wallet (GCash/Maya), and bank transfer payments from your customers — e-wallet payments are logged manually with a reference number for easy end-of-shift reconciliation.',
  },
  {
    question: 'Can I use SplashSphere on a tablet?',
    answer:
      'Yes. The POS terminal is a Progressive Web App optimized for tablets and touchscreens. Install it directly from your browser.',
  },
  {
    question: 'What happens to my data if I cancel?',
    answer:
      'Your data is retained for 30 days after cancellation. You can export all records as CSV before your account is deactivated.',
  },
]

function PricingSection() {
  return (
    <Section id="pricing">
      <SectionHeader
        title="Simple, honest pricing"
        subtitle="No hidden fees. No per-transaction charges. Cancel anytime."
      />

      <motion.div
        variants={staggerContainer}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        className="grid items-start gap-8 md:grid-cols-3"
      >
        {plans.map((plan) => (
          <motion.div
            key={plan.name}
            variants={fadeUp}
            className={cn(
              'relative rounded-2xl border bg-white p-8 shadow-sm',
              plan.popular && 'scale-105 ring-2 ring-splash-500 shadow-lg'
            )}
          >
            {plan.popular && (
              <div className="absolute -top-3.5 left-1/2 -translate-x-1/2 rounded-full bg-splash-500 px-4 py-1 text-xs font-bold uppercase tracking-wider text-white">
                Most Popular
              </div>
            )}

            <h3 className="text-xl font-bold text-foreground">{plan.name}</h3>
            <div className="mt-4 flex items-baseline gap-1">
              <span className="money text-4xl font-extrabold text-foreground">
                &#8369;{plan.price}
              </span>
              <span className="text-muted-foreground">/mo</span>
            </div>

            <ul className="mt-6 space-y-3">
              {plan.features.map((feature) => (
                <li key={feature} className="flex items-start gap-2 text-sm text-muted-foreground">
                  <CheckCircle className="mt-0.5 h-4 w-4 shrink-0 text-splash-500" />
                  {feature}
                </li>
              ))}
            </ul>

            <a
              href="#"
              className={cn(
                'mt-8 block w-full rounded-lg py-3 text-center text-sm font-semibold transition',
                plan.popular
                  ? 'bg-splash-500 text-white shadow-md hover:bg-splash-400'
                  : 'border border-splash-200 text-splash-600 hover:bg-splash-50'
              )}
            >
              Start Free Trial
            </a>
          </motion.div>
        ))}
      </motion.div>

      {/* FAQ */}
      <motion.div
        variants={fadeUp}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        className="mx-auto mt-20 max-w-2xl"
      >
        <h3 className="mb-8 text-center text-2xl font-bold text-foreground">
          Frequently asked questions
        </h3>
        <div className="space-y-3">
          {faqItems.map((item) => (
            <details
              key={item.question}
              className="group rounded-xl border bg-white px-6 py-4 shadow-sm"
            >
              <summary className="flex cursor-pointer items-center justify-between text-base font-semibold text-foreground">
                {item.question}
                <ChevronDown className="h-5 w-5 shrink-0 text-muted-foreground transition group-open:rotate-180" />
              </summary>
              <p className="mt-3 pb-1 text-sm leading-relaxed text-muted-foreground">
                {item.answer}
              </p>
            </details>
          ))}
        </div>
      </motion.div>
    </Section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   7. TESTIMONIALS
   ═══════════════════════════════════════════════════════════════════════ */

const testimonials = [
  {
    quote:
      'Before SplashSphere, payroll took my wife and me an entire evening every week. Now it takes five minutes. The commission tracking alone paid for the subscription.',
    name: 'Ricardo Santos',
    title: 'Owner',
    location: 'SparkleWash Makati',
  },
  {
    quote:
      'My staff loves the queue board. Customers see their number on the TV and wait calmly instead of crowding the cashier. It changed the entire vibe of our shop.',
    name: 'Maria Garcia',
    title: 'Manager',
    location: 'AquaShine BGC',
  },
  {
    quote:
      'I opened my second branch in Antipolo and had it running on SplashSphere within an hour. Seeing both branches on one dashboard is a game changer.',
    name: 'Jun Reyes',
    title: 'Owner',
    location: 'CleanRide Auto Spa',
  },
]

function TestimonialsSection() {
  return (
    <Section className="bg-gray-50" id="testimonials">
      <SectionHeader title="Loved by car wash owners" />

      <motion.div
        variants={staggerContainer}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        className="grid gap-8 md:grid-cols-3"
      >
        {testimonials.map((t) => (
          <motion.div
            key={t.name}
            variants={fadeUp}
            className="rounded-xl border bg-white p-8 shadow-sm"
          >
            {/* Stars */}
            <div className="mb-4 flex gap-1">
              {Array.from({ length: 5 }).map((_, i) => (
                <Star key={i} className="h-4 w-4 fill-yellow-400 text-yellow-400" />
              ))}
            </div>
            <blockquote className="mb-6 text-sm leading-relaxed text-muted-foreground">
              &ldquo;{t.quote}&rdquo;
            </blockquote>
            <div>
              <p className="font-semibold text-foreground">{t.name}</p>
              <p className="text-sm text-muted-foreground">
                {t.title}, {t.location}
              </p>
            </div>
          </motion.div>
        ))}
      </motion.div>
    </Section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   8. FINAL CTA
   ═══════════════════════════════════════════════════════════════════════ */

const ctaBadges = [
  { icon: Shield, label: '14-day free trial' },
  { icon: CreditCard, label: 'No credit card required' },
  { icon: CheckCircle, label: 'Cancel anytime' },
  { icon: Smartphone, label: 'Multi-payment tracking' },
]

function FinalCtaSection() {
  return (
    <section className="relative overflow-hidden bg-splash-900 py-20 md:py-24">
      {/* Decorative */}
      <div className="pointer-events-none absolute -right-32 -top-32 h-[400px] w-[400px] rounded-full bg-splash-500/10 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-32 -left-32 h-[300px] w-[300px] rounded-full bg-aqua-400/10 blur-3xl" />

      <motion.div
        variants={staggerContainer}
        initial="hidden"
        whileInView="visible"
        viewport={{ once: true }}
        className="relative mx-auto max-w-3xl px-6 text-center"
      >
        <motion.h2
          variants={fadeUp}
          className="text-3xl font-extrabold tracking-tight text-white md:text-4xl"
        >
          Ready to modernize your car wash?
        </motion.h2>

        <motion.p variants={fadeUp} className="mx-auto mt-4 max-w-xl text-lg text-white/70">
          Join hundreds of car wash owners who switched from spreadsheets to SplashSphere. Set up
          your first branch in under 15 minutes.
        </motion.p>

        <motion.div
          variants={fadeUp}
          className="mt-8 flex flex-col justify-center gap-4 sm:flex-row"
        >
          <a
            href="#pricing"
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-splash-500 px-8 py-3.5 text-base font-semibold text-white shadow-lg shadow-splash-500/25 transition hover:bg-splash-400"
          >
            Start Free Trial
            <ArrowRight className="h-4 w-4" />
          </a>
          <a
            href="#demo"
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-white/30 px-8 py-3.5 text-base font-semibold text-white transition hover:bg-white/10"
          >
            Book a Demo
          </a>
        </motion.div>

        <motion.div
          variants={fadeUp}
          className="mt-10 flex flex-wrap items-center justify-center gap-6"
        >
          {ctaBadges.map((badge) => (
            <div key={badge.label} className="flex items-center gap-2 text-sm text-white/60">
              <badge.icon className="h-4 w-4 text-aqua-400" />
              {badge.label}
            </div>
          ))}
        </motion.div>
      </motion.div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   PAGE COMPONENT
   ═══════════════════════════════════════════════════════════════════════ */

export default function LandingPage() {
  return (
    <>
      <HeroSection />
      <PainPointsSection />
      <SolutionSection />
      <ScreenshotSection />
      <HowItWorksSection />
      <PricingSection />
      <TestimonialsSection />
      <FinalCtaSection />
    </>
  )
}
