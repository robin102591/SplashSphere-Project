'use client'

import { use, useState } from 'react'
import { Pencil, Power, PowerOff, AlertTriangle, Plus, Minus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { StatusBadge } from '@/components/ui/status-badge'
import { PageHeader } from '@/components/ui/page-header'
import { Skeleton } from '@/components/ui/skeleton'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter,
} from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'
import {
  useMerchandiseItem, useUpdateMerchandise, useToggleMerchandiseStatus, useAdjustStock,
} from '@/hooks/use-merchandise'
import type { UpdateMerchandiseValues } from '@/hooks/use-merchandise'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { formatPeso } from '@/lib/format'

// ── Edit form ─────────────────────────────────────────────────────────────────

const editSchema = z.object({
  name: z.string().min(1, 'Required'),
  price: z.coerce.number().positive('Must be positive'),
  lowStockThreshold: z.coerce.number().int().min(0),
  description: z.string().optional(),
  costPrice: z.coerce.number().positive().optional().or(z.literal('')),
})
type EditFormValues = z.infer<typeof editSchema>

function EditMerchandiseForm({
  item,
  onSubmit,
}: {
  item: { name: string; price: number; lowStockThreshold: number; description: string | null; costPrice: number | null }
  onSubmit: (v: UpdateMerchandiseValues) => Promise<void>
}) {
  const { register, handleSubmit, formState } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    defaultValues: {
      name: item.name,
      price: item.price,
      lowStockThreshold: item.lowStockThreshold,
      description: item.description ?? '',
      costPrice: item.costPrice ?? '',
    },
  })
  return (
    <form
      onSubmit={handleSubmit((v) =>
        onSubmit({
          name: v.name,
          price: v.price,
          lowStockThreshold: v.lowStockThreshold,
          description: v.description || undefined,
          costPrice: v.costPrice ? Number(v.costPrice) : undefined,
        })
      )}
      className="space-y-4"
    >
      <div className="space-y-1.5">
        <Label>Name <span className="text-destructive">*</span></Label>
        <Input {...register('name')} />
        {formState.errors.name && (
          <p className="text-xs text-destructive">{formState.errors.name.message}</p>
        )}
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1.5">
          <Label>Selling price (₱) <span className="text-destructive">*</span></Label>
          <Input type="number" step="0.01" {...register('price')} />
          {formState.errors.price && (
            <p className="text-xs text-destructive">{formState.errors.price.message}</p>
          )}
        </div>
        <div className="space-y-1.5">
          <Label>Cost price (₱)</Label>
          <Input type="number" step="0.01" {...register('costPrice')} />
        </div>
      </div>
      <div className="space-y-1.5">
        <Label>Low stock alert at <span className="text-destructive">*</span></Label>
        <Input type="number" {...register('lowStockThreshold')} />
      </div>
      <div className="space-y-1.5">
        <Label>Description</Label>
        <Input {...register('description')} />
      </div>
      <div className="flex justify-end pt-1">
        <Button type="submit" disabled={formState.isSubmitting}>
          {formState.isSubmitting ? 'Saving…' : 'Save Changes'}
        </Button>
      </div>
    </form>
  )
}

// ── Stock adjustment dialog ───────────────────────────────────────────────────

const adjustSchema = z.object({
  adjustment: z.coerce.number().int().refine((v) => v !== 0, 'Cannot be zero'),
  reason: z.string().optional(),
})
type AdjustFormValues = z.infer<typeof adjustSchema>

