'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  LoyaltyProgramSettingsDto,
  LoyaltyRewardDto,
  MembershipCardDto,
  PointTransactionDto,
  LoyaltyDashboardDto,
  CustomerLoyaltySummaryDto,
  PagedResult,
} from '@splashsphere/types'
import type { RewardType } from '@splashsphere/types'

// ── Settings ──────────────────────────────────────────────────────────────

export function useLoyaltySettings() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['loyalty-settings'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<LoyaltyProgramSettingsDto>('/loyalty/settings', token ?? undefined)
    },
  })
}

export function useUpsertLoyaltySettings() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      pointsPerCurrencyUnit: number
      currencyUnitAmount: number
      isActive: boolean
      pointsExpirationMonths: number | null
      autoEnroll: boolean
    }) => {
      const token = await getToken()
      return apiClient.put<void>('/loyalty/settings', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['loyalty-settings'] }),
  })
}

export function useUpsertLoyaltyTiers() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      tiers: {
        tier: number
        name: string
        minimumLifetimePoints: number
        pointsMultiplier: number
      }[]
    }) => {
      const token = await getToken()
      return apiClient.put<void>('/loyalty/tiers', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['loyalty-settings'] }),
  })
}

// ── Rewards ───────────────────────────────────────────────────────────────

export function useLoyaltyRewards(params: { activeOnly?: boolean; page?: number; pageSize?: number } = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['loyalty-rewards', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.activeOnly != null) qs.set('activeOnly', String(params.activeOnly))
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<LoyaltyRewardDto>>(`/loyalty/rewards?${qs}`, token ?? undefined)
    },
  })
}

export function useCreateLoyaltyReward() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      name: string
      description?: string
      rewardType: RewardType
      pointsCost: number
      serviceId?: string
      packageId?: string
      discountAmount?: number
      discountPercent?: number
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/loyalty/rewards', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['loyalty-rewards'] }),
  })
}

export function useUpdateLoyaltyReward() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string
      name: string
      description?: string
      rewardType: RewardType
      pointsCost: number
      serviceId?: string
      packageId?: string
      discountAmount?: number
      discountPercent?: number
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/loyalty/rewards/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['loyalty-rewards'] }),
  })
}

export function useToggleLoyaltyRewardStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/loyalty/rewards/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['loyalty-rewards'] }),
  })
}

// ── Members ───────────────────────────────────────────────────────────────

export function useMembershipCard(customerId: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['loyalty-member', customerId],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<MembershipCardDto>(`/loyalty/members/by-customer/${customerId}`, token ?? undefined)
    },
    enabled: !!customerId,
  })
}

export function useEnrollMember() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (customerId: string) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/loyalty/members', { customerId }, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['loyalty-member'] })
      qc.invalidateQueries({ queryKey: ['loyalty-dashboard'] })
    },
  })
}

export function usePointHistory(membershipCardId: string, params: { page?: number; pageSize?: number } = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['loyalty-points', membershipCardId, params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<PointTransactionDto>>(
        `/loyalty/members/${membershipCardId}/points?${qs}`,
        token ?? undefined
      )
    },
    enabled: !!membershipCardId,
  })
}

export function useRedeemPoints() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      membershipCardId: string
      rewardId: string
      transactionId?: string
    }) => {
      const token = await getToken()
      return apiClient.post<{ redemptionId: string; pointsDeducted: number; newBalance: number }>(
        `/loyalty/members/${data.membershipCardId}/redeem`,
        { rewardId: data.rewardId, transactionId: data.transactionId },
        token ?? undefined
      )
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['loyalty-member'] })
      qc.invalidateQueries({ queryKey: ['loyalty-points'] })
      qc.invalidateQueries({ queryKey: ['loyalty-dashboard'] })
    },
  })
}

export function useAdjustPoints() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      membershipCardId: string
      points: number
      reason: string
    }) => {
      const token = await getToken()
      return apiClient.post<void>(
        `/loyalty/members/${data.membershipCardId}/adjust`,
        { points: data.points, reason: data.reason },
        token ?? undefined
      )
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['loyalty-member'] })
      qc.invalidateQueries({ queryKey: ['loyalty-points'] })
      qc.invalidateQueries({ queryKey: ['loyalty-dashboard'] })
    },
  })
}

export function useCustomerLoyaltySummary(customerId: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['loyalty-summary', customerId],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CustomerLoyaltySummaryDto | null>(
        `/loyalty/members/by-customer/${customerId}/summary`,
        token ?? undefined
      )
    },
    enabled: !!customerId,
  })
}

// ── Dashboard ─────────────────────────────────────────────────────────────

export function useLoyaltyDashboard(from: string, to: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['loyalty-dashboard', from, to],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from, to })
      return apiClient.get<LoyaltyDashboardDto>(`/loyalty/dashboard?${qs}`, token ?? undefined)
    },
    enabled: !!from && !!to,
  })
}
