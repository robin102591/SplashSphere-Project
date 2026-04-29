'use client'

import { useState } from 'react'
import { Plus, Pencil, Trash2, MonitorSmartphone } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog'
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
  AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { useBranches } from '@/hooks/use-branches'
import {
  usePosStations,
  useCreatePosStation,
  useUpdatePosStation,
  useDeletePosStation,
} from '@/hooks/use-pos-stations'
import type { PosStation } from '@splashsphere/types'
import { toast } from 'sonner'

export default function PosStationsPage() {
  const { data: branches = [], isLoading: branchesLoading } = useBranches()
  const [branchId, setBranchId] = useState<string>('')

  // Default-pick the first branch once it loads.
  if (!branchId && branches.length > 0) {
    setBranchId(branches[0].id)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
          <MonitorSmartphone className="h-6 w-6" />
          POS Stations
        </h1>
        <p className="text-muted-foreground">
          Each station pairs one cashier device with one optional customer display.
          Use multiple stations when a branch runs more than one POS at the same time.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Branch</CardTitle>
          <CardDescription>Stations are scoped per branch.</CardDescription>
        </CardHeader>
        <CardContent>
          {branchesLoading ? (
            <Skeleton className="h-10 w-64" />
          ) : branches.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No branches yet. Create a branch first under Branches.
            </p>
          ) : (
            <Select value={branchId} onValueChange={setBranchId}>
              <SelectTrigger className="w-[280px]">
                <SelectValue placeholder="Select a branch" />
              </SelectTrigger>
              <SelectContent>
                {branches.map((b) => (
                  <SelectItem key={b.id} value={b.id}>
                    {b.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        </CardContent>
      </Card>

      {branchId && <StationList branchId={branchId} />}
    </div>
  )
}

function StationList({ branchId }: { branchId: string }) {
  const { data: stations = [], isLoading } = usePosStations(branchId)
  const { mutate: create, isPending: creating } = useCreatePosStation(branchId)
  const { mutate: update, isPending: updating } = useUpdatePosStation(branchId)
  const { mutate: remove } = useDeletePosStation(branchId)

  const [addOpen, setAddOpen] = useState(false)
  const [editing, setEditing] = useState<PosStation | null>(null)
  const [deleting, setDeleting] = useState<PosStation | null>(null)

  const handleCreate = (name: string) =>
    create(
      { name },
      {
        onSuccess: () => {
          toast.success('Station created')
          setAddOpen(false)
        },
        onError: (err: Error) => toast.error(err.message ?? 'Failed to create station'),
      },
    )

  const handleUpdate = (name: string, isActive: boolean) => {
    if (!editing) return
    update(
      { id: editing.id, name, isActive },
      {
        onSuccess: () => {
          toast.success('Station updated')
          setEditing(null)
        },
        onError: (err: Error) => toast.error(err.message ?? 'Failed to update station'),
      },
    )
  }

  const handleDelete = () => {
    if (!deleting) return
    remove(deleting.id, {
      onSuccess: () => {
        toast.success('Station deleted')
        setDeleting(null)
      },
      onError: (err: Error) => toast.error(err.message ?? 'Failed to delete station'),
    })
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Stations</CardTitle>
          <CardDescription>One row per cashier workstation in this branch.</CardDescription>
        </div>
        <Button size="sm" onClick={() => setAddOpen(true)}>
          <Plus className="mr-1 h-3.5 w-3.5" />
          Add Station
        </Button>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <Skeleton className="h-32 w-full" />
        ) : stations.length === 0 ? (
          <div className="rounded-lg border border-dashed p-10 text-center text-sm text-muted-foreground">
            No stations yet. Add one to get started.
          </div>
        ) : (
          <div className="rounded-lg border divide-y">
            {stations.map((station) => (
              <div key={station.id} className="flex items-center justify-between px-4 py-2.5">
                <div className="flex items-center gap-3">
                  <span className="font-medium text-sm">{station.name}</span>
                  <StatusBadge status={station.isActive ? 'Active' : 'Inactive'} />
                </div>
                <div className="flex gap-1">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-7 w-7"
                    onClick={() => setEditing(station)}
                  >
                    <Pencil className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-7 w-7 text-destructive hover:text-destructive"
                    onClick={() => setDeleting(station)}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>

      <StationDialog
        key={`add-${addOpen}`}
        open={addOpen}
        onOpenChange={setAddOpen}
        title="Add Station"
        onSave={(name) => handleCreate(name)}
        isPending={creating}
      />

      {editing && (
        <StationDialog
          key={`edit-${editing.id}`}
          open
          onOpenChange={(v) => !v && setEditing(null)}
          title="Edit Station"
          initialName={editing.name}
          initialActive={editing.isActive}
          showActiveToggle
          onSave={(name, isActive) => handleUpdate(name, isActive)}
          isPending={updating}
        />
      )}

      <AlertDialog open={!!deleting} onOpenChange={(v) => !v && setDeleting(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete this station?</AlertDialogTitle>
            <AlertDialogDescription>
              {deleting?.name} will be removed. Customer displays paired with this station
              will return to the setup screen the next time they reconnect.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete}>Delete</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </Card>
  )
}

function StationDialog({
  open,
  onOpenChange,
  title,
  initialName,
  initialActive,
  showActiveToggle,
  onSave,
  isPending,
}: {
  open: boolean
  onOpenChange: (v: boolean) => void
  title: string
  initialName?: string
  initialActive?: boolean
  showActiveToggle?: boolean
  onSave: (name: string, isActive: boolean) => void
  isPending: boolean
}) {
  const [name, setName] = useState(initialName ?? '')
  const [isActive, setIsActive] = useState(initialActive ?? true)

  const canSave = name.trim().length > 0

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label htmlFor="station-name">
              Name <span className="text-destructive">*</span>
            </Label>
            <Input
              id="station-name"
              value={name}
              maxLength={100}
              onChange={(e) => setName(e.target.value)}
              placeholder="Counter A"
              onKeyDown={(e) => {
                if (e.key === 'Enter' && canSave) onSave(name.trim(), isActive)
              }}
            />
          </div>
          {showActiveToggle && (
            <div className="flex items-center gap-2 rounded-lg border p-3">
              <input
                id="station-active"
                type="checkbox"
                className="h-4 w-4 accent-primary cursor-pointer"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
              />
              <Label htmlFor="station-active" className="cursor-pointer text-sm">
                Active
              </Label>
            </div>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={() => onSave(name.trim(), isActive)} disabled={!canSave || isPending}>
            {isPending ? 'Saving…' : 'Save'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
