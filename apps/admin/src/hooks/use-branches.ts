'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Branch, PagedResult } from '@splashsphere/types'

export const branchKeys = {
  all: ['branches'] as const,
  detail: (id: string) => ['branches', id] as const,
}

export function useBranches() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: branchKeys.all,
    queryFn: async () => {
      const token = await getToken()
      const result = await apiClient.get<PagedResult<Branch>>('/branches', token ?? undefined)
      return result.items as Branch[]
    },
  })
}

export function useBranch(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: branchKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Branch>(`/branches/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export interface BranchFormValues {
  name: string
  code: string
  address: string
  contactNumber: string
}

export function useCreateBranch() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: BranchFormValues) => {
      const token = await getToken()
      return apiClient.post<Branch>('/branches', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: branchKeys.all }),
  })
}

export function useUpdateBranch(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: BranchFormValues) => {
      const token = await getToken()
      return apiClient.put<Branch>(`/branches/${id}`, data, token ?? undefined)
    },
    onSuccess: (updated) => {
      qc.setQueryData(branchKeys.detail(id), updated)
      qc.invalidateQueries({ queryKey: branchKeys.all })
    },
  })
}

export function useToggleBranchStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, isActive }: { id: string; isActive: boolean }) => {
      const token = await getToken()
      return apiClient.patch<Branch>(`/branches/${id}/status`, { isActive }, token ?? undefined)
    },
    onSuccess: (updated) => {
      qc.setQueryData(branchKeys.detail(updated.id), updated)
      qc.invalidateQueries({ queryKey: branchKeys.all })
    },
  })
}
