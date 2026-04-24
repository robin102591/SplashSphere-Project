'use client'

import { useMemo, useState } from 'react'
import Link from 'next/link'
import { useRouter, useSearchParams } from 'next/navigation'
import { useTranslations } from 'next-intl'
import {
  CalendarCheck,
  CalendarDays,
  ExternalLink,
  LayoutGrid,
  List as ListIcon,
  Lock,
} from 'lucide-react'

import { PageHeader } from '@/components/ui/page-header'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from '@/components/ui/dialog'
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from '@/components/ui/table'

import { useBranches } from '@/hooks/use-branches'
import { useHasFeature } from '@/hooks/use-plan'
import {
  useBookings,
  useBookingDetail,
  useBookingSettings,
} from '@/hooks/use-bookings'
import { FeatureKeys } from '@splashsphere/types'
import type { BookingListItemDto } from '@splashsphere/types'
import { formatDate, formatTime, formatDateTime } from '@/lib/format'
import { formatPeso } from '@/lib/format'
import { cn } from '@/lib/utils'

// ── Helpers ──────────────────────────────────────────────────────────────────

function startOfWeek(d: Date): Date {
  const out = new Date(d)
  const day = out.getDay() // 0=Sun..6=Sat
  out.setHours(0, 0, 0, 0)
  out.setDate(out.getDate() - day)
  return out
}
function addDays(d: Date, n: number): Date {
  const out = new Date(d)
  out.setDate(out.getDate() + n)
  return out
}
function toDateInputValue(d: Date): string {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

/** Format booking price for list: single value or range. */
function formatBookingPrice(b: BookingListItemDto): string {
  if (b.isVehicleClassified) return formatPeso(b.estimatedTotal)
  if (b.estimatedTotalMin != null && b.estimatedTotalMax != null) {
    return `${formatPeso(b.estimatedTotalMin)} – ${formatPeso(b.estimatedTotalMax)}`
  }
  return formatPeso(b.estimatedTotal)
}

const STATUS_OPTIONS = [
  { value: 'Confirmed', label: 'Confirmed' },
  { value: 'Arrived',   label: 'Arrived' },
  { value: 'InService', label: 'In Service' },
  { value: 'Completed', label: 'Completed' },
  { value: 'NoShow',    label: 'No Show' },
  { value: 'Cancelled', label: 'Cancelled' },
]

// ── Upsell ───────────────────────────────────────────────────────────────────

function BookingUpsell({ title, message }: { title: string; message: string }) {
  return (
    <div className="border-2 border-dashed border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-950/30 rounded-xl p-8 text-center">
      <Lock className="h-8 w-8 text-amber-400 mx-auto mb-3" />
      <p className="text-amber-800 dark:text-amber-200 font-semibold mb-1">{title}</p>
      <p className="text-amber-600 dark:text-amber-400 text-sm mb-4">{message}</p>
      <a
        href="/dashboard/subscription"
        className="text-sm font-semibold text-primary hover:underline"
      >
        View Plans &rarr;
      </a>
    </div>
  )
}

// ── Detail dialog ────────────────────────────────────────────────────────────

function BookingDetailDialog({
  id, open, onOpenChange, t,
}: {
  id: string | null
  open: boolean
  onOpenChange: (v: boolean) => void
  t: ReturnType<typeof useTranslations>
}) {
  const { data: booking, isLoading } = useBookingDetail(open ? id : null)

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-xl">
        <DialogHeader>
          <DialogTitle>{t('detailTitle')}</DialogTitle>
          <DialogDescription>{t('detailDescription')}</DialogDescription>
          {id && (
            <Link
              href={`/dashboard/bookings/${id}`}
              onClick={() => onOpenChange(false)}
              className="inline-flex items-center gap-1 text-xs text-primary hover:underline pt-1 w-fit"
            >
              {t('openFullPage')} <ExternalLink className="h-3 w-3" />
            </Link>
          )}
        </DialogHeader>

        {isLoading || !booking ? (
          <div className="space-y-3 py-4">
            <Skeleton className="h-5 w-2/3" />
            <Skeleton className="h-5 w-1/2" />
            <Skeleton className="h-24 w-full" />
          </div>
        ) : (
          <div className="space-y-5 py-2">
            {/* Header row */}
            <div className="flex items-center gap-2">
              <StatusBadge status={booking.status} />
              <span className="text-xs text-muted-foreground">
                {t('createdAt')}: {formatDateTime(booking.createdAtUtc)}
              </span>
            </div>

            {/* Slot */}
            <div className="rounded-lg border p-3 space-y-1">
              <p className="text-xs uppercase tracking-wide text-muted-foreground">{t('slot')}</p>
              <p className="text-sm font-medium">
                {formatDate(booking.slotStartUtc)} · {formatTime(booking.slotStartUtc)} – {formatTime(booking.slotEndUtc)}
              </p>
              <p className="text-xs text-muted-foreground">
                {t('branch')}: {booking.branchName}
              </p>
            </div>

            {/* Customer + vehicle */}
            <div className="grid sm:grid-cols-2 gap-3">
              <div className="rounded-lg border p-3 space-y-1">
                <p className="text-xs uppercase tracking-wide text-muted-foreground">{t('customer')}</p>
                <p className="text-sm font-medium">{booking.customerName}</p>
                {booking.customerPhone && (
                  <p className="text-xs text-muted-foreground">{booking.customerPhone}</p>
                )}
              </div>
              <div className="rounded-lg border p-3 space-y-1">
                <p className="text-xs uppercase tracking-wide text-muted-foreground">{t('vehicle')}</p>
                <p className="text-sm font-medium tabular-nums">{booking.plateNumber}</p>
                {(booking.makeName || booking.modelName) && (
                  <p className="text-xs text-muted-foreground">
                    {[booking.makeName, booking.modelName].filter(Boolean).join(' ')}
                  </p>
                )}
                {!booking.isVehicleClassified && (
                  <p className="text-xs text-amber-600 dark:text-amber-400">
                    {t('vehicleUnclassified')}
                  </p>
                )}
              </div>
            </div>

            {/* Services */}
            <div>
              <p className="text-xs uppercase tracking-wide text-muted-foreground mb-2">{t('services')}</p>
              <div className="rounded-lg border divide-y">
                {booking.services.map((s) => (
                  <div key={s.serviceId} className="flex items-center justify-between px-3 py-2">
                    <span className="text-sm">{s.name}</span>
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
            </div>

            {/* Total */}
            <div className="rounded-lg border p-3 flex items-center justify-between">
              <span className="text-sm font-medium">{t('estimatedTotal')}</span>
              <span className="text-sm font-semibold tabular-nums">
                {booking.isVehicleClassified
                  ? formatPeso(booking.estimatedTotal)
                  : booking.estimatedTotalMin != null && booking.estimatedTotalMax != null
                    ? `${formatPeso(booking.estimatedTotalMin)} – ${formatPeso(booking.estimatedTotalMax)}`
                    : formatPeso(booking.estimatedTotal)}
              </span>
            </div>

            {booking.cancellationReason && (
              <div className="rounded-lg border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-950/30 p-3">
                <p className="text-xs uppercase tracking-wide text-red-700 dark:text-red-400 mb-1">
                  {t('cancellationReason')}
                </p>
                <p className="text-sm">{booking.cancellationReason}</p>
              </div>
            )}

            {/* Links */}
            {(booking.queueEntryId || booking.transactionId) && (
              <div className="flex flex-wrap gap-2 text-xs text-muted-foreground">
                {booking.queueEntryId && <span>Queue #{booking.queueEntryId.slice(-6)}</span>}
                {booking.transactionId && (
                  <a
                    href={`/dashboard/transactions/${booking.transactionId}`}
                    className="text-primary hover:underline"
                  >
                    {t('viewTransaction')} &rarr;
                  </a>
                )}
              </div>
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}

// ── Calendar view ────────────────────────────────────────────────────────────

function CalendarView({
  bookings,
  weekStart,
  openTime,
  closeTime,
  onSelect,
  t,
}: {
  bookings: BookingListItemDto[]
  weekStart: Date
  openTime: string // "HH:mm"
  closeTime: string
  onSelect: (id: string) => void
  t: ReturnType<typeof useTranslations>
}) {
  const startHour = Math.max(0, Math.floor(Number(openTime.split(':')[0] ?? '8')))
  const endHour = Math.min(24, Math.ceil(Number(closeTime.split(':')[0] ?? '20') + (Number(closeTime.split(':')[1] ?? '0') > 0 ? 1 : 0)))
  const hours = Array.from({ length: Math.max(1, endHour - startHour) }, (_, i) => startHour + i)
  const days = Array.from({ length: 7 }, (_, i) => addDays(weekStart, i))

  // Bucket bookings by (dayIndex, hourSlot).
  const grid = useMemo(() => {
    const out: Record<string, BookingListItemDto[]> = {}
    for (const b of bookings) {
      const start = new Date(b.slotStartUtc)
      // Find which day column — compare local date.
      const dayIdx = days.findIndex((d) =>
        d.getFullYear() === start.getFullYear() &&
        d.getMonth() === start.getMonth() &&
        d.getDate() === start.getDate(),
      )
      if (dayIdx < 0) continue
      const hr = start.getHours()
      if (hr < startHour || hr >= endHour) continue
      const key = `${dayIdx}-${hr}`
      if (!out[key]) out[key] = []
      out[key].push(b)
    }
    return out
  }, [bookings, days, startHour, endHour])

  const dayLabelFormatter = new Intl.DateTimeFormat('en-PH', {
    timeZone: 'Asia/Manila',
    weekday: 'short',
    month: 'short',
    day: 'numeric',
  })

  return (
    <div className="rounded-lg border overflow-hidden">
      {/* Header row: day columns */}
      <div
        className="grid border-b bg-muted/30 text-xs font-medium"
        style={{ gridTemplateColumns: '64px repeat(7, 1fr)' }}
      >
        <div className="p-2 border-r" />
        {days.map((d, i) => (
          <div key={i} className="p-2 text-center border-r last:border-r-0">
            {dayLabelFormatter.format(d)}
          </div>
        ))}
      </div>

      {/* Hour rows */}
      {hours.map((h) => (
        <div
          key={h}
          className="grid border-b last:border-b-0"
          style={{ gridTemplateColumns: '64px repeat(7, 1fr)' }}
        >
          <div className="p-2 text-xs text-muted-foreground text-right border-r tabular-nums">
            {String(h).padStart(2, '0')}:00
          </div>
          {days.map((_, di) => {
            const cell = grid[`${di}-${h}`] ?? []
            return (
              <div
                key={di}
                className="min-h-16 p-1.5 border-r last:border-r-0 space-y-1 align-top"
              >
                {cell.map((b) => (
                  <button
                    key={b.id}
                    type="button"
                    onClick={() => onSelect(b.id)}
                    className={cn(
                      'w-full text-left rounded px-1.5 py-1 text-[11px] leading-tight',
                      'border hover:shadow-sm transition',
                      chipClassForStatus(b.status),
                    )}
                  >
                    <div className="font-medium tabular-nums">
                      {formatTime(b.slotStartUtc)}
                    </div>
                    <div className="truncate">{b.customerName}</div>
                    <div className="truncate opacity-80">{b.plateNumber}</div>
                  </button>
                ))}
              </div>
            )
          })}
        </div>
      ))}

      {bookings.length === 0 && (
        <div className="p-10 text-center text-sm text-muted-foreground">
          {t('noBookingsInRange')}
        </div>
      )}
    </div>
  )
}

function chipClassForStatus(status: string): string {
  switch (status) {
    case 'Confirmed': return 'bg-blue-500/10 border-blue-200 text-blue-800 dark:text-blue-300 dark:border-blue-800'
    case 'Arrived':   return 'bg-indigo-500/10 border-indigo-200 text-indigo-800 dark:text-indigo-300 dark:border-indigo-800'
    case 'InService': return 'bg-amber-500/10 border-amber-200 text-amber-800 dark:text-amber-300 dark:border-amber-800'
    case 'Completed': return 'bg-emerald-500/10 border-emerald-200 text-emerald-800 dark:text-emerald-300 dark:border-emerald-800'
    case 'NoShow':    return 'bg-red-500/10 border-red-200 text-red-800 dark:text-red-300 dark:border-red-800'
    case 'Cancelled': return 'bg-gray-500/10 border-gray-200 text-gray-700 dark:text-gray-300 dark:border-gray-700'
    default:          return 'bg-gray-500/10 border-gray-200 text-gray-700'
  }
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function BookingsPage() {
  const t = useTranslations('bookings.admin')
  const tCommon = useTranslations('common')
  const router = useRouter()
  const searchParams = useSearchParams()

  const hasFeature = useHasFeature(FeatureKeys.OnlineBooking)

  // URL-synced view param.
  const view = (searchParams.get('view') === 'calendar' ? 'calendar' : 'list') as 'list' | 'calendar'
  const setView = (next: 'list' | 'calendar') => {
    const params = new URLSearchParams(searchParams.toString())
    params.set('view', next)
    router.replace(`?${params.toString()}`)
  }

  // Filters
  const today = new Date()
  const weekStartDefault = startOfWeek(today)
  const weekEndDefault = addDays(weekStartDefault, 6)

  const [fromDate, setFromDate] = useState<string>(toDateInputValue(weekStartDefault))
  const [toDate, setToDate] = useState<string>(toDateInputValue(weekEndDefault))
  const [branchId, setBranchId] = useState<string>('')
  const [status, setStatus] = useState<string>('')

  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)

  const { data: branches } = useBranches()

  // When status filter is set, forward its BookingStatus enum number to the API.
  const statusNumber = useMemo(() => {
    const map: Record<string, number> = {
      Confirmed: 1, Arrived: 2, InService: 3, Completed: 4, Cancelled: 5, NoShow: 6,
    }
    return status ? String(map[status] ?? '') : ''
  }, [status])

  const { data: bookings, isLoading } = useBookings(
    {
      fromDate: fromDate,
      toDate:   toDate,
      branchId: branchId || undefined,
      status:   statusNumber || undefined,
    },
    hasFeature,
  )

  // Booking settings for calendar (only meaningful when a specific branch is chosen).
  const { data: bookingSettings } = useBookingSettings(branchId || undefined)
  const openTime = bookingSettings?.openTime?.slice(0, 5) ?? '08:00'
  const closeTime = bookingSettings?.closeTime?.slice(0, 5) ?? '20:00'

  if (!hasFeature) {
    return (
      <div className="space-y-6">
        <PageHeader title={t('title')} description={t('subtitle')} />
        <BookingUpsell title={t('upsellTitle')} message={t('upsellMessage')} />
      </div>
    )
  }

  const onOpenBooking = (id: string) => {
    setSelectedId(id)
    setDialogOpen(true)
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('title')}
        description={t('subtitle')}
        badge={<CalendarCheck className="h-5 w-5 text-muted-foreground" />}
        actions={
          <div className="inline-flex items-center gap-1 rounded-md border bg-background p-0.5">
            <Button
              variant={view === 'list' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => setView('list')}
              className="h-8"
            >
              <ListIcon className="mr-1.5 h-3.5 w-3.5" /> {t('viewList')}
            </Button>
            <Button
              variant={view === 'calendar' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => setView('calendar')}
              className="h-8"
            >
              <LayoutGrid className="mr-1.5 h-3.5 w-3.5" /> {t('viewCalendar')}
            </Button>
          </div>
        }
      />

      {/* Toolbar */}
      <Card>
        <CardContent className="pt-6">
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <div className="space-y-1.5">
              <Label htmlFor="fromDate">{t('fromDate')}</Label>
              <Input
                id="fromDate"
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="toDate">{t('toDate')}</Label>
              <Input
                id="toDate"
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
              />
            </div>
            <div className="space-y-1.5">
              <Label>{tCommon('selectBranch')}</Label>
              <Select value={branchId || 'all'} onValueChange={(v) => setBranchId(v === 'all' ? '' : v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">{tCommon('allBranches')}</SelectItem>
                  {branches?.map((b) => (
                    <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>{tCommon('status')}</Label>
              <Select value={status || 'all'} onValueChange={(v) => setStatus(v === 'all' ? '' : v)}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">{tCommon('all')}</SelectItem>
                  {STATUS_OPTIONS.map((s) => (
                    <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          {view === 'calendar' && !branchId && (
            <p className="text-xs text-muted-foreground mt-3 flex items-center gap-1">
              <CalendarDays className="h-3.5 w-3.5" />
              {t('calendarNeedsBranchHint')}
            </p>
          )}
        </CardContent>
      </Card>

      {/* Content */}
      {isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : !bookings || bookings.length === 0 ? (
        <EmptyState
          icon={CalendarCheck}
          title={t('emptyTitle')}
          description={t('emptyDescription')}
        />
      ) : view === 'list' ? (
        <BookingsTable bookings={bookings} onSelect={onOpenBooking} t={t} />
      ) : (
        <CalendarView
          bookings={bookings}
          weekStart={new Date(fromDate)}
          openTime={openTime}
          closeTime={closeTime}
          onSelect={onOpenBooking}
          t={t}
        />
      )}

      <BookingDetailDialog
        id={selectedId}
        open={dialogOpen}
        onOpenChange={(v) => { setDialogOpen(v); if (!v) setSelectedId(null) }}
        t={t}
      />
    </div>
  )
}

// ── List view ────────────────────────────────────────────────────────────────

function BookingsTable({
  bookings, onSelect, t,
}: {
  bookings: BookingListItemDto[]
  onSelect: (id: string) => void
  t: ReturnType<typeof useTranslations>
}) {
  return (
    <div className="rounded-lg border overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('colSlot')}</TableHead>
            <TableHead>{t('colBranch')}</TableHead>
            <TableHead>{t('colCustomer')}</TableHead>
            <TableHead>{t('colVehicle')}</TableHead>
            <TableHead>{t('colServices')}</TableHead>
            <TableHead className="text-right">{t('colPrice')}</TableHead>
            <TableHead>{t('colStatus')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {bookings.map((b) => (
            <TableRow
              key={b.id}
              className="cursor-pointer"
              onClick={() => onSelect(b.id)}
            >
              <TableCell>
                <div className="text-sm font-medium">{formatDate(b.slotStartUtc)}</div>
                <div className="text-xs text-muted-foreground tabular-nums">
                  {formatTime(b.slotStartUtc)} – {formatTime(b.slotEndUtc)}
                </div>
              </TableCell>
              <TableCell className="text-sm">{b.branchName}</TableCell>
              <TableCell>
                <div className="text-sm">{b.customerName}</div>
              </TableCell>
              <TableCell className="text-sm tabular-nums">{b.plateNumber}</TableCell>
              <TableCell className="text-sm max-w-xs">
                <span className="line-clamp-2">{b.serviceSummary}</span>
              </TableCell>
              <TableCell className="text-right text-sm tabular-nums">
                {formatBookingPrice(b)}
              </TableCell>
              <TableCell>
                <StatusBadge status={b.status} />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
