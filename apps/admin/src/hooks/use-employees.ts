'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  Employee,
  EmployeeCommissionDto,
  AttendanceDto,
  PagedResult,
  EmployeeType,
} from '@splashsphere/types'

// ── Query keys ────────────────────────────────────────────────────────────────

export const employeeKeys = {
  all: ['employees'] as const,
  list: (params: EmployeeListParams) => ['employees', 'list', params] as const,
  detail: (id: string) => ['employees', id] as const,
  commissions: (id: string, params: CommissionHistoryParams) =>
    ['employees', id, 'commissions', params] as const,
  attendance: (params: AttendanceParams) => ['employees', 'attendance', params] as const,
}

// ── Param types ───────────────────────────────────────────────────────────────

export interface EmployeeListParams {
  branchId?: string
  employeeType?: EmployeeType
  search?: string
  page?: number
  pageSize?: number
}

export interface CommissionHistoryParams {
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export interface AttendanceParams {
  branchId?: string
  employeeId?: string
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

// ── Form values ───────────────────────────────────────────────────────────────

export interface CreateEmployeeValues {
  branchId: string
  firstName: string
  lastName: string
  employeeType: EmployeeType
  dailyRate?: number
  email?: string
  contactNumber?: string
  hiredDate?: string
}

export interface UpdateEmployeeValues {
  firstName: string
  lastName: string
  dailyRate?: number
  email?: string
  contactNumber?: string
  hiredDate?: string
}

// ── Hooks ─────────────────────────────────────────────────────────────────────

export function useEmployees(params: EmployeeListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: employeeKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.employeeType != null) qs.set('employeeType', String(params.employeeType))
      if (params.search) qs.set('search', params.search)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<Employee>>(
        `/employees?${qs.toString()}`,
        token ?? undefined
      )
    },
  })
}

export function useEmployee(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: employeeKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Employee>(`/employees/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useCreateEmployee() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateEmployeeValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/employees', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: employeeKeys.all }),
  })
}

export function useUpdateEmployee(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: UpdateEmployeeValues) => {
      const token = await getToken()
      return apiClient.put<void>(`/employees/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: employeeKeys.detail(id) })
      qc.invalidateQueries({ queryKey: employeeKeys.all })
    },
  })
}

export function useToggleEmployeeStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/employees/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: (_data, id) => {
      qc.invalidateQueries({ queryKey: employeeKeys.detail(id) })
      qc.invalidateQueries({ queryKey: employeeKeys.all })
    },
  })
}

export function useEmployeeCommissions(id: string, params: CommissionHistoryParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: employeeKeys.commissions(id, params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.from) qs.set('from', params.from)
      if (params.to) qs.set('to', params.to)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<EmployeeCommissionDto>>(
        `/employees/${id}/commissions?${qs.toString()}`,
        token ?? undefined
      )
    },
    enabled: !!id,
  })
}

export function useAttendance(params: AttendanceParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: employeeKeys.attendance(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.employeeId) qs.set('employeeId', params.employeeId)
      if (params.from) qs.set('from', params.from)
      if (params.to) qs.set('to', params.to)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<AttendanceDto>>(
        `/employees/attendance?${qs.toString()}`,
        token ?? undefined
      )
    },
  })
}

export function useInviteEmployee() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (employeeId: string) => {
      const token = await getToken()
      return apiClient.post<void>(`/employees/${employeeId}/invite`, {}, token ?? undefined)
    },
    onSuccess: (_data, employeeId) => {
      qc.invalidateQueries({ queryKey: employeeKeys.detail(employeeId) })
      qc.invalidateQueries({ queryKey: employeeKeys.all })
    },
  })
}

// Keep legacy export for branches detail page
export function useEmployeesByBranch(branchId: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['employees', { branchId }],
    queryFn: async () => {
      const token = await getToken()
      const result = await apiClient.get<PagedResult<Employee>>(
        `/employees?branchId=${encodeURIComponent(branchId)}&pageSize=100`,
        token ?? undefined
      )
      return result.items as Employee[]
    },
    enabled: !!branchId,
  })
}
