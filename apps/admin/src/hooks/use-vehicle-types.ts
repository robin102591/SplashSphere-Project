'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { VehicleType } from '@splashsphere/types'

export function useVehicleTypes() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['vehicle-types'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<VehicleType[]>('/vehicle-types', token ?? undefined)
    },
    staleTime: 1000 * 60 * 10, // reference data — cache 10 min
  })
}
