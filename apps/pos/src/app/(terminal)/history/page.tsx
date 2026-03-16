'use client'

import { useState, useEffect, useCallback } from 'react'
import Link from 'next/link'
import { useAuth } from '@clerk/nextjs'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Search, X, CheckCircle2, Clock, Wrench, XCircle, RotateCcw, ChevronRight } from 'lucide-react'
import type { TransactionSummary } from '@splashsphere/types'
import type { PagedResult } from '@splashsphere/types'
import { TransactionStatus } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { useSignalREvent } from '@/lib/signalr-context'
import { cn } from '@/lib/utils'
import type { TransactionUpdatedPayload } from '@splashsphere/types'

const BRANCH_KEY = 'pos-branch-id'

const TX_STATUS: Record<number, { label: string; cls: string; icon: React.ReactNode }> = {
  [TransactionStatus.Pending]:    { label: 'Pending',     cls: 'bg-yellow-500/20 text-yellow-300', icon: <Clock className="h-3 w-3" /> },
  [TransactionStatus.InProgress]: { label: 'In Progress', cls: 'bg-blue-500/20 text-blue-300',     icon: <Wrench className="h-3 w-3" /> },
  [TransactionStatus.Completed]:  { label: 'Completed',   cls: 'bg-green-500/20 text-green-300',   icon: <CheckCircle2 className="h-3 w-3" /> },
  [TransactionStatus.Cancelled]:  { label: 'Cancelled',   cls: 'bg-red-500/20 text-red-300',       icon: <XCircle className="h-3 w-3" /> },
  [TransactionStatus.Refunded]:   { label: 'Refunded',    cls: 'bg-purple-500/20 text-purple-300', icon: <RotateCcw className="h-3 w-3" /> },
}

const STATUS_FILTERS = [
  { label: 'All',         value: '' },
  { label: 'Pending',     value: String(TransactionStatus.Pending) },
  { label: 'In Progress', value: String(TransactionStatus.InProgress) },
  { label: 'Completed',   value: String(TransactionStatus.Completed) },
  { label: 'Cancelled',   value: String(TransactionStatus.Cancelled) },
]

