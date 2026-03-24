'use client'

import { create } from 'zustand'

interface LockState {
  isLocked: boolean
  lastActivity: number
  failedAttempts: number
  cooldownUntil: number | null

  lock: () => void
  unlock: () => void
  recordActivity: () => void
  recordFailedAttempt: (maxAttempts: number, cooldownMs?: number) => void
  resetAttempts: () => void
}

export const useLockStore = create<LockState>((set) => ({
  isLocked: false,
  lastActivity: Date.now(),
  failedAttempts: 0,
  cooldownUntil: null,

  lock: () => set({ isLocked: true }),

  unlock: () => set({
    isLocked: false,
    failedAttempts: 0,
    cooldownUntil: null,
    lastActivity: Date.now(),
  }),

  recordActivity: () => set({ lastActivity: Date.now() }),

  recordFailedAttempt: (maxAttempts, cooldownMs = 30_000) =>
    set((state) => {
      const next = state.failedAttempts + 1
      return {
        failedAttempts: next,
        cooldownUntil: next >= maxAttempts ? Date.now() + cooldownMs : state.cooldownUntil,
      }
    }),

  resetAttempts: () => set({ failedAttempts: 0, cooldownUntil: null }),
}))
