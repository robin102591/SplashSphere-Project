'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Plus, Package, AlertTriangle, Search } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/ui/empty-state'
import { Input } from '@/components/ui/input'
import { StatusBadge } from '@/components/ui/status-badge'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'
import { useMerchandiseList, useCreateMerchandise } from '@/hooks/use-merchandise'
import type { CreateMerchandiseValues } from '@/hooks/use-merchandise'
import {
  useMerchandiseCategories,
  useCreateMerchandiseCategory,
  useToggleMerchandiseCategoryStatus,
} from '@/hooks/use-merchandise-categories'
import type { CreateCategoryValues } from '@/hooks/use-merchandise-categories'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription,
} from '@/components/ui/sheet'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { formatPeso } from '@/lib/format'

// ── Create dialog ─────────────────────────────────────────────────────────────

const createSchema = z.object({
  name: z.string().min(1, 'Required'),
  sku: z.string().min(1, 'SKU is required'),
  price: z.coerce.number().positive('Must be positive'),
  stockQuantity: z.coerce.number().int().min(0, 'Must be 0 or more'),
  lowStockThreshold: z.coerce.number().int().min(0, 'Must be 0 or more'),
  categoryId: z.string().optional(),
  description: z.string().optional(),
  costPrice: z.coerce.number().positive('Must be positive').optional().or(z.literal('')),
})
type CreateFormValues = z.infer<typeof createSchema>

