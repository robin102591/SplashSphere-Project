'use client'

import { useEffect, useState } from 'react'
import { useFieldArray, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Lock, RotateCcw, Plus, Trash2 } from 'lucide-react'

import {
  DisplayFontSize,
  DisplayOrientation,
  DisplayTheme,
  FeatureKeys,
} from '@splashsphere/types'
import type {
  ApiError,
  DisplaySettingDto,
  UpdateDisplaySettingPayload,
} from '@splashsphere/types'

import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Switch } from '@/components/ui/switch'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Card, CardContent, CardDescription, CardHeader, CardTitle,
} from '@/components/ui/card'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { useBranches } from '@/hooks/use-branches'
import { useHasFeature } from '@/hooks/use-plan'
import {
  useDisplaySetting,
  useUpdateDisplaySetting,
  useDeleteDisplayBranchOverride,
} from '@/hooks/use-display-settings'

// ── Schema ────────────────────────────────────────────────────────────────────

const formSchema = z.object({
  // Idle
  showLogo: z.boolean(),
  showBusinessName: z.boolean(),
  showTagline: z.boolean(),
  showDateTime: z.boolean(),
  showGCashQr: z.boolean(),
  showSocialMedia: z.boolean(),
  // useFieldArray needs each entry as an object
  promoMessages: z.array(z.object({ value: z.string().trim().min(1).max(200) })).max(20),
  promoRotationSeconds: z.number().int().min(3).max(60),

  // Building
  showVehicleInfo: z.boolean(),
  showCustomerName: z.boolean(),
  showLoyaltyTier: z.boolean(),
  showDiscountBreakdown: z.boolean(),
  showTaxLine: z.boolean(),

  // Completion
  showPaymentMethod: z.boolean(),
  showChangeAmount: z.boolean(),
  showPointsEarned: z.boolean(),
  showPointsBalance: z.boolean(),
  showThankYouMessage: z.boolean(),
  showPromoText: z.boolean(),
  completionHoldSeconds: z.number().int().min(3).max(30),

  // Appearance
  theme: z.nativeEnum(DisplayTheme),
  fontSize: z.nativeEnum(DisplayFontSize),
  orientation: z.nativeEnum(DisplayOrientation),
})

type FormValues = z.infer<typeof formSchema>

function dtoToForm(d: DisplaySettingDto): FormValues {
  return {
    ...d,
    promoMessages: d.promoMessages.map((m) => ({ value: m })),
  }
}

function formToPayload(v: FormValues): UpdateDisplaySettingPayload {
  return {
    ...v,
    promoMessages: v.promoMessages.map((m) => m.value.trim()).filter(Boolean),
  }
}

