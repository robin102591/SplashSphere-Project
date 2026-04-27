'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { use, useEffect, useMemo, useState } from 'react'
import {
  ArrowLeft,
  Car,
  ChevronLeft,
  Clock,
  Loader2,
  Sparkles,
  XCircle,
} from 'lucide-react'
import { useTranslations } from 'next-intl'
import type {
  ConnectActiveQueueDto,
  ConnectBookingDetailDto,
  ConnectBookingServiceDto,
} from '@splashsphere/types'
import { QueueStatus } from '@splashsphere/types'
import { formatPeso, formatPesoNoSymbol } from '@splashsphere/format'
import { useActiveQueue } from '@/hooks/use-active-queue'
import { useBooking, useCancelBooking } from '@/hooks/use-bookings'
import { cn } from '@/lib/utils'

const MANILA_TZ = 'Asia/Manila'

const slotFormatter = new Intl.DateTimeFormat('en-PH', {
  weekday: 'short',
  month: 'short',
  day: 'numeric',
  year: 'numeric',
  hour: 'numeric',
  minute: '2-digit',
  hour12: true,
  timeZone: MANILA_TZ,
})

function formatSlot(iso: string): string {
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return '—'
  return slotFormatter.format(d)
}

const UPCOMING_SET = new Set(['Confirmed', 'Arrived', 'InService'])
const CANCELLABLE_SET = new Set(['Confirmed', 'Arrived'])
const QUEUE_VISIBLE_SET = new Set(['Arrived', 'InService'])

/**
 * Booking detail — header card, services, vehicle, live queue panel, and
 * the cancel action. The live queue panel polls
 * `GET /api/v1/connect/queue/active` every 10s (paused when the tab
 * is hidden) and only renders when the active queue entry actually maps
 * to this booking (`activeQueue.bookingId === booking.id`).
 */
export default function BookingDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  // Next 16 passes `params` as a thenable to support streaming; `use()`
  // unwraps it cleanly in a Client Component.
  const { id } = use(params)
  const t = useTranslations('bookings')
  const tDetail = useTranslations('bookings.detail')
  const router = useRouter()

  const {
    data: booking,
    isPending,
    isError,
    error,
    refetch,
    isFetching,
  } = useBooking(id)

  // Snapshot "now" once at mount via the `useState` lazy initializer so the
  // render pass stays pure (the React Compiler flags direct `Date.now()`
  // calls during render). Close enough for a cancel-button gate — the
  // backend enforces cancellability anyway.
  const [nowAtMount] = useState(() => Date.now())

  const isUpcoming = booking ? UPCOMING_SET.has(booking.status) : false
  const futureSlot = booking
    ? new Date(booking.slotStartUtc).getTime() > nowAtMount
    : false
  const isCancellable = booking
    ? CANCELLABLE_SET.has(booking.status) && futureSlot
    : false

  // Poll the active queue only when the booking is in a queue-active
  // phase. Avoids a background request for terminal/pre-arrival states.
  const shouldPollQueue = booking
    ? QUEUE_VISIBLE_SET.has(booking.status) || booking.queueEntryId !== null
    : false

  const { data: activeQueue } = useActiveQueue({ enabled: shouldPollQueue })

  // Only show the queue panel if the active entry belongs to this booking.
  const matchingQueue: ConnectActiveQueueDto | null =
    activeQueue && booking && activeQueue.bookingId === booking.id
      ? activeQueue
      : null

  const notFound =
    isError && (error as { status?: number } | undefined)?.status === 404

  if (isPending) {
    return (
      <>
        <DetailHeader backLabel={t('back')} title={t('title')} />
        <DetailSkeleton />
      </>
    )
  }

  if (notFound || !booking) {
    return (
      <>
        <DetailHeader backLabel={t('back')} title={t('title')} />
        <ErrorState
          title={notFound ? tDetail('notFound') : tDetail('loadError')}
          retryLabel={tDetail('retry')}
          onRetry={() => refetch()}
          isRetrying={isFetching}
        />
      </>
    )
  }

  if (isError) {
    return (
      <>
        <DetailHeader backLabel={t('back')} title={t('title')} />
        <ErrorState
          title={tDetail('loadError')}
          retryLabel={tDetail('retry')}
          onRetry={() => refetch()}
          isRetrying={isFetching}
        />
      </>
    )
  }

  return (
    <section className="space-y-4 pb-8">
      <DetailHeader
        backLabel={t('back')}
        title={t('title')}
        onBack={() => router.back()}
      />

      {matchingQueue?.status === QueueStatus.InService && (
        <NowWashingBanner label={t('queue.nowWashingBanner')} />
      )}

      <HeaderCard booking={booking} />

      {matchingQueue && (
        <QueuePanel queue={matchingQueue} />
      )}

      <ServicesCard services={booking.services} booking={booking} />

      <VehicleCard booking={booking} />

      {isUpcoming && (
        <ActionButtons
          bookingId={booking.id}
          showCancel={isCancellable}
        />
      )}
    </section>
  )
}

