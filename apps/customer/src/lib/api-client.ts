import type { RefreshTokenResponse } from '@splashsphere/types'
import {
  clearTokens,
  getAccessToken,
  getRefreshToken,
  setRefreshedTokens,
} from '@/lib/auth/token-store'

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5221'
const API_PREFIX = '/api/v1/connect'

/**
 * Auth endpoint paths (relative to the `/api/v1/connect` prefix). Kept here so
 * both the client and the auth context agree on where to POST.
 */
export const AUTH_PATHS = {
  otpSend: '/auth/otp/send',
  otpVerify: '/auth/otp/verify',
  refresh: '/auth/refresh',
  signOut: '/auth/sign-out',
} as const

/**
 * A single in-flight refresh promise shared by every request that 401s while
 * a refresh is already underway. This is the classic "refresh lock" — it
 * guarantees we hit `/auth/refresh` at most once per burst of expired calls.
 *
 * The promise resolves to the new access token on success, or `null` on
 * failure (in which case callers should surface the original 401).
 */
let refreshPromise: Promise<string | null> | null = null

/**
 * Hook the auth context installs so the API client can trigger a hard
 * sign-out (clearTokens + redirect) when refresh fails. Defined as a mutable
 * module variable to avoid a circular import with auth-context.tsx.
 */
let onAuthFailure: (() => void) | null = null

export function setAuthFailureHandler(handler: (() => void) | null): void {
  onAuthFailure = handler
}

interface FetchOptions extends RequestInit {
  /** Skip attaching the bearer token (e.g., for the auth endpoints themselves). */
  skipAuth?: boolean
  /** Don't send `Content-Type: application/json` (FormData uploads). */
  skipContentType?: boolean
  /** Internal — prevents infinite retry loops. */
  _retry?: boolean
}

async function rawFetch(path: string, options: FetchOptions): Promise<Response> {
  const {
    skipAuth,
    skipContentType,
    _retry: _retryFlag,
    headers: callerHeaders,
    ...init
  } = options
  void _retryFlag
  const headers: Record<string, string> = {
    ...(skipContentType ? {} : { 'Content-Type': 'application/json' }),
    ...(callerHeaders as Record<string, string> | undefined),
  }
  if (!skipAuth) {
    const token = getAccessToken()
    if (token) headers.Authorization = `Bearer ${token}`
  }
  return fetch(`${API_BASE}${API_PREFIX}${path}`, { ...init, headers })
}

/**
 * Perform a refresh-token exchange. Called from `apiFetch` when a request
 * 401s. Concurrent callers share one in-flight promise.
 */
async function performRefresh(): Promise<string | null> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) return null
  try {
    const res = await fetch(`${API_BASE}${API_PREFIX}${AUTH_PATHS.refresh}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    })
    if (!res.ok) return null
    const body = (await res.json()) as RefreshTokenResponse
    setRefreshedTokens(body)
    return body.accessToken
  } catch {
    return null
  }
}

async function ensureRefresh(): Promise<string | null> {
  if (!refreshPromise) {
    refreshPromise = performRefresh().finally(() => {
      // Release the lock so the next 401 burst can trigger a fresh refresh.
      refreshPromise = null
    })
  }
  return refreshPromise
}

async function apiFetch<T>(path: string, options: FetchOptions = {}): Promise<T> {
  const res = await rawFetch(path, options)

  if (
    res.status === 401 &&
    !options.skipAuth &&
    !options._retry &&
    getRefreshToken()
  ) {
    const newToken = await ensureRefresh()
    if (newToken) {
      // Retry once with the new token attached (rawFetch re-reads from store).
      return apiFetch<T>(path, { ...options, _retry: true })
    }
    // Refresh failed — hard sign-out.
    clearTokens()
    onAuthFailure?.()
    // Fall through to throw below.
  }

  if (!res.ok) {
    const err = await res
      .json()
      .catch(() => ({ title: res.statusText, status: res.status }))
    throw err
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

/**
 * SplashSphere Connect API client.
 *
 * All paths are resolved against `${NEXT_PUBLIC_API_URL}/api/v1/connect`.
 * Callers pass the suffix only — e.g. `apiClient.post('/auth/otp/send', ...)`.
 *
 * Auth: the bearer token is injected automatically from `token-store` when
 * present. On a 401, the client silently exchanges the refresh token for a
 * new pair (once per burst) and retries the original request. If refresh
 * fails, tokens are cleared and the registered auth-failure handler fires
 * (typically a redirect to `/auth`).
 */
export const apiClient = {
  get: <T>(path: string, options?: FetchOptions) =>
    apiFetch<T>(path, { ...options, method: 'GET' }),
  post: <T>(path: string, body: unknown, options?: FetchOptions) =>
    apiFetch<T>(path, {
      ...options,
      method: 'POST',
      body: body === undefined ? undefined : JSON.stringify(body),
    }),
  put: <T>(path: string, body: unknown, options?: FetchOptions) =>
    apiFetch<T>(path, {
      ...options,
      method: 'PUT',
      body: JSON.stringify(body),
    }),
  patch: <T>(path: string, body: unknown, options?: FetchOptions) =>
    apiFetch<T>(path, {
      ...options,
      method: 'PATCH',
      body: JSON.stringify(body),
    }),
  delete: <T>(path: string, options?: FetchOptions) =>
    apiFetch<T>(path, { ...options, method: 'DELETE' }),
  upload: <T>(path: string, formData: FormData, options?: FetchOptions) =>
    apiFetch<T>(path, {
      ...options,
      method: 'POST',
      body: formData,
      skipContentType: true,
    }),
}
