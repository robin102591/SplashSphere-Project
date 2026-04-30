'use client'

import Image from 'next/image'
import { CheckCircle2, Star } from 'lucide-react'
import type {
  DisplayBrandingDto,
  DisplayCompletionPayload,
  DisplaySettingDto,
} from '@splashsphere/types'
import { formatPeso } from '@splashsphere/format'
import type { ThemeClasses } from './page'

/**
 * Shown briefly after payment. Auto-reverts to Idle after the configured hold
 * duration (default 10s) — managed by useDisplayConnection.
 */
export function CompleteScreen({
  completion,
  settings,
  branding,
  theme,
}: {
  completion: DisplayCompletionPayload
  settings: DisplaySettingDto
  branding: DisplayBrandingDto
  theme: ThemeClasses
}) {
  const tx = completion.transaction
  const showLoyalty =
    (settings.showPointsEarned && completion.pointsEarned !== null) ||
    (settings.showPointsBalance && completion.pointsBalance !== null)

  return (
    <div className="flex-1 flex flex-col px-10 py-8 gap-5">

      {/* ── Compact header ──────────────────────────────────────────── */}
      <header className="flex items-center gap-3">
        {settings.showLogo && branding.logoUrl ? (
          <Image
            src={branding.logoUrl}
            alt=""
            width={60}
            height={60}
            className="h-12 w-12 object-contain"
            unoptimized
          />
        ) : null}
        {settings.showBusinessName && (
          <h1 className="text-2xl font-bold tracking-tight">{branding.businessName}</h1>
        )}
      </header>

      {/* ── Big "Payment Complete" banner ───────────────────────────── */}
      <div className="flex items-center justify-center gap-3 py-4">
        <CheckCircle2 className="h-12 w-12 text-emerald-400 animate-pulse-slow" />
        <p className="text-3xl font-bold uppercase tracking-wider text-emerald-400">
          Payment Complete
        </p>
      </div>

      {/* ── Item summary ────────────────────────────────────────────── */}
      <div className="flex-1 min-h-0 overflow-y-auto space-y-1.5">
        {tx.items.map((item) => (
          <div key={item.id} className="flex items-center justify-between text-sm">
            <span>
              {item.name}
              {item.quantity > 1 && (
                <span className={`ml-2 ${theme.textMuted}`}>x{item.quantity}</span>
              )}
            </span>
            <span className="tabular-nums font-medium">{formatPeso(item.totalPrice)}</span>
          </div>
        ))}
      </div>

      {/* ── Totals ──────────────────────────────────────────────────── */}
      <div className={`pt-3 border-t ${theme.border} space-y-1`}>
        <Row label="Subtotal" value={formatPeso(tx.subtotal)} muted={theme.textMuted} />
        {settings.showDiscountBreakdown && tx.discountAmount > 0 && (
          <Row
            label={tx.discountLabel ?? 'Discount'}
            value={`-${formatPeso(tx.discountAmount)}`}
            muted={theme.textMuted}
            accent={theme.accent}
          />
        )}
        {settings.showTaxLine && tx.taxAmount > 0 && (
          <Row label="Tax" value={formatPeso(tx.taxAmount)} muted={theme.textMuted} />
        )}
        <div className="flex items-baseline justify-between pt-1">
          <span className="text-base font-semibold">Total</span>
          <span className="text-2xl font-bold tabular-nums">{formatPeso(tx.total)}</span>
        </div>
      </div>

      {/* ── Payment band ────────────────────────────────────────────── */}
      <div className={`rounded-lg ${theme.surface} ${theme.border} border px-5 py-3 space-y-1`}>
        {settings.showPaymentMethod && (
          <Row label={`Paid: ${completion.paymentMethod}`} value={formatPeso(completion.amountPaid)} muted={theme.textMuted} />
        )}
        {settings.showChangeAmount && completion.changeAmount > 0 && (
          <Row label="Change" value={formatPeso(completion.changeAmount)} muted={theme.textMuted} />
        )}
      </div>

      {/* ── Loyalty band ────────────────────────────────────────────── */}
      {showLoyalty && (
        <div className={`rounded-lg ${theme.surface} ${theme.border} border px-5 py-3 space-y-1`}>
          {settings.showPointsEarned && completion.pointsEarned !== null && (
            <p className="flex items-center justify-between text-sm">
              <span className={`flex items-center gap-1.5 ${theme.accent}`}>
                <Star className="h-4 w-4 fill-current" />
                Points Earned
              </span>
              <span className="font-semibold tabular-nums">+{completion.pointsEarned} pts</span>
            </p>
          )}
          {settings.showPointsBalance && completion.pointsBalance !== null && (
            <p className="flex items-center justify-between text-sm">
              <span className={`flex items-center gap-1.5 ${theme.accent}`}>
                <Star className="h-4 w-4 fill-current" />
                New Balance
              </span>
              <span className="font-semibold tabular-nums">{completion.pointsBalance.toLocaleString()} pts</span>
            </p>
          )}
        </div>
      )}

      {/* ── Footer messages ─────────────────────────────────────────── */}
      <div className="text-center space-y-1.5 mt-auto pt-4">
        {settings.showThankYouMessage && completion.thankYouMessage && (
          <p className="text-xl font-semibold">{completion.thankYouMessage}</p>
        )}
        {settings.showPromoText && completion.promoText && (
          <p className={`text-base ${theme.accent}`}>{completion.promoText}</p>
        )}
      </div>
    </div>
  )
}

function Row({
  label, value, muted, accent,
}: {
  label: string
  value: string
  muted: string
  accent?: string
}) {
  return (
    <div className="flex items-center justify-between text-sm">
      <span className={accent ?? muted}>{label}</span>
      <span className={`tabular-nums font-medium ${accent ?? ''}`}>{value}</span>
    </div>
  )
}
