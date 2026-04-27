'use client'

import { useRouter } from 'next/navigation'
import { useTranslations } from 'next-intl'
import { ChevronLeft } from 'lucide-react'
import { cn } from '@/lib/utils'

interface AppBarProps {
  /** Optional title rendered centered in the bar. Truncates if too long. */
  title?: string
  /** Route to navigate to on back-press. Defaults to `router.back()`. */
  backHref?: string
  /**
   * Custom back handler. When supplied it wins over `backHref` and the
   * default `router.back()` — useful for multi-step flows that want
   * "back" to rewind an in-page state rather than pop a route.
   */
  onBack?: () => void
  /** Optional trailing element (e.g., share icon). */
  trailing?: React.ReactNode
  /**
   * When true the bar is transparent until the user scrolls; callers that
   * render a hero below the bar use this. Default false (opaque card bg).
   */
  transparent?: boolean
  className?: string
}

/**
 * Mobile app-bar used on non-tab detail pages. Fixed to the top, respects the
 * iOS safe-area inset, and provides a consistent back button. Matches the
 * `max-w-lg` column used by the tabbed shell so detail pages line up with
 * the rest of the app on desktop.
 */
export function AppBar({
  title,
  backHref,
  onBack,
  trailing,
  transparent = false,
  className,
}: AppBarProps) {
  const router = useRouter()
  const tCarwash = useTranslations('carwash')

  const handleBack = () => {
    if (onBack) {
      onBack()
      return
    }
    if (backHref) router.push(backHref)
    else router.back()
  }

  return (
    <header
      className={cn(
        'sticky top-0 z-30 w-full',
        'pt-[env(safe-area-inset-top,0px)]',
        transparent
          ? 'bg-transparent'
          : 'border-b border-border bg-background/95 backdrop-blur',
        className,
      )}
    >
      <div className="mx-auto flex h-14 w-full max-w-lg items-center gap-2 px-2">
        <button
          type="button"
          onClick={handleBack}
          aria-label={tCarwash('backLabel')}
          className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full text-foreground transition-colors active:scale-[0.94] hover:bg-muted"
        >
          <ChevronLeft className="h-6 w-6" aria-hidden />
        </button>
        {title && (
          <h1 className="min-w-0 flex-1 truncate text-center text-base font-semibold">
            {title}
          </h1>
        )}
        {!title && <div className="flex-1" />}
        <div className="flex h-11 w-11 shrink-0 items-center justify-end">
          {trailing}
        </div>
      </div>
    </header>
  )
}
