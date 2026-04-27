'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { useTranslations } from 'next-intl'
import { Home, Calendar, Clock, User, type LucideIcon } from 'lucide-react'
import { cn } from '@/lib/utils'

type TabItem = {
  labelKey: 'home' | 'book' | 'history' | 'profile'
  href: string
  icon: LucideIcon
  // Match nested routes under the tab's root path.
  matchPrefix?: boolean
}

const tabs: TabItem[] = [
  { labelKey: 'home', href: '/', icon: Home },
  { labelKey: 'book', href: '/discover', icon: Calendar, matchPrefix: true },
  { labelKey: 'history', href: '/history', icon: Clock, matchPrefix: true },
  { labelKey: 'profile', href: '/profile', icon: User, matchPrefix: true },
]

export function BottomTabNav() {
  const pathname = usePathname()
  const t = useTranslations('nav')

  return (
    <nav
      aria-label="Primary"
      className="fixed bottom-0 inset-x-0 z-40 bg-white/95 backdrop-blur border-t border-border safe-area-pb"
    >
      <ul className="mx-auto max-w-lg flex items-stretch px-1">
        {tabs.map((tab) => {
          const active = tab.matchPrefix
            ? pathname === tab.href || pathname.startsWith(`${tab.href}/`)
            : pathname === tab.href
          const Icon = tab.icon
          return (
            <li key={tab.href} className="flex-1">
              <Link
                href={tab.href}
                aria-current={active ? 'page' : undefined}
                className={cn(
                  'flex flex-col items-center justify-center gap-1 min-h-[56px] py-2 text-xs font-medium transition-colors duration-150 active:scale-[0.97]',
                  active
                    ? 'text-primary'
                    : 'text-muted-foreground hover:text-foreground',
                )}
              >
                <Icon
                  className={cn(
                    'h-6 w-6 shrink-0',
                    active ? 'text-primary' : 'text-muted-foreground',
                  )}
                  aria-hidden
                />
                <span className="truncate">{t(tab.labelKey)}</span>
              </Link>
            </li>
          )
        })}
      </ul>
    </nav>
  )
}
