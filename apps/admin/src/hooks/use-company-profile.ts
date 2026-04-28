'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  CompanyProfileDto,
  UpdateCompanyProfilePayload,
  UploadLogoResult,
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

/**
 * Upload a logo via multipart/form-data. Server resizes to 500/200/80px PNG
 * variants and returns the three public URLs. Invalidates the company
 * profile query so the form re-renders with the new logo.
 */
export function useUploadLogo() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (file: File) => {
      const token = await getToken()
      const formData = new FormData()
      formData.append('file', file)
      return apiClient.upload<UploadLogoResult>(
        '/settings/company/logo',
        formData,
        token ?? undefined,
      )
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: QK }),
  })
}

export function useDeleteLogo() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const token = await getToken()
      return apiClient.delete<void>('/settings/company/logo', token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: QK }),
  })
}
