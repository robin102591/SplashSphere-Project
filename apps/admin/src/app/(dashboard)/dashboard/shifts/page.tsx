'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { StatusBadge } from '@/components/ui/status-badge'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { useShifts } from '@/hooks/use-shifts'
import { useBranches } from '@/hooks/use-branches'
import { ShiftStatus, ReviewStatus } from '@splashsphere/types'
import { cn } from '@/lib/utils'
import type { ShiftSummaryDto } from '@splashsphere/types'
import { Button } from '@/components/ui/button'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { formatPeso } from '@/lib/format'

// ── Status helpers ─────────────────────────────────────────────────────────────

const SHIFT_STATUS_KEYS: Record<ShiftStatus, string> = {
  [ShiftStatus.Open]: 'Open',
  [ShiftStatus.Closed]: 'Closed',
  [ShiftStatus.Voided]: 'Voided',
}

const REVIEW_STATUS_KEYS: Record<ReviewStatus, string> = {
  [ReviewStatus.Pending]: 'Pending',
  [ReviewStatus.Approved]: 'Approved',
  [ReviewStatus.Flagged]: 'Flagged',
}

function VarianceCell({ variance }: { variance: number }) {
  const abs = Math.abs(variance)
  const color = abs <= 50 ? 'text-green-700' : abs <= 200 ? 'text-amber-600' : 'text-red-600'
  return (
    <span className={cn('font-mono tabular-nums font-semibold', color)}>
      {variance >= 0 ? '+' : ''}{formatPeso(variance)}
    </span>
  )
}

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-PH', {
    hour: 'numeric', minute: '2-digit', hour12: true,
  })
}

function fmtDate(dateStr: string) {
  // dateStr is a DateOnly "yyyy-MM-dd"
  return new Date(dateStr + 'T00:00:00').toLocaleDateString('en-PH', {
    month: 'short', day: 'numeric', year: 'numeric',
  })
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function ShiftsPage() {
  const router = useRouter()
  const [branchId, setBranchId] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [statusFilter, setStatusFilter] = useState<string>('__all__')
  const [reviewFilter, setReviewFilter] = useState<string>('__all__')
  const [page, setPage] = useState(1)

  const { data: branches = [] } = useBranches()

  const { data, isLoading } = useShifts({
    branchId: branchId || undefined,
    dateFrom: dateFrom || undefined,
    dateTo: dateTo || undefined,
    status: statusFilter !== '__all__' ? Number(statusFilter) as ShiftStatus : undefined,
    reviewStatus: reviewFilter !== '__all__' ? Number(reviewFilter) as ReviewStatus : undefined,
    page,
    pageSize: 20,
  })

  const items: ShiftSummaryDto[] = data?.items ?? []
  const totalPages = data?.totalPages ?? 1

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Cashier Shifts</h1>
        <p className="text-muted-foreground">Review and manage end-of-day shift reports</p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <Select value={branchId || '__all__'} onValueChange={(v) => { setBranchId(v === '__all__' ? '' : v); setPage(1) }}>
          <SelectTrigger className="w-44">
            <SelectValue placeholder="All branches" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="__all__">All branches</SelectItem>
            {branches.map(b => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
          </SelectContent>
        </Select>

        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">From</label>
          <Input type="date" value={dateFrom} onChange={e => { setDateFrom(e.target.value); setPage(1) }} className="h-9 w-36" />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">To</label>
          <Input type="date" value={dateTo} onChange={e => { setDateTo(e.target.value); setPage(1) }} className="h-9 w-36" />
        </div>

        <Select value={statusFilter} onValueChange={(v) => { setStatusFilter(v); setPage(1) }}>
          <SelectTrigger className="w-36">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="__all__">All statuses</SelectItem>
            <SelectItem value={String(ShiftStatus.Open)}>Open</SelectItem>
            <SelectItem value={String(ShiftStatus.Closed)}>Closed</SelectItem>
            <SelectItem value={String(ShiftStatus.Voided)}>Voided</SelectItem>
          </SelectContent>
        </Select>

        <Select value={reviewFilter} onValueChange={(v) => { setReviewFilter(v); setPage(1) }}>
          <SelectTrigger className="w-40">
            <SelectValue placeholder="All reviews" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="__all__">All reviews</SelectItem>
            <SelectItem value={String(ReviewStatus.Pending)}>Pending</SelectItem>
            <SelectItem value={String(ReviewStatus.Approved)}>Approved</SelectItem>
            <SelectItem value={String(ReviewStatus.Flagged)}>Flagged</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <div className="rounded-lg border overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-muted/50">
            <tr>
              <th className="px-4 py-2.5 text-left font-medium">Date</th>
              <th className="px-4 py-2.5 text-left font-medium">Branch</th>
              <th className="px-4 py-2.5 text-left font-medium">Cashier</th>
              <th className="px-4 py-2.5 text-left font-medium">Opened</th>
              <th className="px-4 py-2.5 text-left font-medium">Closed</th>
              <th className="px-4 py-2.5 text-right font-medium">Revenue</th>
              <th className="px-4 py-2.5 text-right font-medium">Variance</th>
              <th className="px-4 py-2.5 text-left font-medium">Status</th>
              <th className="px-4 py-2.5 text-left font-medium">Review</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {isLoading
              ? Array.from({ length: 8 }).map((_, i) => (
                  <tr key={i}>
                    {Array.from({ length: 9 }).map((__, j) => (
                      <td key={j} className="px-4 py-3">
                        <Skeleton className="h-4 w-full" />
                      </td>
                    ))}
                  </tr>
                ))
              : items.length === 0
              ? (
                <tr>
                  <td colSpan={9} className="px-4 py-12 text-center text-muted-foreground">
                    No shifts found for the selected filters.
                  </td>
                </tr>
              )
              : items.map((shift) => (
                <tr
                  key={shift.id}
                  className="hover:bg-muted/40 cursor-pointer transition-colors"
                  onClick={() => router.push(`/dashboard/shifts/${shift.id}`)}
                >
                  <td className="px-4 py-3 font-medium">{fmtDate(shift.shiftDate)}</td>
                  <td className="px-4 py-3 text-muted-foreground">{shift.branchName}</td>
                  <td className="px-4 py-3">{shift.cashierName}</td>
                  <td className="px-4 py-3 text-muted-foreground font-mono text-xs">{fmtTime(shift.openedAt)}</td>
                  <td className="px-4 py-3 text-muted-foreground font-mono text-xs">
                    {shift.closedAt ? fmtTime(shift.closedAt) : '—'}
                  </td>
                  <td className="px-4 py-3 text-right tabular-nums font-semibold">{formatPeso(shift.totalRevenue)}</td>
                  <td className="px-4 py-3 text-right">
                    <VarianceCell variance={shift.variance} />
                  </td>
                  <td className="px-4 py-3"><StatusBadge status={SHIFT_STATUS_KEYS[shift.status]} /></td>
                  <td className="px-4 py-3"><StatusBadge status={REVIEW_STATUS_KEYS[shift.reviewStatus]} /></td>
                </tr>
              ))
            }
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Page {page} of {totalPages} · {data?.totalCount ?? 0} shifts
          </p>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" onClick={() => setPage(p => p - 1)} disabled={page <= 1}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button variant="outline" size="sm" onClick={() => setPage(p => p + 1)} disabled={page >= totalPages}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
