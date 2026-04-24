import { AuthGuard } from '@/lib/auth/auth-guard'

/**
 * Layout for `/bookings/**`. Mirrors the `(tabs)` route group's auth
 * protection but lives outside of it so detail pages have their own
 * route tree without a bottom-tab entry.
 *
 * We deliberately don't render a shared AppBar component here — the
 * Discover agent (task 22.3-D) also needs a header for `/carwash/**`
 * and owns the creation of the reusable `AppBar`. Each page in this
 * tree renders its own inline header for now; the shared component
 * can be extracted later without changing page behaviour.
 */
export default function BookingsLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <AuthGuard>
      <div className="min-h-[100svh] bg-background">
        <main className="mx-auto max-w-lg w-full px-4 pt-4 pb-[calc(24px+env(safe-area-inset-bottom,0px))]">
          {children}
        </main>
      </div>
    </AuthGuard>
  )
}
