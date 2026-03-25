'use client'

import { useState, useMemo } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, ReferenceLine,
} from 'recharts'
import { useShiftVarianceReport } from '@/hooks/use-shifts'
import { useBranches } from '@/hooks/use-branches'
import { cn } from '@/lib/utils'
import type { ShiftVarianceCashierDto } from '@splashsphere/types'
import { formatPeso } from '@/lib/format'

function dateStr(d: Date) { return d.toISOString().split('T')[0] }

function defaultDates() {
  const to = new Date()
  const from = new Date()
  from.setDate(to.getDate() - 29)
  return { from: dateStr(from), to: dateStr(to) }
}

function VarianceBadge({ value }: { value: number }) {
  const abs = Math.abs(value)
  if (abs <= 50) return <span className="text-green-700 dark:text-green-400 font-mono font-semibold text-sm">{value >= 0 ? '+' : ''}{formatPeso(value)}</span>
  if (abs <= 200) return <span className="text-amber-600 dark:text-amber-400 font-mono font-semibold text-sm">{value >= 0 ? '+' : ''}{formatPeso(value)}</span>
  return <span className="text-red-600 dark:text-red-400 font-mono font-semibold text-sm">{value >= 0 ? '+' : ''}{formatPeso(value)}</span>
}

function CashierRow({ cashier, onClick, selected }: {
  cashier: ShiftVarianceCashierDto
  onClick: () => void
  selected: boolean
}) {
  const isConsistentlyNegative = cashier.totalVariance < -200

  return (
    <tr
      className={cn(
        'cursor-pointer transition-colors',
        selected ? 'bg-primary/5 border-l-2 border-primary' : 'hover:bg-muted/40',
        isConsistentlyNegative && 'bg-red-50/40 dark:bg-red-950/20'
      )}
      onClick={onClick}
    >
      <td className="px-4 py-3 font-medium">
        <div className="flex items-center gap-2">
          {cashier.cashierName}
          {isConsistentlyNegative && (
            <StatusBadge status="Watch" />
          )}
        </div>
      </td>
      <td className="px-4 py-3 text-right tabular-nums">{cashier.shiftCount}</td>
      <td className="px-4 py-3 text-right"><VarianceBadge value={cashier.totalVariance} /></td>
      <td className="px-4 py-3 text-right"><VarianceBadge value={cashier.averageVariance} /></td>
      <td className="px-4 py-3 text-right font-mono text-red-600 dark:text-red-400 font-semibold text-sm">
        {cashier.largestShortage < 0 ? formatPeso(cashier.largestShortage) : '—'}
      </td>
    </tr>
  )
}

export default function ShiftVariancePage() {
  const defaults = useMemo(() => defaultDates(), [])
  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [branchId, setBranchId] = useState('')
  const [selectedCashierId, setSelectedCashierId] = useState<string | null>(null)

  const { data: branches = [] } = useBranches()

  const { data, isLoading } = useShiftVarianceReport({
    branchId: branchId || undefined,
    dateFrom: from,
    dateTo: to,
  })

  // When a cashier row is clicked, re-fetch with cashierId to get trend data
  const { data: cashierData } = useShiftVarianceReport({
    branchId: branchId || undefined,
    cashierId: selectedCashierId ?? undefined,
    dateFrom: from,
    dateTo: to,
  })

  const trendData = cashierData?.trendPoints?.map(p => ({
    date: new Date(p.shiftDate + 'T00:00:00').toLocaleDateString('en-PH', { month: 'short', day: 'numeric' }),
    variance: p.variance,
  })) ?? []

  const cashiers = data?.cashierSummaries ?? []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Shift Variance Analysis</h1>
        <p className="text-muted-foreground">Track cash variance patterns per cashier over time</p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">From</label>
          <Input type="date" value={from} onChange={e => setFrom(e.target.value)} className="h-9 w-36" />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">To</label>
          <Input type="date" value={to} onChange={e => setTo(e.target.value)} className="h-9 w-36" />
        </div>
        <Select value={branchId || '__all__'} onValueChange={v => setBranchId(v === '__all__' ? '' : v)}>
          <SelectTrigger className="w-44">
            <SelectValue placeholder="All branches" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="__all__">All branches</SelectItem>
            {branches.map(b => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
          </SelectContent>
        </Select>
      </div>

      {/* Trend chart — shown when a cashier is selected */}
      {selectedCashierId && trendData.length > 0 && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">
              Variance Trend — {cashiers.find(c => c.cashierId === selectedCashierId)?.cashierName}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={220}>
              <LineChart data={trendData}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis dataKey="date" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} interval="preserveStartEnd" />
                <YAxis
                  tick={{ fontSize: 11 }}
                  tickLine={false}
                  axisLine={false}
                  tickFormatter={(v: number) => `₱${v >= 0 ? '+' : ''}${v}`}
                />
                <Tooltip
                  formatter={(v: number) => [
                    `${v >= 0 ? '+' : ''}${formatPeso(v)}`,
                    'Variance',
                  ]}
                />
                <ReferenceLine y={0} stroke="hsl(var(--muted-foreground))" strokeWidth={1} />
                <ReferenceLine y={50} stroke="oklch(0.55 0.16 155)" strokeDasharray="4 4" strokeWidth={1} />
                <ReferenceLine y={-50} stroke="oklch(0.55 0.16 155)" strokeDasharray="4 4" strokeWidth={1} />
                <ReferenceLine y={200} stroke="oklch(0.70 0.16 70)" strokeDasharray="4 4" strokeWidth={1} />
                <ReferenceLine y={-200} stroke="oklch(0.70 0.16 70)" strokeDasharray="4 4" strokeWidth={1} />
                <Line
                  type="monotone"
                  dataKey="variance"
                  stroke="hsl(var(--chart-1))"
                  strokeWidth={2}
                  dot={{ r: 3, fill: 'hsl(var(--chart-1))' }}
                  activeDot={{ r: 5 }}
                />
              </LineChart>
            </ResponsiveContainer>
            <div className="flex gap-4 mt-2 text-xs text-muted-foreground">
              <span className="flex items-center gap-1"><span className="inline-block w-4 border-t-2 border-green-600 border-dashed" /> ±₱50 auto-approve</span>
              <span className="flex items-center gap-1"><span className="inline-block w-4 border-t-2 border-amber-500 border-dashed" /> ±₱200 flag threshold</span>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Cashier summary table */}
      {isLoading ? (
        <Skeleton className="h-64 w-full" />
      ) : cashiers.length === 0 ? (
        <div className="rounded-lg border border-dashed p-12 text-center text-sm text-muted-foreground">
          No closed shifts found for the selected period.
        </div>
      ) : (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Cashier</th>
                <th className="px-4 py-2.5 text-right font-medium">Shifts</th>
                <th className="px-4 py-2.5 text-right font-medium">Total Variance</th>
                <th className="px-4 py-2.5 text-right font-medium">Avg Variance</th>
                <th className="px-4 py-2.5 text-right font-medium">Largest Shortage</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {cashiers.map(c => (
                <CashierRow
                  key={c.cashierId}
                  cashier={c}
                  selected={selectedCashierId === c.cashierId}
                  onClick={() => setSelectedCashierId(prev => prev === c.cashierId ? null : c.cashierId)}
                />
              ))}
            </tbody>
          </table>
          <p className="px-4 py-2 text-xs text-muted-foreground bg-muted/30 border-t">
            Click a row to view the cashier&apos;s variance trend. Rows in red indicate consistently negative variance.
          </p>
        </div>
      )}
    </div>
  )
}
