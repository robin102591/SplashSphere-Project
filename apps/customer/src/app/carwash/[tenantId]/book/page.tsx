'use client'

import { use, useMemo, useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useTranslations } from 'next-intl'
import {
  AlertCircle,
  Building2,
  Calendar,
  Car,
  CheckCircle2,
  ChevronRight,
  Info,
  Plus,
} from 'lucide-react'
import type {
  ConnectAvailabilityDto,
  ConnectBranchSummaryDto,
  ConnectServicePriceDto,
  ConnectVehicleDto,
} from '@splashsphere/types'
import { AppBar } from '@/components/layout/app-bar'
import { WizardProgress } from '@/components/carwash/wizard-progress'
import { WizardFooter } from '@/components/carwash/wizard-footer'
import {
  useAvailability,
  useCarwashDetail,
  useTenantServices,
} from '@/hooks/use-carwash'
import { useCreateBooking } from '@/hooks/use-create-booking'
import { useVehicles } from '@/hooks/use-profile'
import {
  canAdvance,
  useBookingWizard,
  type WizardState,
} from '@/hooks/use-booking-wizard'
import { cn } from '@/lib/utils'

interface BookingWizardPageProps {
  params: Promise<{ tenantId: string }>
}

const MANILA_TZ = 'Asia/Manila'

// ── Formatting helpers ──────────────────────────────────────────────────────

const pesoFormatter = new Intl.NumberFormat('en-PH', {
  style: 'currency',
  currency: 'PHP',
  minimumFractionDigits: 0,
  maximumFractionDigits: 2,
})

function formatPeso(amount: number): string {
  return pesoFormatter.format(amount ?? 0)
}

const slotDateFormatter = new Intl.DateTimeFormat('en-PH', {
  weekday: 'long',
  month: 'short',
  day: 'numeric',
  year: 'numeric',
  timeZone: MANILA_TZ,
})

const slotTimeFormatter = new Intl.DateTimeFormat('en-PH', {
  hour: 'numeric',
  minute: '2-digit',
  hour12: true,
  timeZone: MANILA_TZ,
})

/** `YYYY-MM-DD` for the given JS Date in Asia/Manila. */
function toManilaDateString(d: Date): string {
  const parts = new Intl.DateTimeFormat('en-CA', {
    timeZone: MANILA_TZ,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  }).formatToParts(d)
  const y = parts.find((p) => p.type === 'year')?.value ?? '1970'
  const m = parts.find((p) => p.type === 'month')?.value ?? '01'
  const day = parts.find((p) => p.type === 'day')?.value ?? '01'
  return `${y}-${m}-${day}`
}

function todayInManila(): string {
  return toManilaDateString(new Date())
}

/**
 * Naive `max` date calculator — adds `days` to the Manila date without
 * worrying about DST (the Philippines doesn't observe it). Good enough
 * for the `max` attribute on a `<input type="date">`.
 */
function addDaysInManila(base: string, days: number): string {
  // `YYYY-MM-DD` parses as UTC midnight — add days there, re-convert.
  const d = new Date(`${base}T00:00:00Z`)
  d.setUTCDate(d.getUTCDate() + days)
  return toManilaDateString(d)
}

// ── Page component ──────────────────────────────────────────────────────────

/**
 * Booking wizard — 4 steps (vehicle → services → schedule → confirm).
 *
 * Data fetched per-step to keep payloads small:
 *  - vehicles: `/profile` (cached from other screens)
 *  - services: `/carwashes/{tenantId}/services?vehicleId={id}` — only
 *    enabled once a vehicle is picked
 *  - slots: `/carwashes/{tenantId}/slots?branchId=&date=` — only
 *    enabled once a branch + date are picked
 *
 * Booking detail (for branches + booking settings like
 * `advanceBookingDays`) lives on the tenant detail endpoint, so step 3
 * reuses the detail already warm from the previous page.
 */
