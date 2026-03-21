'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from './api-client'
import { useBranch } from './branch-context'
import type { ShiftDetailDto } from '@splashsphere/types'
import { ShiftStatus } from '@splashsphere/types'

/**
 * Returns the current open shift for the authenticated cashier at the active branch.
 * Used throughout the POS to show shift status and gate transactions.
 */
export function useCurrentShift() {
  const { getToken } = useAuth()
  const { branchId } = useBranch()

  return useQuery({
    queryKey: ['current-shift', branchId],
    enabled: !!branchId,
    staleTime: 30_000,
    refetchInterval: 60_000,
    queryFn: async (): Promise<ShiftDetailDto | null> => {
      const token = await getToken()
      try {
        return await apiClient.get<ShiftDetailDto>(
          `/shifts/current?branchId=${branchId}`,
          token ?? undefined,
        )
      } catch (err: unknown) {
        // 404 = no open shift
        if ((err as { status?: number })?.status === 404) return null
        throw err
      }
    },
  })
}

export function isShiftOpen(shift: ShiftDetailDto | null | undefined): boolean {
  return shift?.status === ShiftStatus.Open
}
