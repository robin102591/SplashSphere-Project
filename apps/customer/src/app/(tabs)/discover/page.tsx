'use client'

import { useEffect, useMemo, useState } from 'react'
import { useTranslations } from 'next-intl'
import { MapPin, Search, X } from 'lucide-react'
import type { ConnectDiscoveryResultDto } from '@splashsphere/types'
import {
  useDiscoverySearch,
  type DiscoverySearchCoords,
} from '@/hooks/use-discovery'
import { CarwashResultCard } from '@/components/discover/carwash-result-card'

const DEBOUNCE_MS = 300

/**
 * Explicit tri-state for the browser geolocation flow:
 * - `idle`:      we haven't attempted a lookup yet.
 * - `pending`:   asked the browser, waiting for a response.
 * - `granted`:   coords resolved — passed to the search hook.
 * - `denied`:    user declined or the lookup errored — sort alphabetical.
 * - `unsupported`: `navigator.geolocation` isn't available.
 */
type GeoState =
  | { kind: 'idle' }
  | { kind: 'pending' }
  | { kind: 'granted'; coords: DiscoverySearchCoords }
  | { kind: 'denied' }
  | { kind: 'unsupported' }

/**
 * Collapse the branch-level rows returned by the API into one card per
 * tenant. The first row for a tenant wins (the server already sorted by
 * distance when coords were supplied), and we count the remaining branches
 * for the "+N more" label.
 */
interface TenantGroup {
  primary: ConnectDiscoveryResultDto
  extraBranchCount: number
}

function groupByTenant(
  rows: readonly ConnectDiscoveryResultDto[],
): TenantGroup[] {
  const byTenant = new Map<string, TenantGroup>()
  for (const row of rows) {
    const existing = byTenant.get(row.tenantId)
    if (existing) {
      existing.extraBranchCount += 1
    } else {
      byTenant.set(row.tenantId, { primary: row, extraBranchCount: 0 })
    }
  }
  return Array.from(byTenant.values())
}

/**
 * Discover screen — the car-wash directory. Sticky search bar at the top,
 * grouped-by-tenant results below. Debounces the query 300ms before hitting
 * the server and optimistically reuses the previous page of results while
 * typing (see `placeholderData` in `useDiscoverySearch`).
 */
