'use client'

import { use, useState } from 'react'
import { useRouter } from 'next/navigation'
import {
  Pencil, Power, PowerOff, Car, Mail, Phone, FileText, Plus, Award, Star, TrendingUp,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { StatusBadge } from '@/components/ui/status-badge'
import { PageHeader } from '@/components/ui/page-header'
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
import { useCustomerLoyaltySummary, useEnrollMember, usePointHistory } from '@/hooks/use-loyalty'
import { useCreateCar, useMakes, useModelsByMake } from '@/hooks/use-cars'
import { useVehicleTypes } from '@/hooks/use-vehicle-types'
import { useSizes } from '@/hooks/use-sizes'
import type { CustomerDetail, CustomerCar } from '@splashsphere/types'
import { LoyaltyTier } from '@splashsphere/types'
import { Card, CardContent } from '@/components/ui/card'
import { Progress } from '@/components/ui/progress'
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
          <Label>First name <span className="text-destructive">*</span></Label>
          <Input {...register('firstName')} />
          {formState.errors.firstName && (
            <p className="text-xs text-destructive">{formState.errors.firstName.message}</p>
          )}
        </div>
        <div className="space-y-1.5">
          <Label>Last name <span className="text-destructive">*</span></Label>
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
            <Label>Plate number <span className="text-destructive">*</span></Label>
            <Input placeholder="ABC1234" {...register('plateNumber')} />
            {formState.errors.plateNumber && (
              <p className="text-xs text-destructive">{formState.errors.plateNumber.message}</p>
            )}
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Vehicle type <span className="text-destructive">*</span></Label>
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
              <Label>Size <span className="text-destructive">*</span></Label>
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

// ── Loyalty tab ──────────────────────────────────────────────────────────────

const TIER_LABELS: Record<number, string> = {
  [LoyaltyTier.Standard]: 'Standard',
  [LoyaltyTier.Silver]: 'Silver',
  [LoyaltyTier.Gold]: 'Gold',
  [LoyaltyTier.Platinum]: 'Platinum',
}

const TIER_COLORS: Record<number, string> = {
  [LoyaltyTier.Standard]: 'bg-gray-100 text-gray-700 border-gray-200',
  [LoyaltyTier.Silver]: 'bg-slate-100 text-slate-700 border-slate-300',
  [LoyaltyTier.Gold]: 'bg-amber-50 text-amber-700 border-amber-200',
  [LoyaltyTier.Platinum]: 'bg-violet-50 text-violet-700 border-violet-200',
}

function LoyaltyTab({ customerId }: { customerId: string }) {
  const { data: summary, isLoading, isError } = useCustomerLoyaltySummary(customerId)
  const { mutate: enroll, isPending: enrolling } = useEnrollMember()

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-24 w-full" />
      </div>
    )
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
        Failed to load loyalty info. The loyalty feature may not be enabled on your plan.
      </div>
    )
  }

  // Not enrolled — show enroll button
  if (!summary) {
    return (
      <div className="flex flex-col items-center justify-center rounded-lg border border-dashed p-12 text-center gap-3">
        <Award className="h-10 w-10 text-muted-foreground/40" />
        <div>
          <p className="font-medium">Not a loyalty member</p>
          <p className="text-sm text-muted-foreground mt-0.5">
            Enroll this customer to start earning points on every transaction.
          </p>
        </div>
        <Button
          onClick={() => enroll(customerId, {
            onSuccess: () => toast.success('Customer enrolled in loyalty program'),
            onError: () => toast.error('Failed to enroll customer'),
          })}
          disabled={enrolling}
        >
          <Award className="mr-2 h-4 w-4" />
          {enrolling ? 'Enrolling...' : 'Enroll in Loyalty Program'}
        </Button>
      </div>
    )
  }

  // Enrolled — show membership card info
  const tierProgress = summary.pointsToNextTier != null
    ? Math.round(
        (summary.lifetimePointsEarned /
          (summary.lifetimePointsEarned + summary.pointsToNextTier)) *
          100
      )
    : 100

  return (
    <div className="space-y-6 max-w-lg">
      {/* Membership card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-xs text-muted-foreground uppercase tracking-wider">Membership Card</p>
              <p className="text-lg font-mono font-bold mt-0.5">{summary.cardNumber}</p>
            </div>
            <Badge
              variant="outline"
              className={`text-xs font-semibold ${TIER_COLORS[summary.currentTier] ?? ''}`}
            >
              {summary.tierName}
            </Badge>
          </div>

          <div className="grid grid-cols-2 gap-4 mt-5">
            <div>
              <p className="text-xs text-muted-foreground">Available Points</p>
              <p className="text-2xl font-bold tabular-nums">{summary.pointsBalance.toLocaleString()}</p>
            </div>
            <div>
              <p className="text-xs text-muted-foreground">Lifetime Earned</p>
              <p className="text-2xl font-bold tabular-nums">{summary.lifetimePointsEarned.toLocaleString()}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Tier progress */}
      {summary.nextTierName && summary.pointsToNextTier != null && (
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between text-sm">
              <span className="font-medium flex items-center gap-1.5">
                <TrendingUp className="h-3.5 w-3.5 text-muted-foreground" />
                Next tier: {summary.nextTierName}
              </span>
              <span className="text-muted-foreground">
                {summary.pointsToNextTier.toLocaleString()} pts to go
              </span>
            </div>
            <Progress value={tierProgress} className="mt-2 h-2" />
          </CardContent>
        </Card>
      )}

      {/* Available rewards */}
      {summary.availableRewards.length > 0 && (
        <div>
          <p className="text-sm font-medium mb-2 flex items-center gap-1.5">
            <Star className="h-3.5 w-3.5 text-muted-foreground" />
            Rewards available to redeem
          </p>
          <div className="rounded-lg border divide-y">
            {summary.availableRewards.map((r) => (
              <div key={r.id} className="flex items-center justify-between px-4 py-2.5">
                <span className="text-sm">{r.name}</span>
                <span className="text-xs font-mono text-muted-foreground">{r.pointsCost.toLocaleString()} pts</span>
              </div>
            ))}
          </div>
        </div>
      )}
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
        <PageHeader title="Customer" back />
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Customer not found or failed to load.
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={customer.fullName}
        description={customer.email || undefined}
        back
        badge={<StatusBadge status={customer.isActive ? 'Active' : 'Inactive'} />}
        actions={
          <>
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
          </>
        }
      />

      <Tabs defaultValue="details">
        <TabsList variant="line">
          <TabsTrigger value="details">Details</TabsTrigger>
          <TabsTrigger value="vehicles">
            Vehicles
            {customer.cars.length > 0 && (
              <span className="ml-1.5 text-xs bg-muted rounded-full px-1.5 py-0.5">
                {customer.cars.length}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="loyalty">
            <Award className="mr-1.5 h-3.5 w-3.5" />
            Loyalty
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

        <TabsContent value="loyalty" className="mt-6">
          <LoyaltyTab customerId={customer.id} />
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
