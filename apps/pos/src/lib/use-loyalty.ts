'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { CustomerLoyaltySummaryDto } from '@splashsphere/types'

export function useCustomerLoyalty(customerId: string | null) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['customer-loyalty', customerId],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CustomerLoyaltySummaryDto | null>(
        `/loyalty/members/by-customer/${customerId}/summary`,
        token ?? undefined
      )
    },
    enabled: !!customerId,
    staleTime: 30_000,
  })
}
