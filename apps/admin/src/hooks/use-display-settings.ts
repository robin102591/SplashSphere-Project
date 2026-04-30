'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  DisplaySettingDto,
  UpdateDisplaySettingPayload,
} from '@splashsphere/types'

const QK = (branchId?: string | null) =>
  ['settings', 'display', branchId ?? null] as const

/** Fetch the display setting for the given branch (null = tenant default, with fallback). */
export function useDisplaySetting(branchId?: string | null) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: QK(branchId),
    queryFn: async () => {
      const token = await getToken()
      const path = branchId
        ? `/settings/display?branchId=${encodeURIComponent(branchId)}`
        : '/settings/display'
      return apiClient.get<DisplaySettingDto>(path, token ?? undefined)
    },
  })
}

/** Upsert the tenant-default (or a per-branch override) display setting. */
export function useUpdateDisplaySetting() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({
      payload,
      branchId,
    }: {
      payload: UpdateDisplaySettingPayload
      branchId?: string | null
    }) => {
      const token = await getToken()
      const path = branchId
        ? `/settings/display?branchId=${encodeURIComponent(branchId)}`
        : '/settings/display'
      return apiClient.put<void>(path, payload, token ?? undefined)
    },
    onSuccess: (_data, vars) =>
      qc.invalidateQueries({ queryKey: QK(vars.branchId ?? null) }),
  })
}

/**
 * Remove a per-branch override; the branch reverts to the tenant default.
 * The tenant default itself cannot be deleted — server returns 400 without
 * a branchId.
 */
export function useDeleteDisplayBranchOverride() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (branchId: string) => {
      const token = await getToken()
      return apiClient.delete<void>(
        `/settings/display?branchId=${encodeURIComponent(branchId)}`,
        token ?? undefined,
      )
    },
    onSuccess: (_data, branchId) =>
      qc.invalidateQueries({ queryKey: QK(branchId) }),
  })
}
