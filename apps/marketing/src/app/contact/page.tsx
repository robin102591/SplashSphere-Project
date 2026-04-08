'use client'

import { useState } from 'react'
import { motion } from 'framer-motion'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import {
  CheckCircle,
  Phone,
  Mail,
  Facebook,
  Clock,
  Send,
  MessageSquare,
  Building2,
  User,
} from 'lucide-react'

/* ═══════════════════════════════════════════════════════════════════════════
   Animation Variants
   ═══════════════════════════════════════════════════════════════════════════ */

const fadeUp = {
  hidden: { opacity: 0, y: 30 },
  visible: (i: number = 0) => ({
    opacity: 1,
    y: 0,
    transition: { duration: 0.5, delay: i * 0.1, ease: 'easeOut' },
  }),
}

const stagger = {
  visible: { transition: { staggerChildren: 0.1 } },
}

/* ═══════════════════════════════════════════════════════════════════════════
   Zod Schema
   ═══════════════════════════════════════════════════════════════════════════ */

const contactSchema = z.object({
  fullName: z.string().min(2, 'Name is required'),
  phone: z
    .string()
    .regex(/^09\d{9}$/, 'Enter a valid PH mobile number (09XXXXXXXXX)'),
  email: z.string().email('Enter a valid email'),
  businessName: z.string().min(2, 'Business name is required'),
  branches: z.string().min(1, 'Select number of branches'),
  currentSystem: z.string().min(1, 'Select your current system'),
  message: z.string().optional(),
})

type ContactFormData = z.infer<typeof contactSchema>

/* ═══════════════════════════════════════════════════════════════════════════
   Constants
   ═══════════════════════════════════════════════════════════════════════════ */

const branchOptions = [
  { value: '1', label: '1' },
  { value: '2-3', label: '2-3' },
  { value: '4-5', label: '4-5' },
  { value: '6+', label: '6+' },
]

const systemOptions = [
  { value: 'notebook', label: 'Notebook/Paper' },
  { value: 'excel', label: 'Excel/Spreadsheet' },
  { value: 'software', label: 'Other Software' },
  { value: 'nothing', label: 'Nothing' },
]

const contactInfo = [
  {
    icon: Phone,
    label: 'Phone',
    value: '+63 917 123 4567',
    href: 'tel:+639171234567',
  },
  {
    icon: Mail,
    label: 'Email',
    value: 'hello@splashsphere.ph',
    href: 'mailto:hello@splashsphere.ph',
  },
  {
    icon: Facebook,
    label: 'Facebook',
    value: 'facebook.com/splashsphere',
    href: 'https://facebook.com/splashsphere',
  },
  {
    icon: Clock,
    label: 'Office Hours',
    value: 'Mon-Sat, 9 AM - 6 PM PHT',
    href: null,
  },
]

/* ═══════════════════════════════════════════════════════════════════════════
   Sub-components
   ═══════════════════════════════════════════════════════════════════════════ */

function InputField({
  label,
  error,
  children,
}: {
  label: string
  error?: string
  children: React.ReactNode
}) {
  return (
    <div>
      <label className="mb-1.5 block text-sm font-medium text-foreground">
        {label}
      </label>
      {children}
      {error && <p className="mt-1 text-sm text-red-500">{error}</p>}
    </div>
  )
}

function SuccessView() {
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.5, ease: 'easeOut' }}
      className="flex flex-col items-center justify-center py-16 text-center"
    >
      <motion.div
        initial={{ scale: 0 }}
        animate={{ scale: 1 }}
        transition={{ delay: 0.2, type: 'spring', stiffness: 200 }}
      >
        <CheckCircle className="mb-6 h-20 w-20 text-green-500" />
      </motion.div>
      <motion.h3
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.4 }}
        className="mb-3 text-2xl font-bold text-foreground"
      >
        Demo Booked!
      </motion.h3>
      <motion.p
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.5 }}
        className="text-lg text-muted-foreground"
      >
        Thanks! We&apos;ll call you within 24 hours.
      </motion.p>
    </motion.div>
  )
}

