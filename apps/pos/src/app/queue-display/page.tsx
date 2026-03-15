'use client'

import { useEffect, useState, useCallback, Suspense } from 'react'
import { useSearchParams } from 'next/navigation'
import { Droplets, Wifi, WifiOff } from 'lucide-react'
import type { QueueDisplayUpdatedPayload } from '@splashsphere/types'
import { QueueStatus, QueuePriority } from '@splashsphere/types'
import { createHubConnection } from '@/lib/signalr'

function useClock() {
  const [time, setTime] = useState(() => new Date())
  useEffect(() => {
    const timer = setInterval(() => setTime(new Date()), 1000)
    return () => clearInterval(timer)
  }, [])
  return time
}

function Clock() {
  const time = useClock()
  return (
    <div className="text-right">
      <p className="text-3xl font-bold font-mono text-white tabular-nums">
        {time.toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
      </p>
      <p className="text-sm text-gray-400">
        {time.toLocaleDateString('en-PH', { weekday: 'long', month: 'long', day: 'numeric' })}
      </p>
    </div>
  )
}

interface EntryCardProps {
  queueNumber: string
  maskedPlate: string
  priority: QueuePriority
  status: QueueStatus
}

function priorityLabel(p: QueuePriority): string | null {
  if (p === QueuePriority.Vip) return 'VIP'
  if (p === QueuePriority.Express) return 'EXPRESS'
  return null
}

function CallingCard({ queueNumber, maskedPlate, priority }: EntryCardProps) {
  const label = priorityLabel(priority)
  return (
    <div className="rounded-2xl bg-yellow-500 animate-pulse p-6 flex flex-col items-center gap-2 shadow-2xl shadow-yellow-900/50">
      <div className="text-5xl font-black text-yellow-900 tracking-tight">{queueNumber}</div>
      <div className="text-xl font-mono font-semibold text-yellow-800">{maskedPlate}</div>
      {label && (
        <span className="text-xs bg-yellow-800/30 text-yellow-900 px-2 py-0.5 rounded-full font-bold">
          {label}
        </span>
      )}
      <p className="text-sm font-bold text-yellow-900 uppercase tracking-widest mt-1">Please proceed to bay</p>
    </div>
  )
}

function InServiceCard({ queueNumber, maskedPlate, priority }: EntryCardProps) {
  const label = priorityLabel(priority)
  return (
    <div className="rounded-2xl bg-green-900/60 border border-green-700 p-5 flex flex-col items-center gap-1">
      <div className="text-4xl font-black text-green-300 tracking-tight">{queueNumber}</div>
      <div className="text-lg font-mono font-semibold text-green-400">{maskedPlate}</div>
      {label && (
        <span className="text-xs bg-green-800/50 text-green-300 px-2 py-0.5 rounded-full font-bold">
          {label}
        </span>
      )}
    </div>
  )
}

function QueueDisplayContent() {
  const searchParams = useSearchParams()
  const branchId = searchParams.get('branchId') ?? ''
  const [connected, setConnected] = useState(false)
  const [displayData, setDisplayData] = useState<QueueDisplayUpdatedPayload | null>(null)

  const connect = useCallback(async () => {
    if (!branchId) return

    // Fetch initial data
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'
      const res = await fetch(`${apiBase}/api/v1/queue/display?branchId=${branchId}`)
      if (res.ok) {
        const data = await res.json() as QueueDisplayUpdatedPayload
        setDisplayData(data)
      }
    } catch {
      // ignore — SignalR will deliver updates
    }

    // Connect SignalR
    const conn = createHubConnection() // public — no token
    conn.onclose(() => setConnected(false))
    conn.onreconnecting(() => setConnected(false))
    conn.onreconnected(() => setConnected(true))

    try {
      await conn.start()
      setConnected(true)
      await conn.invoke('JoinQueueDisplay', branchId)
      conn.on('QueueDisplayUpdated', (payload: QueueDisplayUpdatedPayload) => {
        setDisplayData(payload)
      })
    } catch {
      setConnected(false)
    }

    return () => {
      conn.stop()
    }
  }, [branchId])

  useEffect(() => {
    const cleanup = connect()
    return () => {
      cleanup.then((fn) => fn?.())
    }
  }, [connect])

  const calling = displayData?.calling ?? []
  const inService = displayData?.inService ?? []
  const waitingCount = displayData?.waitingCount ?? 0

  return (
    <div className="min-h-screen bg-gray-950 flex flex-col select-none">
      {/* Header */}
      <header className="flex items-center justify-between px-8 py-5 border-b border-gray-800">
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-500">
            <Droplets className="h-7 w-7 text-white" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-white">SplashSphere</h1>
            <p className="text-sm text-gray-400">Queue Display</p>
          </div>
        </div>

        <div className="flex items-center gap-6">
          {/* Waiting count */}
          <div className="text-center">
            <p className="text-4xl font-black text-white tabular-nums">{waitingCount}</p>
            <p className="text-xs text-gray-400 uppercase tracking-wider">Waiting</p>
          </div>

          {/* Connection indicator */}
          <div className="flex items-center gap-1.5">
            {connected ? (
              <Wifi className="h-4 w-4 text-green-500" />
            ) : (
              <WifiOff className="h-4 w-4 text-red-500 animate-pulse" />
            )}
            <span className={`text-xs ${connected ? 'text-green-500' : 'text-red-400'}`}>
              {connected ? 'Live' : 'Reconnecting…'}
            </span>
          </div>

          <Clock />
        </div>
      </header>

      {/* Main content */}
      <div className="flex-1 grid grid-cols-2 gap-6 p-8">
        {/* NOW CALLING column */}
        <div className="space-y-4">
          <div className="flex items-center gap-2">
            <div className="h-3 w-3 rounded-full bg-yellow-400 animate-pulse" />
            <h2 className="text-lg font-bold text-yellow-400 uppercase tracking-widest">Now Calling</h2>
          </div>
          <div className="space-y-4">
            {calling.length === 0 ? (
              <div className="rounded-2xl border border-dashed border-gray-800 h-40 flex items-center justify-center">
                <p className="text-gray-700 text-lg">No vehicles being called</p>
              </div>
            ) : (
              calling.map((entry) => (
                <CallingCard
                  key={entry.queueNumber}
                  queueNumber={entry.queueNumber}
                  maskedPlate={entry.maskedPlate}
                  priority={entry.priority}
                  status={QueueStatus.Called}
                />
              ))
            )}
          </div>
        </div>

        {/* IN SERVICE column */}
        <div className="space-y-4">
          <div className="flex items-center gap-2">
            <div className="h-3 w-3 rounded-full bg-green-500" />
            <h2 className="text-lg font-bold text-green-400 uppercase tracking-widest">In Service</h2>
          </div>
          <div className="grid grid-cols-2 gap-3">
            {inService.length === 0 ? (
              <div className="col-span-2 rounded-2xl border border-dashed border-gray-800 h-40 flex items-center justify-center">
                <p className="text-gray-700 text-lg">No vehicles in service</p>
              </div>
            ) : (
              inService.map((entry) => (
                <InServiceCard
                  key={entry.queueNumber}
                  queueNumber={entry.queueNumber}
                  maskedPlate={entry.maskedPlate}
                  priority={entry.priority}
                  status={QueueStatus.InService}
                />
              ))
            )}
          </div>
        </div>
      </div>

      {/* Footer ticker */}
      <footer className="border-t border-gray-800 px-8 py-3">
        <p className="text-sm text-gray-600 text-center">
          Please wait for your queue number to be called • Thank you for choosing SplashSphere
        </p>
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
