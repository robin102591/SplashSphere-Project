'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { PagedResult, TransactionSummary } from '@splashsphere/types'

export const transactionKeys = {
  all: ['transactions'] as const,
  byBranch: (branchId: string, page: number) =>
    ['transactions', { branchId, page }] as const,
}

export function useTransactionsByBranch(branchId: string, page = 1, pageSize = 20) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: transactionKeys.byBranch(branchId, page),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({
        branchId,
        page: String(page),
        pageSize: String(pageSize),
      })
      return apiClient.get<PagedResult<TransactionSummary>>(
        `/transactions?${qs.toString()}`,
        token ?? undefined
      )
    },
    enabled: !!branchId,
  })
}
