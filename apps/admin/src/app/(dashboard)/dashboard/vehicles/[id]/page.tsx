'use client'

import { use, useState } from 'react'
import { useRouter } from 'next/navigation'
import { ArrowLeft, Pencil, ExternalLink } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet'
import { Label } from '@/components/ui/label'
import { Input } from '@/components/ui/input'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { useCar, useUpdateCar, useMakes, useModelsByMake } from '@/hooks/use-cars'
import { useVehicleTypes } from '@/hooks/use-vehicle-types'
import { useSizes } from '@/hooks/use-sizes'
import type { UpdateCarValues } from '@/hooks/use-cars'
import { toast } from 'sonner'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import Link from 'next/link'

const editSchema = z.object({
  vehicleTypeId: z.string().min(1, 'Required'),
  sizeId: z.string().min(1, 'Required'),
  makeId: z.string().optional(),
  modelId: z.string().optional(),
  color: z.string().optional(),
  year: z.coerce.number().int().min(1900).max(2100).optional().or(z.literal('')),
  notes: z.string().optional(),
})
type EditFormValues = z.infer<typeof editSchema>

function EditCarForm({
  car,
  onSubmit,
}: {
  car: { vehicleTypeId: string; sizeId: string; makeId: string | null; modelId: string | null; color: string | null; year: number | null; notes: string | null }
  onSubmit: (v: UpdateCarValues) => Promise<void>
}) {
  const { data: vehicleTypes = [] } = useVehicleTypes()
  const { data: sizes = [] } = useSizes()
  const { data: makes = [] } = useMakes()

  const { register, handleSubmit, watch, setValue, formState } = useForm<EditFormValues>({
    resolver: zodResolver(editSchema),
    defaultValues: {
      vehicleTypeId: car.vehicleTypeId,
      sizeId: car.sizeId,
      makeId: car.makeId ?? '',
      modelId: car.modelId ?? '',
      color: car.color ?? '',
      year: car.year ?? '',
      notes: car.notes ?? '',
    },
  })

  const selectedMakeId = watch('makeId')
  const { data: models = [] } = useModelsByMake(selectedMakeId || undefined)

  const onFormSubmit = async (values: EditFormValues) => {
    await onSubmit({
      vehicleTypeId: values.vehicleTypeId,
      sizeId: values.sizeId,
      makeId: values.makeId || undefined,
      modelId: values.modelId || undefined,
      color: values.color || undefined,
      year: values.year ? Number(values.year) : undefined,
      notes: values.notes || undefined,
    })
  }

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1.5">
          <Label>Vehicle type</Label>
          <Select
            defaultValue={car.vehicleTypeId}
            onValueChange={(v) => setValue('vehicleTypeId', v, { shouldValidate: true })}
          >
            <SelectTrigger><SelectValue /></SelectTrigger>
            <SelectContent>
              {vehicleTypes.map((t) => <SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>)}
            </SelectContent>
          </Select>
          {formState.errors.vehicleTypeId && (
            <p className="text-xs text-destructive">{formState.errors.vehicleTypeId.message}</p>
          )}
        </div>
        <div className="space-y-1.5">
          <Label>Size</Label>
          <Select
            defaultValue={car.sizeId}
            onValueChange={(v) => setValue('sizeId', v, { shouldValidate: true })}
          >
            <SelectTrigger><SelectValue /></SelectTrigger>
            <SelectContent>
              {sizes.map((s) => <SelectItem key={s.id} value={s.id}>{s.name}</SelectItem>)}
            </SelectContent>
          </Select>
          {formState.errors.sizeId && (
            <p className="text-xs text-destructive">{formState.errors.sizeId.message}</p>
          )}
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1.5">
          <Label>Make</Label>
          <Select
            defaultValue={car.makeId ?? ''}
            onValueChange={(v) => { setValue('makeId', v); setValue('modelId', '') }}
          >
            <SelectTrigger><SelectValue placeholder="None" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="">None</SelectItem>
              {makes.map((m) => <SelectItem key={m.id} value={m.id}>{m.name}</SelectItem>)}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-1.5">
          <Label>Model</Label>
          <Select
            defaultValue={car.modelId ?? ''}
            disabled={!selectedMakeId || models.length === 0}
            onValueChange={(v) => setValue('modelId', v)}
          >
            <SelectTrigger><SelectValue placeholder="None" /></SelectTrigger>
            <SelectContent>
              <SelectItem value="">None</SelectItem>
              {models.map((m) => <SelectItem key={m.id} value={m.id}>{m.name}</SelectItem>)}
            </SelectContent>
          </Select>
        </div>
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1.5">
          <Label>Color</Label>
          <Input placeholder="White" {...register('color')} />
        </div>
        <div className="space-y-1.5">
          <Label>Year</Label>
          <Input type="number" placeholder="2022" {...register('year')} />
        </div>
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

export default function VehicleDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const router = useRouter()
  const [editOpen, setEditOpen] = useState(false)

  const { data: car, isLoading, isError } = useCar(id)
  const { mutateAsync: updateCar } = useUpdateCar(id)

  const handleUpdate = async (values: UpdateCarValues) => {
    try {
      await updateCar(values)
      toast.success('Vehicle updated')
      setEditOpen(false)
    } catch {
      toast.error('Failed to update vehicle')
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-48 w-full" />
      </div>
    )
  }

  if (isError || !car) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" size="sm" onClick={() => router.back()}>
          <ArrowLeft className="mr-2 h-4 w-4" />Back
        </Button>
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Vehicle not found or failed to load.
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold tracking-tight font-mono">{car.plateNumber}</h1>
              <Badge variant="outline">{car.vehicleTypeName}</Badge>
              <Badge variant="outline">{car.sizeName}</Badge>
            </div>
            <p className="text-sm text-muted-foreground mt-0.5">
              {[car.makeName, car.modelName, car.color, car.year].filter(Boolean).join(' · ') || 'No additional details'}
            </p>
          </div>
        </div>
        <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
          <Pencil className="mr-2 h-3.5 w-3.5" />Edit
        </Button>
      </div>

      <div className="max-w-md rounded-lg border p-5 space-y-4">
        <dl className="grid grid-cols-2 gap-x-8 gap-y-4 text-sm">
          <div>
            <dt className="text-muted-foreground">Vehicle type</dt>
            <dd className="mt-0.5 font-medium">{car.vehicleTypeName}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">Size</dt>
            <dd className="mt-0.5 font-medium">{car.sizeName}</dd>
          </div>
          {car.makeName && (
            <div>
              <dt className="text-muted-foreground">Make</dt>
              <dd className="mt-0.5 font-medium">{car.makeName}</dd>
            </div>
          )}
          {car.modelName && (
            <div>
              <dt className="text-muted-foreground">Model</dt>
              <dd className="mt-0.5 font-medium">{car.modelName}</dd>
            </div>
          )}
          {car.color && (
            <div>
              <dt className="text-muted-foreground">Color</dt>
              <dd className="mt-0.5 font-medium">{car.color}</dd>
            </div>
          )}
          {car.year && (
            <div>
              <dt className="text-muted-foreground">Year</dt>
              <dd className="mt-0.5 font-medium">{car.year}</dd>
            </div>
          )}
          <div className="col-span-2">
            <dt className="text-muted-foreground">Owner</dt>
            <dd className="mt-0.5">
              {car.customerFullName && car.customerId ? (
                <Link
                  href={`/dashboard/customers/${car.customerId}`}
                  className="font-medium text-primary hover:underline flex items-center gap-1"
                >
                  {car.customerFullName}
                  <ExternalLink className="h-3 w-3" />
                </Link>
              ) : (
                <span className="text-muted-foreground italic">Walk-in (no customer linked)</span>
              )}
            </dd>
          </div>
          {car.notes && (
            <div className="col-span-2">
              <dt className="text-muted-foreground">Notes</dt>
              <dd className="mt-0.5">{car.notes}</dd>
            </div>
          )}
          <div>
            <dt className="text-muted-foreground">Registered</dt>
            <dd className="mt-0.5">{new Date(car.createdAt).toLocaleDateString('en-PH', { year: 'numeric', month: 'short', day: 'numeric' })}</dd>
          </div>
        </dl>
      </div>

      <Sheet open={editOpen} onOpenChange={setEditOpen}>
        <SheetContent className="sm:max-w-md overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Edit Vehicle</SheetTitle>
            <SheetDescription>
              Plate number and linked customer cannot be changed.
            </SheetDescription>
          </SheetHeader>
          <div className="mt-6">
            <EditCarForm car={car} onSubmit={handleUpdate} />
          </div>
        </SheetContent>
      </Sheet>
    </div>
  )
}
