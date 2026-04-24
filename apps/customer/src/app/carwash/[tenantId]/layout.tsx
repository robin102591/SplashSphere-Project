import { AuthGuard } from '@/lib/auth/auth-guard'

/**
 * Layout for `/carwash/[tenantId]/*` pages. These are detail routes that
 * sit *outside* the tabbed shell (no bottom tab nav), so we opt-in to
 * auth here explicitly — `AuthGuard` is normally installed by
 * `(tabs)/layout.tsx`.
 *
 * The page-level component owns its own app-bar so we don't render a
 * global one here — detail and sub-detail pages pick different back
 * targets (e.g., Back vs. Close) and the layout shouldn't prescribe that.
 */
export default function CarwashLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <AuthGuard>
      <div className="min-h-[100svh] bg-background">
        <main className="mx-auto max-w-lg w-full">{children}</main>
      </div>
    </AuthGuard>
  )
}
