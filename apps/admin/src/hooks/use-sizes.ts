'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Size, PagedResult } from '@splashsphere/types'

export function useSizes() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['sizes'],
    queryFn: async () => {
      const token = await getToken()
      const result = await apiClient.get<PagedResult<Size>>(
        '/sizes?pageSize=100',
        token ?? undefined
      )
      return result.items as Size[]
    },
    staleTime: 1000 * 60 * 10,
  })
}

export function useCreateSize() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (name: string) => {
      const token = await getToken()
      return apiClient.post<Size>('/sizes', { name }, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sizes'] }),
  })
}

export function useUpdateSize(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (name: string) => {
      const token = await getToken()
      return apiClient.put<Size>(`/sizes/${id}`, { name }, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sizes'] }),
  })
}

export function useToggleSize() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/sizes/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['sizes'] }),
  })
}
