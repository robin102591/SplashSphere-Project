'use client'

import { motion } from 'framer-motion'
import { MapPin, Zap, Eye, Shield, ArrowRight } from 'lucide-react'
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
   Section Wrapper
   ═══════════════════════════════════════════════════════════════════════ */

function Section({
  children,
  className,
}: {
  children: React.ReactNode
  className?: string
}) {
  return (
    <section className={cn('py-20 md:py-24', className)}>
      <div className="mx-auto max-w-6xl px-6">{children}</div>
    </section>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   Values Data
   ═══════════════════════════════════════════════════════════════════════ */

const values = [
  {
    icon: MapPin,
    title: 'Built for the Philippines',
    description:
      'Peso currency, multi-payment tracking (cash, card, e-wallet with reference numbers), Filipino names, weekly payroll cycles \u2014 everything designed for how car washes actually run here.',
  },
  {
    icon: Zap,
    title: 'Simple yet Powerful',
    description:
      'Simple enough for a cashier on their first day, powerful enough for an owner managing five branches.',
  },
  {
    icon: Eye,
    title: 'Transparent Pricing',
    description:
      'No hidden fees, no per-transaction charges, no lock-in contracts. What you see is what you pay.',
  },
  {
    icon: Shield,
    title: 'Your Data, Secured',
    description:
      'Customer data is encrypted, backed up daily, and never sold. We take privacy seriously.',
  },
]

/* ═══════════════════════════════════════════════════════════════════════════
   Team Placeholder
   ═══════════════════════════════════════════════════════════════════════ */

const teamMembers = [
  { name: 'Founder', role: 'CEO & Lead Developer' },
  { name: 'Team Member', role: 'Full-Stack Engineer' },
  { name: 'Team Member', role: 'UI/UX Designer' },
]

/* ═══════════════════════════════════════════════════════════════════════════
   Page Component
   ═══════════════════════════════════════════════════════════════════════ */

export default function AboutPage() {
  return (
    <main>
      {/* ── Hero ────────────────────────────────────────────────────── */}
      <Section className="bg-gradient-to-br from-splash-900 to-splash-800 text-white">
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          animate="visible"
          className="text-center"
        >
          <motion.h1
            variants={fadeUp}
            className="text-4xl font-bold tracking-tight sm:text-5xl md:text-6xl"
          >
            About SplashSphere
          </motion.h1>
          <motion.p
            variants={fadeUp}
            className="mx-auto mt-6 max-w-2xl text-lg text-splash-200 sm:text-xl"
          >
            Built by LezanobTech for the Philippine car wash industry.
          </motion.p>
        </motion.div>
      </Section>

      {/* ── Our Story ───────────────────────────────────────────────── */}
      <Section className="bg-white">
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-80px' }}
          className="mx-auto max-w-3xl"
        >
          <motion.h2
            variants={fadeUp}
            className="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl"
          >
            Our Story
          </motion.h2>

          <motion.blockquote
            variants={fadeUp}
            className="mt-10 border-l-4 border-splash-500 pl-6 text-xl leading-relaxed text-gray-700 italic sm:text-2xl"
          >
            &ldquo;We built SplashSphere because we saw car wash owners struggling
            with the same problems every week &mdash; lost notebooks, commission
            arguments, no idea if they&rsquo;re actually profitable. We knew there
            had to be a better way.&rdquo;
          </motion.blockquote>

          <motion.p
            variants={fadeUp}
            className="mt-8 text-lg leading-relaxed text-gray-600"
          >
            LezanobTech identified a glaring gap in the Philippine market: while
            large retail chains had access to sophisticated POS and management
            tools, the thousands of car wash businesses across the country were
            still running on paper logs, mental math, and trust. Commission
            disputes were a weekly headache. Owners had no visibility into which
            branches were profitable and which were bleeding money. We set out to
            build affordable, locally-relevant software that speaks the language
            of Philippine car wash operations &mdash; from multi-payment
            tracking and weekly payroll cut-offs to commission splitting and
            vehicle queue management. SplashSphere is the result of that mission.
          </motion.p>
        </motion.div>
      </Section>

      {/* ── Mission ─────────────────────────────────────────────────── */}
      <Section className="bg-gray-50">
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-80px' }}
          className="text-center"
        >
          <motion.h2
            variants={fadeUp}
            className="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl"
          >
            Our Mission
          </motion.h2>
          <motion.p
            variants={fadeUp}
            className="mx-auto mt-6 max-w-3xl text-xl leading-relaxed text-gray-700 sm:text-2xl"
          >
            Empowering car wash entrepreneurs with affordable, locally-relevant
            business management tools.
          </motion.p>
        </motion.div>
      </Section>

      {/* ── Values ──────────────────────────────────────────────────── */}
      <Section className="bg-white">
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-80px' }}
        >
          <motion.h2
            variants={fadeUp}
            className="text-center text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl"
          >
            Our Values
          </motion.h2>

          <motion.div
            variants={staggerContainer}
            className="mt-12 grid gap-8 sm:grid-cols-2"
          >
            {values.map((value) => (
              <motion.div
                key={value.title}
                variants={fadeUp}
                className="rounded-xl border bg-white p-8 shadow-sm"
              >
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-splash-100">
                  <value.icon className="h-6 w-6 text-splash-600" />
                </div>
                <h3 className="mt-4 text-xl font-semibold text-gray-900">
                  {value.title}
                </h3>
                <p className="mt-2 leading-relaxed text-gray-600">
                  {value.description}
                </p>
              </motion.div>
            ))}
          </motion.div>
        </motion.div>
      </Section>

      {/* ── Meet the Team ───────────────────────────────────────────── */}
      <Section className="bg-gray-50">
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-80px' }}
        >
          <motion.h2
            variants={fadeUp}
            className="text-center text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl"
          >
            Meet the Team
          </motion.h2>
          <motion.p
            variants={fadeUp}
            className="mx-auto mt-4 max-w-2xl text-center text-lg text-gray-600"
          >
            SplashSphere is built by a small, passionate team at LezanobTech,
            based in the Philippines.
          </motion.p>

          <motion.div
            variants={staggerContainer}
            className="mt-12 flex flex-wrap items-center justify-center gap-10"
          >
            {teamMembers.map((member) => (
              <motion.div
                key={member.role}
                variants={fadeUp}
                className="flex flex-col items-center"
              >
                <div className="flex h-28 w-28 items-center justify-center rounded-full bg-gray-200 text-sm font-medium text-gray-500">
                  Photo
                </div>
                <p className="mt-4 font-semibold text-gray-900">{member.name}</p>
                <p className="text-sm text-gray-500">{member.role}</p>
              </motion.div>
            ))}
          </motion.div>
        </motion.div>
      </Section>

      {/* ── Bottom CTA ──────────────────────────────────────────────── */}
      <Section className="bg-splash-900 text-white">
        <motion.div
          variants={staggerContainer}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-80px' }}
          className="text-center"
        >
          <motion.h2
            variants={fadeUp}
            className="text-3xl font-bold tracking-tight sm:text-4xl"
          >
            Join us in modernizing car wash management
          </motion.h2>
          <motion.div variants={fadeUp} className="mt-8">
            <a
              href="/pricing"
              className="inline-flex items-center gap-2 rounded-lg bg-white px-8 py-4 text-lg font-semibold text-splash-900 shadow-lg transition-all hover:bg-splash-50 hover:shadow-xl"
            >
              Start Free Trial
              <ArrowRight className="h-5 w-5" />
            </a>
          </motion.div>
        </motion.div>
      </Section>
    </main>
  )
}
