'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { RevenueReport, CommissionsReport, ServicePopularityReport } from '@splashsphere/types'

export interface RevenueReportParams {
  from: string
  to: string
  branchId?: string
}

export interface CommissionsReportParams {
  from: string
  to: string
  branchId?: string
  employeeId?: string
}

export interface ServicePopularityParams {
  from: string
  to: string
  branchId?: string
}

export function useRevenueReport(params: RevenueReportParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'revenue', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from: params.from, to: params.to })
      if (params.branchId) qs.set('branchId', params.branchId)
      return apiClient.get<RevenueReport>(`/reports/revenue?${qs}`, token ?? undefined)
    },
    enabled: !!params.from && !!params.to,
  })
}

export function useCommissionsReport(params: CommissionsReportParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'commissions', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from: params.from, to: params.to })
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.employeeId) qs.set('employeeId', params.employeeId)
      return apiClient.get<CommissionsReport>(`/reports/commissions?${qs}`, token ?? undefined)
    },
    enabled: !!params.from && !!params.to,
  })
}

export function useServicePopularityReport(params: ServicePopularityParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'service-popularity', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from: params.from, to: params.to, top: '15' })
      if (params.branchId) qs.set('branchId', params.branchId)
      return apiClient.get<ServicePopularityReport>(`/reports/service-popularity?${qs}`, token ?? undefined)
    },
    enabled: !!params.from && !!params.to,
  })
}
