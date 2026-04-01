---
name: nextjs-patterns
description: Next.js 16 patterns for admin and POS apps
---

# Next.js 16 Patterns

## App Router Structure
```
app/
├── (dashboard)/          # Admin layout group
│   ├── layout.tsx        # Sidebar + header
│   ├── page.tsx          # Dashboard home
│   ├── services/
│   │   ├── page.tsx      # List
│   │   ├── new/page.tsx  # Create
│   │   └── [id]/page.tsx # Detail
│   └── ...
├── (auth)/               # Auth pages (no sidebar)
│   ├── sign-in/page.tsx
│   └── sign-up/page.tsx
└── layout.tsx            # Root layout (providers)
```

## API Client Pattern
```typescript
// lib/api-client.ts — typed fetch wrapper
export const apiClient = {
  get: <T>(path: string, token?: string) => apiFetch<T>(path, { method: 'GET', token }),
  post: <T>(path: string, body: unknown, token?: string) => apiFetch<T>(path, { method: 'POST', body: JSON.stringify(body), token }),
  put: <T>(path: string, body: unknown, token?: string) => apiFetch<T>(path, { method: 'PUT', body: JSON.stringify(body), token }),
  patch: <T>(path: string, body: unknown, token?: string) => apiFetch<T>(path, { method: 'PATCH', body: JSON.stringify(body), token }),
  delete: <T>(path: string, token?: string) => apiFetch<T>(path, { method: 'DELETE', token }),
}
```

## Proxy.ts (Not middleware.ts)
Next.js 16 renamed middleware.ts to proxy.ts.
Clerk middleware: `clerkMiddleware()` in proxy.ts.

## Tailwind CSS v4
- @theme blocks for custom properties
- @utility directives for custom utilities
- NO tailwind.config.ts — use CSS-first configuration
- NO @apply with theme tokens — causes silent failures

## Clerk Auth (Headless — Custom UI)
- Use `useSignIn()`, `useSignUp()` hooks — NOT prebuilt `<SignIn />` / `<SignUp />` components
- Use `useAuth()`, `useUser()`, `useOrganization()` for session/tenant data
- All API calls pass `Bearer ${token}` from `getToken()`

## Key Libraries
- @tanstack/react-query ^5.x for data fetching
- zustand ^5.x for client state
- react-hook-form ^7.x + zod ^3.x for forms
- shadcn/ui for component base
- recharts ^2.x for charts
- @microsoft/signalr ^8.x for real-time
