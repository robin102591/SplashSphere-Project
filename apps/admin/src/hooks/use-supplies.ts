'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  SupplyCategoryDto,
  SupplyItemDto,
  SupplyItemDetailDto,
  PagedResult,
} from '@splashsphere/types'

// ── Supply Categories ────────────────────────────────────────────────────────

export function useSupplyCategories() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['supply-categories'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<SupplyCategoryDto[]>('/supplies/categories', token ?? undefined)
    },
  })
}

export function useCreateSupplyCategory() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { name: string; description?: string }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/supplies/categories', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['supply-categories'] }),
  })
}

// ── Supply Items ─────────────────────────────────────────────────────────────

export interface SupplyListParams {
  categoryId?: string
  branchId?: string
  stockStatus?: 'low' | 'out' | 'ok'
  page?: number
  pageSize?: number
}

export function useSupplies(params: SupplyListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['supplies', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.categoryId) qs.set('categoryId', params.categoryId)
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.stockStatus) qs.set('stockStatus', params.stockStatus)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<SupplyItemDto>>(`/supplies?${qs}`, token ?? undefined)
    },
  })
}

export function useSupplyById(id: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['supplies', id],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<SupplyItemDetailDto>(`/supplies/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useCreateSupply() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      branchId: string; categoryId?: string; name: string; description?: string;
      unit: string; reorderLevel?: number;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/supplies', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['supplies'] }),
  })
}

export function useUpdateSupply() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; categoryId?: string; name: string; description?: string;
      unit: string; reorderLevel?: number; isActive?: boolean;
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/supplies/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['supplies'] })
    },
  })
}

export function useDeleteSupply() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.delete<void>(`/supplies/${id}`, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['supplies'] }),
  })
}
