'use client'

import { use, useState } from 'react'
import { useRouter } from 'next/navigation'
import {
  ArrowLeft, Pencil, Power, PowerOff, Car, Mail, Phone, FileText, Plus,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { StatusBadge } from '@/components/ui/status-badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription,
} from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'
import { Input } from '@/components/ui/input'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  useCustomer, useUpdateCustomer, useToggleCustomerStatus,
} from '@/hooks/use-customers'
import { useCreateCar, useMakes, useModelsByMake } from '@/hooks/use-cars'
import { useVehicleTypes } from '@/hooks/use-vehicle-types'
import { useSizes } from '@/hooks/use-sizes'
import type { CustomerDetail, CustomerCar } from '@splashsphere/types'
import { toast } from 'sonner'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

// ── Edit customer form ────────────────────────────────────────────────────────

const editSchema = z.object({
  firstName: z.string().min(1, 'Required'),
  lastName: z.string().min(1, 'Required'),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  contactNumber: z.string().optional(),
  notes: z.string().optional(),
})
type EditFormValues = z.infer<typeof editSchema>

function EditCustomerForm({
  customer,
  onSubmit,
}: {
  customer: CustomerDetail
  onSubmit: (v: EditFormValues) => Promise<void>
}) {
  const { register, handleSubmit, formState } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    defaultValues: {
      firstName: customer.firstName,
      lastName: customer.lastName,
      email: customer.email ?? '',
      contactNumber: customer.contactNumber ?? '',
      notes: customer.notes ?? '',
    },
  })
  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1.5">
          <Label>First name</Label>
          <Input {...register('firstName')} />
          {formState.errors.firstName && (
            <p className="text-xs text-destructive">{formState.errors.firstName.message}</p>
          )}
        </div>
        <div className="space-y-1.5">
          <Label>Last name</Label>
          <Input {...register('lastName')} />
          {formState.errors.lastName && (
            <p className="text-xs text-destructive">{formState.errors.lastName.message}</p>
          )}
        </div>
      </div>
      <div className="space-y-1.5">
        <Label>Email</Label>
        <Input type="email" {...register('email')} />
        {formState.errors.email && (
          <p className="text-xs text-destructive">{formState.errors.email.message}</p>
        )}
      </div>
      <div className="space-y-1.5">
        <Label>Contact number</Label>
        <Input {...register('contactNumber')} />
      </div>
      <div className="space-y-1.5">
        <Label>Notes</Label>
        <Input {...register('notes')} />
      </div>
      <div className="flex justify-end pt-1">
        <Button type="submit" disabled={formState.isSubmitting}>
          {formState.isSubmitting ? 'Saving…' : 'Save Changes'}
        </Button>
      </div>
    </form>
  )
}

// ── Add car dialog ────────────────────────────────────────────────────────────

const carSchema = z.object({
  plateNumber: z.string().min(1, 'Plate number is required').toUpperCase(),
  vehicleTypeId: z.string().min(1, 'Required'),
  sizeId: z.string().min(1, 'Required'),
  makeId: z.string().optional(),
  modelId: z.string().optional(),
  color: z.string().optional(),
  year: z.coerce.number().int().min(1900).max(2100).optional().or(z.literal('')),
  notes: z.string().optional(),
})
type CarFormValues = z.infer<typeof carSchema>

