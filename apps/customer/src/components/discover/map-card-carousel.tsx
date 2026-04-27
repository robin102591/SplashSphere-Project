'use client'

import Link from 'next/link'
import { useEffect, useRef } from 'react'
import { useTranslations } from 'next-intl'
import { Building2, ChevronRight, MapPin } from 'lucide-react'
import type { TenantGroup } from './group-by-tenant'

interface MapCardCarouselProps {
  groups: readonly TenantGroup[]
  selectedTenantId: string | null
  /** Fires when the user swipes a different card into view. */
  onVisibleChange: (tenantId: string) => void
}

/**
 * Horizontal snap-scrolling carousel pinned to the bottom of the map.
 * Each card is a compact version of `CarwashResultCard` — tenant name,
 * branch + address, distance chip, joined badge. Tap the card to go to the
 * tenant detail page (same as list view).
 *
 * Sync model:
 *   - `selectedTenantId` from the parent scrolls the matching card into view
 *     (used when the user taps a map marker).
 *   - An IntersectionObserver watches which card is centered and fires
 *     `onVisibleChange` so the parent can highlight the matching marker.
 * We guard against feedback loops with a short programmatic-scroll flag.
 */
export function MapCardCarousel({
  groups,
  selectedTenantId,
  onVisibleChange,
}: MapCardCarouselProps) {
  const containerRef = useRef<HTMLDivElement | null>(null)
  const programmaticScrollUntilRef = useRef<number>(0)

  // Scroll the active card into view when the parent changes selection.
  useEffect(() => {
    if (!selectedTenantId) return
    const container = containerRef.current
    if (!container) return
    const target = container.querySelector<HTMLElement>(
      `[data-tenant-id="${selectedTenantId}"]`,
    )
    if (!target) return
    // Suppress our IntersectionObserver for the duration of the animation so
    // we don't echo the selection change back to the parent.
    programmaticScrollUntilRef.current = performance.now() + 500
    target.scrollIntoView({
      behavior: 'smooth',
      inline: 'center',
      block: 'nearest',
    })
  }, [selectedTenantId])

  // Observe which card is centered.
  useEffect(() => {
    const container = containerRef.current
    if (!container) return

    const observer = new IntersectionObserver(
      (entries) => {
        if (performance.now() < programmaticScrollUntilRef.current) return
        // Pick the most-visible entry that crossed our threshold.
        let best: IntersectionObserverEntry | null = null
        for (const e of entries) {
          if (!e.isIntersecting) continue
          if (!best || e.intersectionRatio > best.intersectionRatio) best = e
        }
        if (!best) return
        const id = (best.target as HTMLElement).dataset.tenantId
        if (id) onVisibleChange(id)
      },
      { root: container, threshold: [0.6, 0.75, 0.9] },
    )

    container
      .querySelectorAll<HTMLElement>('[data-tenant-id]')
      .forEach((el) => observer.observe(el))

    return () => observer.disconnect()
  }, [groups, onVisibleChange])

  return (
    <div
      ref={containerRef}
      className="scrollbar-none pointer-events-auto flex snap-x snap-mandatory gap-3 overflow-x-auto overscroll-x-contain px-4 pb-4"
      aria-label="Nearby car washes"
    >
      {groups.map((g) => (
        <CompactCard
          key={g.primary.tenantId}
          group={g}
          active={g.primary.tenantId === selectedTenantId}
        />
      ))}
      {/* Trailing spacer so the last card can snap to center */}
      <div className="shrink-0" aria-hidden style={{ width: '8vw' }} />
    </div>
  )
}

function initialsOf(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean).slice(0, 2)
  const initials = parts.map((p) => p[0]?.toUpperCase() ?? '').join('')
  return initials || '?'
}

function formatKm(km: number): string {
  return km.toFixed(km < 10 ? 1 : 0)
}

function CompactCard({
  group,
  active,
}: {
  group: TenantGroup
  active: boolean
}) {
  const t = useTranslations('discover')
  const { primary, extraBranchCount } = group

  return (
    <Link
      href={`/carwash/${primary.tenantId}`}
      data-tenant-id={primary.tenantId}
      className={
        'group flex w-[85vw] max-w-[360px] shrink-0 snap-center items-center gap-3 rounded-2xl border bg-card p-3 shadow-lg transition-all active:scale-[0.99] ' +
        (active
          ? 'border-primary ring-2 ring-primary/30'
          : 'border-border')
      }
    >
      <div
        className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-sm font-semibold text-primary"
        aria-hidden
      >
        {initialsOf(primary.tenantName) || <Building2 className="h-5 w-5" />}
      </div>

      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <p className="truncate text-sm font-semibold text-foreground">
            {primary.tenantName}
          </p>
          {primary.isJoined && (
            <span className="inline-flex shrink-0 items-center rounded-full bg-primary/10 px-1.5 py-0.5 text-[9px] font-semibold uppercase tracking-wide text-primary">
              {t('joinedBadge')}
            </span>
          )}
        </div>

        <p className="mt-0.5 flex items-center gap-1 truncate text-[11px] text-muted-foreground">
          <MapPin className="h-3 w-3 shrink-0" aria-hidden />
          <span className="truncate">
            {primary.branchName} · {primary.address}
          </span>
        </p>

        <div className="mt-1 flex items-center gap-2 text-[11px] text-muted-foreground">
          {primary.distanceKm !== null && (
            <span className="inline-flex items-center rounded-full bg-muted px-1.5 py-0.5 font-medium text-foreground">
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
        className="h-4 w-4 shrink-0 text-muted-foreground group-hover:text-foreground"
        aria-hidden
      />
    </Link>
  )
}