// ── Header bar ───────────────────────────────────────────────────────────────

function DetailHeader({
  backLabel,
  title,
  onBack,
}: {
  backLabel: string
  title: string
  onBack?: () => void
}) {
  return (
    <header className="flex items-center gap-3">
      {onBack ? (
        <button
          type="button"
          onClick={onBack}
          className={cn(
            'flex h-11 w-11 shrink-0 items-center justify-center rounded-full',
            'border border-border bg-card text-foreground',
            'transition active:scale-[0.97]',
          )}
          aria-label={backLabel}
        >
          <ChevronLeft className="h-5 w-5" aria-hidden />
        </button>
      ) : (
        <Link
          href="/bookings"
          className={cn(
            'flex h-11 w-11 shrink-0 items-center justify-center rounded-full',
            'border border-border bg-card text-foreground',
            'transition active:scale-[0.97]',
          )}
          aria-label={backLabel}
        >
          <ArrowLeft className="h-5 w-5" aria-hidden />
        </Link>
      )}
      <h1 className="truncate text-xl font-semibold leading-tight">{title}</h1>
    </header>
  )
}

// ── Header card ──────────────────────────────────────────────────────────────

function HeaderCard({ booking }: { booking: ConnectBookingDetailDto }) {
  const t = useTranslations('bookings.detail')
  const code = booking.id.slice(-8).toUpperCase()
  return (
    <article className="rounded-2xl border border-border bg-card p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate text-base font-semibold leading-tight">
            {booking.tenantName}
          </p>
          <p className="truncate text-sm text-muted-foreground">
            {booking.branchName}
          </p>
        </div>
        <ListStatusBadge status={booking.status} />
      </div>

      <p className="mt-3 text-sm text-foreground/90">
        {formatSlot(booking.slotStartUtc)}
      </p>

      <p className="mt-3 font-mono text-[11px] tracking-wider text-muted-foreground">
        {t('bookingCode', { code })}
      </p>
    </article>
  )
}

