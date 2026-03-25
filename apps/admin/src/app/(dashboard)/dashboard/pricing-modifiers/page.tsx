'use client'

import { useState } from 'react'
import {
  Plus, Percent, Clock, CalendarDays, Star, CloudRain,
  Search, MoreHorizontal, Power, PowerOff, Pencil, Trash2, Tag,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { StatusBadge } from '@/components/ui/status-badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Label } from '@/components/ui/label'
import { DatePicker } from '@/components/ui/date-picker'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from '@/components/ui/dialog'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  usePricingModifiers,
  useCreatePricingModifier,
  useUpdatePricingModifier,
  useDeletePricingModifier,
  useTogglePricingModifier,
} from '@/hooks/use-pricing-modifiers'
import type { PricingModifierFormValues } from '@/hooks/use-pricing-modifiers'
import { useBranches } from '@/hooks/use-branches'
import { ModifierType } from '@splashsphere/types'
import type { PricingModifier } from '@splashsphere/types'
import { toast } from 'sonner'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { formatPeso } from '@/lib/format'

// ── Helpers ──────────────────────────────────────────────────────────────────

const MODIFIER_TYPE_OPTIONS = [
  { value: ModifierType.PeakHour, label: 'Peak Hour', icon: Clock, description: 'Active during configured hours' },
  { value: ModifierType.DayOfWeek, label: 'Day of Week', icon: CalendarDays, description: 'Active on a specific day' },
  { value: ModifierType.Holiday, label: 'Holiday', icon: Star, description: 'Active on a holiday date' },
  { value: ModifierType.Promotion, label: 'Promotion', icon: Tag, description: 'Promotional discount (peso amount)' },
  { value: ModifierType.Weather, label: 'Weather', icon: CloudRain, description: 'Weather-triggered surcharge' },
] as const

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']

function typeIcon(type: ModifierType) {
  const opt = MODIFIER_TYPE_OPTIONS.find(o => o.value === type)
  return opt ? opt.icon : Percent
}

function typeLabel(type: ModifierType) {
  return MODIFIER_TYPE_OPTIONS.find(o => o.value === type)?.label ?? 'Unknown'
}

function formatValue(type: ModifierType, value: number) {
  if (type === ModifierType.Promotion) {
    return `${formatPeso(value)} off`
  }
  const pct = ((value - 1) * 100).toFixed(0)
  return value >= 1 ? `+${pct}%` : `${pct}%`
}

function formatCondition(mod: PricingModifier) {
  switch (mod.type) {
    case ModifierType.PeakHour:
      return mod.startTime && mod.endTime
        ? `${mod.startTime.slice(0, 5)} – ${mod.endTime.slice(0, 5)}`
        : '—'
    case ModifierType.DayOfWeek:
      return mod.activeDayOfWeek != null ? DAY_NAMES[mod.activeDayOfWeek] ?? '—' : '—'
    case ModifierType.Holiday:
      return mod.holidayName
        ? `${mod.holidayName} (${mod.holidayDate ?? ''})`
        : mod.holidayDate ?? '—'
    case ModifierType.Promotion:
      return mod.startDate && mod.endDate
        ? `${mod.startDate} → ${mod.endDate}`
        : mod.startDate ?? '—'
    case ModifierType.Weather:
      return 'Manual activation'
    default:
      return '—'
  }
}

// ── Form schema ──────────────────────────────────────────────────────────────

const modifierSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  type: z.coerce.number().min(1, 'Select a type'),
  value: z.coerce.number().positive('Must be positive'),
  branchId: z.string().optional(),
  startTime: z.string().optional(),
  endTime: z.string().optional(),
  activeDayOfWeek: z.coerce.number().min(0).max(6).optional(),
  holidayDate: z.string().optional(),
  holidayName: z.string().optional(),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
})
type FormValues = z.infer<typeof modifierSchema>

// ── Create / Edit Dialog ─────────────────────────────────────────────────────

