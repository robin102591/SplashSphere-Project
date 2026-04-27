'use client'

import { User as UserIcon } from 'lucide-react'
import { useConnectAuth } from '@/lib/auth/use-connect-auth'
import { formatForDisplay } from '@/lib/auth/phone'

/**
 * Small profile summary for the Profile page. Reads the signed-in
 * ConnectUser from context. Because this renders inside an AuthGuard-protected
 * route, `user` is guaranteed non-null at render time — but we still fall
 * back gracefully for the one tick between mount and hydration.
 */
export function ProfileCard() {
  const { user } = useConnectAuth()
  if (!user) return null

  const initials = user.name
    ? user.name
        .split(/\s+/)
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase() ?? '')
        .join('')
    : null

  return (
    <div className="rounded-2xl border border-border bg-card p-6">
      <div className="flex items-center gap-4">
        <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-muted text-lg font-semibold text-muted-foreground">
          {initials || <UserIcon className="h-6 w-6" aria-hidden />}
        </div>
        <div className="min-w-0">
          <p className="truncate text-base font-semibold">
            {user.name || user.phone}
          </p>
          <p className="truncate text-sm text-muted-foreground">
            {formatForDisplay(user.phone)}
          </p>
          {user.email && (
            <p className="truncate text-xs text-muted-foreground">
              {user.email}
            </p>
          )}
        </div>
      </div>
    </div>
  )
}
