'use client'

import Link from 'next/link'
import {
  Wallet, Plus, Clock, Receipt,
  TrendingUp, ArrowUpRight, ArrowDownLeft, ChevronRight, Loader2,
} from 'lucide-react'
import { useCurrentShift } from '@/lib/use-shift'
import { CashMovementType, PaymentMethod } from '@splashsphere/types'
import { cn } from '@/lib/utils'

function fmt(n: number) {
  return `₱${n.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-PH', {
    hour: 'numeric', minute: '2-digit', hour12: true,
  })
}

function fmtDuration(openedAt: string) {
  const diffMs = Date.now() - new Date(openedAt).getTime()
  const h = Math.floor(diffMs / 3_600_000)
  const m = Math.floor((diffMs % 3_600_000) / 60_000)
  return h > 0 ? `${h}h ${m}m` : `${m}m`
}

const METHOD_LABEL: Record<number, string> = {
  [PaymentMethod.Cash]:         'Cash',
  [PaymentMethod.GCash]:        'GCash',
  [PaymentMethod.CreditCard]:   'Credit Card',
  [PaymentMethod.DebitCard]:    'Debit Card',
  [PaymentMethod.BankTransfer]: 'Bank Transfer',
}

export default function ShiftPage() {
  const { data: shift, isLoading } = useCurrentShift()

  if (isLoading) {
    return (
      <div className="p-4 flex items-center justify-center pt-16">
        <Loader2 className="h-6 w-6 text-gray-500 animate-spin" />
      </div>
    )
  }

  if (!shift) {
    return (
      <div className="p-4 max-w-sm mx-auto pt-8 space-y-4">
        <div className="rounded-xl bg-gray-800 border border-gray-700 p-6 text-center space-y-3">
          <Wallet className="h-10 w-10 text-gray-600 mx-auto" />
          <p className="text-white font-semibold">No Active Shift</p>
          <p className="text-sm text-gray-400">Open a shift to start processing transactions.</p>
        </div>
        <Link
          href="/shift/open"
          className="flex items-center justify-center gap-2 w-full py-4 rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-bold text-lg transition-colors min-h-[56px]"
        >
          <Wallet className="h-5 w-5" />
          Open Shift
        </Link>
      </div>
    )
  }

  const cashIn  = shift.cashMovements.filter(m => m.type === CashMovementType.CashIn).reduce((s, m) => s + m.amount, 0)
  const cashOut = shift.cashMovements.filter(m => m.type === CashMovementType.CashOut).reduce((s, m) => s + m.amount, 0)

  return (
    <div className="p-4 max-w-sm mx-auto space-y-4">
      {/* Shift header card */}
      <div className="rounded-xl bg-gray-800 border border-gray-700 p-4 space-y-1">
        <div className="flex items-center justify-between">
          <span className="flex items-center gap-2">
            <span className="h-2.5 w-2.5 rounded-full bg-green-400" />
            <span className="text-green-300 text-sm font-semibold">Shift Active</span>
          </span>
          <span className="text-xs text-gray-500">{fmtDuration(shift.openedAt)}</span>
        </div>
        <p className="text-white text-2xl font-bold">{shift.branchName}</p>
        <p className="text-xs text-gray-400">
          Started {fmtTime(shift.openedAt)} · Opening fund {fmt(shift.openingCashFund)}
        </p>
      </div>

      {/* Stats grid */}
      <div className="grid grid-cols-2 gap-3">
        <div className="rounded-xl bg-gray-800 border border-gray-700 p-3">
          <div className="flex items-center gap-1.5 mb-1">
            <Receipt className="h-3.5 w-3.5 text-blue-400" />
            <span className="text-xs text-gray-400">Transactions</span>
          </div>
          <p className="text-xl font-bold text-white">{shift.totalTransactionCount}</p>
        </div>
        <div className="rounded-xl bg-gray-800 border border-gray-700 p-3">
          <div className="flex items-center gap-1.5 mb-1">
            <TrendingUp className="h-3.5 w-3.5 text-green-400" />
            <span className="text-xs text-gray-400">Revenue</span>
          </div>
          <p className="text-xl font-bold text-white">{fmt(shift.totalRevenue)}</p>
        </div>
        <div className="rounded-xl bg-gray-800 border border-gray-700 p-3">
          <div className="flex items-center gap-1.5 mb-1">
            <ArrowUpRight className="h-3.5 w-3.5 text-yellow-400" />
            <span className="text-xs text-gray-400">Cash In</span>
          </div>
          <p className="text-xl font-bold text-white">{fmt(cashIn)}</p>
        </div>
        <div className="rounded-xl bg-gray-800 border border-gray-700 p-3">
          <div className="flex items-center gap-1.5 mb-1">
            <ArrowDownLeft className="h-3.5 w-3.5 text-red-400" />
            <span className="text-xs text-gray-400">Cash Out</span>
          </div>
          <p className="text-xl font-bold text-white">{fmt(cashOut)}</p>
        </div>
      </div>

      {/* Cash movements log */}
      {shift.cashMovements.length > 0 && (
        <div>
          <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
            Cash Movements
          </h2>
          <div className="space-y-1.5">
            {shift.cashMovements.map(m => (
              <div
                key={m.id}
                className="flex items-center justify-between px-3 py-2 rounded-lg bg-gray-800 border border-gray-700"
              >
                <div className="min-w-0">
                  <p className="text-sm text-white">{m.reason}</p>
                  {m.reference && <p className="text-xs text-gray-500">{m.reference}</p>}
                </div>
                <span className={cn(
                  'text-sm font-semibold shrink-0 ml-2',
                  m.type === CashMovementType.CashIn ? 'text-green-400' : 'text-red-400'
                )}>
                  {m.type === CashMovementType.CashIn ? '+' : '-'}{fmt(m.amount)}
                </span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Payment breakdown */}
      {shift.paymentSummaries.length > 0 && (
        <div>
          <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
            Payments
          </h2>
          <div className="rounded-xl bg-gray-800 border border-gray-700 overflow-hidden">
            {shift.paymentSummaries.map((ps, i) => (
              <div
                key={ps.method}
                className={cn(
                  'flex items-center justify-between px-3 py-2',
                  i !== shift.paymentSummaries.length - 1 && 'border-b border-gray-700'
                )}
              >
                <span className="text-sm text-gray-300">{METHOD_LABEL[ps.method] ?? 'Other'}</span>
                <div className="text-right">
                  <p className="text-sm font-semibold text-white">{fmt(ps.totalAmount)}</p>
                  <p className="text-xs text-gray-500">{ps.transactionCount} txn{ps.transactionCount !== 1 ? 's' : ''}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="space-y-2 pt-1">
        <Link
          href="/shift/cash-movement"
          className="flex items-center justify-between px-4 py-3.5 rounded-xl bg-gray-800 border border-gray-700 hover:border-gray-600 transition-colors min-h-[52px]"
        >
          <div className="flex items-center gap-2">
            <Plus className="h-4 w-4 text-yellow-400" />
            <span className="text-white font-medium">Record Cash In / Out</span>
          </div>
          <ChevronRight className="h-4 w-4 text-gray-600" />
        </Link>

        <Link
          href="/shift/close"
          className="flex items-center justify-center gap-2 w-full py-4 rounded-xl bg-red-600/20 border border-red-500/30 hover:bg-red-600/30 text-red-300 font-semibold transition-colors min-h-[52px]"
        >
          <Clock className="h-4 w-4" />
          Close Shift (End of Day)
        </Link>
      </div>
    </div>
  )
}
