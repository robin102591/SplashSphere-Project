'use client'

import { useEffect, useState } from 'react'
import Image from 'next/image'
import { Droplets } from 'lucide-react'
import type { DisplayConfigDto } from '@splashsphere/types'
import type { ThemeClasses } from './page'

/**
 * Default-state screen shown when no transaction is active. Cycles through
 * promo messages on a configurable interval and keeps the date/time live.
 */
export function IdleScreen({
  config,
  theme,
}: {
  config: DisplayConfigDto
  theme: ThemeClasses
}) {
  const { settings, branding } = config

  // ── Promo rotation ─────────────────────────────────────────────────────
  const [promoIndex, setPromoIndex] = useState(0)
  const promos = settings.promoMessages
  useEffect(() => {
    if (promos.length <= 1) return
    const id = setInterval(
      () => setPromoIndex((i) => (i + 1) % promos.length),
      Math.max(2000, settings.promoRotationSeconds * 1000),
    )
    return () => clearInterval(id)
  }, [promos.length, settings.promoRotationSeconds])

  // ── Live clock ─────────────────────────────────────────────────────────
  const [now, setNow] = useState<Date | null>(null)
  useEffect(() => {
    setNow(new Date())
    const id = setInterval(() => setNow(new Date()), 30_000) // minute precision
    return () => clearInterval(id)
  }, [])

  return (
    <div className="flex-1 flex flex-col items-center justify-center px-12 py-16 gap-12">

      {/* ── Brand block ───────────────────────────────────────────────── */}
      <div className="flex flex-col items-center gap-4 text-center">
        {settings.showLogo && branding.logoUrl ? (
          <Image
            src={branding.logoUrl}
            alt={branding.businessName}
            width={200}
            height={200}
            className="h-32 w-32 object-contain"
            unoptimized
          />
        ) : settings.showLogo ? (
          <div className={`flex h-24 w-24 items-center justify-center rounded-3xl bg-blue-500/90 shadow-lg`}>
            <Droplets className="h-12 w-12 text-white" />
          </div>
        ) : null}

        {settings.showBusinessName && (
          <h1 className="text-5xl font-bold tracking-tight">
            {branding.businessName}
          </h1>
        )}

        {settings.showTagline && branding.tagline && (
          <p className={`text-xl ${theme.textMuted}`}>{branding.tagline}</p>
        )}
      </div>

      {/* ── Divider ──────────────────────────────────────────────────── */}
      {(promos.length > 0 || branding.gCashNumber || branding.facebookUrl) && (
        <div className={`h-px w-1/3 ${theme.border} border-t`} />
      )}

      {/* ── Welcome + rotating promos ─────────────────────────────────── */}
      <div className="flex flex-col items-center gap-3 text-center min-h-[6rem]">
        <p className="text-2xl font-medium">🚗 Welcome! Mabuhay! 🚗</p>
        {promos.length > 0 && (
          <p
            key={promoIndex}
            className={`text-xl ${theme.accent} animate-fade-in max-w-2xl`}
          >
            &ldquo;{promos[promoIndex]}&rdquo;
          </p>
        )}
      </div>

      {/* ── Footer: date, contact, social ─────────────────────────────── */}
      <div className="flex flex-col items-center gap-3 text-center mt-auto">
        {settings.showDateTime && now && (
          <div className={theme.textMuted}>
            <p className="text-lg">
              {now.toLocaleDateString('en-PH', {
                weekday: 'long',
                year: 'numeric',
                month: 'long',
                day: 'numeric',
              })}
            </p>
            <p className="text-2xl font-semibold tabular-nums mt-1">
              {now.toLocaleTimeString('en-PH', {
                hour: '2-digit',
                minute: '2-digit',
              })}
            </p>
          </div>
        )}

        {settings.showSocialMedia && (branding.facebookUrl || branding.instagramHandle) && (
          <p className={`text-sm ${theme.textMuted}`}>
            {branding.facebookUrl && <span>facebook.com/{shortFb(branding.facebookUrl)}</span>}
            {branding.facebookUrl && branding.instagramHandle && <span> · </span>}
            {branding.instagramHandle && <span>@{branding.instagramHandle}</span>}
          </p>
        )}

        {settings.showGCashQr && branding.gCashNumber && (
          <p className={`text-sm ${theme.textMuted}`}>
            GCash: <span className="font-semibold tabular-nums">{branding.gCashNumber}</span>
          </p>
        )}
      </div>
    </div>
  )
}

function shortFb(url: string) {
  // Extract the last path segment so "facebook.com/aquashine" reads cleanly.
  return url.replace(/\/$/, '').split('/').pop() ?? url
}
