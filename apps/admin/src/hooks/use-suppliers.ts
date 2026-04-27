'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { SupplierDto } from '@splashsphere/types'

export function useSuppliers() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['suppliers'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<SupplierDto[]>('/suppliers', token ?? undefined)
    },
  })
}

export function useCreateSupplier() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      name: string; contactPerson?: string; phone?: string;
      email?: string; address?: string;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/suppliers', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['suppliers'] }),
  })
}

export function useUpdateSupplier() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; name: string; contactPerson?: string; phone?: string;
      email?: string; address?: string; isActive?: boolean;
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/suppliers/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['suppliers'] }),
  })
}