function ModifierFormDialog({
  open,
  onOpenChange,
  modifier,
}: {
  open: boolean
  onOpenChange: (o: boolean) => void
  modifier?: PricingModifier
}) {
  const isEdit = !!modifier
  const { mutateAsync: create } = useCreatePricingModifier()
  const { mutateAsync: update } = useUpdatePricingModifier(modifier?.id ?? '')
  const { data: branches } = useBranches()

  const defaults: FormValues = modifier
    ? {
        name: modifier.name,
        type: modifier.type,
        value: modifier.value,
        branchId: modifier.branchId ?? undefined,
        startTime: modifier.startTime?.slice(0, 5) ?? undefined,
        endTime: modifier.endTime?.slice(0, 5) ?? undefined,
        activeDayOfWeek: modifier.activeDayOfWeek ?? undefined,
        holidayDate: modifier.holidayDate ?? undefined,
        holidayName: modifier.holidayName ?? undefined,
        startDate: modifier.startDate ?? undefined,
        endDate: modifier.endDate ?? undefined,
      }
    : {
        name: '',
        type: ModifierType.PeakHour,
        value: 1.2,
      }

  const { register, handleSubmit, watch, control, reset, formState } = useForm<FormValues>({
    resolver: zodResolver(modifierSchema),
    defaultValues: defaults,
  })

  const selectedType = watch('type')

  const onSubmit = async (values: FormValues) => {
    try {
      const payload: PricingModifierFormValues = {
        name: values.name,
        type: values.type as ModifierType,
        value: values.value,
        branchId: values.branchId || undefined,
        startTime: values.startTime || undefined,
        endTime: values.endTime || undefined,
        activeDayOfWeek: selectedType === ModifierType.DayOfWeek ? values.activeDayOfWeek : undefined,
        holidayDate: values.holidayDate || undefined,
        holidayName: values.holidayName || undefined,
        startDate: values.startDate || undefined,
        endDate: values.endDate || undefined,
      }
      if (isEdit) {
        await update(payload)
        toast.success('Modifier updated')
      } else {
        await create(payload)
        toast.success('Modifier created')
      }
      reset()
      onOpenChange(false)
    } catch {
      toast.error(isEdit ? 'Failed to update modifier' : 'Failed to create modifier')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md overflow-y-auto max-h-[90vh]">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Modifier' : 'New Pricing Modifier'}</DialogTitle>
          <DialogDescription>
            {isEdit
              ? 'Update the pricing modifier settings.'
              : 'Create a rule that adjusts service prices based on conditions.'}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          {/* Name */}
          <div className="space-y-1.5">
            <Label htmlFor="name">Name</Label>
            <Input id="name" placeholder="e.g. Weekend Surcharge" {...register('name')} />
            {formState.errors.name && (
              <p className="text-xs text-destructive">{formState.errors.name.message}</p>
            )}
          </div>

          {/* Type */}
          <div className="space-y-1.5">
            <Label htmlFor="type">Type</Label>
            <Controller
              control={control}
              name="type"
              render={({ field }) => (
                <Select
                  value={String(field.value)}
                  onValueChange={(v) => field.onChange(Number(v))}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Select type" />
                  </SelectTrigger>
                  <SelectContent>
                    {MODIFIER_TYPE_OPTIONS.map((opt) => (
                      <SelectItem key={opt.value} value={String(opt.value)}>
                        {opt.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {formState.errors.type && (
              <p className="text-xs text-destructive">{formState.errors.type.message}</p>
            )}
          </div>

          {/* Value */}
          <div className="space-y-1.5">
            <Label htmlFor="value">
              {selectedType === ModifierType.Promotion ? 'Discount Amount (₱)' : 'Multiplier'}
            </Label>
            <Input
              id="value"
              type="number"
              step={selectedType === ModifierType.Promotion ? '0.01' : '0.01'}
              placeholder={selectedType === ModifierType.Promotion ? '50.00' : '1.20'}
              {...register('value')}
            />
            <p className="text-xs text-muted-foreground">
              {selectedType === ModifierType.Promotion
                ? 'Peso amount deducted from the service price.'
                : 'e.g. 1.20 = +20% surcharge, 0.80 = -20% discount.'}
            </p>
            {formState.errors.value && (
              <p className="text-xs text-destructive">{formState.errors.value.message}</p>
            )}
          </div>

          {/* Branch (optional) */}
          <div className="space-y-1.5">
            <Label>Branch (optional)</Label>
            <Controller
              control={control}
              name="branchId"
              render={({ field }) => (
                <Select value={field.value ?? '__all__'} onValueChange={(v) => field.onChange(v === '__all__' ? undefined : v)}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="All branches" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="__all__">All branches</SelectItem>
                    {branches?.map((b) => (
                      <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
          </div>

          {/* Conditional fields based on type */}
          {Number(selectedType) === ModifierType.PeakHour && (
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1.5">
                <Label htmlFor="startTime">Start Time</Label>
                <Input id="startTime" type="time" {...register('startTime')} />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="endTime">End Time</Label>
                <Input id="endTime" type="time" {...register('endTime')} />
              </div>
            </div>
          )}

          {Number(selectedType) === ModifierType.DayOfWeek && (
            <div className="space-y-1.5">
              <Label>Day of Week</Label>
              <Controller
                control={control}
                name="activeDayOfWeek"
                render={({ field }) => (
                  <Select
                    value={field.value != null ? String(field.value) : ''}
                    onValueChange={(v) => field.onChange(Number(v))}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Select day" />
                    </SelectTrigger>
                    <SelectContent>
                      {DAY_NAMES.map((day, i) => (
                        <SelectItem key={i} value={String(i)}>{day}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
          )}

          {Number(selectedType) === ModifierType.Holiday && (
            <>
              <div className="space-y-1.5">
                <Label htmlFor="holidayName">Holiday Name</Label>
                <Input id="holidayName" placeholder="e.g. Christmas Day" {...register('holidayName')} />
              </div>
              <div className="space-y-1.5">
                <Label>Date</Label>
                <Controller
                  control={control}
                  name="holidayDate"
                  render={({ field }) => (
                    <DatePicker
                      value={field.value ?? ''}
                      onChange={field.onChange}
                      placeholder="Select holiday date"
                      className="w-full"
                    />
                  )}
                />
              </div>
            </>
          )}

          {Number(selectedType) === ModifierType.Promotion && (
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1.5">
                <Label>Start Date</Label>
                <Controller
                  control={control}
                  name="startDate"
                  render={({ field }) => (
                    <DatePicker
                      value={field.value ?? ''}
                      onChange={field.onChange}
                      placeholder="Start"
                      className="w-full"
                    />
                  )}
                />
              </div>
              <div className="space-y-1.5">
                <Label>End Date</Label>
                <Controller
                  control={control}
                  name="endDate"
                  render={({ field }) => (
                    <DatePicker
                      value={field.value ?? ''}
                      onChange={field.onChange}
                      placeholder="End"
                      className="w-full"
                    />
                  )}
                />
              </div>
            </div>
          )}

          {/* Submit */}
          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={formState.isSubmitting}>
              {formState.isSubmitting ? 'Saving…' : isEdit ? 'Save Changes' : 'Create Modifier'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// ── Delete Confirmation Dialog ───────────────────────────────────────────────

function DeleteModifierDialog({
  open,
  onOpenChange,
  modifier,
}: {
  open: boolean
  onOpenChange: (o: boolean) => void
  modifier: PricingModifier | null
}) {
  const { mutate: deleteMod, isPending } = useDeletePricingModifier()

  const handleDelete = () => {
    if (!modifier) return
    deleteMod(modifier.id, {
      onSuccess: () => {
        onOpenChange(false)
        toast.success('Modifier deleted')
      },
      onError: () => toast.error('Failed to delete modifier'),
    })
  }

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete modifier?</AlertDialogTitle>
          <AlertDialogDescription>
            This will permanently delete <strong>{modifier?.name}</strong>. This action cannot be
            undone.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction onClick={handleDelete} disabled={isPending} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
            {isPending ? 'Deleting…' : 'Delete'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

// ── Modifier Card ────────────────────────────────────────────────────────────

function ModifierRow({
  mod,
  onEdit,
  onDelete,
  onToggle,
}: {
  mod: PricingModifier
  onEdit: () => void
  onDelete: () => void
  onToggle: () => void
}) {
  const Icon = typeIcon(mod.type)

  return (
    <tr className="hover:bg-muted/40 transition-colors">
      <td className="px-4 py-3">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-muted">
            <Icon className="h-4 w-4 text-muted-foreground" />
          </div>
          <div>
            <p className="font-medium">{mod.name}</p>
            <p className="text-xs text-muted-foreground">{typeLabel(mod.type)}</p>
          </div>
        </div>
      </td>
      <td className="px-4 py-3">
        <Badge variant="outline" className="font-mono tabular-nums">
          {formatValue(mod.type, mod.value)}
        </Badge>
      </td>
      <td className="px-4 py-3 text-sm text-muted-foreground">
        {formatCondition(mod)}
      </td>
      <td className="px-4 py-3 text-sm text-muted-foreground">
        {mod.branchName ?? 'All branches'}
      </td>
      <td className="px-4 py-3">
        <StatusBadge status={mod.isActive ? 'Active' : 'Inactive'} />
      </td>
      <td className="px-4 py-3 text-right">
        <DropdownMenu>
          <DropdownMenuTrigger render={<Button variant="ghost" size="icon" className="h-8 w-8" />}>
            <MoreHorizontal className="h-4 w-4" />
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={onEdit}>
              <Pencil className="mr-2 h-4 w-4" />
              Edit
            </DropdownMenuItem>
            <DropdownMenuItem onClick={onToggle}>
              {mod.isActive ? (
                <>
                  <PowerOff className="mr-2 h-4 w-4" />
                  Deactivate
                </>
              ) : (
                <>
                  <Power className="mr-2 h-4 w-4" />
                  Activate
                </>
              )}
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem variant="destructive" onClick={onDelete}>
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </td>
    </tr>
  )
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function PricingModifiersPage() {
  const [branchFilter, setBranchFilter] = useState<string | undefined>(undefined)
  const [search, setSearch] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [editingMod, setEditingMod] = useState<PricingModifier | null>(null)
  const [deletingMod, setDeletingMod] = useState<PricingModifier | null>(null)

  const { data: modifiers, isLoading, isError } = usePricingModifiers(branchFilter)
  const { data: branches } = useBranches()
  const { mutate: toggleStatus } = useTogglePricingModifier()

  const filtered = (modifiers ?? []).filter((m) => {
    if (!search) return true
    return m.name.toLowerCase().includes(search.toLowerCase())
  })

  const handleToggle = (mod: PricingModifier) => {
    toggleStatus(mod.id, {
      onSuccess: () => toast.success(`${mod.name} ${mod.isActive ? 'deactivated' : 'activated'}`),
      onError: () => toast.error('Failed to update status'),
    })
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Pricing Modifiers</h1>
          <p className="text-muted-foreground">
            Configure peak/off-peak pricing rules and promotions
          </p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          New Modifier
        </Button>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3">
        <div className="relative max-w-sm flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            className="pl-9"
            placeholder="Search modifiers…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <Select
          value={branchFilter ?? '__all__'}
          onValueChange={(v) => setBranchFilter(v === '__all__' ? undefined : v)}
        >
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="All branches" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="__all__">All branches</SelectItem>
            {branches?.map((b) => (
              <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Content */}
      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-14 w-full" />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
          Failed to load pricing modifiers.
        </div>
      )}

      {!isLoading && !isError && filtered.length === 0 && (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed p-16 text-center gap-3">
          <Percent className="h-10 w-10 text-muted-foreground/40" />
          <div>
            <p className="font-medium">
              {search ? 'No modifiers match your search' : 'No pricing modifiers yet'}
            </p>
            <p className="text-sm text-muted-foreground">
              {search
                ? 'Try a different search term.'
                : 'Create your first pricing rule to adjust service prices.'}
            </p>
          </div>
          {!search && (
            <Button variant="outline" onClick={() => setCreateOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              New Modifier
            </Button>
          )}
        </div>
      )}

      {!isLoading && !isError && filtered.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Modifier</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Value</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Condition</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Branch</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Status</th>
                <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">
                  <span className="sr-only">Actions</span>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {filtered.map((mod) => (
                <ModifierRow
                  key={mod.id}
                  mod={mod}
                  onEdit={() => setEditingMod(mod)}
                  onDelete={() => setDeletingMod(mod)}
                  onToggle={() => handleToggle(mod)}
                />
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Create dialog */}
      <ModifierFormDialog open={createOpen} onOpenChange={setCreateOpen} />

      {/* Edit dialog */}
      {editingMod && (
        <ModifierFormDialog
          key={editingMod.id}
          open={!!editingMod}
          onOpenChange={(o) => { if (!o) setEditingMod(null) }}
          modifier={editingMod}
        />
      )}

      {/* Delete confirmation */}
      <DeleteModifierDialog
        open={!!deletingMod}
        onOpenChange={(o) => { if (!o) setDeletingMod(null) }}
        modifier={deletingMod}
      />
    </div>
  )
}
