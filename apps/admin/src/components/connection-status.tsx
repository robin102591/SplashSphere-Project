'use client'

import { useSignalRStatus, type ConnectionState } from '@/lib/signalr-context'
import { cn } from '@/lib/utils'

// ── Dot ─────────────────────────────────────────────────────────────────────

export function ConnectionStatusDot({ className }: { className?: string }) {
  const state = useSignalRStatus()
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
  const cls = COLOR[state]
  return (
    <span className={cn('flex items-center gap-1.5 text-xs font-medium', cls, className)}>
      <ConnectionStatusDot />
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
  connected:    'text-green-600',
  reconnecting: 'text-amber-500',
  connecting:   'text-amber-500',
  disconnected: 'text-destructive',
}
