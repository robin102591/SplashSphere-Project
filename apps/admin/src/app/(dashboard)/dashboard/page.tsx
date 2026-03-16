'use client'

import { useMemo, useState } from 'react'
import { TrendingUp, Car, CreditCard, Users, Activity } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  AreaChart, Area, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts'
import { useDashboardSummary } from '@/hooks/use-dashboard'
import { useRevenueReport, useServicePopularityReport } from '@/hooks/use-reports'
import { useBranches } from '@/hooks/use-branches'
import { useQueryClient } from '@tanstack/react-query'
import { useSignalREvent } from '@/lib/signalr-context'
import type { DashboardMetricsUpdatedPayload } from '@splashsphere/types'

const php = new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' })

const PIE_COLORS = ['#2563eb', '#16a34a', '#d97706', '#dc2626', '#7c3aed', '#0891b2']

function dateStr(d: Date) {
  return d.toISOString().split('T')[0]
}

function defaultRange() {
  const to = new Date()
  const from = new Date()
  from.setDate(to.getDate() - 29)
  return { from: dateStr(from), to: dateStr(to) }
}

function KpiCard({
  title, value, sub, icon: Icon, highlight,
}: {
  title: string
  value: string
  sub?: string
  icon: React.ElementType
  highlight?: boolean
}) {
  return (
    <Card className={highlight ? 'border-primary/30 bg-primary/5' : ''}>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        <Icon className="h-4 w-4 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        <p className={`text-2xl font-bold tabular-nums ${highlight ? 'text-primary' : ''}`}>{value}</p>
        {sub && <p className="text-xs text-muted-foreground mt-0.5">{sub}</p>}
      </CardContent>
    </Card>
  )
}

