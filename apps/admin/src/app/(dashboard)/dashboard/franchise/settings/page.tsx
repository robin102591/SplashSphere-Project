'use client'

import { useEffect } from 'react'
import { useTranslations } from 'next-intl'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Switch } from '@/components/ui/switch'
import { PageHeader } from '@/components/ui/page-header'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { useFranchiseSettings, useUpdateFranchiseSettings } from '@/hooks/use-franchise'
import { toast } from 'sonner'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

// ── Schema ────────────────────────────────────────────────────────────────────

const settingsSchema = z.object({
  royaltyRate: z.coerce.number().min(0, 'Min 0').max(100, 'Max 100'),
  marketingFeeRate: z.coerce.number().min(0, 'Min 0').max(100, 'Max 100'),
  technologyFeeRate: z.coerce.number().min(0, 'Min 0').max(100, 'Max 100'),
  royaltyBasis: z.coerce.number(),
  royaltyFrequency: z.coerce.number(),
  enforceStandardServices: z.boolean(),
  enforceStandardPricing: z.boolean(),
  allowLocalServices: z.boolean(),
  maxPriceVariance: z.coerce.number().nullable(),
  enforceBranding: z.boolean(),
  defaultFranchiseePlan: z.coerce.number(),
  maxBranchesPerFranchisee: z.coerce.number().min(1, 'Must be at least 1'),
})

type SettingsForm = z.infer<typeof settingsSchema>

// ── Enum Labels ───────────────────────────────────────────────────────────────

const ROYALTY_BASIS_LABELS: Record<number, string> = {
  0: 'Gross Revenue',
  1: 'Net Revenue',
  2: 'Service Revenue Only',
}

const ROYALTY_FREQUENCY_LABELS: Record<number, string> = {
  0: 'Weekly',
  1: 'Monthly',
}

const PLAN_LABELS: Record<number, string> = {
  0: 'Starter',
  1: 'Growth',
  2: 'Enterprise',
}

// ── Loading Skeleton ──────────────────────────────────────────────────────────

