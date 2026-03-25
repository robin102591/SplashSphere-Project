'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Plus, Users, Search } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/ui/empty-state'
import { Input } from '@/components/ui/input'
import { StatusBadge } from '@/components/ui/status-badge'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'
import { useCustomers, useCreateCustomer } from '@/hooks/use-customers'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

// ── Create dialog ─────────────────────────────────────────────────────────────

const schema = z.object({
  firstName: z.string().min(1, 'Required'),
  lastName: z.string().min(1, 'Required'),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  contactNumber: z.string().optional(),
  notes: z.string().optional(),
})
type FormValues = z.infer<typeof schema>

function CreateCustomerDialog({
  open,
  onOpenChange,
}: {
  open: boolean
  onOpenChange: (o: boolean) => void
}) {
  const router = useRouter()
  const { mutateAsync: create } = useCreateCustomer()
  const { register, handleSubmit, reset, formState } = useForm<FormValues>({
    resolver: zodResolver(schema),
  })

  const onSubmit = async (values: FormValues) => {
    try {
      const { id } = await create({
        ...values,
        email: values.email || undefined,
        contactNumber: values.contactNumber || undefined,
        notes: values.notes || undefined,
      })
      toast.success('Customer created')
      reset()
      onOpenChange(false)
      router.push(`/dashboard/customers/${id}`)
    } catch {
      toast.error('Failed to create customer')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>New Customer</DialogTitle>
          <DialogDescription>Add a new customer profile.</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>First name</Label>
              <Input placeholder="Juan" {...register('firstName')} />
              {formState.errors.firstName && (
                <p className="text-xs text-destructive">{formState.errors.firstName.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label>Last name</Label>
              <Input placeholder="dela Cruz" {...register('lastName')} />
              {formState.errors.lastName && (
                <p className="text-xs text-destructive">{formState.errors.lastName.message}</p>
              )}
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Email (optional)</Label>
            <Input type="email" placeholder="juan@example.com" {...register('email')} />
            {formState.errors.email && (
              <p className="text-xs text-destructive">{formState.errors.email.message}</p>
            )}
          </div>
          <div className="space-y-1.5">
            <Label>Contact number (optional)</Label>
            <Input placeholder="09XXXXXXXXX" {...register('contactNumber')} />
          </div>
          <div className="space-y-1.5">
            <Label>Notes (optional)</Label>
            <Input placeholder="VIP, prefers Sedan wash…" {...register('notes')} />
          </div>
          <div className="flex justify-end pt-1">
            <Button type="submit" disabled={formState.isSubmitting}>
              {formState.isSubmitting ? 'Saving…' : 'Create Customer'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function CustomersPage() {
  const router = useRouter()
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [createOpen, setCreateOpen] = useState(false)

  const { data, isLoading, isError } = useCustomers({ search: debouncedSearch, pageSize: 50 })
  const customers = data ? [...data.items] : []

  const handleSearchChange = (value: string) => {
    setSearch(value)
    clearTimeout((handleSearchChange as { _t?: ReturnType<typeof setTimeout> })._t)
    ;(handleSearchChange as { _t?: ReturnType<typeof setTimeout> })._t = setTimeout(
      () => setDebouncedSearch(value),
      300
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Customers</h1>
          <p className="text-muted-foreground">View customer profiles and vehicle history</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          New Customer
        </Button>
      </div>

      <div className="relative max-w-sm">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          className="pl-9"
          placeholder="Search by name, email…"
          value={search}
          onChange={(e) => handleSearchChange(e.target.value)}
        />
      </div>

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
          Failed to load customers.
        </div>
      )}

      {!isLoading && !isError && customers.length === 0 && (
        <EmptyState
          icon={Users}
          title="No customers found"
          description={debouncedSearch ? 'Try a different search term' : 'Add your first customer'}
          action={
            !debouncedSearch
              ? { label: 'New Customer', onClick: () => setCreateOpen(true), icon: Plus }
              : undefined
          }
        />
      )}

      {!isLoading && customers.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Name</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Email</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Contact</th>
                <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {customers.map((c) => (
                <tr
                  key={c.id}
                  className="hover:bg-muted/40 cursor-pointer transition-colors"
                  onClick={() => router.push(`/dashboard/customers/${c.id}`)}
                >
                  <td className="px-4 py-3 font-medium">{c.fullName}</td>
                  <td className="px-4 py-3 text-muted-foreground">{c.email ?? '—'}</td>
                  <td className="px-4 py-3 text-muted-foreground">{c.contactNumber ?? '—'}</td>
                  <td className="px-4 py-3">
                    <StatusBadge status={c.isActive ? 'Active' : 'Inactive'} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {data && (
            <div className="px-4 py-3 text-sm text-muted-foreground border-t">
              Showing 1–{customers.length} of {data.totalCount} results
            </div>
          )}
        </div>
      )}

      <CreateCustomerDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  )
}
