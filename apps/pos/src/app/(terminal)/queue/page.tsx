'use client'

import { useEffect, useState } from 'react'
import { useAuth } from '@clerk/nextjs'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import {
  Plus, Clock, PhoneCall, AlertTriangle, Timer,
  Wrench, Eye, RefreshCw,
} from 'lucide-react'
import type { QueueEntry, ServiceSummary, QueueStats } from '@splashsphere/types'
import type { PagedResult, QueueUpdatedPayload } from '@splashsphere/types'
import { QueueStatus, QueuePriority } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { useSignalREvent } from '@/lib/signalr-context'
import { useBranch } from '@/lib/branch-context'
import { ConnectionStatusDot } from '@/components/connection-status'
import { useCurrentShift, isShiftOpen } from '@/lib/use-shift'

// ── Priority config ────────────────────────────────────────────────────────────

const PRIORITY: Record<QueuePriority, { label: string; badgeCls: string; dotCls: string }> = {
  [QueuePriority.Regular]: {
    label: 'Regular',
    badgeCls: 'bg-gray-700 text-gray-300',
    dotCls: 'bg-gray-400',
  },
  [QueuePriority.Express]: {
    label: 'Express',
    badgeCls: 'bg-blue-500/20 text-blue-300 ring-1 ring-blue-500/40',
    dotCls: 'bg-blue-400',
  },
  [QueuePriority.Vip]: {
    label: 'VIP',
    badgeCls: 'bg-purple-500/20 text-purple-300 ring-1 ring-purple-500/40',
    dotCls: 'bg-purple-400',
  },
}

// ── Live timers ────────────────────────────────────────────────────────────────

function LiveCountdown({ calledAt }: { calledAt: string }) {
  const [, forceUpdate] = useState(0)
  useEffect(() => {
    const id = setInterval(() => forceUpdate(n => n + 1), 1000)
    return () => clearInterval(id)
  }, [])

  const deadline = new Date(calledAt).getTime() + 5 * 60 * 1000
  const remaining = Math.max(0, deadline - Date.now())
  const overdue = remaining === 0
  const urgent = !overdue && remaining < 60_000
  const mins = Math.floor(remaining / 60_000)
  const secs = Math.floor((remaining % 60_000) / 1000)

  return (
    <span
      className={`text-xs font-mono font-bold ${
        overdue ? 'text-red-400 animate-pulse' : urgent ? 'text-orange-400' : 'text-yellow-400'
      }`}
    >
      {overdue ? 'OVERDUE' : `${mins}:${String(secs).padStart(2, '0')}`}
    </span>
  )
}

function LiveElapsed({ startedAt }: { startedAt: string }) {
  const [, forceUpdate] = useState(0)
  useEffect(() => {
    const id = setInterval(() => forceUpdate(n => n + 1), 1000)
    return () => clearInterval(id)
  }, [])

  const elapsed = Date.now() - new Date(startedAt).getTime()
  const hours = Math.floor(elapsed / 3_600_000)
  const mins = Math.floor((elapsed % 3_600_000) / 60_000)
  const secs = Math.floor((elapsed % 60_000) / 1000)
  const text = hours > 0 ? `${hours}h ${mins}m` : `${mins}:${String(secs).padStart(2, '0')}`

  return <span className="text-xs font-mono text-green-400">{text}</span>
}

// ── Queue card ─────────────────────────────────────────────────────────────────

interface QueueCardProps {
  entry: QueueEntry
  serviceNames: Map<string, string>
  onCall: (id: string) => void
  onNoShow: (id: string) => void
  onStartService: (id: string) => void
  onViewTransaction: (transactionId: string) => void
  isBusy: boolean
}

function parseServiceIds(raw: string | null): string[] {
  if (!raw) return []
  try { return JSON.parse(raw) as string[] } catch { return [] }
}

