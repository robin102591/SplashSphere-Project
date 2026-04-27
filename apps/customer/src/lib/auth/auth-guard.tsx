'use client'

import { useEffect } from 'react'
import { usePathname, useRouter } from 'next/navigation'
import { Loader2 } from 'lucide-react'
import { useConnectAuth } from '@/lib/auth/use-connect-auth'

/**
 * Client-side route guard for the `(tabs)` group. Redirects unauthenticated
 * visitors to `/auth?redirect=<current-path>` once the auth context finishes
 * hydrating. Tokens live in localStorage, so there's no Next middleware path
 * to do this server-side — the initial HTML always renders, and we swap to
 * a spinner until hydration completes to prevent a flash of authed UI.
 */
export function AuthGuard({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useConnectAuth()
  const router = useRouter()
  const pathname = usePathname()

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      const redirect = pathname && pathname !== '/' ? pathname : '/'
      const target = `/auth?redirect=${encodeURIComponent(redirect)}`
      router.replace(target)
    }
  }, [isLoading, isAuthenticated, pathname, router])

  if (isLoading || !isAuthenticated) {
    return (
      <div
        className="flex min-h-[50vh] items-center justify-center"
        aria-busy="true"
        role="status"
      >
        <Loader2
          className="h-6 w-6 animate-spin text-muted-foreground"
          aria-hidden
        />
      </div>
    )
  }

  return <>{children}</>
}