function CreateMerchandiseDialog({
  open,
  onOpenChange,
  categories,
}: {
  open: boolean
  onOpenChange: (o: boolean) => void
  categories: { id: string; name: string }[]
}) {
  const router = useRouter()
  const { mutateAsync: create } = useCreateMerchandise()
  const { register, handleSubmit, reset, formState, setValue, watch } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: { stockQuantity: 0, lowStockThreshold: 5 },
  })

  const onSubmit = async (values: CreateFormValues) => {
    try {
      const payload: CreateMerchandiseValues = {
        name: values.name,
        sku: values.sku,
        price: values.price,
        stockQuantity: values.stockQuantity,
        lowStockThreshold: values.lowStockThreshold,
        categoryId: values.categoryId || undefined,
        description: values.description || undefined,
        costPrice: values.costPrice ? Number(values.costPrice) : undefined,
      }
      const { id } = await create(payload)
      toast.success('Item created')
      reset()
      onOpenChange(false)
      router.push(`/dashboard/merchandise/${id}`)
    } catch {
      toast.error('Failed to create item')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md overflow-y-auto max-h-[90vh]">
        <DialogHeader>
          <DialogTitle>New Merchandise</DialogTitle>
          <DialogDescription>Add a new item to the inventory.</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label>Name</Label>
            <Input placeholder="Microfiber Cloth" {...register('name')} />
            {formState.errors.name && (
              <p className="text-xs text-destructive">{formState.errors.name.message}</p>
            )}
          </div>
          <div className="space-y-1.5">
            <Label>SKU</Label>
            <Input placeholder="MFC-001" {...register('sku')} />
            {formState.errors.sku && (
              <p className="text-xs text-destructive">{formState.errors.sku.message}</p>
            )}
          </div>
          {categories.length > 0 && (
            <div className="space-y-1.5">
              <Label>Category</Label>
              <Select
                value={watch('categoryId') || '__none__'}
                onValueChange={(v) => setValue('categoryId', v === '__none__' ? '' : v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="No category" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="__none__">No category</SelectItem>
                  {categories.map((c) => (
                    <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Selling price (₱)</Label>
              <Input type="number" step="0.01" placeholder="250.00" {...register('price')} />
              {formState.errors.price && (
                <p className="text-xs text-destructive">{formState.errors.price.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label>Cost price (₱, optional)</Label>
              <Input type="number" step="0.01" placeholder="150.00" {...register('costPrice')} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Opening stock</Label>
              <Input type="number" {...register('stockQuantity')} />
              {formState.errors.stockQuantity && (
                <p className="text-xs text-destructive">{formState.errors.stockQuantity.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label>Low stock alert at</Label>
              <Input type="number" {...register('lowStockThreshold')} />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Description (optional)</Label>
            <Input placeholder="Brief description…" {...register('description')} />
          </div>
          <div className="flex justify-end pt-1">
            <Button type="submit" disabled={formState.isSubmitting}>
              {formState.isSubmitting ? 'Saving…' : 'Create Item'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

// ── Manage Categories Sheet ──────────────────────────────────────────────────

const categorySchema = z.object({
  name: z.string().min(1, 'Required'),
  description: z.string().optional(),
})
type CategoryFormValues = z.infer<typeof categorySchema>

function ManageCategoriesSheet({
  open,
  onOpenChange,
}: {
  open: boolean
  onOpenChange: (o: boolean) => void
}) {
  const { data: catData } = useMerchandiseCategories()
  const categories = catData?.items ?? []
  const { mutateAsync: createCategory } = useCreateMerchandiseCategory()
  const { mutate: toggleStatus } = useToggleMerchandiseCategoryStatus()
  const { register, handleSubmit, reset, formState } = useForm<CategoryFormValues>({
    resolver: zodResolver(categorySchema),
  })

  const onSubmit = async (values: CategoryFormValues) => {
    try {
      const data: CreateCategoryValues = {
        name: values.name,
        description: values.description || undefined,
      }
      await createCategory(data)
      toast.success('Category created')
      reset()
    } catch {
      toast.error('Failed to create category')
    }
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="sm:max-w-md overflow-y-auto">
        <SheetHeader>
          <SheetTitle>Merchandise Categories</SheetTitle>
          <SheetDescription>Create and manage item categories.</SheetDescription>
        </SheetHeader>
        <div className="space-y-6">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-3">
            <div className="space-y-1.5">
              <Label>Name</Label>
              <Input placeholder="Car Care Products" {...register('name')} />
              {formState.errors.name && (
                <p className="text-xs text-destructive">{formState.errors.name.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label>Description (optional)</Label>
              <Input placeholder="Brief description…" {...register('description')} />
            </div>
            <Button type="submit" size="sm" disabled={formState.isSubmitting}>
              {formState.isSubmitting ? 'Creating…' : 'Add Category'}
            </Button>
          </form>

          {categories.length > 0 && (
            <div className="space-y-1">
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Existing Categories</p>
              <div className="divide-y rounded-lg border">
                {categories.map((cat) => (
                  <div key={cat.id} className="flex items-center justify-between px-3 py-2.5">
                    <div>
                      <p className={cn('text-sm font-medium', !cat.isActive && 'text-muted-foreground line-through')}>{cat.name}</p>
                      {cat.description && <p className="text-xs text-muted-foreground">{cat.description}</p>}
                    </div>
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-xs"
                      onClick={() =>
                        toggleStatus(cat.id, {
                          onSuccess: () => toast.success(`Category ${cat.isActive ? 'deactivated' : 'activated'}`),
                          onError: () => toast.error('Failed to update status'),
                        })
                      }
                    >
                      {cat.isActive ? 'Deactivate' : 'Activate'}
                    </Button>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </SheetContent>
    </Sheet>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function MerchandisePage() {
  const router = useRouter()
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [lowStockOnly, setLowStockOnly] = useState(false)
  const [categoryFilter, setCategoryFilter] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [categoriesOpen, setCategoriesOpen] = useState(false)

  const { data: catData } = useMerchandiseCategories()
  const activeCategories = (catData?.items ?? []).filter((c) => c.isActive)

  const { data, isLoading, isError } = useMerchandiseList({
    search: debouncedSearch,
    categoryId: categoryFilter || undefined,
    lowStockOnly: lowStockOnly || undefined,
    pageSize: 100,
  })
  const items = data ? [...data.items] : []
  const lowStockCount = items.filter((i) => i.isLowStock).length

  const handleSearchChange = (value: string) => {
    setSearch(value)
    clearTimeout((handleSearchChange as { _t?: ReturnType<typeof setTimeout> })._t)
    ;(handleSearchChange as { _t?: ReturnType<typeof setTimeout> })._t = setTimeout(
      () => setDebouncedSearch(value),
      300
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Merchandise</h1>
          <p className="text-muted-foreground">Manage inventory and stock levels</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => setCategoriesOpen(true)}>
            Categories
          </Button>
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            New Item
          </Button>
        </div>
      </div>

      {/* Low-stock alert banner */}
      {!isLoading && lowStockCount > 0 && !lowStockOnly && (
        <button
          onClick={() => setLowStockOnly(true)}
          className="w-full flex items-center gap-3 rounded-lg border border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-950/30 px-4 py-3 text-sm text-amber-800 dark:text-amber-300 text-left hover:bg-amber-100 dark:hover:bg-amber-950/50 transition-colors"
        >
          <AlertTriangle className="h-4 w-4 shrink-0" />
          <span>
            <strong>{lowStockCount} item{lowStockCount !== 1 ? 's' : ''}</strong> below low-stock
            threshold — click to filter
          </span>
        </button>
      )}

      <div className="flex items-center gap-3 flex-wrap">
        <div className="relative max-w-sm flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            className="pl-9"
            placeholder="Search by name or SKU…"
            value={search}
            onChange={(e) => handleSearchChange(e.target.value)}
          />
        </div>
        {activeCategories.length > 0 && (
          <Select value={categoryFilter || '__all__'} onValueChange={(v) => setCategoryFilter(v === '__all__' ? '' : v)}>
            <SelectTrigger className="w-44">
              <SelectValue placeholder="All categories" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="__all__">All categories</SelectItem>
              {activeCategories.map((c) => (
                <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
        <Button
          variant={lowStockOnly ? 'default' : 'outline'}
          size="sm"
          onClick={() => setLowStockOnly(!lowStockOnly)}
        >
          <AlertTriangle className="mr-2 h-3.5 w-3.5" />
          Low stock only
        </Button>
      </div>

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
          Failed to load merchandise.
        </div>
      )}

      {!isLoading && !isError && items.length === 0 && (
        <EmptyState
          icon={Package}
          title="No items found"
          description={lowStockOnly ? 'No low-stock items' : 'Add your first merchandise item'}
        />
      )}

      {!isLoading && items.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Item</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">SKU</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Category</th>
                <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">Price</th>
                <th className="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider text-muted-foreground">Stock</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {items.map((item) => (
                <tr
                  key={item.id}
                  className={cn(
                    'hover:bg-muted/40 cursor-pointer transition-colors',
                    item.isLowStock && 'bg-amber-50/60 dark:bg-amber-950/20 hover:bg-amber-50 dark:hover:bg-amber-950/30'
                  )}
                  onClick={() => router.push(`/dashboard/merchandise/${item.id}`)}
                >
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      {item.isLowStock && (
                        <AlertTriangle className="h-3.5 w-3.5 text-amber-500 shrink-0" />
                      )}
                      <span className="font-medium">{item.name}</span>
                    </div>
                    {item.description && (
                      <p className="text-xs text-muted-foreground mt-0.5 truncate max-w-xs">
                        {item.description}
                      </p>
                    )}
                  </td>
                  <td className="px-4 py-3 font-mono text-xs text-muted-foreground">{item.sku}</td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">{item.categoryName ?? '—'}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{formatPeso(item.price)}</td>
                  <td className="px-4 py-3 text-center">
                    <span
                      className={cn(
                        'tabular-nums font-medium',
                        item.isLowStock && 'text-amber-600 dark:text-amber-400'
                      )}
                    >
                      {item.stockQuantity}
                    </span>
                    <span className="text-muted-foreground text-xs"> / {item.lowStockThreshold}</span>
                  </td>
                  <td className="px-4 py-3">
                    <StatusBadge status={item.isLowStock ? 'Low Stock' : item.isActive ? 'Active' : 'Inactive'} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <CreateMerchandiseDialog open={createOpen} onOpenChange={setCreateOpen} categories={activeCategories} />
      <ManageCategoriesSheet open={categoriesOpen} onOpenChange={setCategoriesOpen} />
    </div>
  )
}
