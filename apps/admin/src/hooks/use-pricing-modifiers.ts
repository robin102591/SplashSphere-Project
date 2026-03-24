'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { PricingModifier, PagedResult } from '@splashsphere/types'
import type { ModifierType } from '@splashsphere/types'

export const pricingModifierKeys = {
  all: ['pricing-modifiers'] as const,
  list: (params?: { branchId?: string }) => ['pricing-modifiers', 'list', params] as const,
}

export interface PricingModifierFormValues {
  name: string
  type: ModifierType
  value: number
  branchId?: string
  startTime?: string
  endTime?: string
  activeDayOfWeek?: number
  holidayDate?: string
  holidayName?: string
  startDate?: string
  endDate?: string
}

export function usePricingModifiers(branchId?: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: pricingModifierKeys.list({ branchId }),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ pageSize: '100' })
      if (branchId) qs.set('branchId', branchId)
      const result = await apiClient.get<PagedResult<PricingModifier>>(
        `/pricing-modifiers?${qs}`,
        token ?? undefined
      )
      return (result?.items ?? []) as PricingModifier[]
    },
  })
}

export function useCreatePricingModifier() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: PricingModifierFormValues) => {
      const token = await getToken()
      return apiClient.post<PricingModifier>('/pricing-modifiers', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: pricingModifierKeys.all }),
  })
}

export function useUpdatePricingModifier(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: PricingModifierFormValues) => {
      const token = await getToken()
      return apiClient.put<PricingModifier>(`/pricing-modifiers/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: pricingModifierKeys.all }),
  })
}

export function useDeletePricingModifier() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.delete<void>(`/pricing-modifiers/${id}`, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: pricingModifierKeys.all }),
  })
}

export function useTogglePricingModifier() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/pricing-modifiers/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: pricingModifierKeys.all }),
  })
}
