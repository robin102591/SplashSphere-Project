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

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'

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

    const res = await fetch(`${API_BASE}/api/v1/attendance/export/csv?${qs}`, {
      headers: { Authorization: `Bearer ${token}` },
    })

    if (!res.ok) throw new Error('Export failed')

    const blob = await res.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `attendance_${params.from.replace(/-/g, '')}_${params.to.replace(/-/g, '')}.csv`
    document.body.appendChild(a)
    a.click()
    a.remove()
    URL.revokeObjectURL(url)
  }, [getToken])
}
