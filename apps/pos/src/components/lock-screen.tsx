'use client'

import { useState, useEffect, useCallback } from 'react'
import { useAuth, useUser } from '@clerk/nextjs'
import { Lock, Delete, LogOut, AlertCircle } from 'lucide-react'
import { useLockStore } from '@/lib/use-lock-store'
import { useBranch } from '@/lib/branch-context'
import { apiClient } from '@/lib/api-client'

const PIN_LENGTH = 6 // max digits accepted
const COOLDOWN_MS = 30_000

interface LockScreenProps {
  maxPinAttempts: number
  hasPin: boolean
}

export function LockScreen({ maxPinAttempts, hasPin }: LockScreenProps) {
  const { isLocked, unlock, recordFailedAttempt, resetAttempts, failedAttempts, cooldownUntil } =
    useLockStore()
  const { getToken, signOut } = useAuth()
  const { user } = useUser()
  const { branchName } = useBranch()

  const [pin, setPin] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [verifying, setVerifying] = useState(false)
  const [shake, setShake] = useState(false)
  const [cooldownLeft, setCooldownLeft] = useState(0)
  const [clock, setClock] = useState(new Date())

  // Live clock
  useEffect(() => {
    if (!isLocked) return
    const id = setInterval(() => setClock(new Date()), 1000)
    return () => clearInterval(id)
  }, [isLocked])

  // Cooldown countdown
  useEffect(() => {
    if (!cooldownUntil) { setCooldownLeft(0); return }
    const tick = () => {
      const left = Math.max(0, cooldownUntil - Date.now())
      setCooldownLeft(left)
      if (left <= 0) resetAttempts()
    }
    tick()
    const id = setInterval(tick, 1000)
    return () => clearInterval(id)
  }, [cooldownUntil, resetAttempts])

  const isCoolingDown = cooldownLeft > 0

  const handleVerify = useCallback(async (entered: string) => {
    if (verifying || isCoolingDown) return
    setVerifying(true)
    setError(null)
    try {
      const token = await getToken()
      const res = await apiClient.post<{ success: boolean }>(
        '/auth/verify-pin',
        { pin: entered },
        token ?? undefined,
      )
      if (res.success) {
        unlock()
        setPin('')
      } else {
        recordFailedAttempt(maxPinAttempts, COOLDOWN_MS)
        setError('Incorrect PIN')
        setShake(true)
        setTimeout(() => setShake(false), 500)
        setPin('')
      }
    } catch {
      setError('Verification failed. Try again.')
      setPin('')
    } finally {
      setVerifying(false)
    }
  }, [verifying, isCoolingDown, getToken, unlock, recordFailedAttempt, maxPinAttempts])

  const addDigit = (digit: string) => {
    if (isCoolingDown || verifying) return
    const next = pin + digit
    setPin(next)
    setError(null)
    if (next.length >= 4) {
      void handleVerify(next)
    }
  }

  const removeDigit = () => {
    setPin((prev) => prev.slice(0, -1))
    setError(null)
  }

  const clearPin = () => {
    setPin('')
    setError(null)
  }

  // Keyboard support
  useEffect(() => {
    if (!isLocked) return
    const handler = (e: KeyboardEvent) => {
      if (e.key >= '0' && e.key <= '9') addDigit(e.key)
      else if (e.key === 'Backspace') removeDigit()
      else if (e.key === 'Escape') clearPin()
    }
    document.addEventListener('keydown', handler)
    return () => document.removeEventListener('keydown', handler)
  })

  if (!isLocked) return null

  const manilaTime = clock.toLocaleTimeString('en-PH', {
    timeZone: 'Asia/Manila',
    hour: '2-digit',
    minute: '2-digit',
  })

  return (
    <div className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-gray-950">
      {/* Clock */}
      <p className="text-6xl font-light text-white mb-2 tabular-nums">{manilaTime}</p>
      <p className="text-sm text-gray-500 mb-8">
        {branchName ?? 'SplashSphere POS'}
      </p>

      {/* User info */}
      <div className="flex items-center gap-3 mb-8">
        <div className="h-12 w-12 rounded-full bg-gray-800 flex items-center justify-center">
          <Lock className="h-5 w-5 text-gray-400" />
        </div>
        <div>
          <p className="text-white font-semibold">{user?.fullName ?? 'Cashier'}</p>
          <p className="text-xs text-gray-500">Screen locked</p>
        </div>
      </div>

      {!hasPin ? (
        /* No PIN configured */
        <div className="text-center space-y-4 max-w-sm">
          <div className="rounded-xl border border-yellow-700/50 bg-yellow-950/30 p-5 space-y-2">
            <AlertCircle className="h-8 w-8 text-yellow-400 mx-auto" />
            <p className="text-sm text-gray-300">
              No PIN has been configured for your account. Contact your administrator to set a PIN.
            </p>
          </div>
          <button
            onClick={() => void signOut()}
            className="flex items-center gap-2 mx-auto px-5 min-h-11 rounded-xl bg-gray-800 hover:bg-gray-700 text-gray-300 text-sm font-medium transition-colors"
          >
            <LogOut className="h-4 w-4" />
            Sign Out
          </button>
        </div>
      ) : (
        <>
          {/* PIN dots */}
          <div className={`flex gap-3 mb-6 ${shake ? 'animate-shake' : ''}`}>
            {Array.from({ length: PIN_LENGTH }).map((_, i) => (
              <div
                key={i}
                className={`h-4 w-4 rounded-full transition-colors ${
                  i < pin.length ? 'bg-blue-500' : 'bg-gray-700'
                }`}
              />
            ))}
          </div>

          {/* Error / cooldown message */}
          {isCoolingDown ? (
            <p className="text-sm text-red-400 mb-4">
              Too many attempts. Try again in {Math.ceil(cooldownLeft / 1000)}s
            </p>
          ) : error ? (
            <p className="text-sm text-red-400 mb-4">{error}</p>
          ) : (
            <p className="text-sm text-gray-600 mb-4">
              {failedAttempts > 0
                ? `${maxPinAttempts - failedAttempts} attempt${maxPinAttempts - failedAttempts !== 1 ? 's' : ''} remaining`
                : 'Enter your PIN to unlock'}
            </p>
          )}

          {/* Number pad */}
          <div className="grid grid-cols-3 gap-3 w-64">
            {['1', '2', '3', '4', '5', '6', '7', '8', '9'].map((d) => (
              <button
                key={d}
                onClick={() => addDigit(d)}
                disabled={isCoolingDown || verifying}
                className="h-16 rounded-xl bg-gray-800 hover:bg-gray-700 active:bg-gray-600 disabled:opacity-30 disabled:cursor-not-allowed text-white text-2xl font-semibold transition-colors"
              >
                {d}
              </button>
            ))}
            <button
              onClick={() => void signOut()}
              className="h-16 rounded-xl bg-gray-800 hover:bg-gray-700 text-gray-400 flex items-center justify-center transition-colors"
              title="Sign out"
            >
              <LogOut className="h-5 w-5" />
            </button>
            <button
              onClick={() => addDigit('0')}
              disabled={isCoolingDown || verifying}
              className="h-16 rounded-xl bg-gray-800 hover:bg-gray-700 active:bg-gray-600 disabled:opacity-30 disabled:cursor-not-allowed text-white text-2xl font-semibold transition-colors"
            >
              0
            </button>
            <button
              onClick={removeDigit}
              disabled={isCoolingDown || verifying || pin.length === 0}
              className="h-16 rounded-xl bg-gray-800 hover:bg-gray-700 active:bg-gray-600 disabled:opacity-30 disabled:cursor-not-allowed text-gray-400 flex items-center justify-center transition-colors"
              title="Backspace"
            >
              <Delete className="h-5 w-5" />
            </button>
          </div>
        </>
      )}
    </div>
  )
}
