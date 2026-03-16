'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { DashboardSummary } from '@splashsphere/types'

export function useDashboardSummary(branchId?: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['dashboard-summary', { branchId }],
    queryFn: async () => {
      const token = await getToken()
      const qs = branchId ? `?branchId=${encodeURIComponent(branchId)}` : ''
      return apiClient.get<DashboardSummary>(`/dashboard/summary${qs}`, token ?? undefined)
    },
    refetchInterval: 60_000,
  })
}
