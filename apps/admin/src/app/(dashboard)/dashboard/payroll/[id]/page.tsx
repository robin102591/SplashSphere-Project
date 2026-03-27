'use client'

import { use, useState, useCallback } from 'react'
import { Lock, CheckCheck, AlertTriangle, Pencil, Check, X, Trash2, Plus, FileText, Printer, Banknote, Download } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { StatusBadge } from '@/components/ui/status-badge'
import { PageHeader } from '@/components/ui/page-header'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Sheet, SheetContent, SheetHeader, SheetTitle,
} from '@/components/ui/sheet'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  usePayrollPeriod,
  useClosePayrollPeriod,
  useProcessPayrollPeriod,
  useReleasePayrollPeriod,
  useUpdatePayrollEntry,
  useBulkApplyAdjustment,
  usePayrollTemplates,
  usePayrollEntryDetail,
  useAddAdjustment,
  useUpdateAdjustment,
  useDeleteAdjustment,
  usePayslip,
} from '@/hooks/use-payroll'
import type { AddAdjustmentValues } from '@/hooks/use-payroll'
import { useAuth } from '@clerk/nextjs'
import { PayrollStatus, EmployeeType, AdjustmentType } from '@splashsphere/types'
import type { PayrollEntry, PayrollAdjustment } from '@splashsphere/types'
import { toast } from 'sonner'
import { formatPeso } from '@/lib/format'
import { apiClient } from '@/lib/api-client'

