'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { CashAdvance, PagedResult } from '@splashsphere/types'
import type { CashAdvanceStatus } from '@splashsphere/types'

// ── Query keys ────────────────────────────────────────────────────────────────

export const cashAdvanceKeys = {
  all: ['cash-advances'] as const,
  list: (params: CashAdvanceListParams) => ['cash-advances', 'list', params] as const,
  detail: (id: string) => ['cash-advances', id] as const,
  byEmployee: (employeeId: string) => ['cash-advances', 'employee', employeeId] as const,
}

// ── Param / value types ─────────────────────────────────────────────────────

export interface CashAdvanceListParams {
  employeeId?: string
  status?: CashAdvanceStatus
  page?: number
  pageSize?: number
}

export interface CreateCashAdvanceValues {
  employeeId: string
  amount: number
  deductionPerPeriod: number
  reason?: string
}

// ── Hooks ─────────────────────────────────────────────────────────────────────

export function useCashAdvances(params: CashAdvanceListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: cashAdvanceKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.employeeId) qs.set('employeeId', params.employeeId)
      if (params.status != null) qs.set('status', String(params.status))
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<CashAdvance>>(
        `/cash-advances?${qs.toString()}`,
        token ?? undefined
      )
    },
  })
}

export function useCashAdvance(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: cashAdvanceKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CashAdvance>(`/cash-advances/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useEmployeeCashAdvances(employeeId: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: cashAdvanceKeys.byEmployee(employeeId),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CashAdvance[]>(`/employees/${employeeId}/cash-advances`, token ?? undefined)
    },
    enabled: !!employeeId,
  })
}

export function useCreateCashAdvance() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateCashAdvanceValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/cash-advances', data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: cashAdvanceKeys.all })
    },
  })
}

export function useApproveCashAdvance() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/cash-advances/${id}/approve`, {}, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: cashAdvanceKeys.all })
    },
  })
}

export function useDisburseCashAdvance() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/cash-advances/${id}/disburse`, {}, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: cashAdvanceKeys.all })
    },
  })
}

export function useCancelCashAdvance() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/cash-advances/${id}/cancel`, {}, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: cashAdvanceKeys.all })
    },
  })
}
