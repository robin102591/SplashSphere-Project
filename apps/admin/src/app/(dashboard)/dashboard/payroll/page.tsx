'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Wallet } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { usePayrollPeriods } from '@/hooks/use-payroll'
import { PayrollStatus } from '@splashsphere/types'
import type { PayrollPeriodSummary } from '@splashsphere/types'

const php = new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' })

function StatusBadge({ status }: { status: PayrollStatus }) {
  if (status === PayrollStatus.Open)
    return <Badge className="bg-blue-500/15 text-blue-700 border-blue-200">Open</Badge>
  if (status === PayrollStatus.Closed)
    return <Badge className="bg-amber-500/15 text-amber-700 border-amber-200">Closed</Badge>
  return <Badge className="bg-green-500/15 text-green-700 border-green-200">Processed</Badge>
}

function PeriodRow({ period }: { period: PayrollPeriodSummary }) {
  const router = useRouter()

  const startDate = new Date(period.startDate).toLocaleDateString('en-PH', {
    month: 'short',
    day: 'numeric',
  })
  const endDate = new Date(period.endDate).toLocaleDateString('en-PH', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })

  return (
    <tr
      className="hover:bg-muted/40 cursor-pointer transition-colors"
      onClick={() => router.push(`/dashboard/payroll/${period.id}`)}
    >
      <td className="px-4 py-3 font-medium">
        {period.year} — Week {period.cutOffWeek}
      </td>
      <td className="px-4 py-3 text-sm text-muted-foreground">
        {startDate} – {endDate}
      </td>
      <td className="px-4 py-3">
        <StatusBadge status={period.status} />
      </td>
      <td className="px-4 py-3 text-sm text-center tabular-nums">{period.entryCount}</td>
      <td className="px-4 py-3 text-right font-medium tabular-nums">
        {php.format(period.totalNetPay)}
      </td>
    </tr>
  )
}

export default function PayrollPage() {
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const currentYear = new Date().getFullYear()
  const [yearFilter, setYearFilter] = useState<string>(String(currentYear))

  const { data, isLoading, isError } = usePayrollPeriods({
    status: statusFilter !== 'all' ? (Number(statusFilter) as PayrollStatus) : undefined,
    year: yearFilter !== 'all' ? Number(yearFilter) : undefined,
    pageSize: 52,
  })

  const periods = data ? [...data.items] : []
  const years = Array.from({ length: 3 }, (_, i) => currentYear - i)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Payroll</h1>
        <p className="text-muted-foreground">
          Weekly payroll periods — review, close, and process
        </p>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3">
        <Select value={yearFilter} onValueChange={setYearFilter}>
          <SelectTrigger className="w-32">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All years</SelectItem>
            {years.map((y) => (
              <SelectItem key={y} value={String(y)}>
                {y}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-40">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All statuses</SelectItem>
            <SelectItem value={String(PayrollStatus.Open)}>Open</SelectItem>
            <SelectItem value={String(PayrollStatus.Closed)}>Closed</SelectItem>
            <SelectItem value={String(PayrollStatus.Processed)}>Processed</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
          Failed to load payroll periods.
        </div>
      )}

      {!isLoading && !isError && periods.length === 0 && (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed p-16 text-center gap-3">
          <Wallet className="h-10 w-10 text-muted-foreground/40" />
          <div>
            <p className="font-medium">No payroll periods found</p>
            <p className="text-sm text-muted-foreground">
              Periods are created automatically every Monday by the background job
            </p>
          </div>
        </div>
      )}

      {!isLoading && periods.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left font-medium">Period</th>
                <th className="px-4 py-3 text-left font-medium">Dates</th>
                <th className="px-4 py-3 text-left font-medium">Status</th>
                <th className="px-4 py-3 text-center font-medium">Entries</th>
                <th className="px-4 py-3 text-right font-medium">Total Net Pay</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {periods.map((p) => (
                <PeriodRow key={p.id} period={p} />
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
