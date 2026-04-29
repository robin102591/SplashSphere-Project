'use client'

import { useEffect, useReducer, useRef } from 'react'
import { useAuth } from '@clerk/nextjs'
import * as signalR from '@microsoft/signalr'
import {
  HubEvents,
  type DisplayCompletionPayload,
  type DisplayTransactionPayload,
} from '@splashsphere/types'

/** Discriminated union of what's currently on the screen. */
export type DisplayState =
  | { kind: 'idle' }
  | { kind: 'building'; transaction: DisplayTransactionPayload }
  | { kind: 'complete'; completion: DisplayCompletionPayload }

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

type Action =
  | { type: 'started'; transaction: DisplayTransactionPayload }
  | { type: 'updated'; transaction: DisplayTransactionPayload }
  | { type: 'completed'; completion: DisplayCompletionPayload }
  | { type: 'cancelled' }
  | { type: 'completion-timeout' }

function reducer(state: DisplayState, action: Action): DisplayState {
  switch (action.type) {
    case 'started':
    case 'updated':
      return { kind: 'building', transaction: action.transaction }
    case 'completed':
      return { kind: 'complete', completion: action.completion }
    case 'cancelled':
    case 'completion-timeout':
      return { kind: 'idle' }
  }
}

const HUB_URL = `${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'}/hubs/notifications`

/**
 * Connects the customer display to SignalR and returns the live transaction
 * state plus a connection-status indicator. Auto-reconnects with the SDK's
 * built-in exponential backoff. The completion screen auto-reverts to Idle
 * after `completionHoldSeconds` (configured via display settings).
 */
export function useDisplayConnection({
  branchId,
  stationId,
  completionHoldSeconds,
}: {
  branchId: string
  stationId: string
  completionHoldSeconds: number
}): {
  state: DisplayState
  connection: ConnectionStatus
} {
  const { getToken } = useAuth()
  const [state, dispatch] = useReducer(reducer, { kind: 'idle' } as DisplayState)
  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const statusRef = useRef<ConnectionStatus>('connecting')
  const [, force] = useReducer((x: number) => x + 1, 0)

  // Auto-revert to Idle after the hold timer when on the completion screen.
  useEffect(() => {
    if (state.kind !== 'complete') return
    const timer = setTimeout(
      () => dispatch({ type: 'completion-timeout' }),
      Math.max(1000, completionHoldSeconds * 1000),
    )
    return () => clearTimeout(timer)
  }, [state, completionHoldSeconds])

  useEffect(() => {
    if (!branchId || !stationId) return

    let cancelled = false
    const setStatus = (s: ConnectionStatus) => {
      statusRef.current = s
      force()
    }

    const start = async () => {
      const token = await getToken()
      if (!token) {
        setStatus('disconnected')
        return
      }

      const conn = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL, { accessTokenFactory: () => token })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build()

      conn.onreconnecting(() => setStatus('reconnecting'))
      conn.onreconnected(async () => {
        // Re-join the group after a reconnect — group memberships are
        // dropped on connection loss.
        try {
          await conn.invoke('JoinDisplayGroup', branchId, stationId)
          setStatus('connected')
        } catch {
          setStatus('disconnected')
        }
      })
      conn.onclose(() => setStatus('disconnected'))

      conn.on(HubEvents.DisplayTransactionStarted, (p: DisplayTransactionPayload) =>
        dispatch({ type: 'started', transaction: p }),
      )
      conn.on(HubEvents.DisplayTransactionUpdated, (p: DisplayTransactionPayload) =>
        dispatch({ type: 'updated', transaction: p }),
      )
      conn.on(HubEvents.DisplayTransactionCompleted, (p: DisplayCompletionPayload) =>
        dispatch({ type: 'completed', completion: p }),
      )
      conn.on(HubEvents.DisplayTransactionCancelled, () =>
        dispatch({ type: 'cancelled' }),
      )

      try {
        await conn.start()
        if (cancelled) {
          await conn.stop()
          return
        }
        await conn.invoke('JoinDisplayGroup', branchId, stationId)
        setStatus('connected')
        connectionRef.current = conn
      } catch {
        setStatus('disconnected')
      }
    }

    void start()

    return () => {
      cancelled = true
      const conn = connectionRef.current
      connectionRef.current = null
      if (conn) {
        conn.invoke('LeaveDisplayGroup', branchId, stationId).catch(() => {})
        conn.stop().catch(() => {})
      }
    }
  }, [branchId, stationId, getToken])

  return { state, connection: statusRef.current }
}
