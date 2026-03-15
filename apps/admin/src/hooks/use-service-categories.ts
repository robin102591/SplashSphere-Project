'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { ServiceCategory } from '@splashsphere/types'

export function useServiceCategories() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['service-categories'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ServiceCategory[]>('/service-categories', token ?? undefined)
    },
    staleTime: 1000 * 60 * 10,
  })
}
