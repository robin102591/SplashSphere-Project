'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Employee } from '@splashsphere/types'

export const employeeKeys = {
  all: ['employees'] as const,
  byBranch: (branchId: string) => ['employees', { branchId }] as const,
}

export function useEmployeesByBranch(branchId: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: employeeKeys.byBranch(branchId),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Employee[]>(
        `/employees?branchId=${encodeURIComponent(branchId)}`,
        token ?? undefined
      )
    },
    enabled: !!branchId,
  })
}
