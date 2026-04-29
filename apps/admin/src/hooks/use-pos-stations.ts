'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { PosStation } from '@splashsphere/types'

export const posStationKeys = {
  all: ['pos-stations'] as const,
  byBranch: (branchId: string) => ['pos-stations', branchId] as const,
}

export function usePosStations(branchId: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: posStationKeys.byBranch(branchId ?? ''),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PosStation[]>(`/branches/${branchId}/stations`, token ?? undefined)
    },
    enabled: !!branchId,
  })
}

export interface PosStationFormValues {
  name: string
}

export function useCreatePosStation(branchId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (values: PosStationFormValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>(`/branches/${branchId}/stations`, values, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: posStationKeys.byBranch(branchId) }),
  })
}

export function useUpdatePosStation(branchId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, name, isActive }: { id: string; name: string; isActive: boolean }) => {
      const token = await getToken()
      return apiClient.put<void>(
        `/branches/${branchId}/stations/${id}`,
        { name, isActive },
        token ?? undefined,
      )
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: posStationKeys.byBranch(branchId) }),
  })
}

export function useDeletePosStation(branchId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.delete<void>(`/branches/${branchId}/stations/${id}`, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: posStationKeys.byBranch(branchId) }),
  })
}
