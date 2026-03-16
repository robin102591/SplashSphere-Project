'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Merchandise, PagedResult } from '@splashsphere/types'

export const merchandiseKeys = {
  all: ['merchandise'] as const,
  list: (params: MerchandiseListParams) => ['merchandise', 'list', params] as const,
  detail: (id: string) => ['merchandise', id] as const,
}

export interface MerchandiseListParams {
  search?: string
  lowStockOnly?: boolean
  page?: number
  pageSize?: number
}

export interface CreateMerchandiseValues {
  name: string
  sku: string
  price: number
  stockQuantity: number
  lowStockThreshold: number
  description?: string
  costPrice?: number
}

export interface UpdateMerchandiseValues {
  name: string
  price: number
  lowStockThreshold: number
  description?: string
  costPrice?: number
}

export interface AdjustStockValues {
  adjustment: number
  reason?: string
}

export function useMerchandiseList(params: MerchandiseListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: merchandiseKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.search) qs.set('search', params.search)
      if (params.lowStockOnly) qs.set('lowStockOnly', 'true')
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<Merchandise>>(
        `/merchandise?${qs.toString()}`,
        token ?? undefined
      )
    },
  })
}

export function useMerchandiseItem(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: merchandiseKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Merchandise>(`/merchandise/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useCreateMerchandise() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateMerchandiseValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/merchandise', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: merchandiseKeys.all }),
  })
}

export function useUpdateMerchandise(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: UpdateMerchandiseValues) => {
      const token = await getToken()
      return apiClient.put<void>(`/merchandise/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: merchandiseKeys.detail(id) })
      qc.invalidateQueries({ queryKey: merchandiseKeys.all })
    },
  })
}

export function useToggleMerchandiseStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/merchandise/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: (_data, id) => {
      qc.invalidateQueries({ queryKey: merchandiseKeys.detail(id) })
      qc.invalidateQueries({ queryKey: merchandiseKeys.all })
    },
  })
}

export function useAdjustStock(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: AdjustStockValues) => {
      const token = await getToken()
      return apiClient.post<{ stockQuantity: number; isLowStock: boolean }>(
        `/merchandise/${id}/stock-adjustment`,
        data,
        token ?? undefined
      )
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: merchandiseKeys.detail(id) })
      qc.invalidateQueries({ queryKey: merchandiseKeys.all })
    },
  })
}
