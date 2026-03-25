'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { MerchandiseCategory, PagedResult } from '@splashsphere/types'

export const merchandiseCategoryKeys = {
  all: ['merchandise-categories'] as const,
  list: () => ['merchandise-categories', 'list'] as const,
  detail: (id: string) => ['merchandise-categories', id] as const,
}

export interface CreateCategoryValues {
  name: string
  description?: string
}

export interface UpdateCategoryValues {
  name: string
  description?: string
}

export function useMerchandiseCategories() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: merchandiseCategoryKeys.list(),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PagedResult<MerchandiseCategory>>(
        '/merchandise-categories?pageSize=100',
        token ?? undefined
      )
    },
  })
}

export function useCreateMerchandiseCategory() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateCategoryValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/merchandise-categories', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: merchandiseCategoryKeys.all }),
  })
}

export function useUpdateMerchandiseCategory(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: UpdateCategoryValues) => {
      const token = await getToken()
      return apiClient.put<void>(`/merchandise-categories/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: merchandiseCategoryKeys.detail(id) })
      qc.invalidateQueries({ queryKey: merchandiseCategoryKeys.all })
    },
  })
}

export function useToggleMerchandiseCategoryStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/merchandise-categories/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: merchandiseCategoryKeys.all }),
  })
}
