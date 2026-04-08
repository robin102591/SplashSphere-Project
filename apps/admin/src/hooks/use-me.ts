'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'

interface MeTenant {
  id: string
  name: string
  email: string
  contactNumber: string
  address: string
  isActive: boolean
  tenantType: number
  parentTenantId: string | null
  franchiseCode: string | null
}

interface MeResponse {
  id: string
  clerkUserId: string
  email: string
  firstName: string
  lastName: string
  fullName: string
  role: string | null
  isActive: boolean
  hasPin: boolean
  tenant: MeTenant | null
}

export const meKeys = {
  me: ['me'] as const,
}

export function useMe() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: meKeys.me,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<MeResponse>('/auth/me', token ?? undefined)
    },
    staleTime: 5 * 60_000,
  })
}

export function useTenantType() {
  const { data } = useMe()
  const tenantType = data?.tenant?.tenantType ?? null
  return {
    tenantType,
    isFranchisor: tenantType === 2,
    isFranchisee: tenantType === 3,
  }
}
