'use client'

import { useState } from 'react'
import { useParams } from 'next/navigation'
import {
  ArrowLeft, Wrench, ClipboardCheck, Settings, AlertTriangle, RotateCcw, XCircle,
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
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog'
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import {
  useEquipmentById, useUpdateEquipmentStatus, useLogMaintenance,
} from '@/hooks/use-inventory'
import { formatPeso, formatDate, formatDateTime } from '@/lib/format'
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

const MAINTENANCE_TYPES = [
  { value: 0, key: 'Preventive', label: 'Preventive' },
  { value: 1, key: 'Corrective', label: 'Corrective' },
  { value: 2, key: 'Inspection', label: 'Inspection' },
  { value: 3, key: 'PartReplacement', label: 'Part Replacement' },
]

const MAINTENANCE_TYPE_ICON: Record<string, React.ElementType> = {
  Preventive: ClipboardCheck,
  Corrective: Wrench,
  Inspection: Settings,
  PartReplacement: RotateCcw,
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function EquipmentDetailPage() {
  const params = useParams()
  const id = params.id as string

  const { data: equipment, isLoading } = useEquipmentById(id)
  const { mutate: updateStatus, isPending: updatingStatus } = useUpdateEquipmentStatus()
  const [maintenanceOpen, setMaintenanceOpen] = useState(false)

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  if (!equipment) {
    return (
      <div className="space-y-4">
        <Link href="/dashboard/equipment" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="h-4 w-4" /> Back to Equipment
        </Link>
        <EmptyState icon={Wrench} title="Equipment not found" description="This equipment may have been removed." />
      </div>
    )
  }

  const statusLabel = EQUIPMENT_STATUSES.find((s) => s.key === equipment.status)?.label ?? equipment.status

  const handleStatusChange = (newValue: number) => {
    updateStatus({ id, status: newValue }, {
      onSuccess: () => toast.success(`Status updated to ${EQUIPMENT_STATUSES.find((s) => s.value === newValue)?.label ?? 'Unknown'}`),
      onError: () => toast.error('Failed to update status.'),
    })
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-1">
          <Link href="/dashboard/equipment" className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground">
            <ArrowLeft className="h-4 w-4" /> Back to Equipment
          </Link>
          <h1 className="text-2xl font-bold tracking-tight">{equipment.name}</h1>
        </div>
        <div className="flex items-center gap-2">
          <DropdownMenu>
            <DropdownMenuTrigger>
              <Button variant="outline" disabled={updatingStatus}>
                Update Status
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {EQUIPMENT_STATUSES.filter((s) => s.key !== equipment.status).map((s) => (
                <DropdownMenuItem key={s.value} onClick={() => handleStatusChange(s.value)}>
                  {s.label}
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
          <Button onClick={() => setMaintenanceOpen(true)}>
            <Wrench className="mr-2 h-4 w-4" /> Log Maintenance
          </Button>
        </div>
      </div>

      {/* Info Card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Equipment Details</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <p className="text-xs text-muted-foreground">Name</p>
              <p className="font-medium">{equipment.name}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Brand</p>
              <p className="font-medium">{equipment.brand || '—'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Model</p>
              <p className="font-medium">{equipment.model || '—'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Serial Number</p>
              <p className="font-medium font-mono">{equipment.serialNumber || '—'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Branch</p>
              <p className="font-medium">{equipment.branchName}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Status</p>
              <StatusBadge status={STATUS_BADGE_MAP[equipment.status] || equipment.status} label={statusLabel} />
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Purchase Date</p>
              <p className="font-medium">{equipment.purchaseDate ? formatDate(equipment.purchaseDate) : '—'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Purchase Cost</p>
              <p className="font-medium font-mono tabular-nums">{equipment.purchaseCost ? formatPeso(equipment.purchaseCost) : '—'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Warranty Expiry</p>
              <p className="font-medium">{equipment.warrantyExpiry ? formatDate(equipment.warrantyExpiry) : '—'}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Location</p>
              <p className="font-medium">{equipment.location || '—'}</p>
            </div>
            {equipment.notes && (
              <div className="sm:col-span-2">
                <p className="text-xs text-muted-foreground">Notes</p>
                <p className="font-medium">{equipment.notes}</p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Maintenance Log */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Maintenance Log</CardTitle>
        </CardHeader>
        <CardContent>
          {equipment.maintenanceLogs && equipment.maintenanceLogs.length > 0 ? (
            <div className="space-y-3">
              {equipment.maintenanceLogs.map((log) => {
                const Icon = MAINTENANCE_TYPE_ICON[log.type] ?? Wrench
                return (
                  <div key={log.id} className="flex items-start gap-3 rounded-lg border p-3">
                    <div className="mt-0.5 text-muted-foreground">
                      <Icon className="h-4 w-4" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        <StatusBadge
                          status={log.type === 'Corrective' ? 'Flagged' : log.type === 'Preventive' ? 'Active' : 'Pending'}
                          label={MAINTENANCE_TYPES.find((t) => t.key === log.type)?.label ?? log.type}
                        />
                        {log.cost != null && log.cost > 0 && (
                          <span className="text-xs font-mono tabular-nums text-muted-foreground">{formatPeso(log.cost)}</span>
                        )}
                      </div>
                      <p className="text-sm mt-1">{log.description}</p>
                      {log.performedBy && (
                        <p className="text-xs text-muted-foreground mt-0.5">By: {log.performedBy}</p>
                      )}
                      {log.nextDueDate && (
                        <p className="text-xs text-muted-foreground mt-0.5">Next due: {formatDate(log.nextDueDate)}</p>
                      )}
                      {log.notes && (
                        <p className="text-xs text-muted-foreground mt-0.5">{log.notes}</p>
                      )}
                    </div>
                    <div className="text-xs text-muted-foreground whitespace-nowrap">
                      {formatDateTime(log.performedDate)}
                    </div>
                  </div>
                )
              })}
            </div>
          ) : (
            <p className="text-sm text-muted-foreground text-center py-8">No maintenance logs recorded yet.</p>
          )}
        </CardContent>
      </Card>

      <LogMaintenanceDialog equipmentId={id} equipmentName={equipment.name} open={maintenanceOpen} onOpenChange={setMaintenanceOpen} />
    </div>
  )
}

// ── Log Maintenance Dialog ───────────────────────────────────────────────────

function LogMaintenanceDialog({ equipmentId, equipmentName, open, onOpenChange }: {
  equipmentId: string; equipmentName: string; open: boolean; onOpenChange: (v: boolean) => void
}) {
  const { mutate: logMaint, isPending } = useLogMaintenance()

  const [type, setType] = useState(0)
  const [description, setDescription] = useState('')
  const [cost, setCost] = useState('')
  const [performedBy, setPerformedBy] = useState('')
  const [date, setDate] = useState(new Date().toISOString().split('T')[0])
  const [nextDueDate, setNextDueDate] = useState('')
  const [notes, setNotes] = useState('')

  const resetForm = () => {
    setType(0); setDescription(''); setCost(''); setPerformedBy('')
    setDate(new Date().toISOString().split('T')[0]); setNextDueDate(''); setNotes('')
  }

  const handleSubmit = () => {
    if (!description) return
    logMaint({
      equipmentId,
      type,
      description,
      cost: parseFloat(cost) || undefined,
      performedBy: performedBy || undefined,
      performedDate: new Date(date).toISOString(),
      nextDueDate: nextDueDate ? new Date(nextDueDate).toISOString() : undefined,
      notes: notes || undefined,
    }, {
      onSuccess: () => {
        toast.success('Maintenance logged')
        onOpenChange(false)
        resetForm()
      },
      onError: () => toast.error('Failed to log maintenance.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Log Maintenance — {equipmentName}</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Type <span className="text-destructive">*</span></Label>
              <Select value={String(type)} onValueChange={(v) => setType(Number(v))}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {MAINTENANCE_TYPES.map((t) => <SelectItem key={t.value} value={String(t.value)}>{t.label}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Date <span className="text-destructive">*</span></Label>
              <Input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Description <span className="text-destructive">*</span></Label>
            <Textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={2} placeholder="What was done?" autoFocus />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Cost</Label>
              <Input type="number" min="0" step="0.01" value={cost} onChange={(e) => setCost(e.target.value)} placeholder="0.00" />
            </div>
            <div className="space-y-1.5">
              <Label>Performed By</Label>
              <Input value={performedBy} onChange={(e) => setPerformedBy(e.target.value)} placeholder="Technician name" />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Next Due Date</Label>
            <Input type="date" value={nextDueDate} onChange={(e) => setNextDueDate(e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>Notes</Label>
            <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !description}>
            {isPending ? 'Saving...' : 'Log Maintenance'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
