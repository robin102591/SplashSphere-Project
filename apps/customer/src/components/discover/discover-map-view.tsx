'use client'

import { useCallback, useMemo, useRef, useState } from 'react'
import { useTranslations } from 'next-intl'
import { MapPin, Navigation } from 'lucide-react'
import ReactMapGL, {
  GeolocateControl,
  Marker,
  NavigationControl,
  type MapRef,
} from 'react-map-gl'
import type { TenantGroup } from './group-by-tenant'
import type { DiscoverySearchCoords } from '@/hooks/use-discovery'
import { MapCardCarousel } from './map-card-carousel'

/** Public Mapbox token read once at module load. */
const MAPBOX_TOKEN = process.env.NEXT_PUBLIC_MAPBOX_TOKEN ?? ''

/** Manila city center — used when the user denies geolocation and we have
 *  no pins with coordinates to fit bounds to. */
const MANILA_FALLBACK = { latitude: 14.5995, longitude: 120.9842, zoom: 11 }

/** Distance (px) reserved at the bottom of the map so the carousel never
 *  obscures pins when we `fitBounds`. */
const CAROUSEL_PADDING_PX = 180

interface DiscoverMapViewProps {
  /** All tenant groups — both with and without coordinates. We filter here. */
  groups: readonly TenantGroup[]
  userCoords: DiscoverySearchCoords | null
}

interface PinGroup extends TenantGroup {
  latitude: number
  longitude: number
}

/**
 * Narrow a TenantGroup to one whose primary branch has real coordinates.
 * Branches without coords are hidden from the map entirely (the parent
 * surfaces them as a count chip).
 */
function hasCoords(g: TenantGroup): g is PinGroup {
  return g.primary.latitude !== null && g.primary.longitude !== null
}

/**
 * Full-screen Mapbox view for the Discover screen. Pins are interactive —
 * tap to focus and sync the bottom carousel; swipe the carousel to pan the
 * map. Renders nothing useful when `NEXT_PUBLIC_MAPBOX_TOKEN` is unset — the
 * parent should hide the Map toggle in that case.
 */
