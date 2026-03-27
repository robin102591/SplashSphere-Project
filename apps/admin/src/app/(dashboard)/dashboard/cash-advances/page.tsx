'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
  AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  useCashAdvances,
  useCreateCashAdvance,
  useApproveCashAdvance,
  useDisburseCashAdvance,
  useCancelCashAdvance,
} from '@/hooks/use-cash-advances'
import { useEmployees } from '@/hooks/use-employees'
import { CashAdvanceStatus } from '@splashsphere/types'
import type { CashAdvance } from '@splashsphere/types'
import { formatPeso } from '@/lib/format'
import { ChevronLeft, ChevronRight, Plus, Check, Banknote, X } from 'lucide-react'

// ── Status config ────────────────────────────────────────────────────────────

const STATUS_LABELS: Record<CashAdvanceStatus, { status: string; label?: string }> = {
  [CashAdvanceStatus.Pending]:   { status: 'Pending' },
  [CashAdvanceStatus.Approved]:  { status: 'Approved' },
  [CashAdvanceStatus.Active]:    { status: 'Active' },
  [CashAdvanceStatus.FullyPaid]: { status: 'Fully Paid' },
  [CashAdvanceStatus.Cancelled]: { status: 'Cancelled' },
}

function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-PH', {
    month: 'short', day: 'numeric', year: 'numeric',
  })
}

// ── Create Dialog ────────────────────────────────────────────────────────────

