'use client'

import { useState, useMemo } from 'react'
import { DatePicker } from '@/components/ui/date-picker'
import { Skeleton } from '@/components/ui/skeleton'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts'
import { useProfitLossReport } from '@/hooks/use-expenses'
import { useBranches } from '@/hooks/use-branches'
import { formatPeso, formatPesoCompact } from '@/lib/format'

const TOOLTIP_STYLE: React.CSSProperties = { backgroundColor: 'var(--color-popover)', color: 'var(--color-popover-foreground)', border: '1px solid var(--color-border)', borderRadius: '0.5rem' }

function dateStr(d: Date) { return d.toISOString().split('T')[0] }

export default function ProfitLossPage() {
  const defaults = useMemo(() => {
    const to = new Date(); const from = new Date(); from.setDate(to.getDate() - 29)
    return { from: dateStr(from), to: dateStr(to) }
  }, [])

  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [branchId, setBranchId] = useState('')

  const { data: branches } = useBranches()
  const { data, isLoading } = useProfitLossReport({ from, to, branchId: branchId || undefined })

  const trendData = data?.dailyBreakdown.map((d) => ({
    date: new Date(d.date).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' }),
    revenue: d.revenue,
    expenses: d.expenses,
    netProfit: d.netProfit,
  })) ?? []

  const categoryData = data?.expensesByCategory.map((c) => ({
    name: c.categoryName,
    amount: c.amount,
  })) ?? []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Profit & Loss</h1>
        <p className="text-sm text-muted-foreground">Revenue vs expenses breakdown</p>
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

      {isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : data ? (
        <>
          {/* KPI Cards */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <KpiCard label="Revenue" value={formatPeso(data.revenue)} color="text-foreground" />
            <KpiCard label="Total Expenses" value={formatPeso(data.totalExpenses)} color="text-red-600 dark:text-red-400" />
            <KpiCard label="Net Profit" value={formatPeso(data.netProfit)}
              color={data.netProfit >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'} />
            <KpiCard label="Margin" value={`${data.marginPercent}%`}
              color={data.marginPercent >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'} />
          </div>

          {/* Charts */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Revenue vs Expenses trend */}
            {trendData.length > 0 && (
              <div className="lg:col-span-2 rounded-lg border p-4">
                <p className="text-sm font-medium mb-3">Revenue vs Expenses</p>
                <ResponsiveContainer width="100%" height={250}>
                  <LineChart data={trendData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
                    <XAxis dataKey="date" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} interval="preserveStartEnd" />
                    <YAxis tick={{ fontSize: 11 }} tickLine={false} axisLine={false} tickFormatter={(v) => formatPesoCompact(v)} />
                    <Tooltip formatter={(v: number, name: string) => [formatPeso(v), name]} contentStyle={TOOLTIP_STYLE} />
                    <Legend wrapperStyle={{ fontSize: 11 }} />
                    <Line type="monotone" dataKey="revenue" stroke="#2563eb" strokeWidth={2} dot={false} />
                    <Line type="monotone" dataKey="expenses" stroke="#dc2626" strokeWidth={2} dot={false} />
                    <Line type="monotone" dataKey="netProfit" stroke="#16a34a" strokeWidth={2} strokeDasharray="5 5" dot={false} />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            )}

            {/* Expenses by category */}
            {categoryData.length > 0 && (
              <div className="rounded-lg border p-4">
                <p className="text-sm font-medium mb-3">Expenses by Category</p>
                <ResponsiveContainer width="100%" height={250}>
                  <BarChart data={categoryData} layout="vertical">
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
                    <XAxis type="number" tick={{ fontSize: 10 }} tickLine={false} axisLine={false} tickFormatter={(v) => formatPesoCompact(v)} />
                    <YAxis type="category" dataKey="name" tick={{ fontSize: 10 }} tickLine={false} axisLine={false} width={100} />
                    <Tooltip formatter={(v: number) => formatPeso(v)} contentStyle={TOOLTIP_STYLE} />
                    <Bar dataKey="amount" fill="#dc2626" radius={[0, 4, 4, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            )}
          </div>

          {/* Daily breakdown table */}
          {data.dailyBreakdown.length > 0 && (
            <div className="rounded-lg border overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-2.5 text-left font-medium">Date</th>
                    <th className="px-4 py-2.5 text-right font-medium">Revenue</th>
                    <th className="px-4 py-2.5 text-right font-medium">Expenses</th>
                    <th className="px-4 py-2.5 text-right font-medium">Net Profit</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {data.dailyBreakdown.map((d) => (
                    <tr key={d.date} className="hover:bg-muted/30">
                      <td className="px-4 py-2">{new Date(d.date).toLocaleDateString('en-PH', { weekday: 'short', month: 'short', day: 'numeric' })}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{formatPeso(d.revenue)}</td>
                      <td className="px-4 py-2 text-right tabular-nums text-red-600 dark:text-red-400">{d.expenses > 0 ? formatPeso(d.expenses) : '—'}</td>
                      <td className={`px-4 py-2 text-right tabular-nums font-medium ${d.netProfit >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}`}>
                        {formatPeso(d.netProfit)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      ) : null}
    </div>
  )
}

function KpiCard({ label, value, color }: { label: string; value: string; color: string }) {
  return (
    <div className="rounded-lg border px-4 py-3">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className={`text-xl font-bold tabular-nums mt-0.5 ${color}`}>{value}</p>
    </div>
  )
}
