'use client'

import type { ReactNode } from 'react'
import Link from 'next/link'
import type { LucideIcon } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useSectionParam } from '@/hooks/use-section-param'

export interface SectionNavItem {
  /** Slug used in the `?section=` query param. */
  value: string
  /** Display label rendered in the nav row / chip. */
  label: string
  /** Optional leading icon — same family as the main sidebar items. */
  icon?: LucideIcon
  /** Optional trailing badge (e.g., a count chip). */
  badge?: ReactNode
}

export interface SectionNavAction {
  label: string
  href: string
  icon?: LucideIcon
}

export interface SectionNavProps {
  items: readonly SectionNavItem[]
  /** Secondary actions rendered below the section list (desktop only). */
  actions?: readonly SectionNavAction[]
  /** Query param key. Defaults to `"section"`. */
  paramKey?: string
  /** Slug used when the URL has no value. Defaults to `items[0].value`. */
  defaultValue?: string
  className?: string
}

/**
 * URL-driven secondary navigation for in-page sub-views. Renders as a
 * vertical column on `md:` and up, and as a horizontal scrolling chip
 * strip on small viewports. The active section lives in the URL via
 * `?section=<slug>` so refreshes and deep links land in the right place.
 *
 * Style is intentionally aligned with the main sidebar (`AppSidebar`):
 * same row height, padding, and active-row treatment so the two read as
 * a single navigation hierarchy.
 */
export function SectionNav({
  items,
  actions,
  paramKey = 'section',
  defaultValue,
  className,
}: SectionNavProps) {
  const fallback = defaultValue ?? items[0]?.value ?? ''
  const [active, setActive] = useSectionParam(paramKey, fallback)
  const resolved = items.some((i) => i.value === active) ? active : fallback

  return (
    <nav
      aria-label="Page sections"
      className={cn(
        // Mobile: horizontal scroll chip strip.
        'flex w-full gap-1 overflow-x-auto pb-1 [scrollbar-width:none] [-ms-overflow-style:none] [&::-webkit-scrollbar]:hidden',
        // Desktop: vertical column, sticky inside the scroll container.
        'md:sticky md:top-4 md:h-fit md:flex-col md:gap-0.5 md:overflow-visible md:pb-0',
        className,
      )}
    >
      {items.map((item) => {
        const Icon = item.icon
        const isActive = item.value === resolved
        return (
          <button
            key={item.value}
            type="button"
            onClick={() => setActive(item.value)}
            aria-current={isActive ? 'page' : undefined}
            className={cn(
              // Shared row/chip layout.
              'group inline-flex shrink-0 items-center gap-2 rounded-lg px-3 text-sm font-medium transition-colors outline-none',
              'snap-start',
              // Mobile chip dimensions.
              'h-9 whitespace-nowrap',
              // Desktop row dimensions.
              'md:h-9 md:w-full md:justify-start',
              'focus-visible:ring-2 focus-visible:ring-ring/50',
              isActive
                ? 'bg-splash-50 text-splash-700 hover:bg-splash-100 md:border-l-2 md:border-splash-500 md:rounded-l-none dark:bg-splash-500/10'
                : 'text-muted-foreground hover:bg-muted hover:text-foreground',
            )}
          >
            {Icon ? <Icon className="h-4 w-4 shrink-0" aria-hidden /> : null}
            <span className="flex-1 truncate text-left">{item.label}</span>
            {item.badge != null && (
              <span
                className={cn(
                  'ml-1 inline-flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-[11px] font-semibold tabular-nums',
                  isActive ? 'bg-splash-500/15 text-splash-700' : 'bg-muted text-muted-foreground',
                )}
              >
                {item.badge}
              </span>
            )}
          </button>
        )
      })}

      {actions && actions.length > 0 && (
        <div className="hidden md:mt-3 md:flex md:flex-col md:gap-1 md:border-t md:pt-3">
          {actions.map((action) => {
            const Icon = action.icon
            return (
              <Link
                key={action.href}
                href={action.href}
                className="inline-flex h-9 w-full items-center gap-2 rounded-lg border border-border bg-background px-3 text-sm font-medium text-foreground transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
              >
                {Icon ? <Icon className="h-4 w-4 shrink-0" aria-hidden /> : null}
                <span className="flex-1 truncate text-left">{action.label}</span>
              </Link>
            )
          })}
        </div>
      )}
    </nav>
  )
}
