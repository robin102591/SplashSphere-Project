'use client'

import { useState, useMemo } from 'react'
import {
  Award, Gift, Settings as SettingsIcon, Users, TrendingUp, Star,
  Plus, Pencil, Power, PowerOff,
} from 'lucide-react'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { StatCard } from '@/components/ui/stat-card'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter,
} from '@/components/ui/dialog'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import {
  useLoyaltySettings,
  useUpsertLoyaltySettings,
  useUpsertLoyaltyTiers,
  useLoyaltyRewards,
  useCreateLoyaltyReward,
  useUpdateLoyaltyReward,
  useToggleLoyaltyRewardStatus,
  useLoyaltyDashboard,
} from '@/hooks/use-loyalty'
import { LoyaltyTier, RewardType } from '@splashsphere/types'
import type { LoyaltyRewardDto, LoyaltyTierConfigDto } from '@splashsphere/types'
import { toast } from 'sonner'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { formatDate } from '@/lib/format'

// ── Helpers ───────────────────────────────────────────────────────────────────

const TIER_LABELS: Record<number, string> = {
  [LoyaltyTier.Standard]: 'Standard',
  [LoyaltyTier.Silver]: 'Silver',
  [LoyaltyTier.Gold]: 'Gold',
  [LoyaltyTier.Platinum]: 'Platinum',
}

const REWARD_TYPE_LABELS: Record<number, string> = {
  [RewardType.FreeService]: 'Free Service',
  [RewardType.FreePackage]: 'Free Package',
  [RewardType.DiscountAmount]: 'Discount (Fixed)',
  [RewardType.DiscountPercent]: 'Discount (%)',
}

function defaultDates() {
  const to = new Date()
  const from = new Date()
  from.setDate(from.getDate() - 30)
  return {
    from: from.toISOString().slice(0, 10),
    to: to.toISOString().slice(0, 10),
  }
}

// ── Settings Tab ──────────────────────────────────────────────────────────────

const settingsSchema = z.object({
  pointsPerCurrencyUnit: z.coerce.number().min(1),
  currencyUnitAmount: z.coerce.number().min(1),
  isActive: z.boolean(),
  pointsExpirationMonths: z.coerce.number().min(0).nullable(),
  autoEnroll: z.boolean(),
})
type SettingsForm = z.infer<typeof settingsSchema>

const tierRowSchema = z.object({
  tier: z.coerce.number(),
  name: z.string().min(1, 'Required'),
  minimumLifetimePoints: z.coerce.number().min(0),
  pointsMultiplier: z.coerce.number().min(0.1),
})

