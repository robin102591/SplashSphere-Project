'use client'

import { useSignalRStatus, type ConnectionState } from '@/lib/signalr-context'
export type { ConnectionState }
import { cn } from '@/lib/utils'

// ── Dot ─────────────────────────────────────────────────────────────────────

export function ConnectionStatusDot({ className }: { className?: string }) {
  const state = useSignalRStatus()
  return <StatusDot state={state} className={className} />
}

/** Standalone dot — accepts state directly (useful outside provider, e.g. queue display). */
export function StatusDot({
  state, className,
}: {
  state: ConnectionState
  className?: string
}) {
  return (
    <span
      title={LABEL[state]}
      className={cn(
        'inline-block h-2 w-2 rounded-full transition-colors shrink-0',
        state === 'connected'                               && 'bg-green-500',
        state === 'reconnecting' || state === 'connecting' ? 'bg-yellow-500 animate-pulse' : '',
        state === 'disconnected'                            && 'bg-red-500',
        className,
      )}
    />
  )
}

// ── Badge (dot + label) ────────────────────────────────────────────────────

export function ConnectionStatusBadge({ className }: { className?: string }) {
  const state = useSignalRStatus()
  return <StatusBadge state={state} className={className} />
}

export function StatusBadge({
  state, className,
}: {
  state: ConnectionState
  className?: string
}) {
  const cls = COLOR[state]
  return (
    <span className={cn('flex items-center gap-1.5 text-xs font-medium', cls, className)}>
      <StatusDot state={state} />
      {LABEL[state]}
    </span>
  )
}

// ── Helpers ────────────────────────────────────────────────────────────────

const LABEL: Record<ConnectionState, string> = {
  connected:    'Live',
  reconnecting: 'Reconnecting',
  connecting:   'Connecting',
  disconnected: 'Offline',
}

const COLOR: Record<ConnectionState, string> = {
  connected:    'text-green-400',
  reconnecting: 'text-yellow-400',
  connecting:   'text-yellow-400',
  disconnected: 'text-red-400',
}
