'use client'

import Image from 'next/image'
import { Star } from 'lucide-react'
import type {
  DisplayBrandingDto,
  DisplaySettingDto,
  DisplayTransactionPayload,
} from '@splashsphere/types'
import { formatPeso } from '@splashsphere/format'
import type { ThemeClasses } from './page'

/**
 * Live-builds the customer's bill as the cashier adds line items. The total
 * is the visual anchor — large, prominent, always visible — and matches the
 * trust-and-transparency goal of the feature.
 */
export function BuildingScreen({
  transaction,
  settings,
  branding,
  theme,
}: {
  transaction: DisplayTransactionPayload
  settings: DisplaySettingDto
  branding: DisplayBrandingDto
  theme: ThemeClasses
}) {
  const showVehicle =
    settings.showVehicleInfo && (transaction.vehiclePlate || transaction.vehicleMakeModel)
  const showCustomer =
    settings.showCustomerName && transaction.customerName

  return (
    <div className="flex-1 flex flex-col px-10 py-8 gap-6">

      {/* ── Compact header ───────────────────────────────────────────── */}
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

      {/* ── Vehicle / customer band ─────────────────────────────────── */}
      {(showVehicle || showCustomer) && (
        <div className={`rounded-lg ${theme.surface} ${theme.border} border px-5 py-3 flex flex-wrap items-center gap-x-6 gap-y-1`}>
          {showVehicle && (
            <p className="text-sm">
              <span className={theme.textMuted}>Vehicle: </span>
              <span className="font-semibold">
                {[transaction.vehicleMakeModel, transaction.vehiclePlate, transaction.vehicleTypeSize]
                  .filter(Boolean)
                  .join(' • ')}
              </span>
            </p>
          )}
          {showCustomer && (
            <p className="text-sm flex items-center gap-1.5">
              <span className={theme.textMuted}>Customer: </span>
              <span className="font-semibold">{transaction.customerName}</span>
              {settings.showLoyaltyTier && transaction.loyaltyTier && (
                <span className={`inline-flex items-center gap-1 ${theme.accent} text-xs font-bold`}>
                  <Star className="h-3.5 w-3.5 fill-current" /> {transaction.loyaltyTier}
                </span>
              )}
            </p>
          )}
        </div>
      )}

      {/* ── Items table ─────────────────────────────────────────────── */}
      <div className="flex-1 min-h-0 overflow-y-auto">
        <table className="w-full">
          <thead className={`text-xs uppercase tracking-wider ${theme.textMuted}`}>
            <tr className={`border-b ${theme.border}`}>
              <th className="py-2 text-left font-medium">Service / Item</th>
              <th className="py-2 text-right font-medium w-20">Qty</th>
              <th className="py-2 text-right font-medium w-32">Amount</th>
            </tr>
          </thead>
          <tbody>
            {transaction.items.length === 0 && (
              <tr>
                <td colSpan={3} className={`py-12 text-center ${theme.textMuted}`}>
                  Adding services…
                </td>
              </tr>
            )}
            {transaction.items.map((item) => (
              <tr key={item.id} className={`border-b ${theme.border} animate-slide-in`}>
                <td className="py-3 font-medium">{item.name}</td>
                <td className={`py-3 text-right tabular-nums ${theme.textMuted}`}>
                  {item.quantity}
                </td>
                <td className="py-3 text-right tabular-nums font-semibold">
                  {formatPeso(item.totalPrice)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* ── Totals strip ────────────────────────────────────────────── */}
      <div className={`pt-4 border-t-2 ${theme.border} space-y-1.5`}>
        <Row label="Subtotal" value={formatPeso(transaction.subtotal)} muted={theme.textMuted} />

        {settings.showDiscountBreakdown && transaction.discountAmount > 0 && (
          <Row
            label={transaction.discountLabel ?? 'Discount'}
            value={`-${formatPeso(transaction.discountAmount)}`}
            muted={theme.textMuted}
            accent={theme.accent}
          />
        )}

        {settings.showTaxLine && transaction.taxAmount > 0 && (
          <Row label="Tax" value={formatPeso(transaction.taxAmount)} muted={theme.textMuted} />
        )}

        <div className={`pt-3 mt-2 border-t-2 ${theme.border} flex items-baseline justify-between`}>
          <span className="text-lg font-semibold uppercase tracking-wider">Total</span>
          <span className="text-5xl font-bold tabular-nums">
            {formatPeso(transaction.total)}
          </span>
        </div>
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
