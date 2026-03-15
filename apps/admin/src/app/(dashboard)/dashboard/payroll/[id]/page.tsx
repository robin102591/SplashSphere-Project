'use client'

import { use, useState, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { ArrowLeft, Lock, CheckCheck, AlertTriangle, Pencil, Check, X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  usePayrollPeriod,
  useClosePayrollPeriod,
  useProcessPayrollPeriod,
  useUpdatePayrollEntry,
} from '@/hooks/use-payroll'
import { PayrollStatus, EmployeeType } from '@splashsphere/types'
import type { PayrollEntry } from '@splashsphere/types'
import { toast } from 'sonner'

const php = new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' })

// ── Status badge ──────────────────────────────────────────────────────────────

function StatusBadge({ status }: { status: PayrollStatus }) {
  if (status === PayrollStatus.Open)
    return <Badge className="bg-blue-500/15 text-blue-700 border-blue-200">Open</Badge>
  if (status === PayrollStatus.Closed)
    return <Badge className="bg-amber-500/15 text-amber-700 border-amber-200">Closed</Badge>
  return <Badge className="bg-green-500/15 text-green-700 border-green-200">Processed</Badge>
}

// ── Inline-editable cell ──────────────────────────────────────────────────────

function EditableCell({
  value,
  onSave,
}: {
  value: number
  onSave: (next: number) => Promise<void>
}) {
  const [editing, setEditing] = useState(false)
  const [draft, setDraft] = useState(String(value))
  const [saving, setSaving] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  const startEdit = () => {
    setDraft(String(value))
    setEditing(true)
    setTimeout(() => inputRef.current?.select(), 0)
  }

  const cancel = () => {
    setEditing(false)
    setDraft(String(value))
  }

  const save = async () => {
    const num = parseFloat(draft)
    if (isNaN(num) || num < 0) {
      toast.error('Enter a valid non-negative amount')
      return
    }
    setSaving(true)
    try {
      await onSave(num)
      setEditing(false)
    } finally {
      setSaving(false)
    }
  }

  if (editing) {
    return (
      <div className="flex items-center gap-1">
        <input
          ref={inputRef}
          type="number"
          step="0.01"
          min="0"
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') save()
            if (e.key === 'Escape') cancel()
          }}
          className="w-24 h-7 rounded border border-input bg-background px-2 text-sm tabular-nums"
          disabled={saving}
        />
        <button
          onClick={save}
          disabled={saving}
          className="h-7 w-7 flex items-center justify-center rounded hover:bg-green-100 text-green-700 disabled:opacity-50"
        >
          <Check className="h-3.5 w-3.5" />
        </button>
        <button
          onClick={cancel}
          disabled={saving}
          className="h-7 w-7 flex items-center justify-center rounded hover:bg-muted text-muted-foreground disabled:opacity-50"
        >
          <X className="h-3.5 w-3.5" />
        </button>
      </div>
    )
  }

  return (
    <div className="flex items-center gap-1 group">
      <span className="tabular-nums">{php.format(value)}</span>
      <button
        onClick={startEdit}
        className="opacity-0 group-hover:opacity-100 h-6 w-6 flex items-center justify-center rounded hover:bg-muted transition-opacity"
      >
        <Pencil className="h-3 w-3 text-muted-foreground" />
      </button>
    </div>
  )
}

// ── Notes cell ────────────────────────────────────────────────────────────────