const DEFAULT_VALUES: FormValues = {
  showLogo: true, showBusinessName: true, showTagline: true, showDateTime: true,
  showGCashQr: false, showSocialMedia: false,
  promoMessages: [],
  promoRotationSeconds: 8,
  showVehicleInfo: true, showCustomerName: true, showLoyaltyTier: true,
  showDiscountBreakdown: true, showTaxLine: false,
  showPaymentMethod: true, showChangeAmount: true, showPointsEarned: true,
  showPointsBalance: true, showThankYouMessage: true, showPromoText: true,
  completionHoldSeconds: 10,
  theme: DisplayTheme.Dark,
  fontSize: DisplayFontSize.Large,
  orientation: DisplayOrientation.Landscape,
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function DisplaySettingsPage() {
  const [selectedBranchId, setSelectedBranchId] = useState<string | null>(null)
  const hasBranchOverrides = useHasFeature(FeatureKeys.BranchDisplayOverrides)

  const { data: branches = [] } = useBranches()
  const { data: setting, isLoading } = useDisplaySetting(selectedBranchId)
  const { mutateAsync: save, isPending } = useUpdateDisplaySetting()
  const { mutateAsync: removeOverride, isPending: isRemoving } =
    useDeleteDisplayBranchOverride()

  const hasBranchOverride =
    selectedBranchId !== null && setting?.branchId === selectedBranchId

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: DEFAULT_VALUES,
  })

  useEffect(() => {
    if (setting) form.reset(dtoToForm(setting))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [setting])

  const onSubmit = async (values: FormValues) => {
    try {
      await save({
        payload: formToPayload(values),
        branchId: selectedBranchId,
      })
      toast.success(
        selectedBranchId
          ? 'Branch override saved.'
          : 'Tenant default saved.',
      )
    } catch (err) {
      const apiErr = err as ApiError
      toast.error(apiErr?.detail ?? apiErr?.title ?? 'Failed to save settings.')
    }
  }

  const onResetToDefault = async () => {
    if (!selectedBranchId) return
    try {
      await removeOverride(selectedBranchId)
      toast.success('Branch override removed; using tenant default.')
    } catch (err) {
      const apiErr = err as ApiError
      toast.error(apiErr?.detail ?? apiErr?.title ?? 'Failed to remove override.')
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <PageHeader title="Customer Display" description="Loading…" back="/dashboard/settings" />
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  const locked = selectedBranchId !== null && !hasBranchOverrides

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6 pb-12">
      <PageHeader
        title="Customer Display"
        description="Configure what the customer-facing screen shows in each of its three states."
        back="/dashboard/settings"
        actions={
          <Button type="submit" disabled={isPending || !form.formState.isDirty || locked}>
            {isPending ? 'Saving…' : 'Save changes'}
          </Button>
        }
      />

      {/* ── Scope selector ────────────────────────────────────────────── */}
      <Card>
        <CardContent className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
            <Label className="text-sm font-medium shrink-0">Apply to</Label>
            <Select
              value={selectedBranchId ?? 'default'}
              onValueChange={(v) => setSelectedBranchId(v === 'default' ? null : v)}
            >
              <SelectTrigger className="w-full sm:w-72">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="default">Tenant default (all branches)</SelectItem>
                {hasBranchOverrides && branches.map((b) => (
                  <SelectItem key={b.id} value={b.id}>
                    {b.name} (branch override)
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {hasBranchOverride && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={onResetToDefault}
              disabled={isRemoving}
            >
              <RotateCcw className="mr-1.5 h-3.5 w-3.5" />
              {isRemoving ? 'Removing…' : 'Reset to default'}
            </Button>
          )}
        </CardContent>
      </Card>

      {locked && (
        <div className="flex items-start gap-2 rounded-md border border-amber-500/50 bg-amber-500/10 p-3 text-sm text-amber-900 dark:text-amber-200">
          <Lock className="mt-0.5 h-4 w-4 shrink-0" />
          <div>
            <p className="font-medium">Per-branch display overrides require the Enterprise plan.</p>
            <p className="mt-0.5 text-xs opacity-90">
              You can browse the resolved settings for this branch, but you can&apos;t save changes here. Edit the tenant default to apply across all branches.
            </p>
          </div>
        </div>
      )}

      <div className="space-y-6">
        <IdleSection form={form} />
        <PromoMessagesSection form={form} />
        <BuildingSection form={form} />
        <CompletionSection form={form} />
        <AppearanceSection form={form} />
      </div>

      <div className="flex justify-end pt-2">
        <Button type="submit" disabled={isPending || !form.formState.isDirty || locked}>
          {isPending ? 'Saving…' : 'Save changes'}
        </Button>
      </div>
    </form>
  )
}

// ── Sections ──────────────────────────────────────────────────────────────────

type FormApi = ReturnType<typeof useForm<FormValues>>

function IdleSection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Idle screen</CardTitle>
        <CardDescription>
          Shown when no transaction is in progress. Combines branding with rotating promos.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <Toggle form={form} name="showLogo" label="Show logo" hint="Pulled from Company Profile." />
        <Toggle form={form} name="showBusinessName" label="Show business name" />
        <Toggle form={form} name="showTagline" label="Show tagline" />
        <Toggle form={form} name="showDateTime" label="Show date & time" />
        <Toggle form={form} name="showSocialMedia" label="Show social media handles" hint="Pulled from Company Profile." />
        <Toggle form={form} name="showGCashQr" label="Show GCash QR code" hint="Requires uploading the QR image (slice 3 will wire this up)." />
      </CardContent>
    </Card>
  )
}

function PromoMessagesSection({ form }: { form: FormApi }) {
  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: 'promoMessages',
  })

  return (
    <Card>
      <CardHeader>
        <CardTitle>Promo messages</CardTitle>
        <CardDescription>
          Rotated on the idle screen. Up to 20 messages. Empty = no rotation.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        {fields.length === 0 && (
          <p className="text-sm text-muted-foreground">
            No promo messages yet. The idle screen will only show branding + date.
          </p>
        )}
        {fields.map((field, index) => (
          <div key={field.id} className="flex items-center gap-2">
            <Input
              {...form.register(`promoMessages.${index}.value` as const)}
              maxLength={200}
              placeholder="Next wash 10% off for Gold members!"
            />
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="h-9 w-9 shrink-0 text-destructive hover:text-destructive"
              onClick={() => remove(index)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ))}

        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => append({ value: '' })}
          disabled={fields.length >= 20}
        >
          <Plus className="mr-1.5 h-3.5 w-3.5" />
          Add message
        </Button>

        <div className="grid gap-3 sm:grid-cols-2 pt-2">
          <NumberField
            form={form}
            name="promoRotationSeconds"
            label="Rotation interval (seconds)"
            min={3}
            max={60}
            hint="How long each message stays visible. 3-60 seconds."
          />
        </div>
      </CardContent>
    </Card>
  )
}

