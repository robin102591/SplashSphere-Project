'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { useCallback } from 'react'
import { apiClient } from '@/lib/api-client'
import type { AttendanceReport } from '@splashsphere/types'

export interface AttendanceReportParams {
  from: string
  to: string
  branchId?: string
  employeeId?: string
}

export function useAttendanceReport(params: AttendanceReportParams) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['attendance', 'report', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from: params.from, to: params.to })
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.employeeId) qs.set('employeeId', params.employeeId)
      return apiClient.get<AttendanceReport>(`/attendance/report?${qs}`, token ?? undefined)
    },
    enabled: !!params.from && !!params.to,
  })
}

export function useExportAttendanceCsv() {
  const { getToken } = useAuth()

  return useCallback(async (params: AttendanceReportParams) => {
    const token = await getToken()
    const qs = new URLSearchParams({ from: params.from, to: params.to })
    if (params.branchId) qs.set('branchId', params.branchId)
    if (params.employeeId) qs.set('employeeId', params.employeeId)

    const filename = `attendance_${params.from.replace(/-/g, '')}_${params.to.replace(/-/g, '')}.csv`
    await apiClient.download(`/attendance/export/csv?${qs}`, filename, token ?? undefined)
  }, [getToken])
}
