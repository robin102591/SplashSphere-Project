'use client'

import Link from 'next/link'
import { useRouter, useSearchParams } from 'next/navigation'
import { useMemo, useState } from 'react'
import { ArrowLeft, ArrowRight, Building2, ChevronRight } from 'lucide-react'
import { useTranslations } from 'next-intl'
import type { ConnectBookingListItemDto } from '@splashsphere/types'
import { useBookings } from '@/hooks/use-bookings'
import { formatShortDateTime } from '@/lib/date'
import { cn } from '@/lib/utils'

type Tab = 'upcoming' | 'past'

const UPCOMING_STATUSES = new Set(['Confirmed', 'Arrived', 'InService'])
const TERMINAL_STATUSES = new Set(['Completed', 'Cancelled', 'NoShow'])

const pesoFormatter = new Intl.NumberFormat('en-PH', {
  style: 'currency',
  currency: 'PHP',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

function formatPeso(amount: number): string {
  return pesoFormatter.format(amount ?? 0)
}

/**
 * My Bookings — list view with Upcoming/Past tabs.
 *
 * The backend only has a single boolean filter (`?includePast=true`):
 *
 *  - `Upcoming` tab: request with `includePast=false`. The backend already
 *    scopes this to Confirmed/Arrived/InService with `slotStart >= now`.
 *  - `Past` tab: request with `includePast=true` (returns everything) and
 *    client-filter to terminal statuses (Completed/Cancelled/NoShow) OR
 *    any slot whose `slotStartUtc < now`. That client filter keeps walk-
 *    past (but still in an upcoming state) bookings out of "Past".
 *
 * Tab state lives in the URL (`?tab=upcoming|past`) so deep-links work.
 */
export default function BookingsListPage() {
  const t = useTranslations('bookings')
  const router = useRouter()
  const searchParams = useSearchParams()
  const tabParam = searchParams?.get('tab')
  const activeTab: Tab = tabParam === 'past' ? 'past' : 'upcoming'

  // Request `includePast=true` only when the Past tab is selected. React
  // Query caches each mode separately so the other tab stays warm.
  const { data, isPending, isError, refetch, isFetching } = useBookings(
    activeTab === 'past',
  )

  // Snapshot "now" once at mount so render stays pure (React Compiler flags
  // direct `Date.now()` calls during render). Close enough — the Past tab
  // is fetched on demand with a fresh server-time filter anyway.
  const [nowAtMount] = useState(() => Date.now())

  const items = useMemo(() => {
    if (!data) return []
    const now = nowAtMount
    if (activeTab === 'upcoming') {
      // Backend already filtered, but defensive-sort asc by slotStart.
      return [...data].sort(
        (a, b) =>
          new Date(a.slotStartUtc).getTime() -
          new Date(b.slotStartUtc).getTime(),
      )
    }
    // Past: terminal status OR slot in the past.
    return data
      .filter(
        (b) =>
          TERMINAL_STATUSES.has(b.status) ||
          (!UPCOMING_STATUSES.has(b.status) &&
            new Date(b.slotStartUtc).getTime() < now) ||
          new Date(b.slotStartUtc).getTime() < now,
      )
      .slice()
      .sort(
        (a, b) =>
          new Date(b.slotStartUtc).getTime() -
          new Date(a.slotStartUtc).getTime(),
      )
  }, [data, activeTab, nowAtMount])

  const setTab = (tab: Tab) => {
    const next = new URLSearchParams(searchParams?.toString() ?? '')
    if (tab === 'upcoming') next.delete('tab')
    else next.set('tab', tab)
    const qs = next.toString()
    router.replace(qs ? `/bookings?${qs}` : '/bookings')
  }

  return (
    <section className="space-y-4">
      <Header title={t('title')} backLabel={t('back')} />

      <TabBar
        activeTab={activeTab}
        onChange={setTab}
        upcomingLabel={t('tabs.upcoming')}
        pastLabel={t('tabs.past')}
      />

      {isPending ? (
        <ListSkeleton />
      ) : isError ? (
        <ErrorState
          title={t('errorTitle')}
          retryLabel={t('errorRetry')}
          isRetrying={isFetching}
          onRetry={() => refetch()}
        />
      ) : items.length === 0 ? (
        activeTab === 'upcoming' ? (
          <EmptyUpcoming
            title={t('emptyUpcomingTitle')}
            ctaLabel={t('emptyUpcomingCta')}
          />
        ) : (
          <EmptyPast title={t('emptyPastTitle')} />
        )
      ) : (
        <ul className="space-y-3">
          {items.map((item) => (
            <li key={item.id}>
              <BookingRow item={item} />
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

// ── Header / Tabs ────────────────────────────────────────────────────────────

function Header({ title, backLabel }: { title: string; backLabel: string }) {
  return (
    <header className="flex items-center gap-3">
      <Link
        href="/"
        className={cn(
          'flex h-11 w-11 shrink-0 items-center justify-center rounded-full',
          'border border-border bg-card text-foreground',
          'transition active:scale-[0.97]',
        )}
        aria-label={backLabel}
      >
        <ArrowLeft className="h-5 w-5" aria-hidden />
      </Link>
      <h1 className="text-xl font-semibold leading-tight">{title}</h1>
    </header>
  )
}

function TabBar({
  activeTab,
  onChange,
  upcomingLabel,
  pastLabel,
}: {
  activeTab: Tab
  onChange: (tab: Tab) => void
  upcomingLabel: string
  pastLabel: string
}) {
  return (
    <div
      role="tablist"
      aria-label="Bookings filter"
      className="inline-flex w-full rounded-full border border-border bg-card p-1"
    >
      <TabButton
        isActive={activeTab === 'upcoming'}
        onClick={() => onChange('upcoming')}
        label={upcomingLabel}
      />
      <TabButton
        isActive={activeTab === 'past'}
        onClick={() => onChange('past')}
        label={pastLabel}
      />
    </div>
  )
}

function TabButton({
  isActive,
  onClick,
  label,
}: {
  isActive: boolean
  onClick: () => void
  label: string
}) {
  return (
    <button
      type="button"
      role="tab"
      aria-selected={isActive}
      onClick={onClick}
      className={cn(
        'flex-1 rounded-full px-4 py-2 text-sm font-semibold transition',
        'active:scale-[0.98]',
        isActive
          ? 'bg-primary text-primary-foreground shadow-sm'
          : 'text-muted-foreground hover:text-foreground',
      )}
    >
      {label}
    </button>
  )
}

// ── Row ──────────────────────────────────────────────────────────────────────

function BookingRow({ item }: { item: ConnectBookingListItemDto }) {
  const t = useTranslations('bookings')
  return (
    <Link
      href={`/bookings/${item.id}`}
      className={cn(
        'flex items-start gap-3 rounded-2xl border border-border bg-card p-4 shadow-sm',
        'transition active:scale-[0.99] min-h-[96px]',
      )}
    >
      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-muted text-muted-foreground">
        <Building2 className="h-5 w-5" aria-hidden />
      </div>

      <div className="min-w-0 flex-1">
        <div className="flex items-start justify-between gap-2">
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold leading-tight">
              {item.tenantName}
            </p>
            <p className="truncate text-xs text-muted-foreground">
              {item.branchName}
            </p>
          </div>
          <StatusBadge status={item.status} />
        </div>

        <p className="mt-1.5 text-xs text-muted-foreground">
          {formatShortDateTime(item.slotStartUtc)}
        </p>

        <div className="mt-2 flex items-center justify-between gap-2">
          <p className="truncate text-xs text-foreground/80">
            {item.plateNumber}
          </p>
          <p className="shrink-0 text-sm font-semibold tabular-nums">
            {item.isVehicleClassified ||
            item.estimatedTotalMin === null ||
            item.estimatedTotalMax === null
              ? formatPeso(item.estimatedTotal)
              : t('estimatedRange', {
                  min: pesoFormatter
                    .format(item.estimatedTotalMin)
                    .replace('₱', ''),
                  max: pesoFormatter
                    .format(item.estimatedTotalMax)
                    .replace('₱', ''),
                })}
          </p>
        </div>
      </div>

      <ChevronRight
        className="mt-3 h-4 w-4 shrink-0 text-muted-foreground"
        aria-hidden
      />
    </Link>
  )
}

// ── Status badge ─────────────────────────────────────────────────────────────

function StatusBadge({ status }: { status: string }) {
  const t = useTranslations('bookings.status')
  const tone = statusTone(status)
  // next-intl's `t()` throws for missing keys; guard first.
  const label = hasStatusTranslation(status) ? t(status) : status
  return (
    <span
      className={cn(
        'inline-flex shrink-0 items-center rounded-full px-2 py-0.5',
        'text-[11px] font-semibold leading-none',
        tone,
      )}
    >
      {label}
    </span>
  )
}

function hasStatusTranslation(status: string): status is
  | 'Confirmed'
  | 'Arrived'
  | 'InService'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow' {
  return (
    status === 'Confirmed' ||
    status === 'Arrived' ||
    status === 'InService' ||
    status === 'Completed' ||
    status === 'Cancelled' ||
    status === 'NoShow'
  )
}

function statusTone(status: string): string {
  switch (status) {
    case 'Confirmed':
      return 'bg-splash-50 text-splash-600'
    case 'Arrived':
      return 'bg-amber-50 text-amber-700'
    case 'InService':
      return 'bg-sky-50 text-sky-700'
    case 'Completed':
      return 'bg-emerald-50 text-emerald-700'
    case 'Cancelled':
      return 'bg-muted text-muted-foreground'
    case 'NoShow':
      return 'bg-rose-50 text-rose-700'
    default:
      return 'bg-muted text-muted-foreground'
  }
}

// ── States ───────────────────────────────────────────────────────────────────

function ListSkeleton() {
  return (
    <div className="space-y-3" aria-hidden>
      {Array.from({ length: 4 }).map((_, idx) => (
        <div
          key={idx}
          className="h-28 animate-pulse rounded-2xl border border-border bg-muted/60"
        />
      ))}
    </div>
  )
}

function EmptyUpcoming({
  title,
  ctaLabel,
}: {
  title: string
  ctaLabel: string
}) {
  return (
    <div className="rounded-2xl border border-dashed border-border bg-card p-8 text-center">
      <p className="text-base font-medium">{title}</p>
      <Link
        href="/discover"
        className={cn(
          'mt-4 inline-flex items-center gap-1 rounded-full bg-primary px-4 py-2',
          'text-sm font-semibold text-primary-foreground',
          'transition active:scale-[0.98] min-h-[44px]',
        )}
      >
        {ctaLabel}
        <ArrowRight className="h-4 w-4" aria-hidden />
      </Link>
    </div>
  )
}

function EmptyPast({ title }: { title: string }) {
  return (
    <div className="rounded-2xl border border-dashed border-border bg-card p-8 text-center">
      <p className="text-base font-medium">{title}</p>
    </div>
  )
}

function ErrorState({
  title,
  retryLabel,
  onRetry,
  isRetrying,
}: {
  title: string
  retryLabel: string
  onRetry: () => void
  isRetrying: boolean
}) {
  return (
    <div className="rounded-2xl border border-destructive/30 bg-destructive/5 p-6 text-center">
      <p className="text-sm font-medium text-foreground">{title}</p>
      <button
        type="button"
        onClick={onRetry}
        disabled={isRetrying}
        className={cn(
          'mt-3 inline-flex items-center rounded-full border border-border bg-card px-4 py-2',
          'text-sm font-semibold text-foreground min-h-[44px]',
          'transition active:scale-[0.98] disabled:opacity-60',
        )}
      >
        {retryLabel}
      </button>
    </div>
  )
}
