'use client'

import { List, Map as MapIcon } from 'lucide-react'
import { useTranslations } from 'next-intl'

export type DiscoverView = 'list' | 'map'

interface ViewToggleProps {
  value: DiscoverView
  onChange: (next: DiscoverView) => void
}

/**
 * Segmented List ↔ Map control for the Discover screen. A tab-style toggle
 * rather than a switch so the active option is visually dominant — matching
 * the pattern used by delivery apps.
 */
export function ViewToggle({ value, onChange }: ViewToggleProps) {
  const t = useTranslations('discover')

  return (
    <div
      role="tablist"
      aria-label={`${t('viewList')} / ${t('viewMap')}`}
      className="inline-flex items-center gap-1 rounded-full border border-border bg-card p-1 shadow-sm"
    >
      <TabButton
        active={value === 'list'}
        onClick={() => onChange('list')}
        icon={<List className="h-4 w-4" aria-hidden />}
        label={t('viewList')}
      />
      <TabButton
        active={value === 'map'}
        onClick={() => onChange('map')}
        icon={<MapIcon className="h-4 w-4" aria-hidden />}
        label={t('viewMap')}
      />
    </div>
  )
}

function TabButton({
  active,
  onClick,
  icon,
  label,
}: {
  active: boolean
  onClick: () => void
  icon: React.ReactNode
  label: string
}) {
  return (
    <button
      type="button"
      role="tab"
      aria-selected={active}
      onClick={onClick}
      className={
        'inline-flex min-h-[36px] items-center gap-1.5 rounded-full px-3 text-xs font-semibold transition-colors ' +
        (active
          ? 'bg-primary text-primary-foreground shadow-sm'
          : 'text-muted-foreground hover:text-foreground')
      }
    >
      {icon}
      {label}
    </button>
  )
}
