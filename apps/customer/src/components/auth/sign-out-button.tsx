'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { Loader2, LogOut } from 'lucide-react'
import { useConnectAuth } from '@/lib/auth/use-connect-auth'

/**
 * Profile-page sign-out action. Calls `signOut()` which revokes the refresh
 * token best-effort, clears local storage, and redirects to `/auth`.
 */
export function SignOutButton() {
  const { signOut } = useConnectAuth()
  const t = useTranslations('common')
  const [isBusy, setIsBusy] = useState(false)

  return (
    <button
      type="button"
      onClick={async () => {
        setIsBusy(true)
        try {
          await signOut()
        } finally {
          setIsBusy(false)
        }
      }}
      disabled={isBusy}
      className="flex w-full min-h-[52px] items-center justify-center gap-2 rounded-xl border border-border bg-background px-4 py-3 text-base font-medium text-foreground transition-colors active:scale-[0.97] disabled:cursor-not-allowed disabled:opacity-60 hover:bg-muted"
    >
      {isBusy ? (
        <Loader2 className="h-5 w-5 animate-spin" aria-hidden />
      ) : (
        <LogOut className="h-5 w-5" aria-hidden />
      )}
      <span>{t('signOut')}</span>
    </button>
  )
}
