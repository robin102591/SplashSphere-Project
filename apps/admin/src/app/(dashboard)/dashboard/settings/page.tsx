'use client'

import { useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Button } from '@/components/ui/button'
import { StatusBadge } from '@/components/ui/status-badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
  AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { useVehicleTypes, useCreateVehicleType, useUpdateVehicleType, useToggleVehicleType } from '@/hooks/use-vehicle-types'
import { useSizes, useCreateSize, useUpdateSize, useToggleSize } from '@/hooks/use-sizes'
import { useServiceCategories, useCreateServiceCategory, useUpdateServiceCategory, useToggleServiceCategory } from '@/hooks/use-service-categories'
import { useMakes, useModelsByMake, useCreateMake, useToggleMake, useCreateModel, useToggleModel } from '@/hooks/use-cars'
import { useShiftSettings, useUpdateShiftSettings } from '@/hooks/use-shifts'
import { usePayrollTemplates, useCreatePayrollTemplate, useUpdatePayrollTemplate, useDeletePayrollTemplate, usePayrollSettings, useUpdatePayrollSettings } from '@/hooks/use-payroll'
import type { VehicleType, Size, Make, VehicleModel, ServiceCategory, PayrollAdjustmentTemplate } from '@splashsphere/types'
import { AdjustmentType } from '@splashsphere/types'
import { formatPeso } from '@/lib/format'
import { toast } from 'sonner'
import { cn } from '@/lib/utils'

// ── Reusable simple-name dialog ───────────────────────────────────────────────

