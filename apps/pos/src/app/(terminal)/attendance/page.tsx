'use client'

import { useState, useEffect } from 'react'
import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Fingerprint, CheckCircle2, Clock, Users, LogIn, LogOut } from 'lucide-react'
import type { Employee, AttendanceDto } from '@splashsphere/types'
import type { PagedResult } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { cn } from '@/lib/utils'

const BRANCH_KEY = 'pos-branch-id'

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-PH', {
    hour: 'numeric', minute: '2-digit', hour12: true,
  })
}

export default function AttendancePage() {
  const { getToken } = useAuth()
  const [branchId, setBranchId] = useState('')
  const queryClient = useQueryClient()

  useEffect(() => {
    setBranchId(localStorage.getItem(BRANCH_KEY) ?? '')
  }, [])

  const today = new Date().toISOString().slice(0, 10)

  // Fetch active employees for the branch
  const { data: employeesData, isLoading: empLoading } = useQuery({
    queryKey: ['employees', branchId],
    enabled: !!branchId,
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PagedResult<Employee>>(
        `/employees?branchId=${branchId}&isActive=true&page=1&pageSize=100`,
        token ?? undefined,
      )
    },
  })

  // Fetch today's attendance for the branch
  const { data: attendanceData, isLoading: attLoading } = useQuery({
    queryKey: ['attendance', branchId, today],
    enabled: !!branchId,
    staleTime: 30_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<AttendanceDto[]>(
        `/attendance?branchId=${branchId}&date=${today}`,
        token ?? undefined,
      )
    },
  })

  const clockInMutation = useMutation({
    mutationFn: async (employeeId: string) => {
      const token = await getToken()
      return apiClient.post<AttendanceDto>('/attendance', { employeeId, branchId }, token ?? undefined)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendance', branchId, today] })
    },
  })

  const clockOutMutation = useMutation({
    mutationFn: async (attendanceId: string) => {
      const token = await getToken()
      return apiClient.patch<AttendanceDto>(`/attendance/${attendanceId}/clock-out`, {}, token ?? undefined)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendance', branchId, today] })
    },
  })

  const employees = employeesData?.items ?? []
  const attendance = attendanceData ?? []

  // Map attendance by employeeId for quick lookup
  const attByEmployee = new Map<string, AttendanceDto>()
  for (const att of attendance) {
    attByEmployee.set(att.employeeId, att)
  }

  const clockedInCount = attendance.filter((a) => !a.timeOut).length
  const completedCount = attendance.filter((a) => a.timeOut).length
  const isLoading = empLoading || attLoading

  return (
    <div className="p-4 space-y-4 max-w-lg mx-auto">

      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-white">Attendance</h1>
        <p className="text-sm text-gray-400">
          {new Date().toLocaleDateString('en-PH', { weekday: 'long', month: 'long', day: 'numeric' })}
        </p>
      </div>

      {/* Summary bar */}
      {!isLoading && employees.length > 0 && (
        <div className="grid grid-cols-3 gap-2">
          <SummaryBadge
            icon={<Users className="h-4 w-4 text-gray-400" />}
            label="Total"
            value={employees.length}
            cls="text-gray-300"
          />
          <SummaryBadge
            icon={<LogIn className="h-4 w-4 text-green-400" />}
            label="Clocked In"
            value={clockedInCount}
            cls="text-green-400"
          />
          <SummaryBadge
            icon={<LogOut className="h-4 w-4 text-blue-400" />}
            label="Completed"
            value={completedCount}
            cls="text-blue-400"
          />
        </div>
      )}

      {/* No branch */}
      {!branchId && (
        <div className="rounded-xl border border-dashed border-gray-700 py-12 text-center">
          <p className="text-gray-500">Select a branch from the Queue page first.</p>
        </div>
      )}

      {/* Loading */}
      {isLoading && branchId && (
        <div className="space-y-2">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="h-[72px] rounded-xl bg-gray-800 animate-pulse" />
          ))}
        </div>
      )}

      {/* Employee list */}
      {!isLoading && employees.length > 0 && (
        <div className="space-y-2">
          {employees.map((emp) => {
            const att = attByEmployee.get(emp.id)
            const isClockedIn = att && !att.timeOut
            const isCompleted = att && att.timeOut
            const isPending = clockInMutation.isPending && clockInMutation.variables === emp.id
            const isClockingOut = clockOutMutation.isPending && att && clockOutMutation.variables === att.id

            return (
              <div
                key={emp.id}
                className={cn(
                  'rounded-xl border p-4 transition-colors',
                  isClockedIn
                    ? 'bg-green-900/15 border-green-700/40'
                    : isCompleted
                    ? 'bg-gray-800/60 border-gray-700'
                    : 'bg-gray-800 border-gray-700',
                )}
              >
                <div className="flex items-center justify-between gap-3">
                  <div className="flex items-center gap-3 min-w-0">
                    {/* Status icon */}
                    <div className={cn(
                      'h-10 w-10 rounded-full flex items-center justify-center shrink-0',
                      isClockedIn
                        ? 'bg-green-600/20 border border-green-600/30'
                        : isCompleted
                        ? 'bg-blue-600/20 border border-blue-600/30'
                        : 'bg-gray-700 border border-gray-600',
                    )}>
                      {isClockedIn ? (
                        <Fingerprint className="h-5 w-5 text-green-400" />
                      ) : isCompleted ? (
                        <CheckCircle2 className="h-5 w-5 text-blue-400" />
                      ) : (
                        <Clock className="h-5 w-5 text-gray-500" />
                      )}
                    </div>

                    {/* Name + times */}
                    <div className="min-w-0">
                      <p className="text-sm font-semibold text-white truncate">{emp.fullName}</p>
                      {att ? (
                        <p className="text-xs text-gray-500">
                          In: {fmtTime(att.timeIn)}
                          {att.timeOut && ` · Out: ${fmtTime(att.timeOut)}`}
                        </p>
                      ) : (
                        <p className="text-xs text-gray-600">Not clocked in</p>
                      )}
                    </div>
                  </div>

                  {/* Action button */}
                  {!isCompleted && (
                    isClockedIn ? (
                      <button
                        onClick={() => att && clockOutMutation.mutate(att.id)}
                        disabled={!!isClockingOut}
                        className="flex items-center gap-1.5 min-h-[44px] px-3 rounded-lg bg-blue-600/20 hover:bg-blue-600/30 border border-blue-600/30 text-blue-400 text-sm font-medium transition-colors disabled:opacity-50 shrink-0"
                      >
                        <LogOut className="h-4 w-4" />
                        {isClockingOut ? 'Clocking…' : 'Clock Out'}
                      </button>
                    ) : (
                      <button
                        onClick={() => clockInMutation.mutate(emp.id)}
                        disabled={isPending}
                        className="flex items-center gap-1.5 min-h-[44px] px-3 rounded-lg bg-green-600/20 hover:bg-green-600/30 border border-green-600/30 text-green-400 text-sm font-medium transition-colors disabled:opacity-50 shrink-0"
                      >
                        <LogIn className="h-4 w-4" />
                        {isPending ? 'Clocking…' : 'Clock In'}
                      </button>
                    )
                  )}

                  {isCompleted && (
                    <span className="text-xs text-gray-500 shrink-0">Done</span>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      )}

      {!isLoading && branchId && employees.length === 0 && (
        <div className="rounded-xl border border-dashed border-gray-700 py-12 text-center">
          <p className="text-gray-500">No active employees found for this branch.</p>
        </div>
      )}
    </div>
  )
}

// ── Sub-components ─────────────────────────────────────────────────────────────

function SummaryBadge({
  icon, label, value, cls,
}: {
  icon: React.ReactNode
  label: string
  value: number
  cls: string
}) {
  return (
    <div className="rounded-xl bg-gray-800 border border-gray-700 p-3 text-center">
      <div className="flex justify-center mb-1">{icon}</div>
      <p className={cn('text-xl font-bold', cls)}>{value}</p>
      <p className="text-xs text-gray-500">{label}</p>
    </div>
  )
}
