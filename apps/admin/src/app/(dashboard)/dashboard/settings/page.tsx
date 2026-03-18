'use client'

import { useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
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
import {
  usePricingModifiers, useCreatePricingModifier, useUpdatePricingModifier,
  useDeletePricingModifier, useTogglePricingModifier,
} from '@/hooks/use-pricing-modifiers'
import type { PricingModifier, VehicleType, Size, Make, VehicleModel, ServiceCategory } from '@splashsphere/types'
import { ModifierType } from '@splashsphere/types'
import { useBranches } from '@/hooks/use-branches'
import { toast } from 'sonner'
import { cn } from '@/lib/utils'

const php = new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' })

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
          <Label>Name</Label>
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
                <Badge variant={item.isActive ? 'default' : 'secondary'} className={cn('text-xs', item.isActive && 'bg-green-500/15 text-green-700 border-green-200')}>
                  {item.isActive ? 'Active' : 'Inactive'}
                </Badge>
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
                <Badge variant={item.isActive ? 'default' : 'secondary'} className={cn('text-xs', item.isActive && 'bg-green-500/15 text-green-700 border-green-200')}>
                  {item.isActive ? 'Active' : 'Inactive'}
                </Badge>
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
                  {!make.isActive && <Badge variant="secondary" className="text-xs">Inactive</Badge>}
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
                  {!model.isActive && <Badge variant="secondary" className="text-xs">Inactive</Badge>}
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
                  <Badge variant={item.isActive ? 'default' : 'secondary'} className={cn('text-xs', item.isActive && 'bg-green-500/15 text-green-700 border-green-200')}>
                    {item.isActive ? 'Active' : 'Inactive'}
                  </Badge>
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
              <Label>Name</Label>
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

// ── Pricing Modifiers tab ─────────────────────────────────────────────────────

const MODIFIER_TYPE_LABELS: Record<ModifierType, string> = {
  [ModifierType.PeakHour]: 'Peak Hour',
  [ModifierType.DayOfWeek]: 'Day of Week',
  [ModifierType.Holiday]: 'Holiday',
  [ModifierType.Promotion]: 'Promotion',
  [ModifierType.Weather]: 'Weather',
}

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']

function modifierSubtitle(m: PricingModifier): string {
  if (m.type === ModifierType.PeakHour && m.startTime && m.endTime)
    return `${m.startTime.slice(0, 5)} – ${m.endTime.slice(0, 5)}`
  if (m.type === ModifierType.DayOfWeek && m.activeDayOfWeek != null)
    return DAY_NAMES[m.activeDayOfWeek]
  if (m.type === ModifierType.Holiday && m.holidayDate)
    return `${m.holidayName ?? ''} (${m.holidayDate})`
  if (m.type === ModifierType.Promotion && m.startDate && m.endDate)
    return `${m.startDate} – ${m.endDate}`
  return ''
}

type ModifierForm = {
  name: string
  type: ModifierType
  value: string
  branchId: string
  startTime: string
  endTime: string
  activeDayOfWeek: string
  holidayDate: string
  holidayName: string
  startDate: string
  endDate: string
}

const emptyModifierForm: ModifierForm = {
  name: '', type: ModifierType.PeakHour, value: '',
  branchId: '', startTime: '', endTime: '',
  activeDayOfWeek: '', holidayDate: '', holidayName: '',
  startDate: '', endDate: '',
}

function PricingModifiersTab() {
  const { data: items = [], isLoading } = usePricingModifiers()
  const { mutate: create, isPending: creating } = useCreatePricingModifier()
  const { mutate: toggle } = useTogglePricingModifier()
  const { mutate: remove } = useDeletePricingModifier()
  const [editingId, setEditingId] = useState<string | null>(null)
  const { mutate: update, isPending: updating } = useUpdatePricingModifier(editingId ?? '')
  const { data: branches = [] } = useBranches()

  const [dialogOpen, setDialogOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null)
  const [form, setForm] = useState<ModifierForm>(emptyModifierForm)

  const f = (key: keyof ModifierForm) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((prev) => ({ ...prev, [key]: e.target.value }))

  const openAdd = () => { setEditingId(null); setForm(emptyModifierForm); setDialogOpen(true) }
  const openEdit = (item: PricingModifier) => {
    setEditingId(item.id)
    setForm({
      name: item.name,
      type: item.type,
      value: String(item.value),
      branchId: item.branchId ?? '',
      startTime: item.startTime?.slice(0, 5) ?? '',
      endTime: item.endTime?.slice(0, 5) ?? '',
      activeDayOfWeek: item.activeDayOfWeek != null ? String(item.activeDayOfWeek) : '',
      holidayDate: item.holidayDate ?? '',
      holidayName: item.holidayName ?? '',
      startDate: item.startDate ?? '',
      endDate: item.endDate ?? '',
    })
    setDialogOpen(true)
  }

  const buildPayload = () => ({
    name: form.name.trim(),
    type: form.type,
    value: parseFloat(form.value),
    branchId: form.branchId || undefined,
    startTime: form.type === ModifierType.PeakHour ? form.startTime || undefined : undefined,
    endTime: form.type === ModifierType.PeakHour ? form.endTime || undefined : undefined,
    activeDayOfWeek: form.type === ModifierType.DayOfWeek && form.activeDayOfWeek !== '' ? parseInt(form.activeDayOfWeek) : undefined,
    holidayDate: form.type === ModifierType.Holiday ? form.holidayDate || undefined : undefined,
    holidayName: form.type === ModifierType.Holiday ? form.holidayName || undefined : undefined,
    startDate: form.type === ModifierType.Promotion ? form.startDate || undefined : undefined,
    endDate: form.type === ModifierType.Promotion ? form.endDate || undefined : undefined,
  })

  const handleSave = () => {
    const payload = buildPayload()
    if (editingId) {
      update(payload, { onSuccess: () => { toast.success('Updated'); setDialogOpen(false) }, onError: () => toast.error('Failed') })
    } else {
      create(payload, { onSuccess: () => { toast.success('Pricing modifier created'); setDialogOpen(false) }, onError: () => toast.error('Failed') })
    }
  }

  const handleDelete = (id: string) => remove(id, {
    onSuccess: () => { toast.success('Deleted'); setDeleteTarget(null) },
    onError: () => toast.error('Failed to delete'),
  })

  const isFormValid = form.name.trim() && form.value && !isNaN(parseFloat(form.value))

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">Price adjustment rules for peak hours, holidays, and promotions</p>
        <Button size="sm" onClick={openAdd}><Plus className="mr-1 h-3.5 w-3.5" />Add</Button>
      </div>

      {isLoading ? <Skeleton className="h-40 w-full" /> : (
        <div className="rounded-lg border divide-y">
          {items.length === 0 && <p className="p-6 text-center text-sm text-muted-foreground">No pricing modifiers yet</p>}
          {items.map((item) => (
            <div key={item.id} className="flex items-center justify-between px-4 py-3">
              <div>
                <div className="flex items-center gap-2 flex-wrap">
                  <span className="font-medium text-sm">{item.name}</span>
                  <Badge variant="outline" className="text-xs">{MODIFIER_TYPE_LABELS[item.type]}</Badge>
                  <span className="text-sm font-semibold tabular-nums">{item.value}×</span>
                  {item.branchName && <span className="text-xs text-muted-foreground">{item.branchName}</span>}
                  <Badge variant={item.isActive ? 'default' : 'secondary'} className={cn('text-xs', item.isActive && 'bg-green-500/15 text-green-700 border-green-200')}>
                    {item.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </div>
                {modifierSubtitle(item) && (
                  <p className="text-xs text-muted-foreground mt-0.5">{modifierSubtitle(item)}</p>
                )}
              </div>
              <div className="flex gap-1 shrink-0">
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => openEdit(item)}>
                  <Pencil className="h-3.5 w-3.5" />
                </Button>
                <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={() => toggle(item.id, { onSuccess: () => toast.success('Updated') })}>
                  {item.isActive ? 'Deactivate' : 'Activate'}
                </Button>
                <Button variant="ghost" size="icon" className="h-7 w-7 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(item.id)}>
                  <Trash2 className="h-3.5 w-3.5" />
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add/Edit dialog */}
      <Dialog open={dialogOpen} onOpenChange={(v) => !v && setDialogOpen(false)}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader><DialogTitle>{editingId ? 'Edit Pricing Modifier' : 'Add Pricing Modifier'}</DialogTitle></DialogHeader>
          <div className="space-y-3 py-2 max-h-[60vh] overflow-y-auto pr-1">
            <div className="space-y-1">
              <Label>Name</Label>
              <Input value={form.name} onChange={f('name')} placeholder="e.g. Weekend Surcharge" />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label>Type</Label>
                <Select value={String(form.type)} onValueChange={(v) => setForm((p) => ({ ...p, type: parseInt(v) as ModifierType }))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {Object.entries(MODIFIER_TYPE_LABELS).map(([k, v]) => (
                      <SelectItem key={k} value={k}>{v}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1">
                <Label>Multiplier value</Label>
                <Input type="number" step="0.01" min="0" value={form.value} onChange={f('value')} placeholder="e.g. 1.2" />
              </div>
            </div>
            <div className="space-y-1">
              <Label>Branch <span className="text-muted-foreground">(leave blank for all)</span></Label>
              <Select value={form.branchId || '__all__'} onValueChange={(v) => setForm((p) => ({ ...p, branchId: v === '__all__' ? '' : v }))}>
                <SelectTrigger><SelectValue placeholder="All branches" /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="__all__">All branches</SelectItem>
                  {branches.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>

            {form.type === ModifierType.PeakHour && (
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-1">
                  <Label>Start time</Label>
                  <Input type="time" value={form.startTime} onChange={f('startTime')} />
                </div>
                <div className="space-y-1">
                  <Label>End time</Label>
                  <Input type="time" value={form.endTime} onChange={f('endTime')} />
                </div>
              </div>
            )}

            {form.type === ModifierType.DayOfWeek && (
              <div className="space-y-1">
                <Label>Day of week</Label>
                <Select value={form.activeDayOfWeek} onValueChange={(v) => setForm((p) => ({ ...p, activeDayOfWeek: v }))}>
                  <SelectTrigger><SelectValue placeholder="Select day" /></SelectTrigger>
                  <SelectContent>
                    {DAY_NAMES.map((d, i) => <SelectItem key={i} value={String(i)}>{d}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
            )}

            {form.type === ModifierType.Holiday && (
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-1">
                  <Label>Holiday name</Label>
                  <Input value={form.holidayName} onChange={f('holidayName')} placeholder="e.g. Christmas" />
                </div>
                <div className="space-y-1">
                  <Label>Date</Label>
                  <Input type="date" value={form.holidayDate} onChange={f('holidayDate')} />
                </div>
              </div>
            )}

            {form.type === ModifierType.Promotion && (
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-1">
                  <Label>Start date</Label>
                  <Input type="date" value={form.startDate} onChange={f('startDate')} />
                </div>
                <div className="space-y-1">
                  <Label>End date</Label>
                  <Input type="date" value={form.endDate} onChange={f('endDate')} />
                </div>
              </div>
            )}
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleSave} disabled={!isFormValid || creating || updating}>
              {(creating || updating) ? 'Saving…' : 'Save'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete confirmation */}
      <AlertDialog open={!!deleteTarget} onOpenChange={(v) => !v && setDeleteTarget(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete pricing modifier?</AlertDialogTitle>
            <AlertDialogDescription>This action cannot be undone.</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction className="bg-destructive text-destructive-foreground hover:bg-destructive/90" onClick={() => deleteTarget && handleDelete(deleteTarget)}>
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function SettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Settings</h1>
        <p className="text-muted-foreground">Configure vehicle types, sizes, makes, categories, and pricing rules</p>
      </div>

      <Tabs defaultValue="vehicle-types">
        <TabsList className="flex-wrap h-auto">
          <TabsTrigger value="vehicle-types">Vehicle Types</TabsTrigger>
          <TabsTrigger value="sizes">Sizes</TabsTrigger>
          <TabsTrigger value="makes-models">Makes & Models</TabsTrigger>
          <TabsTrigger value="categories">Categories</TabsTrigger>
          <TabsTrigger value="pricing-modifiers">Pricing Modifiers</TabsTrigger>
        </TabsList>

        <TabsContent value="vehicle-types" className="mt-6"><VehicleTypesTab /></TabsContent>
        <TabsContent value="sizes" className="mt-6"><SizesTab /></TabsContent>
        <TabsContent value="makes-models" className="mt-6"><MakesModelsTab /></TabsContent>
        <TabsContent value="categories" className="mt-6"><CategoriesTab /></TabsContent>
        <TabsContent value="pricing-modifiers" className="mt-6"><PricingModifiersTab /></TabsContent>
      </Tabs>
    </div>
  )
}
