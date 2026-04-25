'use client'

import { useCallback, useMemo, useRef, useState } from 'react'
import { useTranslations } from 'next-intl'
import { MapPin } from 'lucide-react'
import L, { type Map as LeafletMap } from 'leaflet'
import {
  AttributionControl,
  MapContainer,
  Marker,
  TileLayer,
  ZoomControl,
} from 'react-leaflet'
import type { TenantGroup } from './group-by-tenant'
import type { DiscoverySearchCoords } from '@/hooks/use-discovery'
import { MapCardCarousel } from './map-card-carousel'

/** Default OSM tile server. Override with NEXT_PUBLIC_MAP_TILE_URL in prod. */
const TILE_URL =
  process.env.NEXT_PUBLIC_MAP_TILE_URL ??
  'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'

const TILE_ATTRIBUTION =
  process.env.NEXT_PUBLIC_MAP_TILE_ATTRIBUTION ??
  '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'

/** Manila city center — fallback when the user denies geolocation and we have
 *  no pins to fit bounds to. */
const MANILA_FALLBACK: [number, number] = [14.5995, 120.9842]

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
 * Build a Leaflet `divIcon` for a tenant pin. Matches the SplashSphere brand
 * — splash-500 when active, muted-foreground otherwise — and renders an
 * inline SVG so we don't have to ship raster icon assets.
 */
function buildPinIcon(active: boolean): L.DivIcon {
  const fill = active ? 'var(--splash-500)' : 'var(--muted-foreground)'
  const shadow = active
    ? 'drop-shadow(0 4px 6px rgba(0,0,0,0.25))'
    : 'drop-shadow(0 2px 2px rgba(0,0,0,0.2))'
  const scale = active ? 'scale(1.15)' : 'scale(1)'
  return L.divIcon({
    className: 'splashsphere-pin',
    html: `
      <div style="filter:${shadow};transform:${scale};transform-origin:bottom center;transition:transform 150ms ease;">
        <svg width="32" height="40" viewBox="0 0 32 40" xmlns="http://www.w3.org/2000/svg">
          <path d="M16 0C7.163 0 0 7.163 0 16c0 11 16 24 16 24s16-13 16-24C32 7.163 24.837 0 16 0z" fill="${fill}" />
          <circle cx="16" cy="16" r="6" fill="white" />
        </svg>
      </div>
    `,
    iconSize: [32, 40],
    iconAnchor: [16, 40],
    popupAnchor: [0, -40],
  })
}

/** Small blue dot for the user's current location. */
const USER_LOCATION_ICON = L.divIcon({
  className: 'splashsphere-user-location',
  html: `
    <div style="
      width:14px;height:14px;border-radius:9999px;
      background:var(--splash-500);
      border:2px solid #fff;
      box-shadow:0 1px 4px rgba(0,0,0,0.4);
    "></div>
  `,
  iconSize: [14, 14],
  iconAnchor: [7, 7],
})

/**
 * Full-screen Leaflet view for the Discover screen. Pins are interactive —
 * tap to focus and sync the bottom carousel; swipe the carousel to pan the
 * map. Uses OpenStreetMap tiles by default — free, no API key required.
 */
export function DiscoverMapView({ groups, userCoords }: DiscoverMapViewProps) {
  const t = useTranslations('discover')
  const mapRef = useRef<LeafletMap | null>(null)
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
   * overlay. Only runs once — further selection changes use `flyTo` so the
   * user keeps the zoom level they chose.
   */
  const fitToPins = useCallback(
    (map: LeafletMap) => {
      if (didFitRef.current) return
      didFitRef.current = true
      if (pinGroups.length === 0) return

      if (pinGroups.length === 1) {
        map.setView([pinGroups[0].latitude, pinGroups[0].longitude], 14, {
          animate: false,
        })
        return
      }

      const points: [number, number][] = pinGroups.map((g) => [
        g.latitude,
        g.longitude,
      ])
      if (userCoords) points.push([userCoords.lat, userCoords.lng])
      const bounds = L.latLngBounds(points)
      map.fitBounds(bounds, {
        paddingTopLeft: [48, 80],
        paddingBottomRight: [48, CAROUSEL_PADDING_PX],
        maxZoom: 14,
        animate: false,
      })
    },
    [pinGroups, userCoords],
  )

  // The MapContainer ref is set asynchronously when the map mounts. We
  // capture it via the `ref` callback and immediately call fitToPins.
  const handleMapReady = useCallback(
    (map: LeafletMap | null) => {
      if (!map) return
      mapRef.current = map
      fitToPins(map)
    },
    [fitToPins],
  )

  /**
   * On marker tap (or when the carousel swipes into a different card), pan
   * the camera to the target pin. We keep the current zoom unless the user
   * is zoomed out very far.
   */
  const focusTenant = useCallback(
    (tenantId: string) => {
      const pin = pinGroups.find((g) => g.primary.tenantId === tenantId)
      if (!pin) return
      setPickedTenantId(tenantId)
      const map = mapRef.current
      if (!map) return
      const currentZoom = map.getZoom()
      map.flyTo([pin.latitude, pin.longitude], currentZoom < 12 ? 13 : currentZoom, {
        duration: 0.5,
      })
    },
    [pinGroups],
  )

  // Initial center — prefer user's coords, fall back to Manila.
  const initialCenter = useMemo<[number, number]>(
    () =>
      userCoords
        ? [userCoords.lat, userCoords.lng]
        : MANILA_FALLBACK,
    [userCoords],
  )

  return (
    <div className="relative h-[calc(100svh-180px)] min-h-[480px] overflow-hidden rounded-2xl border border-border bg-muted">
      <MapContainer
        ref={handleMapReady}
        center={initialCenter}
        zoom={12}
        zoomControl={false}
        attributionControl={false}
        scrollWheelZoom
        style={{ width: '100%', height: '100%' }}
      >
        <TileLayer url={TILE_URL} attribution={TILE_ATTRIBUTION} maxZoom={19} />
        <ZoomControl position="topright" />
        <AttributionControl position="bottomright" prefix={false} />

        {userCoords && (
          <Marker
            position={[userCoords.lat, userCoords.lng]}
            icon={USER_LOCATION_ICON}
            interactive={false}
          />
        )}

        {pinGroups.map((g) => {
          const active = g.primary.tenantId === selectedTenantId
          return (
            <Marker
              key={g.primary.tenantId}
              position={[g.latitude, g.longitude]}
              icon={buildPinIcon(active)}
              alt={g.primary.tenantName}
              keyboard
              eventHandlers={{
                click: () => focusTenant(g.primary.tenantId),
                keypress: (e) => {
                  // L.DomEvent forwards the original KeyboardEvent on `originalEvent`.
                  const key = (e.originalEvent as KeyboardEvent).key
                  if (key === 'Enter' || key === ' ') {
                    focusTenant(g.primary.tenantId)
                  }
                },
              }}
            />
          )
        })}
      </MapContainer>

      {pinGroups.length === 0 && (
        <div className="pointer-events-none absolute inset-0 z-[400] flex items-center justify-center p-6">
          <div className="pointer-events-auto rounded-2xl border border-border bg-background/95 px-4 py-3 text-center text-sm text-muted-foreground shadow-lg backdrop-blur">
            <MapPin className="mx-auto mb-1 h-4 w-4" aria-hidden />
            {t('mapEmpty')}
          </div>
        </div>
      )}

      {pinGroups.length > 0 && (
        <div className="pointer-events-none absolute inset-x-0 bottom-0 z-[400] pb-3">
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
