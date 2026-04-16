import { Resend } from 'resend'
import { NextResponse } from 'next/server'
import { z } from 'zod'

function getResend() {
  const key = process.env.RESEND_API_KEY
  if (!key) throw new Error('RESEND_API_KEY is not configured')
  return new Resend(key)
}

const contactSchema = z.object({
  fullName: z.string().min(2),
  phone: z.string().regex(/^09\d{9}$/),
  email: z.string().email(),
  businessName: z.string().min(2),
  branches: z.string().min(1),
  currentSystem: z.string().min(1),
  message: z.string().optional(),
})

export async function POST(req: Request) {
  const body = await req.json()
  const parsed = contactSchema.safeParse(body)

  if (!parsed.success) {
    return NextResponse.json(
      { error: 'Invalid form data' },
      { status: 400 },
    )
  }

  const data = parsed.data
  const senderDomain = process.env.RESEND_FROM_DOMAIN ?? 'splashsphere.ph'
  const salesEmail = process.env.CONTACT_RECIPIENT_EMAIL ?? 'sales@splashsphere.ph'

  try {
    const resend = getResend()

    // Send notification to sales team
    await resend.emails.send({
      from: `SplashSphere <demo@${senderDomain}>`,
      to: [salesEmail],
      replyTo: data.email,
      subject: `Demo request — ${data.businessName}`,
      html: `
        <h2>New Demo Request</h2>
        <table style="border-collapse:collapse;font-family:sans-serif;">
          <tr><td style="padding:6px 12px;font-weight:bold;">Name</td><td style="padding:6px 12px;">${escapeHtml(data.fullName)}</td></tr>
          <tr><td style="padding:6px 12px;font-weight:bold;">Phone</td><td style="padding:6px 12px;">${escapeHtml(data.phone)}</td></tr>
          <tr><td style="padding:6px 12px;font-weight:bold;">Email</td><td style="padding:6px 12px;">${escapeHtml(data.email)}</td></tr>
          <tr><td style="padding:6px 12px;font-weight:bold;">Business</td><td style="padding:6px 12px;">${escapeHtml(data.businessName)}</td></tr>
          <tr><td style="padding:6px 12px;font-weight:bold;">Branches</td><td style="padding:6px 12px;">${escapeHtml(data.branches)}</td></tr>
          <tr><td style="padding:6px 12px;font-weight:bold;">Current system</td><td style="padding:6px 12px;">${escapeHtml(data.currentSystem)}</td></tr>
          <tr><td style="padding:6px 12px;font-weight:bold;">Message</td><td style="padding:6px 12px;">${escapeHtml(data.message ?? '—')}</td></tr>
        </table>
      `,
    })

    // Send auto-acknowledgment to the customer
    await resend.emails.send({
      from: `SplashSphere <demo@${senderDomain}>`,
      to: [data.email],
      subject: 'We received your demo request!',
      html: `
        <div style="font-family:sans-serif;max-width:500px;">
          <h2>Hi ${escapeHtml(data.fullName)},</h2>
          <p>Thanks for your interest in SplashSphere! We received your demo request for <strong>${escapeHtml(data.businessName)}</strong>.</p>
          <p>Our team will reach out to you within 24 hours to schedule your personalized walkthrough.</p>
          <p style="margin-top:24px;">Salamat,<br/>The SplashSphere Team</p>
        </div>
      `,
    })

    return NextResponse.json({ ok: true })
  } catch (error) {
    console.error('Failed to send contact email:', error)
    return NextResponse.json(
      { error: 'Failed to send. Please try again later.' },
      { status: 500 },
    )
  }
}

function escapeHtml(str: string): string {
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
}
