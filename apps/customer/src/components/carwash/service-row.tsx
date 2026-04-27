'use client'

import { useTranslations } from 'next-intl'
import type { ConnectServiceSummaryDto } from '@splashsphere/types'

interface ServiceRowProps {
  service: ConnectServiceSummaryDto
}

/**
 * Format a PHP amount for display without the currency symbol (the symbol
 * comes from the i18n string). Uses grouping separators and drops trailing
 * zero decimals so ₱120.00 becomes ₱120 but ₱120.50 stays intact.
 */
function formatPhpAmount(value: number): string {
  const hasFraction = Math.round(value * 100) % 100 !== 0
  return value.toLocaleString('en-PH', {
    minimumFractionDigits: hasFraction ? 2 : 0,
    maximumFractionDigits: 2,
  })
}

/**
 * A single service row on the car-wash detail page. We only show the
 * `basePrice` here because the detail endpoint does not return per-vehicle
 * pricing ranges — the full range lookup lives behind
 * `GET /carwashes/{tenantId}/services?vehicleId=...` and is invoked inside
 * the booking wizard (task 22.3-E) once a vehicle is selected.
 */
export function ServiceRow({ service }: ServiceRowProps) {
  const t = useTranslations('carwash')

  return (
    <div className="flex items-start justify-between gap-3 rounded-xl border border-border bg-card p-4">
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-semibold text-foreground">
          {service.name}
        </p>
        {service.description && (
          <p className="mt-0.5 line-clamp-2 text-xs text-muted-foreground">
            {service.description}
          </p>
        )}
      </div>
      <span className="shrink-0 rounded-lg bg-primary/5 px-2.5 py-1 text-sm font-semibold tabular-nums text-primary">
        {t('fromPrice', { price: formatPhpAmount(service.basePrice) })}
      </span>
    </div>
  )
}
