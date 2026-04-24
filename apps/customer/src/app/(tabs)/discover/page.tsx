'use client'

import dynamic from 'next/dynamic'
import { useEffect, useMemo, useState } from 'react'
import { useTranslations } from 'next-intl'
import { MapPin, Search, X } from 'lucide-react'
import {
  useDiscoverySearch,
  type DiscoverySearchCoords,
} from '@/hooks/use-discovery'
import { CarwashResultCard } from '@/components/discover/carwash-result-card'
import { ViewToggle, type DiscoverView } from '@/components/discover/view-toggle'
import {
  groupByTenant,
  type TenantGroup,
} from '@/components/discover/group-by-tenant'

/**
 * The Mapbox view pulls in ~220KB of JS and touches `window`, so it's
 * lazy-loaded only when the user actually switches to Map mode. SSR is
 * disabled because `mapbox-gl` calls `window` at module scope.
 */
const DiscoverMapView = dynamic(
  () =>
    import('@/components/discover/discover-map-view').then(
      (m) => m.DiscoverMapView,
    ),
  {
    ssr: false,
    loading: () => (
      <div className="h-[calc(100svh-180px)] min-h-[480px] animate-pulse rounded-2xl border border-border bg-muted" />
    ),
  },
)

const DEBOUNCE_MS = 300
const MAPBOX_TOKEN_CONFIGURED = Boolean(process.env.NEXT_PUBLIC_MAPBOX_TOKEN)

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
 * Discover screen — the car-wash directory. Sticky search bar at the top,
 * grouped-by-tenant results below. Debounces the query 300ms before hitting
 * the server and optimistically reuses the previous page of results while
 * typing (see `placeholderData` in `useDiscoverySearch`).
 *
 * Supports two view modes:
 *   - `list` (default): vertical stack of result cards.
 *   - `map`:  full-screen Mapbox map with a bottom card carousel. Hidden
 *     entirely when `NEXT_PUBLIC_MAPBOX_TOKEN` is unset.
 */
export default function DiscoverPage() {
  const t = useTranslations('discover')

  // `input` is the raw text the user types; `debounced` lags behind by
  // DEBOUNCE_MS and drives the query. Seeding `debounced` with the same
  // initial value skips the first-render debounce so the empty-query
  // "recommended" list appears instantly.
  const [input, setInput] = useState('')
  const [debounced, setDebounced] = useState('')

  const [view, setView] = useState<DiscoverView>('list')

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
  }, [geo.kind])

  const coords = geo.kind === 'granted' ? geo.coords : null
  const search = useDiscoverySearch(debounced, coords)
  const groups = useMemo<TenantGroup[]>(
    () => (search.data ? groupByTenant(search.data) : []),
    [search.data],
  )

  const noCoordsCount = useMemo(
    () =>
      groups.reduce(
        (n, g) =>
          g.primary.latitude === null || g.primary.longitude === null
            ? n + 1
            : n,
        0,
      ),
    [groups],
  )

  const clearInput = () => setInput('')

  return (
    <section className="space-y-4">
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

        <div className="mt-2 flex items-center justify-between gap-3">
          <div className="min-w-0 flex-1">
            {geo.kind === 'granted' && (
              <p className="flex items-center gap-1 text-[11px] text-muted-foreground">
                <MapPin className="h-3 w-3" aria-hidden />
                {t('locatingHint')}
              </p>
            )}
            {geo.kind === 'denied' && (
              <p className="text-[11px] text-muted-foreground">
                {t('locationDenied')}
              </p>
            )}
          </div>
          {MAPBOX_TOKEN_CONFIGURED && (
            <ViewToggle value={view} onChange={setView} />
          )}
        </div>
      </div>

      {/* Results */}
      {search.isPending && view === 'list' && <ResultsSkeleton />}

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
        debounced.trim().length === 0 &&
        view === 'list' && (
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

      {view === 'list' && groups.length > 0 && (
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

      {view === 'map' && (
        <div className="space-y-2">
          <DiscoverMapView groups={groups} userCoords={coords} />
          {noCoordsCount > 0 && (
            <p className="flex items-center gap-1 text-[11px] text-muted-foreground">
              <MapPin className="h-3 w-3" aria-hidden />
              {t('noMapLocation', { count: noCoordsCount })}
            </p>
          )}
        </div>
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