function AddCarDialog({
  customerId,
  open,
  onOpenChange,
}: {
  customerId: string
  open: boolean
  onOpenChange: (o: boolean) => void
}) {
  const { mutateAsync: createCar } = useCreateCar()
  const { data: vehicleTypes = [] } = useVehicleTypes()
  const { data: sizes = [] } = useSizes()
  const { data: makes = [] } = useMakes()

  const { register, handleSubmit, watch, setValue, reset, formState } = useForm<CarFormValues>({
    resolver: zodResolver(carSchema),
  })

  const selectedMakeId = watch('makeId')
  const { data: models = [] } = useModelsByMake(selectedMakeId)

  const onSubmit = async (values: CarFormValues) => {
    try {
      await createCar({
        plateNumber: values.plateNumber,
        vehicleTypeId: values.vehicleTypeId,
        sizeId: values.sizeId,
        customerId,
        makeId: values.makeId || undefined,
        modelId: values.modelId || undefined,
        color: values.color || undefined,
        year: values.year ? Number(values.year) : undefined,
        notes: values.notes || undefined,
      })
      toast.success('Vehicle registered')
      reset()
      onOpenChange(false)
    } catch {
      toast.error('Failed to register vehicle')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md overflow-y-auto max-h-[90vh]">
        <DialogHeader>
          <DialogTitle>Register Vehicle</DialogTitle>
          <DialogDescription>Link a vehicle to this customer.</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label>Plate number</Label>
            <Input placeholder="ABC1234" {...register('plateNumber')} />
            {formState.errors.plateNumber && (
              <p className="text-xs text-destructive">{formState.errors.plateNumber.message}</p>
            )}
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Vehicle type</Label>
              <Select onValueChange={(v) => setValue('vehicleTypeId', v, { shouldValidate: true })}>
                <SelectTrigger><SelectValue placeholder="Type…" /></SelectTrigger>
                <SelectContent>
                  {vehicleTypes.map((t) => (
                    <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {formState.errors.vehicleTypeId && (
                <p className="text-xs text-destructive">{formState.errors.vehicleTypeId.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label>Size</Label>
              <Select onValueChange={(v) => setValue('sizeId', v, { shouldValidate: true })}>
                <SelectTrigger><SelectValue placeholder="Size…" /></SelectTrigger>
                <SelectContent>
                  {sizes.map((s) => (
                    <SelectItem key={s.id} value={s.id}>{s.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {formState.errors.sizeId && (
                <p className="text-xs text-destructive">{formState.errors.sizeId.message}</p>
              )}
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Make (optional)</Label>
              <Select
                onValueChange={(v) => {
                  setValue('makeId', v)
                  setValue('modelId', '')
                }}
              >
                <SelectTrigger><SelectValue placeholder="Make…" /></SelectTrigger>
                <SelectContent>
                  {makes.map((m) => (
                    <SelectItem key={m.id} value={m.id}>{m.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Model (optional)</Label>
              <Select
                disabled={!selectedMakeId || models.length === 0}
                onValueChange={(v) => setValue('modelId', v)}
              >
                <SelectTrigger><SelectValue placeholder="Model…" /></SelectTrigger>
                <SelectContent>
                  {models.map((m) => (
                    <SelectItem key={m.id} value={m.id}>{m.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Color (optional)</Label>
              <Input placeholder="White" {...register('color')} />
            </div>
            <div className="space-y-1.5">
              <Label>Year (optional)</Label>
              <Input type="number" placeholder="2022" {...register('year')} />
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Notes (optional)</Label>
            <Input placeholder="Scratch on rear bumper…" {...register('notes')} />
          </div>
          <div className="flex justify-end pt-1">
            <Button type="submit" disabled={formState.isSubmitting}>
              {formState.isSubmitting ? 'Saving…' : 'Register Vehicle'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// ── Vehicles tab ──────────────────────────────────────────────────────────────

function VehiclesTab({ customer }: { customer: CustomerDetail }) {
  const [addOpen, setAddOpen] = useState(false)
  const router = useRouter()

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm font-medium">{customer.cars.length} registered vehicle{customer.cars.length !== 1 ? 's' : ''}</p>
        <Button size="sm" variant="outline" onClick={() => setAddOpen(true)}>
          <Plus className="mr-2 h-3.5 w-3.5" />
          Register Vehicle
        </Button>
      </div>

      {customer.cars.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed p-12 text-center gap-2">
          <Car className="h-8 w-8 text-muted-foreground/40" />
          <p className="text-sm text-muted-foreground">No vehicles registered yet</p>
        </div>
      ) : (
        <div className="rounded-lg border divide-y">
          {customer.cars.map((car: CustomerCar) => (
            <div
              key={car.id}
              className="flex items-center justify-between px-4 py-3 hover:bg-muted/40 cursor-pointer transition-colors"
              onClick={() => router.push(`/dashboard/vehicles/${car.id}`)}
            >
              <div className="flex items-center gap-3">
                <Car className="h-4 w-4 text-muted-foreground shrink-0" />
                <div>
                  <p className="font-medium text-sm font-mono">{car.plateNumber}</p>
                  <p className="text-xs text-muted-foreground">
                    {car.vehicleTypeName} · {car.sizeName}
                    {car.makeName && ` · ${car.makeName}${car.modelName ? ` ${car.modelName}` : ''}`}
                    {car.color && ` · ${car.color}`}
                    {car.year && ` (${car.year})`}
                  </p>
                </div>
              </div>
              <Badge variant="outline" className="text-xs shrink-0">View</Badge>
            </div>
          ))}
        </div>
      )}

      <AddCarDialog customerId={customer.id} open={addOpen} onOpenChange={setAddOpen} />
    </div>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function CustomerDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()
  const [editOpen, setEditOpen] = useState(false)

  const { data: customer, isLoading, isError } = useCustomer(id)
  const { mutate: toggleStatus, isPending: isToggling } = useToggleCustomerStatus()
  const { mutateAsync: updateCustomer } = useUpdateCustomer(id)

  const handleToggle = () => {
    if (!customer) return
    toggleStatus(id, {
      onSuccess: () => toast.success(`Customer ${customer.isActive ? 'deactivated' : 'activated'}`),
      onError: () => toast.error('Failed to update status'),
    })
  }

  const handleUpdate = async (values: EditFormValues) => {
    try {
      await updateCustomer({
        ...values,
        email: values.email || undefined,
        contactNumber: values.contactNumber || undefined,
        notes: values.notes || undefined,
      })
      toast.success('Customer updated')
      setEditOpen(false)
    } catch {
      toast.error('Failed to update customer')
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-96" />
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  if (isError || !customer) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" size="sm" onClick={() => router.back()}>
          <ArrowLeft className="mr-2 h-4 w-4" />Back
        </Button>
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Customer not found or failed to load.
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold tracking-tight">{customer.fullName}</h1>
              <StatusBadge status={customer.isActive ? 'Active' : 'Inactive'} />
            </div>
            {customer.email && (
              <p className="text-sm text-muted-foreground mt-0.5">{customer.email}</p>
            )}
          </div>
        </div>
        <div className="flex items-center gap-2 shrink-0">
          <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
            <Pencil className="mr-2 h-3.5 w-3.5" />Edit
          </Button>
          <Button variant="outline" size="sm" onClick={handleToggle} disabled={isToggling}>
            {customer.isActive ? (
              <><PowerOff className="mr-2 h-3.5 w-3.5" />Deactivate</>
            ) : (
              <><Power className="mr-2 h-3.5 w-3.5" />Activate</>
            )}
          </Button>
        </div>
      </div>

      <Tabs defaultValue="details">
        <TabsList>
          <TabsTrigger value="details">Details</TabsTrigger>
          <TabsTrigger value="vehicles">
            Vehicles
            {customer.cars.length > 0 && (
              <span className="ml-1.5 text-xs bg-muted rounded-full px-1.5 py-0.5">
                {customer.cars.length}
              </span>
            )}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="details" className="mt-6">
          <div className="space-y-4 max-w-md">
            <dl className="grid grid-cols-2 gap-x-8 gap-y-4">
              {customer.email && (
                <div className="col-span-2">
                  <dt className="text-sm text-muted-foreground flex items-center gap-1.5">
                    <Mail className="h-3.5 w-3.5" /> Email
                  </dt>
                  <dd className="mt-0.5">{customer.email}</dd>
                </div>
              )}
              {customer.contactNumber && (
                <div>
                  <dt className="text-sm text-muted-foreground flex items-center gap-1.5">
                    <Phone className="h-3.5 w-3.5" /> Contact
                  </dt>
                  <dd className="mt-0.5 text-sm">{customer.contactNumber}</dd>
                </div>
              )}
              {customer.notes && (
                <div className="col-span-2">
                  <dt className="text-sm text-muted-foreground flex items-center gap-1.5">
                    <FileText className="h-3.5 w-3.5" /> Notes
                  </dt>
                  <dd className="mt-0.5 text-sm">{customer.notes}</dd>
                </div>
              )}
              <div>
                <dt className="text-sm text-muted-foreground">Member since</dt>
                <dd className="mt-0.5 text-sm">
                  {new Date(customer.createdAt).toLocaleDateString('en-PH', {
                    year: 'numeric', month: 'long', day: 'numeric',
                  })}
                </dd>
              </div>
            </dl>
          </div>
        </TabsContent>

        <TabsContent value="vehicles" className="mt-6">
          <VehiclesTab customer={customer} />
        </TabsContent>
      </Tabs>

      <Sheet open={editOpen} onOpenChange={setEditOpen}>
        <SheetContent className="sm:max-w-md overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Edit Customer</SheetTitle>
            <SheetDescription>Update customer contact details.</SheetDescription>
          </SheetHeader>
          <div className="mt-6">
            <EditCustomerForm customer={customer} onSubmit={handleUpdate} />
          </div>
        </SheetContent>
      </Sheet>
    </div>
  )
}
