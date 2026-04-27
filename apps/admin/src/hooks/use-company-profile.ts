'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  CompanyProfileDto,
  UpdateCompanyProfilePayload,
} from '@splashsphere/types'

const QK = ['settings', 'company'] as const

export function useCompanyProfile() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: QK,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CompanyProfileDto>('/settings/company', token ?? undefined)
    },
  })
}

export function useUpdateCompanyProfile() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: UpdateCompanyProfilePayload) => {
      const token = await getToken()
      return apiClient.put<void>('/settings/company', payload, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: QK }),
  })
}
