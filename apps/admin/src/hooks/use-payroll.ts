'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  PayrollPeriodSummary,
  PayrollPeriodDetail,
  PayrollAdjustmentTemplate,
  PayrollAdjustment,
  PayrollEntryDetail,
  PayrollSettingsDto,
  Payslip,
  PagedResult,
} from '@splashsphere/types'
import type { AdjustmentType, PayrollStatus } from '@splashsphere/types'

// ── Query keys ────────────────────────────────────────────────────────────────

export const payrollKeys = {
  all: ['payroll'] as const,
  list: (params: PayrollListParams) => ['payroll', 'list', params] as const,
  detail: (id: string) => ['payroll', id] as const,
  entryDetail: (id: string) => ['payroll', 'entry', id] as const,
  templates: ['payroll', 'templates'] as const,
  settings: ['payroll', 'settings'] as const,
  payslip: (entryId: string) => ['payroll', 'payslip', entryId] as const,
}

// ── Param / value types ───────────────────────────────────────────────────────

export interface PayrollListParams {
  status?: PayrollStatus
  year?: number
  page?: number
  pageSize?: number
}

export interface UpdateEntryValues {
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

export function useCreatePayrollPeriod() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { startDate: string; endDate: string }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/payroll/periods', data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: payrollKeys.all })
    },
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

export function useReleasePayrollPeriod() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (periodId: string) => {
      const token = await getToken()
      return apiClient.post<void>(`/payroll/periods/${periodId}/release`, {}, token ?? undefined)
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

// ── Entry Detail ────────────────────────────────────────────────────────────

export function usePayrollEntryDetail(entryId: string | null) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: payrollKeys.entryDetail(entryId ?? ''),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PayrollEntryDetail>(`/payroll/entries/${entryId}/detail`, token ?? undefined)
    },
    enabled: !!entryId,
  })
}

// ── Bulk Apply ──────────────────────────────────────────────────────────────

export interface BulkAdjustValues {
  entryIds: string[]
  adjustmentType: AdjustmentType
  amount: number
  notes?: string
  templateId?: string
}

// ── Adjustment values ──────────────────────────────────────────────────────

export interface AddAdjustmentValues {
  type: AdjustmentType
  category: string
  amount: number
  notes?: string
  templateId?: string
}

export interface UpdateAdjustmentValues {
  amount: number
  notes?: string
}

export function useBulkApplyAdjustment(periodId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: BulkAdjustValues) => {
      const token = await getToken()
      return apiClient.post<void>('/payroll/entries/bulk-adjust', data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: payrollKeys.detail(periodId) })
    },
  })
}

// ── Entry Adjustments (CRUD) ────────────────────────────────────────────────

export function useAddAdjustment(periodId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ entryId, values }: { entryId: string; values: AddAdjustmentValues }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>(`/payroll/entries/${entryId}/adjustments`, values, token ?? undefined)
    },
    onSuccess: (_data, { entryId }) => {
      qc.invalidateQueries({ queryKey: payrollKeys.detail(periodId) })
      qc.invalidateQueries({ queryKey: payrollKeys.entryDetail(entryId) })
    },
  })
}

export function useUpdateAdjustment(periodId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ adjustmentId, values }: { adjustmentId: string; values: UpdateAdjustmentValues }) => {
      const token = await getToken()
      return apiClient.put<void>(`/payroll/adjustments/${adjustmentId}`, values, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: payrollKeys.detail(periodId) })
    },
  })
}

export function useDeleteAdjustment(periodId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (adjustmentId: string) => {
      const token = await getToken()
      return apiClient.delete<void>(`/payroll/adjustments/${adjustmentId}`, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: payrollKeys.detail(periodId) })
    },
  })
}

// ── Adjustment Templates ────────────────────────────────────────────────────

export interface CreateTemplateValues {
  name: string
  type: AdjustmentType
  defaultAmount: number
}

export interface UpdateTemplateValues {
  name: string
  type: AdjustmentType
  defaultAmount: number
  sortOrder: number
}

export function usePayrollTemplates() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: payrollKeys.templates,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PayrollAdjustmentTemplate[]>('/payroll/templates', token ?? undefined)
    },
  })
}

export function useCreatePayrollTemplate() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateTemplateValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/payroll/templates', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: payrollKeys.templates }),
  })
}

export function useUpdatePayrollTemplate() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, values }: { id: string; values: UpdateTemplateValues }) => {
      const token = await getToken()
      return apiClient.put<void>(`/payroll/templates/${id}`, values, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: payrollKeys.templates }),
  })
}

export function useDeletePayrollTemplate() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.delete<void>(`/payroll/templates/${id}`, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: payrollKeys.templates }),
  })
}

// ── Payroll Settings ───────────────────────────────────────────────────────

export function usePayrollSettings() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: payrollKeys.settings,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PayrollSettingsDto>('/settings/payroll-config', token ?? undefined)
    },
    staleTime: 5 * 60 * 1000,
  })
}

export function useUpdatePayrollSettings() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { cutOffStartDay: number; frequency: number; payReleaseDayOffset: number; autoCalcGovernmentDeductions: boolean }) => {
      const token = await getToken()
      return apiClient.put<void>('/settings/payroll-config', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: payrollKeys.settings }),
  })
}

// ── Payslip ─────────────────────────────────────────────────────────────────

export function usePayslip(entryId: string | null) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: payrollKeys.payslip(entryId ?? ''),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Payslip>(`/payroll/entries/${entryId}/payslip`, token ?? undefined)
    },
    enabled: !!entryId,
  })
}
