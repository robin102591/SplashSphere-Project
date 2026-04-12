const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'

async function apiFetch<T>(
  path: string,
  options: RequestInit & { token?: string; skipContentType?: boolean } = {}
): Promise<T> {
  const { token, skipContentType, ...init } = options
  const headers: Record<string, string> = {
    ...(skipContentType ? {} : { 'Content-Type': 'application/json' }),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(init.headers as Record<string, string>),
  }
  const res = await fetch(`${API_BASE}/api/v1${path}`, {
    ...init,
    headers,
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({ title: res.statusText, status: res.status }))
    throw err
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export const apiClient = {
  get: <T>(path: string, token?: string) => apiFetch<T>(path, { method: 'GET', token }),
  post: <T>(path: string, body: unknown, token?: string) =>
    apiFetch<T>(path, { method: 'POST', body: JSON.stringify(body), token }),
  put: <T>(path: string, body: unknown, token?: string) =>
    apiFetch<T>(path, { method: 'PUT', body: JSON.stringify(body), token }),
  patch: <T>(path: string, body: unknown, token?: string) =>
    apiFetch<T>(path, { method: 'PATCH', body: JSON.stringify(body), token }),
  delete: <T>(path: string, token?: string) => apiFetch<T>(path, { method: 'DELETE', token }),
  upload: <T>(path: string, formData: FormData, token?: string) =>
    apiFetch<T>(path, { method: 'POST', body: formData, token, skipContentType: true }),
}
