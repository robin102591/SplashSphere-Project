'use client'

import { useQuery } from '@tanstack/react-query'
import type { ConnectHistoryItemDto } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

/**
 * Query key factory for the Connect history tab. Exposed so callers
 * (e.g. tests, invalidators) can key off the same string.
 */
export const historyQueryKey = (take: number) =>
  ['connect', 'history', { take }] as const

/** Server default is 50, max 200. We ask for 50 unless overridden. */
const DEFAULT_TAKE = 50

/**
 * Fetch the signed-in customer's completed transactions across every
 * tenant they've joined. The backend returns a flat `IReadOnlyList`
 * ordered newest-first — no pagination wrapper, so we use a plain
 * `useQuery`.
 */
export function useHistory(take: number = DEFAULT_TAKE) {
  return useQuery<ConnectHistoryItemDto[]>({
    queryKey: historyQueryKey(take),
    queryFn: () =>
      apiClient.get<ConnectHistoryItemDto[]>(`/history?take=${take}`),
    staleTime: 30_000,
  })
}
