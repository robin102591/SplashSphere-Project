'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { DatePicker } from '@/components/ui/date-picker'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { Download, CalendarCheck, Clock, AlertTriangle, Users } from 'lucide-react'
import { useAttendanceReport, useExportAttendanceCsv } from '@/hooks/use-attendance-report'
import { useBranches } from '@/hooks/use-branches'
import { useEmployees } from '@/hooks/use-employees'

function dateStr(d: Date) { return d.toISOString().split('T')[0] }

function defaultDates() {
  const to = new Date()
  const from = new Date()
  from.setDate(to.getDate() - 6)
  return { from: dateStr(from), to: dateStr(to) }
}

function StatCard({ label, value, icon: Icon }: { label: string; value: string; icon: React.ElementType }) {
  return (
    <div className="rounded-lg border px-4 py-3">
      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <Icon className="h-3.5 w-3.5" />
        {label}
      </div>
      <p className="text-xl font-bold tabular-nums mt-0.5">{value}</p>
    </div>
  )
}

export default function AttendancePage() {
  const defaults = defaultDates()
  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [branchId, setBranchId] = useState<string>('')
  const [employeeId, setEmployeeId] = useState<string>('')
  const [exporting, setExporting] = useState(false)

  const { data: branches } = useBranches()
  const { data: employeesData } = useEmployees({ branchId: branchId || undefined })
  const employees = employeesData?.items ?? []

  const { data, isLoading } = useAttendanceReport({
    from,
    to,
    branchId: branchId || undefined,
    employeeId: employeeId || undefined,
  })

  const exportCsv = useExportAttendanceCsv()

  const handleExport = async () => {
    setExporting(true)
    try {
      await exportCsv({ from, to, branchId: branchId || undefined, employeeId: employeeId || undefined })
    } finally {
      setExporting(false)
    }
  }

  const summary = data?.summary

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Attendance Report</h1>
          <p className="text-sm text-muted-foreground">Track employee attendance, tardiness, and work hours</p>
        </div>
        <Button variant="outline" size="sm" onClick={handleExport} disabled={exporting || !data}>
          <Download className="mr-2 h-4 w-4" />
          {exporting ? 'Exporting...' : 'Export CSV'}
        </Button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <DatePicker value={from} onChange={setFrom} placeholder="From" className="w-[160px]" />
        <span className="text-muted-foreground text-sm">to</span>
        <DatePicker value={to} onChange={setTo} placeholder="To" className="w-[160px]" />

        <Select value={branchId} onValueChange={(v) => { setBranchId(v === 'all' ? '' : v); setEmployeeId('') }}>
          <SelectTrigger className="w-[180px] h-9">
            <SelectValue placeholder="All Branches" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Branches</SelectItem>
            {branches?.map((b) => (
              <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={employeeId} onValueChange={(v) => setEmployeeId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[200px] h-9">
            <SelectValue placeholder="All Employees" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Employees</SelectItem>
            {employees.map((e) => (
              <SelectItem key={e.id} value={e.id}>{e.firstName} {e.lastName}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Summary Cards */}
      {isLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-[72px]" />)}
        </div>
      ) : summary ? (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <StatCard label="Total Employees" value={String(summary.totalEmployees)} icon={Users} />
          <StatCard label="Attendance Rate" value={`${summary.averageAttendanceRate}%`} icon={CalendarCheck} />
          <StatCard label="Late Arrivals" value={String(summary.totalLateArrivals)} icon={AlertTriangle} />
          <StatCard label="Avg Hours/Day" value={`${summary.averageHoursPerDay}h`} icon={Clock} />
        </div>
      ) : null}

      {/* Table */}
      {isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : data && data.employees.length > 0 ? (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Employee</th>
                <th className="px-4 py-2.5 text-left font-medium">Branch</th>
                <th className="px-4 py-2.5 text-left font-medium">Type</th>
                <th className="px-4 py-2.5 text-right font-medium">Days Present</th>
                <th className="px-4 py-2.5 text-right font-medium">Days Absent</th>
                <th className="px-4 py-2.5 text-right font-medium">Late</th>
                <th className="px-4 py-2.5 text-right font-medium">Early Out</th>
                <th className="px-4 py-2.5 text-right font-medium">Total Hours</th>
                <th className="px-4 py-2.5 text-right font-medium">Avg Hrs/Day</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {data.employees.map((row) => (
                <tr key={row.employeeId} className="hover:bg-muted/30">
                  <td className="px-4 py-2 font-medium">{row.employeeName}</td>
                  <td className="px-4 py-2 text-muted-foreground">{row.branchName}</td>
                  <td className="px-4 py-2">
                    <span className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-muted">
                      {row.employeeType}
                    </span>
                  </td>
                  <td className="px-4 py-2 text-right tabular-nums">{row.daysPresent}</td>
                  <td className="px-4 py-2 text-right tabular-nums">
                    {row.daysAbsent > 0 ? (
                      <span className="text-red-600 dark:text-red-400">{row.daysAbsent}</span>
                    ) : (
                      <span className="text-muted-foreground">0</span>
                    )}
                  </td>
                  <td className="px-4 py-2 text-right tabular-nums">
                    {row.lateCount > 0 ? (
                      <span className="text-amber-600 dark:text-amber-400">{row.lateCount}</span>
                    ) : (
                      <span className="text-muted-foreground">0</span>
                    )}
                  </td>
                  <td className="px-4 py-2 text-right tabular-nums">
                    {row.earlyOutCount > 0 ? (
                      <span className="text-amber-600 dark:text-amber-400">{row.earlyOutCount}</span>
                    ) : (
                      <span className="text-muted-foreground">0</span>
                    )}
                  </td>
                  <td className="px-4 py-2 text-right tabular-nums">{row.totalHours}h</td>
                  <td className="px-4 py-2 text-right tabular-nums">{row.averageHoursPerDay}h</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : data ? (
        <div className="flex h-48 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
          No attendance records found for this period.
        </div>
      ) : null}
    </div>
  )
}
