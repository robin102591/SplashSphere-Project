import { BottomTabNav } from '@/components/layout/bottom-tab-nav'

/**
 * Mobile-first shell for the SplashSphere Connect customer app.
 *
 * - Centers content in a `max-w-lg` column so desktop users see a phone-like
 *   column instead of sprawling full-width.
 * - Reserves space at the bottom for the fixed bottom tab nav (64px base
 *   + safe-area inset on iOS home-indicator devices).
 */
export function AppShell({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-[100svh] bg-background">
      <main
        className="mx-auto max-w-lg w-full px-4 pt-4 pb-[calc(72px+env(safe-area-inset-bottom,0px))]"
      >
        {children}
      </main>
      <BottomTabNav />
    </div>
  )
}
