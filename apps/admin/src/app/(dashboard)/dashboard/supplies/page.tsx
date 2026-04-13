'use client'

import { useState } from 'react'
import { Plus, Package, Minus, PackagePlus, Tags } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import { EmptyState } from '@/components/ui/empty-state'
import { StatusBadge } from '@/components/ui/status-badge'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog'
import {
  useSupplies, useSupplyCategories, useCreateSupply, useCreateSupplyCategory, useRecordStockMovement,
} from '@/hooks/use-inventory'
import { useBranches } from '@/hooks/use-branches'
import { formatPeso } from '@/lib/format'
import { toast } from 'sonner'
import Link from 'next/link'

// ── Helpers ──────────────────────────────────────────────────────────────────

function stockColor(current: number, reorder: number): string {
  if (reorder <= 0) return 'text-emerald-600 dark:text-emerald-400'
  const ratio = current / reorder
  if (ratio <= 0.2) return 'text-red-600 dark:text-red-400'
  if (ratio <= 0.5) return 'text-amber-600 dark:text-amber-400'
  return 'text-emerald-600 dark:text-emerald-400'
}

function stockStatus(current: number, reorder: number): string {
  if (current <= 0) return 'Out of Stock'
  if (current <= reorder) return 'Low Stock'
  return 'In Stock'
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function SuppliesPage() {
  const [categoryId, setCategoryId] = useState('')
  const [branchId, setBranchId] = useState('')
  const [stockFilter, setStockFilter] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [categoriesOpen, setCategoriesOpen] = useState(false)
  const [usageSupply, setUsageSupply] = useState<{ id: string; name: string } | null>(null)
  const [restockSupply, setRestockSupply] = useState<{ id: string; name: string } | null>(null)

  const { data: branches } = useBranches()
  const { data: categories } = useSupplyCategories()
  const { data, isLoading } = useSupplies({
    categoryId: categoryId || undefined,
    branchId: branchId || undefined,
    stockStatus: (stockFilter || undefined) as 'low' | 'out' | 'ok' | undefined,
    pageSize: 100,
  })

  const supplies = data?.items ?? []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Supplies</h1>
          <p className="text-sm text-muted-foreground">Manage consumable supplies and stock levels</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => setCategoriesOpen(true)}>
            <Tags className="mr-2 h-4 w-4" /> Categories
          </Button>
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="mr-2 h-4 w-4" /> Add Supply
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <Select value={categoryId || 'all'} onValueChange={(v) => setCategoryId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Categories" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Categories</SelectItem>
            {categories?.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
          </SelectContent>
        </Select>
        <Select value={branchId || 'all'} onValueChange={(v) => setBranchId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Branches" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Branches</SelectItem>
            {branches?.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
          </SelectContent>
        </Select>
        <Select value={stockFilter || 'all'} onValueChange={(v) => setStockFilter(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Stock Levels" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Stock Levels</SelectItem>
            <SelectItem value="low">Low Stock</SelectItem>
            <SelectItem value="out">Out of Stock</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      {isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : supplies.length > 0 ? (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Name</th>
                <th className="px-4 py-2.5 text-left font-medium">Category</th>
                <th className="px-4 py-2.5 text-left font-medium">Branch</th>
                <th className="px-4 py-2.5 text-right font-medium">Stock</th>
                <th className="px-4 py-2.5 text-left font-medium">Unit</th>
                <th className="px-4 py-2.5 text-right font-medium">Reorder Level</th>
                <th className="px-4 py-2.5 text-right font-medium">Unit Cost</th>
                <th className="px-4 py-2.5 text-left font-medium">Status</th>
                <th className="px-4 py-2.5 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {supplies.map((s) => {
                const reorder = s.reorderLevel ?? 0
                const status = stockStatus(s.currentStock, reorder)
                return (
                  <tr key={s.id} className="hover:bg-muted/30">
                    <td className="px-4 py-2 font-medium">
                      <Link href={`/dashboard/supplies/${s.id}`} className="hover:underline">
                        {s.name}
                      </Link>
                    </td>
                    <td className="px-4 py-2 text-muted-foreground">{s.categoryName}</td>
                    <td className="px-4 py-2 text-muted-foreground">{s.branchName}</td>
                    <td className="px-4 py-2 text-right">
                      <span className={`font-mono tabular-nums font-medium ${stockColor(s.currentStock, reorder)}`}>
                        {s.currentStock}
                      </span>
                      <span className="text-muted-foreground"> / {reorder}</span>
                    </td>
                    <td className="px-4 py-2 text-muted-foreground">{s.unit}</td>
                    <td className="px-4 py-2 text-right tabular-nums">{reorder}</td>
                    <td className="px-4 py-2 text-right font-mono tabular-nums">{formatPeso(s.averageUnitCost)}</td>
                    <td className="px-4 py-2">
                      <StatusBadge status={status === 'In Stock' ? 'Active' : status === 'Low Stock' ? 'Low Stock' : 'Inactive'} label={status} />
                    </td>
                    <td className="px-4 py-2 text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button variant="ghost" size="sm" className="h-7 text-xs"
                          onClick={() => setUsageSupply({ id: s.id, name: s.name })}>
                          <Minus className="mr-1 h-3 w-3" /> Use
                        </Button>
                        <Button variant="ghost" size="sm" className="h-7 text-xs"
                          onClick={() => setRestockSupply({ id: s.id, name: s.name })}>
                          <PackagePlus className="mr-1 h-3 w-3" /> Restock
                        </Button>
                      </div>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      ) : (
        <EmptyState
          icon={Package}
          title="No supplies found"
          description="Add your first supply item to start tracking inventory."
          action={{ label: 'Add Supply', onClick: () => setCreateOpen(true), icon: Plus }}
        />
      )}

      <CreateSupplyDialog open={createOpen} onOpenChange={setCreateOpen} />
      <ManageCategoriesDialog open={categoriesOpen} onOpenChange={setCategoriesOpen} />
      <RecordUsageDialog supply={usageSupply} onOpenChange={(v) => { if (!v) setUsageSupply(null) }} />
      <RestockDialog supply={restockSupply} onOpenChange={(v) => { if (!v) setRestockSupply(null) }} />
    </div>
  )
}

// ── Create Supply Dialog ─────────────────────────────────────────────────────

function CreateSupplyDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (v: boolean) => void }) {
  const { data: branches } = useBranches()
  const { data: categories } = useSupplyCategories()
  const { mutate: create, isPending } = useCreateSupply()

  const [name, setName] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [branchId, setBranchId] = useState('')
  const [unit, setUnit] = useState('')
  const [reorderLevel, setReorderLevel] = useState('')
  const [unitCost, setUnitCost] = useState('')
  const [initialStock, setInitialStock] = useState('')

  const resetForm = () => {
    setName(''); setCategoryId(''); setBranchId(''); setUnit('')
    setReorderLevel(''); setUnitCost(''); setInitialStock('')
  }

  const handleSubmit = () => {
    if (!name || !categoryId || !branchId || !unit) return
    create({
      name,
      categoryId,
      branchId,
      unit,
      reorderLevel: parseInt(reorderLevel) || 0,
    }, {
      onSuccess: () => {
        toast.success(`Supply "${name}" created`)
        onOpenChange(false)
        resetForm()
      },
      onError: () => toast.error('Failed to create supply.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add Supply</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Name <span className="text-destructive">*</span></Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g. Car Shampoo" autoFocus />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Category <span className="text-destructive">*</span></Label>
              <Select value={categoryId} onValueChange={setCategoryId}>
                <SelectTrigger><SelectValue placeholder="Select" /></SelectTrigger>
                <SelectContent>
                  {categories?.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Branch <span className="text-destructive">*</span></Label>
              <Select value={branchId} onValueChange={setBranchId}>
                <SelectTrigger><SelectValue placeholder="Select" /></SelectTrigger>
                <SelectContent>
                  {branches?.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="grid grid-cols-3 gap-3">
            <div className="space-y-1.5">
              <Label>Unit <span className="text-destructive">*</span></Label>
              <Input value={unit} onChange={(e) => setUnit(e.target.value)} placeholder="liters" />
            </div>
            <div className="space-y-1.5">
              <Label>Reorder Level</Label>
              <Input type="number" min="0" value={reorderLevel} onChange={(e) => setReorderLevel(e.target.value)} placeholder="10" />
            </div>
            <div className="space-y-1.5">
              <Label>Unit Cost</Label>
              <Input type="number" min="0" step="0.01" value={unitCost} onChange={(e) => setUnitCost(e.target.value)} placeholder="0.00" />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Initial Stock</Label>
            <Input type="number" min="0" value={initialStock} onChange={(e) => setInitialStock(e.target.value)} placeholder="0" />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !name || !categoryId || !branchId || !unit}>
            {isPending ? 'Creating...' : 'Create Supply'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Manage Categories Dialog ────────────────────────────────────────────────

function ManageCategoriesDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (v: boolean) => void }) {
  const { data: categories, isLoading } = useSupplyCategories()
  const { mutate: createCategory, isPending } = useCreateSupplyCategory()
  const [newName, setNewName] = useState('')
  const [newDesc, setNewDesc] = useState('')

  const handleCreate = () => {
    if (!newName.trim()) return
    createCategory({ name: newName.trim(), description: newDesc.trim() || undefined }, {
      onSuccess: () => {
        toast.success(`Category "${newName}" created`)
        setNewName(''); setNewDesc('')
      },
      onError: () => toast.error('Failed to create category.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Supply Categories</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-2">
          {/* Existing categories */}
          <div className="space-y-1.5">
            <Label className="text-xs uppercase tracking-wider text-muted-foreground">Existing Categories</Label>
            {isLoading ? (
              <Skeleton className="h-20 w-full" />
            ) : categories && categories.length > 0 ? (
              <div className="max-h-48 overflow-y-auto space-y-1">
                {categories.map((c) => (
                  <div key={c.id} className="flex items-center justify-between rounded-md border px-3 py-2 text-sm">
                    <div>
                      <span className="font-medium">{c.name}</span>
                      {c.description && (
                        <span className="ml-2 text-muted-foreground text-xs">{c.description}</span>
                      )}
                    </div>
                    <StatusBadge status={c.isActive ? 'Active' : 'Inactive'} label={c.isActive ? 'Active' : 'Inactive'} />
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No categories yet.</p>
            )}
          </div>

          {/* Add new */}
          <div className="space-y-2 border-t pt-3">
            <Label className="text-xs uppercase tracking-wider text-muted-foreground">Add New Category</Label>
            <Input
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              placeholder="Category name"
              onKeyDown={(e) => e.key === 'Enter' && handleCreate()}
            />
            <Input
              value={newDesc}
              onChange={(e) => setNewDesc(e.target.value)}
              placeholder="Description (optional)"
            />
            <Button size="sm" onClick={handleCreate} disabled={isPending || !newName.trim()}>
              <Plus className="mr-1.5 h-3.5 w-3.5" />
              {isPending ? 'Creating...' : 'Add Category'}
            </Button>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Close</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Record Usage Dialog ──────────────────────────────────────────────────────

function RecordUsageDialog({ supply, onOpenChange }: { supply: { id: string; name: string } | null; onOpenChange: (v: boolean) => void }) {
  const { mutate: recordMovement, isPending } = useRecordStockMovement()
  const [quantity, setQuantity] = useState('')
  const [notes, setNotes] = useState('')

  const handleSubmit = () => {
    if (!supply || !quantity) return
    recordMovement({
      supplyItemId: supply.id,
      type: 1,
      quantity: parseInt(quantity),
      notes: notes || undefined,
    }, {
      onSuccess: () => {
        toast.success(`Recorded usage of ${quantity} units`)
        onOpenChange(false)
        setQuantity(''); setNotes('')
      },
      onError: () => toast.error('Failed to record usage.'),
    })
  }

  return (
    <Dialog open={!!supply} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Record Usage — {supply?.name}</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Quantity Used <span className="text-destructive">*</span></Label>
            <Input type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} placeholder="0" autoFocus />
          </div>
          <div className="space-y-1.5">
            <Label>Notes</Label>
            <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} placeholder="Optional notes..." />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !quantity}>
            {isPending ? 'Recording...' : 'Record Usage'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Restock Dialog ───────────────────────────────────────────────────────────

function RestockDialog({ supply, onOpenChange }: { supply: { id: string; name: string } | null; onOpenChange: (v: boolean) => void }) {
  const { mutate: recordMovement, isPending } = useRecordStockMovement()
  const [quantity, setQuantity] = useState('')
  const [unitCost, setUnitCost] = useState('')
  const [reference, setReference] = useState('')
  const [notes, setNotes] = useState('')

  const handleSubmit = () => {
    if (!supply || !quantity) return
    recordMovement({
      supplyItemId: supply.id,
      type: 0,
      quantity: parseInt(quantity),
      unitCost: parseFloat(unitCost) || undefined,
      reference: reference || undefined,
      notes: notes || undefined,
    }, {
      onSuccess: () => {
        toast.success(`Restocked ${quantity} units`)
        onOpenChange(false)
        setQuantity(''); setUnitCost(''); setReference(''); setNotes('')
      },
      onError: () => toast.error('Failed to record restock.'),
    })
  }

  return (
    <Dialog open={!!supply} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Restock — {supply?.name}</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Quantity <span className="text-destructive">*</span></Label>
            <Input type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} placeholder="0" autoFocus />
          </div>
          <div className="space-y-1.5">
            <Label>Unit Cost</Label>
            <Input type="number" min="0" step="0.01" value={unitCost} onChange={(e) => setUnitCost(e.target.value)} placeholder="0.00" />
          </div>
          <div className="space-y-1.5">
            <Label>Reference</Label>
            <Input value={reference} onChange={(e) => setReference(e.target.value)} placeholder="PO number, receipt, etc." />
          </div>
          <div className="space-y-1.5">
            <Label>Notes</Label>
            <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !quantity}>
            {isPending ? 'Restocking...' : 'Restock'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
