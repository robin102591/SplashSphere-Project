'use client'

import { useLocale } from 'next-intl'
import { locales, localeNames, type Locale } from '@/i18n/config'

function setLocaleCookie(locale: Locale) {
  document.cookie = `NEXT_LOCALE=${locale};path=/;max-age=31536000;SameSite=Lax`
  localStorage.setItem('splashsphere-locale', locale)
  window.location.reload()
}

export function LanguageSwitcher() {
  const currentLocale = useLocale() as Locale

  return (
    <div className="flex rounded-lg overflow-hidden border border-gray-700">
      {locales.map((locale) => (
        <button
          key={locale}
          onClick={() => {
            if (locale !== currentLocale) setLocaleCookie(locale)
          }}
          className={`px-3 py-1.5 text-xs font-medium min-h-[36px] transition-colors ${
            currentLocale === locale
              ? 'bg-blue-600 text-white'
              : 'bg-gray-800 text-gray-400 hover:text-gray-200'
          }`}
        >
          {localeNames[locale]}
        </button>
      ))}
    </div>
  )
}