function NotesCell({
  value,
  onSave,
}: {
  value: string | null
  onSave: (next: string) => Promise<void>
}) {
  const [editing, setEditing] = useState(false)
  const [draft, setDraft] = useState(value ?? '')
  const [saving, setSaving] = useState(false)

  const save = async () => {
    setSaving(true)
    try {
      await onSave(draft)
      setEditing(false)
    } finally {
      setSaving(false)
    }
  }

  if (editing) {
    return (
      <div className="flex items-center gap-1">
        <input
          type="text"
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') save()
            if (e.key === 'Escape') setEditing(false)
          }}
          autoFocus
          className="w-36 h-7 rounded border border-input bg-background px-2 text-sm"
          disabled={saving}
        />
        <button
          onClick={save}
          disabled={saving}
          className="h-7 w-7 flex items-center justify-center rounded hover:bg-green-100 text-green-700 disabled:opacity-50"
        >
          <Check className="h-3.5 w-3.5" />
        </button>
        <button
          onClick={() => setEditing(false)}
          disabled={saving}
          className="h-7 w-7 flex items-center justify-center rounded hover:bg-muted text-muted-foreground"
        >
          <X className="h-3.5 w-3.5" />
        </button>
      </div>
    )
  }

  return (
    <div className="flex items-center gap-1 group">
      <span className="text-xs text-muted-foreground">
        {value ?? <span className="italic">—</span>}
      </span>
      <button
        onClick={() => setEditing(true)}
        className="opacity-0 group-hover:opacity-100 h-6 w-6 flex items-center justify-center rounded hover:bg-muted transition-opacity"
      >
        <Pencil className="h-3 w-3 text-muted-foreground" />
      </button>
    </div>
  )
}

// ── Entry row ─────────────────────────────────────────────────────────────────

