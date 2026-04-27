'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Lock, CalendarCheck } from 'lucide-react'
import { toast } from 'sonner'

import { PageHeader } from '@/components/ui/page-header'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Switch } from '@/components/ui/switch'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'

import { useBranches } from '@/hooks/use-branches'
import { useHasFeature } from '@/hooks/use-plan'
import {
  useBookingSettings,
  useUpsertBookingSettings,
  type UpsertBookingSettingBody,
} from '@/hooks/use-bookings'
import { FeatureKeys } from '@splashsphere/types'

// ── Schema ────────────────────────────────────────────────────────────────────

const timeRegex = /^([01]\d|2[0-3]):[0-5]\d$/

const schema = z
  .object({
    openTime: z.string().regex(timeRegex, 'Use HH:mm format'),
    closeTime: z.string().regex(timeRegex, 'Use HH:mm format'),
    slotIntervalMinutes: z.coerce.number().refine(
      (v) => [15, 30, 45, 60].includes(v),
      'Must be 15, 30, 45, or 60',
    ),
    maxBookingsPerSlot: z.coerce.number().int().min(1, 'Must be at least 1'),
    advanceBookingDays: z.coerce.number().int().min(1, 'Min 1').max(30, 'Max 30'),
    minLeadTimeMinutes: z.coerce.number().int().min(0, 'Cannot be negative'),
    noShowGraceMinutes: z.coerce.number().int().min(0, 'Cannot be negative'),
    isBookingEnabled: z.boolean(),
    showInPublicDirectory: z.boolean(),
  })
  .refine(
    (v) => toMinutes(v.closeTime) > toMinutes(v.openTime),
    { path: ['closeTime'], message: 'Close time must be after open time' },
  )

type FormValues = z.infer<typeof schema>

function toMinutes(hhmm: string): number {
  const [h, m] = hhmm.split(':').map(Number)
  return (h || 0) * 60 + (m || 0)
}

const DEFAULTS: FormValues = {
  openTime: '08:00',
  closeTime: '20:00',
  slotIntervalMinutes: 30,
  maxBookingsPerSlot: 3,
  advanceBookingDays: 7,
  minLeadTimeMinutes: 120,
  noShowGraceMinutes: 15,
  isBookingEnabled: false,
  showInPublicDirectory: false,
}

// ── Upsell card ───────────────────────────────────────────────────────────────

function BookingUpsell() {
  return (
    <div className="border-2 border-dashed border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-950/30 rounded-xl p-8 text-center">
      <Lock className="h-8 w-8 text-amber-400 mx-auto mb-3" />
      <p className="text-amber-800 dark:text-amber-200 font-semibold mb-1">
        Online Booking is not in your current plan
      </p>
      <p className="text-amber-600 dark:text-amber-400 text-sm mb-4">
        Upgrade to Growth or Enterprise to let customers book slots through the Customer Connect app.
      </p>
      <a
        href="/dashboard/subscription"
        className="text-sm font-semibold text-primary hover:underline"
      >
        View Plans &rarr;
      </a>
    </div>
  )
}

// ── Form body ─────────────────────────────────────────────────────────────────

