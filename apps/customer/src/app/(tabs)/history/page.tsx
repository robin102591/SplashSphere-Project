'use client'

import Link from 'next/link'
import { useMemo } from 'react'
import { useTranslations } from 'next-intl'
import { ArrowRight, Building2, Sparkles } from 'lucide-react'
import type { ConnectHistoryItemDto } from '@splashsphere/types'
import { useHistory } from '@/hooks/use-history'
import { formatShortDateTime, groupByMonth } from '@/lib/date'
import { cn } from '@/lib/utils'

/**
 * Peso formatter using the native `en-PH` locale — gives us the proper
 * "₱" prefix and the usual 1,234.56 grouping without a 3rd-party dep.
 */
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
 * History tab — the signed-in customer's completed transactions across
 * every SplashSphere-powered branch they've visited, grouped by
 * Manila-time calendar month, newest first.
 *
 * Endpoint: `GET /api/v1/connect/history` returns a flat array of
 * `ConnectHistoryItemDto` (server caps at 200). No pagination wrapper,
 * so we render the whole list in one pass.
 */
export default function HistoryPage() {
  const t = useTranslations('history')
  const { data, isPending, isError, refetch, isFetching } = useHistory()

  const groups = useMemo(
    () => groupByMonth(data ?? [], (item) => item.completedAt),
    [data],
  )

  return (
    <section className="space-y-6">
      <header>
        <h1 className="text-xl font-semibold leading-tight">{t('title')}</h1>
        <p className="text-sm text-muted-foreground">{t('subtitle')}</p>
      </header>

      {isPending ? (
        <HistorySkeleton />
      ) : isError ? (
        <HistoryError
          title={t('errorTitle')}
          retryLabel={t('errorRetry')}
          onRetry={() => refetch()}
          isRetrying={isFetching}
        />
      ) : groups.length === 0 ? (
        <EmptyHistory
          title={t('emptyTitle')}
          ctaLabel={t('emptyCta')}
        />
      ) : (
        <div className="space-y-6">
          {groups.map((group) => (
            <div key={group.key} className="space-y-3">
              <h2
                className={cn(
                  'sticky top-0 z-10 -mx-4 px-4 py-2',
                  'bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/70',
                  'text-xs font-semibold uppercase tracking-wide text-muted-foreground',
                )}
              >
                {group.label}
              </h2>
              <ul className="space-y-3">
                {group.items.map((item) => (
                  <li key={item.transactionId}>
                    <HistoryCard item={item} pointsLabel={t('pointsEarned', { points: item.pointsEarned })} />
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      )}
    </section>
  )
}

// ── Card ─────────────────────────────────────────────────────────────────────

function HistoryCard({
  item,
  pointsLabel,
}: {
  item: ConnectHistoryItemDto
  pointsLabel: string
}) {
  const services = item.serviceNames.length > 0
    ? item.serviceNames.join(' · ')
    : '—'

  return (
    <article className="rounded-2xl border border-border bg-card p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted text-muted-foreground">
              <Building2 className="h-4 w-4" aria-hidden />
            </div>
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold leading-tight">
                {item.tenantName}
              </p>
              <p className="truncate text-xs text-muted-foreground">
                {item.branchName}
              </p>
            </div>
          </div>
        </div>
        <div className="shrink-0 text-right">
          <p className="text-base font-semibold leading-tight tabular-nums">
            {formatPeso(item.finalAmount)}
          </p>
          <p className="text-[11px] text-muted-foreground">
            {formatShortDateTime(item.completedAt)}
          </p>
        </div>
      </div>

      <p className="mt-3 text-sm text-foreground/90 line-clamp-2">
        {services}
      </p>

      <div className="mt-3 flex items-center justify-between gap-2">
        <p className="truncate text-xs text-muted-foreground">
          {item.plateNumber}
        </p>
        {item.pointsEarned > 0 && (
          <span className="inline-flex items-center gap-1 rounded-full bg-splash-50 px-2 py-0.5 text-[11px] font-semibold text-splash-600">
            <Sparkles className="h-3 w-3" aria-hidden />
            {pointsLabel}
          </span>
        )}
      </div>
    </article>
  )
}

// ── States ───────────────────────────────────────────────────────────────────

function HistorySkeleton() {
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

function EmptyHistory({
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
          'transition active:scale-[0.98]',
        )}
      >
        {ctaLabel}
        <ArrowRight className="h-4 w-4" aria-hidden />
      </Link>
    </div>
  )
}

function HistoryError({
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
          'text-sm font-semibold text-foreground',
          'transition active:scale-[0.98] disabled:opacity-60',
        )}
      >
        {retryLabel}
      </button>
    </div>
  )
}
