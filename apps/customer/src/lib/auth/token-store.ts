/**
 * Token storage for the SplashSphere Connect customer PWA.
 *
 * Tokens live in localStorage under the `splashsphere.connect.*` namespace so
 * the admin / POS apps (which may one day share an origin) can't collide with
 * our keys. A module-level cache avoids re-reading localStorage on every
 * request. All accessors are SSR-safe — they short-circuit to `null` when
 * `window` is undefined.
 *
 * This module intentionally holds NO React state. The auth context reads from
 * it on mount and mirrors the values into React state; `apiClient` reads from
 * it on every outgoing request.
 */

import type {
  ConnectUserDto,
  VerifyOtpResponse,
  RefreshTokenResponse,
} from '@splashsphere/types'

const KEY_PREFIX = 'splashsphere.connect.'
const K_ACCESS_TOKEN = `${KEY_PREFIX}accessToken`
const K_ACCESS_EXPIRES = `${KEY_PREFIX}accessTokenExpiresAt`
const K_REFRESH_TOKEN = `${KEY_PREFIX}refreshToken`
const K_REFRESH_EXPIRES = `${KEY_PREFIX}refreshTokenExpiresAt`
const K_USER = `${KEY_PREFIX}user`

export interface ConnectTokens {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
}

interface CacheShape {
  tokens: ConnectTokens | null
  user: ConnectUserDto | null
  hydrated: boolean
}

const cache: CacheShape = {
  tokens: null,
  user: null,
  hydrated: false,
}

// Listener set for useSyncExternalStore subscribers. Kept small (effectively
// one subscriber: AuthProvider) but the pattern scales if we ever want to
// read the user outside the context.
const listeners = new Set<() => void>()

function notify(): void {
  for (const listener of listeners) listener()
}

export function subscribeTokenStore(listener: () => void): () => void {
  listeners.add(listener)
  return () => {
    listeners.delete(listener)
  }
}

function isBrowser(): boolean {
  return typeof window !== 'undefined'
}

function hydrate(): void {
  if (cache.hydrated || !isBrowser()) return
  cache.hydrated = true
  try {
    const accessToken = window.localStorage.getItem(K_ACCESS_TOKEN)
    const accessTokenExpiresAt = window.localStorage.getItem(K_ACCESS_EXPIRES)
    const refreshToken = window.localStorage.getItem(K_REFRESH_TOKEN)
    const refreshTokenExpiresAt = window.localStorage.getItem(K_REFRESH_EXPIRES)
    const userJson = window.localStorage.getItem(K_USER)

    if (
      accessToken &&
      accessTokenExpiresAt &&
      refreshToken &&
      refreshTokenExpiresAt
    ) {
      cache.tokens = {
        accessToken,
        accessTokenExpiresAt,
        refreshToken,
        refreshTokenExpiresAt,
      }
    }
    if (userJson) {
      cache.user = JSON.parse(userJson) as ConnectUserDto
    }
  } catch {
    // Corrupt storage — treat as signed out.
    cache.tokens = null
    cache.user = null
  }
}

/** Returns the full token triple (or null if the user is signed out). */
export function getTokens(): ConnectTokens | null {
  hydrate()
  return cache.tokens
}

/** Returns just the access token (convenience for the API client). */
export function getAccessToken(): string | null {
  hydrate()
  return cache.tokens?.accessToken ?? null
}

/** Returns just the refresh token (convenience for the refresh flow). */
export function getRefreshToken(): string | null {
  hydrate()
  return cache.tokens?.refreshToken ?? null
}

/** Returns the cached user profile, or null if signed out. */
export function getUser(): ConnectUserDto | null {
  hydrate()
  return cache.user
}

/**
 * Persist tokens + user from a verify-OTP response.
 * Used on initial sign-in.
 */
