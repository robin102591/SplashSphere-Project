'use client'

import { useState, useMemo } from 'react'
import { DatePicker } from '@/components/ui/date-picker'
import { Skeleton } from '@/components/ui/skeleton'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import { useEmployeePerformance } from '@/hooks/use-analytics'
import { useBranches } from '@/hooks/use-branches'
import { formatPeso } from '@/lib/format'
import { cn } from '@/lib/utils'
import { Trophy, Medal, Award } from 'lucide-react'

const TOOLTIP_STYLE: React.CSSProperties = {
  backgroundColor: 'var(--color-popover)',
  color: 'var(--color-popover-foreground)',
  border: '1px solid var(--color-border)',
  borderRadius: '0.5rem',
}

function dateStr(d: Date) { return d.toISOString().split('T')[0] }

const RANK_ICONS = [
  <Trophy key="1" className="h-5 w-5 text-yellow-500" />,
  <Medal key="2" className="h-5 w-5 text-gray-400" />,
  <Award key="3" className="h-5 w-5 text-amber-600" />,
]

export default function EmployeePerformancePage() {
  const defaults = useMemo(() => {
    const to = new Date(); const from = new Date(); from.setDate(to.getDate() - 29)
    return { from: dateStr(from), to: dateStr(to) }
  }, [])

  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [branchId, setBranchId] = useState('')
  const [sortBy, setSortBy] = useState<'revenue' | 'services' | 'commissions' | 'attendance'>('revenue')

  const { data: branches } = useBranches()
  const { data, isLoading } = useEmployeePerformance({ from, to, branchId: branchId || undefined })

  const sorted = useMemo(() => {
    if (!data) return []
    return [...data.rankings].sort((a, b) => {
      switch (sortBy) {
        case 'services': return b.servicesPerformed - a.servicesPerformed
        case 'commissions': return b.commissionsEarned - a.commissionsEarned
        case 'attendance': return b.attendanceRate - a.attendanceRate
        default: return b.revenueGenerated - a.revenueGenerated
      }
    })
  }, [data, sortBy])

  const top10Chart = sorted.slice(0, 10).map((e) => ({
    name: e.employeeName.split(' ')[0],
    revenue: e.revenueGenerated,
    commissions: e.commissionsEarned,
  }))

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Employee Performance</h1>
        <p className="text-sm text-muted-foreground">Rankings by revenue, services, commissions, and attendance</p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <DatePicker value={from} onChange={setFrom} placeholder="From" className="w-[150px]" />
        <span className="text-muted-foreground text-sm">to</span>
        <DatePicker value={to} onChange={setTo} placeholder="To" className="w-[150px]" />
        <Select value={branchId || 'all'} onValueChange={(v) => setBranchId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Branches" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Branches</SelectItem>
            {branches?.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
          </SelectContent>
        </Select>
        <Select value={sortBy} onValueChange={(v) => setSortBy(v as typeof sortBy)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="revenue">Sort by Revenue</SelectItem>
            <SelectItem value="services">Sort by Services</SelectItem>
            <SelectItem value="commissions">Sort by Commissions</SelectItem>
            <SelectItem value="attendance">Sort by Attendance</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Summary KPIs */}
      <div className="grid gap-4 grid-cols-3">
        {isLoading ? (
          Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-xl" />)
        ) : (
          <>
            <div className="rounded-xl border bg-card p-4">
              <p className="text-xs text-muted-foreground">Active Employees</p>
              <p className="text-2xl font-bold mt-1">{data?.totalEmployees ?? 0}</p>
            </div>
            <div className="rounded-xl border bg-card p-4">
              <p className="text-xs text-muted-foreground">Total Commissions</p>
              <p className="text-2xl font-bold mt-1 text-green-600">{formatPeso(data?.totalCommissions ?? 0)}</p>
            </div>
            <div className="rounded-xl border bg-card p-4">
              <p className="text-xs text-muted-foreground">Services Performed</p>
              <p className="text-2xl font-bold mt-1">{(data?.totalServicesPerformed ?? 0).toLocaleString()}</p>
            </div>
          </>
        )}
      </div>

      {/* Top 10 Chart */}
      <div className="rounded-xl border bg-card p-4">
        <h3 className="text-sm font-medium mb-4">Top 10 — Revenue vs Commissions</h3>
        {isLoading ? <Skeleton className="h-64" /> : (
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={top10Chart} layout="vertical" margin={{ left: 10 }}>
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis type="number" fontSize={11} tickLine={false} tickFormatter={(v) => `₱${(v / 1000).toFixed(0)}k`} />
              <YAxis type="category" dataKey="name" fontSize={11} tickLine={false} width={70} />
              <Tooltip contentStyle={TOOLTIP_STYLE} formatter={(v: number) => formatPeso(v)} />
              <Bar dataKey="revenue" name="Revenue" fill="#2563eb" radius={[0, 4, 4, 0]} />
              <Bar dataKey="commissions" name="Commissions" fill="#16a34a" radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>

      {/* Rankings Table */}
      <div className="rounded-xl border bg-card">
        <div className="p-4 border-b">
          <h3 className="text-sm font-medium">Employee Rankings</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="text-left px-4 py-2 font-medium w-10">#</th>
                <th className="text-left px-4 py-2 font-medium">Employee</th>
                <th className="text-left px-4 py-2 font-medium">Branch</th>
                <th className="text-left px-4 py-2 font-medium">Type</th>
                <th className="text-right px-4 py-2 font-medium">Services</th>
                <th className="text-right px-4 py-2 font-medium">Revenue</th>
                <th className="text-right px-4 py-2 font-medium">Commissions</th>
                <th className="text-right px-4 py-2 font-medium">Days</th>
                <th className="text-right px-4 py-2 font-medium">Attendance</th>
                <th className="text-right px-4 py-2 font-medium">Avg/Service</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <tr key={i} className="border-b"><td colSpan={10} className="px-4 py-3"><Skeleton className="h-4" /></td></tr>
                ))
              ) : sorted.length === 0 ? (
                <tr><td colSpan={10} className="text-center text-muted-foreground py-8">No employee data for this period</td></tr>
              ) : (
                sorted.map((e, i) => (
                  <tr key={e.employeeId} className="border-b hover:bg-muted/30 transition-colors">
                    <td className="px-4 py-2.5">
                      {i < 3 ? RANK_ICONS[i] : <span className="text-muted-foreground">{i + 1}</span>}
                    </td>
                    <td className="px-4 py-2.5 font-medium">{e.employeeName}</td>
                    <td className="px-4 py-2.5 text-muted-foreground">{e.branchName}</td>
                    <td className="px-4 py-2.5">
                      <span className={cn(
                        'inline-block px-2 py-0.5 rounded-full text-xs font-medium',
                        e.employeeType === 'Commission' && 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
                        e.employeeType === 'Daily' && 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-400',
                        e.employeeType === 'Hybrid' && 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400',
                      )}>
                        {e.employeeType}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-right">{e.servicesPerformed}</td>
                    <td className="px-4 py-2.5 text-right font-medium">{formatPeso(e.revenueGenerated)}</td>
                    <td className="px-4 py-2.5 text-right text-green-600">{formatPeso(e.commissionsEarned)}</td>
                    <td className="px-4 py-2.5 text-right">{e.daysWorked}</td>
                    <td className="px-4 py-2.5 text-right">
                      <span className={cn(
                        e.attendanceRate >= 90 && 'text-green-600',
                        e.attendanceRate >= 70 && e.attendanceRate < 90 && 'text-yellow-600',
                        e.attendanceRate < 70 && 'text-red-600',
                      )}>
                        {e.attendanceRate}%
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-right text-muted-foreground">{formatPeso(e.averageRevenuePerService)}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
