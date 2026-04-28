'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  ReceiptSettingDto,
  UpdateReceiptSettingPayload,
} from '@splashsphere/types'

const QK = (branchId?: string | null) =>
  ['settings', 'receipt', branchId ?? null] as const

/** Fetch the receipt setting that applies for the given branch (null = tenant default). */
export function useReceiptSetting(branchId?: string | null) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: QK(branchId),
    queryFn: async () => {
      const token = await getToken()
      const path = branchId
        ? `/settings/receipt?branchId=${encodeURIComponent(branchId)}`
        : '/settings/receipt'
      return apiClient.get<ReceiptSettingDto>(path, token ?? undefined)
    },
  })
}

/** Upsert the tenant-default (or a per-branch override) receipt setting. */
export function useUpdateReceiptSetting() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({
      payload,
      branchId,
    }: {
      payload: UpdateReceiptSettingPayload
      branchId?: string | null
    }) => {
      const token = await getToken()
      const path = branchId
        ? `/settings/receipt?branchId=${encodeURIComponent(branchId)}`
        : '/settings/receipt'
      return apiClient.put<void>(path, payload, token ?? undefined)
    },
    onSuccess: (_data, vars) =>
      qc.invalidateQueries({ queryKey: QK(vars.branchId ?? null) }),
  })
}

/**
 * Remove a per-branch override; the branch reverts to the tenant default.
 * The tenant default itself (branchId === null) cannot be deleted — the
 * server returns a 400 if called without a branchId.
 */
export function useDeleteReceiptBranchOverride() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (branchId: string) => {
      const token = await getToken()
      return apiClient.delete<void>(
        `/settings/receipt?branchId=${encodeURIComponent(branchId)}`,
        token ?? undefined,
      )
    },
    onSuccess: (_data, branchId) =>
      qc.invalidateQueries({ queryKey: QK(branchId) }),
  })
}