function BuildingSection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Transaction screen</CardTitle>
        <CardDescription>
          Live updates as the cashier builds the bill. Items, subtotal, and total are always shown.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <Toggle form={form} name="showVehicleInfo" label="Show vehicle info" hint="Plate, make/model, type/size." />
        <Toggle form={form} name="showCustomerName" label="Show customer name" />
        <Toggle form={form} name="showLoyaltyTier" label="Show loyalty tier badge" />
        <Toggle form={form} name="showDiscountBreakdown" label="Show discount breakdown" />
        <Toggle form={form} name="showTaxLine" label="Show tax line" hint="Off by default — most car washes are non-VAT." />
      </CardContent>
    </Card>
  )
}

function CompletionSection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Completion screen</CardTitle>
        <CardDescription>
          Shown briefly after payment, then auto-returns to idle.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <Toggle form={form} name="showPaymentMethod" label="Show payment method" />
        <Toggle form={form} name="showChangeAmount" label="Show change amount" />
        <Toggle form={form} name="showPointsEarned" label="Show loyalty points earned" />
        <Toggle form={form} name="showPointsBalance" label="Show points balance" />
        <Toggle form={form} name="showThankYouMessage" label="Show thank-you message" hint="Pulled from Receipt Designer." />
        <Toggle form={form} name="showPromoText" label="Show promo text" hint="Pulled from Receipt Designer." />

        <div className="grid gap-3 sm:grid-cols-2 pt-2">
          <NumberField
            form={form}
            name="completionHoldSeconds"
            label="Hold duration (seconds)"
            min={3}
            max={30}
            hint="How long the completion screen stays visible. 3-30 seconds."
          />
        </div>
      </CardContent>
    </Card>
  )
}

function AppearanceSection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Appearance</CardTitle>
        <CardDescription>Theme, font, and screen orientation.</CardDescription>
      </CardHeader>
      <CardContent className="grid gap-3 sm:grid-cols-3">
        <SelectField form={form} name="theme" label="Theme" options={[
          { value: DisplayTheme.Dark,  label: 'Dark — navy + white' },
          { value: DisplayTheme.Light, label: 'Light — white + dark' },
          { value: DisplayTheme.Brand, label: 'Brand colors' },
        ]} />
        <SelectField form={form} name="fontSize" label="Font size" options={[
          { value: DisplayFontSize.Normal,     label: 'Normal' },
          { value: DisplayFontSize.Large,      label: 'Large' },
          { value: DisplayFontSize.ExtraLarge, label: 'Extra Large' },
        ]} />
        <SelectField form={form} name="orientation" label="Orientation" options={[
          { value: DisplayOrientation.Landscape, label: 'Landscape' },
          { value: DisplayOrientation.Portrait,  label: 'Portrait' },
        ]} />
      </CardContent>
    </Card>
  )
}

// ── Reusable controls ─────────────────────────────────────────────────────────

function Toggle({
  form, name, label, hint,
}: {
  form: FormApi
  // useFieldArray fields aren't valid for the Switch toggle.
  name: Exclude<keyof FormValues, 'promoMessages' | 'promoRotationSeconds' | 'completionHoldSeconds' | 'theme' | 'fontSize' | 'orientation'>
  label: string
  hint?: string
}) {
  // useWatch (not form.watch) — per-field subscription so each toggle only
  // re-renders itself, not every other toggle on the page.
  const checked = useWatch({ control: form.control, name }) as boolean
  return (
    <div className="flex items-start justify-between gap-4">
      <div className="space-y-0.5">
        <Label htmlFor={name} className="text-sm font-medium">{label}</Label>
        {hint && <p className="text-xs text-muted-foreground">{hint}</p>}
      </div>
      <Switch
        id={name}
        checked={checked}
        onCheckedChange={(v) => form.setValue(name, v as never, { shouldDirty: true })}
      />
    </div>
  )
}

function SelectField<TName extends keyof FormValues>({
  form, name, label, options,
}: {
  form: FormApi
  name: TName
  label: string
  options: { value: number; label: string }[]
}) {
  const value = useWatch({ control: form.control, name }) as number
  return (
    <div className="space-y-1.5">
      <Label className="text-sm font-medium">{label}</Label>
      <Select
        value={String(value)}
        onValueChange={(v) => form.setValue(name, Number(v) as never, { shouldDirty: true })}
      >
        <SelectTrigger><SelectValue /></SelectTrigger>
        <SelectContent>
          {options.map((o) => (
            <SelectItem key={o.value} value={String(o.value)}>{o.label}</SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  )
}

function NumberField({
  form, name, label, hint, min, max,
}: {
  form: FormApi
  name: 'promoRotationSeconds' | 'completionHoldSeconds'
  label: string
  hint?: string
  min: number
  max: number
}) {
  return (
    <div className="space-y-1.5">
      <Label className="text-sm font-medium">{label}</Label>
      <Input
        type="number"
        min={min}
        max={max}
        {...form.register(name, { valueAsNumber: true })}
      />
      {hint && <p className="text-xs text-muted-foreground">{hint}</p>}
    </div>
  )
}