export default function BookingWizardPage({ params }: BookingWizardPageProps) {
  const { tenantId } = use(params)
  const t = useTranslations('booking')
  const tCommon = useTranslations('common')
  const router = useRouter()

  const [wizard, dispatch] = useBookingWizard()
  const [submitError, setSubmitError] = useState<string | null>(null)

  const tenantDetail = useCarwashDetail(tenantId)
  const vehicles = useVehicles()
  const services = useTenantServices(tenantId, wizard.vehicleId)
  const availability = useAvailability(
    tenantId,
    wizard.branchId,
    wizard.date,
  )
  const createBooking = useCreateBooking()

  const selectedVehicle = useMemo(
    () => vehicles.data?.find((v) => v.id === wizard.vehicleId) ?? null,
    [vehicles.data, wizard.vehicleId],
  )

  const selectedServices = useMemo(() => {
    const list = services.data?.services ?? []
    return wizard.serviceIds
      .map((id) => list.find((s) => s.serviceId === id))
      .filter((s): s is ConnectServicePriceDto => Boolean(s))
  }, [services.data, wizard.serviceIds])

  const selectedBranch = useMemo(
    () =>
      tenantDetail.data?.branches.find((b) => b.id === wizard.branchId) ?? null,
    [tenantDetail.data, wizard.branchId],
  )

  const totals = useMemo(() => computeTotals(selectedServices), [selectedServices])

  const advanceBookingDays = 14 // backend default; server re-validates regardless.

  // ── Navigation wiring ─────────────────────────────────────────────────────

  const handleBack = () => {
    if (wizard.step === 0) {
      router.push(`/carwash/${tenantId}`)
      return
    }
    dispatch({ type: 'back' })
  }

  const handleAdvance = () => {
    if (wizard.step < 3) {
      dispatch({ type: 'next' })
      return
    }
    // Confirm step → submit.
    if (
      !wizard.vehicleId ||
      !wizard.branchId ||
      !wizard.slotStartUtc ||
      wizard.serviceIds.length === 0
    ) {
      return
    }
    setSubmitError(null)
    createBooking.mutate(
      {
        tenantId,
        branchId: wizard.branchId,
        vehicleId: wizard.vehicleId,
        slotStartUtc: wizard.slotStartUtc,
        serviceIds: [...wizard.serviceIds],
        notes: wizard.notes.trim() || undefined,
      },
      {
        onSuccess: (created) => {
          router.replace(`/bookings/${created.id}`)
        },
        onError: (err) => {
          const message =
            (err as { title?: string; detail?: string })?.detail ||
            (err as { title?: string })?.title ||
            t('errors.submitGeneric')
          setSubmitError(message)
        },
      },
    )
  }

  // ── Top bar + progress ────────────────────────────────────────────────────

  const tenantName = tenantDetail.data?.tenantName ?? ''

  return (
    <div className="flex min-h-[100svh] flex-col bg-background">
      <AppBar
        title={tenantName || t('title')}
        onBack={handleBack}
      />
      <WizardProgress current={wizard.step} />

      <main className="flex-1 pb-4">
        {wizard.step === 0 && (
          <VehicleStep
            vehicles={vehicles.data ?? []}
            isLoading={vehicles.isPending}
            selectedId={wizard.vehicleId}
            onPick={(id) => dispatch({ type: 'pickVehicle', vehicleId: id })}
          />
        )}
        {wizard.step === 1 && (
          <ServicesStep
            loading={services.isPending}
            error={services.isError}
            onRetry={() => services.refetch()}
            data={services.data}
            selectedIds={wizard.serviceIds}
            onToggle={(id) => dispatch({ type: 'toggleService', serviceId: id })}
          />
        )}
        {wizard.step === 2 && (
          <ScheduleStep
            branches={tenantDetail.data?.branches ?? []}
            loadingBranches={tenantDetail.isPending}
            state={wizard}
            maxDate={addDaysInManila(todayInManila(), advanceBookingDays)}
            minDate={todayInManila()}
            availability={availability.data ?? []}
            availabilityLoading={availability.isPending}
            availabilityError={availability.isError}
            onRetryAvailability={() => availability.refetch()}
            onPickBranch={(id) => dispatch({ type: 'pickBranch', branchId: id })}
            onPickDate={(d) => dispatch({ type: 'pickDate', date: d })}
            onPickSlot={(s) =>
              dispatch({ type: 'pickSlot', slotStartUtc: s })
            }
          />
        )}
        {wizard.step === 3 && (
          <ConfirmStep
            vehicle={selectedVehicle}
            services={selectedServices}
            priceMode={services.data?.priceMode}
            totals={totals}
            branch={selectedBranch}
            slotStartUtc={wizard.slotStartUtc}
            notes={wizard.notes}
            onNotesChange={(v) => dispatch({ type: 'setNotes', notes: v })}
            submitError={submitError}
          />
        )}
      </main>

      <WizardFooter
        label={getFooterLabel(wizard, t, tCommon)}
        onClick={handleAdvance}
        disabled={!canAdvance(wizard)}
        loading={createBooking.isPending}
        helperText={getHelperText(wizard, totals, services.data?.priceMode, t)}
      />
    </div>
  )
}

