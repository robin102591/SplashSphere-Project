'use client'

import { use } from 'react'
import Link from 'next/link'
import { useTranslations } from 'next-intl'
import {
  Car,
  CalendarDays,
  Clock,
  ExternalLink,
  ListOrdered,
  MapPin,
  Receipt,
  User,
} from 'lucide-react'

import { PageHeader } from '@/components/ui/page-header'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { useBookingDetail } from '@/hooks/use-bookings'
import { formatDate, formatTime, formatDateTime, formatPeso } from '@/lib/format'

export default function BookingDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const t = useTranslations('bookings.admin')

  const { data: booking, isLoading, isError } = useBookingDetail(id)

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-96" />
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Skeleton className="h-28 w-full" />
          <Skeleton className="h-28 w-full" />
          <Skeleton className="h-28 w-full" />
        </div>
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  if (isError || !booking) {
    return (
      <div className="space-y-4">
        <PageHeader title={t('notFoundTitle')} back="/dashboard/bookings" />
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          {t('notFoundDescription')}
        </div>
      </div>
    )
  }

  const vehicleLine = [booking.makeName, booking.modelName].filter(Boolean).join(' ')

  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader
        title={`${formatDate(booking.slotStartUtc)} · ${formatTime(booking.slotStartUtc)}`}
        description={`${t('createdAt')}: ${formatDateTime(booking.createdAtUtc)}`}
        back="/dashboard/bookings"
        badge={<StatusBadge status={booking.status} />}
        actions={
          <Link
            href="/dashboard/bookings"
            className="text-sm text-muted-foreground hover:text-foreground inline-flex items-center gap-1"
          >
            <ListOrdered className="h-4 w-4" />
            {t('backToList')}
          </Link>
        }
      />

      {/* Summary bar */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="rounded-lg border px-4 py-3">
          <p className="text-xs text-muted-foreground flex items-center gap-1">
            <CalendarDays className="h-3.5 w-3.5" /> {t('slot')}
          </p>
          <p className="font-semibold tabular-nums mt-0.5 text-sm">
            {formatTime(booking.slotStartUtc)} – {formatTime(booking.slotEndUtc)}
          </p>
          <p className="text-xs text-muted-foreground">{formatDate(booking.slotStartUtc)}</p>
        </div>
        <div className="rounded-lg border px-4 py-3">
          <p className="text-xs text-muted-foreground flex items-center gap-1">
            <Clock className="h-3.5 w-3.5" /> {t('duration')}
          </p>
          <p className="font-semibold tabular-nums mt-0.5 text-sm">
            {t('durationMinutes', { minutes: booking.estimatedDurationMinutes })}
          </p>
        </div>
        <div className="rounded-lg border px-4 py-3">
          <p className="text-xs text-muted-foreground flex items-center gap-1">
            <MapPin className="h-3.5 w-3.5" /> {t('branch')}
          </p>
          <p className="font-semibold mt-0.5 text-sm">{booking.branchName}</p>
        </div>
        <div className="rounded-lg border border-primary/30 bg-primary/5 px-4 py-3">
          <p className="text-xs text-muted-foreground">{t('estimatedTotal')}</p>
          <p className="text-lg font-bold tabular-nums mt-0.5 text-primary">
            {booking.isVehicleClassified
              ? formatPeso(booking.estimatedTotal)
              : booking.estimatedTotalMin != null && booking.estimatedTotalMax != null
                ? `${formatPeso(booking.estimatedTotalMin)} – ${formatPeso(booking.estimatedTotalMax)}`
                : formatPeso(booking.estimatedTotal)}
          </p>
        </div>
      </div>

      {/* Customer + vehicle */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div className="rounded-lg border px-4 py-3 space-y-1">
          <p className="text-xs text-muted-foreground flex items-center gap-1">
            <User className="h-3.5 w-3.5" /> {t('customer')}
          </p>
          <p className="font-medium">{booking.customerName}</p>
          {booking.customerPhone && (
            <p className="text-xs text-muted-foreground tabular-nums">
              {booking.customerPhone}
            </p>
          )}
          <Link
            href={`/dashboard/customers/${booking.customerId}`}
            className="text-xs text-primary hover:underline inline-flex items-center gap-1 pt-1"
          >
            View profile <ExternalLink className="h-3 w-3" />
          </Link>
        </div>
        <div className="rounded-lg border px-4 py-3 space-y-1">
          <p className="text-xs text-muted-foreground flex items-center gap-1">
            <Car className="h-3.5 w-3.5" /> {t('vehicle')}
          </p>
          <p className="font-mono font-semibold tabular-nums">{booking.plateNumber}</p>
          {vehicleLine && (
            <p className="text-xs text-muted-foreground">{vehicleLine}</p>
          )}
          {!booking.isVehicleClassified && (
            <p className="text-xs text-amber-600 dark:text-amber-400 pt-1">
              {t('vehicleUnclassified')}
            </p>
          )}
        </div>
      </div>

      {/* Services */}
      <section className="space-y-2">
        <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
          {t('services')} ({booking.services.length})
        </h3>
        <div className="rounded-lg border divide-y">
          {booking.services.map((s) => (
            <div
              key={s.serviceId}
              className="flex items-center justify-between px-4 py-3"
            >
              <span className="text-sm font-medium">{s.name}</span>
              <span className="text-sm tabular-nums">
                {s.price != null
                  ? formatPeso(s.price)
                  : s.priceMin != null && s.priceMax != null
                    ? `${formatPeso(s.priceMin)} – ${formatPeso(s.priceMax)}`
                    : '—'}
              </span>
            </div>
          ))}
        </div>
      </section>

      {/* Cancellation reason */}
      {booking.cancellationReason && (
        <div className="rounded-lg border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-950/30 p-4">
          <p className="text-xs uppercase tracking-wide text-red-700 dark:text-red-400 mb-1">
            {t('cancellationReason')}
          </p>
          <p className="text-sm">{booking.cancellationReason}</p>
        </div>
      )}

      {/* Links out */}
      {(booking.queueEntryId || booking.transactionId) && (
        <section className="space-y-2">
          <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
            Links
          </h3>
          <div className="rounded-lg border divide-y">
            {booking.queueEntryId && (
              <div className="flex items-center justify-between px-4 py-3">
                <div className="flex items-center gap-2 text-sm">
                  <ListOrdered className="h-4 w-4 text-muted-foreground" />
                  <span>
                    Queue #
                    <span className="tabular-nums">
                      {booking.queueEntryId.slice(-6)}
                    </span>
                  </span>
                </div>
                <Link
                  href="/dashboard/queue"
                  className="text-sm text-primary hover:underline inline-flex items-center gap-1"
                >
                  {t('viewQueue')} <ExternalLink className="h-3.5 w-3.5" />
                </Link>
              </div>
            )}
            {booking.transactionId && (
              <div className="flex items-center justify-between px-4 py-3">
                <div className="flex items-center gap-2 text-sm">
                  <Receipt className="h-4 w-4 text-muted-foreground" />
                  <span>Transaction</span>
                </div>
                <Link
                  href={`/dashboard/transactions/${booking.transactionId}`}
                  className="text-sm text-primary hover:underline inline-flex items-center gap-1"
                >
                  {t('viewTransaction')} <ExternalLink className="h-3.5 w-3.5" />
                </Link>
              </div>
            )}
          </div>
        </section>
      )}
    </div>
  )
}