function fmt(amount: number) {
  return `₱${amount.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function fmtTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-PH', { hour: 'numeric', minute: '2-digit', hour12: true })
}

export default function HistoryPage() {
  const { getToken } = useAuth()
  const queryClient = useQueryClient()
  const [branchId, setBranchId] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [search, setSearch] = useState('')
  const [date, setDate] = useState(() => new Date().toISOString().slice(0, 10))
  const [page, setPage] = useState(1)

  useEffect(() => {
    setBranchId(localStorage.getItem(BRANCH_KEY) ?? '')
  }, [])

  // Reset page when filters change
  const resetPage = useCallback(() => setPage(1), [])

  // Invalidate when any transaction changes in real time
  useSignalREvent<TransactionUpdatedPayload>('TransactionUpdated', () => {
    queryClient.invalidateQueries({ queryKey: ['transactions'] })
  })

  const { data, isLoading, isFetching } = useQuery({
    queryKey: ['transactions', branchId, date, statusFilter, search, page],
    enabled: !!branchId,
    staleTime: 15_000,
    queryFn: async () => {
      const token = await getToken()
      const params = new URLSearchParams({
        branchId,
        date,
        page: String(page),
        pageSize: '20',
      })
      if (statusFilter) params.set('status', statusFilter)
      if (search.trim()) params.set('search', search.trim())
      return apiClient.get<PagedResult<TransactionSummary>>(
        `/transactions?${params.toString()}`,
        token ?? undefined,
      )
    },
  })

  const items = data?.items ?? []
  const totalPages = data?.totalPages ?? 1

  return (
    <div className="p-4 space-y-4 max-w-2xl mx-auto">

      {/* Header */}
      <div className="flex items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-bold text-white">Transaction History</h1>
          {data && (
            <p className="text-sm text-gray-400">{data.totalCount} transactions</p>
          )}
        </div>
        <input
          type="date"
          value={date}
          onChange={(e) => { setDate(e.target.value); resetPage() }}
          className="min-h-[44px] px-3 rounded-lg bg-gray-800 border border-gray-700 text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-500 pointer-events-none" />
        <input
          type="text"
          value={search}
          onChange={(e) => { setSearch(e.target.value); resetPage() }}
          placeholder="Search plate or transaction #…"
          className="w-full min-h-[48px] pl-10 pr-10 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        {search && (
          <button
            onClick={() => { setSearch(''); resetPage() }}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-white"
          >
            <X className="h-4 w-4" />
          </button>
        )}
      </div>

      {/* Status filter tabs */}
      <div className="flex gap-1.5 overflow-x-auto pb-0.5 scrollbar-none">
        {STATUS_FILTERS.map((f) => (
          <button
            key={f.value}
            onClick={() => { setStatusFilter(f.value); resetPage() }}
            className={cn(
              'shrink-0 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors min-h-[36px]',
              statusFilter === f.value
                ? 'bg-blue-600 text-white'
                : 'bg-gray-800 text-gray-400 hover:text-white border border-gray-700',
            )}
          >
            {f.label}
          </button>
        ))}
      </div>

      {/* Transaction list */}
      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="h-[68px] rounded-xl bg-gray-800 animate-pulse" />
          ))}
        </div>
      ) : items.length === 0 ? (
        <div className="rounded-xl border border-dashed border-gray-700 py-16 text-center">
          <p className="text-gray-500">No transactions found</p>
          {(search || statusFilter) && (
            <button
              onClick={() => { setSearch(''); setStatusFilter(''); resetPage() }}
              className="mt-3 text-sm text-blue-400 hover:text-blue-300"
            >
              Clear filters
            </button>
          )}
        </div>
      ) : (
        <div className="space-y-1.5">
          {items.map((tx) => {
            const st = TX_STATUS[tx.status] ?? { label: 'Unknown', cls: 'text-gray-400', icon: null }
            return (
              <Link
                key={tx.id}
                href={`/transactions/${tx.id}`}
                className="flex items-center gap-3 px-3 py-3 rounded-xl bg-gray-800 border border-gray-700 hover:border-gray-600 transition-colors"
              >
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-mono font-bold text-white">{tx.plateNumber}</p>
                    <span className={cn('flex items-center gap-1 text-xs px-1.5 py-0.5 rounded-full', st.cls)}>
                      {st.icon}
                      {st.label}
                    </span>
                  </div>
                  <p className="text-xs text-gray-500 truncate mt-0.5">
                    {tx.transactionNumber} · {tx.cashierName} · {fmtTime(tx.createdAt)}
                  </p>
                </div>
                <div className="flex items-center gap-1.5 shrink-0">
                  <span className="text-sm font-semibold text-white">{fmt(tx.finalAmount)}</span>
                  <ChevronRight className="h-4 w-4 text-gray-600" />
                </div>
              </Link>
            )
          })}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between gap-2 pt-2">
          <button
            disabled={page === 1 || isFetching}
            onClick={() => setPage((p) => p - 1)}
            className="min-h-[44px] px-4 rounded-lg bg-gray-800 border border-gray-700 text-sm text-white disabled:opacity-40 hover:bg-gray-700 transition-colors"
          >
            Previous
          </button>
          <span className="text-sm text-gray-500">
            Page {page} of {totalPages}
          </span>
          <button
            disabled={page >= totalPages || isFetching}
            onClick={() => setPage((p) => p + 1)}
            className="min-h-[44px] px-4 rounded-lg bg-gray-800 border border-gray-700 text-sm text-white disabled:opacity-40 hover:bg-gray-700 transition-colors"
          >
            Next
          </button>
        </div>
      )}
    </div>
  )
}