function SettingsTab() {
  const { data: settings, isLoading } = useLoyaltySettings()
  const { mutateAsync: upsertSettings, isPending: savingSettings } = useUpsertLoyaltySettings()
  const { mutateAsync: upsertTiers, isPending: savingTiers } = useUpsertLoyaltyTiers()

  const [tiers, setTiers] = useState<{
    tier: number; name: string; minimumLifetimePoints: number; pointsMultiplier: number
  }[]>([])
  const [tiersLoaded, setTiersLoaded] = useState(false)

  const {
    register, handleSubmit, formState, reset, watch, setValue,
  } = useForm<SettingsForm>({
    resolver: zodResolver(settingsSchema),
    defaultValues: {
      pointsPerCurrencyUnit: 1,
      currencyUnitAmount: 100,
      isActive: true,
      pointsExpirationMonths: null,
      autoEnroll: true,
    },
  })

  // Load settings into form when data arrives
  if (settings && !formState.isDirty && !tiersLoaded) {
    reset({
      pointsPerCurrencyUnit: settings.pointsPerCurrencyUnit,
      currencyUnitAmount: settings.currencyUnitAmount,
      isActive: settings.isActive,
      pointsExpirationMonths: settings.pointsExpirationMonths,
      autoEnroll: settings.autoEnroll,
    })
    setTiers(
      settings.tiers.map((t) => ({
        tier: t.tier,
        name: t.name,
        minimumLifetimePoints: t.minimumLifetimePoints,
        pointsMultiplier: t.pointsMultiplier,
      }))
    )
    setTiersLoaded(true)
  }

  const isActive = watch('isActive')
  const autoEnroll = watch('autoEnroll')

  const onSaveSettings = async (values: SettingsForm) => {
    try {
      await upsertSettings(values)
      toast.success('Loyalty settings saved')
    } catch {
      toast.error('Failed to save settings')
    }
  }

  const onSaveTiers = async () => {
    try {
      for (const t of tiers) {
        const parsed = tierRowSchema.safeParse(t)
        if (!parsed.success) {
          toast.error(`Invalid tier: ${parsed.error.issues[0]?.message}`)
          return
        }
      }
      await upsertTiers({ tiers })
      toast.success('Tier configuration saved')
    } catch {
      toast.error('Failed to save tiers')
    }
  }

  const updateTier = (index: number, field: string, value: string | number) => {
    setTiers((prev) => prev.map((t, i) => (i === index ? { ...t, [field]: value } : t)))
  }

  const addTier = () => {
    const nextTierValue = tiers.length
    if (nextTierValue > 3) return // max Platinum
    setTiers((prev) => [
      ...prev,
      {
        tier: nextTierValue,
        name: TIER_LABELS[nextTierValue] ?? `Tier ${nextTierValue}`,
        minimumLifetimePoints: nextTierValue * 500,
        pointsMultiplier: 1 + nextTierValue * 0.25,
      },
    ])
  }

  const removeTier = (index: number) => {
    setTiers((prev) => prev.filter((_, i) => i !== index))
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-64 w-full" />
        <Skeleton className="h-48 w-full" />
      </div>
    )
  }

  return (
    <div className="space-y-8 max-w-2xl">
      {/* Program Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Program Settings</CardTitle>
          <CardDescription>Configure how loyalty points are earned and managed.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSaveSettings)} className="space-y-4">
            <div className="flex items-center gap-3">
              <Label className="flex-1">Program Active</Label>
              <Button
                type="button"
                variant={isActive ? 'default' : 'outline'}
                size="sm"
                onClick={() => setValue('isActive', !isActive, { shouldDirty: true })}
              >
                {isActive ? 'Active' : 'Inactive'}
              </Button>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Points per unit</Label>
                <Input type="number" {...register('pointsPerCurrencyUnit')} />
                {formState.errors.pointsPerCurrencyUnit && (
                  <p className="text-xs text-destructive">{formState.errors.pointsPerCurrencyUnit.message}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label>Currency unit amount (PHP)</Label>
                <Input type="number" {...register('currencyUnitAmount')} />
                {formState.errors.currencyUnitAmount && (
                  <p className="text-xs text-destructive">{formState.errors.currencyUnitAmount.message}</p>
                )}
              </div>
            </div>

            <div className="space-y-1.5">
              <Label>Points expiration (months)</Label>
              <Input
                type="number"
                placeholder="Leave empty for no expiration"
                {...register('pointsExpirationMonths')}
              />
              <p className="text-xs text-muted-foreground">Set to 0 or leave empty for points that never expire.</p>
            </div>

            <div className="flex items-center gap-3">
              <Label className="flex-1">Auto-enroll customers on first transaction</Label>
              <Button
                type="button"
                variant={autoEnroll ? 'default' : 'outline'}
                size="sm"
                onClick={() => setValue('autoEnroll', !autoEnroll, { shouldDirty: true })}
              >
                {autoEnroll ? 'Enabled' : 'Disabled'}
              </Button>
            </div>

            <div className="flex justify-end pt-2">
              <Button type="submit" disabled={savingSettings}>
                {savingSettings ? 'Saving...' : 'Save Settings'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {/* Tier Configuration */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle className="text-lg">Tier Configuration</CardTitle>
            <CardDescription>Define membership tiers and their point multipliers.</CardDescription>
          </div>
          <Button variant="outline" size="sm" onClick={addTier} disabled={tiers.length >= 4}>
            <Plus className="mr-2 h-3.5 w-3.5" />
            Add Tier
          </Button>
        </CardHeader>
        <CardContent>
          {tiers.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No tiers configured. All members will be at Standard tier with 1x multiplier.
            </p>
          ) : (
            <div className="space-y-3">
              <div className="grid grid-cols-[1fr_1fr_1fr_1fr_auto] gap-2 text-xs font-medium text-muted-foreground uppercase tracking-wider">
                <span>Tier</span><span>Name</span><span>Min Points</span><span>Multiplier</span><span />
              </div>
              {tiers.map((t, i) => (
                <div key={i} className="grid grid-cols-[1fr_1fr_1fr_1fr_auto] gap-2 items-center">
                  <span className="text-sm font-medium">{TIER_LABELS[t.tier] ?? t.tier}</span>
                  <Input
                    value={t.name}
                    onChange={(e) => updateTier(i, 'name', e.target.value)}
                    className="h-8 text-sm"
                  />
                  <Input
                    type="number"
                    value={t.minimumLifetimePoints}
                    onChange={(e) => updateTier(i, 'minimumLifetimePoints', Number(e.target.value))}
                    className="h-8 text-sm"
                  />
                  <Input
                    type="number"
                    step="0.05"
                    value={t.pointsMultiplier}
                    onChange={(e) => updateTier(i, 'pointsMultiplier', Number(e.target.value))}
                    className="h-8 text-sm"
                  />
                  <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => removeTier(i)}>
                    <span className="text-destructive text-sm">x</span>
                  </Button>
                </div>
              ))}
            </div>
          )}
          <div className="flex justify-end pt-4">
            <Button onClick={onSaveTiers} disabled={savingTiers}>
              {savingTiers ? 'Saving...' : 'Save Tiers'}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