// ── Footer label helpers ────────────────────────────────────────────────────

function getFooterLabel(
  wizard: WizardState,
  t: (key: string) => string,
  tCommon: (key: string) => string,
): string {
  if (wizard.step === 3) return t('actions.confirmBooking')
  return tCommon('continue')
}

function getHelperText(
  wizard: WizardState,
  totals: Totals,
  priceMode: string | undefined,
  t: (key: string, values?: Record<string, string | number>) => string,
): string | null {
  if (wizard.step === 1 && wizard.serviceIds.length > 0) {
    if (priceMode === 'exact') {
      return t('totals.exact', { total: formatPeso(totals.exact) })
    }
    return t('totals.estimated', {
      min: formatPeso(totals.min),
      max: formatPeso(totals.max),
    })
  }
  return null
}

// ── Totals ─────────────────────────────────────────────────────────────────

interface Totals {
  exact: number
  min: number
  max: number
}

function computeTotals(services: readonly ConnectServicePriceDto[]): Totals {
  let exact = 0
  let min = 0
  let max = 0
  for (const s of services) {
    if (s.price != null) {
      exact += s.price
      min += s.price
      max += s.price
    } else {
      min += s.priceMin ?? 0
      max += s.priceMax ?? 0
    }
  }
  return { exact, min, max }
}

// ── Step 1: Vehicle ─────────────────────────────────────────────────────────

