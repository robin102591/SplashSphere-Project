'use client'

import { useAuth } from '@clerk/nextjs'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  PagedResult,
  ServiceSummary,
  ServiceDetail,
  ServicePricingRow,
  ServiceCommissionRow,
} from '@splashsphere/types'
import type { CommissionType } from '@splashsphere/types'

export const serviceKeys = {
  all:    ['services'] as const,
  list:   (p: ServiceListParams) => ['services', 'list', p] as const,
  detail: (id: string)           => ['services', id] as const,
}

export interface ServiceListParams {
  categoryId?: string
  search?: string
  page?: number
  pageSize?: number
}

export interface ServiceFormValues {
  categoryId: string
  name: string
  basePrice: number
  description?: string
}

export interface PricingRowPayload {
  vehicleTypeId: string
  sizeId: string
  price: number
}

export interface CommissionRowPayload {
  vehicleTypeId: string
  sizeId: string
  type: CommissionType
  fixedAmount: number | null
  percentageRate: number | null
}

// ── Queries ───────────────────────────────────────────────────────────────────

export function useServices(params: ServiceListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: serviceKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.categoryId) qs.set('categoryId', params.categoryId)
      if (params.search)     qs.set('search', params.search)
      if (params.page)       qs.set('page', String(params.page))
      if (params.pageSize)   qs.set('pageSize', String(params.pageSize))
      const query = qs.toString()
      return apiClient.get<PagedResult<ServiceSummary>>(
        `/services${query ? `?${query}` : ''}`,
        token ?? undefined
      )
    },
  })
}

export function useService(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: serviceKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ServiceDetail>(`/services/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

// ── Mutations ─────────────────────────────────────────────────────────────────

export function useCreateService() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: ServiceFormValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/services', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: serviceKeys.all }),
  })
}

export function useUpdateService(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: ServiceFormValues) => {
      const token = await getToken()
      return apiClient.put<void>(`/services/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: serviceKeys.detail(id) })
      qc.invalidateQueries({ queryKey: serviceKeys.all })
    },
  })
}

export function useToggleServiceStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/services/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: (_v, id) => {
      qc.invalidateQueries({ queryKey: serviceKeys.detail(id) })
      qc.invalidateQueries({ queryKey: serviceKeys.all })
    },
  })
}

export function useUpsertServicePricing(serviceId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (rows: PricingRowPayload[]) => {
      const token = await getToken()
      return apiClient.put<void>(
        `/services/${serviceId}/pricing`,
        { rows },
        token ?? undefined
      )
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: serviceKeys.detail(serviceId) }),
  })
}

export function useUpsertServiceCommissions(serviceId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (rows: CommissionRowPayload[]) => {
      const token = await getToken()
      return apiClient.put<void>(
        `/services/${serviceId}/commissions`,
        { rows },
        token ?? undefined
      )
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: serviceKeys.detail(serviceId) }),
  })
}
