'use client'

import { useContext } from 'react'
import {
  ConnectAuthContext,
  type ConnectAuthContextValue,
} from '@/lib/auth/auth-context'

/**
 * Access the Connect auth state + actions. Throws if used outside
 * `<AuthProvider>` — every tree render path in this app wraps the provider
 * at the root, so a missing context is a programmer error.
 */
export function useConnectAuth(): ConnectAuthContextValue {
  const ctx = useContext(ConnectAuthContext)
  if (!ctx) {
    throw new Error('useConnectAuth must be used within <AuthProvider>')
  }
  return ctx
}
