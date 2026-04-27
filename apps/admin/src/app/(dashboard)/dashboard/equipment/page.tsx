'use client'

import { useState } from 'react'
import { Plus, Wrench } from 'lucide-react'
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
import { useEquipment, useRegisterEquipment } from '@/hooks/use-equipment'
import { useBranches } from '@/hooks/use-branches'
import { formatPeso, formatDate } from '@/lib/format'
import { toast } from 'sonner'
import Link from 'next/link'

// ── Helpers ──────────────────────────────────────────────────────────────────

const EQUIPMENT_STATUSES = [
  { value: 0, key: 'Operational', label: 'Operational' },
  { value: 1, key: 'NeedsMaintenance', label: 'Needs Maintenance' },
  { value: 2, key: 'UnderRepair', label: 'Under Repair' },
  { value: 3, key: 'Retired', label: 'Retired' },
]

const STATUS_BADGE_MAP: Record<string, string> = {
  Operational: 'Active',
  NeedsMaintenance: 'Low Stock',
  UnderRepair: 'Flagged',
  Retired: 'Inactive',
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function EquipmentPage() {
  const [branchId, setBranchId] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [createOpen, setCreateOpen] = useState(false)

  const { data: branches } = useBranches()
  const { data, isLoading } = useEquipment({
    branchId: branchId || undefined,
    status: statusFilter ? Number(statusFilter) : undefined,
    pageSize: 100,
  })

  const equipment = data?.items ?? []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Equipment</h1>
          <p className="text-sm text-muted-foreground">Track equipment, maintenance schedules, and status</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" /> Register Equipment
        </Button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <Select value={branchId || 'all'} onValueChange={(v) => setBranchId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Branches" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Branches</SelectItem>
            {branches?.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
          </SelectContent>
        </Select>
        <Select value={statusFilter || 'all'} onValueChange={(v) => setStatusFilter(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[200px] h-9"><SelectValue placeholder="All Statuses" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Statuses</SelectItem>
            {EQUIPMENT_STATUSES.map((s) => <SelectItem key={s.value} value={String(s.value)}>{s.label}</SelectItem>)}
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      {isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : equipment.length > 0 ? (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Name</th>
                <th className="px-4 py-2.5 text-left font-medium">Brand / Model</th>
                <th className="px-4 py-2.5 text-left font-medium">Branch</th>
                <th className="px-4 py-2.5 text-left font-medium">Status</th>
                <th className="px-4 py-2.5 text-left font-medium">Location</th>
                <th className="px-4 py-2.5 text-left font-medium">Last Maintenance</th>
                <th className="px-4 py-2.5 text-left font-medium">Next Due</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {equipment.map((e) => (
                <tr key={e.id} className="hover:bg-muted/30">
                  <td className="px-4 py-2 font-medium">
                    <Link href={`/dashboard/equipment/${e.id}`} className="hover:underline">
                      {e.name}
                    </Link>
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">
                    {[e.brand, e.model].filter(Boolean).join(' ') || '—'}
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">{e.branchName}</td>
                  <td className="px-4 py-2">
                    <StatusBadge
                      status={STATUS_BADGE_MAP[e.status] ?? e.status}
                      label={EQUIPMENT_STATUSES.find((s) => s.key === e.status)?.label ?? e.status}
                    />
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">{e.location || '—'}</td>
                  <td className="px-4 py-2 text-muted-foreground">
                    {e.lastMaintenanceDate ? formatDate(e.lastMaintenanceDate) : '—'}
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">
                    {e.nextMaintenanceDue ? formatDate(e.nextMaintenanceDue) : '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <EmptyState
          icon={Wrench}
          title="No equipment registered"
          description="Register your first piece of equipment to start tracking maintenance."
          action={{ label: 'Register Equipment', onClick: () => setCreateOpen(true), icon: Plus }}
        />
      )}

      <RegisterEquipmentDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  )
}

// ── Register Equipment Dialog ────────────────────────────────────────────────

function RegisterEquipmentDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (v: boolean) => void }) {
  const { data: branches } = useBranches()
  const { mutate: register, isPending } = useRegisterEquipment()

  const [name, setName] = useState('')
  const [brand, setBrand] = useState('')
  const [model, setModel] = useState('')
  const [serialNumber, setSerialNumber] = useState('')
  const [branchId, setBranchId] = useState('')
  const [purchaseDate, setPurchaseDate] = useState('')
  const [purchaseCost, setPurchaseCost] = useState('')
  const [warrantyExpiry, setWarrantyExpiry] = useState('')
  const [location, setLocation] = useState('')
  const [notes, setNotes] = useState('')

  const resetForm = () => {
    setName(''); setBrand(''); setModel(''); setSerialNumber(''); setBranchId('')
    setPurchaseDate(''); setPurchaseCost(''); setWarrantyExpiry(''); setLocation(''); setNotes('')
  }

  const handleSubmit = () => {
    if (!name || !branchId) return
    register({
      name,
      brand: brand || undefined,
      model: model || undefined,
      serialNumber: serialNumber || undefined,
      branchId,
      purchaseDate: purchaseDate ? new Date(purchaseDate).toISOString() : undefined,
      purchaseCost: parseFloat(purchaseCost) || undefined,
      warrantyExpiry: warrantyExpiry ? new Date(warrantyExpiry).toISOString() : undefined,
      location: location || undefined,
      notes: notes || undefined,
    }, {
      onSuccess: () => {
        toast.success(`Equipment "${name}" registered`)
        onOpenChange(false)
        resetForm()
      },
      onError: () => toast.error('Failed to register equipment.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Register Equipment</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2 max-h-[60vh] overflow-y-auto">
          <div className="space-y-1.5">
            <Label>Name <span className="text-destructive">*</span></Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g. Pressure Washer" autoFocus />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Brand</Label>
              <Input value={brand} onChange={(e) => setBrand(e.target.value)} placeholder="e.g. Karcher" />
            </div>
            <div className="space-y-1.5">
              <Label>Model</Label>
              <Input value={model} onChange={(e) => setModel(e.target.value)} placeholder="e.g. K5 Premium" />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Serial Number</Label>
              <Input value={serialNumber} onChange={(e) => setSerialNumber(e.target.value)} />
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
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Purchase Date</Label>
              <Input type="date" value={purchaseDate} onChange={(e) => setPurchaseDate(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Purchase Cost</Label>
              <Input type="number" min="0" step="0.01" value={purchaseCost} onChange={(e) => setPurchaseCost(e.target.value)} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Warranty Expiry</Label>
              <Input type="date" value={warrantyExpiry} onChange={(e) => setWarrantyExpiry(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Location</Label>
              <Input value={location} onChange={(e) => setLocation(e.target.value)} placeholder="e.g. Bay 1" />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Notes</Label>
            <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !name || !branchId}>
            {isPending ? 'Registering...' : 'Register'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
