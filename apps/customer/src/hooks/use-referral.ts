'use client'

import {
  useQuery,
  type UseQueryResult,
} from '@tanstack/react-query'
import type {
  ConnectReferralCodeDto,
  ConnectReferralHistoryItemDto,
} from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

/**
 * React Query keys for the Connect referral domain, scoped per tenant
 * because every joined car wash issues its own code.
 */
export const referralKeys = {
  all: ['referral'] as const,
  code: (tenantId: string) => [...referralKeys.all, 'code', tenantId] as const,
  history: (tenantId: string) =>
    [...referralKeys.all, 'history', tenantId] as const,
}

/**
 * Fetch (and lazily issue on first call) the caller's referral code at a
 * tenant. Disabled until `tenantId` is set. Failures surface through the
 * query's `error` field — the most common is the tenant not offering a
 * loyalty program.
 */
export function useReferralCode(
  tenantId: string | null | undefined,
): UseQueryResult<ConnectReferralCodeDto> {
  return useQuery({
    queryKey: referralKeys.code(tenantId ?? ''),
    queryFn: () =>
      apiClient.get<ConnectReferralCodeDto>(
        `/carwashes/${tenantId}/referral-code`,
      ),
    enabled: Boolean(tenantId),
  })
}

/**
 * List the referrals this customer has made at a tenant (only rows where
 * someone actually used the code — pending-but-unused codes are just the
 * caller's own code and aren't shown here). Newest first.
 */
export function useReferralHistory(
  tenantId: string | null | undefined,
): UseQueryResult<readonly ConnectReferralHistoryItemDto[]> {
  return useQuery({
    queryKey: referralKeys.history(tenantId ?? ''),
    queryFn: () =>
      apiClient.get<readonly ConnectReferralHistoryItemDto[]>(
        `/carwashes/${tenantId}/referrals`,
      ),
    enabled: Boolean(tenantId),
  })
}
