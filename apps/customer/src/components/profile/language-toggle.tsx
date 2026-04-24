'use client'

import { useLocale, useTranslations } from 'next-intl'
import { Globe } from 'lucide-react'
import { locales, localeNames, type Locale } from '@/i18n/config'
import { cn } from '@/lib/utils'

/**
 * Settings-card language toggle. Writes the selected locale to the
 * `NEXT_LOCALE` cookie (read server-side by `i18n/request.ts`) and hard-
 * reloads so the new messages are picked up from the fresh RSC render.
 */
export function LanguageToggle() {
  const currentLocale = useLocale() as Locale
  const t = useTranslations('profile.language')

  const setLocale = (locale: Locale) => {
    if (locale === currentLocale) return
    // Write the locale cookie and hard-reload so the RSC render picks up the
    // new messages. Assignments to `document.cookie` are additive, not
    // destructive — this appends/updates the NEXT_LOCALE entry only.
    // eslint-disable-next-line react-hooks/immutability
    document.cookie = `NEXT_LOCALE=${locale};path=/;max-age=31536000;SameSite=Lax`
    window.location.reload()
  }

  return (
    <section className="rounded-2xl border border-border bg-card p-4">
      <header className="mb-3 flex items-center gap-2 text-sm font-semibold text-foreground">
        <Globe className="h-4 w-4 text-muted-foreground" aria-hidden />
        {t('title')}
      </header>
      <div
        role="radiogroup"
        aria-label={t('title')}
        className="flex items-center gap-2"
      >
        {locales.map((locale) => {
          const active = currentLocale === locale
          return (
            <button
              key={locale}
              type="button"
              role="radio"
              aria-checked={active}
              onClick={() => setLocale(locale)}
              className={cn(
                'flex min-h-[44px] flex-1 items-center justify-center rounded-xl px-3 text-sm font-medium transition-colors active:scale-[0.97]',
                active
                  ? 'bg-primary text-primary-foreground'
                  : 'border border-border bg-background text-foreground hover:bg-muted',
              )}
            >
              {localeNames[locale]}
            </button>
          )
        })}
      </div>
    </section>
  )
}
