'use client'

import { useState, useEffect } from 'react'
import { Plus, Pencil, Truck } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import { EmptyState } from '@/components/ui/empty-state'
import { StatusBadge } from '@/components/ui/status-badge'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter,
} from '@/components/ui/dialog'
import { useSuppliers, useCreateSupplier, useUpdateSupplier } from '@/hooks/use-suppliers'
import { toast } from 'sonner'

// ── Main Page ────────────────────────────────────────────────────────────────

export default function SuppliersPage() {
  const [createOpen, setCreateOpen] = useState(false)
  const [editSupplier, setEditSupplier] = useState<SupplierRow | null>(null)

  const { data, isLoading } = useSuppliers()
  const suppliers = data ?? []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Suppliers</h1>
          <p className="text-sm text-muted-foreground">Manage your supply vendors and contacts</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" /> Add Supplier
        </Button>
      </div>

      {/* Table */}
      {isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : suppliers.length > 0 ? (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Name</th>
                <th className="px-4 py-2.5 text-left font-medium">Contact Person</th>
                <th className="px-4 py-2.5 text-left font-medium">Phone</th>
                <th className="px-4 py-2.5 text-left font-medium">Email</th>
                <th className="px-4 py-2.5 text-left font-medium">Status</th>
                <th className="px-4 py-2.5 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {suppliers.map((s) => (
                <tr key={s.id} className="hover:bg-muted/30 cursor-pointer" onClick={() => setEditSupplier(s)}>
                  <td className="px-4 py-2 font-medium">{s.name}</td>
                  <td className="px-4 py-2 text-muted-foreground">{s.contactPerson || '—'}</td>
                  <td className="px-4 py-2 text-muted-foreground">{s.phone || '—'}</td>
                  <td className="px-4 py-2 text-muted-foreground">{s.email || '—'}</td>
                  <td className="px-4 py-2">
                    <StatusBadge status={s.isActive ? 'Active' : 'Inactive'} />
                  </td>
                  <td className="px-4 py-2 text-right">
                    <Button variant="ghost" size="sm" className="h-7 text-xs"
                      onClick={(e) => { e.stopPropagation(); setEditSupplier(s) }}>
                      <Pencil className="h-3 w-3" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <EmptyState
          icon={Truck}
          title="No suppliers"
          description="Add your first supplier to manage vendor contacts."
          action={{ label: 'Add Supplier', onClick: () => setCreateOpen(true), icon: Plus }}
        />
      )}

      <CreateSupplierDialog open={createOpen} onOpenChange={setCreateOpen} />
      <EditSupplierDialog supplier={editSupplier} onOpenChange={(v) => { if (!v) setEditSupplier(null) }} />
    </div>
  )
}

// ── Types ────────────────────────────────────────────────────────────────────

interface SupplierRow {
  id: string
  name: string
  contactPerson: string | null
  phone: string | null
  email: string | null
  address: string | null
  isActive: boolean
}

// ── Create Supplier Dialog ───────────────────────────────────────────────────

function CreateSupplierDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (v: boolean) => void }) {
  const { mutate: create, isPending } = useCreateSupplier()

  const [name, setName] = useState('')
  const [contactPerson, setContactPerson] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [address, setAddress] = useState('')
  const [notes, setNotes] = useState('')

  const resetForm = () => {
    setName(''); setContactPerson(''); setPhone(''); setEmail(''); setAddress(''); setNotes('')
  }

  const handleSubmit = () => {
    if (!name) return
    create({
      name,
      contactPerson: contactPerson || undefined,
      phone: phone || undefined,
      email: email || undefined,
      address: address || undefined,
    }, {
      onSuccess: () => {
        toast.success(`Supplier "${name}" added`)
        onOpenChange(false)
        resetForm()
      },
      onError: () => toast.error('Failed to add supplier.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add Supplier</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Name <span className="text-destructive">*</span></Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="Supplier name" autoFocus />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Contact Person</Label>
              <Input value={contactPerson} onChange={(e) => setContactPerson(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Phone</Label>
              <Input value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="09XX XXX XXXX" />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Email</Label>
            <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>Address</Label>
            <Textarea value={address} onChange={(e) => setAddress(e.target.value)} rows={2} />
          </div>
          <div className="space-y-1.5">
            <Label>Notes</Label>
            <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !name}>
            {isPending ? 'Adding...' : 'Add Supplier'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Edit Supplier Dialog ─────────────────────────────────────────────────────

function EditSupplierDialog({ supplier, onOpenChange }: { supplier: SupplierRow | null; onOpenChange: (v: boolean) => void }) {
  const { mutate: update, isPending } = useUpdateSupplier()

  const [name, setName] = useState('')
  const [contactPerson, setContactPerson] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [address, setAddress] = useState('')
  const [notes, setNotes] = useState('')

  useEffect(() => {
    if (supplier) {
      setName(supplier.name)
      setContactPerson(supplier.contactPerson ?? '')
      setPhone(supplier.phone ?? '')
      setEmail(supplier.email ?? '')
      setAddress(supplier.address ?? '')
      setNotes('')
    }
  }, [supplier])

  const handleSubmit = () => {
    if (!supplier || !name) return
    update({
      id: supplier.id,
      name,
      contactPerson: contactPerson || undefined,
      phone: phone || undefined,
      email: email || undefined,
      address: address || undefined,
    }, {
      onSuccess: () => {
        toast.success('Supplier updated')
        onOpenChange(false)
      },
      onError: () => toast.error('Failed to update supplier.'),
    })
  }

  return (
    <Dialog open={!!supplier} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Edit Supplier</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Name <span className="text-destructive">*</span></Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} autoFocus />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Contact Person</Label>
              <Input value={contactPerson} onChange={(e) => setContactPerson(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Phone</Label>
              <Input value={phone} onChange={(e) => setPhone(e.target.value)} />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Email</Label>
            <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>Address</Label>
            <Textarea value={address} onChange={(e) => setAddress(e.target.value)} rows={2} />
          </div>
          <div className="space-y-1.5">
            <Label>Notes</Label>
            <Textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !name}>
            {isPending ? 'Saving...' : 'Save Changes'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
