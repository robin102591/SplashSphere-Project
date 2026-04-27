'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { ArrowLeft, Plus, Trash2 } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { useSuppliers } from '@/hooks/use-suppliers'
import { useCreatePurchaseOrder } from '@/hooks/use-purchase-orders'
import { useSupplies } from '@/hooks/use-supplies'
import { useMerchandiseList } from '@/hooks/use-merchandise'
import { useBranches } from '@/hooks/use-branches'
import { formatPeso } from '@/lib/format'
import { toast } from 'sonner'
import Link from 'next/link'

// ── Types ────────────────────────────────────────────────────────────────────

type ItemType = 'supply' | 'merchandise' | ''

interface LineItem {
  id: string
  type: ItemType
  itemId: string
  itemName: string
  quantity: string
  unitCost: string
}

function newLineItem(): LineItem {
  return { id: crypto.randomUUID(), type: '', itemId: '', itemName: '', quantity: '', unitCost: '' }
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function CreatePurchaseOrderPage() {
  const router = useRouter()
  const { data: suppliers } = useSuppliers()
  const { data: branches } = useBranches()
  const { data: suppliesData } = useSupplies({ pageSize: 200 })
  const { data: merchandiseData } = useMerchandiseList({ pageSize: 200 })
  const { mutate: createPO, isPending } = useCreatePurchaseOrder()

  const [supplierId, setSupplierId] = useState('')
  const [branchId, setBranchId] = useState('')
  const [expectedDeliveryDate, setExpectedDeliveryDate] = useState('')
  const [notes, setNotes] = useState('')
  const [lineItems, setLineItems] = useState<LineItem[]>([newLineItem()])

  const supplyItems = suppliesData?.items ?? []
  const merchandiseItems = merchandiseData?.items ?? []

  const updateLine = (id: string, updates: Partial<LineItem>) => {
    setLineItems((prev) => prev.map((li) => li.id === id ? { ...li, ...updates } : li))
  }

  const handleTypeChange = (lineId: string, newType: ItemType) => {
    updateLine(lineId, { type: newType, itemId: '', itemName: '', unitCost: '' })
  }

  const handleItemSelect = (lineId: string, line: LineItem, selectedItemId: string) => {
    if (line.type === 'supply') {
      const item = supplyItems.find((s) => s.id === selectedItemId)
      if (item) {
        updateLine(lineId, {
          itemId: item.id,
          itemName: item.name,
          unitCost: item.averageUnitCost > 0 ? String(item.averageUnitCost) : '',
        })
      }
    } else if (line.type === 'merchandise') {
      const item = merchandiseItems.find((m) => m.id === selectedItemId)
      if (item) {
        const cost = item.costPrice ?? item.price
        updateLine(lineId, {
          itemId: item.id,
          itemName: item.name,
          unitCost: cost > 0 ? String(cost) : '',
        })
      }
    }
  }

  const removeLine = (id: string) => {
    setLineItems((prev) => prev.length > 1 ? prev.filter((li) => li.id !== id) : prev)
  }

  const addLine = () => {
    setLineItems((prev) => [...prev, newLineItem()])
  }

  const totalAmount = lineItems.reduce((sum, li) => {
    const qty = parseFloat(li.quantity) || 0
    const cost = parseFloat(li.unitCost) || 0
    return sum + qty * cost
  }, 0)

  const canSubmit = supplierId && branchId && lineItems.some((li) => li.itemId && li.quantity && li.unitCost)

  const handleSubmit = () => {
    if (!canSubmit) return

    const validLines = lineItems
      .filter((li) => li.itemId && li.quantity && li.unitCost)
      .map((li) => ({
        supplyItemId: li.type === 'supply' ? li.itemId : undefined,
        merchandiseId: li.type === 'merchandise' ? li.itemId : undefined,
        itemName: li.itemName,
        quantity: parseInt(li.quantity),
        unitCost: parseFloat(li.unitCost),
      }))

    if (validLines.length === 0) return

    createPO({
      supplierId,
      branchId,
      expectedDeliveryDate: expectedDeliveryDate ? new Date(expectedDeliveryDate).toISOString() : undefined,
      notes: notes || undefined,
      lines: validLines,
    }, {
      onSuccess: (data) => {
        toast.success('Purchase order created')
        router.push(`/dashboard/purchase-orders/${data?.id ?? ''}`)
      },
      onError: () => toast.error('Failed to create purchase order.'),
    })
  }

  return (
    <div className="space-y-6 max-w-4xl">
      <div className="space-y-1">
        <Link href="/dashboard/purchase-orders" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" /> Back to Purchase Orders
        </Link>
        <h1 className="text-2xl font-bold tracking-tight">New Purchase Order</h1>
      </div>

      {/* PO Details */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Order Details</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <Label>Supplier <span className="text-destructive">*</span></Label>
              <Select value={supplierId} onValueChange={setSupplierId}>
                <SelectTrigger><SelectValue placeholder="Select supplier" /></SelectTrigger>
                <SelectContent>
                  {suppliers?.map((s: { id: string; name: string }) => <SelectItem key={s.id} value={s.id}>{s.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Branch <span className="text-destructive">*</span></Label>
              <Select value={branchId} onValueChange={setBranchId}>
                <SelectTrigger><SelectValue placeholder="Select branch" /></SelectTrigger>
                <SelectContent>
                  {branches?.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <Label>Expected Delivery Date</Label>
              <Input type="date" value={expectedDeliveryDate} onChange={(e) => setExpectedDeliveryDate(e.target.value)} />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Notes</Label>
            <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} placeholder="Additional instructions..." />
          </div>
        </CardContent>
      </Card>

      {/* Line Items */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-lg">Line Items</CardTitle>
          <Button variant="outline" size="sm" onClick={addLine}>
            <Plus className="mr-2 h-3.5 w-3.5" /> Add Item
          </Button>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {/* Header */}
            <div className="grid grid-cols-[1fr_2fr_1fr_1fr_1fr_auto] gap-2 text-xs font-medium text-muted-foreground uppercase tracking-wider">
              <span>Type</span>
              <span>Item</span>
              <span>Quantity</span>
              <span>Unit Cost</span>
              <span className="text-right">Total</span>
              <span />
            </div>
            {/* Rows */}
            {lineItems.map((li) => {
              const lineTotal = (parseFloat(li.quantity) || 0) * (parseFloat(li.unitCost) || 0)
              return (
                <div key={li.id} className="grid grid-cols-[1fr_2fr_1fr_1fr_1fr_auto] gap-2 items-center">
                  {/* Type selector */}
                  <Select
                    value={li.type || undefined}
                    onValueChange={(v) => handleTypeChange(li.id, v as ItemType)}
                  >
                    <SelectTrigger className="h-9">
                      <SelectValue placeholder="Type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="supply">Supply</SelectItem>
                      <SelectItem value="merchandise">Merchandise</SelectItem>
                    </SelectContent>
                  </Select>

                  {/* Item picker */}
                  <Select
                    value={li.itemId || undefined}
                    onValueChange={(v) => handleItemSelect(li.id, li, v)}
                    disabled={!li.type}
                  >
                    <SelectTrigger className="h-9">
                      <SelectValue placeholder={li.type ? 'Select item' : 'Choose type first'} />
                    </SelectTrigger>
                    <SelectContent>
                      {li.type === 'supply' && supplyItems.map((s) => (
                        <SelectItem key={s.id} value={s.id}>
                          {s.name} ({s.unit} - stock: {s.currentStock})
                        </SelectItem>
                      ))}
                      {li.type === 'merchandise' && merchandiseItems.map((m) => (
                        <SelectItem key={m.id} value={m.id}>
                          {m.name} {m.sku ? `(${m.sku})` : ''} - stock: {m.stockQuantity}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>

                  {/* Quantity */}
                  <Input
                    type="number"
                    min="1"
                    value={li.quantity}
                    onChange={(e) => updateLine(li.id, { quantity: e.target.value })}
                    placeholder="0"
                    className="h-9"
                  />

                  {/* Unit Cost (auto-filled, manually overridable) */}
                  <Input
                    type="number"
                    min="0"
                    step="0.01"
                    value={li.unitCost}
                    onChange={(e) => updateLine(li.id, { unitCost: e.target.value })}
                    placeholder="0.00"
                    className="h-9"
                  />

                  {/* Line total */}
                  <p className="text-right text-sm font-mono tabular-nums">
                    {formatPeso(lineTotal)}
                  </p>

                  {/* Delete */}
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-9 w-9"
                    onClick={() => removeLine(li.id)}
                    disabled={lineItems.length <= 1}
                  >
                    <Trash2 className="h-3.5 w-3.5 text-destructive" />
                  </Button>
                </div>
              )
            })}
            {/* Total */}
            <div className="grid grid-cols-[1fr_2fr_1fr_1fr_1fr_auto] gap-2 items-center border-t pt-3">
              <span className="text-sm font-medium col-span-2">Total</span>
              <span />
              <span />
              <p className="text-right text-sm font-bold font-mono tabular-nums">
                {formatPeso(totalAmount)}
              </p>
              <span />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex justify-end gap-3">
        <Link href="/dashboard/purchase-orders">
          <Button variant="outline">Cancel</Button>
        </Link>
        <Button onClick={handleSubmit} disabled={isPending || !canSubmit}>
          {isPending ? 'Creating...' : 'Create Purchase Order'}
        </Button>
      </div>
    </div>
  )
}
