'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { ServiceCategory, PagedResult } from '@splashsphere/types'

export function useServiceCategories() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['service-categories'],
    queryFn: async () => {
      const token = await getToken()
      const result = await apiClient.get<PagedResult<ServiceCategory>>(
        '/service-categories?pageSize=100',
        token ?? undefined
      )
      return result.items as ServiceCategory[]
    },
    staleTime: 1000 * 60 * 10,
  })
}

export function useCreateServiceCategory() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { name: string; description?: string }) => {
      const token = await getToken()
      return apiClient.post<ServiceCategory>('/service-categories', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['service-categories'] }),
  })
}

export function useUpdateServiceCategory(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { name: string; description?: string }) => {
      const token = await getToken()
      return apiClient.put<ServiceCategory>(`/service-categories/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['service-categories'] }),
  })
}

export function useToggleServiceCategory() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/service-categories/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['service-categories'] }),
  })
}