export default function DashboardPage() {
  const queryClient = useQueryClient()
  const [branchId, setBranchId] = useState<string>('')
  const { from, to } = useMemo(() => defaultRange(), [])

  // Refresh KPIs when any transaction or queue change is broadcast
  useSignalREvent<DashboardMetricsUpdatedPayload>('DashboardMetricsUpdated', () => {
    queryClient.invalidateQueries({ queryKey: ['dashboard-summary'] })
  })

  const { data: branches = [] } = useBranches()
  const { data: summary, isLoading: summaryLoading } = useDashboardSummary(branchId || undefined)
  const { data: revenueReport } = useRevenueReport({ from, to, branchId: branchId || undefined })
  const { data: popularityReport } = useServicePopularityReport({ from, to, branchId: branchId || undefined })

  const chartData = revenueReport?.dailyBreakdown.map((d) => ({
    date: new Date(d.date).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' }),
    revenue: d.revenue,
    transactions: d.transactionCount,
  })) ?? []

  const pieData = revenueReport?.byPaymentMethod.map((p) => ({
    name: p.paymentMethod,
    value: p.amount,
  })) ?? []

  const topServices = popularityReport?.services.slice(0, 8).map((s) => ({
    name: s.serviceName.length > 20 ? s.serviceName.slice(0, 18) + '…' : s.serviceName,
    revenue: s.totalRevenue,
    count: s.timesPerformed,
  })) ?? []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
          <p className="text-muted-foreground">Overview of your car wash operations</p>
        </div>
        <Select value={branchId} onValueChange={setBranchId}>
          <SelectTrigger className="w-52">
            <SelectValue placeholder="All branches" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="">All branches</SelectItem>
            {branches.map((b) => (
              <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Primary KPI row */}
      {summaryLoading ? (
        <div className="grid gap-4 grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-24 w-full" />)}
        </div>
      ) : (
        <div className="grid gap-4 grid-cols-2 lg:grid-cols-4">
          <KpiCard
            title="Revenue Today"
            value={php.format(summary?.revenueToday ?? 0)}
            sub={`This week: ${php.format(summary?.revenueThisWeek ?? 0)}`}
            icon={CreditCard}
            highlight
          />
          <KpiCard
            title="Transactions Today"
            value={String(summary?.transactionsToday ?? 0)}
            sub={`This week: ${summary?.transactionsThisWeek ?? 0}`}
            icon={Activity}
          />
          <KpiCard
            title="Queue Waiting"
            value={String(summary?.queueWaiting ?? 0)}
            sub={`In service: ${summary?.queueInService ?? 0}`}
            icon={Car}
          />
          <KpiCard
            title="Active Employees"
            value={String(summary?.activeEmployees ?? 0)}
            sub={`Clocked in: ${summary?.clockedInToday ?? 0}`}
            icon={Users}
          />
        </div>
      )}

      {/* Secondary KPI row */}
      {!summaryLoading && (
        <div className="grid gap-4 grid-cols-2 lg:grid-cols-4">
          <div className="rounded-lg border px-4 py-3">
            <p className="text-xs text-muted-foreground">Revenue This Month</p>
            <p className="text-lg font-bold tabular-nums mt-0.5">{php.format(summary?.revenueThisMonth ?? 0)}</p>
          </div>
          <div className="rounded-lg border px-4 py-3">
            <p className="text-xs text-muted-foreground">Transactions This Month</p>
            <p className="text-lg font-bold tabular-nums mt-0.5">{summary?.transactionsThisMonth ?? 0}</p>
          </div>
          <div className="rounded-lg border px-4 py-3">
            <p className="text-xs text-muted-foreground">Revenue This Week</p>
            <p className="text-lg font-bold tabular-nums mt-0.5">{php.format(summary?.revenueThisWeek ?? 0)}</p>
          </div>
          <div className="rounded-lg border px-4 py-3">
            <p className="text-xs text-muted-foreground">Transactions This Week</p>
            <p className="text-lg font-bold tabular-nums mt-0.5">{summary?.transactionsThisWeek ?? 0}</p>
          </div>
        </div>
      )}

      {/* Charts row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Revenue area chart */}
        <Card className="lg:col-span-2">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium flex items-center gap-2">
              <TrendingUp className="h-4 w-4 text-muted-foreground" />
              Revenue — Last 30 Days
            </CardTitle>
          </CardHeader>
          <CardContent>
            {chartData.length === 0 ? (
              <div className="h-52 flex items-center justify-center text-sm text-muted-foreground">No data</div>
            ) : (
              <ResponsiveContainer width="100%" height={208}>
                <AreaChart data={chartData}>
                  <defs>
                    <linearGradient id="colorRev" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#2563eb" stopOpacity={0.25} />
                      <stop offset="95%" stopColor="#2563eb" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                  <XAxis dataKey="date" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} interval="preserveStartEnd" />
                  <YAxis tick={{ fontSize: 11 }} tickLine={false} axisLine={false} tickFormatter={(v) => `₱${(v / 1000).toFixed(0)}k`} />
                  <Tooltip formatter={(v: number) => [php.format(v), 'Revenue']} />
                  <Area type="monotone" dataKey="revenue" stroke="#2563eb" strokeWidth={2} fill="url(#colorRev)" />
                </AreaChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        {/* Payment method pie */}
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Payment Methods</CardTitle>
          </CardHeader>
          <CardContent>
            {pieData.length === 0 ? (
              <div className="h-52 flex items-center justify-center text-sm text-muted-foreground">No data</div>
            ) : (
              <ResponsiveContainer width="100%" height={208}>
                <PieChart>
                  <Pie
                    data={pieData}
                    dataKey="value"
                    nameKey="name"
                    cx="50%"
                    cy="45%"
                    outerRadius={70}
                    strokeWidth={1}
                  >
                    {pieData.map((_, i) => (
                      <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(v: number) => php.format(v)} />
                  <Legend iconSize={10} wrapperStyle={{ fontSize: 11 }} />
                </PieChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Top services bar */}
      {topServices.length > 0 && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Top Services by Revenue — Last 30 Days</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={topServices} layout="vertical" margin={{ left: 8 }}>
                <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="hsl(var(--border))" />
                <XAxis type="number" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} tickFormatter={(v) => `₱${(v / 1000).toFixed(0)}k`} />
                <YAxis type="category" dataKey="name" width={130} tick={{ fontSize: 11 }} tickLine={false} axisLine={false} />
                <Tooltip formatter={(v: number) => [php.format(v), 'Revenue']} />
                <Bar dataKey="revenue" fill="#2563eb" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {/* Branch KPI table — only tenant-wide */}
      {!branchId && summary?.branches && summary.branches.length > 0 && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Branch Breakdown — Today</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-sm">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-4 py-2.5 text-left font-medium">Branch</th>
                  <th className="px-4 py-2.5 text-right font-medium">Revenue</th>
                  <th className="px-4 py-2.5 text-right font-medium">Transactions</th>
                  <th className="px-4 py-2.5 text-right font-medium">Queue Waiting</th>
                  <th className="px-4 py-2.5 text-right font-medium">In Service</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {summary.branches.map((b) => (
                  <tr key={b.branchId} className="hover:bg-muted/30">
                    <td className="px-4 py-2.5 font-medium">{b.branchName}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{php.format(b.revenueToday)}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{b.transactionsToday}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{b.queueWaiting}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums">{b.queueInService}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
