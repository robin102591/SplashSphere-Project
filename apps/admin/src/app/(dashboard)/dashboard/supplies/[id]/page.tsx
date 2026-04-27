'use client'

import { useState } from 'react'
import { useParams } from 'next/navigation'
import {
  ArrowLeft, Package, Minus, PackagePlus, ArrowUpDown,
  ArrowDownLeft, ArrowUpRight, RotateCcw, Trash2,
} from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog'
import { useSupplyById } from '@/hooks/use-supplies'
import { useStockMovements, useRecordStockMovement } from '@/hooks/use-stock-movements'
import { formatPeso, formatDateTime } from '@/lib/format'
import { toast } from 'sonner'
import Link from 'next/link'

// ── Helpers ──────────────────────────────────────────────────────────────────

const MOVEMENT_TYPE_CONFIG: Record<string, { label: string; icon: React.ElementType; color: string }> = {
  PurchaseIn:      { label: 'Purchase In',      icon: ArrowDownLeft,  color: 'text-emerald-600 dark:text-emerald-400' },
  UsageOut:        { label: 'Usage Out',         icon: ArrowUpRight,   color: 'text-orange-600 dark:text-orange-400' },
  SaleOut:         { label: 'Sale Out',          icon: ArrowUpRight,   color: 'text-blue-600 dark:text-blue-400' },
  Adjustment:      { label: 'Adjustment',        icon: ArrowUpDown,    color: 'text-violet-600 dark:text-violet-400' },
  Return:          { label: 'Return',            icon: RotateCcw,      color: 'text-cyan-600 dark:text-cyan-400' },
  Waste:           { label: 'Waste',             icon: Trash2,         color: 'text-red-600 dark:text-red-400' },
  TransferIn:      { label: 'Transfer In',       icon: ArrowDownLeft,  color: 'text-emerald-600 dark:text-emerald-400' },
  TransferOut:     { label: 'Transfer Out',      icon: ArrowUpRight,   color: 'text-amber-600 dark:text-amber-400' },
}

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

