'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  FranchiseSettingsDto, FranchiseeListItem, FranchiseeDetail,
  FranchiseAgreementDto, RoyaltyPeriodDto, NetworkSummaryDto,
  FranchiseComplianceItem, FranchiseBenchmarkDto, FranchiseServiceTemplateDto,
  InvitationDetailsDto, PagedResult,
} from '@splashsphere/types'

export const franchiseKeys = {
  settings: ['franchise', 'settings'] as const,
  franchisees: (page: number) => ['franchise', 'franchisees', page] as const,
  franchiseeDetail: (id: string) => ['franchise', 'franchisees', id] as const,
  royalties: (page: number, franchiseeId?: string, status?: number) =>
    ['franchise', 'royalties', page, franchiseeId, status] as const,
  networkSummary: ['franchise', 'network-summary'] as const,
  compliance: ['franchise', 'compliance'] as const,
  templates: ['franchise', 'templates'] as const,
  myAgreement: ['franchise', 'my-agreement'] as const,
  myRoyalties: (page: number) => ['franchise', 'my-royalties', page] as const,
  benchmarks: ['franchise', 'benchmarks'] as const,
}

// ── Franchisor Queries ──────────────────────────────────────────────────────

export function useFranchiseSettings() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.settings,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<FranchiseSettingsDto>('/franchise/settings', token ?? undefined)
    },
  })
}

export function useFranchisees(page: number) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.franchisees(page),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PagedResult<FranchiseeListItem>>(
        `/franchise/franchisees?page=${page}&pageSize=10`, token ?? undefined)
    },
  })
}

export function useFranchiseeDetail(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.franchiseeDetail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<FranchiseeDetail>(`/franchise/franchisees/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useRoyaltyPeriods(page: number, franchiseeId?: string, status?: number) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.royalties(page, franchiseeId, status),
    queryFn: async () => {
      const token = await getToken()
      const params = new URLSearchParams({ page: String(page), pageSize: '10' })
      if (franchiseeId) params.set('franchiseeId', franchiseeId)
      if (status !== undefined) params.set('status', String(status))
      return apiClient.get<PagedResult<RoyaltyPeriodDto>>(
        `/franchise/royalties?${params}`, token ?? undefined)
    },
  })
}

export function useNetworkSummary() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.networkSummary,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<NetworkSummaryDto>('/franchise/network-summary', token ?? undefined)
    },
  })
}

export function useComplianceReport() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.compliance,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<FranchiseComplianceItem[]>('/franchise/compliance', token ?? undefined)
    },
  })
}

export function useServiceTemplates() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.templates,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<FranchiseServiceTemplateDto[]>('/franchise/templates', token ?? undefined)
    },
  })
}

// ── Franchisor Mutations ────────────────────────────────────────────────────

export function useUpdateFranchiseSettings() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: Partial<FranchiseSettingsDto>) => {
      const token = await getToken()
      return apiClient.put<FranchiseSettingsDto>('/franchise/settings', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: franchiseKeys.settings }),
  })
}

export function useCreateAgreement() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      franchiseeId: string
      startDate: string
      endDate: string
      royaltyPercent: number
      marketingFeePercent: number
      technologyFeeFlat: number
    }) => {
      const token = await getToken()
      return apiClient.post<FranchiseAgreementDto>('/franchise/agreements', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['franchise', 'franchisees'] }),
  })
}

export function useSuspendFranchisee() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.post<void>(`/franchise/franchisees/${id}/suspend`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['franchise', 'franchisees'] }),
  })
}

export function useReactivateFranchisee() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.post<void>(`/franchise/franchisees/${id}/reactivate`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['franchise', 'franchisees'] }),
  })
}

export function useUpsertServiceTemplate() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      id?: string
      name: string
      description: string
      categoryId: string
      basePrice: number
      estimatedDuration: number
    }) => {
      const token = await getToken()
      return apiClient.post<FranchiseServiceTemplateDto>(
        '/franchise/templates', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: franchiseKeys.templates }),
  })
}

export function usePushServiceTemplates() {
  const { getToken } = useAuth()
  return useMutation({
    mutationFn: async (data: { franchiseeId: string; templateIds: string[] }) => {
      const token = await getToken()
      return apiClient.post<void>(
        `/franchise/franchisees/${data.franchiseeId}/push-templates`,
        { templateIds: data.templateIds },
        token ?? undefined)
    },
  })
}

export function useCalculateRoyalties() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { periodStart: string; periodEnd: string }) => {
      const token = await getToken()
      return apiClient.post<void>('/franchise/royalties/calculate', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['franchise', 'royalties'] }),
  })
}

export function useMarkRoyaltyPaid() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/franchise/royalties/${id}/paid`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['franchise', 'royalties'] }),
  })
}

export function useInviteFranchisee() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      email: string
      businessName: string
      ownerName?: string
      franchiseCode?: string
      territoryName?: string
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/franchise/invite', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['franchise', 'franchisees'] }),
  })
}

// ── Franchisee Queries ──────────────────────────────────────────────────────

export function useMyAgreement() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.myAgreement,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<FranchiseAgreementDto>('/franchise/my-agreement', token ?? undefined)
    },
  })
}

export function useMyRoyalties(page: number) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.myRoyalties(page),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PagedResult<RoyaltyPeriodDto>>(
        `/franchise/my-royalties?page=${page}&pageSize=10`, token ?? undefined)
    },
  })
}

export function useBenchmarks() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: franchiseKeys.benchmarks,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<FranchiseBenchmarkDto>('/franchise/benchmarks', token ?? undefined)
    },
  })
}

// ── Invitation (public + auth) ────────────────────────────────────────────

export function useValidateInvitation(token: string) {
  return useQuery({
    queryKey: ['franchise', 'invitation', token] as const,
    queryFn: () =>
      apiClient.get<InvitationDetailsDto>(`/franchise/invitations/${token}/validate`),
    enabled: !!token,
    retry: false,
  })
}

export function useAcceptInvitation() {
  const { getToken } = useAuth()
  return useMutation({
    mutationFn: async (data: {
      token: string
      businessName: string
      email: string
      contactNumber: string
      address: string
      branchName: string
      branchCode: string
      branchAddress: string
      branchContactNumber: string
    }) => {
      const authToken = await getToken()
      return apiClient.post<{ id: string }>(
        `/franchise/invitations/${data.token}/accept`,
        data,
        authToken ?? undefined)
    },
  })
}