export default function DiscoverPage() {
  const t = useTranslations('discover')

  // `input` is the raw text the user types; `debounced` lags behind by
  // DEBOUNCE_MS and drives the query. Seeding `debounced` with the same
  // initial value skips the first-render debounce so the empty-query
  // "recommended" list appears instantly.
  const [input, setInput] = useState('')
  const [debounced, setDebounced] = useState('')

  // Geolocation is fired once on mount by a small "bridge" effect that only
  // *subscribes* to the browser API — it does not call setState in its body.
  // When the callback resolves later (off the render path) we update state.
  const [geo, setGeo] = useState<GeoState>(() =>
    typeof navigator !== 'undefined' && 'geolocation' in navigator
      ? { kind: 'pending' }
      : { kind: 'unsupported' },
  )

  // Debounce the `input` → `debounced` transition.
  useEffect(() => {
    if (input === debounced) return
    const t = setTimeout(() => setDebounced(input), DEBOUNCE_MS)
    return () => clearTimeout(t)
  }, [input, debounced])

  // Ask the browser for the user's coordinates. Non-blocking — the search
  // runs immediately without coords, and picks them up as soon as the
  // browser resolves the permission prompt.
  useEffect(() => {
    if (geo.kind !== 'pending') return
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setGeo({
          kind: 'granted',
          coords: {
            lat: pos.coords.latitude,
            lng: pos.coords.longitude,
          },
        })
      },
      () => setGeo({ kind: 'denied' }),
      { enableHighAccuracy: false, maximumAge: 5 * 60 * 1000, timeout: 8000 },
    )
    // getCurrentPosition cannot be cancelled; the component will either see
    // the success/error callback or unmount first (and then our setGeo is a
    // no-op because React silently drops updates on unmounted components in
    // strict mode — acceptable for this fire-and-forget flow).
  }, [geo.kind])

  const coords = geo.kind === 'granted' ? geo.coords : null
  const search = useDiscoverySearch(debounced, coords)
  const groups = useMemo(
    () => (search.data ? groupByTenant(search.data) : []),
    [search.data],
  )

  const clearInput = () => setInput('')

  return (
    <section className="space-y-4">
      {/* Header — sticks below the device top inset. The tab shell already
          accounts for header height in its top padding. */}
      <div className="space-y-2">
        <h1 className="text-xl font-semibold">{t('title')}</h1>
        <p className="text-sm text-muted-foreground">{t('subtitle')}</p>
      </div>

      {/* Sticky search bar — sits just under the page heading. */}
      <div className="sticky top-0 z-20 -mx-4 bg-background/95 px-4 py-2 backdrop-blur">
        <label className="relative block">
          <span className="sr-only">{t('searchPlaceholder')}</span>
          <Search
            className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground"
            aria-hidden
          />
          <input
            type="search"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder={t('searchPlaceholder')}
            autoComplete="off"
            className="h-12 w-full rounded-2xl border border-border bg-card pl-10 pr-10 text-base text-foreground placeholder:text-muted-foreground focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30"
          />
          {input.length > 0 && (
            <button
              type="button"
              onClick={clearInput}
              aria-label="Clear search"
              className="absolute right-2 top-1/2 flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-full text-muted-foreground transition-colors active:scale-[0.94] hover:bg-muted"
            >
              <X className="h-4 w-4" aria-hidden />
            </button>
          )}
        </label>

        {geo.kind === 'granted' && (
          <p className="mt-2 flex items-center gap-1 text-[11px] text-muted-foreground">
            <MapPin className="h-3 w-3" aria-hidden />
            {t('locatingHint')}
          </p>
        )}
        {geo.kind === 'denied' && (
          <p className="mt-2 text-[11px] text-muted-foreground">
            {t('locationDenied')}
          </p>
        )}
      </div>

      {/* Results */}
      {search.isPending && <ResultsSkeleton />}

      {search.isError && (
        <div className="rounded-2xl border border-destructive/20 bg-destructive/5 p-4 text-center">
          <p className="text-sm font-medium text-destructive">
            {t('errorTitle')}
          </p>
          <button
            type="button"
            onClick={() => search.refetch()}
            className="mt-3 inline-flex min-h-[44px] items-center justify-center rounded-xl border border-border bg-background px-4 text-sm font-semibold text-foreground transition-colors active:scale-[0.97] hover:bg-muted"
          >
            {t('errorRetry')}
          </button>
        </div>
      )}

      {search.data && groups.length === 0 && debounced.trim().length > 0 && (
        <div className="rounded-2xl border border-border bg-card p-6 text-center">
          <p className="text-base font-semibold text-foreground">
            {t('emptyTitle')}
          </p>
          <p className="mt-1 text-sm text-muted-foreground">
            {t('emptySubtitle')}
          </p>
        </div>
      )}

      {search.data &&
        groups.length === 0 &&
        debounced.trim().length === 0 && (
          <div className="rounded-2xl border border-dashed border-border bg-card p-8 text-center">
            <div className="mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-full bg-primary/10">
              <Search className="h-6 w-6 text-primary" aria-hidden />
            </div>
            <p className="text-base font-semibold text-foreground">
              {t('heroTitle')}
            </p>
            <p className="mt-1 text-sm text-muted-foreground">
              {t('heroSubtitle')}
            </p>
          </div>
        )}

      {groups.length > 0 && (
        <ul className="space-y-3">
          {groups.map((g) => (
            <li key={g.primary.tenantId}>
              <CarwashResultCard
                primary={g.primary}
                extraBranchCount={g.extraBranchCount}
              />
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

/** Four-row skeleton matching the CarwashResultCard height. */
function ResultsSkeleton() {
  return (
    <ul className="space-y-3" aria-busy="true">
      {[0, 1, 2, 3].map((i) => (
        <li
          key={i}
          className="h-[88px] animate-pulse rounded-2xl border border-border bg-card"
        />
      ))}
    </ul>
  )
}
