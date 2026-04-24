'use client'

import { useTranslations } from 'next-intl'
import { Clock, MapPin, Phone } from 'lucide-react'
import type { ConnectBranchSummaryDto } from '@splashsphere/types'

interface BranchCardProps {
  branch: ConnectBranchSummaryDto
}

/**
 * Summary card for a single branch on the car-wash detail page. Shows
 * address, contact number, and today's hours when booking is enabled;
 * falls back to an "In-store only" badge for branches that haven't
 * opted into online booking.
 */
export function BranchCard({ branch }: BranchCardProps) {
  const t = useTranslations('carwash')

  return (
    <div className="rounded-2xl border border-border bg-card p-4">
      <div className="flex items-start justify-between gap-3">
        <p className="text-base font-semibold text-foreground">{branch.name}</p>
        {!branch.isBookingEnabled && (
          <span className="shrink-0 rounded-full bg-muted px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-muted-foreground">
            {t('bookingDisabled')}
          </span>
        )}
      </div>

      <p className="mt-1 flex items-start gap-1.5 text-xs text-muted-foreground">
        <MapPin className="mt-0.5 h-3.5 w-3.5 shrink-0" aria-hidden />
        <span>{branch.address}</span>
      </p>

      {branch.contactNumber && (
        <p className="mt-1 flex items-center gap-1.5 text-xs text-muted-foreground">
          <Phone className="h-3.5 w-3.5 shrink-0" aria-hidden />
          <a
            href={`tel:${branch.contactNumber}`}
            className="underline-offset-2 hover:underline"
          >
            {branch.contactNumber}
          </a>
        </p>
      )}

      {branch.isBookingEnabled && branch.openTime && branch.closeTime && (
        <p className="mt-1 flex items-center gap-1.5 text-xs text-muted-foreground">
          <Clock className="h-3.5 w-3.5 shrink-0" aria-hidden />
          <span>
            {t('hoursToday', {
              open: branch.openTime,
              close: branch.closeTime,
            })}
          </span>
        </p>
      )}
    </div>
  )
}
