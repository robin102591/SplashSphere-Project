'use client'

import Link from 'next/link'
import { useTranslations } from 'next-intl'
import { Calendar, Compass, Droplets, Sparkles } from 'lucide-react'
import { useConnectAuth } from '@/lib/auth/use-connect-auth'
import { useMyTenants } from '@/hooks/use-profile'
import { TenantCard } from '@/components/home/tenant-card'

// TODO(22.3-F): bookings link is a dead route until F lands

/**
 * Derive the user's first name for a friendly greeting. Handles single-word
 * names, multi-word names, and whitespace-only inputs.
 */
function firstName(full: string | undefined | null): string | null {
  if (!full) return null
  const first = full.trim().split(/\s+/)[0]
  return first || null
}

export default function HomePage() {
  const t = useTranslations('home')
  const tApp = useTranslations('app')
  const { user } = useConnectAuth()

  const { data: tenants, isPending, isError } = useMyTenants()
  const given = firstName(user?.name ?? null)

  return (
    <section className="space-y-6">
      {/* Greeting card */}
      <header className="rounded-2xl border border-border bg-card p-5">
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-primary text-primary-foreground">
            <Droplets className="h-6 w-6" aria-hidden />
          </div>
          <div className="min-w-0">
            <p className="text-xs text-muted-foreground">{tApp('name')}</p>
            <h1 className="text-xl font-semibold leading-tight">
              {given ? t('greetingNamed', { name: given }) : t('greetingAnon')}
            </h1>
          </div>
        </div>
      </header>

      {/* Quick actions */}
      <div className="grid grid-cols-2 gap-3">
        <Link
          href="/discover"
          className="flex min-h-[88px] flex-col items-start justify-between rounded-2xl bg-primary p-4 text-primary-foreground transition-transform active:scale-[0.97]"
        >
          <Sparkles className="h-5 w-5" aria-hidden />
          <span className="text-sm font-semibold leading-tight">
            {t('actions.bookWash')}
          </span>
        </Link>
        <Link
          href="/bookings"
          className="flex min-h-[88px] flex-col items-start justify-between rounded-2xl border border-border bg-card p-4 text-foreground transition-colors active:scale-[0.97] hover:bg-accent/30"
        >
          <Calendar
            className="h-5 w-5 text-muted-foreground"
            aria-hidden
          />
          <span className="text-sm font-semibold leading-tight">
            {t('actions.myBookings')}
          </span>
        </Link>
      </div>

      {/* My Car Washes */}
      <div className="space-y-3">
        <div className="flex items-baseline justify-between">
          <h2 className="text-base font-semibold">{t('myCarWashes.title')}</h2>
          {tenants && tenants.length > 0 && (
            <Link
              href="/discover"
              className="text-sm font-medium text-primary"
            >
              {t('myCarWashes.discoverMore')}
            </Link>
          )}
        </div>

        {isPending && <TenantsSkeleton />}

        {isError && (
          <p className="rounded-2xl border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
            {t('myCarWashes.error')}
          </p>
        )}

        {tenants && tenants.length === 0 && (
          <Link
            href="/discover"
            className="flex min-h-[88px] flex-col items-start justify-center gap-1 rounded-2xl border border-dashed border-border bg-card p-4 transition-colors active:scale-[0.99] hover:bg-accent/30"
          >
            <div className="flex items-center gap-2 text-foreground">
              <Compass className="h-5 w-5 text-primary" aria-hidden />
              <span className="text-sm font-semibold">
                {t('myCarWashes.emptyTitle')}
              </span>
            </div>
            <p className="text-sm text-muted-foreground">
              {t('myCarWashes.emptyCta')}
            </p>
          </Link>
        )}

        {tenants && tenants.length > 0 && (
          <ul className="space-y-3">
            {tenants.map((tenant) => (
              <li key={tenant.tenantId}>
                <TenantCard tenant={tenant} />
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}

/** Three-row skeleton that mimics the tenant-card height. */
function TenantsSkeleton() {
  return (
    <ul className="space-y-3" aria-busy="true">
      {[0, 1, 2].map((i) => (
        <li
          key={i}
          className="h-[72px] animate-pulse rounded-2xl border border-border bg-card"
        />
      ))}
    </ul>
  )
}