export default function SupplyDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: supply, isLoading } = useSupplyById(id)
  const { data: movements, isLoading: movementsLoading } = useStockMovements({ supplyItemId: id, pageSize: 50 })

  const [usageOpen, setUsageOpen] = useState(false)
  const [restockOpen, setRestockOpen] = useState(false)
  const [adjustOpen, setAdjustOpen] = useState(false)

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  if (!supply) {
    return (
      <div className="space-y-4">
        <Link href="/dashboard/supplies" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" /> Back to Supplies
        </Link>
        <EmptyState icon={Package} title="Supply not found" description="This supply item may have been deleted." />
      </div>
    )
  }

  const reorderLevel = supply.reorderLevel ?? 0
  const status = stockStatus(supply.currentStock, reorderLevel)

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <Link href="/dashboard/supplies" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
            <ArrowLeft className="h-4 w-4" /> Back to Supplies
          </Link>
          <h1 className="text-2xl font-bold tracking-tight">{supply.name}</h1>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => setUsageOpen(true)}>
            <Minus className="mr-2 h-4 w-4" /> Record Usage
          </Button>
          <Button variant="outline" onClick={() => setRestockOpen(true)}>
            <PackagePlus className="mr-2 h-4 w-4" /> Restock
          </Button>
          <Button variant="outline" onClick={() => setAdjustOpen(true)}>
            <ArrowUpDown className="mr-2 h-4 w-4" /> Adjust Stock
          </Button>
        </div>
      </div>

      {/* Info Card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Supply Details</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <p className="text-xs text-muted-foreground">Category</p>
              <p className="font-medium">{supply.categoryName}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Branch</p>
              <p className="font-medium">{supply.branchName}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Unit</p>
              <p className="font-medium">{supply.unit}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Average Unit Cost</p>
              <p className="font-medium font-mono tabular-nums">{formatPeso(supply.averageUnitCost)}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Current Stock</p>
              <p className={`text-2xl font-bold tabular-nums ${stockColor(supply.currentStock, reorderLevel)}`}>
                {supply.currentStock}
              </p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Reorder Level</p>
              <p className="font-medium tabular-nums">{reorderLevel}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Status</p>
              <StatusBadge
                status={status === 'In Stock' ? 'Active' : status === 'Low Stock' ? 'Low Stock' : 'Inactive'}
                label={status}
              />
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Stock Level</p>
              <div className="mt-1 h-2.5 rounded-full bg-muted overflow-hidden">
                <div
                  className={`h-full rounded-full transition-all ${
                    reorderLevel > 0 && supply.currentStock / reorderLevel <= 0.2
                      ? 'bg-red-500'
                      : reorderLevel > 0 && supply.currentStock / reorderLevel <= 0.5
                        ? 'bg-amber-500'
                        : 'bg-emerald-500'
                  }`}
                  style={{
                    width: `${Math.min(100, reorderLevel > 0 ? (supply.currentStock / (reorderLevel * 2)) * 100 : 100)}%`,
                  }}
                />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Stock Movements Timeline */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Stock Movements</CardTitle>
        </CardHeader>
        <CardContent>
          {movementsLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-12 w-full" />)}
            </div>
          ) : movements && movements.items.length > 0 ? (
            <div className="space-y-3">
              {movements.items.map((m) => {
                const config = MOVEMENT_TYPE_CONFIG[m.type] ?? { label: m.type, icon: ArrowUpDown, color: 'text-gray-500' }
                const Icon = config.icon
                const isInbound = ['PurchaseIn', 'Return', 'TransferIn', 'Adjustment'].includes(m.type) && m.quantity > 0
                return (
                  <div key={m.id} className="flex items-start gap-3 rounded-lg border p-3">
                    <div className={`mt-0.5 ${config.color}`}>
                      <Icon className="h-4 w-4" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="text-sm font-medium">{config.label}</span>
                        <span className={`text-sm font-mono tabular-nums font-bold ${isInbound ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'}`}>
                          {isInbound ? '+' : '-'}{Math.abs(m.quantity)}
                        </span>
                        {m.unitCost != null && m.unitCost > 0 && (
                          <span className="text-xs text-muted-foreground">@ {formatPeso(m.unitCost)}</span>
                        )}
                      </div>
                      {m.reference && (
                        <p className="text-xs text-muted-foreground mt-0.5">Ref: {m.reference}</p>
                      )}
                      {m.notes && (
                        <p className="text-xs text-muted-foreground mt-0.5">{m.notes}</p>
                      )}
                    </div>
                    <div className="text-xs text-muted-foreground whitespace-nowrap">
                      {formatDateTime(m.movementDate)}
                    </div>
                  </div>
                )
              })}
            </div>
          ) : (
            <p className="text-sm text-muted-foreground text-center py-8">No stock movements recorded yet.</p>
          )}
        </CardContent>
      </Card>

      {/* Dialogs */}
      <RecordUsageDialog supplyId={id} supplyName={supply.name} open={usageOpen} onOpenChange={setUsageOpen} />
      <RestockDialog supplyId={id} supplyName={supply.name} open={restockOpen} onOpenChange={setRestockOpen} />
      <AdjustStockDialog supplyId={id} supplyName={supply.name} currentStock={supply.currentStock} open={adjustOpen} onOpenChange={setAdjustOpen} />
    </div>
  )
}

// ── Record Usage Dialog ──────────────────────────────────────────────────────

