'use client'

import { useEffect, useReducer, useRef } from 'react'
import { useAuth } from '@clerk/nextjs'
import * as signalR from '@microsoft/signalr'
import {
  HubEvents,
  type DisplayCompletionPayload,
  type DisplayCurrentResultDto,
  type DisplayTransactionPayload,
} from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

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
 *
 * Auth model: waits for Clerk's `isLoaded` before attempting any connection
 * — calling `getToken()` before that returns null and would put us in a
 * fake "disconnected" state forever. The token is fetched lazily via
 * SignalR's `accessTokenFactory` so every reconnect grabs a fresh JWT.
 *
 * Effect deps: only branchId / stationId / isSignedIn / isLoaded. `getToken`
 * is stashed in a ref because its function identity changes on auth-state
 * shifts and would otherwise tear down + rebuild the connection on every
 * render.
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
  const { getToken, isLoaded, isSignedIn } = useAuth()
  const [state, dispatch] = useReducer(reducer, { kind: 'idle' } as DisplayState)
  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const statusRef = useRef<ConnectionStatus>('connecting')
  const [, force] = useReducer((x: number) => x + 1, 0)

  // Keep getToken in a ref so the connect effect doesn't re-fire on each
  // render. We still want the freshest function the next time we call it.
  const getTokenRef = useRef(getToken)
  useEffect(() => { getTokenRef.current = getToken }, [getToken])

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

    // Wait for Clerk to finish hydrating before we try to connect.
    if (!isLoaded) return
    if (!isSignedIn) {
      statusRef.current = 'disconnected'
      force()
      return
    }

    let cancelled = false
    const setStatus = (s: ConnectionStatus) => {
      if (cancelled) return
      statusRef.current = s
      force()
    }

    /**
     * Pulls the current in-progress transaction (if any) from the API and
     * rehydrates the reducer. Used both on first connect and after a
     * reconnect — events fired between the drop and reconnect would
     * otherwise leave the screen out of sync.
     */
    const rehydrate = async (): Promise<void> => {
      try {
        const token = await getTokenRef.current()
        const path = `/display/current?branchId=${encodeURIComponent(branchId)}&stationId=${encodeURIComponent(stationId)}`
        const result = await apiClient.get<DisplayCurrentResultDto>(path, token ?? undefined)
        if (cancelled) return
        if (result.transaction) {
          dispatch({ type: 'updated', transaction: result.transaction })
        }
        // No active transaction → leave whatever state we had. Don't dispatch
        // 'cancelled' here: it would force-clear a Complete screen mid-hold,
        // which would feel jumpy on a fast reconnect.
      } catch (err) {
        console.warn('[display] rehydrate failed', err)
      }
    }

    const conn = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        // Fresh token per connection attempt so SignalR's auto-reconnect
        // doesn't reuse an expired JWT.
        accessTokenFactory: async () => (await getTokenRef.current()) ?? '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    conn.onreconnecting(() => setStatus('reconnecting'))
    conn.onreconnected(async () => {
      try {
        await conn.invoke('JoinDisplayGroup', branchId, stationId)
        setStatus('connected')
        await rehydrate()
      } catch (err) {
        console.warn('[display] rejoin after reconnect failed', err)
        setStatus('disconnected')
      }
    })
    conn.onclose((err) => {
      if (err) console.warn('[display] connection closed with error', err)
      setStatus('disconnected')
    })

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

    connectionRef.current = conn

    void (async () => {
      try {
        await conn.start()
        if (cancelled) {
          await conn.stop()
          return
        }
        await conn.invoke('JoinDisplayGroup', branchId, stationId)
        setStatus('connected')
        // Initial rehydrate — covers the case where a transaction was already
        // in progress before the display device booted.
        await rehydrate()
      } catch (err) {
        console.error('[display] failed to start connection or join group', err)
        setStatus('disconnected')
      }
    })()

    return () => {
      cancelled = true
      const c = connectionRef.current
      connectionRef.current = null
      if (c) {
        // Best-effort leave — silent if connection isn't open yet.
        if (c.state === signalR.HubConnectionState.Connected) {
          c.invoke('LeaveDisplayGroup', branchId, stationId).catch(() => {})
        }
        c.stop().catch(() => {})
      }
    }
  }, [branchId, stationId, isLoaded, isSignedIn])

  return { state, connection: statusRef.current }
}
