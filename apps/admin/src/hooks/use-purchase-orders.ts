'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  PurchaseOrderDto,
  PurchaseOrderDetailDto,
  PagedResult,
} from '@splashsphere/types'

export interface PurchaseOrderListParams {
  supplierId?: string
  branchId?: string
  status?: number
  page?: number
  pageSize?: number
}

export function usePurchaseOrders(params: PurchaseOrderListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['purchase-orders', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.supplierId) qs.set('supplierId', params.supplierId)
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.status != null) qs.set('status', String(params.status))
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<PurchaseOrderDto>>(`/purchase-orders?${qs}`, token ?? undefined)
    },
  })
}

export function usePurchaseOrderById(id: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['purchase-orders', id],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PurchaseOrderDetailDto>(`/purchase-orders/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useCreatePurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      supplierId: string; branchId: string; expectedDeliveryDate?: string; notes?: string;
      lines: { supplyItemId?: string; merchandiseId?: string; itemName: string; quantity: number; unitCost: number }[];
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/purchase-orders', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-orders'] }),
  })
}

export function useUpdatePurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; supplierId: string; expectedDeliveryDate?: string; notes?: string;
      lines: { supplyItemId?: string; merchandiseId?: string; itemName: string; quantity: number; unitCost: number }[];
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/purchase-orders/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-orders'] }),
  })
}

export function useSendPurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/purchase-orders/${id}/send`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-orders'] }),
  })
}

export function useReceivePurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; lines: { lineId: string; receivedQuantity: number; unitCost?: number }[];
    }) => {
      const token = await getToken()
      return apiClient.post<void>(`/purchase-orders/${id}/receive`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['purchase-orders'] })
      qc.invalidateQueries({ queryKey: ['supplies'] })
      qc.invalidateQueries({ queryKey: ['stock-movements'] })
    },
  })
}

export function useCancelPurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/purchase-orders/${id}/cancel`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-orders'] }),
  })
}
