'use client'

import { Suspense, useEffect, useState } from 'react'
import { useSearchParams } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { Droplets } from 'lucide-react'
import {
  DisplayFontSize,
  DisplayOrientation,
  DisplayTheme,
  type DisplayConfigDto,
} from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { useDisplayConnection } from './use-display-connection'
import { IdleScreen } from './idle-screen'
import { BuildingScreen } from './building-screen'
import { CompleteScreen } from './complete-screen'

export default function DisplayLivePage() {
  return (
    <Suspense fallback={<BootSplash />}>
      <DisplayLive />
    </Suspense>
  )
}

function DisplayLive() {
  const params = useSearchParams()
  const branchId = params.get('branchId') ?? ''
  const stationId = params.get('stationId') ?? ''
  const { getToken } = useAuth()

  const { data: config, isLoading } = useQuery({
    queryKey: ['display-config', branchId],
    enabled: !!branchId,
    queryFn: async () => {
      const token = await getToken()
      const path = `/display/config?branchId=${encodeURIComponent(branchId)}`
      return apiClient.get<DisplayConfigDto>(path, token ?? undefined)
    },
  })

  const { state, connection } = useDisplayConnection({
    branchId,
    stationId,
    completionHoldSeconds: config?.settings.completionHoldSeconds ?? 10,
  })

  if (!branchId || !stationId) {
    return (
      <BootSplash message="Missing branch or station. Re-launch the display from the setup page." />
    )
  }

  if (isLoading || !config) return <BootSplash />

  const themeClasses = themeToClasses(config.settings.theme)
  const fontClasses = fontToClasses(config.settings.fontSize)
  const isPortrait = config.settings.orientation === DisplayOrientation.Portrait

  return (
    <div
      className={`min-h-screen flex flex-col select-none ${themeClasses.bg} ${themeClasses.text} ${fontClasses}`}
      data-orientation={isPortrait ? 'portrait' : 'landscape'}
    >
      {/* ── Reconnecting banner — subtle so it doesn't alarm customers ─── */}
      {connection === 'reconnecting' && (
        <div className={`text-center py-1.5 text-xs font-medium uppercase tracking-wider ${themeClasses.banner}`}>
          Reconnecting…
        </div>
      )}
      {connection === 'disconnected' && (
        <div className="bg-red-600/90 text-white text-center py-1.5 text-xs font-medium uppercase tracking-wider">
          Connection lost — Trying to reconnect…
        </div>
      )}

      {state.kind === 'idle' && <IdleScreen config={config} theme={themeClasses} />}
      {state.kind === 'building' && (
        <BuildingScreen
          transaction={state.transaction}
          settings={config.settings}
          branding={config.branding}
          theme={themeClasses}
        />
      )}
      {state.kind === 'complete' && (
        <CompleteScreen
          completion={state.completion}
          settings={config.settings}
          branding={config.branding}
          theme={themeClasses}
        />
      )}
    </div>
  )
}

// ── Boot splash (loading + missing-params) ────────────────────────────────────

function BootSplash({ message }: { message?: string }) {
  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center text-slate-300">
      <div className="text-center space-y-4">
        <div className="inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-500 animate-pulse">
          <Droplets className="h-9 w-9 text-white" />
        </div>
        <p className="text-sm text-slate-400">{message ?? 'Connecting display…'}</p>
      </div>
    </div>
  )
}

// ── Theme + font helpers (shared with state components) ──────────────────────

export interface ThemeClasses {
  bg: string
  text: string
  textMuted: string
  surface: string
  border: string
  accent: string
  banner: string
}

function themeToClasses(theme: DisplayTheme): ThemeClasses {
  switch (theme) {
    case DisplayTheme.Light:
      return {
        bg: 'bg-white',
        text: 'text-slate-900',
        textMuted: 'text-slate-500',
        surface: 'bg-slate-50',
        border: 'border-slate-200',
        accent: 'text-blue-600',
        banner: 'bg-amber-200 text-amber-900',
      }
    case DisplayTheme.Brand:
      // For now Brand resolves to Dark with blue accents until the company
      // profile feeds in a primary brand color.
      return {
        bg: 'bg-gradient-to-br from-blue-950 to-slate-950',
        text: 'text-white',
        textMuted: 'text-slate-400',
        surface: 'bg-blue-900/30',
        border: 'border-blue-800/40',
        accent: 'text-blue-300',
        banner: 'bg-blue-500/30 text-blue-100',
      }
    case DisplayTheme.Dark:
    default:
      return {
        bg: 'bg-slate-950',
        text: 'text-white',
        textMuted: 'text-slate-400',
        surface: 'bg-slate-900',
        border: 'border-slate-800',
        accent: 'text-blue-400',
        banner: 'bg-amber-500/30 text-amber-100',
      }
  }
}

function fontToClasses(size: DisplayFontSize): string {
  switch (size) {
    case DisplayFontSize.ExtraLarge:
      return 'text-[1.25rem]'
    case DisplayFontSize.Large:
      return 'text-[1.125rem]'
    case DisplayFontSize.Normal:
    default:
      return 'text-base'
  }
}
