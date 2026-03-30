'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { TenantPlan, BillingRecord, CheckoutResult, PagedResult } from '@splashsphere/types'

export const planKeys = {
  plan: ['plan'] as const,
  billingHistory: (page: number) => ['billing', 'history', page] as const,
}

export function usePlan() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: planKeys.plan,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<TenantPlan>('/billing/plan', token ?? undefined)
    },
    staleTime: 60_000,
  })
}

export function useHasFeature(featureKey: string): boolean {
  const { data: plan } = usePlan()
  if (!plan) return true // Default to allowed while loading
  if (plan.status === 'suspended') return false
  if (plan.status === 'cancelled') return false
  if (plan.trial?.expired) return false
  return plan.features.includes(featureKey)
}

export function useBillingHistory(page: number = 1) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: planKeys.billingHistory(page),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PagedResult<BillingRecord>>(
        `/billing/history?page=${page}&pageSize=10`, token ?? undefined)
    },
  })
}

export function useCreateCheckout() {
  const { getToken } = useAuth()
  return useMutation({
    mutationFn: async (data: { targetPlan: number; successUrl: string; cancelUrl: string }) => {
      const token = await getToken()
      return apiClient.post<CheckoutResult>('/billing/checkout', data, token ?? undefined)
    },
  })
}

export function useChangePlan() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (newPlan: number) => {
      const token = await getToken()
      return apiClient.post<void>('/billing/change-plan', { newPlan }, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: planKeys.plan }),
  })
}

export function useCancelSubscription() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const token = await getToken()
      return apiClient.post<void>('/billing/cancel', {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: planKeys.plan }),
  })
}