function BookingSettingsForm({ branchId, t }: { branchId: string; t: ReturnType<typeof useTranslations> }) {
  const { data: settings, isLoading } = useBookingSettings(branchId)
  const { mutateAsync: save, isPending: saving } = useUpsertBookingSettings(branchId)

  const { register, handleSubmit, formState, reset, watch, setValue } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: DEFAULTS,
  })

  // Re-seed form when branch or settings change.
  useEffect(() => {
    if (settings) {
      reset({
        openTime: settings.openTime.slice(0, 5),
        closeTime: settings.closeTime.slice(0, 5),
        slotIntervalMinutes: settings.slotIntervalMinutes,
        maxBookingsPerSlot: settings.maxBookingsPerSlot,
        advanceBookingDays: settings.advanceBookingDays,
        minLeadTimeMinutes: settings.minLeadTimeMinutes,
        noShowGraceMinutes: settings.noShowGraceMinutes,
        isBookingEnabled: settings.isBookingEnabled,
        showInPublicDirectory: settings.showInPublicDirectory,
      })
    } else {
      reset(DEFAULTS)
    }
  }, [settings, reset, branchId])

  const isBookingEnabled = watch('isBookingEnabled')
  const showInPublicDirectory = watch('showInPublicDirectory')

  const onSubmit = async (values: FormValues) => {
    const payload: UpsertBookingSettingBody = {
      openTime: values.openTime,
      closeTime: values.closeTime,
      slotIntervalMinutes: values.slotIntervalMinutes,
      maxBookingsPerSlot: values.maxBookingsPerSlot,
      advanceBookingDays: values.advanceBookingDays,
      minLeadTimeMinutes: values.minLeadTimeMinutes,
      noShowGraceMinutes: values.noShowGraceMinutes,
      isBookingEnabled: values.isBookingEnabled,
      showInPublicDirectory: values.showInPublicDirectory,
    }
    try {
      await save(payload)
      toast.success(t('saved'))
    } catch (err) {
      const msg = (err as { detail?: string; title?: string })?.detail
        ?? (err as { title?: string })?.title
        ?? t('saveFailed')
      toast.error(msg)
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-4 max-w-2xl">
        <Skeleton className="h-80 w-full" />
        <Skeleton className="h-60 w-full" />
      </div>
    )
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 max-w-2xl">
      {/* Hours & slots */}
      <Card>
        <CardHeader>
          <CardTitle>{t('hoursAndSlots')}</CardTitle>
          <CardDescription>{t('hoursAndSlotsDesc')}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label htmlFor="openTime">{t('openTime')}</Label>
              <Input id="openTime" type="time" {...register('openTime')} />
              <p className="text-xs text-muted-foreground">{t('openTimeHelp')}</p>
              {formState.errors.openTime && (
                <p className="text-xs text-destructive">{formState.errors.openTime.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="closeTime">{t('closeTime')}</Label>
              <Input id="closeTime" type="time" {...register('closeTime')} />
              <p className="text-xs text-muted-foreground">{t('closeTimeHelp')}</p>
              {formState.errors.closeTime && (
                <p className="text-xs text-destructive">{formState.errors.closeTime.message}</p>
              )}
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label>{t('slotInterval')}</Label>
              <Select
                value={String(watch('slotIntervalMinutes'))}
                onValueChange={(v) => setValue('slotIntervalMinutes', Number(v), { shouldDirty: true })}
              >
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="15">15 minutes</SelectItem>
                  <SelectItem value="30">30 minutes</SelectItem>
                  <SelectItem value="45">45 minutes</SelectItem>
                  <SelectItem value="60">60 minutes</SelectItem>
                </SelectContent>
              </Select>
              <p className="text-xs text-muted-foreground">{t('slotIntervalHelp')}</p>
              {formState.errors.slotIntervalMinutes && (
                <p className="text-xs text-destructive">{formState.errors.slotIntervalMinutes.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="maxBookingsPerSlot">{t('maxBookingsPerSlot')}</Label>
              <Input
                id="maxBookingsPerSlot"
                type="number"
                min={1}
                step={1}
                {...register('maxBookingsPerSlot')}
              />
              <p className="text-xs text-muted-foreground">{t('maxBookingsPerSlotHelp')}</p>
              {formState.errors.maxBookingsPerSlot && (
                <p className="text-xs text-destructive">{formState.errors.maxBookingsPerSlot.message}</p>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Lead / horizon / grace */}
      <Card>
        <CardHeader>
          <CardTitle>{t('windowTitle')}</CardTitle>
          <CardDescription>{t('windowDesc')}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-3">
            <div className="space-y-1.5">
              <Label htmlFor="advanceBookingDays">{t('advanceBookingDays')}</Label>
              <Input
                id="advanceBookingDays"
                type="number"
                min={1}
                max={30}
                step={1}
                {...register('advanceBookingDays')}
              />
              <p className="text-xs text-muted-foreground">{t('advanceBookingDaysHelp')}</p>
              {formState.errors.advanceBookingDays && (
                <p className="text-xs text-destructive">{formState.errors.advanceBookingDays.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="minLeadTimeMinutes">{t('minLeadTime')}</Label>
              <Input
                id="minLeadTimeMinutes"
                type="number"
                min={0}
                step={15}
                {...register('minLeadTimeMinutes')}
              />
              <p className="text-xs text-muted-foreground">{t('minLeadTimeHelp')}</p>
              {formState.errors.minLeadTimeMinutes && (
                <p className="text-xs text-destructive">{formState.errors.minLeadTimeMinutes.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="noShowGraceMinutes">{t('noShowGrace')}</Label>
              <Input
                id="noShowGraceMinutes"
                type="number"
                min={0}
                step={5}
                {...register('noShowGraceMinutes')}
              />
              <p className="text-xs text-muted-foreground">{t('noShowGraceHelp')}</p>
              {formState.errors.noShowGraceMinutes && (
                <p className="text-xs text-destructive">{formState.errors.noShowGraceMinutes.message}</p>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Toggles */}
      <Card>
        <CardHeader>
          <CardTitle>{t('availability')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-start justify-between gap-4 rounded-lg border p-4">
            <div className="space-y-0.5">
              <p className="text-sm font-medium">{t('enableBooking')}</p>
              <p className="text-xs text-muted-foreground">{t('enableBookingHelp')}</p>
            </div>
            <Switch
              checked={isBookingEnabled}
              onCheckedChange={(v) => setValue('isBookingEnabled', v, { shouldDirty: true })}
            />
          </div>

          <div className="flex items-start justify-between gap-4 rounded-lg border p-4">
            <div className="space-y-0.5">
              <p className="text-sm font-medium">{t('showInDirectory')}</p>
              <p className="text-xs text-muted-foreground">{t('showInDirectoryHelp')}</p>
            </div>
            <Switch
              checked={showInPublicDirectory}
              onCheckedChange={(v) => setValue('showInPublicDirectory', v, { shouldDirty: true })}
            />
          </div>
        </CardContent>
      </Card>

      <div className="flex justify-end">
        <Button type="submit" disabled={saving}>
          {saving ? t('saving') : t('saveSettings')}
        </Button>
      </div>
    </form>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function BookingSettingsPage() {
  const t = useTranslations('settings.booking')
  const tCommon = useTranslations('common')
  const hasFeature = useHasFeature(FeatureKeys.OnlineBooking)
  const { data: branches, isLoading: branchesLoading } = useBranches()
  const [selectedBranch, setSelectedBranch] = useState<string>('')

  // Auto-select the first active branch when it loads.
  useEffect(() => {
    if (!selectedBranch && branches && branches.length > 0) {
      setSelectedBranch(branches[0].id)
    }
  }, [branches, selectedBranch])

  if (!hasFeature) {
    return (
      <div className="space-y-6">
        <PageHeader
          title={t('title')}
          description={t('subtitle')}
          back="/dashboard/settings"
        />
        <BookingUpsell />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('title')}
        description={t('subtitle')}
        back="/dashboard/settings"
        badge={<CalendarCheck className="h-5 w-5 text-muted-foreground" />}
      />

      <Card>
        <CardContent className="pt-6">
          <div className="flex items-end gap-3 max-w-md">
            <div className="flex-1 space-y-1.5">
              <Label>{tCommon('selectBranch')}</Label>
              <Select value={selectedBranch} onValueChange={setSelectedBranch} disabled={branchesLoading}>
                <SelectTrigger>
                  <SelectValue placeholder={tCommon('selectBranch')} />
                </SelectTrigger>
                <SelectContent>
                  {branches?.map((b) => (
                    <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <p className="text-xs text-muted-foreground mt-2">{t('branchScopeHelp')}</p>
        </CardContent>
      </Card>

      {selectedBranch ? (
        <BookingSettingsForm branchId={selectedBranch} t={t} />
      ) : (
        <Card>
          <CardContent className="py-10 text-center text-sm text-muted-foreground">
            {branchesLoading ? tCommon('loading') : t('noBranchSelected')}
          </CardContent>
        </Card>
      )}
    </div>
  )
}
