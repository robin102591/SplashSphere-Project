'use client'

import { useState } from 'react'
import { Plus, ClipboardList } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import { StatusBadge } from '@/components/ui/status-badge'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { usePurchaseOrders } from '@/hooks/use-purchase-orders'
import { useSuppliers } from '@/hooks/use-suppliers'
import { useBranches } from '@/hooks/use-branches'
import { formatPeso, formatDate } from '@/lib/format'
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

export default function PurchaseOrdersPage() {
  const [supplierId, setSupplierId] = useState('')
  const [branchId, setBranchId] = useState('')
  const [status, setStatus] = useState('')

  const { data: branches } = useBranches()
  const { data: suppliers } = useSuppliers()
  const { data, isLoading } = usePurchaseOrders({
    supplierId: supplierId || undefined,
    branchId: branchId || undefined,
    status: status ? Number(status) : undefined,
    pageSize: 100,
  })

  const orders = data?.items ?? []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Purchase Orders</h1>
          <p className="text-sm text-muted-foreground">Manage supply purchase orders and receiving</p>
        </div>
        <Link href="/dashboard/purchase-orders/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" /> New Purchase Order
          </Button>
        </Link>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <Select value={supplierId || 'all'} onValueChange={(v) => setSupplierId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Suppliers" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Suppliers</SelectItem>
            {suppliers?.map((s: { id: string; name: string }) => <SelectItem key={s.id} value={s.id}>{s.name}</SelectItem>)}
          </SelectContent>
        </Select>
        <Select value={branchId || 'all'} onValueChange={(v) => setBranchId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Branches" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Branches</SelectItem>
            {branches?.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
          </SelectContent>
        </Select>
        <Select value={status || 'all'} onValueChange={(v) => setStatus(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[200px] h-9"><SelectValue placeholder="All Statuses" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Statuses</SelectItem>
            {PO_STATUSES.map((s) => <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>)}
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      {isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : orders.length > 0 ? (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">PO Number</th>
                <th className="px-4 py-2.5 text-left font-medium">Supplier</th>
                <th className="px-4 py-2.5 text-left font-medium">Branch</th>
                <th className="px-4 py-2.5 text-left font-medium">Status</th>
                <th className="px-4 py-2.5 text-right font-medium">Total Amount</th>
                <th className="px-4 py-2.5 text-left font-medium">Order Date</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {orders.map((po) => (
                <tr key={po.id} className="hover:bg-muted/30">
                  <td className="px-4 py-2 font-medium font-mono">
                    <Link href={`/dashboard/purchase-orders/${po.id}`} className="hover:underline">
                      {po.poNumber}
                    </Link>
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">{po.supplierName}</td>
                  <td className="px-4 py-2 text-muted-foreground">{po.branchName}</td>
                  <td className="px-4 py-2">
                    <StatusBadge
                      status={PO_STATUS_BADGE_MAP[po.status] ?? po.status}
                      label={PO_STATUSES.find((s) => s.value === po.status)?.label ?? po.status}
                    />
                  </td>
                  <td className="px-4 py-2 text-right font-mono tabular-nums">{formatPeso(po.totalAmount)}</td>
                  <td className="px-4 py-2 text-muted-foreground">{po.orderDate ? formatDate(po.orderDate) : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <EmptyState
          icon={ClipboardList}
          title="No purchase orders"
          description="Create your first purchase order to manage supply procurement."
          action={{ label: 'New Purchase Order', onClick: () => {}, icon: Plus }}
        />
      )}
    </div>
  )
}
