'use client'

import { use, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { ArrowLeft, Pencil, Power, PowerOff } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { StatusBadge } from '@/components/ui/status-badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import {
  useService,
  useUpdateService,
  useToggleServiceStatus,
  useUpsertServicePricing,
  useUpsertServiceCommissions,
} from '@/hooks/use-services'
import { useVehicleTypes } from '@/hooks/use-vehicle-types'
import { useSizes } from '@/hooks/use-sizes'
import { useServiceCategories } from '@/hooks/use-service-categories'
import { PricingMatrixEditor } from '@/components/pricing-matrix-editor'
import { CommissionMatrixEditor } from '@/components/commission-matrix-editor'
import type { ServiceFormValues } from '@/hooks/use-services'
import { toast } from 'sonner'
import { formatPeso } from '@/lib/format'

// ── Edit form ─────────────────────────────────────────────────────────────────

const editSchema = z.object({
  name: z.string().min(2, 'Name is required'),
  categoryId: z.string().min(1, 'Category is required'),
  basePrice: z.coerce.number().min(0),
  description: z.string().optional(),
})
type EditValues = z.infer<typeof editSchema>

interface EditSheetProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  serviceId: string
  defaultValues: ServiceFormValues
}

function EditSheet({ open, onOpenChange, serviceId, defaultValues }: EditSheetProps) {
  const { mutateAsync: updateService } = useUpdateService(serviceId)
  const { data: categories } = useServiceCategories()

  const { register, handleSubmit, formState } = useForm<EditValues>({
    resolver: zodResolver(editSchema),
    values: defaultValues as EditValues,
  })

  const onSubmit = async (values: EditValues) => {
    try {
      await updateService({
        name: values.name,
        categoryId: values.categoryId,
        basePrice: values.basePrice,
        description: values.description || undefined,
      })
      toast.success('Service updated')
      onOpenChange(false)
    } catch {
      toast.error('Failed to update service')
    }
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="sm:max-w-md">
        <SheetHeader>
          <SheetTitle>Edit Service</SheetTitle>
          <SheetDescription>Update service details and base price</SheetDescription>
        </SheetHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="edit-name">Service name</Label>
            <Input id="edit-name" {...register('name')} />
            {formState.errors.name && (
              <p className="text-xs text-destructive">{formState.errors.name.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="edit-category">Category</Label>
            <select
              id="edit-category"
              className="w-full h-10 rounded-md border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              {...register('categoryId')}
            >
              <option value="">Select category…</option>
              {categories?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
            {formState.errors.categoryId && (
              <p className="text-xs text-destructive">{formState.errors.categoryId.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="edit-basePrice">Base price (₱)</Label>
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm select-none">
                ₱
              </span>
              <Input
                id="edit-basePrice"
                type="number"
                min="0"
                step="0.01"
                className="pl-7"
                {...register('basePrice')}
              />
            </div>
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="edit-description">Description (optional)</Label>
            <Input id="edit-description" {...register('description')} />
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={formState.isSubmitting}>
              {formState.isSubmitting ? 'Saving…' : 'Save Changes'}
            </Button>
          </div>
        </form>
      </SheetContent>
    </Sheet>
  )
}

// ── Details tab ───────────────────────────────────────────────────────────────

function DetailsTab({
  basePrice,
  description,
  categoryName,
  createdAt,
  updatedAt,
}: {
  basePrice: number
  description: string | null
  categoryName: string
  createdAt: string
  updatedAt: string
}) {
  function formatDate(iso: string) {
    return new Date(iso).toLocaleString('en-PH', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  return (
    <dl className="grid grid-cols-2 gap-x-8 gap-y-4 max-w-lg">
      <div>
        <dt className="text-sm text-muted-foreground">Category</dt>
        <dd className="mt-0.5 font-medium">{categoryName}</dd>
      </div>
      <div>
        <dt className="text-sm text-muted-foreground">Base price</dt>
        <dd className="mt-0.5 font-mono font-medium">{formatPeso(basePrice)}</dd>
      </div>
      <div className="col-span-2">
        <dt className="text-sm text-muted-foreground">Description</dt>
        <dd className="mt-0.5">{description ?? <span className="text-muted-foreground italic">No description</span>}</dd>
      </div>
      <div>
        <dt className="text-sm text-muted-foreground">Created</dt>
        <dd className="mt-0.5 text-sm">{formatDate(createdAt)}</dd>
      </div>
      <div>
        <dt className="text-sm text-muted-foreground">Last updated</dt>
        <dd className="mt-0.5 text-sm">{formatDate(updatedAt)}</dd>
      </div>
    </dl>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function ServiceDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const router = useRouter()
  const [editOpen, setEditOpen] = useState(false)

  const { data: service, isLoading, isError } = useService(id)
  const { data: vehicleTypes = [], isLoading: vtLoading } = useVehicleTypes()
  const { data: sizes = [], isLoading: sizeLoading } = useSizes()
  const { mutate: toggleStatus, isPending: isToggling } = useToggleServiceStatus()
  const { mutateAsync: upsertPricing, isPending: isPricingSaving } = useUpsertServicePricing(id)
  const { mutateAsync: upsertCommissions, isPending: isCommissionSaving } = useUpsertServiceCommissions(id)

  const matrixLoading = vtLoading || sizeLoading

  const handleToggle = () => {
    if (!service) return
    toggleStatus(id, {
      onSuccess: () => toast.success(`Service ${service.isActive ? 'deactivated' : 'activated'}`),
      onError: () => toast.error('Failed to update status'),
    })
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

  if (isError || !service) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" size="sm" onClick={() => router.back()}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back
        </Button>
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Service not found or failed to load.
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
              <h1 className="text-2xl font-bold tracking-tight">{service.name}</h1>
              <StatusBadge status={service.isActive ? 'Active' : 'Inactive'} />
              <Badge variant="outline" className="text-xs">
                {service.categoryName}
              </Badge>
            </div>
            <p className="text-sm text-muted-foreground mt-0.5">
              Base price:{' '}
              <span className="font-medium font-mono text-foreground">
                {formatPeso(service.basePrice)}
              </span>
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2 shrink-0">
          <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
            <Pencil className="mr-2 h-3.5 w-3.5" />
            Edit
          </Button>
          <Button variant="outline" size="sm" onClick={handleToggle} disabled={isToggling}>
            {service.isActive ? (
              <>
                <PowerOff className="mr-2 h-3.5 w-3.5" />
                Deactivate
              </>
            ) : (
              <>
                <Power className="mr-2 h-3.5 w-3.5" />
                Activate
              </>
            )}
          </Button>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="details">
        <TabsList>
          <TabsTrigger value="details">Details</TabsTrigger>
          <TabsTrigger value="pricing">
            Pricing Matrix
            {service.pricing.length > 0 && (
              <span className="ml-1.5 text-xs bg-muted rounded-full px-1.5 py-0.5">
                {service.pricing.length}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="commissions">
            Commission Matrix
            {service.commissions.length > 0 && (
              <span className="ml-1.5 text-xs bg-muted rounded-full px-1.5 py-0.5">
                {service.commissions.length}
              </span>
            )}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="details" className="mt-6">
          <DetailsTab
            basePrice={service.basePrice}
            description={service.description}
            categoryName={service.categoryName}
            createdAt={service.createdAt}
            updatedAt={service.updatedAt}
          />
        </TabsContent>

        <TabsContent value="pricing" className="mt-6">
          <PricingMatrixEditor
            vehicleTypes={vehicleTypes}
            sizes={sizes}
            initialRows={service.pricing}
            basePrice={service.basePrice}
            onSave={upsertPricing}
            isSaving={isPricingSaving}
            isLoading={matrixLoading}
          />
        </TabsContent>

        <TabsContent value="commissions" className="mt-6">
          <CommissionMatrixEditor
            serviceId={id}
            vehicleTypes={vehicleTypes}
            sizes={sizes}
            initialRows={service.commissions}
            isLoading={matrixLoading}
          />
        </TabsContent>
      </Tabs>

      <EditSheet
        open={editOpen}
        onOpenChange={setEditOpen}
        serviceId={id}
        defaultValues={{
          name: service.name,
          categoryId: service.categoryId,
          basePrice: service.basePrice,
          description: service.description ?? '',
        }}
      />
    </div>
  )
}