function RecordUsageDialog({ supplyId, supplyName, open, onOpenChange }: {
  supplyId: string; supplyName: string; open: boolean; onOpenChange: (v: boolean) => void
}) {
  const { mutate: record, isPending } = useRecordStockMovement()
  const [quantity, setQuantity] = useState('')
  const [notes, setNotes] = useState('')

  const handleSubmit = () => {
    if (!quantity) return
    record({ supplyItemId: supplyId, type: 1, quantity: parseInt(quantity), notes: notes || undefined }, {
      onSuccess: () => {
        toast.success(`Recorded usage of ${quantity} units`)
        onOpenChange(false); setQuantity(''); setNotes('')
      },
      onError: () => toast.error('Failed to record usage.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader><DialogTitle>Record Usage — {supplyName}</DialogTitle></DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Quantity Used <span className="text-destructive">*</span></Label>
            <Input type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} autoFocus />
          </div>
          <div className="space-y-1.5">
            <Label>Notes</Label>
            <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !quantity}>{isPending ? 'Recording...' : 'Record Usage'}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Restock Dialog ───────────────────────────────────────────────────────────

function RestockDialog({ supplyId, supplyName, open, onOpenChange }: {
  supplyId: string; supplyName: string; open: boolean; onOpenChange: (v: boolean) => void
}) {
  const { mutate: record, isPending } = useRecordStockMovement()
  const [quantity, setQuantity] = useState('')
  const [unitCost, setUnitCost] = useState('')
  const [reference, setReference] = useState('')
  const [notes, setNotes] = useState('')

  const handleSubmit = () => {
    if (!quantity) return
    record({
      supplyItemId: supplyId, type: 0, quantity: parseInt(quantity),
      unitCost: parseFloat(unitCost) || undefined,
      reference: reference || undefined,
      notes: notes || undefined,
    }, {
      onSuccess: () => {
        toast.success(`Restocked ${quantity} units`)
        onOpenChange(false); setQuantity(''); setUnitCost(''); setReference(''); setNotes('')
      },
      onError: () => toast.error('Failed to restock.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader><DialogTitle>Restock — {supplyName}</DialogTitle></DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Quantity <span className="text-destructive">*</span></Label>
            <Input type="number" min="1" value={quantity} onChange={(e) => setQuantity(e.target.value)} autoFocus />
          </div>
          <div className="space-y-1.5">
            <Label>Unit Cost</Label>
            <Input type="number" min="0" step="0.01" value={unitCost} onChange={(e) => setUnitCost(e.target.value)} />
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
          <Button onClick={handleSubmit} disabled={isPending || !quantity}>{isPending ? 'Restocking...' : 'Restock'}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Adjust Stock Dialog ──────────────────────────────────────────────────────

function AdjustStockDialog({ supplyId, supplyName, currentStock, open, onOpenChange }: {
  supplyId: string; supplyName: string; currentStock: number; open: boolean; onOpenChange: (v: boolean) => void
}) {
  const { mutate: record, isPending } = useRecordStockMovement()
  const [newStock, setNewStock] = useState(String(currentStock))
  const [reason, setReason] = useState('')

  const adjustment = parseInt(newStock) - currentStock

  const handleSubmit = () => {
    if (adjustment === 0 || !reason) return
    record({
      supplyItemId: supplyId, type: 3, quantity: adjustment,
      notes: reason,
    }, {
      onSuccess: () => {
        toast.success(`Stock adjusted by ${adjustment > 0 ? '+' : ''}${adjustment}`)
        onOpenChange(false); setNewStock(String(currentStock)); setReason('')
      },
      onError: () => toast.error('Failed to adjust stock.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader><DialogTitle>Adjust Stock — {supplyName}</DialogTitle></DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Current Stock</Label>
            <p className="text-lg font-bold tabular-nums">{currentStock}</p>
          </div>
          <div className="space-y-1.5">
            <Label>New Stock Level <span className="text-destructive">*</span></Label>
            <Input type="number" min="0" value={newStock} onChange={(e) => setNewStock(e.target.value)} autoFocus />
          </div>
          {adjustment !== 0 && (
            <p className={`text-sm font-medium tabular-nums ${adjustment > 0 ? 'text-emerald-600' : 'text-red-600'}`}>
              Adjustment: {adjustment > 0 ? '+' : ''}{adjustment}
            </p>
          )}
          <div className="space-y-1.5">
            <Label>Reason <span className="text-destructive">*</span></Label>
            <Textarea value={reason} onChange={(e) => setReason(e.target.value)} rows={2} placeholder="Why is the stock being adjusted?" />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || adjustment === 0 || !reason}>
            {isPending ? 'Adjusting...' : 'Adjust Stock'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
