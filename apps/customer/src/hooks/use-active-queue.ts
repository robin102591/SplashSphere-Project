'use client'

import { useEffect, useState } from 'react'
import { useQuery, type UseQueryResult } from '@tanstack/react-query'
import type { ConnectActiveQueueDto } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

export const activeQueueQueryKey = ['connect', 'queue', 'active'] as const

/**
 * Poll the Connect user's currently active queue entry every 10s while
 * the tab is visible. Returns `null` when the backend responds with
 * `204 No Content` (no active entry).
 *
 * We pause polling when the tab is hidden (`visibilitychange`) so the
 * app doesn't burn cellular data / battery while the customer is
 * elsewhere. React Query would otherwise keep polling in the background
 * because its default schedulers don't key off document visibility.
 *
 * The backend endpoint: `GET /api/v1/connect/queue/active`. On 204 the
 * `apiClient` returns `undefined`, which we normalize to `null`.
 */
export function useActiveQueue(
  options?: { enabled?: boolean },
): UseQueryResult<ConnectActiveQueueDto | null> {
  const [isVisible, setIsVisible] = useState(
    typeof document === 'undefined'
      ? true
      : document.visibilityState === 'visible',
  )

  useEffect(() => {
    if (typeof document === 'undefined') return
    const onChange = () => setIsVisible(document.visibilityState === 'visible')
    document.addEventListener('visibilitychange', onChange)
    return () => {
      document.removeEventListener('visibilitychange', onChange)
    }
  }, [])

  const enabled = (options?.enabled ?? true) && isVisible

  return useQuery({
    queryKey: activeQueueQueryKey,
    queryFn: async () => {
      // `apiClient.get` returns `undefined` for 204 responses — normalize to null.
      const res = await apiClient.get<ConnectActiveQueueDto | undefined>(
        '/queue/active',
      )
      return (res ?? null) as ConnectActiveQueueDto | null
    },
    enabled,
    refetchInterval: isVisible ? 10_000 : false,
    refetchIntervalInBackground: false,
    staleTime: 5_000,
  })
}
