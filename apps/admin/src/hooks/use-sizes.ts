'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Size } from '@splashsphere/types'

export function useSizes() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['sizes'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Size[]>('/sizes', token ?? undefined)
    },
    staleTime: 1000 * 60 * 10,
  })
}
