'use client'

import { useState, useMemo } from 'react'
import { DatePicker } from '@/components/ui/date-picker'
import { Skeleton } from '@/components/ui/skeleton'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { usePeakHours } from '@/hooks/use-analytics'
import { useBranches } from '@/hooks/use-branches'
import type { HourlySlot } from '@splashsphere/types'
import { cn } from '@/lib/utils'

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
const HOURS = Array.from({ length: 24 }, (_, i) => i)

function dateStr(d: Date) { return d.toISOString().split('T')[0] }

function formatHour(h: number) {
  if (h === 0) return '12am'
  if (h < 12) return `${h}am`
  if (h === 12) return '12pm'
  return `${h - 12}pm`
}

function getIntensity(count: number, max: number): string {
  if (max === 0 || count === 0) return 'bg-muted/30'
  const ratio = count / max
  if (ratio > 0.75) return 'bg-red-500 text-white'
  if (ratio > 0.5) return 'bg-orange-400 text-white'
  if (ratio > 0.25) return 'bg-yellow-300 text-yellow-900'
  if (ratio > 0) return 'bg-green-200 text-green-900'
  return 'bg-muted/30'
}

export default function PeakHoursPage() {
  const defaults = useMemo(() => {
    const to = new Date(); const from = new Date(); from.setDate(to.getDate() - 29)
    return { from: dateStr(from), to: dateStr(to) }
  }, [])

  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [branchId, setBranchId] = useState('')
  const [mode, setMode] = useState<'transactions' | 'revenue'>('transactions')

  const { data: branches } = useBranches()
  const { data, isLoading } = usePeakHours({ from, to, branchId: branchId || undefined })

  // Build lookup: [day][hour] → slot
  const slotMap = useMemo(() => {
    const map: Record<string, HourlySlot> = {}
    data?.slots.forEach((s) => { map[`${s.dayOfWeek}-${s.hour}`] = s })
    return map
  }, [data])

  const maxVal = useMemo(() => {
    if (!data) return 0
    return Math.max(
      ...data.slots.map((s) => mode === 'transactions' ? s.transactionCount : s.revenue),
      1,
    )
  }, [data, mode])

  // Filter to business hours (6am-10pm) for cleaner display
  const displayHours = HOURS.filter((h) => h >= 6 && h <= 22)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Peak Hours Analysis</h1>
        <p className="text-sm text-muted-foreground">Identify busiest times for staffing and scheduling decisions</p>
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
        <Select value={mode} onValueChange={(v) => setMode(v as 'transactions' | 'revenue')}>
          <SelectTrigger className="w-[160px] h-9"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem value="transactions">By Transactions</SelectItem>
            <SelectItem value="revenue">By Revenue</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Summary KPIs */}
      {!isLoading && data && (
        <div className="grid gap-4 grid-cols-3">
          <div className="rounded-xl border bg-card p-4">
            <p className="text-xs text-muted-foreground">Total Transactions</p>
            <p className="text-2xl font-bold mt-1">{data.totalTransactions.toLocaleString()}</p>
          </div>
          <div className="rounded-xl border bg-card p-4">
            <p className="text-xs text-muted-foreground">Busiest Day</p>
            <p className="text-2xl font-bold mt-1">{data.peakDay}</p>
          </div>
          <div className="rounded-xl border bg-card p-4">
            <p className="text-xs text-muted-foreground">Busiest Hour</p>
            <p className="text-2xl font-bold mt-1">{formatHour(data.peakHour)}</p>
          </div>
        </div>
      )}

      {/* Heatmap */}
      <div className="rounded-xl border bg-card p-4 overflow-x-auto">
        <h3 className="text-sm font-medium mb-4">
          {mode === 'transactions' ? 'Transaction Volume' : 'Revenue'} Heatmap
        </h3>
        {isLoading ? (
          <Skeleton className="h-64" />
        ) : (
          <div className="min-w-[700px]">
            {/* Header row: hours */}
            <div className="grid gap-1" style={{ gridTemplateColumns: `60px repeat(${displayHours.length}, 1fr)` }}>
              <div />
              {displayHours.map((h) => (
                <div key={h} className="text-center text-[10px] text-muted-foreground font-medium py-1">
                  {formatHour(h)}
                </div>
              ))}
            </div>

            {/* Day rows */}
            {DAY_NAMES.map((day, dayIdx) => (
              <div
                key={day}
                className="grid gap-1 mt-1"
                style={{ gridTemplateColumns: `60px repeat(${displayHours.length}, 1fr)` }}
              >
                <div className="flex items-center text-xs font-medium text-muted-foreground pr-2 justify-end">
                  {day}
                </div>
                {displayHours.map((h) => {
                  const slot = slotMap[`${dayIdx}-${h}`]
                  const val = slot
                    ? (mode === 'transactions' ? slot.transactionCount : slot.revenue)
                    : 0
                  return (
                    <div
                      key={h}
                      title={`${day} ${formatHour(h)}: ${mode === 'transactions' ? `${val} transactions` : `₱${val.toLocaleString()}`}`}
                      className={cn(
                        'rounded-sm h-8 flex items-center justify-center text-[10px] font-medium transition-colors cursor-default',
                        getIntensity(val, maxVal),
                      )}
                    >
                      {val > 0 ? (mode === 'transactions' ? val : '') : ''}
                    </div>
                  )
                })}
              </div>
            ))}

            {/* Legend */}
            <div className="flex items-center gap-2 mt-4 text-xs text-muted-foreground">
              <span>Less</span>
              <div className="h-4 w-6 rounded-sm bg-muted/30" />
              <div className="h-4 w-6 rounded-sm bg-green-200" />
              <div className="h-4 w-6 rounded-sm bg-yellow-300" />
              <div className="h-4 w-6 rounded-sm bg-orange-400" />
              <div className="h-4 w-6 rounded-sm bg-red-500" />
              <span>More</span>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
