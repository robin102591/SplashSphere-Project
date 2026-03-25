'use client'

import { useState, useMemo } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQueryClient, useMutation } from '@tanstack/react-query'
import {
  ArrowLeft, ArrowRight, CheckCircle2, AlertTriangle, XCircle, Loader2,
} from 'lucide-react'
import Link from 'next/link'
import { apiClient } from '@/lib/api-client'
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

const BILLS   = [1000, 500, 200, 100, 50, 20]
const COINS   = [10, 5, 1, 0.25]
const ALL_DENOMS = [...BILLS, ...COINS]

const METHOD_LABEL: Record<number, string> = {
  [PaymentMethod.Cash]:         'Cash',
  [PaymentMethod.GCash]:        'GCash',
  [PaymentMethod.CreditCard]:   'Credit Card',
  [PaymentMethod.DebitCard]:    'Debit Card',
  [PaymentMethod.BankTransfer]: 'Bank Transfer',
}

type DenomCounts = Record<number, number>

export default function CloseShiftPage() {
  const router = useRouter()
  const { getToken } = useAuth()
  const queryClient = useQueryClient()
  const { data: shift, isLoading } = useCurrentShift()

  const [step, setStep] = useState<1 | 2 | 3>(1)
  const [counts, setCounts] = useState<DenomCounts>(() =>
    Object.fromEntries(ALL_DENOMS.map(d => [d, 0]))
  )
  const [error, setError] = useState<string | null>(null)

  const closeMutation = useMutation({
    mutationFn: async () => {
      if (!shift) throw new Error('No shift found.')
      const token = await getToken()
      const denominations = ALL_DENOMS.map(d => ({
        denominationValue: d,
        count: counts[d] ?? 0,
      }))
      await apiClient.post<void>(
        `/shifts/${shift.id}/close`,
        { denominations },
        token ?? undefined,
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['current-shift'] })
      router.push(`/shift/report?shiftId=${shift!.id}`)
    },
    onError: (err: unknown) => {
      setError((err as { detail?: string })?.detail ?? 'Failed to close shift.')
    },
  })

  const totalCounted = useMemo(
    () => ALL_DENOMS.reduce((sum, d) => sum + d * (counts[d] ?? 0), 0),
    [counts]
  )

  if (isLoading) {
    return (
      <div className="p-4 max-w-lg mx-auto space-y-4 animate-pulse">
        {/* Header skeleton */}
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-lg bg-gray-800/60" />
          <div className="flex-1 space-y-2">
            <div className="h-6 w-32 rounded bg-gray-800/60" />
            <div className="h-4 w-20 rounded bg-gray-800/60" />
          </div>
        </div>
        {/* Summary card skeleton */}
        <div className="h-48 rounded-xl bg-gray-800/60" />
        {/* Action button skeleton */}
        <div className="h-14 rounded-xl bg-gray-800/60" />
      </div>
    )
  }

  if (!shift) {
    return (
      <div className="p-4 max-w-sm mx-auto pt-8 text-center">
        <p className="text-gray-400">No active shift to close.</p>
        <Link href="/shift/open" className="text-blue-400 underline mt-2 block">Open a shift</Link>
      </div>
    )
  }

  const cashIn  = shift.cashMovements.filter(m => m.type === CashMovementType.CashIn).reduce((s, m) => s + m.amount, 0)
  const cashOut = shift.cashMovements.filter(m => m.type === CashMovementType.CashOut).reduce((s, m) => s + m.amount, 0)
  const expectedEstimate = shift.openingCashFund + shift.totalCashPayments + cashIn - cashOut

  const variance = totalCounted - expectedEstimate
  const absVariance = Math.abs(variance)

  const varianceColor = absVariance <= 50
    ? 'text-green-400'
    : absVariance <= 200
      ? 'text-amber-400'
      : 'text-red-400'

  const varianceIcon = absVariance <= 50
    ? <CheckCircle2 className="h-4 w-4" />
    : absVariance <= 200
      ? <AlertTriangle className="h-4 w-4" />
      : <XCircle className="h-4 w-4" />

  const goBack = () => {
    if (step > 1) {
      setStep((s) => (s - 1) as 1 | 2 | 3)
    } else {
      router.back()
    }
  }

  return (
    <div className="p-4 max-w-lg mx-auto space-y-4">
      {/* Header */}
      <div className="flex items-center gap-3">
        <button
          onClick={goBack}
          className="p-2 rounded-lg text-gray-400 hover:text-white hover:bg-gray-800 min-h-[44px] min-w-[44px] flex items-center justify-center transition-all duration-150 active:scale-[0.97]"
        >
          <ArrowLeft className="h-5 w-5" />
        </button>
        <div className="flex-1">
          <h1 className="text-xl font-bold text-white">Close Shift</h1>
          <p className="text-base text-gray-400">Step {step} of 3</p>
        </div>
        {/* Step indicators */}
        <div className="flex gap-1.5">
          {([1, 2, 3] as const).map(s => (
            <div
              key={s}
              className={cn(
                'h-2 w-2 rounded-full',
                s === step ? 'bg-blue-500' : s < step ? 'bg-green-500' : 'bg-gray-700'
              )}
            />
          ))}
        </div>
      </div>

      {/* ── Step 1: Summary ─────────────────────────────────────────────────── */}
      {step === 1 && (
        <div className="space-y-4">
          <div className="rounded-xl bg-gray-800 border border-gray-700 p-4 space-y-3">
            <h2 className="text-sm font-semibold text-gray-300">Shift Summary</h2>

            <div className="grid grid-cols-2 gap-y-2 text-base">
              <span className="text-gray-400">Started</span>
              <span className="text-white font-medium text-right">{fmtTime(shift.openedAt)}</span>

              <span className="text-gray-400">Opening Fund</span>
              <span className="text-white font-medium text-right font-mono tabular-nums">{fmt(shift.openingCashFund)}</span>

              <span className="text-gray-400">Transactions</span>
              <span className="text-white font-medium text-right">{shift.totalTransactionCount}</span>

              <span className="text-gray-400">Revenue</span>
              <span className="text-white font-medium text-right font-mono tabular-nums">{fmt(shift.totalRevenue)}</span>
            </div>
          </div>

          {/* Cash movements summary */}
          {shift.cashMovements.length > 0 && (
            <div className="rounded-xl bg-gray-800 border border-gray-700 p-4 space-y-2">
              <h2 className="text-sm font-semibold text-gray-300">
                Cash Movements ({shift.cashMovements.length})
              </h2>
              {shift.cashMovements.map(m => (
                <div key={m.id} className="flex items-center justify-between text-base">
                  <span className="text-gray-400">{m.reason}</span>
                  <span className={cn(
                    'font-semibold font-mono tabular-nums',
                    m.type === CashMovementType.CashIn ? 'text-green-400' : 'text-red-400'
                  )}>
                    {m.type === CashMovementType.CashIn ? '+' : '-'}{fmt(m.amount)}
                  </span>
                </div>
              ))}
            </div>
          )}

          {/* Payment breakdown */}
          {shift.paymentSummaries.length > 0 && (
            <div className="rounded-xl bg-gray-800 border border-gray-700 p-4 space-y-2">
              <h2 className="text-sm font-semibold text-gray-300">Payments</h2>
              {shift.paymentSummaries.map(ps => (
                <div key={ps.method} className="flex items-center justify-between text-base">
                  <span className="text-gray-400">{METHOD_LABEL[ps.method]}</span>
                  <span className="text-white font-medium font-mono tabular-nums">{fmt(ps.totalAmount)}</span>
                </div>
              ))}
            </div>
          )}

          <button
            onClick={() => setStep(2)}
            className="w-full flex items-center justify-center gap-2 py-4 rounded-xl bg-blue-600 hover:bg-blue-500 active:scale-[0.97] text-white font-bold text-lg transition-all duration-150 min-h-[56px]"
          >
            Continue to Cash Count
            <ArrowRight className="h-5 w-5" />
          </button>
        </div>
      )}

      {/* ── Step 2: Denomination count ───────────────────────────────────────── */}
      {step === 2 && (
        <div className="space-y-4">
          <p className="text-base text-gray-400">Count each denomination in your drawer.</p>

          {/* Bills */}
          <div className="rounded-xl bg-gray-800 border border-gray-700 overflow-hidden">
            <div className="px-4 py-2 border-b border-gray-700">
              <p className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Bills</p>
            </div>
            {BILLS.map(d => {
              const subtotal = d * (counts[d] ?? 0)
              return (
                <div key={d} className="flex items-center px-4 py-2.5 border-b border-gray-700 last:border-0">
                  <span className="font-mono font-bold text-white w-20 shrink-0">
                    ₱{d.toLocaleString()}
                  </span>
                  <span className="text-gray-600 mx-2">×</span>
                  <input
                    type="number"
                    min={0}
                    step={1}
                    value={counts[d] || ''}
                    onChange={e => setCounts(prev => ({ ...prev, [d]: Math.max(0, parseInt(e.target.value) || 0) }))}
                    placeholder="0"
                    className="w-20 text-center text-lg font-mono text-white bg-gray-700 border border-gray-600 rounded-lg py-1.5 focus:border-blue-500 focus:outline-none"
                  />
                  <span className="ml-auto font-mono tabular-nums text-gray-300 text-base w-28 text-right">
                    = {fmt(subtotal)}
                  </span>
                </div>
              )
            })}
          </div>

          {/* Coins */}
          <div className="rounded-xl bg-gray-800 border border-gray-700 overflow-hidden">
            <div className="px-4 py-2 border-b border-gray-700">
              <p className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Coins</p>
            </div>
            {COINS.map(d => {
              const subtotal = d * (counts[d] ?? 0)
              return (
                <div key={d} className="flex items-center px-4 py-2.5 border-b border-gray-700 last:border-0">
                  <span className="font-mono font-bold text-white w-20 shrink-0">
                    {d >= 1 ? `₱${d}` : '₱0.25'}
                  </span>
                  <span className="text-gray-600 mx-2">×</span>
                  <input
                    type="number"
                    min={0}
                    step={1}
                    value={counts[d] || ''}
                    onChange={e => setCounts(prev => ({ ...prev, [d]: Math.max(0, parseInt(e.target.value) || 0) }))}
                    placeholder="0"
                    className="w-20 text-center text-lg font-mono text-white bg-gray-700 border border-gray-600 rounded-lg py-1.5 focus:border-blue-500 focus:outline-none"
                  />
                  <span className="ml-auto font-mono tabular-nums text-gray-300 text-base w-28 text-right">
                    = {fmt(subtotal)}
                  </span>
                </div>
              )
            })}
          </div>

          {/* Running total */}
          <div className="rounded-xl bg-gray-900 border border-gray-700 p-4 space-y-2">
            <div className="flex justify-between text-base">
              <span className="text-gray-400">Total Counted</span>
              <span className="font-mono tabular-nums font-bold text-white">{fmt(totalCounted)}</span>
            </div>
            <div className="flex justify-between text-base">
              <span className="text-gray-400">Expected</span>
              <span className="font-mono tabular-nums text-gray-300">{fmt(expectedEstimate)}</span>
            </div>
            <div className={cn('flex items-center justify-between text-base font-semibold', varianceColor)}>
              <span className="flex items-center gap-1">
                {varianceIcon}
                Variance
              </span>
              <span className="font-mono tabular-nums">
                {variance >= 0 ? '+' : ''}{fmt(variance)}
                {absVariance <= 50 ? ' ✓' : absVariance <= 200 ? ' ⚠' : ' ✗'}
              </span>
            </div>
          </div>

          <button
            onClick={() => setStep(3)}
            className="w-full flex items-center justify-center gap-2 py-4 rounded-xl bg-blue-600 hover:bg-blue-500 active:scale-[0.97] text-white font-bold text-lg transition-all duration-150 min-h-[56px]"
          >
            Review &amp; Confirm
            <ArrowRight className="h-5 w-5" />
          </button>
        </div>
      )}

      {/* ── Step 3: Confirm & close ──────────────────────────────────────────── */}
      {step === 3 && (
        <div className="space-y-4">
          <div className="rounded-xl bg-gray-800 border border-gray-700 p-4 space-y-3">
            <h2 className="text-sm font-semibold text-gray-300">Final Summary</h2>

            <div className="grid grid-cols-2 gap-y-2 text-base">
              <span className="text-gray-400">Opening Fund</span>
              <span className="text-white font-medium font-mono tabular-nums text-right">{fmt(shift.openingCashFund)}</span>

              <span className="text-gray-400">Cash Counted</span>
              <span className="text-white font-mono tabular-nums font-bold text-right">{fmt(totalCounted)}</span>

              <span className="text-gray-400">Expected</span>
              <span className="text-gray-300 font-mono tabular-nums text-right">{fmt(expectedEstimate)}</span>
            </div>

            <div className={cn(
              'flex items-center justify-between pt-2 border-t border-gray-700 text-base font-bold',
              varianceColor
            )}>
              <span className="flex items-center gap-1.5">
                {varianceIcon}
                Variance
              </span>
              <span className="font-mono tabular-nums">
                {variance >= 0 ? '+' : ''}{fmt(variance)}
              </span>
            </div>
          </div>

          {absVariance > 200 && (
            <div className="rounded-xl bg-red-500/10 border border-red-500/30 p-3 text-base text-red-300">
              <strong>Large variance detected.</strong> This shift will be flagged for manager review.
            </div>
          )}

          {absVariance > 50 && absVariance <= 200 && (
            <div className="rounded-xl bg-amber-500/10 border border-amber-500/30 p-3 text-base text-amber-300">
              Variance exceeds threshold. This shift will need manager approval.
            </div>
          )}

          {error && (
            <p className="text-sm text-red-400 bg-red-500/10 border border-red-500/20 rounded-lg px-3 py-2">
              {error}
            </p>
          )}

          <button
            onClick={() => closeMutation.mutate()}
            disabled={closeMutation.isPending}
            className="w-full flex items-center justify-center gap-2 py-4 rounded-xl bg-red-600 hover:bg-red-500 disabled:opacity-50 active:scale-[0.97] text-white font-bold text-lg transition-all duration-150 min-h-[56px]"
          >
            {closeMutation.isPending ? (
              <Loader2 className="h-5 w-5 animate-spin" />
            ) : (
              <CheckCircle2 className="h-5 w-5" />
            )}
            Confirm &amp; Close Shift
          </button>
        </div>
      )}
    </div>
  )
}