function StockAdjustDialog({
  itemId,
  currentStock,
  open,
  onOpenChange,
}: {
  itemId: string
  currentStock: number
  open: boolean
  onOpenChange: (o: boolean) => void
}) {
  const { mutateAsync: adjustStock } = useAdjustStock(itemId)
  const { register, handleSubmit, watch, reset, formState } = useForm<AdjustFormValues>({
    resolver: zodResolver(adjustSchema),
    defaultValues: { adjustment: 0 },
  })
  const adjustment = watch('adjustment') || 0
  const newStock = currentStock + Number(adjustment)

  const onSubmit = async (values: AdjustFormValues) => {
    try {
      const result = await adjustStock({ adjustment: values.adjustment, reason: values.reason || undefined })
      toast.success(`Stock updated to ${result.stockQuantity}${result.isLowStock ? ' — now below threshold' : ''}`)
      reset()
      onOpenChange(false)
    } catch {
      toast.error('Failed to adjust stock')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Adjust Stock</DialogTitle>
          <DialogDescription>
            Enter a positive number to restock, negative to write off.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="flex items-center justify-center gap-6 py-3">
            <div className="text-center">
              <p className="text-3xl font-bold tabular-nums">{currentStock}</p>
              <p className="text-xs text-muted-foreground">Current</p>
            </div>
            <span className="text-muted-foreground">→</span>
            <div className="text-center">
              <p className={cn('text-3xl font-bold tabular-nums', newStock < 0 && 'text-destructive')}>
                {newStock}
              </p>
              <p className="text-xs text-muted-foreground">New</p>
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Adjustment <span className="text-destructive">*</span></Label>
            <div className="flex items-center gap-2">
              <Input
                type="number"
                placeholder="+10 or -5"
                {...register('adjustment')}
                className="text-center tabular-nums"
              />
            </div>
            {formState.errors.adjustment && (
              <p className="text-xs text-destructive">{formState.errors.adjustment.message}</p>
            )}
          </div>
          <div className="space-y-1.5">
            <Label>Reason (optional)</Label>
            <Input placeholder="Restock delivery, damaged goods…" {...register('reason')} />
          </div>
          <DialogFooter>
            <Button variant="outline" type="button" onClick={() => { reset(); onOpenChange(false) }}>
              Cancel
            </Button>
            <Button type="submit" disabled={formState.isSubmitting || newStock < 0}>
              {formState.isSubmitting ? 'Saving…' : 'Apply'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function MerchandiseDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const [editOpen, setEditOpen] = useState(false)
  const [adjustOpen, setAdjustOpen] = useState(false)

  const { data: item, isLoading, isError } = useMerchandiseItem(id)
  const { mutate: toggleStatus, isPending: isToggling } = useToggleMerchandiseStatus()
  const { mutateAsync: updateItem } = useUpdateMerchandise(id)

  const handleToggle = () => {
    if (!item) return
    toggleStatus(id, {
      onSuccess: () => toast.success(`Item ${item.isActive ? 'deactivated' : 'activated'}`),
      onError: () => toast.error('Failed to update status'),
    })
  }

  const handleUpdate = async (values: UpdateMerchandiseValues) => {
    try {
      await updateItem(values)
      toast.success('Item updated')
      setEditOpen(false)
    } catch {
      toast.error('Failed to update item')
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 w-full" />
      </div>
    )
  }

  if (isError || !item) {
    return (
      <div className="space-y-4">
        <PageHeader title="Error" back />
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Item not found or failed to load.
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={item.name}
        description={item.sku}
        back
        badge={
          <>
            {item.isLowStock && <StatusBadge status="Low Stock" />}
            <StatusBadge status={item.isActive ? 'Active' : 'Inactive'} />
          </>
        }
        actions={
          <>
            <Button variant="outline" size="sm" onClick={() => setAdjustOpen(true)}>
              <Plus className="mr-1.5 h-3.5 w-3.5" />/<Minus className="mr-2 h-3.5 w-3.5" />
              Stock
            </Button>
            <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
              <Pencil className="mr-2 h-3.5 w-3.5" />Edit
            </Button>
            <Button variant="outline" size="sm" onClick={handleToggle} disabled={isToggling}>
              {item.isActive ? (
                <><PowerOff className="mr-2 h-3.5 w-3.5" />Deactivate</>
              ) : (
                <><Power className="mr-2 h-3.5 w-3.5" />Activate</>
              )}
            </Button>
          </>
        }
      />

      {/* Stock card */}
      <div className={cn(
        'rounded-lg border px-5 py-4 flex items-center gap-8',
        item.isLowStock ? 'border-amber-200 bg-amber-50/60' : ''
      )}>
        <div>
          <p className="text-xs text-muted-foreground">Current stock</p>
          <p className={cn('text-4xl font-bold tabular-nums mt-0.5', item.isLowStock && 'text-amber-700')}>
            {item.stockQuantity}
          </p>
        </div>
        <div>
          <p className="text-xs text-muted-foreground">Low stock threshold</p>
          <p className="text-xl font-semibold tabular-nums mt-0.5">{item.lowStockThreshold}</p>
        </div>
        {item.isLowStock && (
          <div className="ml-auto flex items-center gap-2 text-amber-700 text-sm">
            <AlertTriangle className="h-4 w-4" />
            Restock soon
          </div>
        )}
      </div>

      {/* Details */}
      <div className="max-w-md rounded-lg border p-5">
        <dl className="grid grid-cols-2 gap-x-8 gap-y-4 text-sm">
          <div>
            <dt className="text-muted-foreground">Selling price</dt>
            <dd className="mt-0.5 font-semibold text-base">{formatPeso(item.price)}</dd>
          </div>
          {item.costPrice != null && (
            <div>
              <dt className="text-muted-foreground">Cost price</dt>
              <dd className="mt-0.5 font-medium">{formatPeso(item.costPrice)}</dd>
            </div>
          )}
          {item.categoryName && (
            <div>
              <dt className="text-muted-foreground">Category</dt>
              <dd className="mt-0.5 font-medium">{item.categoryName}</dd>
            </div>
          )}
          {item.description && (
            <div className="col-span-2">
              <dt className="text-muted-foreground">Description</dt>
              <dd className="mt-0.5">{item.description}</dd>
            </div>
          )}
          <div>
            <dt className="text-muted-foreground">Created</dt>
            <dd className="mt-0.5">{new Date(item.createdAt).toLocaleDateString('en-PH', { year: 'numeric', month: 'short', day: 'numeric' })}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">Last updated</dt>
            <dd className="mt-0.5">{new Date(item.updatedAt).toLocaleDateString('en-PH', { year: 'numeric', month: 'short', day: 'numeric' })}</dd>
          </div>
        </dl>
      </div>

      <Sheet open={editOpen} onOpenChange={setEditOpen}>
        <SheetContent className="sm:max-w-md overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Edit Item</SheetTitle>
            <SheetDescription>SKU cannot be changed. Use stock adjustment to change quantity.</SheetDescription>
          </SheetHeader>
          <div className="mt-6">
            <EditMerchandiseForm item={item} onSubmit={handleUpdate} />
          </div>
        </SheetContent>
      </Sheet>

      <StockAdjustDialog
        itemId={id}
        currentStock={item.stockQuantity}
        open={adjustOpen}
        onOpenChange={setAdjustOpen}
      />
    </div>
  )
}
