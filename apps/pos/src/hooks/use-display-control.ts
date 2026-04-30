'use client'

import { useCallback } from 'react'
import { useAuth } from '@clerk/nextjs'
import { apiClient } from '@/lib/api-client'
import { useBranch } from '@/lib/branch-context'

/**
 * Cashier-side actions that explicitly steer the customer display, used when
 * a transaction-lifecycle event isn't enough to keep the display in sync:
 *
 * - <code>show(transactionId)</code>: pushes a transaction to the station's
 *   paired display. Call this whenever the cashier opens a transaction
 *   page (whether via /transactions/new after creating one, or via
 *   /transactions/[id] for an existing Pay-Later transaction). This makes
 *   the display follow the cashier's focus.
 *
 * - <code>clear()</code>: reverts the station's display to Idle. Call this
 *   after Pay Later or any other "park & walk away" flow — the customer's
 *   car is being washed, the cashier is free, the screen should be ready
 *   for the next person at the counter.
 *
 * Both actions silently no-op when no station is selected (the cashier is
 * still doing valid work; just no display routing).
 */
export function useDisplayControl() {
  const { getToken } = useAuth()
  const { branchId, stationId } = useBranch()

  const show = useCallback(
    async (transactionId: string) => {
      if (!stationId) return
      try {
        const token = await getToken()
        await apiClient.post<void>(
          `/display/show/${transactionId}`,
          {},
          token ?? undefined,
        )
      } catch {
        // Best-effort — display steering must never block the cashier.
      }
    },
    [getToken, stationId],
  )

  const clear = useCallback(async () => {
    if (!branchId || !stationId) return
    try {
      const token = await getToken()
      await apiClient.post<void>(
        '/display/clear',
        { branchId, stationId },
        token ?? undefined,
      )
    } catch {
      // Best-effort.
    }
  }, [getToken, branchId, stationId])

  return { show, clear }
}