/* ═══════════════════════════════════════════════════════════════════════════
   Page Component
   ═══════════════════════════════════════════════════════════════════════════ */

export default function ContactPage() {
  const [submitted, setSubmitted] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ContactFormData>({
    resolver: zodResolver(contactSchema),
    defaultValues: {
      fullName: '',
      phone: '',
      email: '',
      businessName: '',
      branches: '',
      currentSystem: '',
      message: '',
    },
  })

  const onSubmit = (data: ContactFormData) => {
    console.log('Contact form submitted:', data)
    setSubmitted(true)
  }

  const inputClassName =
    'w-full px-4 py-3 border border-border rounded-lg focus:ring-2 focus:ring-splash-500 focus:border-splash-500 outline-none transition-colors bg-white text-foreground'

  return (
    <>
      {/* ── Hero ──────────────────────────────────────────────────────── */}
      <section className="bg-gradient-to-br from-splash-900 to-splash-800 py-20 text-white">
        <div className="mx-auto max-w-6xl px-6">
          <motion.div
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            variants={stagger}
            className="text-center"
          >
            <motion.div variants={fadeUp} custom={0}>
              <span className="mb-4 inline-flex items-center gap-2 rounded-full border border-white/20 bg-white/10 px-4 py-1.5 text-sm font-medium backdrop-blur-sm">
                <MessageSquare className="h-4 w-4" />
                Get in Touch
              </span>
            </motion.div>
            <motion.h1
              variants={fadeUp}
              custom={1}
              className="mb-4 text-4xl font-bold tracking-tight md:text-5xl"
            >
              Let&apos;s talk about your car wash
            </motion.h1>
            <motion.p
              variants={fadeUp}
              custom={2}
              className="mx-auto max-w-2xl text-lg text-splash-200"
            >
              Book a free 30-minute demo, or just ask us anything.
            </motion.p>
          </motion.div>
        </div>
      </section>

      {/* ── Form + Contact Info ───────────────────────────────────────── */}
      <section className="mx-auto max-w-6xl px-6 py-20">
        <div className="grid gap-12 lg:grid-cols-5">
          {/* Left Column — Form */}
          <motion.div
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: '-50px' }}
            variants={fadeUp}
            custom={0}
            className="lg:col-span-3"
          >
            <div className="rounded-xl border border-border bg-white p-8 shadow-sm">
              {submitted ? (
                <SuccessView />
              ) : (
                <>
                  <div className="mb-8">
                    <h2 className="text-2xl font-bold text-foreground">
                      Book a Free Demo
                    </h2>
                    <p className="mt-1 text-muted-foreground">
                      Fill out the form and our team will reach out to schedule
                      your personalized walkthrough.
                    </p>
                  </div>

                  <form
                    onSubmit={handleSubmit(onSubmit)}
                    className="space-y-5"
                  >
                    {/* Full Name */}
                    <InputField
                      label="Full Name"
                      error={errors.fullName?.message}
                    >
                      <div className="relative">
                        <User className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                        <input
                          type="text"
                          placeholder="Juan Dela Cruz"
                          className={`${inputClassName} pl-10`}
                          {...register('fullName')}
                        />
                      </div>
                    </InputField>

                    {/* Phone + Email row */}
                    <div className="grid gap-5 sm:grid-cols-2">
                      <InputField
                        label="Phone Number"
                        error={errors.phone?.message}
                      >
                        <div className="relative">
                          <Phone className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                          <input
                            type="tel"
                            placeholder="09XXXXXXXXX"
                            className={`${inputClassName} pl-10`}
                            {...register('phone')}
                          />
                        </div>
                      </InputField>

                      <InputField
                        label="Email"
                        error={errors.email?.message}
                      >
                        <div className="relative">
                          <Mail className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                          <input
                            type="email"
                            placeholder="juan@business.com"
                            className={`${inputClassName} pl-10`}
                            {...register('email')}
                          />
                        </div>
                      </InputField>
                    </div>

                    {/* Business Name */}
                    <InputField
                      label="Business Name"
                      error={errors.businessName?.message}
                    >
                      <div className="relative">
                        <Building2 className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                        <input
                          type="text"
                          placeholder="SparkleWash Carwash"
                          className={`${inputClassName} pl-10`}
                          {...register('businessName')}
                        />
                      </div>
                    </InputField>

                    {/* Branches + Current System row */}
                    <div className="grid gap-5 sm:grid-cols-2">
                      <InputField
                        label="Number of Branches"
                        error={errors.branches?.message}
                      >
                        <select
                          className={`${inputClassName} appearance-none`}
                          {...register('branches')}
                        >
                          <option value="">Select...</option>
                          {branchOptions.map((opt) => (
                            <option key={opt.value} value={opt.value}>
                              {opt.label}
                            </option>
                          ))}
                        </select>
                      </InputField>

                      <InputField
                        label="Current System"
                        error={errors.currentSystem?.message}
                      >
                        <select
                          className={`${inputClassName} appearance-none`}
                          {...register('currentSystem')}
                        >
                          <option value="">Select...</option>
                          {systemOptions.map((opt) => (
                            <option key={opt.value} value={opt.value}>
                              {opt.label}
                            </option>
                          ))}
                        </select>
                      </InputField>
                    </div>

                    {/* Message */}
                    <InputField label="Message (optional)">
                      <textarea
                        rows={4}
                        placeholder="Tell us about your car wash business..."
                        className={`${inputClassName} resize-none`}
                        {...register('message')}
                      />
                    </InputField>

                    {/* Submit */}
                    <button
                      type="submit"
                      disabled={isSubmitting}
                      className="flex w-full items-center justify-center gap-2 rounded-lg bg-splash-500 py-3 font-semibold text-white transition-colors hover:bg-splash-600 disabled:opacity-60"
                    >
                      {isSubmitting ? (
                        'Sending...'
                      ) : (
                        <>
                          <Send className="h-4 w-4" />
                          Book My Demo
                        </>
                      )}
                    </button>
                  </form>
                </>
              )}
            </div>
          </motion.div>

          {/* Right Column — Contact Info */}
          <motion.div
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: '-50px' }}
            variants={stagger}
            className="lg:col-span-2"
          >
            <motion.div
              variants={fadeUp}
              custom={0}
              className="rounded-xl bg-gray-50 p-8"
            >
              <h3 className="mb-6 text-xl font-bold text-foreground">
                Contact Information
              </h3>

              <div className="space-y-6">
                {contactInfo.map((item, i) => {
                  const Icon = item.icon
                  const content = (
                    <motion.div
                      key={item.label}
                      variants={fadeUp}
                      custom={i + 1}
                      className="flex items-start gap-4"
                    >
                      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-splash-100 text-splash-600">
                        <Icon className="h-5 w-5" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-muted-foreground">
                          {item.label}
                        </p>
                        <p className="font-medium text-foreground">
                          {item.value}
                        </p>
                      </div>
                    </motion.div>
                  )

                  if (item.href) {
                    return (
                      <a
                        key={item.label}
                        href={item.href}
                        target={
                          item.href.startsWith('http') ? '_blank' : undefined
                        }
                        rel={
                          item.href.startsWith('http')
                            ? 'noopener noreferrer'
                            : undefined
                        }
                        className="block transition-opacity hover:opacity-80"
                      >
                        {content}
                      </a>
                    )
                  }

                  return content
                })}
              </div>
            </motion.div>

            {/* Extra CTA card */}
            <motion.div
              variants={fadeUp}
              custom={5}
              className="mt-6 rounded-xl border border-splash-200 bg-splash-50 p-8"
            >
              <h3 className="mb-2 text-lg font-bold text-splash-900">
                Want to see it live?
              </h3>
              <p className="text-sm text-splash-700">
                We can set up a working demo environment with your actual
                services, pricing, and branch layout -- so you see exactly how
                SplashSphere fits your business.
              </p>
            </motion.div>
          </motion.div>
        </div>
      </section>
    </>
  )
}