function VehicleStep({
  vehicles,
  isLoading,
  selectedId,
  onPick,
}: {
  vehicles: readonly ConnectVehicleDto[]
  isLoading: boolean
  selectedId: string | null
  onPick: (id: string) => void
}) {
  const t = useTranslations('booking.vehicle')

  if (isLoading) {
    return (
      <div className="space-y-3 p-4">
        <div className="h-20 animate-pulse rounded-2xl border border-border bg-card" />
        <div className="h-20 animate-pulse rounded-2xl border border-border bg-card" />
      </div>
    )
  }

  return (
    <div className="space-y-4 p-4">
      <div>
        <h2 className="text-lg font-semibold leading-tight">{t('title')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">{t('subtitle')}</p>
      </div>

      {vehicles.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-border bg-card p-6 text-center">
          <Car
            className="mx-auto h-8 w-8 text-muted-foreground"
            aria-hidden
          />
          <p className="mt-2 text-sm font-semibold">{t('emptyTitle')}</p>
          <p className="mt-1 text-xs text-muted-foreground">{t('emptyHint')}</p>
          <Link
            href="/profile"
            className="mt-4 inline-flex min-h-[44px] items-center justify-center gap-1.5 rounded-xl bg-primary px-4 text-sm font-semibold text-primary-foreground transition-transform active:scale-[0.97]"
          >
            <Plus className="h-4 w-4" aria-hidden />
            {t('addVehicle')}
          </Link>
        </div>
      ) : (
        <ul className="space-y-2">
          {vehicles.map((v) => {
            const selected = v.id === selectedId
            const label = [v.year, v.makeName, v.modelName]
              .filter(Boolean)
              .join(' ')
            return (
              <li key={v.id}>
                <button
                  type="button"
                  onClick={() => onPick(v.id)}
                  aria-pressed={selected}
                  className={cn(
                    'flex min-h-[64px] w-full items-center gap-3 rounded-2xl border bg-card p-4 text-left transition-colors active:scale-[0.99]',
                    selected
                      ? 'border-primary bg-primary/5'
                      : 'border-border hover:bg-muted/40',
                  )}
                >
                  <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
                    <Car className="h-5 w-5" aria-hidden />
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-semibold text-foreground">
                      {label || v.plateNumber}
                    </p>
                    <p className="mt-0.5 flex flex-wrap items-center gap-x-2 gap-y-0.5 text-xs text-muted-foreground">
                      <span className="font-mono font-medium tracking-wide text-foreground">
                        {v.plateNumber}
                      </span>
                      {v.color && <span>· {v.color}</span>}
                    </p>
                  </div>
                  {selected && (
                    <CheckCircle2
                      className="h-5 w-5 shrink-0 text-primary"
                      aria-hidden
                    />
                  )}
                </button>
              </li>
            )
          })}
        </ul>
      )}

      {vehicles.length > 0 && (
        <Link
          href="/profile"
          className="flex min-h-[48px] w-full items-center justify-center gap-1.5 rounded-2xl border border-dashed border-border bg-card px-4 text-sm font-medium text-muted-foreground transition-colors active:scale-[0.97] hover:bg-muted/40"
        >
          <Plus className="h-4 w-4" aria-hidden />
          {t('addVehicle')}
        </Link>
      )}
    </div>
  )
}

// ── Step 2: Services ────────────────────────────────────────────────────────

