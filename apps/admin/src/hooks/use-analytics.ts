'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  CustomerAnalytics,
  PeakHoursReport,
  EmployeePerformanceReport,
} from '@splashsphere/types'

export interface AnalyticsParams {
  from: string
  to: string
  branchId?: string
}

export function useCustomerAnalytics(params: AnalyticsParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'customer-analytics', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from: params.from, to: params.to })
      if (params.branchId) qs.set('branchId', params.branchId)
      return apiClient.get<CustomerAnalytics>(`/reports/customer-analytics?${qs}`, token ?? undefined)
    },
    enabled: !!params.from && !!params.to,
  })
}

export function usePeakHours(params: AnalyticsParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'peak-hours', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from: params.from, to: params.to })
      if (params.branchId) qs.set('branchId', params.branchId)
      return apiClient.get<PeakHoursReport>(`/reports/peak-hours?${qs}`, token ?? undefined)
    },
    enabled: !!params.from && !!params.to,
  })
}

export function useEmployeePerformance(params: AnalyticsParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'employee-performance', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from: params.from, to: params.to })
      if (params.branchId) qs.set('branchId', params.branchId)
      return apiClient.get<EmployeePerformanceReport>(`/reports/employee-performance?${qs}`, token ?? undefined)
    },
    enabled: !!params.from && !!params.to,
  })
}