export function DiscoverMapView({ groups, userCoords }: DiscoverMapViewProps) {
  const t = useTranslations('discover')
  const mapRef = useRef<MapRef | null>(null)
  const didFitRef = useRef(false)

  const pinGroups = useMemo<PinGroup[]>(
    () => groups.filter(hasCoords),
    [groups],
  )

  // Raw user-picked selection. May reference a tenant that's no longer in
  // the result set (e.g., after the user refines the search query); we
  // reconcile against the live pin list during render below rather than in
  // an effect so state stays a pure function of props.
  const [pickedTenantId, setPickedTenantId] = useState<string | null>(null)

  // Derive the effective selection from props each render. Falls back to
  // the first pin when the user's pick is stale or unset, and to `null`
  // when there are no pins at all.
  const selectedTenantId = useMemo<string | null>(() => {
    if (pinGroups.length === 0) return null
    if (
      pickedTenantId &&
      pinGroups.some((g) => g.primary.tenantId === pickedTenantId)
    ) {
      return pickedTenantId
    }
    return pinGroups[0].primary.tenantId
  }, [pinGroups, pickedTenantId])

  /**
   * Fit the camera to all pins on first load, accounting for the carousel
   * overlay. Only runs once — further selection changes use `easeTo` instead
   * so the user keeps the zoom level they chose.
   */
  const handleLoad = useCallback(() => {
    const map = mapRef.current
    if (!map || didFitRef.current) return
    didFitRef.current = true
    if (pinGroups.length === 0) return

    if (pinGroups.length === 1) {
      map.easeTo({
        center: [pinGroups[0].longitude, pinGroups[0].latitude],
        zoom: 14,
        duration: 0,
      })
      return
    }

    let minLng = Infinity
    let minLat = Infinity
    let maxLng = -Infinity
    let maxLat = -Infinity
    for (const g of pinGroups) {
      if (g.longitude < minLng) minLng = g.longitude
      if (g.longitude > maxLng) maxLng = g.longitude
      if (g.latitude < minLat) minLat = g.latitude
      if (g.latitude > maxLat) maxLat = g.latitude
    }
    if (userCoords) {
      if (userCoords.lng < minLng) minLng = userCoords.lng
      if (userCoords.lng > maxLng) maxLng = userCoords.lng
      if (userCoords.lat < minLat) minLat = userCoords.lat
      if (userCoords.lat > maxLat) maxLat = userCoords.lat
    }
    map.fitBounds(
      [
        [minLng, minLat],
        [maxLng, maxLat],
      ],
      {
        padding: { top: 80, right: 48, bottom: CAROUSEL_PADDING_PX, left: 48 },
        maxZoom: 14,
        duration: 0,
      },
    )
  }, [pinGroups, userCoords])

  /**
   * On marker tap (or when the carousel swipes into a different card), ease
   * the camera to the target pin. We keep the current zoom unless the user
   * was zoomed out very far.
   */
  const focusTenant = useCallback(
    (tenantId: string) => {
      const pin = pinGroups.find((g) => g.primary.tenantId === tenantId)
      if (!pin) return
      setPickedTenantId(tenantId)
      const map = mapRef.current
      if (!map) return
      const currentZoom = map.getZoom()
      map.easeTo({
        center: [pin.longitude, pin.latitude],
        zoom: currentZoom < 12 ? 13 : currentZoom,
        duration: 500,
        offset: [0, -CAROUSEL_PADDING_PX / 4],
      })
    },
    [pinGroups],
  )

  // Initial viewport — prefer user's coords, fall back to Manila.
  const initialViewState = useMemo(() => {
    if (userCoords) {
      return { latitude: userCoords.lat, longitude: userCoords.lng, zoom: 12 }
    }
    return MANILA_FALLBACK
  }, [userCoords])

  if (!MAPBOX_TOKEN) {
    return (
      <div className="flex h-[60vh] items-center justify-center rounded-2xl border border-border bg-card text-sm text-muted-foreground">
        {t('mapUnavailable')}
      </div>
    )
  }

  return (
    <div className="relative h-[calc(100svh-180px)] min-h-[480px] overflow-hidden rounded-2xl border border-border bg-muted">
      <ReactMapGL
        ref={mapRef}
        mapboxAccessToken={MAPBOX_TOKEN}
        initialViewState={initialViewState}
        mapStyle="mapbox://styles/mapbox/streets-v12"
        onLoad={handleLoad}
        style={{ width: '100%', height: '100%' }}
      >
        <NavigationControl position="top-right" showCompass={false} />
        <GeolocateControl
          position="top-right"
          trackUserLocation
          showUserHeading
          positionOptions={{ enableHighAccuracy: false }}
        />

        {userCoords && (
          <Marker
            latitude={userCoords.lat}
            longitude={userCoords.lng}
            anchor="center"
          >
            <div
              className="h-4 w-4 rounded-full border-2 border-white bg-[color:var(--splash-500)] shadow-md"
              aria-hidden
            />
          </Marker>
        )}

        {pinGroups.map((g) => {
          const active = g.primary.tenantId === selectedTenantId
          return (
            <Marker
              key={g.primary.tenantId}
              latitude={g.latitude}
              longitude={g.longitude}
              anchor="bottom"
              onClick={(e) => {
                // Prevent the map click handler from firing.
                e.originalEvent.stopPropagation()
                focusTenant(g.primary.tenantId)
              }}
            >
              <button
                type="button"
                aria-label={g.primary.tenantName}
                className={
                  'flex items-center justify-center transition-transform ' +
                  (active ? 'scale-110' : 'scale-100 hover:scale-105')
                }
              >
                <PinSvg active={active} />
              </button>
            </Marker>
          )
        })}
      </ReactMapGL>

      {pinGroups.length === 0 && (
        <div className="pointer-events-none absolute inset-0 flex items-center justify-center p-6">
          <div className="pointer-events-auto rounded-2xl border border-border bg-background/95 px-4 py-3 text-center text-sm text-muted-foreground shadow-lg backdrop-blur">
            <MapPin className="mx-auto mb-1 h-4 w-4" aria-hidden />
            {t('mapEmpty')}
          </div>
        </div>
      )}

      {pinGroups.length > 0 && (
        <div className="pointer-events-none absolute inset-x-0 bottom-0 pb-3">
          <MapCardCarousel
            groups={pinGroups}
            selectedTenantId={selectedTenantId}
            onVisibleChange={focusTenant}
          />
        </div>
      )}
    </div>
  )
}

/**
 * A minimal SplashSphere-tinted pin. Active pins use the brand color and
 * a drop shadow; inactive pins lean on the muted palette so the active one
 * is easy to pick out at a glance.
 */
function PinSvg({ active }: { active: boolean }) {
  const fill = active ? 'var(--splash-500)' : 'var(--muted-foreground)'
  return (
    <svg
      width="32"
      height="40"
      viewBox="0 0 32 40"
      xmlns="http://www.w3.org/2000/svg"
      style={{
        filter: active
          ? 'drop-shadow(0 4px 6px rgba(0,0,0,0.25))'
          : 'drop-shadow(0 2px 2px rgba(0,0,0,0.2))',
      }}
    >
      <path
        d="M16 0C7.163 0 0 7.163 0 16c0 11 16 24 16 24s16-13 16-24C32 7.163 24.837 0 16 0z"
        fill={fill}
      />
      <circle cx="16" cy="16" r="6" fill="white" />
    </svg>
  )
}

/**
 * Convenience icon for callers that want to advertise the map toggle with a
 * brand-consistent chip. Exported separately so the parent page can compose
 * it without importing `lucide-react` directly.
 */
export function MapToggleIcon() {
  return <Navigation className="h-4 w-4" aria-hidden />
}