function QueueCard({
  entry,
  serviceNames,
  onCall,
  onNoShow,
  onStartService,
  onViewTransaction,
  isBusy,
}: QueueCardProps) {
  const priority = PRIORITY[entry.priority]
  const serviceIds = parseServiceIds(entry.preferredServices)
  const serviceLabels = serviceIds.map(id => serviceNames.get(id) ?? '…').slice(0, 3)
  const overflow = serviceIds.length > 3

  const cardCls =
    entry.status === QueueStatus.Called
      ? 'bg-yellow-950/40 border-yellow-700/50 shadow-lg shadow-yellow-900/20'
      : entry.status === QueueStatus.InService
        ? 'bg-green-950/40 border-green-700/50'
        : 'bg-gray-800 border-gray-700 hover:border-gray-600'

  return (
    <div className={`rounded-xl p-4 space-y-3 border transition-colors ${cardCls}`}>
      {/* Top row: queue number + timer + priority */}
      <div className="flex items-start justify-between gap-2">
        <div className="flex items-center gap-2 min-w-0">
          <span className="text-2xl font-black text-white tracking-tight shrink-0">
            {entry.queueNumber}
          </span>
          {entry.status === QueueStatus.Called && entry.calledAt && (
            <div className="flex items-center gap-1">
              <Clock className="h-3 w-3 text-yellow-400 shrink-0" />
              <LiveCountdown calledAt={entry.calledAt} />
            </div>
          )}
          {entry.status === QueueStatus.InService && entry.startedAt && (
            <div className="flex items-center gap-1">
              <Timer className="h-3 w-3 text-green-400 shrink-0" />
              <LiveElapsed startedAt={entry.startedAt} />
            </div>
          )}
        </div>
        <span className={`shrink-0 text-xs px-2 py-0.5 rounded-full font-semibold ${priority.badgeCls}`}>
          {priority.label}
        </span>
      </div>

      {/* Plate + customer */}
      <div className="space-y-0.5">
        <p className="text-base font-mono font-bold text-white tracking-widest">{entry.plateNumber}</p>
        {entry.customerFullName && (
          <p className="text-xs text-gray-400">{entry.customerFullName}</p>
        )}
      </div>

      {/* Preferred services */}
      {serviceLabels.length > 0 && (
        <div className="flex flex-wrap gap-1">
          {serviceLabels.map((name, i) => (
            <span key={i} className="text-xs bg-gray-700/80 text-gray-300 px-2 py-0.5 rounded-md">
              {name}
            </span>
          ))}
          {overflow && (
            <span className="text-xs text-gray-500">+{serviceIds.length - 3} more</span>
          )}
        </div>
      )}

      {/* Notes */}
      {entry.notes && (
        <p className="text-xs text-gray-500 italic line-clamp-2">{entry.notes}</p>
      )}

      {/* Wait time (waiting only) */}
      {entry.status === QueueStatus.Waiting && entry.estimatedWaitMinutes != null && (
        <p className="text-xs text-gray-500">~{entry.estimatedWaitMinutes} min wait</p>
      )}

      {/* Actions */}
      <div className="flex gap-2 pt-0.5">
        {entry.status === QueueStatus.Waiting && (
          <button
            onClick={() => onCall(entry.id)}
            disabled={isBusy}
            className="flex-1 flex items-center justify-center gap-1.5 min-h-[44px] rounded-lg bg-yellow-600 hover:bg-yellow-500 disabled:opacity-50 text-white text-sm font-semibold transition-colors duration-150 active:scale-[0.97]"
          >
            {isBusy ? <RefreshCw className="h-3.5 w-3.5 animate-spin" /> : <PhoneCall className="h-3.5 w-3.5" />}
            Call
          </button>
        )}

        {entry.status === QueueStatus.Called && (
          <>
            <button
              onClick={() => onStartService(entry.id)}
              disabled={isBusy}
              className="flex-1 flex items-center justify-center gap-1.5 min-h-[44px] rounded-lg bg-green-600 hover:bg-green-500 disabled:opacity-50 text-white text-sm font-semibold transition-colors duration-150 active:scale-[0.97]"
            >
              <Wrench className="h-3.5 w-3.5" />
              Start Service
            </button>
            <button
              onClick={() => onNoShow(entry.id)}
              disabled={isBusy}
              title="Mark as no-show"
              className="flex items-center justify-center min-h-[44px] px-3 rounded-lg bg-gray-700 hover:bg-red-900/50 border border-gray-600 hover:border-red-700 disabled:opacity-50 text-gray-400 hover:text-red-400 transition-colors duration-150 active:scale-[0.97]"
            >
              {isBusy ? <RefreshCw className="h-3.5 w-3.5 animate-spin" /> : <AlertTriangle className="h-3.5 w-3.5" />}
            </button>
          </>
        )}

        {entry.status === QueueStatus.InService && entry.transactionId && (
          <button
            onClick={() => onViewTransaction(entry.transactionId!)}
            className="flex-1 flex items-center justify-center gap-1.5 min-h-[44px] rounded-lg bg-gray-700 hover:bg-gray-600 text-gray-300 text-sm font-semibold transition-colors duration-150 active:scale-[0.97]"
          >
            <Eye className="h-3.5 w-3.5" />
            View Transaction
          </button>
        )}
      </div>
    </div>
  )
}