function ServicesStep({
  loading,
  error,
  onRetry,
  data,
  selectedIds,
  onToggle,
}: {
  loading: boolean
  error: boolean
  onRetry: () => void
  data:
    | {
        priceMode: string
        services: readonly ConnectServicePriceDto[]
      }
    | undefined
  selectedIds: readonly string[]
  onToggle: (id: string) => void
}) {
  const t = useTranslations('booking.services')

  if (loading) {
    return (
      <div className="space-y-3 p-4">
        <div className="h-14 animate-pulse rounded-2xl border border-border bg-card" />
        <div className="h-14 animate-pulse rounded-2xl border border-border bg-card" />
        <div className="h-14 animate-pulse rounded-2xl border border-border bg-card" />
      </div>
    )
  }

  if (error || !data) {
    return (
      <div className="p-4">
        <div className="rounded-2xl border border-destructive/20 bg-destructive/5 p-4 text-center">
          <p className="text-sm font-medium text-destructive">
            {t('loadError')}
          </p>
          <button
            type="button"
            onClick={onRetry}
            className="mt-3 inline-flex min-h-[44px] items-center justify-center rounded-xl border border-border bg-background px-4 text-sm font-semibold transition-colors active:scale-[0.97]"
          >
            {t('retry')}
          </button>
        </div>
      </div>
    )
  }

  const unclassified = data.priceMode === 'estimate'
  const items = data.services

  return (
    <div className="space-y-4 p-4">
      <div>
        <h2 className="text-lg font-semibold leading-tight">{t('title')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">{t('subtitle')}</p>
      </div>

      {unclassified && (
        <div className="flex items-start gap-2 rounded-2xl border border-amber-200 bg-amber-50 p-3 text-xs text-amber-900">
          <Info className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
          <p>{t('unclassifiedBanner')}</p>
        </div>
      )}

      {items.length === 0 ? (
        <p className="rounded-2xl border border-dashed border-border bg-card p-4 text-center text-sm text-muted-foreground">
          {t('empty')}
        </p>
      ) : (
        <ul className="space-y-2">
          {items.map((s) => {
            const selected = selectedIds.includes(s.serviceId)
            return (
              <li key={s.serviceId}>
                <button
                  type="button"
                  onClick={() => onToggle(s.serviceId)}
                  aria-pressed={selected}
                  className={cn(
                    'flex min-h-[56px] w-full items-start gap-3 rounded-2xl border bg-card p-4 text-left transition-colors active:scale-[0.99]',
                    selected
                      ? 'border-primary bg-primary/5'
                      : 'border-border hover:bg-muted/40',
                  )}
                >
                  <span
                    className={cn(
                      'mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-md border',
                      selected
                        ? 'border-primary bg-primary text-primary-foreground'
                        : 'border-border bg-background',
                    )}
                    aria-hidden
                  >
                    {selected && <CheckCircle2 className="h-4 w-4" />}
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-semibold text-foreground">
                      {s.name}
                    </p>
                    {s.description && (
                      <p className="mt-0.5 line-clamp-2 text-xs text-muted-foreground">
                        {s.description}
                      </p>
                    )}
                  </div>
                  <span className="shrink-0 rounded-lg bg-primary/5 px-2.5 py-1 text-sm font-semibold tabular-nums text-primary">
                    {s.price != null
                      ? formatPeso(s.price)
                      : `${formatPeso(s.priceMin ?? 0)}–${formatPeso(s.priceMax ?? 0)}`}
                  </span>
                </button>
              </li>
            )
          })}
        </ul>
      )}
    </div>
  )
}

// ── Step 3: Schedule ────────────────────────────────────────────────────────

function ScheduleStep({
  branches,
  loadingBranches,
  state,
  maxDate,
  minDate,
  availability,
  availabilityLoading,
  availabilityError,
  onRetryAvailability,
  onPickBranch,
  onPickDate,
  onPickSlot,
}: {
  branches: readonly ConnectBranchSummaryDto[]
  loadingBranches: boolean
  state: WizardState
  maxDate: string
  minDate: string
  availability: readonly ConnectAvailabilityDto[]
  availabilityLoading: boolean
  availabilityError: boolean
  onRetryAvailability: () => void
  onPickBranch: (id: string) => void
  onPickDate: (date: string) => void
  onPickSlot: (iso: string) => void
}) {
  const t = useTranslations('booking.schedule')

  const bookableBranches = branches.filter((b) => b.isBookingEnabled)
  const showDropdown = bookableBranches.length > 5

  return (
    <div className="space-y-6 p-4">
      <div>
        <h2 className="text-lg font-semibold leading-tight">{t('title')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">{t('subtitle')}</p>
      </div>

      {/* Branch picker */}
      <section className="space-y-2">
        <h3 className="text-sm font-semibold">{t('branchLabel')}</h3>
        {loadingBranches ? (
          <div className="h-14 animate-pulse rounded-2xl border border-border bg-card" />
        ) : bookableBranches.length === 0 ? (
          <p className="rounded-2xl border border-dashed border-border bg-card p-4 text-center text-sm text-muted-foreground">
            {t('noBookableBranches')}
          </p>
        ) : showDropdown ? (
          <select
            value={state.branchId ?? ''}
            onChange={(e) => onPickBranch(e.target.value)}
            className="flex min-h-[48px] w-full items-center rounded-2xl border border-border bg-background px-4 text-base font-medium"
          >
            <option value="" disabled>
              {t('branchPlaceholder')}
            </option>
            {bookableBranches.map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </select>
        ) : (
          <ul className="space-y-2">
            {bookableBranches.map((b) => {
              const selected = b.id === state.branchId
              return (
                <li key={b.id}>
                  <button
                    type="button"
                    onClick={() => onPickBranch(b.id)}
                    aria-pressed={selected}
                    className={cn(
                      'flex min-h-[56px] w-full items-center gap-3 rounded-2xl border bg-card p-4 text-left transition-colors active:scale-[0.99]',
                      selected
                        ? 'border-primary bg-primary/5'
                        : 'border-border hover:bg-muted/40',
                    )}
                  >
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
                      <Building2 className="h-4 w-4" aria-hidden />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-semibold text-foreground">
                        {b.name}
                      </p>
                      <p className="mt-0.5 truncate text-xs text-muted-foreground">
                        {b.address}
                      </p>
                    </div>
                    {selected && (
                      <CheckCircle2
                        className="h-5 w-5 shrink-0 text-primary"
                        aria-hidden
                      />
                    )}
                  </button>
                </li>
              )
            })}
          </ul>
        )}
      </section>

      {/* Date picker */}
      <section className="space-y-2">
        <h3 className="text-sm font-semibold">{t('dateLabel')}</h3>
        <input
          type="date"
          value={state.date ?? ''}
          min={minDate}
          max={maxDate}
          onChange={(e) => onPickDate(e.target.value)}
          disabled={!state.branchId}
          className="flex min-h-[48px] w-full items-center rounded-2xl border border-border bg-background px-4 text-base font-medium disabled:opacity-50"
        />
        {!state.branchId && (
          <p className="text-xs text-muted-foreground">
            {t('pickBranchFirst')}
          </p>
        )}
      </section>

      {/* Time slot grid */}
      <section className="space-y-2">
        <h3 className="text-sm font-semibold">{t('timeLabel')}</h3>
        {!state.branchId || !state.date ? (
          <p className="rounded-2xl border border-dashed border-border bg-card p-4 text-center text-sm text-muted-foreground">
            {t('pickBranchAndDate')}
          </p>
        ) : availabilityLoading ? (
          <div className="grid grid-cols-3 gap-2">
            {Array.from({ length: 6 }).map((_, i) => (
              <div
                key={i}
                className="h-12 animate-pulse rounded-xl border border-border bg-card"
              />
            ))}
          </div>
        ) : availabilityError ? (
          <div className="rounded-2xl border border-destructive/20 bg-destructive/5 p-4 text-center">
            <p className="text-sm font-medium text-destructive">
              {t('slotsError')}
            </p>
            <button
              type="button"
              onClick={onRetryAvailability}
              className="mt-3 inline-flex min-h-[44px] items-center justify-center rounded-xl border border-border bg-background px-4 text-sm font-semibold transition-colors active:scale-[0.97]"
            >
              {t('retry')}
            </button>
          </div>
        ) : availability.length === 0 ? (
          <p className="rounded-2xl border border-dashed border-border bg-card p-4 text-center text-sm text-muted-foreground">
            {t('noSlots')}
          </p>
        ) : (
          <div className="grid grid-cols-3 gap-2">
            {availability.map((slot) => {
              const selected = slot.slotStartUtc === state.slotStartUtc
              const disabled = slot.remainingCapacity <= 0
              return (
                <button
                  key={slot.slotStartUtc}
                  type="button"
                  onClick={() => onPickSlot(slot.slotStartUtc)}
                  disabled={disabled}
                  aria-pressed={selected}
                  className={cn(
                    'flex min-h-[48px] flex-col items-center justify-center rounded-xl border px-2 py-1 text-sm font-semibold transition-colors active:scale-[0.97]',
                    selected
                      ? 'border-primary bg-primary text-primary-foreground'
                      : disabled
                        ? 'border-border bg-muted/40 text-muted-foreground'
                        : 'border-border bg-card text-foreground hover:bg-muted/40',
                  )}
                >
                  <span className="tabular-nums">{slot.localTime}</span>
                </button>
              )
            })}
          </div>
        )}
      </section>
    </div>
  )
}

// ── Step 4: Confirm ─────────────────────────────────────────────────────────

function ConfirmStep({
  vehicle,
  services,
  priceMode,
  totals,
  branch,
  slotStartUtc,
  notes,
  onNotesChange,
  submitError,
}: {
  vehicle: ConnectVehicleDto | null
  services: readonly ConnectServicePriceDto[]
  priceMode: string | undefined
  totals: Totals
  branch: ConnectBranchSummaryDto | null
  slotStartUtc: string | null
  notes: string
  onNotesChange: (value: string) => void
  submitError: string | null
}) {
  const t = useTranslations('booking.confirm')
  const slotDate = slotStartUtc ? new Date(slotStartUtc) : null
  const showRange = priceMode === 'estimate'

  return (
    <div className="space-y-4 p-4">
      <div>
        <h2 className="text-lg font-semibold leading-tight">{t('title')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">{t('subtitle')}</p>
      </div>

      {/* Vehicle */}
      {vehicle && (
        <section className="rounded-2xl border border-border bg-card p-4">
          <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
            {t('vehicleLabel')}
          </p>
          <p className="mt-1 text-sm font-semibold">
            {[vehicle.year, vehicle.makeName, vehicle.modelName]
              .filter(Boolean)
              .join(' ') || vehicle.plateNumber}
          </p>
          <p className="mt-0.5 font-mono text-xs text-muted-foreground">
            {vehicle.plateNumber}
          </p>
        </section>
      )}

      {/* Services */}
      <section className="rounded-2xl border border-border bg-card p-4">
        <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
          {t('servicesLabel')}
        </p>
        <ul className="mt-2 space-y-1.5">
          {services.map((s) => (
            <li
              key={s.serviceId}
              className="flex items-baseline justify-between gap-3 text-sm"
            >
              <span className="min-w-0 flex-1 truncate font-medium">
                {s.name}
              </span>
              <span className="shrink-0 tabular-nums text-muted-foreground">
                {s.price != null
                  ? formatPeso(s.price)
                  : `${formatPeso(s.priceMin ?? 0)}–${formatPeso(s.priceMax ?? 0)}`}
              </span>
            </li>
          ))}
        </ul>
        <div className="mt-3 flex items-baseline justify-between border-t border-border pt-3 text-sm">
          <span className="font-semibold">
            {showRange ? t('estimatedTotal') : t('total')}
          </span>
          <span className="text-base font-bold tabular-nums text-primary">
            {showRange
              ? `${formatPeso(totals.min)}–${formatPeso(totals.max)}`
              : formatPeso(totals.exact)}
          </span>
        </div>
      </section>

      {/* Branch + time */}
      {branch && slotDate && (
        <section className="rounded-2xl border border-border bg-card p-4">
          <div className="flex items-start gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
              <Building2 className="h-4 w-4" aria-hidden />
            </div>
            <div className="min-w-0 flex-1">
              <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
                {t('branchLabel')}
              </p>
              <p className="mt-0.5 text-sm font-semibold">{branch.name}</p>
              <p className="mt-0.5 truncate text-xs text-muted-foreground">
                {branch.address}
              </p>
            </div>
          </div>
          <div className="mt-3 flex items-center gap-3 border-t border-border pt-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
              <Calendar className="h-4 w-4" aria-hidden />
            </div>
            <div className="min-w-0 flex-1">
              <p className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
                {t('slotLabel')}
              </p>
              <p className="mt-0.5 text-sm font-semibold">
                {slotDateFormatter.format(slotDate)}
              </p>
              <p className="mt-0.5 text-xs text-muted-foreground">
                {slotTimeFormatter.format(slotDate)}
              </p>
            </div>
          </div>
        </section>
      )}

      {/* Notes */}
      <section className="space-y-2">
        <label
          htmlFor="booking-notes"
          className="text-sm font-semibold"
        >
          {t('notesLabel')}
        </label>
        <textarea
          id="booking-notes"
          rows={3}
          value={notes}
          onChange={(e) => onNotesChange(e.target.value)}
          placeholder={t('notesPlaceholder')}
          maxLength={500}
          className="w-full rounded-2xl border border-border bg-background p-3 text-sm leading-relaxed"
        />
      </section>

      {/* Policy reminder */}
      <section className="flex items-start gap-2 rounded-2xl border border-border bg-muted/30 p-3 text-xs text-muted-foreground">
        <ChevronRight className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
        <p>{t('policy')}</p>
      </section>

      {/* Error */}
      {submitError && (
        <section className="flex items-start gap-2 rounded-2xl border border-destructive/30 bg-destructive/5 p-3 text-sm text-destructive">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
          <p>{submitError}</p>
        </section>
      )}
    </div>
  )
}

