'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Plus, Wallet } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { EmptyState } from '@/components/ui/empty-state'
import { StatusBadge } from '@/components/ui/status-badge'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter,
} from '@/components/ui/dialog'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { usePayrollPeriods, useCreatePayrollPeriod, usePayrollSettings } from '@/hooks/use-payroll'
import { useBranches } from '@/hooks/use-branches'
import { PayrollStatus } from '@splashsphere/types'
import type { PayrollPeriodSummary } from '@splashsphere/types'
import { formatPeso } from '@/lib/format'
import { toast } from 'sonner'

const PAYROLL_STATUS_KEYS: Record<PayrollStatus, string> = {
  [PayrollStatus.Open]: 'Open',
  [PayrollStatus.Closed]: 'Closed',
  [PayrollStatus.Processed]: 'Processed',
  [PayrollStatus.Released]: 'Released',
}

function periodLabel(p: PayrollPeriodSummary): string {
  const start = new Date(p.startDate)
  const end = new Date(p.endDate)
  const daysDiff = Math.round((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24))
  if (daysDiff <= 7) return `${p.year} — Week ${p.cutOffWeek}`
  return `${p.year} — Period ${p.cutOffWeek}`
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
        {periodLabel(period)}
      </td>
      <td className="px-4 py-3 text-sm text-muted-foreground">
        {period.branchName ?? 'All Branches'}
      </td>
      <td className="px-4 py-3 text-sm text-muted-foreground">
        {startDate} – {endDate}
      </td>
      <td className="px-4 py-3">
        <StatusBadge status={PAYROLL_STATUS_KEYS[period.status]} />
      </td>
      <td className="px-4 py-3 text-sm text-center tabular-nums">{period.entryCount}</td>
      <td className="px-4 py-3 text-right font-medium tabular-nums">
        {formatPeso(period.totalNetPay)}
      </td>
    </tr>
  )
}

