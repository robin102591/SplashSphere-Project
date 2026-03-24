'use client'

import { useEffect, useRef } from 'react'
import { useLockStore } from './use-lock-store'

/**
 * Tracks user activity (mouse, keyboard, touch) and auto-locks the POS
 * after `timeoutMinutes` of inactivity. Renders nothing — mount once
 * in the terminal layout.
 *
 * @param timeoutMinutes  Minutes before auto-lock (0 = disabled)
 * @param enabled         Whether the tracker is active (e.g. user has a PIN)
 */
export function useActivityTracker(timeoutMinutes: number, enabled: boolean) {
  const { isLocked, lastActivity, lock, recordActivity } = useLockStore()
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null)

  // ── Record activity on user interaction (throttled to once per 10s) ───────
  useEffect(() => {
    if (!enabled || isLocked) return

    let lastRecorded = Date.now()
    const THROTTLE_MS = 10_000

    const handler = () => {
      const now = Date.now()
      if (now - lastRecorded >= THROTTLE_MS) {
        lastRecorded = now
        recordActivity()
      }
    }

    const events = ['mousedown', 'keydown', 'touchstart', 'scroll'] as const
    for (const evt of events) document.addEventListener(evt, handler, { passive: true })
    return () => {
      for (const evt of events) document.removeEventListener(evt, handler)
    }
  }, [enabled, isLocked, recordActivity])

  // ── Check for inactivity timeout every 15s ────────────────────────────────
  useEffect(() => {
    if (!enabled || isLocked || timeoutMinutes <= 0) {
      if (intervalRef.current) clearInterval(intervalRef.current)
      return
    }

    const timeoutMs = timeoutMinutes * 60 * 1000

    intervalRef.current = setInterval(() => {
      const idle = Date.now() - useLockStore.getState().lastActivity
      if (idle >= timeoutMs) {
        lock()
      }
    }, 15_000)

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current)
    }
  }, [enabled, isLocked, timeoutMinutes, lock, lastActivity])
}