function EntryRow({
  entry,
  editable,
  periodId,
}: {
  entry: PayrollEntry
  editable: boolean
  periodId: string
}) {
  const { mutateAsync: updateEntry } = useUpdatePayrollEntry(periodId)

  const saveField = async (field: 'bonuses' | 'deductions', val: number) => {
    await updateEntry({
      entryId: entry.id,
      values: {
        bonuses: field === 'bonuses' ? val : entry.bonuses,
        deductions: field === 'deductions' ? val : entry.deductions,
        notes: entry.notes ?? undefined,
      },
    })
  }

  const saveNotes = async (notes: string) => {
    await updateEntry({
      entryId: entry.id,
      values: {
        bonuses: entry.bonuses,
        deductions: entry.deductions,
        notes: notes || undefined,
      },
    })
  }

  return (
    <tr className="hover:bg-muted/30">
      <td className="px-4 py-3">
        <p className="font-medium text-sm">{entry.employeeName}</p>
        <p className="text-xs text-muted-foreground">{entry.branchName}</p>
      </td>
      <td className="px-4 py-3 text-center text-sm tabular-nums">
        {entry.employeeTypeSnapshot === EmployeeType.Daily ? (
          <span>{entry.daysWorked}d</span>
        ) : (
          <span className="text-muted-foreground">—</span>
        )}
      </td>
      <td className="px-4 py-3 text-right text-sm tabular-nums">
        {entry.employeeTypeSnapshot === EmployeeType.Daily
          ? php.format(entry.baseSalary)
          : <span className="text-muted-foreground">—</span>}
      </td>
      <td className="px-4 py-3 text-right text-sm tabular-nums">
        {php.format(entry.totalCommissions)}
      </td>
      <td className="px-4 py-3 text-right text-sm">
        {editable ? (
          <EditableCell value={entry.bonuses} onSave={(v) => saveField('bonuses', v)} />
        ) : (
          <span className="tabular-nums">{php.format(entry.bonuses)}</span>
        )}
      </td>
      <td className="px-4 py-3 text-right text-sm">
        {editable ? (
          <EditableCell value={entry.deductions} onSave={(v) => saveField('deductions', v)} />
        ) : (
          <span className="tabular-nums">{php.format(entry.deductions)}</span>
        )}
      </td>
      <td className="px-4 py-3 text-right font-semibold text-sm tabular-nums">
        {php.format(entry.netPay)}
      </td>
      <td className="px-4 py-3 text-sm max-w-[140px]">
        {editable ? (
          <NotesCell value={entry.notes} onSave={saveNotes} />
        ) : (
          <span className="text-xs text-muted-foreground">{entry.notes ?? '—'}</span>
        )}
      </td>
    </tr>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function PayrollPeriodDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const router = useRouter()
  const [confirmClose, setConfirmClose] = useState(false)
  const [confirmProcess, setConfirmProcess] = useState(false)

  const { data: period, isLoading, isError } = usePayrollPeriod(id)
  const { mutate: closePeriod, isPending: isClosing } = useClosePayrollPeriod()
  const { mutate: processPeriod, isPending: isProcessing } = useProcessPayrollPeriod()

  const handleClose = () => {
    closePeriod(id, {
      onSuccess: () => {
        toast.success('Payroll period closed — entries are ready for review')
        setConfirmClose(false)
      },
      onError: () => {
        toast.error('Failed to close payroll period')
        setConfirmClose(false)
      },
    })
  }

  const handleProcess = () => {
    processPeriod(id, {
      onSuccess: () => {
        toast.success('Payroll period processed — entries are now immutable')
        setConfirmProcess(false)
      },
      onError: () => {
        toast.error('Failed to process payroll period')
        setConfirmProcess(false)
      },
    })
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-96" />
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  if (isError || !period) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" size="sm" onClick={() => router.back()}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back
        </Button>
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Payroll period not found or failed to load.
        </div>
      </div>
    )
  }

  const entries = [...period.entries]
  const editable = period.status === PayrollStatus.Closed
  const totalNetPay = entries.reduce((s, e) => s + e.netPay, 0)
  const totalCommissions = entries.reduce((s, e) => s + e.totalCommissions, 0)
  const totalBonuses = entries.reduce((s, e) => s + e.bonuses, 0)
  const totalDeductions = entries.reduce((s, e) => s + e.deductions, 0)

  const startDate = new Date(period.startDate).toLocaleDateString('en-PH', {
    month: 'long',
    day: 'numeric',
  })
  const endDate = new Date(period.endDate).toLocaleDateString('en-PH', {
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  })

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold tracking-tight">
                {period.year} — Week {period.cutOffWeek}
              </h1>
              <StatusBadge status={period.status} />
            </div>
            <p className="text-sm text-muted-foreground mt-0.5">
              {startDate} – {endDate} · {entries.length}{' '}
              {entries.length === 1 ? 'entry' : 'entries'}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2 shrink-0">
          {period.status === PayrollStatus.Open && (
            <Button onClick={() => setConfirmClose(true)}>
              <Lock className="mr-2 h-3.5 w-3.5" />
              Close Period
            </Button>
          )}
          {period.status === PayrollStatus.Closed && (
            <Button onClick={() => setConfirmProcess(true)} variant="default">
              <CheckCheck className="mr-2 h-3.5 w-3.5" />
              Process Payroll
            </Button>
          )}
          {period.status === PayrollStatus.Processed && (
            <Badge variant="outline" className="px-3 py-1.5 text-xs">
              <CheckCheck className="mr-1.5 h-3.5 w-3.5 text-green-600" />
              Payroll finalised
            </Badge>
          )}
        </div>
      </div>

      {/* Status callout */}
      {editable && (
        <div className="flex items-start gap-3 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          <Pencil className="h-4 w-4 mt-0.5 shrink-0" />
          <span>
            Period is <strong>Closed</strong> — hover any Bonuses, Deductions, or Notes cell to
            edit inline. Changes are saved immediately. Process the payroll when ready to finalise.
          </span>
        </div>
      )}

      {period.status === PayrollStatus.Open && entries.length === 0 && (
        <div className="flex items-start gap-3 rounded-lg border border-blue-200 bg-blue-50 px-4 py-3 text-sm text-blue-800">
          <AlertTriangle className="h-4 w-4 mt-0.5 shrink-0" />
          <span>
            No entries yet. Entries are generated when you <strong>Close</strong> this period.
          </span>
        </div>
      )}

      {/* Summary cards */}
      {entries.length > 0 && (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          {[
            { label: 'Total Commissions', value: totalCommissions },
            { label: 'Total Bonuses', value: totalBonuses },
            { label: 'Total Deductions', value: totalDeductions },
            { label: 'Total Net Pay', value: totalNetPay, highlight: true },
          ].map(({ label, value, highlight }) => (
            <div
              key={label}
              className={`rounded-lg border px-4 py-3 ${highlight ? 'border-primary/30 bg-primary/5' : ''}`}
            >
              <p className="text-xs text-muted-foreground">{label}</p>
              <p className={`text-lg font-bold tabular-nums mt-0.5 ${highlight ? 'text-primary' : ''}`}>
                {php.format(value)}
              </p>
            </div>
          ))}
        </div>
      )}

      {/* Entries table */}
      {entries.length > 0 && (
        <div className="rounded-lg border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left font-medium">Employee</th>
                <th className="px-4 py-3 text-center font-medium">Days</th>
                <th className="px-4 py-3 text-right font-medium">Base Salary</th>
                <th className="px-4 py-3 text-right font-medium">Commissions</th>
                <th className="px-4 py-3 text-right font-medium">
                  Bonuses
                  {editable && <span className="ml-1 text-xs font-normal text-muted-foreground">(editable)</span>}
                </th>
                <th className="px-4 py-3 text-right font-medium">
                  Deductions
                  {editable && <span className="ml-1 text-xs font-normal text-muted-foreground">(editable)</span>}
                </th>
                <th className="px-4 py-3 text-right font-medium">Net Pay</th>
                <th className="px-4 py-3 text-left font-medium">Notes</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {entries.map((entry) => (
                <EntryRow
                  key={entry.id}
                  entry={entry}
                  editable={editable}
                  periodId={id}
                />
              ))}
            </tbody>
            <tfoot className="border-t bg-muted/50">
              <tr>
                <td colSpan={3} className="px-4 py-3 font-medium">
                  Totals ({entries.length} employees)
                </td>
                <td className="px-4 py-3 text-right font-medium tabular-nums">
                  {php.format(totalCommissions)}
                </td>
                <td className="px-4 py-3 text-right font-medium tabular-nums">
                  {php.format(totalBonuses)}
                </td>
                <td className="px-4 py-3 text-right font-medium tabular-nums">
                  {php.format(totalDeductions)}
                </td>
                <td className="px-4 py-3 text-right font-bold tabular-nums">
                  {php.format(totalNetPay)}
                </td>
                <td />
              </tr>
            </tfoot>
          </table>
        </div>
      )}

      {/* Close confirmation */}
      <Dialog open={confirmClose} onOpenChange={setConfirmClose}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Close Payroll Period?</DialogTitle>
            <DialogDescription>
              This will tally attendance and commissions for all active employees and generate
              payroll entries. You can still edit bonuses and deductions after closing.
            </DialogDescription>
          </DialogHeader>
          <div className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            <strong>Period:</strong> {period.year} Week {period.cutOffWeek} ({startDate} –{' '}
            {endDate})
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmClose(false)} disabled={isClosing}>
              Cancel
            </Button>
            <Button onClick={handleClose} disabled={isClosing}>
              {isClosing ? 'Closing…' : 'Close Period'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Process confirmation */}
      <Dialog open={confirmProcess} onOpenChange={setConfirmProcess}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Process Payroll?</DialogTitle>
            <DialogDescription>
              This will finalise the payroll period. No further adjustments will be allowed after
              this action. This cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <div className="rounded-lg border border-destructive/30 bg-destructive/5 px-4 py-3 text-sm text-destructive flex items-start gap-2">
            <AlertTriangle className="h-4 w-4 mt-0.5 shrink-0" />
            <span>
              <strong>Irreversible.</strong> Once processed, all {entries.length} entries are
              locked permanently.
            </span>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setConfirmProcess(false)}
              disabled={isProcessing}
            >
              Cancel
            </Button>
            <Button onClick={handleProcess} disabled={isProcessing}>
              {isProcessing ? 'Processing…' : 'Process Payroll'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