const PAYROLL_STATUS_KEYS: Record<PayrollStatus, string> = {
  [PayrollStatus.Open]: 'Open',
  [PayrollStatus.Closed]: 'Closed',
  [PayrollStatus.Processed]: 'Processed',
  [PayrollStatus.Released]: 'Released',
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
  selected,
  onToggle,
  onNameClick,
}: {
  entry: PayrollEntry
  editable: boolean
  periodId: string
  selected?: boolean
  onToggle?: (id: string) => void
  onNameClick?: (id: string) => void
}) {
  const { mutateAsync: updateEntry } = useUpdatePayrollEntry(periodId)

  const saveNotes = async (notes: string) => {
    await updateEntry({
      entryId: entry.id,
      values: { notes: notes || undefined },
    })
  }

  return (
    <tr className="hover:bg-muted/30">
      {editable && onToggle && (
        <td className="pl-4 pr-0 py-3 w-8">
          <input
            type="checkbox"
            checked={selected}
            onChange={() => onToggle(entry.id)}
            className="h-4 w-4 rounded border-gray-300"
          />
        </td>
      )}
      <td className="px-4 py-3">
        <button
          className="text-left font-medium text-sm hover:underline cursor-pointer"
          onClick={() => onNameClick?.(entry.id)}
        >
          {entry.employeeName}
        </button>
        <p className="text-xs text-muted-foreground">{entry.branchName}</p>
      </td>
      <td className="px-4 py-3 text-center text-sm tabular-nums">
        {entry.employeeTypeSnapshot === EmployeeType.Daily || entry.employeeTypeSnapshot === EmployeeType.Hybrid ? (
          <span>{entry.daysWorked}d</span>
        ) : (
          <span className="text-muted-foreground">—</span>
        )}
      </td>
      <td className="px-4 py-3 text-right text-sm tabular-nums">
        {entry.employeeTypeSnapshot === EmployeeType.Daily || entry.employeeTypeSnapshot === EmployeeType.Hybrid
          ? formatPeso(entry.baseSalary)
          : <span className="text-muted-foreground">—</span>}
      </td>
      <td className="px-4 py-3 text-right text-sm tabular-nums">
        {formatPeso(entry.totalCommissions)}
      </td>
      <td className="px-4 py-3 text-right text-sm tabular-nums text-muted-foreground italic">
        {entry.totalTips > 0 ? formatPeso(entry.totalTips) : '—'}
      </td>
      <td className="px-4 py-3 text-right text-sm tabular-nums">
        {formatPeso(entry.bonuses)}
      </td>
      <td className="px-4 py-3 text-right text-sm tabular-nums">
        {formatPeso(entry.deductions)}
      </td>
      <td className="px-4 py-3 text-right font-semibold text-sm tabular-nums">
        {formatPeso(entry.netPay)}
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

// ── Employee detail sheet ────────────────────────────────────────────────────

// ── Payslip dialog ──────────────────────────────────────────────────────────

function PayslipDialog({
  entryId,
  open,
  onOpenChange,
}: {
  entryId: string | null
  open: boolean
  onOpenChange: (v: boolean) => void
}) {
  const { data: payslip, isLoading } = usePayslip(open ? entryId : null)

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto print:max-w-none print:shadow-none print:border-none">
        {isLoading || !payslip ? (
          <div className="space-y-4 py-8">
            <Skeleton className="h-6 w-48 mx-auto" />
            <Skeleton className="h-4 w-32 mx-auto" />
            <Skeleton className="h-64 w-full" />
          </div>
        ) : (
          <div className="space-y-6 print:space-y-4">
            {/* Header */}
            <div className="text-center border-b pb-4">
              <h2 className="text-xl font-bold">{payslip.tenantName}</h2>
              <p className="text-sm text-muted-foreground">{payslip.branchName}</p>
              <p className="text-sm font-medium mt-2">PAYSLIP</p>
              <p className="text-sm text-muted-foreground">{payslip.periodLabel}</p>
              <p className="text-xs text-muted-foreground">
                {new Date(payslip.periodStart).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' })}
                {' — '}
                {new Date(payslip.periodEnd).toLocaleDateString('en-PH', { month: 'short', day: 'numeric', year: 'numeric' })}
              </p>
            </div>

            {/* Employee info */}
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-muted-foreground">Employee:</span>{' '}
                <span className="font-medium">{payslip.employeeName}</span>
              </div>
              <div>
                <span className="text-muted-foreground">Type:</span>{' '}
                <span className="font-medium">{payslip.employeeType}</span>
              </div>
            </div>

            {/* Earnings */}
            <div>
              <h3 className="text-sm font-semibold border-b pb-1 mb-2">Earnings</h3>
              <table className="w-full text-sm">
                <tbody>
                  <tr className="border-b border-dashed">
                    <td className="py-1.5">Base Salary ({payslip.daysWorked} days)</td>
                    <td className="py-1.5 text-right font-mono tabular-nums">{formatPeso(payslip.baseSalary)}</td>
                  </tr>
                  <tr className="border-b border-dashed">
                    <td className="py-1.5">Commissions ({payslip.commissionTransactions} transactions)</td>
                    <td className="py-1.5 text-right font-mono tabular-nums">{formatPeso(payslip.totalCommissions)}</td>
                  </tr>
                  {payslip.totalTips > 0 && (
                    <tr className="border-b border-dashed text-muted-foreground italic">
                      <td className="py-1.5">Tips (paid out separately)</td>
                      <td className="py-1.5 text-right font-mono tabular-nums">{formatPeso(payslip.totalTips)}</td>
                    </tr>
                  )}
                  <tr className="font-semibold">
                    <td className="py-1.5">Gross Earnings</td>
                    <td className="py-1.5 text-right font-mono tabular-nums">{formatPeso(payslip.grossEarnings)}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            {/* Bonuses */}
            {payslip.bonuses.length > 0 && (
              <div>
                <h3 className="text-sm font-semibold border-b pb-1 mb-2">Bonuses</h3>
                <table className="w-full text-sm">
                  <tbody>
                    {payslip.bonuses.map((b, i) => (
                      <tr key={i} className="border-b border-dashed">
                        <td className="py-1.5">
                          {b.category}
                          {b.notes && <span className="text-xs text-muted-foreground ml-2">({b.notes})</span>}
                        </td>
                        <td className="py-1.5 text-right font-mono tabular-nums">{formatPeso(b.amount)}</td>
                      </tr>
                    ))}
                    <tr className="font-semibold">
                      <td className="py-1.5">Total Bonuses</td>
                      <td className="py-1.5 text-right font-mono tabular-nums">{formatPeso(payslip.totalBonuses)}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            )}

            {/* Deductions */}
            {payslip.deductions.length > 0 && (
              <div>
                <h3 className="text-sm font-semibold border-b pb-1 mb-2">Deductions</h3>
                <table className="w-full text-sm">
                  <tbody>
                    {payslip.deductions.map((d, i) => (
                      <tr key={i} className="border-b border-dashed">
                        <td className="py-1.5">
                          {d.category}
                          {d.notes && <span className="text-xs text-muted-foreground ml-2">({d.notes})</span>}
                        </td>
                        <td className="py-1.5 text-right font-mono tabular-nums text-red-600">({formatPeso(d.amount)})</td>
                      </tr>
                    ))}
                    <tr className="font-semibold">
                      <td className="py-1.5">Total Deductions</td>
                      <td className="py-1.5 text-right font-mono tabular-nums text-red-600">({formatPeso(payslip.totalDeductions)})</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            )}

            {/* Net Pay */}
            <div className="rounded-lg border-2 border-primary/30 bg-primary/5 p-4 text-center">
              <p className="text-sm text-muted-foreground">Net Pay</p>
              <p className="text-2xl font-bold tabular-nums text-primary">{formatPeso(payslip.netPay)}</p>
            </div>

            {/* Footer */}
            <div className="flex items-center justify-between text-xs text-muted-foreground border-t pt-3 print:hidden">
              <span>Generated: {new Date(payslip.generatedAt).toLocaleString('en-PH')}</span>
              <Button size="sm" variant="outline" onClick={() => window.print()}>
                <Printer className="mr-1.5 h-3.5 w-3.5" /> Print
              </Button>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}

function EmployeeDetailSheet({
  entryId,
  periodId,
  editable,
  onClose,
}: {
  entryId: string | null
  periodId: string
  editable: boolean
  onClose: () => void
}) {
  const { data, isLoading } = usePayrollEntryDetail(entryId)
  const { data: templates = [] } = usePayrollTemplates()
  const { mutate: addAdj, isPending: isAdding } = useAddAdjustment(periodId)
  const { mutate: updateAdj } = useUpdateAdjustment(periodId)
  const { mutate: deleteAdj } = useDeleteAdjustment(periodId)

  const [showAddForm, setShowAddForm] = useState(false)
  const [addType, setAddType] = useState<AdjustmentType>(AdjustmentType.Deduction)
  const [addCategory, setAddCategory] = useState('')
  const [addAmount, setAddAmount] = useState('')
  const [addNotes, setAddNotes] = useState('')
  const [addTemplateId, setAddTemplateId] = useState<string | null>(null)

  // Inline-edit state for individual adjustment rows
  const [editingAdjId, setEditingAdjId] = useState<string | null>(null)
  const [editAmount, setEditAmount] = useState('')
  const [editNotes, setEditNotes] = useState('')
  const [payslipOpen, setPayslipOpen] = useState(false)

  const activeTemplates = templates.filter((t) => t.isActive)

  const resetAddForm = () => {
    setShowAddForm(false)
    setAddType(AdjustmentType.Deduction)
    setAddCategory('')
    setAddAmount('')
    setAddNotes('')
    setAddTemplateId(null)
  }

  const handleTemplateSelect = (templateId: string) => {
    const tpl = activeTemplates.find((t) => t.id === templateId)
    if (!tpl) return
    setAddType(tpl.type)
    setAddCategory(tpl.name)
    setAddAmount(String(tpl.defaultAmount))
    setAddTemplateId(tpl.id)
  }

  const handleAddAdjustment = () => {
    if (!entryId) return
    const amt = parseFloat(addAmount)
    if (isNaN(amt) || amt <= 0) { toast.error('Amount must be > 0'); return }
    if (!addCategory.trim()) { toast.error('Category is required'); return }
    addAdj(
      {
        entryId,
        values: {
          type: addType,
          category: addCategory.trim(),
          amount: amt,
          notes: addNotes || undefined,
          templateId: addTemplateId ?? undefined,
        },
      },
      {
        onSuccess: () => { toast.success('Adjustment added'); resetAddForm() },
        onError: () => toast.error('Failed to add adjustment'),
      }
    )
  }

  const startEditAdj = (adj: PayrollAdjustment) => {
    setEditingAdjId(adj.id)
    setEditAmount(String(adj.amount))
    setEditNotes(adj.notes ?? '')
  }

  const saveEditAdj = (adjustmentId: string) => {
    const amt = parseFloat(editAmount)
    if (isNaN(amt) || amt <= 0) { toast.error('Amount must be > 0'); return }
    updateAdj(
      { adjustmentId, values: { amount: amt, notes: editNotes || undefined } },
      {
        onSuccess: () => { toast.success('Adjustment updated'); setEditingAdjId(null) },
        onError: () => toast.error('Failed to update'),
      }
    )
  }

  const handleDeleteAdj = (adjustmentId: string) => {
    deleteAdj(adjustmentId, {
      onSuccess: () => toast.success('Adjustment removed'),
      onError: () => toast.error('Failed to remove'),
    })
  }

  // Detect legacy adjustments (flat values > 0 but no adjustment rows)
  const hasLegacyAdjustments = data
    ? (data.entry.bonuses > 0 || data.entry.deductions > 0) && data.adjustments.length === 0
    : false

  return (
    <Sheet open={!!entryId} onOpenChange={(open) => { if (!open) { onClose(); resetAddForm(); setEditingAdjId(null) } }}>
      <SheetContent className="w-full sm:max-w-3xl overflow-y-auto">
        {isLoading || !data ? (
          <div className="space-y-4 pt-6">
            <Skeleton className="h-6 w-48" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-64 w-full" />
          </div>
        ) : (
          <>
            <SheetHeader>
              <SheetTitle className="text-lg">{data.entry.employeeName}</SheetTitle>
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <span>{data.entry.branchName}</span>
                <Badge variant="outline" className="text-[10px] capitalize">
                  {data.entry.employeeTypeSnapshot === EmployeeType.Daily ? 'Daily' : data.entry.employeeTypeSnapshot === EmployeeType.Hybrid ? 'Hybrid' : 'Commission'}
                </Badge>
              </div>
              <div className="mt-2">
                <Button size="sm" variant="outline" onClick={() => setPayslipOpen(true)}>
                  <FileText className="mr-1.5 h-3.5 w-3.5" /> View Payslip
                </Button>
              </div>
            </SheetHeader>

            <PayslipDialog entryId={entryId} open={payslipOpen} onOpenChange={setPayslipOpen} />

            {/* Summary — 3-col grid for wider layout */}
            <div className="grid grid-cols-3 gap-3 mt-5">
              {[
                { label: 'Base Salary', value: data.entry.baseSalary },
                { label: 'Commissions', value: data.entry.totalCommissions },
                { label: 'Tips (paid)', value: data.entry.totalTips, muted: true },
                { label: `Bonuses (${data.adjustments.filter(a => a.type === AdjustmentType.Bonus).length})`, value: data.entry.bonuses },
                { label: `Deductions (${data.adjustments.filter(a => a.type === AdjustmentType.Deduction).length})`, value: data.entry.deductions },
              ].map(({ label, value, muted }) => (
                <div key={label} className="rounded-lg border px-3 py-2.5">
                  <p className="text-[11px] text-muted-foreground">{label}</p>
                  <p className={`text-sm tabular-nums ${muted ? 'text-muted-foreground italic' : 'font-semibold'}`}>{formatPeso(value)}</p>
                </div>
              ))}
              <div className="rounded-lg border border-primary/30 bg-primary/5 px-3 py-2.5">
                <p className="text-[11px] text-muted-foreground">Net Pay</p>
                <p className="text-lg font-bold tabular-nums text-primary">{formatPeso(data.entry.netPay)}</p>
              </div>
            </div>

            {/* Tabs */}
            <Tabs defaultValue="adjustments" className="mt-5">
              <TabsList variant="line">
                <TabsTrigger value="adjustments">
                  Adjustments ({data.adjustments.length})
                </TabsTrigger>
                <TabsTrigger value="commissions">
                  Commissions ({data.commissionLineItems.length})
                </TabsTrigger>
                <TabsTrigger value="attendance">
                  Attendance ({data.attendanceRecords.length})
                </TabsTrigger>
              </TabsList>

              {/* ── Adjustments tab ──────────────────────────────────── */}
              <TabsContent value="adjustments" className="mt-4 space-y-3">
                {hasLegacyAdjustments && (
                  <div className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-2.5 text-xs text-amber-800">
                    This entry has {formatPeso(data.entry.bonuses)} in bonuses and {formatPeso(data.entry.deductions)} in
                    deductions from before itemised tracking. Add individual adjustments to itemise them.
                  </div>
                )}

                {editable && !showAddForm && (
                  <Button size="sm" variant="outline" onClick={() => setShowAddForm(true)}>
                    <Plus className="mr-1.5 h-3.5 w-3.5" />
                    Add Adjustment
                  </Button>
                )}

                {showAddForm && (
                  <div className="rounded-lg border p-4 space-y-3">
                    <p className="text-sm font-medium">New Adjustment</p>
                    {activeTemplates.length > 0 && (
                      <div className="space-y-1.5">
                        <Label className="text-xs">Quick template</Label>
                        <Select onValueChange={handleTemplateSelect}>
                          <SelectTrigger><SelectValue placeholder="Select template…" /></SelectTrigger>
                          <SelectContent>
                            {activeTemplates.map((t) => (
                              <SelectItem key={t.id} value={t.id}>
                                {t.name} — {t.type === AdjustmentType.Bonus ? 'Bonus' : 'Deduction'} ({formatPeso(t.defaultAmount)})
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    )}
                    <div className="grid grid-cols-3 gap-3">
                      <div className="space-y-1.5">
                        <Label className="text-xs">Type</Label>
                        <Select value={String(addType)} onValueChange={(v) => setAddType(Number(v) as AdjustmentType)}>
                          <SelectTrigger><SelectValue /></SelectTrigger>
                          <SelectContent>
                            <SelectItem value={String(AdjustmentType.Deduction)}>Deduction</SelectItem>
                            <SelectItem value={String(AdjustmentType.Bonus)}>Bonus</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="space-y-1.5">
                        <Label className="text-xs">Category</Label>
                        <Input value={addCategory} onChange={(e) => setAddCategory(e.target.value)} placeholder="e.g. SSS, PhilHealth" />
                      </div>
                      <div className="space-y-1.5">
                        <Label className="text-xs">Amount (₱)</Label>
                        <Input type="number" min="0.01" step="0.01" value={addAmount} onChange={(e) => setAddAmount(e.target.value)} placeholder="0.00" />
                      </div>
                    </div>
                    <div className="space-y-1.5">
                      <Label className="text-xs">Notes (optional)</Label>
                      <Input value={addNotes} onChange={(e) => setAddNotes(e.target.value)} placeholder="Optional notes" />
                    </div>
                    <div className="flex gap-2 pt-1">
                      <Button size="sm" onClick={handleAddAdjustment} disabled={isAdding}>
                        {isAdding ? 'Adding…' : 'Add Adjustment'}
                      </Button>
                      <Button size="sm" variant="ghost" onClick={resetAddForm}>Cancel</Button>
                    </div>
                  </div>
                )}

                {data.adjustments.length === 0 && !hasLegacyAdjustments ? (
                  <p className="py-8 text-center text-sm text-muted-foreground">No adjustments yet</p>
                ) : data.adjustments.length > 0 && (
                  <div className="rounded-lg border overflow-hidden">
                    <table className="w-full text-sm">
                      <thead className="bg-muted/50">
                        <tr>
                          <th className="px-4 py-2.5 text-left text-xs font-medium">Category</th>
                          <th className="px-4 py-2.5 text-left text-xs font-medium">Type</th>
                          <th className="px-4 py-2.5 text-right text-xs font-medium">Amount</th>
                          <th className="px-4 py-2.5 text-left text-xs font-medium">Notes</th>
                          {editable && <th className="px-4 py-2.5 w-20" />}
                        </tr>
                      </thead>
                      <tbody className="divide-y">
                        {data.adjustments.map((adj) => (
                          <tr key={adj.id} className="hover:bg-muted/30">
                            {editingAdjId === adj.id ? (
                              <>
                                <td className="px-4 py-2.5 text-sm">{adj.category}</td>
                                <td className="px-4 py-2.5">
                                  <Badge variant={adj.type === AdjustmentType.Bonus ? 'default' : 'destructive'} className="text-[10px]">
                                    {adj.type === AdjustmentType.Bonus ? 'Bonus' : 'Deduction'}
                                  </Badge>
                                </td>
                                <td className="px-4 py-2.5">
                                  <input type="number" step="0.01" min="0.01" value={editAmount} onChange={(e) => setEditAmount(e.target.value)}
                                    className="w-24 h-7 rounded border border-input bg-background px-2 text-sm tabular-nums" />
                                </td>
                                <td className="px-4 py-2.5">
                                  <input type="text" value={editNotes} onChange={(e) => setEditNotes(e.target.value)}
                                    className="w-full h-7 rounded border border-input bg-background px-2 text-sm" />
                                </td>
                                <td className="px-4 py-2.5">
                                  <div className="flex gap-1">
                                    <button onClick={() => saveEditAdj(adj.id)} className="h-7 w-7 flex items-center justify-center rounded hover:bg-green-100 text-green-700">
                                      <Check className="h-3.5 w-3.5" />
                                    </button>
                                    <button onClick={() => setEditingAdjId(null)} className="h-7 w-7 flex items-center justify-center rounded hover:bg-muted text-muted-foreground">
                                      <X className="h-3.5 w-3.5" />
                                    </button>
                                  </div>
                                </td>
                              </>
                            ) : (
                              <>
                                <td className="px-4 py-2.5 text-sm">
                                  {adj.category}
                                  {adj.templateName && adj.templateName !== adj.category && (
                                    <span className="text-muted-foreground text-xs ml-1.5">via {adj.templateName}</span>
                                  )}
                                </td>
                                <td className="px-4 py-2.5">
                                  <Badge variant={adj.type === AdjustmentType.Bonus ? 'default' : 'destructive'} className="text-[10px]">
                                    {adj.type === AdjustmentType.Bonus ? 'Bonus' : 'Deduction'}
                                  </Badge>
                                </td>
                                <td className="px-4 py-2.5 text-right text-sm tabular-nums font-medium">{formatPeso(adj.amount)}</td>
                                <td className="px-4 py-2.5 text-sm text-muted-foreground">{adj.notes ?? '—'}</td>
                                {editable && (
                                  <td className="px-4 py-2.5">
                                    <div className="flex gap-1">
                                      <button onClick={() => startEditAdj(adj)} className="h-7 w-7 flex items-center justify-center rounded hover:bg-muted text-muted-foreground">
                                        <Pencil className="h-3.5 w-3.5" />
                                      </button>
                                      <button onClick={() => handleDeleteAdj(adj.id)} className="h-7 w-7 flex items-center justify-center rounded hover:bg-destructive/10 text-destructive">
                                        <Trash2 className="h-3.5 w-3.5" />
                                      </button>
                                    </div>
                                  </td>
                                )}
                              </>
                            )}
                          </tr>
                        ))}
                      </tbody>
                      <tfoot className="border-t bg-muted/50">
                        <tr>
                          <td colSpan={2} className="px-4 py-2.5 text-sm font-medium">
                            Net Adjustments
                          </td>
                          <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">
                            {formatPeso(data.adjustments.reduce((s, a) => s + (a.type === AdjustmentType.Bonus ? a.amount : -a.amount), 0))}
                          </td>
                          <td colSpan={editable ? 2 : 1} />
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                )}
              </TabsContent>

              {/* ── Commissions tab ──────────────────────────────────── */}
              <TabsContent value="commissions" className="mt-4">
                {data.commissionLineItems.length === 0 ? (
                  <p className="py-8 text-center text-sm text-muted-foreground">No commissions earned this period</p>
                ) : (
                  <div className="rounded-lg border overflow-hidden">
                    <table className="w-full text-sm">
                      <thead className="bg-muted/50">
                        <tr>
                          <th className="px-4 py-2.5 text-left text-xs font-medium">Transaction</th>
                          <th className="px-4 py-2.5 text-left text-xs font-medium">Service / Package</th>
                          <th className="px-4 py-2.5 text-right text-xs font-medium">Commission</th>
                          <th className="px-4 py-2.5 text-right text-xs font-medium">Date</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y">
                        {data.commissionLineItems.map((item, i) => (
                          <tr key={i} className="hover:bg-muted/30">
                            <td className="px-4 py-2.5 text-sm font-mono">{item.transactionNumber}</td>
                            <td className="px-4 py-2.5 text-sm">{item.serviceName}</td>
                            <td className="px-4 py-2.5 text-right text-sm tabular-nums font-medium">{formatPeso(item.commissionAmount)}</td>
                            <td className="px-4 py-2.5 text-right text-sm text-muted-foreground">
                              {new Date(item.completedAt).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' })}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot className="border-t bg-muted/50">
                        <tr>
                          <td colSpan={2} className="px-4 py-2.5 text-sm font-medium">Total</td>
                          <td className="px-4 py-2.5 text-right text-sm font-semibold tabular-nums">
                            {formatPeso(data.commissionLineItems.reduce((s, c) => s + c.commissionAmount, 0))}
                          </td>
                          <td />
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                )}
              </TabsContent>

              {/* ── Attendance tab ──────────────────────────────────── */}
              <TabsContent value="attendance" className="mt-4">
                {data.attendanceRecords.length === 0 ? (
                  <p className="py-8 text-center text-sm text-muted-foreground">No attendance recorded this period</p>
                ) : (
                  <div className="rounded-lg border overflow-hidden">
                    <table className="w-full text-sm">
                      <thead className="bg-muted/50">
                        <tr>
                          <th className="px-4 py-2.5 text-left text-xs font-medium">Date</th>
                          <th className="px-4 py-2.5 text-right text-xs font-medium">Time In</th>
                          <th className="px-4 py-2.5 text-right text-xs font-medium">Time Out</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y">
                        {data.attendanceRecords.map((rec, i) => (
                          <tr key={i} className="hover:bg-muted/30">
                            <td className="px-4 py-2.5 text-sm">
                              {new Date(rec.date).toLocaleDateString('en-PH', { weekday: 'short', month: 'short', day: 'numeric' })}
                            </td>
                            <td className="px-4 py-2.5 text-right text-sm tabular-nums">
                              {new Date(rec.timeIn).toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit' })}
                            </td>
                            <td className="px-4 py-2.5 text-right text-sm tabular-nums text-muted-foreground">
                              {rec.timeOut
                                ? new Date(rec.timeOut).toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit' })
                                : '—'}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                      <tfoot className="border-t bg-muted/50">
                        <tr>
                          <td colSpan={3} className="px-4 py-2.5 text-sm font-medium">
                            {data.attendanceRecords.length} day{data.attendanceRecords.length !== 1 ? 's' : ''} worked
                          </td>
                        </tr>
                      </tfoot>
                    </table>
                  </div>
                )}
              </TabsContent>
            </Tabs>
          </>
        )}
      </SheetContent>
    </Sheet>
  )
}

// ── Bulk apply dialog ────────────────────────────────────────────────────────

function BulkApplyDialog({
  open,
  onOpenChange,
  selectedCount,
  periodId,
  selectedIds,
  onSuccess,
}: {
  open: boolean
  onOpenChange: (v: boolean) => void
  selectedCount: number
  periodId: string
  selectedIds: string[]
  onSuccess: () => void
}) {
  const { data: templates = [] } = usePayrollTemplates()
  const { mutate: bulkApply, isPending } = useBulkApplyAdjustment(periodId)
  const [type, setType] = useState<AdjustmentType>(AdjustmentType.Deduction)
  const [amount, setAmount] = useState('')
  const [notes, setNotes] = useState('')

  const activeTemplates = templates.filter((t) => t.isActive)

  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null)

  const handleTemplateSelect = (templateId: string) => {
    const tpl = activeTemplates.find((t) => t.id === templateId)
    if (!tpl) return
    setType(tpl.type)
    setAmount(String(tpl.defaultAmount))
    setNotes(tpl.name)
    setSelectedTemplateId(tpl.id)
  }

  const handleApply = () => {
    const num = parseFloat(amount)
    if (isNaN(num) || num <= 0) {
      toast.error('Amount must be greater than 0')
      return
    }
    bulkApply(
      { entryIds: selectedIds, adjustmentType: type, amount: num, notes: notes || undefined, templateId: selectedTemplateId ?? undefined },
      {
        onSuccess: () => {
          toast.success(`Adjustment applied to ${selectedCount} entries`)
          onOpenChange(false)
          onSuccess()
        },
        onError: () => toast.error('Failed to apply adjustment'),
      }
    )
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Apply Bulk Adjustment</DialogTitle>
          <DialogDescription>
            Apply a bonus or deduction to {selectedCount} selected {selectedCount === 1 ? 'entry' : 'entries'}.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-3 py-2">
          {activeTemplates.length > 0 && (
            <div className="space-y-1.5">
              <Label>Quick template</Label>
              <Select onValueChange={handleTemplateSelect}>
                <SelectTrigger><SelectValue placeholder="Select a template…" /></SelectTrigger>
                <SelectContent>
                  {activeTemplates.map((t) => (
                    <SelectItem key={t.id} value={t.id}>
                      {t.name} — {t.type === AdjustmentType.Bonus ? 'Bonus' : 'Deduction'} ({formatPeso(t.defaultAmount)})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}
          <div className="space-y-1.5">
            <Label>Type</Label>
            <Select value={String(type)} onValueChange={(v) => setType(Number(v) as AdjustmentType)}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value={String(AdjustmentType.Deduction)}>Deduction</SelectItem>
                <SelectItem value={String(AdjustmentType.Bonus)}>Bonus</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1.5">
            <Label>Amount (₱)</Label>
            <Input type="number" min="0.01" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} placeholder="0.00" />
          </div>
          <div className="space-y-1.5">
            <Label>Notes (optional)</Label>
            <Input value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="e.g. SSS March 2026" />
          </div>
          <div className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-xs text-amber-800">
            Each application creates a new itemised adjustment row per entry.
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleApply} disabled={isPending || !amount}>
            {isPending ? 'Applying…' : `Apply to ${selectedCount} ${selectedCount === 1 ? 'entry' : 'entries'}`}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function PayrollPeriodDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const [confirmClose, setConfirmClose] = useState(false)
  const [confirmProcess, setConfirmProcess] = useState(false)
  const [confirmRelease, setConfirmRelease] = useState(false)
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [bulkOpen, setBulkOpen] = useState(false)
  const [detailEntryId, setDetailEntryId] = useState<string | null>(null)

  const { getToken } = useAuth()
  const { data: period, isLoading, isError } = usePayrollPeriod(id)
  const { mutate: closePeriod, isPending: isClosing } = useClosePayrollPeriod()
  const { mutate: processPeriod, isPending: isProcessing } = useProcessPayrollPeriod()
  const { mutate: releasePeriod, isPending: isReleasing } = useReleasePayrollPeriod()

  const toggleEntry = useCallback((entryId: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (next.has(entryId)) next.delete(entryId)
      else next.add(entryId)
      return next
    })
  }, [])

  const toggleAll = useCallback(() => {
    const entries = period?.entries ?? []
    setSelectedIds((prev) =>
      prev.size === entries.length ? new Set() : new Set(entries.map((e) => e.id))
    )
  }, [period?.entries])

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

  const [exporting, setExporting] = useState(false)
  const handleExportCsv = async () => {
    setExporting(true)
    try {
      const token = await getToken()
      const apiBase = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'
      const res = await fetch(`${apiBase}/api/v1/payroll/periods/${id}/export/csv`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      })
      if (!res.ok) throw new Error('Export failed')
      const blob = await res.blob()
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = res.headers.get('content-disposition')?.match(/filename="?(.+?)"?$/)?.[1]
        ?? `payroll_${id}.csv`
      a.click()
      URL.revokeObjectURL(url)
      toast.success('CSV exported')
    } catch {
      toast.error('Failed to export CSV')
    } finally {
      setExporting(false)
    }
  }

  const handleRelease = () => {
    releasePeriod(id, {
      onSuccess: () => {
        toast.success('Pay has been released to employees')
        setConfirmRelease(false)
      },
      onError: () => {
        toast.error('Failed to release payroll')
        setConfirmRelease(false)
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
        <PageHeader title="Payroll" back />
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
  const totalTips = entries.reduce((s, e) => s + e.totalTips, 0)
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
      <PageHeader
        title={(() => {
          const s = new Date(period.startDate)
          const e = new Date(period.endDate)
          const daysDiff = Math.round((e.getTime() - s.getTime()) / (1000 * 60 * 60 * 24))
          return daysDiff <= 7 ? `${period.year} — Week ${period.cutOffWeek}` : `${period.year} — Period ${period.cutOffWeek}`
        })()}
        description={`${startDate} – ${endDate} · ${entries.length} ${entries.length === 1 ? 'entry' : 'entries'}`}
        back
        badge={<StatusBadge status={PAYROLL_STATUS_KEYS[period.status]} />}
        actions={
          <>
            {entries.length > 0 && (
              <Button variant="outline" onClick={handleExportCsv} disabled={exporting}>
                <Download className="mr-2 h-3.5 w-3.5" />
                {exporting ? 'Exporting…' : 'Export CSV'}
              </Button>
            )}
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
              <Button onClick={() => setConfirmRelease(true)} variant="default">
                <Banknote className="mr-2 h-3.5 w-3.5" />
                Release Pay
              </Button>
            )}
            {period.status === PayrollStatus.Released && (
              <Badge variant="outline" className="px-3 py-1.5 text-xs">
                <CheckCheck className="mr-1.5 h-3.5 w-3.5 text-green-600" />
                Pay released{period.releasedAt ? ` · ${new Date(period.releasedAt).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' })}` : ''}
              </Badge>
            )}
          </>
        }
      />

      {/* Status callout */}
      {editable && (
        <div className="flex items-start gap-3 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          <Pencil className="h-4 w-4 mt-0.5 shrink-0" />
          <span>
            Period is <strong>Closed</strong> — click an employee name to view details and manage
            itemised adjustments. Use bulk apply to add deductions to multiple entries. Process the payroll when ready to finalise.
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
                {formatPeso(value)}
              </p>
            </div>
          ))}
        </div>
      )}

      {/* Bulk selection toolbar */}
      {editable && selectedIds.size > 0 && (
        <div className="flex items-center gap-3 rounded-lg border bg-muted/50 px-4 py-2">
          <span className="text-sm font-medium">{selectedIds.size} selected</span>
          <Button size="sm" onClick={() => setBulkOpen(true)}>
            Apply Adjustment
          </Button>
          <Button size="sm" variant="ghost" onClick={() => setSelectedIds(new Set())}>
            Clear
          </Button>
        </div>
      )}

      {/* Entries table */}
      {entries.length > 0 && (
        <div className="rounded-lg border overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                {editable && (
                  <th className="pl-4 pr-0 py-3 w-8">
                    <input
                      type="checkbox"
                      checked={selectedIds.size === entries.length && entries.length > 0}
                      onChange={toggleAll}
                      className="h-4 w-4 rounded border-gray-300"
                    />
                  </th>
                )}
                <th className="px-4 py-3 text-left font-medium">Employee</th>
                <th className="px-4 py-3 text-center font-medium">Days</th>
                <th className="px-4 py-3 text-right font-medium">Base Salary</th>
                <th className="px-4 py-3 text-right font-medium">Commissions</th>
                <th className="px-4 py-3 text-right font-medium text-muted-foreground">Tips (paid)</th>
                <th className="px-4 py-3 text-right font-medium">Bonuses</th>
                <th className="px-4 py-3 text-right font-medium">Deductions</th>
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
                  selected={selectedIds.has(entry.id)}
                  onToggle={toggleEntry}
                  onNameClick={setDetailEntryId}
                />
              ))}
            </tbody>
            <tfoot className="border-t bg-muted/50">
              <tr>
                <td colSpan={editable ? 4 : 3} className="px-4 py-3 font-medium">
                  Totals ({entries.length} employees)
                </td>
                <td className="px-4 py-3 text-right font-medium tabular-nums">
                  {formatPeso(totalCommissions)}
                </td>
                <td className="px-4 py-3 text-right tabular-nums text-muted-foreground italic">
                  {totalTips > 0 ? formatPeso(totalTips) : '—'}
                </td>
                <td className="px-4 py-3 text-right font-medium tabular-nums">
                  {formatPeso(totalBonuses)}
                </td>
                <td className="px-4 py-3 text-right font-medium tabular-nums">
                  {formatPeso(totalDeductions)}
                </td>
                <td className="px-4 py-3 text-right font-bold tabular-nums">
                  {formatPeso(totalNetPay)}
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

      {/* Employee detail sheet */}
      <EmployeeDetailSheet entryId={detailEntryId} periodId={id} editable={editable} onClose={() => setDetailEntryId(null)} />

      {/* Bulk apply dialog */}
      <BulkApplyDialog
        open={bulkOpen}
        onOpenChange={setBulkOpen}
        selectedCount={selectedIds.size}
        periodId={id}
        selectedIds={Array.from(selectedIds)}
        onSuccess={() => setSelectedIds(new Set())}
      />

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

      {/* Release confirmation */}
      <Dialog open={confirmRelease} onOpenChange={setConfirmRelease}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Release Pay?</DialogTitle>
            <DialogDescription>
              This will mark the payroll as released — confirming that pay has been disbursed to all
              {' '}{entries.length} employees. This cannot be undone.
            </DialogDescription>
          </DialogHeader>
          {period.scheduledReleaseDate && (
            <div className="rounded-lg border bg-muted/50 px-4 py-3 text-sm flex items-start gap-2">
              <Banknote className="h-4 w-4 mt-0.5 shrink-0 text-muted-foreground" />
              <span>
                Scheduled release date:{' '}
                <strong>
                  {new Date(period.scheduledReleaseDate).toLocaleDateString('en-PH', { month: 'long', day: 'numeric', year: 'numeric' })}
                </strong>
              </span>
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setConfirmRelease(false)}
              disabled={isReleasing}
            >
              Cancel
            </Button>
            <Button onClick={handleRelease} disabled={isReleasing}>
              {isReleasing ? 'Releasing…' : 'Release Pay'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
