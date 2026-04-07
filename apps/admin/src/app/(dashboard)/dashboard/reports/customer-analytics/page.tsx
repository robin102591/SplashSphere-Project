'use client'

import { useState, useMemo } from 'react'
import { DatePicker } from '@/components/ui/date-picker'
import { Skeleton } from '@/components/ui/skeleton'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, Legend, PieChart, Pie, Cell,
} from 'recharts'
import { useCustomerAnalytics } from '@/hooks/use-analytics'
import { useBranches } from '@/hooks/use-branches'
import { formatPeso } from '@/lib/format'

const TOOLTIP_STYLE: React.CSSProperties = {
  backgroundColor: 'var(--color-popover)',
  color: 'var(--color-popover-foreground)',
  border: '1px solid var(--color-border)',
  borderRadius: '0.5rem',
}

const PIE_COLORS = ['#2563eb', '#7c3aed', '#d97706', '#16a34a', '#dc2626']

function dateStr(d: Date) { return d.toISOString().split('T')[0] }

export default function CustomerAnalyticsPage() {
  const defaults = useMemo(() => {
    const to = new Date(); const from = new Date(); from.setDate(to.getDate() - 29)
    return { from: dateStr(from), to: dateStr(to) }
  }, [])

  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [branchId, setBranchId] = useState('')

  const { data: branches } = useBranches()
  const { data, isLoading } = useCustomerAnalytics({ from, to, branchId: branchId || undefined })

  const trendData = data?.dailyTrend.map((d) => ({
    date: new Date(d.date).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' }),
    new: d.newCustomers,
    returning: d.returningCustomers,
    total: d.totalTransactions,
  })) ?? []

  const freqData = [...(data?.visitFrequencyDistribution ?? [])]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Customer Analytics</h1>
        <p className="text-sm text-muted-foreground">Customer behavior, retention, and visit patterns</p>
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
      </div>

      {/* KPI Cards */}
      <div className="grid gap-4 grid-cols-2 md:grid-cols-3 lg:grid-cols-6">
        {isLoading ? (
          Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-24 rounded-xl" />)
        ) : (
          <>
            <KpiCard label="Total Customers" value={data?.totalCustomers ?? 0} />
            <KpiCard label="New Customers" value={data?.newCustomers ?? 0} color="text-green-600" />
            <KpiCard label="Returning" value={data?.returningCustomers ?? 0} color="text-blue-600" />
            <KpiCard label="Retention Rate" value={`${data?.retentionRate ?? 0}%`} color="text-purple-600" />
            <KpiCard label="Avg Visits" value={data?.averageVisitsPerCustomer ?? 0} />
            <KpiCard label="Avg Spend/Visit" value={formatPeso(data?.averageSpendPerVisit ?? 0)} />
          </>
        )}
      </div>

      {/* Charts Row */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* New vs Returning Trend */}
        <div className="rounded-xl border bg-card p-4">
          <h3 className="text-sm font-medium mb-4">New vs Returning Customers</h3>
          {isLoading ? <Skeleton className="h-64" /> : (
            <ResponsiveContainer width="100%" height={260}>
              <LineChart data={trendData}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="date" fontSize={11} tickLine={false} />
                <YAxis fontSize={11} tickLine={false} allowDecimals={false} />
                <Tooltip contentStyle={TOOLTIP_STYLE} />
                <Legend />
                <Line type="monotone" dataKey="new" name="New" stroke="#16a34a" strokeWidth={2} dot={false} />
                <Line type="monotone" dataKey="returning" name="Returning" stroke="#2563eb" strokeWidth={2} dot={false} />
              </LineChart>
            </ResponsiveContainer>
          )}
        </div>

        {/* Visit Frequency Distribution */}
        <div className="rounded-xl border bg-card p-4">
          <h3 className="text-sm font-medium mb-4">Visit Frequency Distribution</h3>
          {isLoading ? <Skeleton className="h-64" /> : (
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={freqData}>
                <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                <XAxis dataKey="bucket" fontSize={11} tickLine={false} />
                <YAxis fontSize={11} tickLine={false} allowDecimals={false} />
                <Tooltip contentStyle={TOOLTIP_STYLE} />
                <Bar dataKey="customerCount" name="Customers" fill="#2563eb" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>

      {/* Top Customers Table */}
      <div className="rounded-xl border bg-card">
        <div className="p-4 border-b">
          <h3 className="text-sm font-medium">Top Customers by Spend</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="text-left px-4 py-2 font-medium">#</th>
                <th className="text-left px-4 py-2 font-medium">Customer</th>
                <th className="text-left px-4 py-2 font-medium">Plate</th>
                <th className="text-right px-4 py-2 font-medium">Visits</th>
                <th className="text-right px-4 py-2 font-medium">Total Spent</th>
                <th className="text-right px-4 py-2 font-medium">Avg Spend</th>
                <th className="text-right px-4 py-2 font-medium">Last Visit</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <tr key={i} className="border-b"><td colSpan={7} className="px-4 py-3"><Skeleton className="h-4" /></td></tr>
                ))
              ) : data?.topCustomers.length === 0 ? (
                <tr><td colSpan={7} className="text-center text-muted-foreground py-8">No customer data for this period</td></tr>
              ) : (
                data?.topCustomers.map((c, i) => (
                  <tr key={c.customerId} className="border-b hover:bg-muted/30 transition-colors">
                    <td className="px-4 py-2.5 text-muted-foreground">{i + 1}</td>
                    <td className="px-4 py-2.5 font-medium">{c.customerName}</td>
                    <td className="px-4 py-2.5 font-mono text-xs">{c.plateNumber ?? '—'}</td>
                    <td className="px-4 py-2.5 text-right">{c.visitCount}</td>
                    <td className="px-4 py-2.5 text-right font-medium">{formatPeso(c.totalSpent)}</td>
                    <td className="px-4 py-2.5 text-right">{formatPeso(c.averageSpend)}</td>
                    <td className="px-4 py-2.5 text-right text-muted-foreground">
                      {new Date(c.lastVisit).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' })}
                    </td>
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

function KpiCard({ label, value, color }: { label: string; value: string | number; color?: string }) {
  return (
    <div className="rounded-xl border bg-card p-4">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className={`text-2xl font-bold mt-1 ${color ?? ''}`}>{value}</p>
    </div>
  )
}
