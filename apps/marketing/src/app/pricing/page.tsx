'use client'

import { motion } from 'framer-motion'
import { CheckCircle, Minus, ArrowRight, ChevronDown, Shield, CreditCard, Smartphone } from 'lucide-react'
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

/* ═══════════════════════════════════════════════════════════════════════════
   Data
   ═══════════════════════════════════════════════════════════════════════ */

const plans = [
  {
    name: 'Starter',
    price: '1,499',
    popular: false,
    cta: 'Start Free Trial',
    ctaHref: '#',
    features: [
      '1 branch',
      'Up to 5 employees',
      'POS & transactions',
      'Commission tracking',
      'Weekly payroll',
      'Basic reports',
      'Branded receipts (logo upload + designer toggles)',
      'Supply tracking',
      'Email support',
    ],
  },
  {
    name: 'Growth',
    price: '2,999',
    popular: true,
    cta: 'Start Free Trial',
    ctaHref: '#',
    features: [
      'Up to 3 branches',
      'Up to 15 employees',
      'Everything in Starter',
      'Queue management',
      'Online booking + Customer Connect listing',
      'Customer loyalty, membership tiers, & referral program',
      'Digital email receipts (auto-send on completion)',
      'Supplies inventory, auto-deduction, & purchase orders',
      'Equipment maintenance tracking',
      'Cash advance & expense tracking',
      'Cash reconciliation / shift reports',
      'POS lock screen & PIN unlock',
      'Pricing modifiers (peak / weekends / promotions)',
      'P&L + cost-per-wash reports',
      'SMS notifications (50/mo)',
      'Priority support',
    ],
  },
  {
    name: 'Enterprise',
    price: '4,999',
    popular: false,
    cta: 'Contact Sales',
    ctaHref: '#',
    features: [
      'Unlimited branches',
      'Unlimited employees',
      'Everything in Growth',
      'Per-branch receipt overrides',
      'Franchise management (invitations, royalties, templates)',
      'API access',
      'Custom integrations',
      'Dedicated support',
      'SLA & onboarding',
      'SMS notifications (200/mo)',
    ],
  },
]

type CellValue = true | false | string

interface ComparisonRow {
  feature: string
  starter: CellValue
  growth: CellValue
  enterprise: CellValue
}

const comparisonRows: ComparisonRow[] = [
  { feature: 'Branches', starter: '1', growth: '3', enterprise: 'Unlimited' },
  { feature: 'Employees', starter: '5', growth: '15', enterprise: 'Unlimited' },
  { feature: 'Transactions', starter: 'Unlimited', growth: 'Unlimited', enterprise: 'Unlimited' },
  { feature: 'POS Terminal', starter: true, growth: true, enterprise: true },
  { feature: 'Commission Tracking', starter: true, growth: true, enterprise: true },
  { feature: 'Weekly Payroll', starter: true, growth: true, enterprise: true },
  { feature: 'Basic Reports', starter: true, growth: true, enterprise: true },
  { feature: 'Receipt Designer + Logo Upload', starter: true, growth: true, enterprise: true },
  { feature: 'Supply Tracking', starter: true, growth: true, enterprise: true },
  { feature: 'POS Lock Screen & PIN', starter: true, growth: true, enterprise: true },
  { feature: 'Queue Management', starter: false, growth: true, enterprise: true },
  { feature: 'Online Booking + Connect Listing', starter: false, growth: true, enterprise: true },
  { feature: 'Customer Loyalty & Tiers', starter: false, growth: true, enterprise: true },
  { feature: 'Referral Program', starter: false, growth: true, enterprise: true },
  { feature: 'Digital Email Receipts', starter: false, growth: true, enterprise: true },
  { feature: 'Pricing Modifiers (Peak / Promos)', starter: false, growth: true, enterprise: true },
  { feature: 'Purchase Orders + Auto-Deduction', starter: false, growth: true, enterprise: true },
  { feature: 'Equipment Maintenance', starter: false, growth: true, enterprise: true },
  { feature: 'Cash Advance Tracking', starter: false, growth: true, enterprise: true },
  { feature: 'Expense Tracking', starter: false, growth: true, enterprise: true },
  { feature: 'Shift / Cash Reconciliation', starter: false, growth: true, enterprise: true },
  { feature: 'P&L + Cost-per-Wash Reports', starter: false, growth: true, enterprise: true },
  { feature: 'Branch Receipt Overrides', starter: false, growth: false, enterprise: true },
  { feature: 'Franchise Management', starter: false, growth: false, enterprise: true },
  { feature: 'SMS Notifications', starter: false, growth: '50/mo', enterprise: '200/mo' },
  { feature: 'API Access', starter: false, growth: false, enterprise: true },
  { feature: 'Custom Integrations', starter: false, growth: false, enterprise: true },
  { feature: 'Dedicated Support', starter: false, growth: false, enterprise: true },
  { feature: 'SLA', starter: false, growth: false, enterprise: true },
  { feature: 'Support', starter: 'Email', growth: 'Priority', enterprise: 'Dedicated' },
]

