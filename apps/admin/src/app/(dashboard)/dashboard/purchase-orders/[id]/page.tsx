'use client'

import { useState } from 'react'
import { useParams } from 'next/navigation'
import { ArrowLeft, Send, PackageCheck, XCircle, ClipboardList } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter,
} from '@/components/ui/dialog'
import {
  usePurchaseOrderById, useSendPurchaseOrder, useReceivePurchaseOrder, useCancelPurchaseOrder,
} from '@/hooks/use-inventory'
import { formatPeso, formatDate } from '@/lib/format'
import { toast } from 'sonner'
import Link from 'next/link'

// ── Helpers ──────────────────────────────────────────────────────────────────

const PO_STATUSES = [
  { value: 'Draft', label: 'Draft' },
  { value: 'Sent', label: 'Sent' },
  { value: 'PartiallyReceived', label: 'Partially Received' },
  { value: 'Received', label: 'Received' },
  { value: 'Cancelled', label: 'Cancelled' },
]

const PO_STATUS_BADGE_MAP: Record<string, string> = {
  Draft: 'Draft',
  Sent: 'Open',
  PartiallyReceived: 'Low Stock',
  Received: 'Completed',
  Cancelled: 'Cancelled',
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function PurchaseOrderDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: po, isLoading } = usePurchaseOrderById(id)
  const { mutate: sendPO, isPending: sending } = useSendPurchaseOrder()
  const { mutate: cancelPO, isPending: cancelling } = useCancelPurchaseOrder()
  const [receiveOpen, setReceiveOpen] = useState(false)

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  if (!po) {
    return (
      <div className="space-y-4">
        <Link href="/dashboard/purchase-orders" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" /> Back to Purchase Orders
        </Link>
        <EmptyState icon={ClipboardList} title="Purchase order not found" description="This purchase order may have been deleted." />
      </div>
    )
  }

  const statusLabel = PO_STATUSES.find((s) => s.value === po.status)?.label ?? po.status
  const canSend = po.status === 'Draft'
  const canReceive = po.status === 'Sent' || po.status === 'PartiallyReceived'
  const canCancel = po.status === 'Draft' || po.status === 'Sent'
  const canEdit = po.status === 'Draft'

  const handleSend = () => {
    sendPO(id, {
      onSuccess: () => toast.success('Purchase order sent to supplier'),
      onError: () => toast.error('Failed to send purchase order.'),
    })
  }

  const handleCancel = () => {
    if (!confirm('Cancel this purchase order?')) return
    cancelPO(id, {
      onSuccess: () => toast.success('Purchase order cancelled'),
      onError: () => toast.error('Failed to cancel purchase order.'),
    })
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <Link href="/dashboard/purchase-orders" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
            <ArrowLeft className="h-4 w-4" /> Back to Purchase Orders
          </Link>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold tracking-tight font-mono">{po.poNumber}</h1>
            <StatusBadge
              status={PO_STATUS_BADGE_MAP[po.status] ?? po.status}
              label={statusLabel}
            />
          </div>
        </div>
        <div className="flex items-center gap-2">
          {canSend && (
            <Button onClick={handleSend} disabled={sending}>
              <Send className="mr-2 h-4 w-4" /> {sending ? 'Sending...' : 'Send to Supplier'}
            </Button>
          )}
          {canEdit && (
            <Link href={`/dashboard/purchase-orders/new`}>
              <Button variant="outline">Edit</Button>
            </Link>
          )}
          {canReceive && (
            <Button onClick={() => setReceiveOpen(true)}>
              <PackageCheck className="mr-2 h-4 w-4" /> Receive Items
            </Button>
          )}
          {canCancel && (
            <Button variant="outline" onClick={handleCancel} disabled={cancelling} className="text-destructive">
              <XCircle className="mr-2 h-4 w-4" /> {cancelling ? 'Cancelling...' : 'Cancel'}
            </Button>
          )}
        </div>
      </div>

      {/* PO Info */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Order Details</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <p className="text-xs text-muted-foreground">Supplier</p>
              <p className="font-medium">{po.supplierName}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Branch</p>
              <p className="font-medium">{po.branchName}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Order Date</p>
              <p className="font-medium">{po.orderDate ? formatDate(po.orderDate) : '—'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Expected Delivery</p>
              <p className="font-medium">{po.expectedDeliveryDate ? formatDate(po.expectedDeliveryDate) : '—'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Total Amount</p>
              <p className="font-bold font-mono tabular-nums text-lg">{formatPeso(po.totalAmount)}</p>
            </div>
            {po.notes && (
              <div className="sm:col-span-2">
                <p className="text-xs text-muted-foreground">Notes</p>
                <p className="font-medium">{po.notes}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Line Items */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Line Items</CardTitle>
        </CardHeader>
        <CardContent>
          {po.lines && po.lines.length > 0 ? (
            <div className="rounded-lg border overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-2.5 text-left font-medium">Item Name</th>
                    <th className="px-4 py-2.5 text-right font-medium">Qty Ordered</th>
                    <th className="px-4 py-2.5 text-right font-medium">Qty Received</th>
                    <th className="px-4 py-2.5 text-right font-medium">Unit Cost</th>
                    <th className="px-4 py-2.5 text-right font-medium">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {po.lines.map((li: { id: string; itemName: string; quantity: number; receivedQuantity: number; unitCost: number }, idx: number) => (
                    <tr key={li.id ?? idx} className="hover:bg-muted/30">
                      <td className="px-4 py-2 font-medium">{li.itemName}</td>
                      <td className="px-4 py-2 text-right tabular-nums">{li.quantity}</td>
                      <td className="px-4 py-2 text-right tabular-nums">
                        <span className={li.receivedQuantity >= li.quantity ? 'text-emerald-600 dark:text-emerald-400' : ''}>
                          {li.receivedQuantity ?? 0}
                        </span>
                      </td>
                      <td className="px-4 py-2 text-right font-mono tabular-nums">{formatPeso(li.unitCost)}</td>
                      <td className="px-4 py-2 text-right font-mono tabular-nums font-medium">
                        {formatPeso(li.quantity * li.unitCost)}
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="border-t bg-muted/30">
                  <tr>
                    <td className="px-4 py-2 font-medium">Total</td>
                    <td />
                    <td />
                    <td />
                    <td className="px-4 py-2 text-right font-bold font-mono tabular-nums">{formatPeso(po.totalAmount)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          ) : (
            <p className="text-sm text-muted-foreground text-center py-8">No line items.</p>
          )}
        </CardContent>
      </Card>

      {canReceive && (
        <ReceiveItemsDialog
          poId={id}
          lineItems={[...(po.lines ?? [])]}
          open={receiveOpen}
          onOpenChange={setReceiveOpen}
        />
      )}
    </div>
  )
}

// ── Receive Items Dialog ─────────────────────────────────────────────────────

interface POLineItem {
  id?: string
  itemName: string
  quantity: number
  receivedQuantity?: number
  unitCost: number
}

function ReceiveItemsDialog({ poId, lineItems, open, onOpenChange }: {
  poId: string; lineItems: POLineItem[]; open: boolean; onOpenChange: (v: boolean) => void
}) {
  const { mutate: receive, isPending } = useReceivePurchaseOrder()
  const [receivedQtys, setReceivedQtys] = useState<Record<string, string>>({})

  const updateQty = (itemId: string, value: string) => {
    setReceivedQtys((prev) => ({ ...prev, [itemId]: value }))
  }

  const handleSubmit = () => {
    const lines = lineItems
      .filter((li) => li.id)
      .map((li) => ({
        lineId: li.id!,
        receivedQuantity: parseInt(receivedQtys[li.id!] || '0'),
      }))
      .filter((item) => item.receivedQuantity > 0)

    if (lines.length === 0) {
      toast.error('Enter received quantities for at least one item.')
      return
    }

    receive({ id: poId, lines }, {
      onSuccess: () => {
        toast.success('Items received successfully')
        onOpenChange(false)
        setReceivedQtys({})
      },
      onError: () => toast.error('Failed to receive items.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Receive Items</DialogTitle>
          <DialogDescription>Enter the quantity received for each line item.</DialogDescription>
        </DialogHeader>
        <div className="space-y-3 py-2 max-h-[50vh] overflow-y-auto">
          {lineItems.map((li, idx) => {
            const remaining = li.quantity - (li.receivedQuantity ?? 0)
            return (
              <div key={li.id ?? idx} className="flex items-center gap-3 rounded-lg border p-3">
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium">{li.itemName}</p>
                  <p className="text-xs text-muted-foreground">
                    Ordered: {li.quantity} | Received: {li.receivedQuantity ?? 0} | Remaining: {remaining}
                  </p>
                </div>
                <div className="w-24">
                  <Input
                    type="number"
                    min="0"
                    max={remaining}
                    value={receivedQtys[li.id ?? ''] ?? ''}
                    onChange={(e) => updateQty(li.id ?? '', e.target.value)}
                    placeholder="0"
                    className="h-9"
                  />
                </div>
              </div>
            )
          })}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending}>
            {isPending ? 'Receiving...' : 'Confirm Receipt'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
