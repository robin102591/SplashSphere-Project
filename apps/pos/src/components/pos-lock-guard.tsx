'use client'

import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@clerk/nextjs'
import { apiClient } from '@/lib/api-client'
import { useActivityTracker } from '@/lib/use-activity-tracker'
import { LockScreen } from '@/components/lock-screen'
import type { CurrentUser, ShiftSettingsDto } from '@splashsphere/types'

/**
 * Client component mounted once in the terminal layout.
 * Handles activity tracking and renders the lock screen overlay when locked.
 */
export function PosLockGuard() {
  const { getToken } = useAuth()

  const { data: currentUser } = useQuery({
    queryKey: ['current-user-lock'],
    staleTime: 60_000,
    refetchInterval: 120_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CurrentUser>('/auth/me', token ?? undefined)
    },
  })

  const { data: settings } = useQuery({
    queryKey: ['shift-settings-lock'],
    staleTime: 60_000,
    refetchInterval: 120_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ShiftSettingsDto>('/settings/shift-config', token ?? undefined)
    },
  })

  const hasPin = currentUser?.hasPin ?? false
  const lockTimeout = settings?.lockTimeoutMinutes ?? 5
  const maxAttempts = settings?.maxPinAttempts ?? 5

  // Only track activity and auto-lock if the user has a PIN configured
  useActivityTracker(lockTimeout, hasPin)

  return (
    <LockScreen
      maxPinAttempts={maxAttempts}
      hasPin={hasPin}
    />
  )
}
