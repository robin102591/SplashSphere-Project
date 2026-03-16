'use client'

import { useState } from 'react'
import Link from 'next/link'
import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import {
  Plus, ListOrdered, LayoutGrid, Users,
  TrendingUp, Receipt, Clock, Car, ChevronRight,
} from 'lucide-react'
import type { DailySummary, QueueStats, TransactionSummary } from '@splashsphere/types'
import type { PagedResult } from '@splashsphere/types'
import { TransactionStatus } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { useBranch } from '@/lib/branch-context'
import { cn } from '@/lib/utils'

const TX_STATUS: Record<number, { label: string; cls: string }> = {
  [TransactionStatus.Pending]:    { label: 'Pending',     cls: 'text-yellow-400' },
  [TransactionStatus.InProgress]: { label: 'In Progress', cls: 'text-blue-400' },
  [TransactionStatus.Completed]:  { label: 'Completed',   cls: 'text-green-400' },
  [TransactionStatus.Cancelled]:  { label: 'Cancelled',   cls: 'text-red-400' },
  [TransactionStatus.Refunded]:   { label: 'Refunded',    cls: 'text-purple-400' },
}

function fmt(amount: number) {
  return `₱${amount.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

export default function HomePage() {
  const { getToken } = useAuth()
  const { branchId } = useBranch()

  const today = new Date().toISOString().slice(0, 10)

  const { data: summary, isLoading: summaryLoading } = useQuery({
    queryKey: ['daily-summary', branchId, today],
    enabled: !!branchId,
    staleTime: 30_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<DailySummary>(
        `/transactions/daily-summary?branchId=${branchId}&date=${today}`,
        token ?? undefined,
      )
    },
  })

  const { data: queueStats } = useQuery({
    queryKey: ['queue-stats', branchId],
    enabled: !!branchId,
    refetchInterval: 15_000,
    staleTime: 10_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<QueueStats>(`/queue/stats?branchId=${branchId}`, token ?? undefined)
    },
  })

  const { data: recentTx } = useQuery({
    queryKey: ['transactions-recent', branchId],
    enabled: !!branchId,
    staleTime: 30_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PagedResult<TransactionSummary>>(
        `/transactions?branchId=${branchId}&page=1&pageSize=5`,
        token ?? undefined,
      )
    },
  })

  const dateLabel = new Date().toLocaleDateString('en-PH', {
    weekday: 'long', month: 'long', day: 'numeric',
  })

  return (
    <div className="p-4 space-y-5 max-w-lg mx-auto">

      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-white">{dateLabel}</h1>
        {!branchId && (
          <p className="text-sm text-yellow-400 mt-1">
            No branch selected —{' '}
            <Link href="/queue" className="underline hover:text-yellow-300">go to Queue</Link>
            {' '}to pick one.
          </p>
        )}
      </div>

      {/* Quick actions */}
      <div className="grid grid-cols-2 gap-3">
        <QuickAction
          href="/transactions/new"
          icon={<Plus className="h-5 w-5" />}
          label="New Transaction"
          sub="Direct service"
          accent="blue"
        />
        <QuickAction
          href="/queue/add"
          icon={<ListOrdered className="h-5 w-5" />}
          label="Add to Queue"
          sub="Queue a vehicle"
          accent="yellow"
        />
        <QuickAction
          href="/queue"
          icon={<LayoutGrid className="h-5 w-5" />}
          label="Queue Board"
          sub={queueStats ? `${queueStats.waitingCount} waiting` : 'Manage queue'}
          accent="green"
        />
        <QuickAction
          href="/customers/lookup"
          icon={<Users className="h-5 w-5" />}
          label="Customer Lookup"
          sub="Search by plate"
          accent="purple"
        />
      </div>

      {/* Today's stats */}
      {branchId && (
        <div>
          <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">
            Today&apos;s Summary
          </h2>
          {summaryLoading ? (
            <div className="grid grid-cols-2 gap-2">
              {Array.from({ length: 4 }).map((_, i) => (
                <div key={i} className="h-[72px] rounded-xl bg-gray-800 animate-pulse" />
              ))}
            </div>
          ) : summary ? (
            <div className="grid grid-cols-2 gap-2">
              <StatCard
                icon={<TrendingUp className="h-4 w-4 text-green-400" />}
                label="Revenue"
                value={fmt(summary.totalRevenue)}
                sub={`${summary.completedTransactions} completed`}
              />
              <StatCard
                icon={<Receipt className="h-4 w-4 text-blue-400" />}
                label="Transactions"
                value={String(summary.totalTransactions)}
                sub={`${summary.pendingTransactions} pending`}
              />
              {queueStats && (
                <>
                  <StatCard
                    icon={<Clock className="h-4 w-4 text-yellow-400" />}
                    label="Queue Waiting"
                    value={String(queueStats.waitingCount)}
                    sub={
                      queueStats.avgWaitMinutes != null
                        ? `~${Math.round(queueStats.avgWaitMinutes)}m avg`
                        : 'in queue'
                    }
                  />
                  <StatCard
                    icon={<Car className="h-4 w-4 text-purple-400" />}
                    label="Served Today"
                    value={String(queueStats.servedToday)}
                    sub={`${queueStats.inServiceCount} in bay`}
                  />
                </>
              )}
            </div>
          ) : null}
        </div>
      )}

      {/* Recent transactions */}
      {recentTx && recentTx.items.length > 0 && (
        <div>
          <div className="flex items-center justify-between mb-2">
            <h2 className="text-xs font-semibold text-gray-500 uppercase tracking-wider">
              Recent
            </h2>
            <Link
              href="/history"
              className="text-xs text-blue-400 hover:text-blue-300 flex items-center gap-0.5"
            >
              View all <ChevronRight className="h-3 w-3" />
            </Link>
          </div>
          <div className="space-y-1.5">
            {recentTx.items.map((tx) => {
              const st = TX_STATUS[tx.status] ?? { label: 'Unknown', cls: 'text-gray-400' }
              return (
                <Link
                  key={tx.id}
                  href={`/transactions/${tx.id}`}
                  className="flex items-center justify-between px-3 py-2.5 rounded-xl bg-gray-800 border border-gray-700 hover:border-gray-600 transition-colors"
                >
                  <div className="min-w-0">
                    <p className="text-sm font-mono font-semibold text-white">{tx.plateNumber}</p>
                    <p className="text-xs text-gray-500 truncate">{tx.transactionNumber}</p>
                  </div>
                  <div className="flex items-center gap-2 shrink-0 ml-2">
                    <span className={cn('text-xs font-medium', st.cls)}>{st.label}</span>
                    <span className="text-sm font-semibold text-white">{fmt(tx.finalAmount)}</span>
                  </div>
                </Link>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}

// ── Sub-components ─────────────────────────────────────────────────────────────

function QuickAction({
  href, icon, label, sub, accent,
}: {
  href: string
  icon: React.ReactNode
  label: string
  sub: string
  accent: 'blue' | 'yellow' | 'green' | 'purple'
}) {
  const accentMap = {
    blue:   'text-blue-400',
    yellow: 'text-yellow-400',
    green:  'text-green-400',
    purple: 'text-purple-400',
  }
  return (
    <Link
      href={href}
      className="flex items-start gap-3 p-4 rounded-xl bg-gray-800 border border-gray-700 hover:border-gray-600 active:scale-95 transition-all min-h-[72px]"
    >
      <span className={cn('mt-0.5 shrink-0', accentMap[accent])}>{icon}</span>
      <div className="min-w-0">
        <p className="text-sm font-semibold text-white leading-tight">{label}</p>
        <p className="text-xs text-gray-400 mt-0.5">{sub}</p>
      </div>
    </Link>
  )
}

function StatCard({
  icon, label, value, sub,
}: {
  icon: React.ReactNode
  label: string
  value: string
  sub: string
}) {
  return (
    <div className="rounded-xl bg-gray-800 border border-gray-700 p-3">
      <div className="flex items-center gap-1.5 mb-1.5">
        {icon}
        <span className="text-xs text-gray-400">{label}</span>
      </div>
      <p className="text-xl font-bold text-white">{value}</p>
      <p className="text-xs text-gray-500">{sub}</p>
    </div>
  )
}