export function setTokens(response: VerifyOtpResponse): void {
  cache.hydrated = true
  cache.tokens = {
    accessToken: response.accessToken,
    accessTokenExpiresAt: response.accessTokenExpiresAt,
    refreshToken: response.refreshToken,
    refreshTokenExpiresAt: response.refreshTokenExpiresAt,
  }
  cache.user = response.user
  if (isBrowser()) {
    try {
      window.localStorage.setItem(K_ACCESS_TOKEN, response.accessToken)
      window.localStorage.setItem(
        K_ACCESS_EXPIRES,
        response.accessTokenExpiresAt,
      )
      window.localStorage.setItem(K_REFRESH_TOKEN, response.refreshToken)
      window.localStorage.setItem(
        K_REFRESH_EXPIRES,
        response.refreshTokenExpiresAt,
      )
      window.localStorage.setItem(K_USER, JSON.stringify(response.user))
    } catch {
      // Swallow quota/security errors — cache is still authoritative for the session.
    }
  }
  notify()
}

/**
 * Replace just the tokens (not the user) after a silent refresh.
 */
export function setRefreshedTokens(response: RefreshTokenResponse): void {
  cache.hydrated = true
  cache.tokens = {
    accessToken: response.accessToken,
    accessTokenExpiresAt: response.accessTokenExpiresAt,
    refreshToken: response.refreshToken,
    refreshTokenExpiresAt: response.refreshTokenExpiresAt,
  }
  if (isBrowser()) {
    try {
      window.localStorage.setItem(K_ACCESS_TOKEN, response.accessToken)
      window.localStorage.setItem(
        K_ACCESS_EXPIRES,
        response.accessTokenExpiresAt,
      )
      window.localStorage.setItem(K_REFRESH_TOKEN, response.refreshToken)
      window.localStorage.setItem(
        K_REFRESH_EXPIRES,
        response.refreshTokenExpiresAt,
      )
    } catch {
      /* see setTokens */
    }
  }
  notify()
}

/** Wipe all Connect auth state (sign-out, refresh failure, corrupt data). */
export function clearTokens(): void {
  cache.tokens = null
  cache.user = null
  cache.hydrated = true
  if (isBrowser()) {
    try {
      window.localStorage.removeItem(K_ACCESS_TOKEN)
      window.localStorage.removeItem(K_ACCESS_EXPIRES)
      window.localStorage.removeItem(K_REFRESH_TOKEN)
      window.localStorage.removeItem(K_REFRESH_EXPIRES)
      window.localStorage.removeItem(K_USER)
    } catch {
      /* ignore */
    }
  }
  notify()
}

/**
 * Immutable snapshot used by `useSyncExternalStore`. We compute a stable
 * identity so React can cheaply bail out when nothing has changed —
 * returning the same object reference when the underlying data is equal.
 *
 * `hydrated` distinguishes "we haven't read localStorage yet" (server render
 * or before the first client render) from "we read it and found no tokens"
 * (user is genuinely signed out). AuthGuard uses this to avoid flashing a
 * redirect during hydration.
 */
interface StoreSnapshot {
  readonly tokens: ConnectTokens | null
  readonly user: ConnectUserDto | null
  readonly hydrated: boolean
}

const SERVER_SNAPSHOT: StoreSnapshot = {
  tokens: null,
  user: null,
  hydrated: false,
}

let cachedSnapshot: StoreSnapshot = SERVER_SNAPSHOT
let lastTokens: ConnectTokens | null = null
let lastUser: ConnectUserDto | null = null
let lastHydrated = false

export function getSnapshot(): StoreSnapshot {
  hydrate()
  if (
    cache.tokens !== lastTokens ||
    cache.user !== lastUser ||
    cache.hydrated !== lastHydrated
  ) {
    lastTokens = cache.tokens
    lastUser = cache.user
    lastHydrated = cache.hydrated
    cachedSnapshot = {
      tokens: cache.tokens,
      user: cache.user,
      hydrated: cache.hydrated,
    }
  }
  return cachedSnapshot
}

/**
 * Server-side snapshot — no tokens, not hydrated. Keeps the initial SSR
 * render consistent and avoids hydration mismatches.
 */
export function getServerSnapshot(): StoreSnapshot {
  return SERVER_SNAPSHOT
}