// ── Rewards Tab ───────────────────────────────────────────────────────────────

const rewardSchema = z.object({
  name: z.string().min(1, 'Required'),
  description: z.string().optional(),
  rewardType: z.coerce.number(),
  pointsCost: z.coerce.number().min(1, 'Must be at least 1'),
  serviceId: z.string().optional(),
  packageId: z.string().optional(),
  discountAmount: z.coerce.number().optional(),
  discountPercent: z.coerce.number().min(0).max(100).optional(),
})
type RewardForm = z.infer<typeof rewardSchema>

function RewardDialog({
  open,
  onOpenChange,
  reward,
}: {
  open: boolean
  onOpenChange: (v: boolean) => void
  reward?: LoyaltyRewardDto | null
}) {
  const { mutateAsync: create, isPending: creating } = useCreateLoyaltyReward()
  const { mutateAsync: update, isPending: updating } = useUpdateLoyaltyReward()
  const isEdit = !!reward

  const { register, handleSubmit, formState, control, watch, reset } = useForm<RewardForm>({
    resolver: zodResolver(rewardSchema),
    defaultValues: reward
      ? {
          name: reward.name,
          description: reward.description ?? '',
          rewardType: reward.rewardType,
          pointsCost: reward.pointsCost,
          discountAmount: reward.discountAmount ?? undefined,
          discountPercent: reward.discountPercent ?? undefined,
        }
      : {
          name: '',
          description: '',
          rewardType: RewardType.DiscountPercent,
          pointsCost: 100,
        },
  })

  const rewardType = watch('rewardType')

  const onSubmit = async (values: RewardForm) => {
    try {
      if (isEdit) {
        await update({
          id: reward.id,
          name: values.name,
          description: values.description,
          rewardType: values.rewardType,
          pointsCost: values.pointsCost,
          discountAmount: values.discountAmount,
          discountPercent: values.discountPercent,
        })
        toast.success('Reward updated')
      } else {
        await create({
          name: values.name,
          description: values.description,
          rewardType: values.rewardType,
          pointsCost: values.pointsCost,
          discountAmount: values.discountAmount,
          discountPercent: values.discountPercent,
        })
        toast.success('Reward created')
      }
      reset()
      onOpenChange(false)
    } catch {
      toast.error(isEdit ? 'Failed to update reward' : 'Failed to create reward')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Reward' : 'New Reward'}</DialogTitle>
          <DialogDescription>
            {isEdit ? 'Update the reward details.' : 'Create a new reward for customers to redeem.'}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label>Name <span className="text-destructive">*</span></Label>
            <Input placeholder="e.g. 10% Off Next Wash" {...register('name')} />
            {formState.errors.name && (
              <p className="text-xs text-destructive">{formState.errors.name.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label>Description</Label>
            <Textarea placeholder="Optional description..." {...register('description')} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Reward Type</Label>
              <Controller
                control={control}
                name="rewardType"
                render={({ field }) => (
                  <Select value={String(field.value)} onValueChange={(v) => field.onChange(Number(v))}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(REWARD_TYPE_LABELS).map(([val, label]) => (
                        <SelectItem key={val} value={val}>{label}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
            <div className="space-y-1.5">
              <Label>Points Cost <span className="text-destructive">*</span></Label>
              <Input type="number" {...register('pointsCost')} />
              {formState.errors.pointsCost && (
                <p className="text-xs text-destructive">{formState.errors.pointsCost.message}</p>
              )}
            </div>
          </div>

          {rewardType === RewardType.DiscountAmount && (
            <div className="space-y-1.5">
              <Label>Discount Amount (PHP)</Label>
              <Input type="number" step="0.01" {...register('discountAmount')} />
            </div>
          )}

          {rewardType === RewardType.DiscountPercent && (
            <div className="space-y-1.5">
              <Label>Discount Percent (%)</Label>
              <Input type="number" step="0.1" min="0" max="100" {...register('discountPercent')} />
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" type="button" onClick={() => onOpenChange(false)}>Cancel</Button>
            <Button type="submit" disabled={creating || updating}>
              {(creating || updating) ? 'Saving...' : isEdit ? 'Update' : 'Create'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function RewardsTab() {
  const [page, setPage] = useState(1)
  const { data, isLoading } = useLoyaltyRewards({ page, pageSize: 20 })
  const { mutate: toggleStatus } = useToggleLoyaltyRewardStatus()
  const [createOpen, setCreateOpen] = useState(false)
  const [editing, setEditing] = useState<LoyaltyRewardDto | null>(null)

  const rewards = data?.items ?? []

  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          New Reward
        </Button>
      </div>

      {rewards.length === 0 ? (
        <EmptyState
          icon={Gift}
          title="No rewards yet"
          description="Create rewards that customers can redeem with their loyalty points."
          action={{ label: 'New Reward', onClick: () => setCreateOpen(true), icon: Plus }}
        />
      ) : (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Name</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Type</th>
                <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">Points Cost</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Status</th>
                <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {rewards.map((r) => (
                <tr key={r.id} className="hover:bg-muted/40 transition-colors">
                  <td className="px-4 py-3">
                    <div>
                      <p className="font-medium">{r.name}</p>
                      {r.description && (
                        <p className="text-xs text-muted-foreground mt-0.5">{r.description}</p>
                      )}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {REWARD_TYPE_LABELS[r.rewardType] ?? r.rewardType}
                    {r.discountPercent != null && ` (${r.discountPercent}%)`}
                    {r.discountAmount != null && ` (PHP ${r.discountAmount})`}
                  </td>
                  <td className="px-4 py-3 text-right font-mono tabular-nums">
                    {r.pointsCost.toLocaleString()}
                  </td>
                  <td className="px-4 py-3">
                    <StatusBadge status={r.isActive ? 'Active' : 'Inactive'} />
                  </td>
                  <td className="px-4 py-3 text-right">
                    <div className="flex items-center justify-end gap-1">
                      <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => setEditing(r)}>
                        <Pencil className="h-3.5 w-3.5" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8"
                        onClick={() => toggleStatus(r.id, {
                          onSuccess: () => toast.success(`Reward ${r.isActive ? 'deactivated' : 'activated'}`),
                          onError: () => toast.error('Failed to toggle status'),
                        })}
                      >
                        {r.isActive ? <PowerOff className="h-3.5 w-3.5" /> : <Power className="h-3.5 w-3.5" />}
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {data && data.totalCount > 20 && (
            <div className="flex items-center justify-between px-4 py-3 border-t">
              <p className="text-sm text-muted-foreground">
                Page {page} of {Math.ceil(data.totalCount / 20)}
              </p>
              <div className="flex gap-2">
                <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(page - 1)}>
                  Previous
                </Button>
                <Button variant="outline" size="sm" disabled={page * 20 >= data.totalCount} onClick={() => setPage(page + 1)}>
                  Next
                </Button>
              </div>
            </div>
          )}
        </div>
      )}

      <RewardDialog open={createOpen} onOpenChange={setCreateOpen} />
      <RewardDialog
        open={!!editing}
        onOpenChange={(v) => !v && setEditing(null)}
        reward={editing}
      />
    </div>
  )
}

// ── Dashboard Tab ─────────────────────────────────────────────────────────────

function DashboardTab() {
  const { from, to } = useMemo(defaultDates, [])
  const { data: dashboard, isLoading } = useLoyaltyDashboard(from, to)

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-28" />
          ))}
        </div>
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  if (!dashboard) {
    return (
      <EmptyState
        icon={Award}
        title="No loyalty data"
        description="Configure your loyalty program in the Settings tab to get started."
      />
    )
  }

  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Total Members" value={dashboard.totalMembers.toLocaleString()} icon={Users} />
        <StatCard
          title="Points Earned (30d)"
          value={dashboard.totalPointsEarnedInPeriod.toLocaleString()}
          icon={TrendingUp}
        />
        <StatCard
          title="Points Redeemed (30d)"
          value={dashboard.totalPointsRedeemedInPeriod.toLocaleString()}
          icon={Gift}
        />
        <StatCard
          title="Redemptions (30d)"
          value={dashboard.totalRedemptionsInPeriod.toLocaleString()}
          icon={Star}
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        {/* Tier Distribution */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Tier Distribution</CardTitle>
          </CardHeader>
          <CardContent>
            {dashboard.tierDistribution.length === 0 ? (
              <p className="text-sm text-muted-foreground">No members yet.</p>
            ) : (
              <div className="space-y-3">
                {dashboard.tierDistribution.map((td) => {
                  const pct = dashboard.totalMembers > 0
                    ? Math.round((td.count / dashboard.totalMembers) * 100)
                    : 0
                  return (
                    <div key={td.tier} className="space-y-1">
                      <div className="flex items-center justify-between text-sm">
                        <span className="font-medium">{TIER_LABELS[td.tier] ?? td.tierName}</span>
                        <span className="text-muted-foreground">{td.count} ({pct}%)</span>
                      </div>
                      <div className="h-2 rounded-full bg-muted overflow-hidden">
                        <div
                          className="h-full rounded-full bg-primary transition-all"
                          style={{ width: `${pct}%` }}
                        />
                      </div>
                    </div>
                  )
                })}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Top Customers */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Top Loyal Customers</CardTitle>
          </CardHeader>
          <CardContent>
            {dashboard.topCustomers.length === 0 ? (
              <p className="text-sm text-muted-foreground">No members yet.</p>
            ) : (
              <div className="space-y-2">
                {dashboard.topCustomers.map((c, i) => (
                  <div key={c.customerId} className="flex items-center gap-3 py-1.5">
                    <span className="text-xs font-medium text-muted-foreground w-5 text-right">
                      {i + 1}
                    </span>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium truncate">{c.customerName}</p>
                      <p className="text-xs text-muted-foreground">
                        {c.cardNumber} &middot; {TIER_LABELS[c.currentTier] ?? 'Standard'}
                      </p>
                    </div>
                    <div className="text-right shrink-0">
                      <p className="text-sm font-mono tabular-nums">
                        {c.lifetimePointsEarned.toLocaleString()}
                      </p>
                      <p className="text-xs text-muted-foreground">lifetime pts</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function LoyaltyPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Loyalty Program</h1>
        <p className="text-muted-foreground">Manage your customer loyalty program, rewards, and membership tiers.</p>
      </div>

      <Tabs defaultValue="dashboard">
        <TabsList>
          <TabsTrigger value="dashboard">
            <Award className="mr-2 h-4 w-4" />
            Dashboard
          </TabsTrigger>
          <TabsTrigger value="rewards">
            <Gift className="mr-2 h-4 w-4" />
            Rewards
          </TabsTrigger>
          <TabsTrigger value="settings">
            <SettingsIcon className="mr-2 h-4 w-4" />
            Settings
          </TabsTrigger>
        </TabsList>

        <TabsContent value="dashboard" className="mt-6">
          <DashboardTab />
        </TabsContent>

        <TabsContent value="rewards" className="mt-6">
          <RewardsTab />
        </TabsContent>

        <TabsContent value="settings" className="mt-6">
          <SettingsTab />
        </TabsContent>
      </Tabs>
    </div>
  )
}
