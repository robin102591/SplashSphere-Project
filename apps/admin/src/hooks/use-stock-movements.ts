'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { StockMovementDto, PagedResult } from '@splashsphere/types'

export interface StockMovementListParams {
  supplyItemId?: string
  branchId?: string
  type?: number
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export function useStockMovements(params: StockMovementListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['stock-movements', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.supplyItemId) qs.set('supplyItemId', params.supplyItemId)
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.type != null) qs.set('type', String(params.type))
      if (params.from) qs.set('from', params.from)
      if (params.to) qs.set('to', params.to)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<StockMovementDto>>(`/stock-movements?${qs}`, token ?? undefined)
    },
  })
}

export function useRecordStockMovement() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      supplyItemId: string; type: number; quantity: number;
      unitCost?: number; reference?: string; notes?: string;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/stock-movements', data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['stock-movements'] })
      qc.invalidateQueries({ queryKey: ['supplies'] })
    },
  })
}

export function useRecordBulkUsage() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      transactionId?: string; items: { supplyItemId: string; quantity: number }[];
      notes?: string;
    }) => {
      const token = await getToken()
      return apiClient.post<void>('/stock-movements/bulk-usage', data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['stock-movements'] })
      qc.invalidateQueries({ queryKey: ['supplies'] })
    },
  })
}
