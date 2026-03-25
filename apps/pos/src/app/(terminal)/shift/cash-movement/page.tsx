'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQueryClient, useMutation } from '@tanstack/react-query'
import { ArrowLeft, ArrowUpRight, ArrowDownLeft, Loader2 } from 'lucide-react'
import Link from 'next/link'
import { apiClient } from '@/lib/api-client'
import { useCurrentShift } from '@/lib/use-shift'
import { CashMovementType } from '@splashsphere/types'
import { cn } from '@/lib/utils'

const PRESETS: { label: string; type: CashMovementType }[] = [
  { label: 'Supplies Purchase', type: CashMovementType.CashOut },
  { label: 'Employee Meals',    type: CashMovementType.CashOut },
  { label: 'Employee Vale',     type: CashMovementType.CashOut },
  { label: 'Additional Change', type: CashMovementType.CashIn  },
]

export default function CashMovementPage() {
  const router = useRouter()
  const { getToken } = useAuth()
  const queryClient = useQueryClient()
  const { data: shift } = useCurrentShift()

  const [type, setType]           = useState<CashMovementType>(CashMovementType.CashOut)
  const [amount, setAmount]       = useState('')
  const [reason, setReason]       = useState('')
  const [reference, setReference] = useState('')
  const [error, setError]         = useState<string | null>(null)

  const mutation = useMutation({
    mutationFn: async () => {
      if (!shift) throw new Error('No active shift.')
      const token = await getToken()
      return apiClient.post<{ id: string }>(
        `/shifts/${shift.id}/cash-movement`,
        { type, amount: parseFloat(amount), reason, reference: reference || null },
        token ?? undefined,
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['current-shift'] })
      router.push('/shift')
    },
    onError: (err: unknown) => {
      setError((err as { detail?: string })?.detail ?? 'Failed to record movement.')
    },
  })

  const handlePreset = (preset: typeof PRESETS[0]) => {
    setType(preset.type)
    setReason(preset.label)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    const parsed = parseFloat(amount)
    if (isNaN(parsed) || parsed <= 0) {
      setError('Enter a valid amount greater than zero.')
      return
    }
    if (!reason.trim()) {
      setError('Reason is required.')
      return
    }
    mutation.mutate()
  }

  if (!shift) {
    return (
      <div className="p-4 max-w-sm mx-auto pt-8 text-center">
        <p className="text-gray-400">No active shift.</p>
        <Link href="/shift/open" className="text-blue-400 underline mt-2 block">Open a shift</Link>
      </div>
    )
  }

  const isCashIn = type === CashMovementType.CashIn

  return (
    <div className="p-4 max-w-sm mx-auto space-y-5">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Link href="/shift" className="p-2 rounded-lg text-gray-400 hover:text-white hover:bg-gray-800 min-h-[44px] min-w-[44px] flex items-center justify-center transition-all duration-150 active:scale-[0.97]">
          <ArrowLeft className="h-5 w-5" />
        </Link>
        <div>
          <h1 className="text-xl font-bold text-white">Record Cash Movement</h1>
          <p className="text-base text-gray-400">{shift.branchName}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        {/* Type toggle */}
        <div className="grid grid-cols-2 gap-2">
          <button
            type="button"
            onClick={() => setType(CashMovementType.CashOut)}
            className={cn(
              'flex items-center justify-center gap-2 py-3 rounded-xl font-semibold border transition-all duration-150 active:scale-[0.97] min-h-[44px]',
              !isCashIn
                ? 'bg-red-600/20 border-red-500/50 text-red-300'
                : 'bg-gray-800 border-gray-700 text-gray-400 hover:border-gray-600'
            )}
          >
            <ArrowDownLeft className="h-4 w-4" />
            Cash Out
          </button>
          <button
            type="button"
            onClick={() => setType(CashMovementType.CashIn)}
            className={cn(
              'flex items-center justify-center gap-2 py-3 rounded-xl font-semibold border transition-all duration-150 active:scale-[0.97] min-h-[44px]',
              isCashIn
                ? 'bg-green-600/20 border-green-500/50 text-green-300'
                : 'bg-gray-800 border-gray-700 text-gray-400 hover:border-gray-600'
            )}
          >
            <ArrowUpRight className="h-4 w-4" />
            Cash In
          </button>
        </div>

        {/* Quick presets */}
        <div>
          <p className="text-xs text-gray-500 mb-2">Quick presets</p>
          <div className="flex flex-wrap gap-2">
            {PRESETS.map(p => (
              <button
                key={p.label}
                type="button"
                onClick={() => handlePreset(p)}
                className={cn(
                  'px-3 py-2 rounded-lg text-sm font-medium border transition-all duration-150 active:scale-[0.97] min-h-[44px]',
                  reason === p.label && type === p.type
                    ? 'bg-blue-600 border-blue-500 text-white'
                    : 'bg-gray-800 border-gray-700 text-gray-400 hover:border-gray-600'
                )}
              >
                {p.label}
              </button>
            ))}
          </div>
        </div>

        {/* Amount */}
        <div>
          <label className="block text-sm text-gray-400 mb-1.5">Amount</label>
          <div className="relative">
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 text-xl font-semibold">₱</span>
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={amount}
              onChange={e => setAmount(e.target.value)}
              placeholder="0.00"
              className="w-full pl-9 pr-4 py-3.5 rounded-xl bg-gray-800 border border-gray-700 focus:border-blue-500 focus:outline-none text-white text-xl font-mono tabular-nums font-bold text-right"
            />
          </div>
        </div>

        {/* Reason */}
        <div>
          <label className="block text-sm text-gray-400 mb-1.5">Reason</label>
          <input
            type="text"
            value={reason}
            onChange={e => setReason(e.target.value)}
            maxLength={500}
            placeholder="e.g. Soap purchase, Employee vale…"
            className="w-full px-4 py-3 rounded-xl bg-gray-800 border border-gray-700 focus:border-blue-500 focus:outline-none text-white"
          />
        </div>

        {/* Reference (optional) */}
        <div>
          <label className="block text-sm text-gray-400 mb-1.5">
            Reference <span className="text-gray-600">(optional)</span>
          </label>
          <input
            type="text"
            value={reference}
            onChange={e => setReference(e.target.value)}
            maxLength={256}
            placeholder="Receipt no., employee name…"
            className="w-full px-4 py-3 rounded-xl bg-gray-800 border border-gray-700 focus:border-blue-500 focus:outline-none text-white"
          />
        </div>

        {error && (
          <p className="text-sm text-red-400 bg-red-500/10 border border-red-500/20 rounded-lg px-3 py-2">
            {error}
          </p>
        )}

        <button
          type="submit"
          disabled={mutation.isPending}
          className={cn(
            'w-full flex items-center justify-center gap-2 py-4 rounded-xl font-bold text-lg transition-all duration-150 active:scale-[0.97] min-h-[56px] disabled:opacity-50',
            isCashIn
              ? 'bg-green-600 hover:bg-green-500 text-white'
              : 'bg-red-600 hover:bg-red-500 text-white'
          )}
        >
          {mutation.isPending ? (
            <Loader2 className="h-5 w-5 animate-spin" />
          ) : isCashIn ? (
            <ArrowUpRight className="h-5 w-5" />
          ) : (
            <ArrowDownLeft className="h-5 w-5" />
          )}
          Record {isCashIn ? 'Cash In' : 'Cash Out'}
        </button>
      </form>
    </div>
  )
}
