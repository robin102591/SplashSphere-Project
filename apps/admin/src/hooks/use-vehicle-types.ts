'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { VehicleType, PagedResult } from '@splashsphere/types'

export function useVehicleTypes() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['vehicle-types'],
    queryFn: async () => {
      const token = await getToken()
      const result = await apiClient.get<PagedResult<VehicleType>>(
        '/vehicle-types?pageSize=100',
        token ?? undefined
      )
      return result.items as VehicleType[]
    },
    staleTime: 1000 * 60 * 10,
  })
}

export function useCreateVehicleType() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (name: string) => {
      const token = await getToken()
      return apiClient.post<VehicleType>('/vehicle-types', { name }, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['vehicle-types'] }),
  })
}

export function useUpdateVehicleType(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (name: string) => {
      const token = await getToken()
      return apiClient.put<VehicleType>(`/vehicle-types/${id}`, { name }, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['vehicle-types'] }),
  })
}

export function useToggleVehicleType() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/vehicle-types/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['vehicle-types'] }),
  })
}
