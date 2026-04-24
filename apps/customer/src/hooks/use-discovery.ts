'use client'

import { useQuery, type UseQueryResult } from '@tanstack/react-query'
import type { ConnectDiscoveryResultDto } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

/** Optional geolocation payload used to sort results by straight-line distance. */
export interface DiscoverySearchCoords {
  lat: number
  lng: number
}

/** React Query keys for the Connect discovery domain. */
export const discoveryKeys = {
  all: ['discover'] as const,
  search: (query: string, coords: DiscoverySearchCoords | null) =>
    [
      ...discoveryKeys.all,
      'search',
      query,
      coords ? `${coords.lat.toFixed(4)},${coords.lng.toFixed(4)}` : 'no-geo',
    ] as const,
}

/**
 * Search the public car-wash directory. Debouncing is the caller's
 * responsibility — pass a stable `query` string. Passing an empty string
 * returns the default/nearby list (server-sorted by distance when coords are
 * provided, alphabetical otherwise). `take` is clamped server-side (1..200).
 */
export function useDiscoverySearch(
  query: string,
  coords: DiscoverySearchCoords | null,
  take: number = 50,
): UseQueryResult<readonly ConnectDiscoveryResultDto[]> {
  const params = new URLSearchParams()
  if (query.trim().length > 0) params.set('search', query.trim())
  if (coords) {
    params.set('lat', coords.lat.toString())
    params.set('lng', coords.lng.toString())
  }
  params.set('take', String(take))

  const qs = params.toString()
  const path = `/carwashes${qs ? `?${qs}` : ''}`

  return useQuery({
    queryKey: discoveryKeys.search(query.trim(), coords),
    queryFn: () =>
      apiClient.get<readonly ConnectDiscoveryResultDto[]>(path),
    // Keep the previous page of results visible while a new query is in
    // flight — avoids a flash of skeletons on every keystroke.
    placeholderData: (prev) => prev,
  })
}