function ListStatusBadge({ status }: { status: string }) {
  const t = useTranslations('bookings.status')
  const tone = statusTone(status)
  const label = hasStatusKey(status) ? t(status) : status
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

function hasStatusKey(s: string): s is
  | 'Confirmed'
  | 'Arrived'
  | 'InService'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow' {
  return (
    s === 'Confirmed' ||
    s === 'Arrived' ||
    s === 'InService' ||
    s === 'Completed' ||
    s === 'Cancelled' ||
    s === 'NoShow'
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

// ── Services ─────────────────────────────────────────────────────────────────

function ServicesCard({
  services,
  booking,
}: {
  services: readonly ConnectBookingServiceDto[]
  booking: ConnectBookingDetailDto
}) {
  const t = useTranslations('bookings')
  const tDetail = useTranslations('bookings.detail')
  const isRange =
    !booking.isVehicleClassified &&
    booking.estimatedTotalMin !== null &&
    booking.estimatedTotalMax !== null

  return (
    <article className="rounded-2xl border border-border bg-card p-4 shadow-sm">
      <h2 className="text-sm font-semibold">{tDetail('servicesTitle')}</h2>
      <ul className="mt-3 divide-y divide-border">
        {services.map((s) => (
          <li
            key={s.serviceId}
            className="flex items-start justify-between gap-3 py-2.5"
          >
            <p className="min-w-0 flex-1 text-sm text-foreground">{s.name}</p>
            <p className="shrink-0 text-sm font-medium tabular-nums">
              {s.price !== null
                ? formatPeso(s.price)
                : s.priceMin !== null && s.priceMax !== null
                  ? t('estimatedRange', {
                      min: formatPesoNoSymbol(s.priceMin),
                      max: formatPesoNoSymbol(s.priceMax),
                    })
                  : '—'}
            </p>
          </li>
        ))}
      </ul>

      <div className="mt-3 flex items-center justify-between border-t border-border pt-3">
        <p className="text-sm font-semibold">
          {isRange ? tDetail('estimatedTotalLabel') : tDetail('totalLabel')}
        </p>
        <p className="text-base font-bold tabular-nums">
          {isRange
            ? t('estimatedRange', {
                min: formatPesoNoSymbol(
                  booking.estimatedTotalMin ?? 0,
                ),
                max: formatPesoNoSymbol(
                  booking.estimatedTotalMax ?? 0,
                ),
              })
            : formatPeso(booking.estimatedTotal)}
        </p>
      </div>
    </article>
  )
}

// ── Vehicle ──────────────────────────────────────────────────────────────────

function VehicleCard({ booking }: { booking: ConnectBookingDetailDto }) {
  const t = useTranslations('bookings.detail')
  return (
    <article className="rounded-2xl border border-border bg-card p-4 shadow-sm">
      <h2 className="text-sm font-semibold">{t('vehicleTitle')}</h2>
      <div className="mt-3 flex items-center gap-3">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-muted text-muted-foreground">
          <Car className="h-5 w-5" aria-hidden />
        </div>
        <p className="text-sm font-medium">{booking.plateNumber}</p>
      </div>
      {!booking.isVehicleClassified && (
        <p className="mt-3 rounded-xl bg-muted/60 px-3 py-2 text-xs text-muted-foreground">
          {t('unclassifiedHint')}
        </p>
      )}
    </article>
  )
}

// ── Queue panel ──────────────────────────────────────────────────────────────

function QueuePanel({ queue }: { queue: ConnectActiveQueueDto }) {
  const t = useTranslations('bookings.queue')

  if (queue.status === QueueStatus.InService) {
    return (
      <article className="rounded-2xl border border-sky-200 bg-sky-50 p-4 shadow-sm">
        <div className="flex items-center gap-2">
          <Sparkles className="h-4 w-4 text-sky-700" aria-hidden />
          <h2 className="text-sm font-semibold text-sky-800">{t('title')}</h2>
        </div>
        <p className="mt-2 text-sm font-semibold text-sky-900">
          {t('inServiceLabel')}
        </p>
        <p className="mt-3 text-3xl font-bold tabular-nums text-sky-900">
          #{queue.queueNumber}
        </p>
        {queue.startedAt && (
          <p className="mt-2 flex items-center gap-1.5 text-xs text-sky-800">
            <Clock className="h-3.5 w-3.5" aria-hidden />
            {t('elapsedLabel')}{' '}
            <span className="font-mono">
              <ElapsedClock startedAt={queue.startedAt} />
            </span>
          </p>
        )}
      </article>
    )
  }

  if (queue.status === QueueStatus.Called) {
    return (
      <article className="rounded-2xl border border-amber-200 bg-amber-50 p-4 shadow-sm">
        <h2 className="text-sm font-semibold text-amber-800">{t('title')}</h2>
        <p className="mt-2 text-base font-semibold text-amber-900">
          {t('calledLabel')}
        </p>
        <p className="mt-3 text-3xl font-bold tabular-nums text-amber-900">
          #{queue.queueNumber}
        </p>
      </article>
    )
  }

  // Waiting
  return (
    <article className="rounded-2xl border border-splash-100 bg-splash-50 p-4 shadow-sm">
      <h2 className="text-sm font-semibold text-splash-600">{t('title')}</h2>
      <p className="mt-2 text-base font-semibold text-foreground">
        {t('waitingLabel')}
      </p>
      <p className="mt-3 text-3xl font-bold tabular-nums text-foreground">
        #{queue.queueNumber}
      </p>
      {queue.aheadCount !== null && (
        <p className="mt-2 text-sm text-foreground/80">
          {t('aheadOfYou', { count: queue.aheadCount })}
        </p>
      )}
      {queue.estimatedWaitMinutes !== null && (
        <p className="mt-1 flex items-center gap-1.5 text-xs text-muted-foreground">
          <Clock className="h-3.5 w-3.5" aria-hidden />
          {t('estimatedWait', { minutes: queue.estimatedWaitMinutes })}
        </p>
      )}
    </article>
  )
}

/**
 * Ticking `mm:ss` (or `Xh Ym` past the hour) clock from a UTC
 * `startedAt`. Updates every second via a `setInterval`.
 */
function ElapsedClock({ startedAt }: { startedAt: string }) {
  const start = useMemo(() => new Date(startedAt).getTime(), [startedAt])
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    const id = setInterval(() => setNow(Date.now()), 1_000)
    return () => clearInterval(id)
  }, [])

  const elapsedMs = Math.max(0, now - start)
  const totalSeconds = Math.floor(elapsedMs / 1_000)
  const hours = Math.floor(totalSeconds / 3_600)
  const minutes = Math.floor((totalSeconds % 3_600) / 60)
  const seconds = totalSeconds % 60

  if (hours > 0) {
    return <>{`${hours}h ${minutes.toString().padStart(2, '0')}m`}</>
  }
  return (
    <>
      {minutes.toString().padStart(2, '0')}:
      {seconds.toString().padStart(2, '0')}
    </>
  )
}

