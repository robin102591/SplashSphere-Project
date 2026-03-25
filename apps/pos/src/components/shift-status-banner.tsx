'use client'

import Link from 'next/link'
import { AlertTriangle, Wallet } from 'lucide-react'
import { useCurrentShift, isShiftOpen } from '@/lib/use-shift'

function fmt(n: number) {
  return `₱${n.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-PH', {
    hour: 'numeric', minute: '2-digit', hour12: true,
  })
}

/**
 * Shown at the top of every POS page:
 * - No shift → yellow "Open a shift" banner
 * - Open shift + after 8 PM → amber "Close your shift" nudge
 * - Open shift (daytime) → subtle green info strip
 */
export function ShiftStatusBanner() {
  const { data: shift, isLoading } = useCurrentShift()

  if (isLoading) return null

  const open = isShiftOpen(shift)
  const hour = new Date().getHours()

  if (!open) {
    return (
      <div className="bg-yellow-500/10 border-b border-yellow-500/30 px-4 py-2.5 flex items-center gap-2 min-h-[44px]">
        <AlertTriangle className="h-4 w-4 text-yellow-400 shrink-0" />
        <p className="text-base text-yellow-300 flex-1">
          No active shift.{' '}
          <Link href="/shift/open" className="font-semibold underline hover:text-yellow-200 transition-colors duration-150">
            Open a shift
          </Link>{' '}
          before processing transactions.
        </p>
      </div>
    )
  }

  if (open && hour >= 20) {
    return (
      <div className="bg-amber-500/10 border-b border-amber-500/30 px-4 py-2.5 flex items-center gap-2 min-h-[44px]">
        <AlertTriangle className="h-4 w-4 text-amber-400 shrink-0" />
        <p className="text-base text-amber-300 flex-1">
          End of day?{' '}
          <Link href="/shift/close" className="font-semibold underline hover:text-amber-200 transition-colors duration-150">
            Close your shift
          </Link>.
        </p>
      </div>
    )
  }

  if (open && shift) {
    return (
      <div className="bg-green-500/10 border-b border-green-500/20 px-4 py-1.5 flex items-center gap-2 min-h-[44px]">
        <span className="h-2 w-2 rounded-full bg-green-400 shrink-0" />
        <p className="text-sm text-green-300 flex-1">
          Shift active since {fmtTime(shift.openedAt)} · Fund <span className="font-mono tabular-nums">{fmt(shift.openingCashFund)}</span>
          {shift.cashMovements.length > 0 && (
            <> · {shift.cashMovements.length} movement{shift.cashMovements.length !== 1 ? 's' : ''}</>
          )}
        </p>
        <Link
          href="/shift"
          className="text-sm text-green-400 hover:text-green-300 flex items-center gap-1 shrink-0 font-medium transition-colors duration-150"
        >
          <Wallet className="h-3 w-3" />
          Shift
        </Link>
      </div>
    )
  }

  return null
}
