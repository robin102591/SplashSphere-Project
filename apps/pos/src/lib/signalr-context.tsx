'use client'

import {
  createContext, useCallback, useContext,
  useEffect, useRef, useState,
} from 'react'
import * as signalR from '@microsoft/signalr'
import { useAuth } from '@clerk/nextjs'

// ── Types ──────────────────────────────────────────────────────────────────────

export type ConnectionState = 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

interface SignalRContextValue {
  state: ConnectionState
  /** Register an event handler. Returns an unsubscribe function. */
  on: (event: string, handler: (...args: unknown[]) => void) => () => void
  /** Invoke a hub method. No-ops if not connected. */
  invoke: (method: string, ...args: unknown[]) => void
}

// ── Context ────────────────────────────────────────────────────────────────────

const SignalRContext = createContext<SignalRContextValue>({
  state: 'disconnected',
  on: () => () => {},
  invoke: () => {},
})

// ── Provider ───────────────────────────────────────────────────────────────────

const HUB_URL = `${process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'}/hubs/notifications`

export function SignalRProvider({ children }: { children: React.ReactNode }) {
  const { getToken } = useAuth()
  const [state, setState] = useState<ConnectionState>('connecting')
  const connRef = useRef<signalR.HubConnection | null>(null)

  useEffect(() => {
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        // accessTokenFactory is called on every connect / reconnect
        accessTokenFactory: async () => {
          const token = await getToken()
          return token ?? ''
        },
      })
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    connRef.current = conn

    conn.onclose(() => setState('disconnected'))
    conn.onreconnecting(() => setState('reconnecting'))
    conn.onreconnected(() => setState('connected'))

    conn.start()
      .then(() => setState('connected'))
      .catch(() => setState('disconnected'))

    return () => {
      conn.stop()
      connRef.current = null
    }
  // getToken is stable across renders; intentionally omitted to run once
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const on = useCallback((event: string, handler: (...args: unknown[]) => void) => {
    connRef.current?.on(event, handler)
    return () => {
      connRef.current?.off(event, handler)
    }
  }, [])

  const invoke = useCallback((method: string, ...args: unknown[]) => {
    const conn = connRef.current
    if (conn?.state === 'Connected') {
      conn.invoke(method, ...args).catch(() => {})
    }
  }, [])

  return (
    <SignalRContext.Provider value={{ state, on, invoke }}>
      {children}
    </SignalRContext.Provider>
  )
}

// ── Hooks ──────────────────────────────────────────────────────────────────────

/** Returns the current hub connection state. */
export function useSignalRStatus(): ConnectionState {
  return useContext(SignalRContext).state
}

/** Returns a stable function to invoke hub methods. */
export function useSignalRInvoke() {
  return useContext(SignalRContext).invoke
}

/**
 * Subscribe to a hub event. The handler is called with the first argument
 * the hub sends. Uses a stable ref so the handler can safely close over
 * component state without needing to be memoised by the caller.
 */
export function useSignalREvent<T = unknown>(
  event: string,
  handler: (payload: T) => void,
): void {
  const { on } = useContext(SignalRContext)
  const handlerRef = useRef(handler)
  // Keep ref current without re-subscribing
  useEffect(() => { handlerRef.current = handler })

  useEffect(() => {
    const stableHandler = (...args: unknown[]) => handlerRef.current(args[0] as T)
    return on(event, stableHandler)
  }, [event, on])
}