function NowWashingBanner({ label }: { label: string }) {
  return (
    <div
      role="status"
      className={cn(
        'flex items-center gap-2 rounded-xl border border-sky-200 bg-sky-50',
        'px-3 py-2 text-sm font-medium text-sky-900',
      )}
    >
      <Sparkles className="h-4 w-4 text-sky-700" aria-hidden />
      {label}
    </div>
  )
}

// ── Actions ──────────────────────────────────────────────────────────────────

function ActionButtons({
  bookingId,
  showCancel,
}: {
  bookingId: string
  showCancel: boolean
}) {
  const t = useTranslations('bookings.actions')
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [errorMsg, setErrorMsg] = useState<string | null>(null)
  const cancel = useCancelBooking()

  // Auto-dismiss error after 6s so the banner doesn't linger.
  useEffect(() => {
    if (!errorMsg) return
    const id = setTimeout(() => setErrorMsg(null), 6_000)
    return () => clearTimeout(id)
  }, [errorMsg])

  const onConfirm = async () => {
    setErrorMsg(null)
    try {
      await cancel.mutateAsync({ id: bookingId })
      setConfirmOpen(false)
    } catch {
      setErrorMsg(t('cancelError'))
    }
  }

  if (!showCancel) return null

  return (
    <div className="space-y-3">
      {errorMsg && (
        <div
          role="alert"
          className={cn(
            'flex items-start gap-2 rounded-xl border border-destructive/30',
            'bg-destructive/5 px-3 py-2 text-sm text-destructive',
          )}
        >
          <XCircle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
          <p>{errorMsg}</p>
        </div>
      )}

      <button
        type="button"
        onClick={() => setConfirmOpen(true)}
        className={cn(
          'flex w-full items-center justify-center gap-2 rounded-full',
          'border border-destructive/40 bg-card px-4 py-3 text-sm font-semibold',
          'text-destructive transition active:scale-[0.98] min-h-[48px]',
        )}
      >
        {t('cancel')}
      </button>

      {confirmOpen && (
        <CancelDialog
          onConfirm={onConfirm}
          onCancel={() => setConfirmOpen(false)}
          isSubmitting={cancel.isPending}
        />
      )}
    </div>
  )
}

function CancelDialog({
  onConfirm,
  onCancel,
  isSubmitting,
}: {
  onConfirm: () => void
  onCancel: () => void
  isSubmitting: boolean
}) {
  const t = useTranslations('bookings.actions')
  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="cancel-dialog-title"
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 px-4 pb-6 sm:items-center"
    >
      <div className="w-full max-w-md rounded-2xl border border-border bg-card p-5 shadow-xl">
        <h3 id="cancel-dialog-title" className="text-base font-semibold">
          {t('confirmCancelTitle')}
        </h3>
        <p className="mt-2 text-sm text-muted-foreground">
          {t('confirmCancelBody')}
        </p>
        <div className="mt-5 flex flex-col gap-2">
          <button
            type="button"
            onClick={onConfirm}
            disabled={isSubmitting}
            className={cn(
              'flex items-center justify-center gap-2 rounded-full bg-destructive',
              'px-4 py-3 text-sm font-semibold text-destructive-foreground',
              'transition active:scale-[0.98] disabled:opacity-60 min-h-[48px]',
            )}
          >
            {isSubmitting && (
              <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
            )}
            {isSubmitting ? t('cancelling') : t('confirmCancelConfirm')}
          </button>
          <button
            type="button"
            onClick={onCancel}
            disabled={isSubmitting}
            className={cn(
              'flex items-center justify-center rounded-full border border-border',
              'bg-card px-4 py-3 text-sm font-semibold text-foreground',
              'transition active:scale-[0.98] disabled:opacity-60 min-h-[48px]',
            )}
          >
            {t('confirmCancelKeep')}
          </button>
        </div>
      </div>
    </div>
  )
}

// ── Skeleton / error states ──────────────────────────────────────────────────

function DetailSkeleton() {
  return (
    <div className="mt-4 space-y-3" aria-hidden>
      <div className="h-28 animate-pulse rounded-2xl border border-border bg-muted/60" />
      <div className="h-40 animate-pulse rounded-2xl border border-border bg-muted/60" />
      <div className="h-24 animate-pulse rounded-2xl border border-border bg-muted/60" />
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
    <div className="mt-4 rounded-2xl border border-destructive/30 bg-destructive/5 p-6 text-center">
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

// NOTE: the booking detail DTO intentionally doesn't expose branch
// lat/lng today, so we don't render a "Directions" button. When the
// backend adds geolocation we can hydrate a `https://maps.google.com/?q=lat,lng`
// deep-link alongside the Cancel action.
