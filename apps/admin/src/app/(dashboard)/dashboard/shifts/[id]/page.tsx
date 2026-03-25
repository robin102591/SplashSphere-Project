'use client'

import { useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import {
  Card, CardContent, CardHeader, CardTitle,
} from '@/components/ui/card'
import { StatusBadge } from '@/components/ui/status-badge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import { Label } from '@/components/ui/label'
import { Separator } from '@/components/ui/separator'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
  AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { toast } from 'sonner'
import { ArrowLeft, CheckCircle2, AlertTriangle, XCircle, Printer, RotateCcw } from 'lucide-react'
import Link from 'next/link'
import { useShiftById, useShiftReport, useReviewShift, useReopenShift } from '@/hooks/use-shifts'
import { ShiftStatus, ReviewStatus, PaymentMethod, CashMovementType } from '@splashsphere/types'
import { cn } from '@/lib/utils'
import { formatPeso } from '@/lib/format'

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-PH', { hour: 'numeric', minute: '2-digit', hour12: true })
}

function fmtDate(dateStr: string) {
  return new Date(dateStr + 'T00:00:00').toLocaleDateString('en-PH', {
    year: 'numeric', month: 'long', day: 'numeric',
  })
}

function fmtDuration(openedAt: string, closedAt: string | null) {
  if (!closedAt) return 'Ongoing'
  const diffMs = new Date(closedAt).getTime() - new Date(openedAt).getTime()
  const h = Math.floor(diffMs / 3_600_000)
  const m = Math.floor((diffMs % 3_600_000) / 60_000)
  return `${h}h ${m}m`
}

const METHOD_LABEL: Record<number, string> = {
  [PaymentMethod.Cash]:         'Cash',
  [PaymentMethod.GCash]:        'GCash',
  [PaymentMethod.CreditCard]:   'Credit Card',
  [PaymentMethod.DebitCard]:    'Debit Card',
  [PaymentMethod.BankTransfer]: 'Bank Transfer',
}

const REVIEW_STATUS_KEYS: Record<ReviewStatus, string> = {
  [ReviewStatus.Pending]: 'Pending',
  [ReviewStatus.Approved]: 'Approved',
  [ReviewStatus.Flagged]: 'Flagged',
}

function VarianceDisplay({ variance }: { variance: number }) {
  const abs = Math.abs(variance)
  const color = abs <= 50 ? 'text-green-700' : abs <= 200 ? 'text-amber-600' : 'text-red-600'
  const label = abs <= 50 ? 'BALANCED' : variance > 0 ? 'OVER' : 'SHORT'
  return (
    <span className={cn('font-mono font-bold text-lg', color)}>
      {variance >= 0 ? '+' : ''}{formatPeso(variance)} · {label}
    </span>
  )
}

function ReportRow({ label, value, bold, valueClass }: { label: string; value: string; bold?: boolean; valueClass?: string }) {
  return (
    <div className="flex items-center justify-between py-1.5 text-sm">
      <span className="text-muted-foreground">{label}</span>
      <span className={cn(bold ? 'font-bold' : 'font-medium', valueClass)}>{value}</span>
    </div>
  )
}

