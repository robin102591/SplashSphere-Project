'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  PayrollPeriodSummary,
  PayrollPeriodDetail,
  PagedResult,
} from '@splashsphere/types'
import type { PayrollStatus } from '@splashsphere/types'

// ── Query keys ────────────────────────────────────────────────────────────────

export const payrollKeys = {
  all: ['payroll'] as const,
  list: (params: PayrollListParams) => ['payroll', 'list', params] as const,
  detail: (id: string) => ['payroll', id] as const,
}

// ── Param / value types ───────────────────────────────────────────────────────

export interface PayrollListParams {
  status?: PayrollStatus
  year?: number
  page?: number
  pageSize?: number
}

export interface UpdateEntryValues {
  bonuses: number
  deductions: number
  notes?: string
}

// ── Hooks ─────────────────────────────────────────────────────────────────────

export function usePayrollPeriods(params: PayrollListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: payrollKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.status != null) qs.set('status', String(params.status))
      if (params.year != null) qs.set('year', String(params.year))
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<PayrollPeriodSummary>>(
        `/payroll/periods?${qs.toString()}`,
        token ?? undefined
      )
    },
  })
}

export function usePayrollPeriod(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: payrollKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PayrollPeriodDetail>(`/payroll/periods/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useClosePayrollPeriod() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (periodId: string) => {
      const token = await getToken()
      return apiClient.post<void>(`/payroll/periods/${periodId}/close`, {}, token ?? undefined)
    },
    onSuccess: (_data, periodId) => {
      qc.invalidateQueries({ queryKey: payrollKeys.detail(periodId) })
      qc.invalidateQueries({ queryKey: payrollKeys.all })
    },
  })
}

export function useProcessPayrollPeriod() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (periodId: string) => {
      const token = await getToken()
      return apiClient.post<void>(`/payroll/periods/${periodId}/process`, {}, token ?? undefined)
    },
    onSuccess: (_data, periodId) => {
      qc.invalidateQueries({ queryKey: payrollKeys.detail(periodId) })
      qc.invalidateQueries({ queryKey: payrollKeys.all })
    },
  })
}

export function useUpdatePayrollEntry(periodId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ entryId, values }: { entryId: string; values: UpdateEntryValues }) => {
      const token = await getToken()
      return apiClient.patch<void>(`/payroll/entries/${entryId}`, values, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: payrollKeys.detail(periodId) })
    },
  })
}