function SettingsSkeleton() {
  return (
    <div className="space-y-6 max-w-2xl">
      <Skeleton className="h-72 w-full" />
      <Skeleton className="h-56 w-full" />
      <Skeleton className="h-44 w-full" />
    </div>
  )
}

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function FranchiseSettingsPage() {
  const t = useTranslations('franchise')
  const { data: settings, isLoading } = useFranchiseSettings()
  const { mutateAsync: updateSettings, isPending } = useUpdateFranchiseSettings()

  const { register, handleSubmit, formState, control, reset, watch } = useForm<SettingsForm>({
    resolver: zodResolver(settingsSchema),
    defaultValues: {
      royaltyRate: 0,
      marketingFeeRate: 0,
      technologyFeeRate: 0,
      royaltyBasis: 0,
      royaltyFrequency: 1,
      enforceStandardServices: false,
      enforceStandardPricing: false,
      allowLocalServices: true,
      maxPriceVariance: null,
      enforceBranding: false,
      defaultFranchiseePlan: 0,
      maxBranchesPerFranchisee: 3,
    },
  })

  const enforceStandardPricing = watch('enforceStandardPricing')

  useEffect(() => {
    if (settings) {
      reset({
        royaltyRate: settings.royaltyRate,
        marketingFeeRate: settings.marketingFeeRate,
        technologyFeeRate: settings.technologyFeeRate,
        royaltyBasis: settings.royaltyBasis,
        royaltyFrequency: settings.royaltyFrequency,
        enforceStandardServices: settings.enforceStandardServices,
        enforceStandardPricing: settings.enforceStandardPricing,
        allowLocalServices: settings.allowLocalServices,
        maxPriceVariance: settings.maxPriceVariance,
        enforceBranding: settings.enforceBranding,
        defaultFranchiseePlan: settings.defaultFranchiseePlan,
        maxBranchesPerFranchisee: settings.maxBranchesPerFranchisee,
      })
    }
  }, [settings, reset])

  const onSubmit = async (values: SettingsForm) => {
    try {
      await updateSettings(values)
      toast.success('Settings updated')
    } catch {
      toast.error('Failed to update settings')
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('settings')}
        back="/dashboard/franchise"
        description="Configure franchise network rules, royalties, and standards."
      />

      {isLoading ? (
        <SettingsSkeleton />
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 max-w-2xl">
          {/* Royalty Configuration */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Royalty Configuration</CardTitle>
              <CardDescription>Define how royalties, marketing fees, and technology fees are calculated.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-1.5">
                  <Label>{t('royaltyRate')} (%)</Label>
                  <Input type="number" step="0.01" min="0" max="100" {...register('royaltyRate')} />
                  {formState.errors.royaltyRate && (
                    <p className="text-xs text-destructive">{formState.errors.royaltyRate.message}</p>
                  )}
                </div>
                <div className="space-y-1.5">
                  <Label>{t('marketingFee')} (%)</Label>
                  <Input type="number" step="0.01" min="0" max="100" {...register('marketingFeeRate')} />
                  {formState.errors.marketingFeeRate && (
                    <p className="text-xs text-destructive">{formState.errors.marketingFeeRate.message}</p>
                  )}
                </div>
                <div className="space-y-1.5">
                  <Label>{t('technologyFee')} (%)</Label>
                  <Input type="number" step="0.01" min="0" max="100" {...register('technologyFeeRate')} />
                  {formState.errors.technologyFeeRate && (
                    <p className="text-xs text-destructive">{formState.errors.technologyFeeRate.message}</p>
                  )}
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label>Royalty Basis</Label>
                  <Controller
                    control={control}
                    name="royaltyBasis"
                    render={({ field }) => (
                      <Select value={String(field.value)} onValueChange={(v) => field.onChange(Number(v))}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {Object.entries(ROYALTY_BASIS_LABELS).map(([val, label]) => (
                            <SelectItem key={val} value={val}>{label}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
                <div className="space-y-1.5">
                  <Label>Royalty Frequency</Label>
                  <Controller
                    control={control}
                    name="royaltyFrequency"
                    render={({ field }) => (
                      <Select value={String(field.value)} onValueChange={(v) => field.onChange(Number(v))}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {Object.entries(ROYALTY_FREQUENCY_LABELS).map(([val, label]) => (
                            <SelectItem key={val} value={val}>{label}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Service Standards */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Service Standards</CardTitle>
              <CardDescription>Control how franchisees manage their service offerings and pricing.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <Label>{t('enforceServices')}</Label>
                  <p className="text-xs text-muted-foreground">Franchisees must offer all standard services.</p>
                </div>
                <Controller
                  control={control}
                  name="enforceStandardServices"
                  render={({ field }) => (
                    <Switch checked={field.value} onCheckedChange={field.onChange} />
                  )}
                />
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <Label>{t('enforcePricing')}</Label>
                  <p className="text-xs text-muted-foreground">Franchisees must follow standard pricing within allowed variance.</p>
                </div>
                <Controller
                  control={control}
                  name="enforceStandardPricing"
                  render={({ field }) => (
                    <Switch checked={field.value} onCheckedChange={field.onChange} />
                  )}
                />
              </div>

              {enforceStandardPricing && (
                <div className="space-y-1.5 pl-4 border-l-2 border-muted">
                  <Label>{t('maxPriceVariance')}</Label>
                  <Input
                    type="number"
                    step="0.1"
                    min="0"
                    max="100"
                    placeholder="e.g. 10"
                    {...register('maxPriceVariance')}
                  />
                  <p className="text-xs text-muted-foreground">
                    Maximum percentage deviation from standard pricing allowed.
                  </p>
                </div>
              )}

              <div className="flex items-center justify-between">
                <div>
                  <Label>{t('allowLocalServices')}</Label>
                  <p className="text-xs text-muted-foreground">Allow franchisees to add their own local services.</p>
                </div>
                <Controller
                  control={control}
                  name="allowLocalServices"
                  render={({ field }) => (
                    <Switch checked={field.value} onCheckedChange={field.onChange} />
                  )}
                />
              </div>
            </CardContent>
          </Card>

          {/* Branding & Plans */}
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Branding &amp; Plans</CardTitle>
              <CardDescription>Set branding enforcement and default plan assignment for new franchisees.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <Label>{t('enforceBranding')}</Label>
                  <p className="text-xs text-muted-foreground">Require franchisees to use the standard brand identity.</p>
                </div>
                <Controller
                  control={control}
                  name="enforceBranding"
                  render={({ field }) => (
                    <Switch checked={field.value} onCheckedChange={field.onChange} />
                  )}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label>Default Franchisee Plan</Label>
                  <Controller
                    control={control}
                    name="defaultFranchiseePlan"
                    render={({ field }) => (
                      <Select value={String(field.value)} onValueChange={(v) => field.onChange(Number(v))}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {Object.entries(PLAN_LABELS).map(([val, label]) => (
                            <SelectItem key={val} value={val}>{label}</SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
                <div className="space-y-1.5">
                  <Label>Max Branches per Franchisee</Label>
                  <Input type="number" min="1" {...register('maxBranchesPerFranchisee')} />
                  {formState.errors.maxBranchesPerFranchisee && (
                    <p className="text-xs text-destructive">{formState.errors.maxBranchesPerFranchisee.message}</p>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>

          <div className="flex justify-end">
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Saving...' : t('saveSettings')}
            </Button>
          </div>
        </form>
      )}
    </div>
  )
}