export default function ShiftDetailPage() {
  const { id } = useParams<{ id: string }>()
  const router = useRouter()

  const { data: shift, isLoading: shiftLoading } = useShiftById(id)
  const { data: report } = useShiftReport(id)

  const [approveDialogOpen, setApproveDialogOpen] = useState(false)
  const [flagDialogOpen, setFlagDialogOpen]       = useState(false)
  const [reopenDialogOpen, setReopenDialogOpen]   = useState(false)
  const [flagNotes, setFlagNotes]                 = useState('')

  const reviewMutation  = useReviewShift()
  const reopenMutation  = useReopenShift()

  const handleApprove = async () => {
    try {
      await reviewMutation.mutateAsync({ shiftId: id, newReviewStatus: ReviewStatus.Approved })
      toast.success('Shift approved.')
      setApproveDialogOpen(false)
    } catch {
      toast.error('Failed to approve shift.')
    }
  }

  const handleFlag = async () => {
    if (!flagNotes.trim()) {
      toast.error('Notes are required when flagging a shift.')
      return
    }
    try {
      await reviewMutation.mutateAsync({ shiftId: id, newReviewStatus: ReviewStatus.Flagged, notes: flagNotes })
      toast.success('Shift flagged for investigation.')
      setFlagDialogOpen(false)
      setFlagNotes('')
    } catch {
      toast.error('Failed to flag shift.')
    }
  }

  const handleReopen = async () => {
    try {
      await reopenMutation.mutateAsync(id)
      toast.success('Shift reopened.')
      setReopenDialogOpen(false)
    } catch {
      toast.error('Failed to reopen shift.')
    }
  }

  if (shiftLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  if (!shift) {
    return (
      <div className="text-center py-16">
        <p className="text-muted-foreground">Shift not found.</p>
        <Link href="/dashboard/shifts" className="text-primary underline mt-2 block">Back to shifts</Link>
      </div>
    )
  }

  const isClosed = shift.status === ShiftStatus.Closed
  const canReview = isClosed && shift.reviewStatus === ReviewStatus.Pending
  const canReopen = isClosed && shift.reviewStatus === ReviewStatus.Pending
  const absVariance = Math.abs(shift.variance)

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Shift Report</h1>
            <p className="text-muted-foreground">
              {shift.cashierName} · {shift.branchName} · {fmtDate(shift.shiftDate)}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <StatusBadge status={REVIEW_STATUS_KEYS[shift.reviewStatus]} />
          <Button variant="outline" size="sm" onClick={() => window.print()} className="print:hidden">
            <Printer className="h-4 w-4 mr-1.5" />
            Print
          </Button>
        </div>
      </div>

      {/* Manager review actions */}
      {canReview && (
        <Card className="border-amber-200 bg-amber-50/50 print:hidden">
          <CardContent className="pt-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="font-semibold text-amber-900">Pending Review</p>
                <p className="text-sm text-amber-700">
                  Variance: <strong>{shift.variance >= 0 ? '+' : ''}{formatPeso(shift.variance)}</strong>
                  {absVariance > 200 && ' — Large variance, investigation recommended.'}
                  {absVariance > 50 && absVariance <= 200 && ' — Within moderate range.'}
                  {absVariance <= 50 && ' — Acceptable.'}
                </p>
              </div>
              <div className="flex gap-2">
                {canReopen && (
                  <Button variant="outline" size="sm" onClick={() => setReopenDialogOpen(true)}>
                    <RotateCcw className="h-3.5 w-3.5 mr-1.5" />
                    Reopen
                  </Button>
                )}
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setFlagDialogOpen(true)}
                  className="border-red-200 text-red-700 hover:bg-red-50"
                >
                  <XCircle className="h-3.5 w-3.5 mr-1.5" />
                  Flag
                </Button>
                <Button
                  size="sm"
                  onClick={() => setApproveDialogOpen(true)}
                  className="bg-green-600 hover:bg-green-700 text-white"
                >
                  <CheckCircle2 className="h-3.5 w-3.5 mr-1.5" />
                  Approve
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Already reviewed */}
      {isClosed && !canReview && (
        <Card className={cn(
          'print:hidden',
          shift.reviewStatus === ReviewStatus.Approved && 'border-green-200 bg-green-50/50',
          shift.reviewStatus === ReviewStatus.Flagged && 'border-red-200 bg-red-50/50',
        )}>
          <CardContent className="pt-4">
            <div className="flex items-center justify-between">
              <div>
                <StatusBadge status={REVIEW_STATUS_KEYS[shift.reviewStatus]} />
                {shift.reviewedByName && (
                  <p className="text-sm text-muted-foreground mt-1">
                    By {shift.reviewedByName}
                    {shift.reviewedAt && ` · ${fmtTime(shift.reviewedAt)}`}
                  </p>
                )}
                {shift.reviewNotes && (
                  <p className="text-sm mt-1 italic">"{shift.reviewNotes}"</p>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Report grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">

        {/* Left column */}
        <div className="space-y-4">

          {/* Shift info */}
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium">Shift Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-0">
              <ReportRow label="Branch"   value={shift.branchName} />
              <ReportRow label="Cashier"  value={shift.cashierName} />
              <ReportRow label="Date"     value={fmtDate(shift.shiftDate)} />
              <ReportRow label="Opened"   value={fmtTime(shift.openedAt)} />
              <ReportRow label="Closed"   value={shift.closedAt ? fmtTime(shift.closedAt) : '—'} />
              <ReportRow label="Duration" value={fmtDuration(shift.openedAt, shift.closedAt)} />
            </CardContent>
          </Card>

          {/* Transaction summary */}
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium">Transaction Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-0">
              <ReportRow label="Total Transactions" value={String(shift.totalTransactionCount)} bold />
              <ReportRow label="Total Revenue"      value={formatPeso(shift.totalRevenue)} bold />
              <ReportRow label="Total Discounts"    value={`−${formatPeso(shift.totalDiscounts)}`} valueClass="text-red-600" />
              <ReportRow label="Net Revenue"        value={formatPeso(shift.totalRevenue - shift.totalDiscounts)} bold />
              <ReportRow label="Total Commissions"  value={formatPeso(shift.totalCommissions)} />
            </CardContent>
          </Card>

          {/* Payment breakdown */}
          {shift.paymentSummaries.length > 0 && (
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium">Payment Method Breakdown</CardTitle>
              </CardHeader>
              <CardContent className="space-y-0">
                {shift.paymentSummaries.map(ps => (
                  <div key={ps.method} className="flex items-center justify-between py-1.5 text-sm">
                    <span className="text-muted-foreground">{METHOD_LABEL[ps.method] ?? 'Other'}</span>
                    <div className="text-right">
                      <span className="font-semibold">{formatPeso(ps.totalAmount)}</span>
                      <span className="text-muted-foreground text-xs ml-2">{ps.transactionCount} txns</span>
                    </div>
                  </div>
                ))}
                <Separator className="my-2" />
                <div className="flex justify-between text-sm font-bold">
                  <span>Total</span>
                  <span>{formatPeso(shift.totalRevenue)}</span>
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        {/* Right column */}
        <div className="space-y-4">

          {/* Cash flow */}
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium">Cash Flow</CardTitle>
            </CardHeader>
            <CardContent className="space-y-0">
              <ReportRow label="Opening Cash Fund"        value={formatPeso(shift.openingCashFund)} />
              <ReportRow label="(+) Cash Payments"        value={formatPeso(shift.totalCashPayments)} valueClass="text-green-700" />
              <ReportRow label="(+) Manual Cash-In"       value={formatPeso(shift.totalCashIn)} valueClass="text-green-700" />
              <ReportRow label="(−) Manual Cash-Out"      value={`−${formatPeso(shift.totalCashOut)}`} valueClass="text-red-600" />
              <Separator className="my-2" />
              <ReportRow label="Expected Cash in Drawer"  value={formatPeso(shift.expectedCashInDrawer)} bold />
              <ReportRow label="Actual Cash Counted"      value={formatPeso(shift.actualCashInDrawer)} bold />
              <div className="flex items-center justify-between pt-2 border-t mt-2">
                <span className="text-sm font-semibold">Variance</span>
                <VarianceDisplay variance={shift.variance} />
              </div>
            </CardContent>
          </Card>

          {/* Denomination count */}
          {shift.denominations.length > 0 && (
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium">Cash Count</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-1">
                  {shift.denominations.map(d => (
                    <div key={d.denominationValue} className="flex items-center justify-between text-sm">
                      <span className="font-mono text-muted-foreground">
                        ₱{d.denominationValue >= 1 ? d.denominationValue.toLocaleString() : '0.25'} × {d.count}
                      </span>
                      <span className="font-mono font-semibold">{formatPeso(d.subtotal)}</span>
                    </div>
                  ))}
                  <Separator className="my-2" />
                  <div className="flex justify-between text-sm font-bold">
                    <span>Total Counted</span>
                    <span className="font-mono">{formatPeso(shift.actualCashInDrawer)}</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Cash movements */}
          {shift.cashMovements.length > 0 && (
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium">Cash Movements</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {shift.cashMovements.map(m => (
                    <div key={m.id} className="flex items-start justify-between text-sm">
                      <div className="min-w-0">
                        <p className="font-medium">{m.reason}</p>
                        {m.reference && <p className="text-xs text-muted-foreground">{m.reference}</p>}
                        <p className="text-xs text-muted-foreground">{fmtTime(m.movementTime)}</p>
                      </div>
                      <span className={cn(
                        'font-mono font-semibold shrink-0 ml-2',
                        m.type === CashMovementType.CashIn ? 'text-green-700' : 'text-red-600'
                      )}>
                        {m.type === CashMovementType.CashIn ? '+' : '−'}{formatPeso(m.amount)}
                      </span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>

      {/* Top services & employees (from report) */}
      {report && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {report.topServices.length > 0 && (
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium">Top Services</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {report.topServices.map((svc, i) => (
                    <div key={svc.serviceName} className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">{i + 1}. {svc.serviceName}</span>
                      <div className="text-right">
                        <span className="font-semibold">{formatPeso(svc.totalAmount)}</span>
                        <span className="text-xs text-muted-foreground ml-2">{svc.transactionCount} txns</span>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}

          {report.topEmployees.length > 0 && (
            <Card>
              <CardHeader className="pb-2">
                <CardTitle className="text-sm font-medium">Top Employees by Commission</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {report.topEmployees.map((emp, i) => (
                    <div key={emp.employeeId} className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">{i + 1}. {emp.employeeName}</span>
                      <div className="text-right">
                        <span className="font-semibold">{formatPeso(emp.totalCommission)}</span>
                        <span className="text-xs text-muted-foreground ml-2">{emp.serviceCount} svc</span>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      )}

      {/* Approve dialog */}
      <AlertDialog open={approveDialogOpen} onOpenChange={setApproveDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Approve Shift</AlertDialogTitle>
            <AlertDialogDescription>
              Approve this shift for {shift.cashierName} with variance of {formatPeso(shift.variance)}?
              This action acknowledges the variance is acceptable.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleApprove}
              className="bg-green-600 hover:bg-green-700"
              disabled={reviewMutation.isPending}
            >
              Approve
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Flag dialog */}
      <AlertDialog open={flagDialogOpen} onOpenChange={setFlagDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Flag Shift for Investigation</AlertDialogTitle>
            <AlertDialogDescription>
              Flagging this shift requires investigation notes. The cashier and branch manager will be notified.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <div className="px-6 pb-2">
            <Label htmlFor="flag-notes">Notes (required)</Label>
            <Textarea
              id="flag-notes"
              className="mt-1.5"
              rows={3}
              placeholder="Describe the reason for flagging this shift…"
              value={flagNotes}
              onChange={e => setFlagNotes(e.target.value)}
            />
          </div>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleFlag}
              className="bg-red-600 hover:bg-red-700"
              disabled={reviewMutation.isPending || !flagNotes.trim()}
            >
              Flag Shift
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Reopen dialog */}
      <AlertDialog open={reopenDialogOpen} onOpenChange={setReopenDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Reopen Shift</AlertDialogTitle>
            <AlertDialogDescription>
              Reopen this closed shift? The cashier will be able to make corrections and re-submit the denomination count.
              This only works if the review is still pending.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleReopen} disabled={reopenMutation.isPending}>
              Reopen Shift
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