function formatDateOnly(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

function CreatePeriodDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (v: boolean) => void }) {
  const { data: branches } = useBranches()
  const [selectedBranch, setSelectedBranch] = useState<string>('')
  const { data: settings } = usePayrollSettings(selectedBranch || undefined)
  const { mutate: create, isPending } = useCreatePayrollPeriod()

  const isSemiMonthly = settings?.frequency === 2

  // Default dates based on frequency
  const today = new Date()
  const yesterday = new Date(today)
  yesterday.setDate(yesterday.getDate() - 1)
  const weekAgo = new Date(yesterday)
  weekAgo.setDate(weekAgo.getDate() - 6)

  // Semi-monthly: default to the most recent completed half
  const defaultSemiMonthlyDates = (): { start: string; end: string } => {
    const now = new Date()
    if (now.getDate() >= 16) {
      // We're in the second half — default to first half (1st–15th)
      return {
        start: `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`,
        end: `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-15`,
      }
    }
    // We're in the first half — default to second half of prev month
    const prev = new Date(now.getFullYear(), now.getMonth() - 1, 16)
    const lastDay = new Date(now.getFullYear(), now.getMonth(), 0).getDate()
    return {
      start: `${prev.getFullYear()}-${String(prev.getMonth() + 1).padStart(2, '0')}-16`,
      end: `${prev.getFullYear()}-${String(prev.getMonth() + 1).padStart(2, '0')}-${lastDay}`,
    }
  }

  const semiDefaults = defaultSemiMonthlyDates()
  const [startDate, setStartDate] = useState(isSemiMonthly ? semiDefaults.start : formatDateOnly(weekAgo))
  const [endDate, setEndDate] = useState(isSemiMonthly ? semiDefaults.end : formatDateOnly(yesterday))
  const [semiHalf, setSemiHalf] = useState<'first' | 'second'>('first')

  // Weekly: auto-compute end date (+6 days)
  const handleStartChange = (val: string) => {
    setStartDate(val)
    if (val && !isSemiMonthly) {
      const d = new Date(val)
      d.setDate(d.getDate() + 6)
      setEndDate(formatDateOnly(d))
    }
  }

  // Semi-monthly: pick month + half
  const handleSemiMonthChange = (month: string, half: 'first' | 'second') => {
    setSemiHalf(half)
    const [y, m] = month.split('-').map(Number)
    if (half === 'first') {
      setStartDate(`${y}-${String(m).padStart(2, '0')}-01`)
      setEndDate(`${y}-${String(m).padStart(2, '0')}-15`)
    } else {
      const lastDay = new Date(y, m, 0).getDate()
      setStartDate(`${y}-${String(m).padStart(2, '0')}-16`)
      setEndDate(`${y}-${String(m).padStart(2, '0')}-${lastDay}`)
    }
  }

  const handleCreate = () => {
    if (!startDate || !endDate) return
    create(
      { startDate, endDate, branchId: selectedBranch || undefined },
      {
        onSuccess: () => {
          toast.success('Payroll period created')
          onOpenChange(false)
        },
        onError: (err: unknown) => {
          const message = err instanceof Error ? err.message : 'Failed to create payroll period'
          toast.error(message)
        },
      }
    )
  }

  const currentMonth = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}`

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Create Payroll Period</DialogTitle>
          <DialogDescription>
            {isSemiMonthly
              ? 'Create a semi-monthly payroll period (1st–15th or 16th–end).'
              : 'Manually create a 7-day payroll period. The end date is auto-calculated.'}
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Branch</Label>
            <Select value={selectedBranch || 'none'} onValueChange={(v) => setSelectedBranch(v === 'none' ? '' : v)}>
              <SelectTrigger><SelectValue placeholder="Select branch" /></SelectTrigger>
              <SelectContent>
                <SelectItem value="none">Select a branch</SelectItem>
                {branches?.map((b) => (
                  <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          {isSemiMonthly ? (
            <>
              <div className="space-y-1.5">
                <Label>Month</Label>
                <Input
                  type="month"
                  defaultValue={currentMonth}
                  onChange={(e) => handleSemiMonthChange(e.target.value, semiHalf)}
                />
              </div>
              <div className="space-y-1.5">
                <Label>Half</Label>
                <Select value={semiHalf} onValueChange={(v) => handleSemiMonthChange(startDate.slice(0, 7), v as 'first' | 'second')}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="first">1st – 15th</SelectItem>
                    <SelectItem value="second">16th – End of month</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <p className="text-xs text-muted-foreground">
                Period: {startDate} to {endDate}
              </p>
            </>
          ) : (
            <>
              <div className="space-y-1.5">
                <Label>Start Date</Label>
                <Input type="date" value={startDate} onChange={(e) => handleStartChange(e.target.value)} />
              </div>
              <div className="space-y-1.5">
                <Label>End Date</Label>
                <Input type="date" value={endDate} disabled />
                <p className="text-xs text-muted-foreground">Automatically set to 6 days after start date.</p>
              </div>
            </>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleCreate} disabled={isPending || !startDate || !selectedBranch}>
            {isPending ? 'Creating…' : 'Create Period'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

export default function PayrollPage() {
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const [branchFilter, setBranchFilter] = useState<string>('all')
  const [createOpen, setCreateOpen] = useState(false)
  const currentYear = new Date().getFullYear()
  const [yearFilter, setYearFilter] = useState<string>(String(currentYear))

  const { data: branches } = useBranches()

  const { data, isLoading, isError } = usePayrollPeriods({
    status: statusFilter !== 'all' ? (Number(statusFilter) as PayrollStatus) : undefined,
    year: yearFilter !== 'all' ? Number(yearFilter) : undefined,
    branchId: branchFilter !== 'all' ? branchFilter : undefined,
    pageSize: 52,
  })

  const periods = data ? [...data.items] : []
  const years = Array.from({ length: 3 }, (_, i) => currentYear - i)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Payroll</h1>
          <p className="text-muted-foreground">
            Payroll periods — review, close, and process
          </p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Create Period
        </Button>
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

        <Select value={branchFilter} onValueChange={setBranchFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="All branches" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Branches</SelectItem>
            {branches?.map((b) => (
              <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
            ))}
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
        <EmptyState
          icon={Wallet}
          title="No payroll periods found"
          description="Periods are created automatically by the background job"
        />
      )}

      {!isLoading && periods.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Period</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Branch</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Dates</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Status</th>
                <th className="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider text-muted-foreground">Entries</th>
                <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">Total Net Pay</th>
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

      <CreatePeriodDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  )
}
