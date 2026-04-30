'use client'

import { useEffect, useState } from 'react'
import { useFieldArray, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Lock, RotateCcw, Plus, Trash2, CheckCircle2, Star, Droplets } from 'lucide-react'
import Image from 'next/image'

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
import { useCompanyProfile } from '@/hooks/use-company-profile'
import { useHasFeature, usePlan } from '@/hooks/use-plan'
import { formatPeso } from '@splashsphere/format'
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

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_420px]">
        {/* ── Form column ──────────────────────────────────────────── */}
        <div className="space-y-6 min-w-0">
          <IdleSection form={form} />
          <PromoMessagesSection form={form} />
          <BuildingSection form={form} />
          <CompletionSection form={form} />
          <AppearanceSection form={form} />
        </div>

        {/* ── Preview column ──────────────────────────────────────── */}
        <div className="lg:sticky lg:top-4 lg:self-start">
          <DisplayPreview form={form} />
        </div>
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
  const { data: plan } = usePlan()
  // Default conservatively to 1 (Starter) until the plan response lands so
  // we don't briefly enable Add for users who can't actually save more.
  const cap = plan?.limits.maxPromoMessages ?? 1
  const atCap = fields.length >= cap

  return (
    <Card>
      <CardHeader>
        <CardTitle>Promo messages</CardTitle>
        <CardDescription>
          Rotated on the idle screen. Your {plan?.planName ?? 'current'} plan
          allows up to {cap} message{cap === 1 ? '' : 's'}.
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
          disabled={atCap}
          title={atCap ? `Upgrade your plan to add more than ${cap} promo message${cap === 1 ? '' : 's'}.` : undefined}
        >
          <Plus className="mr-1.5 h-3.5 w-3.5" />
          Add message {fields.length > 0 && <span className="ml-1 text-muted-foreground">({fields.length}/{cap})</span>}
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

// ── Live preview ──────────────────────────────────────────────────────────────
//
// Scaled-down render of the customer display in a 16:9 frame, with a tab
// switcher across the three states (Idle / Building / Complete). Reads the
// form's current values via useWatch (no rerender of the form sections) and
// pulls the tenant's branding from useCompanyProfile so the preview matches
// what a customer would actually see at the counter.

const SAMPLE_TX = {
  vehicle: { plate: 'ABC-1234', makeModel: 'Toyota Vios', typeSize: 'Sedan / Medium' },
  customer: { name: 'Maria Santos', tier: 'Gold' },
  items: [
    { name: 'Basic Wash',       qty: 1, total: 200 },
    { name: 'Tire & Rim Shine', qty: 1, total: 120 },
    { name: 'Air Freshener',    qty: 1, total:  85 },
  ],
  subtotal: 405, discount: 32, tax: 0, total: 373,
  paid: 400, change: 27, pointsEarned: 37, pointsBalance: 1277,
}

function DisplayPreview({ form }: { form: FormApi }) {
  const { data: company } = useCompanyProfile()
  const [tab, setTab] = useState<'idle' | 'building' | 'complete'>('idle')

  // Watch the whole form so the preview reflects every toggle. The receipt
  // designer scoped useWatch per-control because the controls themselves are
  // performance-sensitive; here, the preview is the consumer and re-rendering
  // it on every change is the desired behaviour.
  const v = useWatch({ control: form.control })

  const theme = themeClasses(v.theme as DisplayTheme | undefined)
  const businessName = company?.name ?? 'Your Business'
  const tagline = company?.tagline ?? null
  const logoUrl = company?.logoThumbnailUrl ?? null

  return (
    <Card>
      <CardHeader className="space-y-2 pb-3">
        <CardTitle className="text-base">Live preview</CardTitle>
        <div className="flex gap-1">
          {(['idle', 'building', 'complete'] as const).map((k) => (
            <button
              key={k}
              type="button"
              onClick={() => setTab(k)}
              className={`px-2.5 py-1 text-xs rounded-md transition-colors capitalize ${
                tab === k
                  ? 'bg-primary text-primary-foreground'
                  : 'bg-muted text-muted-foreground hover:bg-muted/70'
              }`}
            >
              {k}
            </button>
          ))}
        </div>
      </CardHeader>
      <CardContent>
        <div className={`aspect-video w-full overflow-hidden rounded-lg border ${theme.bg} ${theme.text} text-[10px]`}>
          {tab === 'idle' && (
            <PreviewIdle v={v} theme={theme} businessName={businessName} tagline={tagline} logoUrl={logoUrl} />
          )}
          {tab === 'building' && (
            <PreviewBuilding v={v} theme={theme} businessName={businessName} logoUrl={logoUrl} />
          )}
          {tab === 'complete' && (
            <PreviewComplete v={v} theme={theme} businessName={businessName} logoUrl={logoUrl} />
          )}
        </div>
        <p className="text-[11px] text-muted-foreground mt-2 leading-snug">
          Sample data — actual content (vehicle, customer, items) flows in live from the POS.
        </p>
      </CardContent>
    </Card>
  )
}

interface PreviewTheme {
  bg: string
  text: string
  textMuted: string
  surface: string
  border: string
  accent: string
}

function themeClasses(t: DisplayTheme | undefined): PreviewTheme {
  switch (t) {
    case DisplayTheme.Light:
      return {
        bg: 'bg-white', text: 'text-slate-900', textMuted: 'text-slate-500',
        surface: 'bg-slate-50', border: 'border-slate-200', accent: 'text-blue-600',
      }
    case DisplayTheme.Brand:
      return {
        bg: 'bg-gradient-to-br from-blue-950 to-slate-950', text: 'text-white',
        textMuted: 'text-slate-400', surface: 'bg-blue-900/30',
        border: 'border-blue-800/40', accent: 'text-blue-300',
      }
    case DisplayTheme.Dark:
    default:
      return {
        bg: 'bg-slate-950', text: 'text-white', textMuted: 'text-slate-400',
        surface: 'bg-slate-900', border: 'border-slate-800', accent: 'text-blue-400',
      }
  }
}

type PreviewValues = ReturnType<typeof useWatch<FormValues>>

function PreviewIdle({
  v, theme, businessName, tagline, logoUrl,
}: {
  v: PreviewValues
  theme: PreviewTheme
  businessName: string
  tagline: string | null
  logoUrl: string | null
}) {
  const promo = v.promoMessages?.[0]?.value
  return (
    <div className="h-full flex flex-col items-center justify-center px-4 py-3 gap-2 text-center">
      {v.showLogo && (
        logoUrl ? (
          <Image src={logoUrl} alt="" width={32} height={32} className="h-8 w-8 object-contain" unoptimized />
        ) : (
          <div className="flex h-8 w-8 items-center justify-center rounded-md bg-blue-500/90">
            <Droplets className="h-4 w-4 text-white" />
          </div>
        )
      )}
      {v.showBusinessName && <p className="text-sm font-bold tracking-tight">{businessName}</p>}
      {v.showTagline && tagline && <p className={`text-[9px] ${theme.textMuted}`}>{tagline}</p>}
      <div className={`h-px w-1/3 ${theme.border} border-t my-1`} />
      <p className="text-[10px]">🚗 Welcome! Mabuhay! 🚗</p>
      {promo && <p className={`text-[9px] ${theme.accent} px-2`}>&ldquo;{promo}&rdquo;</p>}
      {v.showDateTime && (
        <div className={`mt-auto ${theme.textMuted}`}>
          <p className="text-[8px]">Saturday, April 30, 2026</p>
          <p className="text-[10px] font-semibold tabular-nums">2:41 PM</p>
        </div>
      )}
    </div>
  )
}

function PreviewBuilding({
  v, theme, businessName, logoUrl,
}: {
  v: PreviewValues
  theme: PreviewTheme
  businessName: string
  logoUrl: string | null
}) {
  return (
    <div className="h-full flex flex-col px-3 py-2 gap-1.5">
      <header className="flex items-center gap-1.5">
        {v.showLogo && (
          logoUrl ? (
            <Image src={logoUrl} alt="" width={20} height={20} className="h-4 w-4 object-contain" unoptimized />
          ) : (
            <div className="flex h-4 w-4 items-center justify-center rounded-sm bg-blue-500/90">
              <Droplets className="h-2.5 w-2.5 text-white" />
            </div>
          )
        )}
        {v.showBusinessName && <p className="text-[10px] font-bold">{businessName}</p>}
      </header>
      {(v.showVehicleInfo || v.showCustomerName) && (
        <div className={`rounded ${theme.surface} ${theme.border} border px-2 py-1 space-y-0.5 text-[8px]`}>
          {v.showVehicleInfo && (
            <p>
              <span className={theme.textMuted}>Vehicle: </span>
              <span className="font-semibold">{SAMPLE_TX.vehicle.makeModel} • {SAMPLE_TX.vehicle.plate} • {SAMPLE_TX.vehicle.typeSize}</span>
            </p>
          )}
          {v.showCustomerName && (
            <p className="flex items-center gap-1">
              <span className={theme.textMuted}>Customer: </span>
              <span className="font-semibold">{SAMPLE_TX.customer.name}</span>
              {v.showLoyaltyTier && (
                <span className={`inline-flex items-center gap-0.5 ${theme.accent} text-[7px] font-bold`}>
                  <Star className="h-2 w-2 fill-current" />
                  {SAMPLE_TX.customer.tier}
                </span>
              )}
            </p>
          )}
        </div>
      )}
      <div className="flex-1 min-h-0 overflow-hidden">
        <div className={`text-[7px] uppercase tracking-wider ${theme.textMuted} flex items-center justify-between border-b ${theme.border} pb-0.5`}>
          <span>Item</span>
          <span>Amount</span>
        </div>
        {SAMPLE_TX.items.map((item, i) => (
          <div key={i} className={`flex items-center justify-between text-[9px] py-0.5 border-b ${theme.border}`}>
            <span>{item.name}</span>
            <span className="tabular-nums font-semibold">{formatPeso(item.total)}</span>
          </div>
        ))}
      </div>
      <div className={`pt-1 border-t-2 ${theme.border} space-y-0.5 text-[8px]`}>
        <Row label="Subtotal" value={formatPeso(SAMPLE_TX.subtotal)} muted={theme.textMuted} />
        {v.showDiscountBreakdown && (
          <Row label="Discount" value={`-${formatPeso(SAMPLE_TX.discount)}`} muted={theme.textMuted} accent={theme.accent} />
        )}
        {v.showTaxLine && (
          <Row label="Tax" value={formatPeso(SAMPLE_TX.tax)} muted={theme.textMuted} />
        )}
        <div className={`pt-1 mt-0.5 border-t ${theme.border} flex items-baseline justify-between`}>
          <span className="text-[9px] font-semibold uppercase tracking-wider">Total</span>
          <span className="text-base font-bold tabular-nums">{formatPeso(SAMPLE_TX.total)}</span>
        </div>
      </div>
    </div>
  )
}

function PreviewComplete({
  v, theme, businessName, logoUrl,
}: {
  v: PreviewValues
  theme: PreviewTheme
  businessName: string
  logoUrl: string | null
}) {
  return (
    <div className="h-full flex flex-col px-3 py-2 gap-1.5">
      <header className="flex items-center gap-1.5">
        {v.showLogo && (
          logoUrl ? (
            <Image src={logoUrl} alt="" width={20} height={20} className="h-4 w-4 object-contain" unoptimized />
          ) : (
            <div className="flex h-4 w-4 items-center justify-center rounded-sm bg-blue-500/90">
              <Droplets className="h-2.5 w-2.5 text-white" />
            </div>
          )
        )}
        {v.showBusinessName && <p className="text-[10px] font-bold">{businessName}</p>}
      </header>
      <div className="flex items-center justify-center gap-1.5 py-1">
        <CheckCircle2 className="h-4 w-4 text-emerald-400" />
        <p className="text-[11px] font-bold uppercase tracking-wider text-emerald-400">Payment Complete</p>
      </div>
      <div className={`pt-1 border-t ${theme.border} space-y-0.5 text-[8px]`}>
        <Row label="Subtotal" value={formatPeso(SAMPLE_TX.subtotal)} muted={theme.textMuted} />
        {v.showDiscountBreakdown && (
          <Row label="Discount" value={`-${formatPeso(SAMPLE_TX.discount)}`} muted={theme.textMuted} accent={theme.accent} />
        )}
        <div className="flex items-baseline justify-between pt-0.5">
          <span className="text-[9px] font-semibold">Total</span>
          <span className="text-xs font-bold tabular-nums">{formatPeso(SAMPLE_TX.total)}</span>
        </div>
      </div>
      <div className={`rounded ${theme.surface} ${theme.border} border px-2 py-1 space-y-0.5 text-[8px]`}>
        {v.showPaymentMethod && (
          <Row label="Paid: Cash" value={formatPeso(SAMPLE_TX.paid)} muted={theme.textMuted} />
        )}
        {v.showChangeAmount && (
          <Row label="Change" value={formatPeso(SAMPLE_TX.change)} muted={theme.textMuted} />
        )}
      </div>
      {(v.showPointsEarned || v.showPointsBalance) && (
        <div className={`rounded ${theme.surface} ${theme.border} border px-2 py-1 space-y-0.5 text-[8px]`}>
          {v.showPointsEarned && (
            <p className="flex items-center justify-between">
              <span className={`flex items-center gap-1 ${theme.accent}`}>
                <Star className="h-2.5 w-2.5 fill-current" /> Points Earned
              </span>
              <span className="font-semibold tabular-nums">+{SAMPLE_TX.pointsEarned} pts</span>
            </p>
          )}
          {v.showPointsBalance && (
            <p className="flex items-center justify-between">
              <span className={`flex items-center gap-1 ${theme.accent}`}>
                <Star className="h-2.5 w-2.5 fill-current" /> New Balance
              </span>
              <span className="font-semibold tabular-nums">{SAMPLE_TX.pointsBalance.toLocaleString()} pts</span>
            </p>
          )}
        </div>
      )}
      <div className="text-center mt-auto pt-1 space-y-0.5">
        {v.showThankYouMessage && (
          <p className="text-[9px] font-semibold">Thank you for your patronage!</p>
        )}
        {v.showPromoText && (
          <p className={`text-[8px] ${theme.accent}`}>Next wash 10% off!</p>
        )}
      </div>
    </div>
  )
}

function Row({
  label, value, muted, accent,
}: {
  label: string
  value: string
  muted: string
  accent?: string
}) {
  return (
    <div className="flex items-center justify-between">
      <span className={accent ?? muted}>{label}</span>
      <span className={`tabular-nums font-medium ${accent ?? ''}`}>{value}</span>
    </div>
  )
}
