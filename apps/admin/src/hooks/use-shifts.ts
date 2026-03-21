'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  ShiftSummaryDto, ShiftDetailDto, ShiftReportDto,
  ShiftSettingsDto, ShiftVarianceCashierDto, VarianceTrendPointDto,
} from '@splashsphere/types'
import type { ShiftStatus, ReviewStatus } from '@splashsphere/types'

// ── Query keys ────────────────────────────────────────────────────────────────

export const shiftKeys = {
  all:      ['shifts'] as const,
  list:     (params: object) => ['shifts', 'list', params] as const,
  detail:   (id: string) => ['shifts', id] as const,
  report:   (id: string) => ['shifts', id, 'report'] as const,
  variance: (params: object) => ['shifts', 'variance', params] as const,
  settings: ['shift-settings'] as const,
}

// ── Params interfaces ─────────────────────────────────────────────────────────

export interface GetShiftsParams {
  branchId?: string
  cashierId?: string
  dateFrom?: string
  dateTo?: string
  status?: ShiftStatus
  reviewStatus?: ReviewStatus
  page?: number
  pageSize?: number
}

export interface GetVarianceReportParams {
  branchId?: string
  cashierId?: string
  dateFrom?: string
  dateTo?: string
}

// ── Queries ───────────────────────────────────────────────────────────────────

export function useShifts(params: GetShiftsParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: shiftKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.branchId)     qs.set('branchId', params.branchId)
      if (params.cashierId)    qs.set('cashierId', params.cashierId)
      if (params.dateFrom)     qs.set('dateFrom', params.dateFrom)
      if (params.dateTo)       qs.set('dateTo', params.dateTo)
      if (params.status != null)       qs.set('status', String(params.status))
      if (params.reviewStatus != null) qs.set('reviewStatus', String(params.reviewStatus))
      qs.set('page', String(params.page ?? 1))
      qs.set('pageSize', String(params.pageSize ?? 20))
      return apiClient.get<{ items: ShiftSummaryDto[]; totalCount: number; page: number; pageSize: number; totalPages: number }>(
        `/shifts?${qs}`, token ?? undefined,
      )
    },
  })
}

export function useShiftById(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: shiftKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ShiftDetailDto>(`/shifts/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useShiftReport(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: shiftKeys.report(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ShiftReportDto>(`/shifts/${id}/report`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useShiftVarianceReport(params: GetVarianceReportParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: shiftKeys.variance(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.branchId)  qs.set('branchId', params.branchId)
      if (params.cashierId) qs.set('cashierId', params.cashierId)
      if (params.dateFrom)  qs.set('dateFrom', params.dateFrom)
      if (params.dateTo)    qs.set('dateTo', params.dateTo)
      return apiClient.get<{
        cashierSummaries: ShiftVarianceCashierDto[]
        trendPoints: VarianceTrendPointDto[] | null
      }>(`/reports/shift-variance?${qs}`, token ?? undefined)
    },
  })
}

export function useShiftSettings() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: shiftKeys.settings,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ShiftSettingsDto>('/settings/shift-config', token ?? undefined)
    },
    staleTime: 5 * 60_000,
  })
}

// ── Mutations ─────────────────────────────────────────────────────────────────

export function useReviewShift() {
  const { getToken } = useAuth()
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      shiftId,
      newReviewStatus,
      notes,
    }: {
      shiftId: string
      newReviewStatus: ReviewStatus
      notes?: string
    }) => {
      const token = await getToken()
      return apiClient.patch<void>(
        `/shifts/${shiftId}/review`,
        { newReviewStatus, notes },
        token ?? undefined,
      )
    },
    onSuccess: (_data, { shiftId }) => {
      queryClient.invalidateQueries({ queryKey: shiftKeys.detail(shiftId) })
      queryClient.invalidateQueries({ queryKey: shiftKeys.all })
    },
  })
}

export function useReopenShift() {
  const { getToken } = useAuth()
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (shiftId: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/shifts/${shiftId}/reopen`, {}, token ?? undefined)
    },
    onSuccess: (_data, shiftId) => {
      queryClient.invalidateQueries({ queryKey: shiftKeys.detail(shiftId) })
      queryClient.invalidateQueries({ queryKey: shiftKeys.all })
    },
  })
}

export function useUpdateShiftSettings() {
  const { getToken } = useAuth()
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (body: ShiftSettingsDto) => {
      const token = await getToken()
      return apiClient.put<void>('/settings/shift-config', body, token ?? undefined)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: shiftKeys.settings })
    },
  })
}
