'use client'

import { Suspense } from 'react'
import { useSearchParams } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { Printer, CheckCircle2, AlertTriangle, XCircle, Home, Loader2 } from 'lucide-react'
import Link from 'next/link'
import { apiClient } from '@/lib/api-client'
import type { ShiftReportDto } from '@splashsphere/types'
import { PaymentMethod, ReviewStatus, CashMovementType } from '@splashsphere/types'
import { cn } from '@/lib/utils'

function fmt(n: number) {
  return `₱${n.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-PH', {
    hour: 'numeric', minute: '2-digit', hour12: true,
  })
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

function ShiftReportContent() {
  const searchParams = useSearchParams()
  const { getToken } = useAuth()
  const shiftId = searchParams.get('shiftId')

  const { data: report, isLoading, error } = useQuery({
    queryKey: ['shift-report', shiftId],
    enabled: !!shiftId,
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ShiftReportDto>(`/shifts/${shiftId}/report`, token ?? undefined)
    },
  })

  if (!shiftId) {
    return (
      <div className="p-4 max-w-sm mx-auto pt-8 text-center">
        <p className="text-gray-400">No shift ID provided.</p>
        <Link href="/home" className="text-blue-400 underline mt-2 block">Go home</Link>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="p-4 flex justify-center pt-16">
        <Loader2 className="h-6 w-6 text-gray-500 animate-spin" />
      </div>
    )
  }

  if (error || !report) {
    return (
      <div className="p-4 max-w-sm mx-auto pt-8 text-center">
        <p className="text-red-400">Failed to load report.</p>
        <Link href="/home" className="text-blue-400 underline mt-2 block">Go home</Link>
      </div>
    )
  }

  const s = report.shift
  const absVariance = Math.abs(s.variance)
  const varianceColor = absVariance <= 50 ? 'text-green-400' : absVariance <= 200 ? 'text-amber-400' : 'text-red-400'
  const varianceLabel = absVariance <= 50 ? '✓ BALANCED' : s.variance > 0 ? '⚠ OVER' : '✗ SHORT'

  return (
    <div className="p-4 max-w-lg mx-auto space-y-4">
      {/* Actions bar — hidden on print */}
      <div className="flex items-center justify-between print:hidden">
        <h1 className="text-xl font-bold text-white">Shift Report</h1>
        <div className="flex gap-2">
          <button
            onClick={() => window.print()}
            className="flex items-center gap-2 px-3 py-2 rounded-lg bg-gray-800 border border-gray-700 text-gray-300 hover:text-white hover:border-gray-600 text-sm transition-colors"
          >
            <Printer className="h-4 w-4" />
            Print
          </button>
          <Link
            href="/home"
            className="flex items-center gap-2 px-3 py-2 rounded-lg bg-blue-600 hover:bg-blue-500 text-white text-sm transition-colors"
          >
            <Home className="h-4 w-4" />
            Done
          </Link>
        </div>
      </div>

      {/* ── Report body (print-friendly) ──────────────────────────────────── */}
      <div className="rounded-xl bg-gray-800 border border-gray-700 overflow-hidden print:border-0 print:rounded-none">

        {/* Report header */}
        <div className="bg-gray-900 px-6 py-4 text-center border-b border-gray-700">
          <p className="text-xs text-gray-500 uppercase tracking-widest mb-1">SplashSphere</p>
          <p className="text-lg font-bold text-white">End of Day Report</p>
        </div>

        {/* Shift info */}
        <div className="px-6 py-4 border-b border-gray-700 grid grid-cols-2 gap-y-1.5 text-sm">
          <span className="text-gray-400">Branch</span>
          <span className="text-white font-medium text-right">{s.branchName}</span>

          <span className="text-gray-400">Cashier</span>
          <span className="text-white font-medium text-right">{s.cashierName}</span>

          <span className="text-gray-400">Date</span>
          <span className="text-white font-medium text-right">{fmtDate(s.shiftDate)}</span>

          <span className="text-gray-400">Shift</span>
          <span className="text-white font-medium text-right">
            {fmtTime(s.openedAt)}
            {s.closedAt && ` — ${fmtTime(s.closedAt)}`}
            {' '}({fmtDuration(s.openedAt, s.closedAt)})
          </span>
        </div>

        {/* Transaction summary */}
        <ReportSection title="Transaction Summary">
          <Row label="Total Transactions"   value={String(s.totalTransactionCount)} />
          <Row label="Total Revenue"        value={fmt(s.totalRevenue)} />
          <Row label="Total Discounts"      value={`-${fmt(s.totalDiscounts)}`} valueClass="text-red-400" />
          <Row label="Net Revenue"          value={fmt(s.totalRevenue - s.totalDiscounts)} bold />
          <Row label="Total Commissions"    value={fmt(s.totalCommissions)} />
        </ReportSection>

        {/* Payment method breakdown */}
        <ReportSection title="Payment Method Breakdown">
          {s.paymentSummaries.map(ps => (
            <div key={ps.method} className="flex items-center justify-between py-1.5 text-sm">
              <span className="text-gray-400">{METHOD_LABEL[ps.method]}</span>
              <div className="text-right">
                <span className="text-white font-medium">{fmt(ps.totalAmount)}</span>
                <span className="text-gray-500 text-xs ml-2">{ps.transactionCount} txns</span>
              </div>
            </div>
          ))}
          <div className="flex justify-between pt-2 border-t border-gray-700 text-sm font-semibold">
            <span className="text-gray-300">Total</span>
            <span className="text-white">{fmt(s.totalRevenue)}</span>
          </div>
        </ReportSection>

        {/* Cash flow */}
        <ReportSection title="Cash Flow">
          <Row label="Opening Cash Fund"           value={fmt(s.openingCashFund)} />
          <Row label="(+) Cash Payments Received"  value={fmt(s.totalCashPayments)} />
          {s.cashMovements.filter(m => m.type === CashMovementType.CashIn).map(m => (
            <div key={m.id} className="flex items-start justify-between py-1 text-xs ml-4">
              <span className="text-gray-500">• {m.reason}</span>
              <span className="text-green-400 font-mono">+{fmt(m.amount)}</span>
            </div>
          ))}
          <Row label="(+) Manual Cash-In"  value={fmt(s.totalCashIn)} valueClass="text-green-400" />
          {s.cashMovements.filter(m => m.type === CashMovementType.CashOut).map(m => (
            <div key={m.id} className="flex items-start justify-between py-1 text-xs ml-4">
              <span className="text-gray-500">• {m.reason}</span>
              <span className="text-red-400 font-mono">-{fmt(m.amount)}</span>
            </div>
          ))}
          <Row label="(-) Manual Cash-Out" value={`-${fmt(s.totalCashOut)}`} valueClass="text-red-400" />
          <div className="pt-2 border-t border-gray-700">
            <Row label="Expected Cash in Drawer" value={fmt(s.expectedCashInDrawer)} bold />
          </div>
        </ReportSection>

        {/* Denomination count */}
        {s.denominations.length > 0 && (
          <ReportSection title="Cash Count (Denomination Breakdown)">
            {s.denominations.map(d => (
              <div key={d.denominationValue} className="flex items-center justify-between py-1 text-sm">
                <span className="font-mono text-gray-300">
                  ₱{d.denominationValue >= 1 ? d.denominationValue.toLocaleString() : '0.25'}
                  {' '}× {d.count}
                </span>
                <span className="font-mono text-white">{fmt(d.subtotal)}</span>
              </div>
            ))}
            <div className="flex justify-between pt-2 border-t border-gray-700 text-sm font-semibold">
              <span className="text-gray-300">Actual Cash Counted</span>
              <span className="text-white">{fmt(s.actualCashInDrawer)}</span>
            </div>
          </ReportSection>
        )}

        {/* Variance */}
        <ReportSection title="Variance">
          <Row label="Expected"  value={fmt(s.expectedCashInDrawer)} />
          <Row label="Actual"    value={fmt(s.actualCashInDrawer)} />
          <div className={cn('flex items-center justify-between pt-2 border-t border-gray-700 text-sm font-bold', varianceColor)}>
            <span>Variance</span>
            <span className="font-mono">{s.variance >= 0 ? '+' : ''}{fmt(s.variance)} {varianceLabel}</span>
          </div>
        </ReportSection>

        {/* Top services */}
        {report.topServices.length > 0 && (
          <ReportSection title="Top Services Today">
            {report.topServices.slice(0, 8).map((svc, i) => (
              <div key={svc.serviceName} className="flex items-center justify-between py-1 text-sm">
                <span className="text-gray-400">{i + 1}. {svc.serviceName}</span>
                <div className="text-right">
                  <span className="text-white font-medium">{fmt(svc.totalAmount)}</span>
                  <span className="text-gray-500 text-xs ml-2">{svc.transactionCount} txns</span>
                </div>
              </div>
            ))}
          </ReportSection>
        )}

        {/* Top employees */}
        {report.topEmployees.length > 0 && (
          <ReportSection title="Top Employees by Commission">
            {report.topEmployees.slice(0, 8).map((emp, i) => (
              <div key={emp.employeeId} className="flex items-center justify-between py-1 text-sm">
                <span className="text-gray-400">{i + 1}. {emp.employeeName}</span>
                <div className="text-right">
                  <span className="text-white font-medium">{fmt(emp.totalCommission)}</span>
                  <span className="text-gray-500 text-xs ml-2">{emp.serviceCount} services</span>
                </div>
              </div>
            ))}
          </ReportSection>
        )}

        {/* Review status */}
        <div className="px-6 py-4 border-t border-gray-700 text-sm">
          <div className="flex items-center justify-between">
            <span className="text-gray-400">Review Status</span>
            <ReviewBadge status={s.reviewStatus} />
          </div>
          {s.reviewedByName && (
            <p className="text-xs text-gray-500 mt-1">
              {s.reviewStatus === ReviewStatus.Approved ? 'Approved' : 'Reviewed'} by {s.reviewedByName}
              {s.reviewedAt && ` at ${fmtTime(s.reviewedAt)}`}
            </p>
          )}
          {s.reviewNotes && (
            <p className="text-xs text-gray-400 mt-1 italic">{s.reviewNotes}</p>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-gray-700 text-center space-y-4">
          <div className="grid grid-cols-2 gap-8 pt-2">
            <div className="text-center">
              <div className="border-t border-gray-600 pt-2">
                <p className="text-xs text-gray-500">Cashier Signature</p>
              </div>
            </div>
            <div className="text-center">
              <div className="border-t border-gray-600 pt-2">
                <p className="text-xs text-gray-500">Manager Signature</p>
              </div>
            </div>
          </div>
          <p className="text-xs text-gray-600">
            Report generated: {new Date(report.generatedAt).toLocaleString('en-PH')}
          </p>
        </div>
      </div>

      {/* Done button (non-print) */}
      <div className="flex gap-3 print:hidden">
        <button
          onClick={() => window.print()}
          className="flex-1 flex items-center justify-center gap-2 py-3 rounded-xl bg-gray-800 border border-gray-700 text-gray-300 hover:text-white font-semibold transition-colors"
        >
          <Printer className="h-4 w-4" />
          Print Report
        </button>
        <Link
          href="/home"
          className="flex-1 flex items-center justify-center gap-2 py-3 rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-semibold transition-colors"
        >
          <Home className="h-4 w-4" />
          Done
        </Link>
      </div>
    </div>
  )
}

export default function ShiftReportPage() {
  return (
    <Suspense fallback={
      <div className="p-4 flex justify-center pt-16">
        <Loader2 className="h-6 w-6 text-gray-500 animate-spin" />
      </div>
    }>
      <ShiftReportContent />
    </Suspense>
  )
}

// ── Sub-components ────────────────────────────────────────────────────────────

function ReportSection({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="px-6 py-4 border-t border-gray-700">
      <h3 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">{title}</h3>
      {children}
    </div>
  )
}

function Row({
  label, value, valueClass, bold,
}: {
  label: string
  value: string
  valueClass?: string
  bold?: boolean
}) {
  return (
    <div className="flex items-center justify-between py-1 text-sm">
      <span className="text-gray-400">{label}</span>
      <span className={cn(bold ? 'font-bold text-white' : 'text-white', valueClass)}>{value}</span>
    </div>
  )
}

function ReviewBadge({ status }: { status: ReviewStatus }) {
  if (status === ReviewStatus.Approved)
    return <span className="flex items-center gap-1 text-green-400 text-xs font-semibold"><CheckCircle2 className="h-3.5 w-3.5" /> Approved</span>
  if (status === ReviewStatus.Flagged)
    return <span className="flex items-center gap-1 text-red-400 text-xs font-semibold"><XCircle className="h-3.5 w-3.5" /> Flagged</span>
  return <span className="flex items-center gap-1 text-yellow-400 text-xs font-semibold"><AlertTriangle className="h-3.5 w-3.5" /> Pending Review</span>
}
