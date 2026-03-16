'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { PagedResult, TransactionSummary, TransactionDetail } from '@splashsphere/types'
import type { TransactionStatus } from '@splashsphere/types'

export const transactionKeys = {
  all: ['transactions'] as const,
  list: (params: TransactionListParams) => ['transactions', 'list', params] as const,
  detail: (id: string) => ['transactions', id] as const,
  // Keep legacy key for branches detail page
  byBranch: (branchId: string, page: number) =>
    ['transactions', { branchId, page }] as const,
}

export interface TransactionListParams {
  branchId?: string
  status?: TransactionStatus
  dateFrom?: string
  dateTo?: string
  search?: string
  page?: number
  pageSize?: number
}

export function useTransactions(params: TransactionListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: transactionKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.status != null) qs.set('status', String(params.status))
      if (params.dateFrom) qs.set('dateFrom', params.dateFrom)
      if (params.dateTo) qs.set('dateTo', params.dateTo)
      if (params.search) qs.set('search', params.search)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<TransactionSummary>>(
        `/transactions?${qs.toString()}`,
        token ?? undefined
      )
    },
    enabled: !!params.branchId,
  })
}

export function useTransaction(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: transactionKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<TransactionDetail>(`/transactions/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

// Legacy export kept for branches detail page compatibility
export function useTransactionsByBranch(branchId: string, page = 1, pageSize = 20) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: transactionKeys.byBranch(branchId, page),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ branchId, page: String(page), pageSize: String(pageSize) })
      return apiClient.get<PagedResult<TransactionSummary>>(
        `/transactions?${qs.toString()}`,
        token ?? undefined
      )
    },
    enabled: !!branchId,
  })
}