const faqItems = [
  {
    question: 'Can I switch plans later?',
    answer:
      'Yes. You can upgrade or downgrade anytime from your dashboard. Changes take effect on your next billing cycle.',
  },
  {
    question: 'Is there a setup fee?',
    answer:
      'No. Setup is completely free and takes about 15 minutes. Our onboarding wizard guides you through everything.',
  },
  {
    question: 'What payment methods do you accept for my subscription?',
    answer:
      'SplashSphere subscriptions are billed via credit/debit card and bank transfer. Inside your POS, your cashiers can record cash, card, bank transfer, and manually tagged e-wallet (GCash / Maya) payments from customers — each e-wallet entry captures a reference number for easy reconciliation at shift close.',
  },
  {
    question: 'Do you charge per transaction?',
    answer:
      'No. All plans include unlimited transactions. We believe in simple, predictable pricing.',
  },
  {
    question: 'Can I cancel anytime?',
    answer:
      'Yes. There are no lock-in contracts. Cancel from your dashboard and your subscription ends at the current billing period.',
  },
  {
    question: 'Is my data safe?',
    answer:
      'Yes. Your data is encrypted in transit and at rest, backed up daily, and hosted on secure cloud infrastructure with 99.9% uptime.',
  },
]

/* ═══════════════════════════════════════════════════════════════════════════
   Helper — render comparison cell
   ═══════════════════════════════════════════════════════════════════════ */

function ComparisonCell({ value }: { value: CellValue }) {
  if (value === true) {
    return <CheckCircle className="mx-auto h-5 w-5 text-splash-500" />
  }
  if (value === false) {
    return <Minus className="mx-auto h-5 w-5 text-gray-300" />
  }
  return <span className="text-sm font-medium text-foreground">{value}</span>
}

/* ═══════════════════════════════════════════════════════════════════════════
   1. HERO
   ═══════════════════════════════════════════════════════════════════════ */

