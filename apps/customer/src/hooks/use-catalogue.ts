'use client'

import { useQuery, type UseQueryResult } from '@tanstack/react-query'
import type { GlobalMakeDto, GlobalModelDto } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

/** React Query keys for the global vehicle catalogue. */
export const catalogueKeys = {
  all: ['catalogue'] as const,
  makes: () => [...catalogueKeys.all, 'makes'] as const,
  modelsByMake: (makeId: string) =>
    [...catalogueKeys.all, 'makes', makeId, 'models'] as const,
}

/**
 * Fetch the full list of active global vehicle makes. Cached aggressively —
 * the makes list rarely changes so a 1 hour stale window is fine.
 */
export function useMakes(): UseQueryResult<readonly GlobalMakeDto[]> {
  return useQuery({
    queryKey: catalogueKeys.makes(),
    queryFn: () => apiClient.get<readonly GlobalMakeDto[]>('/catalogue/makes'),
    staleTime: 1000 * 60 * 60,
  })
}

/**
 * Fetch the active models for a given make. Disabled until `makeId` is
 * supplied so the query only runs once the user picks a make.
 */
export function useModelsByMake(
  makeId: string | null | undefined,
): UseQueryResult<readonly GlobalModelDto[]> {
  return useQuery({
    queryKey: catalogueKeys.modelsByMake(makeId ?? ''),
    queryFn: () =>
      apiClient.get<readonly GlobalModelDto[]>(
        `/catalogue/makes/${makeId}/models`,
      ),
    enabled: Boolean(makeId),
    staleTime: 1000 * 60 * 60,
  })
}