function NameDialog({
  open, onOpenChange, title, initialName, onSave, isPending,
}: {
  open: boolean
  onOpenChange: (v: boolean) => void
  title: string
  initialName?: string
  onSave: (name: string) => void
  isPending: boolean
}) {
  const [name, setName] = useState(initialName ?? '')
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader><DialogTitle>{title}</DialogTitle></DialogHeader>
        <div className="space-y-2 py-2">
          <Label>Name <span className="text-destructive">*</span></Label>
          <Input value={name} onChange={(e) => setName(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && name.trim() && onSave(name.trim())} />
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={() => onSave(name.trim())} disabled={!name.trim() || isPending}>
            {isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Vehicle Types tab ─────────────────────────────────────────────────────────

function VehicleTypesTab() {
  const { data: items = [], isLoading } = useVehicleTypes()
  const { mutate: create, isPending: creating } = useCreateVehicleType()
  const { mutate: toggle } = useToggleVehicleType()
  const [addOpen, setAddOpen] = useState(false)
  const [editing, setEditing] = useState<VehicleType | null>(null)
  const { mutate: update, isPending: updating } = useUpdateVehicleType(editing?.id ?? '')

  const handleCreate = (name: string) => create(name, {
    onSuccess: () => { toast.success('Vehicle type created'); setAddOpen(false) },
    onError: () => toast.error('Failed to create vehicle type'),
  })

  const handleUpdate = (name: string) => update(name, {
    onSuccess: () => { toast.success('Updated'); setEditing(null) },
    onError: () => toast.error('Failed to update'),
  })

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">Vehicle types used in pricing matrices (e.g. Sedan, SUV, Van)</p>
        <Button size="sm" onClick={() => setAddOpen(true)}><Plus className="mr-1 h-3.5 w-3.5" />Add</Button>
      </div>
      {isLoading ? <Skeleton className="h-40 w-full" /> : (
        <div className="rounded-lg border divide-y">
          {items.length === 0 && <p className="p-6 text-center text-sm text-muted-foreground">No vehicle types yet</p>}
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between px-4 py-2.5">
              <div className="flex items-center gap-3">
                <span className="font-medium text-sm">{item.name}</span>
                <StatusBadge status={item.isActive ? 'Active' : 'Inactive'} />
              </div>
              <div className="flex gap-2">
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => setEditing(item)}>
                  <Pencil className="h-3.5 w-3.5" />
                </Button>
                <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={() => toggle(item.id, { onSuccess: () => toast.success('Updated') })}>
                  {item.isActive ? 'Deactivate' : 'Activate'}
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}
      <NameDialog open={addOpen} onOpenChange={setAddOpen} title="Add Vehicle Type" onSave={handleCreate} isPending={creating} />
      {editing && (
        <NameDialog open onOpenChange={(v) => !v && setEditing(null)} title="Edit Vehicle Type" initialName={editing.name} onSave={handleUpdate} isPending={updating} />
      )}
    </div>
  )
}

// ── Sizes tab ─────────────────────────────────────────────────────────────────

function SizesTab() {
  const { data: items = [], isLoading } = useSizes()
  const { mutate: create, isPending: creating } = useCreateSize()
  const { mutate: toggle } = useToggleSize()
  const [addOpen, setAddOpen] = useState(false)
  const [editing, setEditing] = useState<Size | null>(null)
  const { mutate: update, isPending: updating } = useUpdateSize(editing?.id ?? '')

  const handleCreate = (name: string) => create(name, {
    onSuccess: () => { toast.success('Size created'); setAddOpen(false) },
    onError: () => toast.error('Failed to create size'),
  })

  const handleUpdate = (name: string) => update(name, {
    onSuccess: () => { toast.success('Updated'); setEditing(null) },
    onError: () => toast.error('Failed to update'),
  })

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">Vehicle sizes for pricing matrices (e.g. Small, Medium, Large, XL)</p>
        <Button size="sm" onClick={() => setAddOpen(true)}><Plus className="mr-1 h-3.5 w-3.5" />Add</Button>
      </div>
      {isLoading ? <Skeleton className="h-40 w-full" /> : (
        <div className="rounded-lg border divide-y">
          {items.length === 0 && <p className="p-6 text-center text-sm text-muted-foreground">No sizes yet</p>}
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between px-4 py-2.5">
              <div className="flex items-center gap-3">
                <span className="font-medium text-sm">{item.name}</span>
                <StatusBadge status={item.isActive ? 'Active' : 'Inactive'} />
              </div>
              <div className="flex gap-2">
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => setEditing(item)}>
                  <Pencil className="h-3.5 w-3.5" />
                </Button>
                <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={() => toggle(item.id, { onSuccess: () => toast.success('Updated') })}>
                  {item.isActive ? 'Deactivate' : 'Activate'}
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}
      <NameDialog open={addOpen} onOpenChange={setAddOpen} title="Add Size" onSave={handleCreate} isPending={creating} />
      {editing && (
        <NameDialog open onOpenChange={(v) => !v && setEditing(null)} title="Edit Size" initialName={editing.name} onSave={handleUpdate} isPending={updating} />
      )}
    </div>
  )
}

// ── Makes & Models tab ────────────────────────────────────────────────────────

function MakesModelsTab() {
  const { data: makes = [], isLoading: makesLoading } = useMakes()
  const { mutate: createMake, isPending: creatingMake } = useCreateMake()
  const { mutate: toggleMake } = useToggleMake()
  const { mutate: createModel, isPending: creatingModel } = useCreateModel()
  const { mutate: toggleModel } = useToggleModel()

  const [selectedMake, setSelectedMake] = useState<Make | null>(null)
  const { data: models = [], isLoading: modelsLoading } = useModelsByMake(selectedMake?.id)

  const [addMakeOpen, setAddMakeOpen] = useState(false)
  const [addModelOpen, setAddModelOpen] = useState(false)

  const handleCreateMake = (name: string) => createMake(name, {
    onSuccess: () => { toast.success('Make created'); setAddMakeOpen(false) },
    onError: () => toast.error('Failed to create make'),
  })

  const handleCreateModel = (name: string) => {
    if (!selectedMake) return
    createModel({ makeId: selectedMake.id, name }, {
      onSuccess: () => { toast.success('Model created'); setAddModelOpen(false) },
      onError: () => toast.error('Failed to create model'),
    })
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
      {/* Makes */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <p className="font-medium text-sm">Makes</p>
          <Button size="sm" onClick={() => setAddMakeOpen(true)}><Plus className="mr-1 h-3.5 w-3.5" />Add</Button>
        </div>
        {makesLoading ? <Skeleton className="h-60 w-full" /> : (
          <div className="rounded-lg border divide-y max-h-96 overflow-y-auto">
            {makes.length === 0 && <p className="p-6 text-center text-sm text-muted-foreground">No makes yet</p>}
            {makes.map((make) => (
              <div
                key={make.id}
                className={cn('flex items-center justify-between px-3 py-2 cursor-pointer hover:bg-muted/40 transition-colors', selectedMake?.id === make.id && 'bg-primary/5 border-l-2 border-primary')}
                onClick={() => setSelectedMake(make)}
              >
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium">{make.name}</span>
                  {!make.isActive && <StatusBadge status="Inactive" />}
                </div>
                <Button variant="ghost" size="sm" className="h-6 text-xs" onClick={(e) => { e.stopPropagation(); toggleMake(make.id, { onSuccess: () => toast.success('Updated') }) }}>
                  {make.isActive ? 'Deactivate' : 'Activate'}
                </Button>
              </div>
            ))}
          </div>
        )}
        <NameDialog open={addMakeOpen} onOpenChange={setAddMakeOpen} title="Add Make" onSave={handleCreateMake} isPending={creatingMake} />
      </div>

      {/* Models */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <p className="font-medium text-sm">
            {selectedMake ? `Models — ${selectedMake.name}` : 'Models'}
          </p>
          <Button size="sm" disabled={!selectedMake} onClick={() => setAddModelOpen(true)}>
            <Plus className="mr-1 h-3.5 w-3.5" />Add
          </Button>
        </div>
        {!selectedMake ? (
          <div className="rounded-lg border border-dashed p-10 text-center text-sm text-muted-foreground">
            Select a make to view its models
          </div>
        ) : modelsLoading ? <Skeleton className="h-60 w-full" /> : (
          <div className="rounded-lg border divide-y max-h-96 overflow-y-auto">
            {models.length === 0 && <p className="p-6 text-center text-sm text-muted-foreground">No models for {selectedMake.name}</p>}
            {models.map((model) => (
              <div key={model.id} className="flex items-center justify-between px-3 py-2">
                <div className="flex items-center gap-2">
                  <span className="text-sm">{model.name}</span>
                  {!model.isActive && <StatusBadge status="Inactive" />}
                </div>
                <Button variant="ghost" size="sm" className="h-6 text-xs" onClick={() => toggleModel({ id: model.id, makeId: selectedMake.id }, { onSuccess: () => toast.success('Updated') })}>
                  {model.isActive ? 'Deactivate' : 'Activate'}
                </Button>
              </div>
            ))}
          </div>
        )}
        <NameDialog open={addModelOpen} onOpenChange={setAddModelOpen} title={`Add Model — ${selectedMake?.name ?? ''}`} onSave={handleCreateModel} isPending={creatingModel} />
      </div>
    </div>
  )
}

// ── Categories tab ────────────────────────────────────────────────────────────

function CategoriesTab() {
  const { data: items = [], isLoading } = useServiceCategories()
  const { mutate: create, isPending: creating } = useCreateServiceCategory()
  const { mutate: toggle } = useToggleServiceCategory()
  const [addOpen, setAddOpen] = useState(false)
  const [editing, setEditing] = useState<ServiceCategory | null>(null)
  const { mutate: update, isPending: updating } = useUpdateServiceCategory(editing?.id ?? '')

  const [form, setForm] = useState({ name: '', description: '' })

  const handleSave = () => {
    const payload = { name: form.name.trim(), description: form.description.trim() || undefined }
    if (editing) {
      update(payload, { onSuccess: () => { toast.success('Updated'); setEditing(null) }, onError: () => toast.error('Failed') })
    } else {
      create(payload, { onSuccess: () => { toast.success('Category created'); setAddOpen(false) }, onError: () => toast.error('Failed') })
    }
  }

  const openAdd = () => { setForm({ name: '', description: '' }); setAddOpen(true) }
  const openEdit = (cat: ServiceCategory) => { setEditing(cat); setForm({ name: cat.name, description: cat.description ?? '' }) }
  const closeDialog = () => { setAddOpen(false); setEditing(null) }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">Service categories for grouping car wash services</p>
        <Button size="sm" onClick={openAdd}><Plus className="mr-1 h-3.5 w-3.5" />Add</Button>
      </div>
      {isLoading ? <Skeleton className="h-40 w-full" /> : (
        <div className="rounded-lg border divide-y">
          {items.length === 0 && <p className="p-6 text-center text-sm text-muted-foreground">No categories yet</p>}
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between px-4 py-2.5">
              <div>
                <div className="flex items-center gap-2">
                  <span className="font-medium text-sm">{item.name}</span>
                  <StatusBadge status={item.isActive ? 'Active' : 'Inactive'} />
                </div>
                {item.description && <p className="text-xs text-muted-foreground">{item.description}</p>}
              </div>
              <div className="flex gap-2">
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => openEdit(item)}>
                  <Pencil className="h-3.5 w-3.5" />
                </Button>
                <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={() => toggle(item.id, { onSuccess: () => toast.success('Updated') })}>
                  {item.isActive ? 'Deactivate' : 'Activate'}
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      <Dialog open={addOpen || !!editing} onOpenChange={(v) => !v && closeDialog()}>
        <DialogContent className="sm:max-w-sm">
          <DialogHeader><DialogTitle>{editing ? 'Edit Category' : 'Add Category'}</DialogTitle></DialogHeader>
          <div className="space-y-3 py-2">
            <div className="space-y-1">
              <Label>Name <span className="text-destructive">*</span></Label>
              <Input value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} />
            </div>
            <div className="space-y-1">
              <Label>Description <span className="text-muted-foreground">(optional)</span></Label>
              <Textarea value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} rows={2} />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={closeDialog}>Cancel</Button>
            <Button onClick={handleSave} disabled={!form.name.trim() || creating || updating}>
              {(creating || updating) ? 'Saving…' : 'Save'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}

// ── Shift Config tab ──────────────────────────────────────────────────────────

function ShiftConfigTab() {
  const { data: settings, isLoading } = useShiftSettings()
  const { mutate: save, isPending: saving } = useUpdateShiftSettings()

  const [form, setForm] = useState({
    defaultOpeningFund: '',
    autoApproveThreshold: '',
    flagThreshold: '',
    requireShiftForTransactions: false,
    endOfDayReminderTime: '',
    lockTimeoutMinutes: '',
    maxPinAttempts: '',
  })
  const [initialized, setInitialized] = useState(false)

  if (settings && !initialized) {
    setForm({
      defaultOpeningFund: String(settings.defaultOpeningFund),
      autoApproveThreshold: String(settings.autoApproveThreshold),
      flagThreshold: String(settings.flagThreshold),
      requireShiftForTransactions: settings.requireShiftForTransactions,
      endOfDayReminderTime: settings.endOfDayReminderTime,
      lockTimeoutMinutes: String(settings.lockTimeoutMinutes),
      maxPinAttempts: String(settings.maxPinAttempts),
    })
    setInitialized(true)
  }

  const handleSave = () => {
    const payload = {
      defaultOpeningFund: parseFloat(form.defaultOpeningFund) || 0,
      autoApproveThreshold: parseFloat(form.autoApproveThreshold) || 0,
      flagThreshold: parseFloat(form.flagThreshold) || 0,
      requireShiftForTransactions: form.requireShiftForTransactions,
      endOfDayReminderTime: form.endOfDayReminderTime,
      lockTimeoutMinutes: parseInt(form.lockTimeoutMinutes) || 5,
      maxPinAttempts: parseInt(form.maxPinAttempts) || 5,
    }
    save(payload, {
      onSuccess: () => toast.success('Shift settings saved.'),
      onError: () => toast.error('Failed to save shift settings.'),
    })
  }

  if (isLoading) return <Skeleton className="h-64 w-full" />

  return (
    <div className="space-y-6 max-w-lg">
      <p className="text-sm text-muted-foreground">Configure cash drawer defaults and variance thresholds for cashier shifts.</p>

      <div className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="default-opening-fund">Default Opening Cash Fund (₱)</Label>
          <Input
            id="default-opening-fund"
            type="number"
            min="0"
            step="100"
            value={form.defaultOpeningFund}
            onChange={e => setForm(f => ({ ...f, defaultOpeningFund: e.target.value }))}
            placeholder="e.g. 2000"
          />
          <p className="text-xs text-muted-foreground">Pre-filled when a cashier opens a new shift.</p>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="auto-approve-threshold">Auto-Approve Threshold (₱)</Label>
          <Input
            id="auto-approve-threshold"
            type="number"
            min="0"
            step="10"
            value={form.autoApproveThreshold}
            onChange={e => setForm(f => ({ ...f, autoApproveThreshold: e.target.value }))}
            placeholder="e.g. 50"
          />
          <p className="text-xs text-muted-foreground">Shifts with |variance| ≤ this amount are automatically approved.</p>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="flag-threshold">Flag Threshold (₱)</Label>
          <Input
            id="flag-threshold"
            type="number"
            min="0"
            step="50"
            value={form.flagThreshold}
            onChange={e => setForm(f => ({ ...f, flagThreshold: e.target.value }))}
            placeholder="e.g. 200"
          />
          <p className="text-xs text-muted-foreground">Shifts with |variance| &gt; this amount are automatically flagged for review.</p>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="eod-reminder">End-of-Day Reminder Time</Label>
          <Input
            id="eod-reminder"
            type="time"
            value={form.endOfDayReminderTime}
            onChange={e => setForm(f => ({ ...f, endOfDayReminderTime: e.target.value }))}
          />
          <p className="text-xs text-muted-foreground">Cashiers will receive a reminder to close their shift at this time.</p>
        </div>

        <div className="flex items-start gap-3 rounded-lg border p-4">
          <input
            id="require-shift"
            type="checkbox"
            className="mt-0.5 h-4 w-4 accent-primary cursor-pointer"
            checked={form.requireShiftForTransactions}
            onChange={e => setForm(f => ({ ...f, requireShiftForTransactions: e.target.checked }))}
          />
          <div>
            <label htmlFor="require-shift" className="text-sm font-medium cursor-pointer">
              Require open shift for transactions
            </label>
            <p className="text-xs text-muted-foreground mt-0.5">
              When enabled, the POS will block new transactions unless the cashier has an open shift.
            </p>
          </div>
        </div>
        <div className="pt-2 border-t">
          <p className="text-sm font-medium mb-3">POS Lock Screen</p>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="lock-timeout">Auto-Lock Timeout (minutes)</Label>
          <Input
            id="lock-timeout"
            type="number"
            min="0"
            max="60"
            step="1"
            value={form.lockTimeoutMinutes}
            onChange={e => setForm(f => ({ ...f, lockTimeoutMinutes: e.target.value }))}
            placeholder="e.g. 5"
          />
          <p className="text-xs text-muted-foreground">Minutes of inactivity before the POS auto-locks. Set to 0 to disable.</p>
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="max-pin-attempts">Max PIN Attempts</Label>
          <Input
            id="max-pin-attempts"
            type="number"
            min="1"
            max="20"
            step="1"
            value={form.maxPinAttempts}
            onChange={e => setForm(f => ({ ...f, maxPinAttempts: e.target.value }))}
            placeholder="e.g. 5"
          />
          <p className="text-xs text-muted-foreground">Wrong PIN attempts before a 30-second cooldown.</p>
        </div>
      </div>

      <Button onClick={handleSave} disabled={saving}>
        {saving ? 'Saving…' : 'Save Shift Settings'}
      </Button>
    </div>
  )
}

// ── Payroll Templates tab ────────────────────────────────────────────────────

function PayrollTemplateDialog({
  open, onOpenChange, title, initial, onSave, isPending,
}: {
  open: boolean
  onOpenChange: (v: boolean) => void
  title: string
  initial?: PayrollAdjustmentTemplate
  onSave: (values: { name: string; type: AdjustmentType; defaultAmount: number; sortOrder: number }) => void
  isPending: boolean
}) {
  const [name, setName] = useState(initial?.name ?? '')
  const [type, setType] = useState<AdjustmentType>(initial?.type ?? AdjustmentType.Deduction)
  const [amount, setAmount] = useState(String(initial?.defaultAmount ?? ''))
  const [sortOrder, setSortOrder] = useState(String(initial?.sortOrder ?? '0'))

  const canSave = name.trim() && Number(amount) >= 0

  const handleSave = () => {
    if (!canSave) return
    onSave({ name: name.trim(), type, defaultAmount: Number(amount), sortOrder: Number(sortOrder) || 0 })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader><DialogTitle>{title}</DialogTitle></DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Name <span className="text-destructive">*</span></Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g. SSS Contribution" />
          </div>
          <div className="space-y-1.5">
            <Label>Type</Label>
            <Select value={String(type)} onValueChange={(v) => setType(Number(v) as AdjustmentType)}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value={String(AdjustmentType.Deduction)}>Deduction</SelectItem>
                <SelectItem value={String(AdjustmentType.Bonus)}>Bonus</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1.5">
            <Label>Default Amount (₱)</Label>
            <Input type="number" min="0" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)} placeholder="0.00" />
          </div>
          {initial && (
            <div className="space-y-1.5">
              <Label>Sort Order</Label>
              <Input type="number" min="0" value={sortOrder} onChange={(e) => setSortOrder(e.target.value)} />
            </div>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSave} disabled={!canSave || isPending}>
            {isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

const DAY_NAMES = [
  { value: '0', label: 'Sunday' },
  { value: '1', label: 'Monday' },
  { value: '2', label: 'Tuesday' },
  { value: '3', label: 'Wednesday' },
  { value: '4', label: 'Thursday' },
  { value: '5', label: 'Friday' },
  { value: '6', label: 'Saturday' },
]

function PayrollConfigSection() {
  const { data: settings, isLoading } = usePayrollSettings()
  const { mutate: save, isPending: saving } = useUpdatePayrollSettings()
  const [cutOffDay, setCutOffDay] = useState('')
  const [initialized, setInitialized] = useState(false)

  if (settings && !initialized) {
    setCutOffDay(String(settings.cutOffStartDay))
    setInitialized(true)
  }

  const handleSave = () => {
    save(
      { cutOffStartDay: parseInt(cutOffDay) },
      {
        onSuccess: () => toast.success('Payroll settings saved.'),
        onError: () => toast.error('Failed to save payroll settings.'),
      }
    )
  }

  if (isLoading) return <Skeleton className="h-32 w-full" />

  const endDay = DAY_NAMES[((parseInt(cutOffDay) || 1) + 6) % 7]

  return (
    <div className="space-y-4">
      <div>
        <h3 className="text-sm font-medium">Period Configuration</h3>
        <p className="text-xs text-muted-foreground mt-1">
          Configure when each 7-day payroll period begins. New periods are created automatically each week.
        </p>
      </div>
      <div className="grid gap-4 sm:grid-cols-2 max-w-lg">
        <div className="space-y-1.5">
          <Label>Cut-Off Start Day</Label>
          <Select value={cutOffDay} onValueChange={setCutOffDay}>
            <SelectTrigger><SelectValue /></SelectTrigger>
            <SelectContent>
              {DAY_NAMES.map((d) => (
                <SelectItem key={d.value} value={d.value}>{d.label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
          <p className="text-xs text-muted-foreground">
            Period runs {DAY_NAMES.find((d) => d.value === cutOffDay)?.label ?? 'Monday'} through {endDay?.label ?? 'Sunday'}.
          </p>
        </div>
      </div>
      <Button onClick={handleSave} disabled={saving} size="sm">
        {saving ? 'Saving…' : 'Save Payroll Settings'}
      </Button>
    </div>
  )
}

function PayrollTemplatesTab() {
  const { data: items = [], isLoading } = usePayrollTemplates()
  const { mutate: create, isPending: creating } = useCreatePayrollTemplate()
  const { mutate: update, isPending: updating } = useUpdatePayrollTemplate()
  const { mutate: toggleActive } = useDeletePayrollTemplate()
  const [addOpen, setAddOpen] = useState(false)
  const [editing, setEditing] = useState<PayrollAdjustmentTemplate | null>(null)

  const handleCreate = (values: { name: string; type: AdjustmentType; defaultAmount: number }) =>
    create(values, {
      onSuccess: () => { toast.success('Template created'); setAddOpen(false) },
      onError: () => toast.error('Failed to create template'),
    })

  const handleUpdate = (values: { name: string; type: AdjustmentType; defaultAmount: number; sortOrder: number }) => {
    if (!editing) return
    update({ id: editing.id, values }, {
      onSuccess: () => { toast.success('Updated'); setEditing(null) },
      onError: () => toast.error('Failed to update'),
    })
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          Reusable payroll adjustment presets (e.g. SSS, PhilHealth, Pag-IBIG, overtime) for quick bulk apply
        </p>
        <Button size="sm" onClick={() => setAddOpen(true)}><Plus className="mr-1 h-3.5 w-3.5" />Add Template</Button>
      </div>
      {isLoading ? <Skeleton className="h-40 w-full" /> : (
        <div className="rounded-lg border divide-y">
          {items.length === 0 && <p className="p-6 text-center text-sm text-muted-foreground">No payroll templates yet</p>}
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between px-4 py-2.5">
              <div className="flex items-center gap-3">
                <span className="font-medium text-sm">{item.name}</span>
                <span className={cn(
                  'inline-flex items-center rounded-full px-2 py-0.5 text-[11px] font-medium',
                  item.type === AdjustmentType.Bonus
                    ? 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-400'
                    : 'bg-red-500/15 text-red-700 dark:text-red-400'
                )}>
                  {item.type === AdjustmentType.Bonus ? 'Bonus' : 'Deduction'}
                </span>
                <span className="text-sm tabular-nums text-muted-foreground">{formatPeso(item.defaultAmount)}</span>
                <StatusBadge status={item.isActive ? 'Active' : 'Inactive'} />
              </div>
              <div className="flex gap-2">
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => setEditing(item)}>
                  <Pencil className="h-3.5 w-3.5" />
                </Button>
                <Button
                  variant="ghost" size="sm" className="h-7 text-xs"
                  onClick={() => toggleActive(item.id, { onSuccess: () => toast.success('Updated') })}
                >
                  {item.isActive ? 'Deactivate' : 'Activate'}
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}
      <PayrollTemplateDialog open={addOpen} onOpenChange={setAddOpen} title="Add Payroll Template" onSave={handleCreate} isPending={creating} />
      {editing && (
        <PayrollTemplateDialog
          open
          onOpenChange={(v) => !v && setEditing(null)}
          title="Edit Payroll Template"
          initial={editing}
          onSave={handleUpdate}
          isPending={updating}
        />
      )}
    </div>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function SettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Settings</h1>
        <p className="text-muted-foreground">Configure vehicle types, sizes, makes, categories, and shift settings</p>
      </div>

      <Tabs defaultValue="vehicle-types">
        <TabsList variant="line" className="flex-wrap h-auto">
          <TabsTrigger value="vehicle-types">Vehicle Types</TabsTrigger>
          <TabsTrigger value="sizes">Sizes</TabsTrigger>
          <TabsTrigger value="makes-models">Makes & Models</TabsTrigger>
          <TabsTrigger value="categories">Categories</TabsTrigger>
          <TabsTrigger value="shift-config">Cash Drawer</TabsTrigger>
          <TabsTrigger value="payroll">Payroll</TabsTrigger>
        </TabsList>

        <TabsContent value="vehicle-types" className="mt-6"><VehicleTypesTab /></TabsContent>
        <TabsContent value="sizes" className="mt-6"><SizesTab /></TabsContent>
        <TabsContent value="makes-models" className="mt-6"><MakesModelsTab /></TabsContent>
        <TabsContent value="categories" className="mt-6"><CategoriesTab /></TabsContent>
        <TabsContent value="shift-config" className="mt-6"><ShiftConfigTab /></TabsContent>
        <TabsContent value="payroll" className="mt-6">
          <PayrollConfigSection />
          <div className="my-6 border-t" />
          <PayrollTemplatesTab />
        </TabsContent>
      </Tabs>
    </div>
  )
}