function HeroSection() {
  return (
    <section className="relative overflow-hidden bg-gradient-to-br from-splash-900 to-splash-800 py-20 md:py-28">
      <div className="pointer-events-none absolute -left-40 -top-40 h-[500px] w-[500px] rounded-full bg-splash-500/10 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-32 -right-32 h-[400px] w-[400px] rounded-full bg-aqua-400/10 blur-3xl" />

      <motion.div
        initial="hidden"
        animate="visible"
        variants={staggerContainer}
        className="relative mx-auto max-w-6xl px-6 text-center"
      >
        <motion.h1
          variants={fadeUp}
          className="text-4xl font-extrabold leading-tight tracking-tight text-white md:text-5xl lg:text-6xl"
        >
          Simple, honest pricing
        </motion.h1>
        <motion.p
          variants={fadeUp}
          className="mx-auto mt-4 max-w-xl text-lg text-white/70"
        >
          No hidden fees. No per-transaction charges. Cancel anytime.
        </motion.p>
      </motion.div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   2. PRICING CARDS
   ═══════════════════════════════════════════════════════════════════════ */

function PricingCardsSection() {
  return (
    <section className="py-20 md:py-24">
      <div className="mx-auto max-w-6xl px-6">
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
              <p className="mt-1 text-sm text-muted-foreground">per month</p>

              <ul className="mt-6 space-y-3">
                {plan.features.map((feature) => (
                  <li
                    key={feature}
                    className="flex items-start gap-2 text-sm text-muted-foreground"
                  >
                    <CheckCircle className="mt-0.5 h-4 w-4 shrink-0 text-splash-500" />
                    {feature}
                  </li>
                ))}
              </ul>

              <a
                href={plan.ctaHref}
                className={cn(
                  'mt-8 block w-full rounded-lg py-3 text-center text-sm font-semibold transition',
                  plan.popular
                    ? 'bg-splash-500 text-white shadow-md hover:bg-splash-400'
                    : 'border border-splash-200 text-splash-600 hover:bg-splash-50'
                )}
              >
                {plan.cta}
              </a>
            </motion.div>
          ))}
        </motion.div>

        <motion.p
          variants={fadeUp}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          className="mt-10 text-center text-sm text-muted-foreground"
        >
          All plans include a 14-day free trial. No credit card required.
        </motion.p>
      </div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   3. FEATURE COMPARISON TABLE
   ═══════════════════════════════════════════════════════════════════════ */

function ComparisonTableSection() {
  return (
    <section className="bg-gray-50 py-20 md:py-24">
      <div className="mx-auto max-w-6xl px-6">
        <motion.div
          variants={fadeUp}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          className="mb-14 text-center"
        >
          <h2 className="text-3xl font-extrabold tracking-tight text-foreground md:text-4xl">
            Compare plans
          </h2>
          <p className="mx-auto mt-4 max-w-2xl text-lg text-muted-foreground">
            See exactly what you get with each plan.
          </p>
        </motion.div>

        <motion.div
          variants={fadeUp}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          className="overflow-x-auto rounded-xl border bg-white shadow-sm"
        >
          <table className="w-full min-w-[640px] text-left">
            <thead>
              <tr className="sticky top-0 z-10 bg-splash-50">
                <th className="px-6 py-4 text-sm font-semibold text-foreground">Feature</th>
                <th className="px-6 py-4 text-center text-sm font-semibold text-foreground">
                  Starter
                </th>
                <th className="px-6 py-4 text-center text-sm font-semibold text-foreground">
                  Growth
                </th>
                <th className="px-6 py-4 text-center text-sm font-semibold text-foreground">
                  Enterprise
                </th>
              </tr>
            </thead>
            <tbody>
              {comparisonRows.map((row, i) => (
                <tr
                  key={row.feature}
                  className={cn(i % 2 === 0 ? 'bg-white' : 'bg-gray-50')}
                >
                  <td className="px-6 py-3.5 text-sm text-foreground">{row.feature}</td>
                  <td className="px-6 py-3.5 text-center">
                    <ComparisonCell value={row.starter} />
                  </td>
                  <td className="px-6 py-3.5 text-center">
                    <ComparisonCell value={row.growth} />
                  </td>
                  <td className="px-6 py-3.5 text-center">
                    <ComparisonCell value={row.enterprise} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </motion.div>
      </div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   4. FAQ
   ═══════════════════════════════════════════════════════════════════════ */

function FaqSection() {
  return (
    <section className="py-20 md:py-24">
      <div className="mx-auto max-w-6xl px-6">
        <motion.div
          variants={fadeUp}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          className="mb-14 text-center"
        >
          <h2 className="text-3xl font-extrabold tracking-tight text-foreground md:text-4xl">
            Frequently asked questions
          </h2>
        </motion.div>

        <motion.div
          variants={staggerContainer}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          className="mx-auto max-w-2xl space-y-3"
        >
          {faqItems.map((item) => (
            <motion.div key={item.question} variants={fadeUp}>
              <details className="group rounded-xl border bg-white px-6 py-4 shadow-sm">
                <summary className="flex cursor-pointer items-center justify-between text-base font-semibold text-foreground">
                  {item.question}
                  <ChevronDown className="h-5 w-5 shrink-0 text-muted-foreground transition group-open:rotate-180" />
                </summary>
                <p className="mt-3 pb-1 text-sm leading-relaxed text-muted-foreground">
                  {item.answer}
                </p>
              </details>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   5. BOTTOM CTA
   ═══════════════════════════════════════════════════════════════════════ */

const ctaBadges = [
  { icon: Shield, label: '14-day free trial' },
  { icon: CreditCard, label: 'No credit card required' },
  { icon: Smartphone, label: 'Multi-payment tracking' },
]

function BottomCtaSection() {
  return (
    <section className="relative overflow-hidden bg-splash-900 py-20 md:py-24">
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
          Ready to get started?
        </motion.h2>

        <motion.p
          variants={fadeUp}
          className="mx-auto mt-4 max-w-xl text-lg text-white/70"
        >
          Start your 14-day free trial today. No credit card required.
        </motion.p>

        <motion.div
          variants={fadeUp}
          className="mt-8 flex flex-col justify-center gap-4 sm:flex-row"
        >
          <a
            href="#"
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-splash-500 px-8 py-3.5 text-base font-semibold text-white shadow-lg shadow-splash-500/25 transition hover:bg-splash-400"
          >
            Start Free Trial
            <ArrowRight className="h-4 w-4" />
          </a>
          <a
            href="#"
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-white/30 px-8 py-3.5 text-base font-semibold text-white transition hover:bg-white/10"
          >
            Contact Sales
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
   PAGE
   ═══════════════════════════════════════════════════════════════════════ */

export default function PricingPage() {
  return (
    <>
      <HeroSection />
      <PricingCardsSection />
      <ComparisonTableSection />
      <FaqSection />
      <BottomCtaSection />
    </>
  )
}
