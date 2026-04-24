'use client'

import {
  useMutation,
  useQuery,
  useQueryClient,
  type UseMutationResult,
  type UseQueryResult,
} from '@tanstack/react-query'
import type {
  ConnectLoyaltyCardDto,
  ConnectPointTransactionDto,
  ConnectRewardDto,
  RedeemRewardResponseDto,
} from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

/**
 * React Query keys for the Connect loyalty domain at a single tenant.
 * Card / rewards / history are cached independently so a redemption can
 * invalidate the trio and nothing else.
 */
export const loyaltyKeys = {
  all: ['loyalty'] as const,
  card: (tenantId: string) => [...loyaltyKeys.all, 'card', tenantId] as const,
  rewards: (tenantId: string) =>
    [...loyaltyKeys.all, 'rewards', tenantId] as const,
  history: (tenantId: string) =>
    [...loyaltyKeys.all, 'history', tenantId] as const,
}

/**
 * Fetch the signed-in Connect user's membership card at a single tenant.
 * Always returns a DTO — `isEnrolled === false` indicates the tenant
 * doesn't offer loyalty or the user hasn't earned a card yet.
 */
export function useLoyaltyCard(
  tenantId: string | null | undefined,
): UseQueryResult<ConnectLoyaltyCardDto> {
  return useQuery({
    queryKey: loyaltyKeys.card(tenantId ?? ''),
    queryFn: () =>
      apiClient.get<ConnectLoyaltyCardDto>(`/carwashes/${tenantId}/loyalty`),
    enabled: Boolean(tenantId),
  })
}

/**
 * List the tenant's available rewards with affordability flags derived
 * from the caller's current points balance. Empty list when the tenant
 * does not offer loyalty.
 */
export function useRewards(
  tenantId: string | null | undefined,
): UseQueryResult<readonly ConnectRewardDto[]> {
  return useQuery({
    queryKey: loyaltyKeys.rewards(tenantId ?? ''),
    queryFn: () =>
      apiClient.get<readonly ConnectRewardDto[]>(
        `/carwashes/${tenantId}/rewards`,
      ),
    enabled: Boolean(tenantId),
  })
}

/**
 * Fetch the last N points movements for this customer at this tenant,
 * newest first. Empty list when the customer isn't enrolled.
 */
export function usePointsHistory(
  tenantId: string | null | undefined,
  take?: number,
): UseQueryResult<readonly ConnectPointTransactionDto[]> {
  return useQuery({
    queryKey: [...loyaltyKeys.history(tenantId ?? ''), take ?? null] as const,
    queryFn: () =>
      apiClient.get<readonly ConnectPointTransactionDto[]>(
        `/carwashes/${tenantId}/points-history${
          take ? `?take=${take}` : ''
        }`,
      ),
    enabled: Boolean(tenantId),
  })
}

/**
 * Redeem a reward. On success invalidates the loyalty card, rewards list
 * (affordability flags recompute), and points history so the UI re-reads
 * all three in one go.
 */
export function useRedeemReward(
  tenantId: string,
): UseMutationResult<RedeemRewardResponseDto, unknown, string> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (rewardId: string) =>
      apiClient.post<RedeemRewardResponseDto>(
        `/carwashes/${tenantId}/rewards/redeem`,
        { rewardId },
      ),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: loyaltyKeys.card(tenantId) })
      qc.invalidateQueries({ queryKey: loyaltyKeys.rewards(tenantId) })
      qc.invalidateQueries({ queryKey: loyaltyKeys.history(tenantId) })
    },
  })
}
