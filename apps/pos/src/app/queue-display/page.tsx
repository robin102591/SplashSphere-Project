'use client'

import { useEffect, useState, useCallback, useRef, Suspense } from 'react'
import { useSearchParams } from 'next/navigation'
import { Droplets, Users } from 'lucide-react'
import { StatusBadge, type ConnectionState } from '@/components/connection-status'
import type { QueueDisplayUpdatedPayload, QueueDisplayEntry } from '@splashsphere/types'
import { QueuePriority } from '@splashsphere/types'
import { createHubConnection } from '@/lib/signalr'

// ── Clock ─────────────────────────────────────────────────────────────────────

function useClock() {
  // Initialize as null to avoid SSR/client mismatch (hydration error)
  const [time, setTime] = useState<Date | null>(null)
  useEffect(() => {
    setTime(new Date())
    const id = setInterval(() => setTime(new Date()), 1000)
    return () => clearInterval(id)
  }, [])
  return time
}

function Clock() {
  const time = useClock()
  // Render placeholder until client-side time is available
  if (!time) return <div className="w-48" />
  return (
    <div className="text-right tabular-nums">
      <p className="text-4xl font-black font-mono text-white">
        {time.toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
      </p>
      <p className="text-sm text-gray-400 mt-0.5">
        {time.toLocaleDateString('en-PH', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}
      </p>
    </div>
  )
}

// ── Priority helpers ──────────────────────────────────────────────────────────

function priorityBadge(p: QueuePriority) {
  if (p === QueuePriority.Vip)     return { label: 'VIP',     cls: 'bg-purple-500 text-white' }
  if (p === QueuePriority.Express) return { label: 'EXPRESS', cls: 'bg-blue-500 text-white' }
  return null
}

// ── Now Calling cards (animated) ─────────────────────────────────────────────

function CallingCard({ entry }: { entry: QueueDisplayEntry }) {
  const badge = priorityBadge(entry.priority)
  return (
    <div className="rounded-2xl bg-yellow-400 p-6 flex flex-col items-center gap-2 shadow-2xl shadow-yellow-900/40 animate-calling-flash">
      <p className="text-xs font-bold text-yellow-900 uppercase tracking-[0.3em]">Now Calling</p>
      <p className="text-6xl font-black text-yellow-900 tracking-tight leading-none">{entry.queueNumber}</p>
      <p className="text-2xl font-mono font-bold text-yellow-800">{entry.maskedPlate}</p>
      {badge && (
        <span className={`text-xs font-bold px-3 py-0.5 rounded-full ${badge.cls}`}>
          {badge.label}
        </span>
      )}
      <p className="text-sm font-bold text-yellow-900 uppercase tracking-widest mt-1">
        Please proceed to the bay
      </p>
    </div>
  )
}

// ── In-Service table row ──────────────────────────────────────────────────────

function ServiceRow({ entry, index }: { entry: QueueDisplayEntry; index: number }) {
  const badge = priorityBadge(entry.priority)
  return (
    <tr className={index % 2 === 0 ? 'bg-green-950/30' : 'bg-transparent'}>
      <td className="px-5 py-3 font-black text-2xl text-green-300 font-mono tracking-tight">
        {entry.queueNumber}
      </td>
      <td className="px-5 py-3 font-mono font-bold text-xl text-green-400">
        {entry.maskedPlate}
      </td>
      <td className="px-5 py-3">
        {badge ? (
          <span className={`text-xs font-bold px-2.5 py-0.5 rounded-full ${badge.cls}`}>
            {badge.label}
          </span>
        ) : (
          <span className="text-sm text-gray-600">—</span>
        )}
      </td>
      <td className="px-5 py-3 text-sm text-green-600 font-mono">IN SERVICE</td>
    </tr>
  )
}

// ── Main content ──────────────────────────────────────────────────────────────

function QueueDisplayContent() {
  const searchParams = useSearchParams()
  const branchId = searchParams.get('branchId') ?? ''
  const [connState, setConnState] = useState<ConnectionState>('connecting')
  const [displayData, setDisplayData] = useState<QueueDisplayUpdatedPayload | null>(null)
  const serviceScrollRef = useRef<HTMLDivElement>(null)

  // Auto-scroll in-service table
  useEffect(() => {
    const el = serviceScrollRef.current
    if (!el) return
    if (el.scrollHeight <= el.clientHeight + 10) return

    let pos = 0
    let direction = 1
    const id = setInterval(() => {
      if (!el) return
      const max = el.scrollHeight - el.clientHeight
      pos += direction * 0.5
      if (pos >= max) { pos = max; direction = -1 }
      if (pos <= 0)   { pos = 0;   direction = 1 }
      el.scrollTop = pos
    }, 40)
    return () => clearInterval(id)
  }, [displayData?.inService])

  const connect = useCallback(async () => {
    if (!branchId) return

    // Initial data fetch (public — no auth)
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'
      const res = await fetch(`${apiBase}/api/v1/queue/display?branchId=${encodeURIComponent(branchId)}`)
      if (res.ok) {
        const data = (await res.json()) as QueueDisplayUpdatedPayload
        setDisplayData(data)
      }
    } catch {
      // ignore — SignalR will deliver first update
    }

    const conn = createHubConnection() // public — no token
    conn.onclose(() => setConnState('disconnected'))
    conn.onreconnecting(() => setConnState('reconnecting'))
    conn.onreconnected(() => setConnState('connected'))

    try {
      await conn.start()
      setConnState('connected')
      await conn.invoke('JoinQueueDisplay', branchId)
      conn.on('QueueDisplayUpdated', (payload: QueueDisplayUpdatedPayload) => {
        setDisplayData(payload)
      })
    } catch {
      setConnState('disconnected')
    }

    return () => { conn.stop() }
  }, [branchId])

  useEffect(() => {
    const cleanup = connect()
    return () => { cleanup.then(fn => fn?.()) }
  }, [connect])

  const calling    = displayData?.calling      ?? []
  const inService  = displayData?.inService    ?? []
  const waiting    = displayData?.waitingCount ?? 0
  const servedToday = displayData?.servedToday ?? 0
  const avgWait    = displayData?.avgWaitMinutes ?? null

  return (
    <div className="min-h-screen bg-gray-950 flex flex-col select-none">

      {/* ── Reconnecting banner ───────────────────────────────────────────── */}
      {connState === 'reconnecting' && (
        <div className="bg-yellow-500 text-yellow-900 text-center py-2 text-sm font-bold uppercase tracking-wider">
          Reconnecting…
        </div>
      )}
      {connState === 'disconnected' && (
        <div className="bg-red-600 text-white text-center py-2 text-sm font-bold uppercase tracking-wider">
          Connection lost — Trying to reconnect…
        </div>
      )}

      {/* ── Header ──────────────────────────────────────────────────────────── */}
      <header className="flex items-center justify-between px-8 py-5 border-b border-gray-800/60">
        {/* Brand */}
        <div className="flex items-center gap-4">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-blue-500 shadow-lg shadow-blue-900/40">
            <Droplets className="h-8 w-8 text-white" />
          </div>
          <div>
            <h1 className="text-3xl font-black text-white tracking-tight">SplashSphere</h1>
            <p className="text-sm text-gray-500 uppercase tracking-widest">Queue Display</p>
          </div>
        </div>

        {/* Stats + connection + clock */}
        <div className="flex items-center gap-10">
          {/* Waiting count */}
          <div className="text-center">
            <div className="flex items-center gap-2 justify-center">
              <Users className="h-5 w-5 text-gray-500" />
              <p className="text-5xl font-black text-white tabular-nums">{waiting}</p>
            </div>
            <p className="text-xs text-gray-500 uppercase tracking-widest mt-1">Waiting</p>
          </div>

          {/* In service count */}
          <div className="text-center">
            <p className="text-5xl font-black text-green-400 tabular-nums">{inService.length}</p>
            <p className="text-xs text-gray-500 uppercase tracking-widest mt-1">In Service</p>
          </div>

          {/* Connection */}
          <StatusBadge state={connState} className="text-base" />

          <Clock />
        </div>
      </header>

      {/* ── Main grid ───────────────────────────────────────────────────────── */}
      <div className="flex-1 grid grid-cols-5 gap-0 overflow-hidden">

        {/* NOW CALLING — left 2 cols */}
        <section className="col-span-2 border-r border-gray-800/60 p-8 flex flex-col gap-6">
          <div className="flex items-center gap-3">
            <div className="h-4 w-4 rounded-full bg-yellow-400 animate-calling-flash" />
            <h2 className="text-xl font-black text-yellow-400 uppercase tracking-[0.2em]">
              Now Calling
            </h2>
          </div>

          {calling.length === 0 ? (
            <div className="flex-1 flex items-center justify-center">
              <div className="text-center space-y-3">
                <div className="h-24 w-24 rounded-full border-2 border-dashed border-gray-800 flex items-center justify-center mx-auto">
                  <p className="text-4xl font-black text-gray-800">—</p>
                </div>
                <p className="text-lg text-gray-700">No vehicles being called</p>
              </div>
            </div>
          ) : (
            <div className="space-y-5">
              {calling.map(entry => (
                <CallingCard key={entry.queueNumber} entry={entry} />
              ))}
            </div>
          )}
        </section>

        {/* IN SERVICE — right 3 cols */}
        <section className="col-span-3 flex flex-col">
          <div className="flex items-center gap-3 px-8 py-6 border-b border-gray-800/60">
            <div className="h-4 w-4 rounded-full bg-green-500" />
            <h2 className="text-xl font-black text-green-400 uppercase tracking-[0.2em]">
              In Service
            </h2>
            <span className="ml-auto text-sm text-gray-600 uppercase tracking-wider">
              {inService.length} vehicle{inService.length !== 1 ? 's' : ''}
            </span>
          </div>

          {inService.length === 0 ? (
            <div className="flex-1 flex items-center justify-center">
              <p className="text-lg text-gray-700">No vehicles currently in service</p>
            </div>
          ) : (
            <div
              ref={serviceScrollRef}
              className="flex-1 overflow-hidden"
            >
              <table className="w-full">
                <thead className="sticky top-0 bg-gray-900/90 backdrop-blur-sm">
                  <tr className="text-xs text-gray-600 uppercase tracking-widest">
                    <th className="px-5 py-3 text-left font-semibold">Queue #</th>
                    <th className="px-5 py-3 text-left font-semibold">Plate</th>
                    <th className="px-5 py-3 text-left font-semibold">Priority</th>
                    <th className="px-5 py-3 text-left font-semibold">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {inService.map((entry, i) => (
                    <ServiceRow key={entry.queueNumber} entry={entry} index={i} />
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </div>

      {/* ── Footer ────────────────────────────────────────────────────────── */}
      <footer className="border-t border-gray-800/60 px-8 py-4 flex items-center justify-between">
        <p className="text-sm text-gray-600 tracking-wide">
          Please listen for your queue number &nbsp;•&nbsp; Proceed to bay when called
        </p>
        <div className="flex items-center gap-6 text-sm text-gray-500">
          <span>
            Today: <span className="font-bold text-gray-300 font-mono tabular-nums">{servedToday}</span> served
          </span>
          <span>
            Average wait: <span className="font-bold text-gray-300 font-mono tabular-nums">
              {avgWait != null ? `${Math.round(avgWait)} min` : '—'}
            </span>
          </span>
        </div>
      </footer>
    </div>
  )
}

export default function QueueDisplayPage() {
  return (
    <Suspense>
      <QueueDisplayContent />
    </Suspense>
  )
}