function CreateAdvanceDialog({
  open,
  onOpenChange,
}: {
  open: boolean
  onOpenChange: (v: boolean) => void
}) {
  const { data: empData } = useEmployees({ pageSize: 200 })
  const employees = empData?.items ?? []
  const create = useCreateCashAdvance()

  const [employeeId, setEmployeeId] = useState('')
  const [amount, setAmount] = useState('')
  const [deductionPerPeriod, setDeductionPerPeriod] = useState('')
  const [reason, setReason] = useState('')

  function reset() {
    setEmployeeId('')
    setAmount('')
    setDeductionPerPeriod('')
    setReason('')
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!employeeId || !amount || !deductionPerPeriod) return
    await create.mutateAsync({
      employeeId,
      amount: Number(amount),
      deductionPerPeriod: Number(deductionPerPeriod),
      reason: reason || undefined,
    })
    reset()
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>New Cash Advance</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label>Employee</Label>
            <Select value={employeeId} onValueChange={setEmployeeId}>
              <SelectTrigger><SelectValue placeholder="Select employee" /></SelectTrigger>
              <SelectContent>
                {employees.map(e => (
                  <SelectItem key={e.id} value={e.id}>
                    {e.firstName} {e.lastName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Amount (PHP)</Label>
              <Input type="number" step="0.01" min="1" value={amount} onChange={e => setAmount(e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label>Deduction / Period</Label>
              <Input type="number" step="0.01" min="1" value={deductionPerPeriod} onChange={e => setDeductionPerPeriod(e.target.value)} />
            </div>
          </div>
          <div className="space-y-2">
            <Label>Reason (optional)</Label>
            <Textarea value={reason} onChange={e => setReason(e.target.value)} rows={2} />
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
            <Button type="submit" disabled={create.isPending || !employeeId || !amount || !deductionPerPeriod}>
              {create.isPending ? 'Creating…' : 'Create'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function CashAdvancesPage() {
  const [statusFilter, setStatusFilter] = useState<string>('__all__')
  const [page, setPage] = useState(1)
  const [createOpen, setCreateOpen] = useState(false)
  const [confirmAction, setConfirmAction] = useState<{ id: string; action: 'approve' | 'disburse' | 'cancel' } | null>(null)

  const { data, isLoading } = useCashAdvances({
    status: statusFilter !== '__all__' ? Number(statusFilter) as CashAdvanceStatus : undefined,
    page,
    pageSize: 20,
  })

  const approve = useApproveCashAdvance()
  const disburse = useDisburseCashAdvance()
  const cancel = useCancelCashAdvance()

  const items = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.ceil(totalCount / 20)

  async function handleConfirm() {
    if (!confirmAction) return
    const { id, action } = confirmAction
    if (action === 'approve') await approve.mutateAsync(id)
    else if (action === 'disburse') await disburse.mutateAsync(id)
    else await cancel.mutateAsync(id)
    setConfirmAction(null)
  }

  return (
    <div className="space-y-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Cash Advances</h1>
          <p className="text-sm text-muted-foreground">Manage employee cash advances and payroll deductions</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" /> New Cash Advance
        </Button>
      </div>

      {/* Filters */}
      <div className="flex gap-3">
        <Select value={statusFilter} onValueChange={v => { setStatusFilter(v); setPage(1) }}>
          <SelectTrigger className="w-[160px]"><SelectValue placeholder="All statuses" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="__all__">All Statuses</SelectItem>
            {Object.entries(STATUS_LABELS).map(([val, { status, label }]) => (
              <SelectItem key={val} value={val}>{label ?? status}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <div className="rounded-md border">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="px-4 py-3 text-left font-medium">Employee</th>
              <th className="px-4 py-3 text-right font-medium">Amount</th>
              <th className="px-4 py-3 text-right font-medium">Remaining</th>
              <th className="px-4 py-3 text-right font-medium">Per Period</th>
              <th className="px-4 py-3 text-left font-medium">Status</th>
              <th className="px-4 py-3 text-left font-medium">Reason</th>
              <th className="px-4 py-3 text-left font-medium">Created</th>
              <th className="px-4 py-3 text-right font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <tr key={i} className="border-b">
                  {Array.from({ length: 8 }).map((_, j) => (
                    <td key={j} className="px-4 py-3"><Skeleton className="h-4 w-24" /></td>
                  ))}
                </tr>
              ))
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={8} className="px-4 py-12 text-center text-muted-foreground">
                  No cash advances found.
                </td>
              </tr>
            ) : (
              items.map(ca => {
                const statusInfo = STATUS_LABELS[ca.status] ?? { status: 'Unknown' }
                const progress = ca.amount > 0 ? Math.round(((ca.amount - ca.remainingBalance) / ca.amount) * 100) : 100
                return (
                  <tr key={ca.id} className="border-b hover:bg-muted/30">
                    <td className="px-4 py-3 font-medium">{ca.employeeName}</td>
                    <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(ca.amount)}</td>
                    <td className="px-4 py-3 text-right">
                      <span className="font-mono tabular-nums">{formatPeso(ca.remainingBalance)}</span>
                      {ca.status === CashAdvanceStatus.Active && (
                        <div className="mt-1 h-1.5 w-full rounded-full bg-muted">
                          <div className="h-1.5 rounded-full bg-primary" style={{ width: `${progress}%` }} />
                        </div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(ca.deductionPerPeriod)}</td>
                    <td className="px-4 py-3"><StatusBadge status={statusInfo.status} label={statusInfo.label} /></td>
                    <td className="px-4 py-3 max-w-[200px] truncate text-muted-foreground">{ca.reason ?? '—'}</td>
                    <td className="px-4 py-3 text-muted-foreground">{fmtDate(ca.createdAt)}</td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex justify-end gap-1">
                        {ca.status === CashAdvanceStatus.Pending && (
                          <Button size="sm" variant="outline" onClick={() => setConfirmAction({ id: ca.id, action: 'approve' })}>
                            <Check className="mr-1 h-3 w-3" /> Approve
                          </Button>
                        )}
                        {ca.status === CashAdvanceStatus.Approved && (
                          <Button size="sm" variant="outline" onClick={() => setConfirmAction({ id: ca.id, action: 'disburse' })}>
                            <Banknote className="mr-1 h-3 w-3" /> Disburse
                          </Button>
                        )}
                        {(ca.status === CashAdvanceStatus.Pending || ca.status === CashAdvanceStatus.Approved) && (
                          <Button size="sm" variant="ghost" className="text-destructive" onClick={() => setConfirmAction({ id: ca.id, action: 'cancel' })}>
                            <X className="mr-1 h-3 w-3" /> Cancel
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                )
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Page {page} of {totalPages} ({totalCount} total)
          </p>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}

      <CreateAdvanceDialog open={createOpen} onOpenChange={setCreateOpen} />

      {/* Confirm action dialog */}
      <AlertDialog open={!!confirmAction} onOpenChange={open => !open && setConfirmAction(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {confirmAction?.action === 'approve' && 'Approve Cash Advance?'}
              {confirmAction?.action === 'disburse' && 'Disburse Cash Advance?'}
              {confirmAction?.action === 'cancel' && 'Cancel Cash Advance?'}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {confirmAction?.action === 'approve' && 'This will mark the advance as approved and ready for disbursement.'}
              {confirmAction?.action === 'disburse' && 'This will mark the advance as active. Deductions will begin on the next payroll close.'}
              {confirmAction?.action === 'cancel' && 'This will cancel the advance. This action cannot be undone.'}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Go Back</AlertDialogCancel>
            <AlertDialogAction onClick={handleConfirm}>
              {confirmAction?.action === 'cancel' ? 'Cancel Advance' : 'Confirm'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
