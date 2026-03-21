'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQueryClient, useQuery, useMutation } from '@tanstack/react-query'
import { Wallet, ArrowLeft, Loader2 } from 'lucide-react'
import Link from 'next/link'
import { apiClient } from '@/lib/api-client'
import { useBranch } from '@/lib/branch-context'
import { useCurrentShift } from '@/lib/use-shift'
import type { ShiftSettingsDto } from '@splashsphere/types'
import { cn } from '@/lib/utils'

function fmt(n: number) {
  return `₱${n.toLocaleString('en-PH', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`
}

export default function OpenShiftPage() {
  const router = useRouter()
  const { getToken } = useAuth()
  const { branchId } = useBranch()
  const queryClient = useQueryClient()
  const { data: currentShift } = useCurrentShift()

  const [amount, setAmount] = useState('')
  const [error, setError] = useState<string | null>(null)

  // Load settings for default opening fund
  const { data: settings } = useQuery({
    queryKey: ['shift-settings'],
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ShiftSettingsDto>('/settings/shift-config', token ?? undefined)
    },
  })

  // Pre-fill with default on load
  useEffect(() => {
    if (settings && !amount) {
      setAmount(String(settings.defaultOpeningFund))
    }
  }, [settings]) // eslint-disable-line react-hooks/exhaustive-deps

  const openMutation = useMutation({
    mutationFn: async (openingCashFund: number) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/shifts/open', { branchId, openingCashFund }, token ?? undefined)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['current-shift'] })
      router.push('/shift')
    },
    onError: (err: unknown) => {
      setError((err as { detail?: string })?.detail ?? 'Failed to open shift.')
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    const parsed = parseFloat(amount)
    if (isNaN(parsed) || parsed < 0) {
      setError('Please enter a valid opening cash fund amount.')
      return
    }
    openMutation.mutate(parsed)
  }

  // Already has an open shift
  if (currentShift) {
    return (
      <div className="p-4 max-w-sm mx-auto space-y-4 pt-8">
        <div className="rounded-xl bg-green-500/10 border border-green-500/30 p-4 text-center">
          <span className="inline-block h-3 w-3 rounded-full bg-green-400 mb-2" />
          <p className="text-green-300 font-semibold">You have an active shift</p>
          <p className="text-sm text-green-400 mt-1">Close it before opening a new one.</p>
        </div>
        <Link
          href="/shift"
          className="flex items-center justify-center gap-2 w-full py-3 rounded-xl bg-gray-800 text-white font-semibold"
        >
          View Active Shift
        </Link>
      </div>
    )
  }

  const presets = [1000, 2000, 3000, 5000]

  return (
    <div className="p-4 max-w-sm mx-auto space-y-6 pt-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Link href="/home" className="p-2 rounded-lg text-gray-400 hover:text-white hover:bg-gray-800">
          <ArrowLeft className="h-5 w-5" />
        </Link>
        <div>
          <h1 className="text-xl font-bold text-white">Open Shift</h1>
          <p className="text-sm text-gray-400">Enter your starting cash fund</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-5">
        {/* Amount input */}
        <div>
          <label className="block text-sm text-gray-400 mb-2">Opening Cash Fund</label>
          <div className="relative">
            <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 text-xl font-semibold">₱</span>
            <input
              type="number"
              min="0"
              step="0.01"
              value={amount}
              onChange={e => setAmount(e.target.value)}
              placeholder="0.00"
              className="w-full pl-9 pr-4 py-4 rounded-xl bg-gray-800 border border-gray-700 focus:border-blue-500 focus:outline-none text-white text-2xl font-mono font-bold text-right"
            />
          </div>
        </div>

        {/* Quick presets */}
        <div>
          <p className="text-xs text-gray-500 mb-2">Quick amounts</p>
          <div className="grid grid-cols-4 gap-2">
            {presets.map(p => (
              <button
                key={p}
                type="button"
                onClick={() => setAmount(String(p))}
                className={cn(
                  'py-2 rounded-lg text-sm font-medium border transition-colors',
                  amount === String(p)
                    ? 'bg-blue-600 border-blue-500 text-white'
                    : 'bg-gray-800 border-gray-700 text-gray-300 hover:border-gray-600'
                )}
              >
                {fmt(p)}
              </button>
            ))}
          </div>
        </div>

        {error && (
          <p className="text-sm text-red-400 bg-red-500/10 border border-red-500/20 rounded-lg px-3 py-2">
            {error}
          </p>
        )}

        <button
          type="submit"
          disabled={openMutation.isPending || !branchId}
          className="w-full flex items-center justify-center gap-2 py-4 rounded-xl bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white font-bold text-lg transition-colors min-h-[56px]"
        >
          {openMutation.isPending ? (
            <Loader2 className="h-5 w-5 animate-spin" />
          ) : (
            <Wallet className="h-5 w-5" />
          )}
          Start Shift
        </button>

        {!branchId && (
          <p className="text-xs text-yellow-400 text-center">Select a branch first.</p>
        )}
      </form>
    </div>
  )
}