// ── Column ─────────────────────────────────────────────────────────────────────

function Column({
  title,
  count,
  dotCls,
  children,
}: {
  title: string
  count: number
  dotCls: string
  children: React.ReactNode
}) {
  return (
    <div className="flex flex-col min-h-0 overflow-hidden">
      <div className="flex items-center gap-2 px-1 pb-3 shrink-0">
        <div className={`h-2.5 w-2.5 rounded-full shrink-0 ${dotCls}`} />
        <h2 className="font-semibold text-gray-200 truncate">{title}</h2>
        <span className="ml-auto text-sm text-gray-500 bg-gray-800/80 px-2 py-0.5 rounded-full shrink-0">
          {count}
        </span>
      </div>
      <div className="space-y-3 overflow-y-auto flex-1 min-h-0 pr-0.5">{children}</div>
    </div>
  )
}

// ── Main page ──────────────────────────────────────────────────────────────────

export default function QueuePage() {
  const { getToken } = useAuth()
  const router = useRouter()
  const queryClient = useQueryClient()
  const { branchId } = useBranch()
  const { data: currentShift } = useCurrentShift()
  const shiftOpen = isShiftOpen(currentShift)
  const [busyId, setBusyId] = useState<string | null>(null)

  // Queue entries — only fetch when a branch is selected
  const {
    data: entries = [],
    refetch,
    isFetching,
  } = useQuery({
    queryKey: ['queue', branchId],
    enabled: !!branchId,
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<QueueEntry>>(
        `/queue?branchId=${encodeURIComponent(branchId)}&pageSize=200`,
        token ?? undefined,
      )
      return res.items as QueueEntry[]
    },
    staleTime: 3_000,
    refetchInterval: 30_000,
  })

  // Services for name lookup
  const { data: services = [] } = useQuery({
    queryKey: ['services-compact'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<ServiceSummary>>('/services?pageSize=100', token ?? undefined)
      return res.items as ServiceSummary[]
    },
  })
  const serviceNames = new Map(services.map(s => [s.id, s.name]))

  // Queue stats for top bar
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

  // Real-time queue updates via shared SignalR context
  useSignalREvent<QueueUpdatedPayload>('QueueUpdated', () => {
    void Promise.all([
      queryClient.invalidateQueries({ queryKey: ['queue'] }),
      queryClient.invalidateQueries({ queryKey: ['queue-stats'] }),
    ])
  })

  // ── Actions ─────────────────────────────────────────────────────────────────

  const callEntry = async (id: string) => {
    setBusyId(id)
    try {
      const token = await getToken()
      // Endpoint {id} is branchId — it picks the highest-priority waiting entry automatically
      await apiClient.patch(`/queue/${branchId}/call`, {}, token ?? undefined)
      await refetch()
    } finally {
      setBusyId(null)
    }
  }

  const noShowEntry = async (id: string) => {
    setBusyId(id)
    try {
      const token = await getToken()
      await apiClient.patch(`/queue/${id}/no-show`, {}, token ?? undefined)
      await refetch()
    } finally {
      setBusyId(null)
    }
  }

  const startService = (id: string) => {
    router.push(`/transactions/new?queueEntryId=${encodeURIComponent(id)}`)
  }

  const viewTransaction = (transactionId: string) => {
    router.push(`/transactions/${transactionId}`)
  }

  // ── Split by status (priority-sorted for waiting) ───────────────────────────

  const waiting = [...entries]
    .filter(e => e.status === QueueStatus.Waiting)
    .sort((a, b) => b.priority - a.priority || new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())

  const called = [...entries]
    .filter(e => e.status === QueueStatus.Called)
    .sort((a, b) =>
      new Date(a.calledAt ?? a.createdAt).getTime() - new Date(b.calledAt ?? b.createdAt).getTime()
    )

  const inService = [...entries]
    .filter(e => e.status === QueueStatus.InService)
    .sort((a, b) =>
      new Date(a.startedAt ?? a.createdAt).getTime() - new Date(b.startedAt ?? b.createdAt).getTime()
    )

  return (
    <div className="flex flex-col overflow-hidden" style={{ height: 'calc(100vh - 3.5rem - 3.5rem)' }}>
      {/* ── Header ──────────────────────────────────────────────────────────── */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-gray-800 shrink-0">
        <div className="flex items-center gap-4">
          <div>
            <h1 className="text-lg font-bold text-white leading-tight">Queue Board</h1>
          </div>
          <div className="flex items-center gap-1.5">
            {isFetching && <RefreshCw className="h-3.5 w-3.5 text-gray-600 animate-spin" />}
            <ConnectionStatusDot />
          </div>
          {/* Stats pills */}
          <div className="hidden sm:flex items-center gap-3 text-sm">
            <span className="text-gray-400">
              Waiting: <span className="font-bold text-white font-mono tabular-nums">{waiting.length}</span>
            </span>
            <span className="text-gray-700">|</span>
            <span className="text-gray-400">
              Avg Wait: <span className="font-bold text-white font-mono tabular-nums">
                {queueStats?.avgWaitMinutes != null ? `${Math.round(queueStats.avgWaitMinutes)} min` : '—'}
              </span>
            </span>
            <span className="text-gray-700">|</span>
            <span className="text-gray-400">
              Served Today: <span className="font-bold text-white font-mono tabular-nums">{queueStats?.servedToday ?? 0}</span>
            </span>
          </div>
        </div>

        {shiftOpen ? (
          <Link
            href="/queue/add"
            className="flex items-center gap-2 px-5 min-h-[44px] rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-semibold text-sm transition-colors duration-150 active:scale-[0.97] shrink-0"
          >
            <Plus className="h-4 w-4" />
            Add to Queue
          </Link>
        ) : (
          <span
            title="Open a shift first"
            className="flex items-center gap-2 px-5 min-h-[44px] rounded-xl bg-gray-700 text-gray-500 font-semibold text-sm cursor-not-allowed shrink-0"
          >
            <Plus className="h-4 w-4" />
            Add to Queue
          </span>
        )}
      </div>

      {/* ── Kanban ──────────────────────────────────────────────────────────── */}
      <div className="flex-1 grid grid-cols-3 gap-4 p-4 min-h-0 overflow-hidden">
        <Column title="Waiting" count={waiting.length} dotCls="bg-gray-400">
          {waiting.length === 0 ? (
            <p className="text-center text-sm text-gray-700 py-10">No vehicles waiting</p>
          ) : (
            waiting.map(entry => (
              <QueueCard
                key={entry.id}
                entry={entry}
                serviceNames={serviceNames}
                onCall={callEntry}
                onNoShow={noShowEntry}
                onStartService={startService}
                onViewTransaction={viewTransaction}
                isBusy={busyId === entry.id}
              />
            ))
          )}
        </Column>

        <Column title="Called" count={called.length} dotCls="bg-yellow-400">
          {called.length === 0 ? (
            <p className="text-center text-sm text-gray-700 py-10">No vehicles called</p>
          ) : (
            called.map(entry => (
              <QueueCard
                key={entry.id}
                entry={entry}
                serviceNames={serviceNames}
                onCall={callEntry}
                onNoShow={noShowEntry}
                onStartService={startService}
                onViewTransaction={viewTransaction}
                isBusy={busyId === entry.id}
              />
            ))
          )}
        </Column>

        <Column title="In Service" count={inService.length} dotCls="bg-green-500">
          {inService.length === 0 ? (
            <p className="text-center text-sm text-gray-700 py-10">No vehicles in service</p>
          ) : (
            inService.map(entry => (
              <QueueCard
                key={entry.id}
                entry={entry}
                serviceNames={serviceNames}
                onCall={callEntry}
                onNoShow={noShowEntry}
                onStartService={startService}
                onViewTransaction={viewTransaction}
                isBusy={busyId === entry.id}
              />
            ))
          )}
        </Column>
      </div>
    </div>
  )
}
