'use client'

import Link from 'next/link'
import { useTranslations } from 'next-intl'
import { Building2, ChevronRight, MapPin } from 'lucide-react'
import type { ConnectDiscoveryResultDto } from '@splashsphere/types'

interface CarwashResultCardProps {
  /**
   * The nearest / primary branch row for this tenant. The discovery endpoint
   * emits one row per branch, so callers de-duplicate and pass the closest
   * branch here.
   */
  primary: ConnectDiscoveryResultDto
  /** How many additional branches this tenant has beyond `primary`. */
  extraBranchCount?: number
}

/**
 * Build a 1-2 character initial badge from the tenant name. Matches the style
 * of `tenant-card.tsx` on the Home screen so joined vs discoverable tenants
 * feel visually consistent.
 */
function initialsOf(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean).slice(0, 2)
  const initials = parts.map((p) => p[0]?.toUpperCase() ?? '').join('')
  return initials || '?'
}

/** Round to 1 decimal for display, e.g., 2.347 -> "2.3". */
function formatKm(km: number): string {
  return km.toFixed(km < 10 ? 1 : 0)
}

/**
 * A search result card for the Discover screen. Full-width, 56px+ touch
 * target, tappable on mobile. Links to the tenant's detail page.
 */
export function CarwashResultCard({
  primary,
  extraBranchCount = 0,
}: CarwashResultCardProps) {
  const t = useTranslations('discover')

  return (
    <Link
      href={`/carwash/${primary.tenantId}`}
      className="group flex min-h-[88px] items-center gap-3 rounded-2xl border border-border bg-card p-4 transition-colors active:scale-[0.99] hover:border-primary/40 hover:bg-accent/30"
    >
      <div
        className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-sm font-semibold text-primary"
        aria-hidden
      >
        {initialsOf(primary.tenantName) || <Building2 className="h-5 w-5" />}
      </div>

      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <p className="truncate text-base font-semibold text-foreground">
            {primary.tenantName}
          </p>
          {primary.isJoined && (
            <span className="inline-flex shrink-0 items-center rounded-full bg-primary/10 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-primary">
              {t('joinedBadge')}
            </span>
          )}
        </div>

        <p className="mt-0.5 flex items-center gap-1 truncate text-xs text-muted-foreground">
          <MapPin className="h-3 w-3 shrink-0" aria-hidden />
          <span className="truncate">
            {primary.branchName} · {primary.address}
          </span>
        </p>

        <div className="mt-1 flex items-center gap-2 text-xs text-muted-foreground">
          {primary.distanceKm !== null && (
            <span className="inline-flex items-center rounded-full bg-muted px-2 py-0.5 font-medium text-foreground">
              {t('distanceKm', { km: formatKm(primary.distanceKm) })}
            </span>
          )}
          {extraBranchCount > 0 && (
            <span className="truncate">
              {t('branchesMore', { count: extraBranchCount })}
            </span>
          )}
        </div>
      </div>

      <ChevronRight
        className="h-5 w-5 shrink-0 text-muted-foreground group-hover:text-foreground"
        aria-hidden
      />
    </Link>
  )
}
