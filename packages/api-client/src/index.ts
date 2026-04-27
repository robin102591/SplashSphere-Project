/**
 * Shared HTTP client for SplashSphere admin + POS apps.
 *
 * Used by both `@splashsphere/admin` and `@splashsphere/pos`. The customer
 * Connect app keeps its own client because it has structurally different
 * concerns (path prefix, OTP refresh-token rotation, localStorage-backed
 * bearer injection) — see apps/customer/src/lib/api-client.ts.
 *
 * Conventions:
 * - All paths are resolved against `${NEXT_PUBLIC_API_URL}/api/v1`.
 * - Bearer tokens are passed explicitly per call (not pulled from any store);
 *   callers fetch the token via Clerk's `getToken()` and pass it through.
 * - Non-OK responses throw the parsed JSON body (an RFC 9457 ProblemDetails)
 *   so React Query / try-catch handlers can read `.title` / `.detail`.
 * - 204 No Content returns `undefined`.
 */

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'

interface ApiOptions extends Omit<RequestInit, 'body'> {
  token?: string
  /** Skip the default `Content-Type: application/json` header (used for FormData). */
  skipContentType?: boolean
}

async function apiFetch<T>(
  path: string,
  options: ApiOptions & { body?: BodyInit | null } = {},
): Promise<T> {
  const { token, skipContentType, headers: callerHeaders, ...init } = options

  const headers: Record<string, string> = {
    ...(skipContentType ? {} : { 'Content-Type': 'application/json' }),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(callerHeaders as Record<string, string> | undefined),
  }

  const res = await fetch(`${API_BASE}/api/v1${path}`, { ...init, headers })

  if (!res.ok) {
    const err = await res
      .json()
      .catch(() => ({ title: res.statusText, status: res.status }))
    throw err
  }
  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}

/**
 * Returns the absolute URL for a versioned API path. Useful for building
 * `<a href>` PDF/CSV links that need to bypass the JSON pipeline (the browser
 * handles the download). Use sparingly — prefer `apiClient.download()` for
 * authenticated downloads.
 */
function fileUrl(path: string): string {
  return `${API_BASE}/api/v1${path}`
}

/**
 * Authenticated binary download. Triggers the browser's "save as" flow.
 * If the server sends a `Content-Disposition: attachment; filename=...`
 * header, that takes precedence over the caller's `fallbackFilename`.
 * Throws the ProblemDetails body on non-OK responses, so UI callers can
 * show a toast.
 */
async function download(
  path: string,
  fallbackFilename: string,
  token?: string,
): Promise<void> {
  const headers: Record<string, string> = {}
  if (token) headers.Authorization = `Bearer ${token}`

  const res = await fetch(`${API_BASE}/api/v1${path}`, { headers })

  if (!res.ok) {
    const err = await res
      .json()
      .catch(() => ({ title: res.statusText, status: res.status }))
    throw err
  }

  // Prefer the server-supplied filename (e.g. "payroll_2026-04-14_2026-04-20.csv")
  // when the response advertises one.
  const disposition = res.headers.get('Content-Disposition')
  const match = disposition?.match(/filename="?([^";\n]+)"?/)
  const filename = match?.[1] ?? fallbackFilename

  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

// `exactOptionalPropertyTypes` requires us to omit optional properties
// rather than set them to `undefined`. This helper keeps call sites tidy.
function withToken(token: string | undefined): { token?: string } {
  return token === undefined ? {} : { token }
}

export const apiClient = {
  get: <T>(path: string, token?: string) =>
    apiFetch<T>(path, { method: 'GET', ...withToken(token) }),

  post: <T>(path: string, body: unknown, token?: string) =>
    apiFetch<T>(path, { method: 'POST', body: JSON.stringify(body), ...withToken(token) }),

  put: <T>(path: string, body: unknown, token?: string) =>
    apiFetch<T>(path, { method: 'PUT', body: JSON.stringify(body), ...withToken(token) }),

  patch: <T>(path: string, body: unknown, token?: string) =>
    apiFetch<T>(path, { method: 'PATCH', body: JSON.stringify(body), ...withToken(token) }),

  delete: <T>(path: string, token?: string) =>
    apiFetch<T>(path, { method: 'DELETE', ...withToken(token) }),

  /** POST a FormData payload. Caller is responsible for constructing the form. */
  upload: <T>(path: string, formData: FormData, token?: string) =>
    apiFetch<T>(path, {
      method: 'POST',
      body: formData,
      skipContentType: true,
      ...withToken(token),
    }),

  /** Trigger an authenticated browser download. Throws on non-OK. */
  download,

  /** Build the absolute API URL for a path. Use only when bypassing apiClient (e.g. <a href>). */
  fileUrl,
}

export type ApiClient = typeof apiClient
