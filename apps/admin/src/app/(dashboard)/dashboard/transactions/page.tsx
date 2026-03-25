'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Receipt } from 'lucide-react'
import { EmptyState } from '@/components/ui/empty-state'
import { StatusBadge } from '@/components/ui/status-badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Input } from '@/components/ui/input'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { useTransactions, transactionKeys } from '@/hooks/use-transactions'
import { useBranches } from '@/hooks/use-branches'
import { useQueryClient } from '@tanstack/react-query'
import { useSignalREvent } from '@/lib/signalr-context'
import type { TransactionUpdatedPayload } from '@splashsphere/types'
import { TransactionStatus } from '@splashsphere/types'
import type { TransactionSummary } from '@splashsphere/types'
import { cn } from '@/lib/utils'
import { formatPeso } from '@/lib/format'

const STATUS_LABELS: Record<TransactionStatus, string> = {
  [TransactionStatus.Pending]: 'Pending',
  [TransactionStatus.InProgress]: 'In Progress',
  [TransactionStatus.Completed]: 'Completed',
  [TransactionStatus.Cancelled]: 'Cancelled',
  [TransactionStatus.Refunded]: 'Refunded',
}

const STATUS_KEYS: Record<TransactionStatus, string> = {
  [TransactionStatus.Pending]: 'Pending',
  [TransactionStatus.InProgress]: 'InProgress',
  [TransactionStatus.Completed]: 'Completed',
  [TransactionStatus.Cancelled]: 'Cancelled',
  [TransactionStatus.Refunded]: 'Refunded',
}

function TransactionRow({ tx }: { tx: TransactionSummary }) {
  const router = useRouter()
  return (
    <tr
      className={cn(
        'hover:bg-muted/40 cursor-pointer transition-colors',
        tx.status === TransactionStatus.Cancelled && 'opacity-60'
      )}
      onClick={() => router.push(`/dashboard/transactions/${tx.id}`)}
    >
      <td className="px-4 py-3 font-mono text-xs font-semibold">{tx.transactionNumber}</td>
      <td className="px-4 py-3">
        <p className="font-medium text-sm font-mono">{tx.plateNumber}</p>
        <p className="text-xs text-muted-foreground">{tx.vehicleTypeName} · {tx.sizeName}</p>
      </td>
      <td className="px-4 py-3 text-sm text-muted-foreground">{tx.customerName ?? '—'}</td>
      <td className="px-4 py-3 text-right font-semibold tabular-nums">{formatPeso(tx.finalAmount)}</td>
      <td className="px-4 py-3"><StatusBadge status={STATUS_KEYS[tx.status]} /></td>
      <td className="px-4 py-3 text-sm text-muted-foreground">
        {new Date(tx.createdAt).toLocaleDateString('en-PH', {
          month: 'short', day: 'numeric', year: 'numeric',
        })}
        {' '}
        <span className="text-xs">
          {new Date(tx.createdAt).toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit' })}
        </span>
      </td>
    </tr>
  )
}

export default function TransactionsPage() {
  const router = useRouter()
  const queryClient = useQueryClient()
  const [branchId, setBranchId] = useState<string>('')

  // Refresh list when any transaction status changes
  useSignalREvent<TransactionUpdatedPayload>('TransactionUpdated', () => {
    queryClient.invalidateQueries({ queryKey: transactionKeys.all })
  })
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')

  const { data: branches = [] } = useBranches()
  const { data, isLoading, isError } = useTransactions({
    branchId: branchId || undefined,
    status: statusFilter !== 'all' ? (Number(statusFilter) as TransactionStatus) : undefined,
    dateFrom: dateFrom || undefined,
    dateTo: dateTo || undefined,
    pageSize: 50,
  })

  const transactions = data ? [...data.items] : []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Transactions</h1>
        <p className="text-muted-foreground">Transaction history across branches</p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <Select value={branchId} onValueChange={setBranchId}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="Select branch…" />
          </SelectTrigger>
          <SelectContent>
            {branches.map((b) => (
              <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-40">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All statuses</SelectItem>
            {Object.entries(STATUS_LABELS).map(([k, v]) => (
              <SelectItem key={k} value={k}>{v}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">From</label>
          <Input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="h-9 w-36"
          />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">To</label>
          <Input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="h-9 w-36"
          />
        </div>
      </div>

      {!branchId && (
        <div className="rounded-lg border border-muted bg-muted/30 p-6 text-center text-sm text-muted-foreground">
          Select a branch to view transactions
        </div>
      )}

      {branchId && isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      )}

      {branchId && isError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
          Failed to load transactions.
        </div>
      )}

      {branchId && !isLoading && !isError && transactions.length === 0 && (
        <EmptyState
          icon={Receipt}
          title="No transactions found"
          description="Try adjusting the filters"
        />
      )}

      {branchId && !isLoading && transactions.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Txn #</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Vehicle</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Customer</th>
                <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">Amount</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Status</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Date</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {transactions.map((tx) => (
                <TransactionRow key={tx.id} tx={tx} />
              ))}
            </tbody>
          </table>
          {data && (
            <div className="px-4 py-3 text-sm text-muted-foreground border-t">
              Showing 1–{transactions.length} of {data.totalCount} results
            </div>
          )}
        </div>
      )}
    </div>
  )
}
