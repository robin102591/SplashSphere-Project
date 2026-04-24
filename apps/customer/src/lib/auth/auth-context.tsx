'use client'

import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useSyncExternalStore,
} from 'react'
import { useRouter } from 'next/navigation'
import type { ConnectUserDto, VerifyOtpResponse } from '@splashsphere/types'
import { apiClient, AUTH_PATHS, setAuthFailureHandler } from '@/lib/api-client'
import {
  clearTokens,
  getRefreshToken,
  getServerSnapshot,
  getSnapshot,
  setTokens,
  subscribeTokenStore,
} from '@/lib/auth/token-store'

export interface ConnectAuthContextValue {
  user: ConnectUserDto | null
  isAuthenticated: boolean
  isLoading: boolean
  signIn: (response: VerifyOtpResponse) => void
  signOut: () => Promise<void>
}

export const ConnectAuthContext = createContext<ConnectAuthContextValue | null>(
  null,
)

/**
 * Wires the Connect auth state into React via `useSyncExternalStore` — the
 * token store is the single source of truth and this hook subscribes to it.
 * `isLoading` comes from the snapshot's `hydrated` flag: false during SSR +
 * first client render (server snapshot), then true once `getSnapshot()` has
 * read localStorage.
 */
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter()
  const snapshot = useSyncExternalStore(
    subscribeTokenStore,
    getSnapshot,
    getServerSnapshot,
  )

  // Stable ref so the failure handler doesn't re-subscribe on every router
  // instance change (Next hands us a new one per render).
  const routerRef = useRef(router)
  useEffect(() => {
    routerRef.current = router
  }, [router])

  const hardSignOut = useCallback(() => {
    clearTokens()
    routerRef.current.replace('/auth')
  }, [])

  // Let the API client trigger hardSignOut when refresh fails.
  useEffect(() => {
    setAuthFailureHandler(hardSignOut)
    return () => setAuthFailureHandler(null)
  }, [hardSignOut])

  const signIn = useCallback((response: VerifyOtpResponse) => {
    setTokens(response)
  }, [])

  const signOut = useCallback(async () => {
    const refreshToken = getRefreshToken()
    if (refreshToken) {
      // Best-effort revoke — backend is idempotent, so a failure is fine.
      try {
        await apiClient.post(
          AUTH_PATHS.signOut,
          { refreshToken },
          { skipAuth: true },
        )
      } catch {
        // Swallow — we still want to clear local state.
      }
    }
    hardSignOut()
  }, [hardSignOut])

  const value = useMemo<ConnectAuthContextValue>(
    () => ({
      user: snapshot.user,
      isAuthenticated: Boolean(snapshot.tokens && snapshot.user),
      // "Loading" == we haven't hydrated yet. After the first client render,
      // getSnapshot flips `hydrated` to true and we're done.
      isLoading: !snapshot.hydrated,
      signIn,
      signOut,
    }),
    [snapshot, signIn, signOut],
  )

  return (
    <ConnectAuthContext.Provider value={value}>
      {children}
    </ConnectAuthContext.Provider>
  )
}
