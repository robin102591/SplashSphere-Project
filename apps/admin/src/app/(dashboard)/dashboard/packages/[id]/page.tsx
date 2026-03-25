'use client'

import { use, useState } from 'react'
import { Pencil, Power, PowerOff, CheckCircle2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { StatusBadge } from '@/components/ui/status-badge'
import { PageHeader } from '@/components/ui/page-header'
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
  usePackage,
  useUpdatePackage,
  useTogglePackageStatus,
  useUpsertPackagePricing,
  useUpsertPackageCommissions,
} from '@/hooks/use-packages'
import { useVehicleTypes } from '@/hooks/use-vehicle-types'
import { useSizes } from '@/hooks/use-sizes'
import { PricingMatrixEditor } from '@/components/pricing-matrix-editor'
import { PackageCommissionMatrixEditor } from '@/components/package-commission-matrix-editor'
import { PackageForm } from '../_components/package-form'
import type { PackageFormValues } from '@/hooks/use-packages'
import { toast } from 'sonner'

// ── Details tab ───────────────────────────────────────────────────────────────

function DetailsTab({
  description,
  services,
  createdAt,
  updatedAt,
}: {
  description: string | null
  services: readonly { serviceId: string; serviceName: string; categoryName: string }[]
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
    <div className="space-y-6 max-w-lg">
      <dl className="grid grid-cols-2 gap-x-8 gap-y-4">
        <div className="col-span-2">
          <dt className="text-sm text-muted-foreground">Description</dt>
          <dd className="mt-0.5">
            {description ?? (
              <span className="text-muted-foreground italic">No description</span>
            )}
          </dd>
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

      <div>
        <p className="text-sm font-medium mb-2">Included services ({services.length})</p>
        {services.length === 0 ? (
          <p className="text-sm text-muted-foreground italic">No services assigned</p>
        ) : (
          <ul className="rounded-lg border divide-y">
            {services.map((svc) => (
              <li key={svc.serviceId} className="flex items-center gap-3 px-4 py-3">
                <CheckCircle2 className="h-4 w-4 text-green-500 shrink-0" />
                <div>
                  <p className="text-sm font-medium">{svc.serviceName}</p>
                  <p className="text-xs text-muted-foreground">{svc.categoryName}</p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function PackageDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const [editOpen, setEditOpen] = useState(false)

  const { data: pkg, isLoading, isError } = usePackage(id)
  const { data: vehicleTypes = [], isLoading: vtLoading } = useVehicleTypes()
  const { data: sizes = [], isLoading: sizeLoading } = useSizes()
  const { mutate: toggleStatus, isPending: isToggling } = useTogglePackageStatus()
  const { mutateAsync: updatePackage } = useUpdatePackage(id)
  const { mutateAsync: upsertPricing, isPending: isPricingSaving } = useUpsertPackagePricing(id)
  const { mutateAsync: upsertCommissions, isPending: isCommissionSaving } =
    useUpsertPackageCommissions(id)

  const matrixLoading = vtLoading || sizeLoading

  const handleToggle = () => {
    if (!pkg) return
    toggleStatus(id, {
      onSuccess: () => toast.success(`Package ${pkg.isActive ? 'deactivated' : 'activated'}`),
      onError: () => toast.error('Failed to update status'),
    })
  }

  const handleUpdate = async (values: PackageFormValues) => {
    try {
      await updatePackage(values)
      toast.success('Package updated')
      setEditOpen(false)
    } catch {
      toast.error('Failed to update package')
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

  if (isError || !pkg) {
    return (
      <div className="space-y-4">
        <PageHeader title="Package" back />
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Package not found or failed to load.
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader
        title={pkg.name}
        back
        badge={
          <>
            <StatusBadge status={pkg.isActive ? 'Active' : 'Inactive'} />
            <Badge variant="outline" className="text-xs">
              {pkg.serviceCount} {pkg.serviceCount === 1 ? 'service' : 'services'}
            </Badge>
          </>
        }
        description={pkg.description ?? undefined}
        actions={
          <>
            <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
              <Pencil className="mr-2 h-3.5 w-3.5" />
              Edit
            </Button>
            <Button variant="outline" size="sm" onClick={handleToggle} disabled={isToggling}>
              {pkg.isActive ? (
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
          </>
        }
      />

      {/* Tabs */}
      <Tabs defaultValue="details">
        <TabsList variant="line">
          <TabsTrigger value="details">Details</TabsTrigger>
          <TabsTrigger value="pricing">
            Pricing Matrix
            {pkg.pricing.length > 0 && (
              <span className="ml-1.5 text-xs bg-muted rounded-full px-1.5 py-0.5">
                {pkg.pricing.length}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="commissions">
            Commission Matrix
            {pkg.commissions.length > 0 && (
              <span className="ml-1.5 text-xs bg-muted rounded-full px-1.5 py-0.5">
                {pkg.commissions.length}
              </span>
            )}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="details" className="mt-6">
          <DetailsTab
            description={pkg.description}
            services={pkg.services}
            createdAt={pkg.createdAt}
            updatedAt={pkg.updatedAt}
          />
        </TabsContent>

        <TabsContent value="pricing" className="mt-6">
          <PricingMatrixEditor
            vehicleTypes={vehicleTypes}
            sizes={sizes}
            initialRows={pkg.pricing}
            basePrice={0}
            onSave={upsertPricing}
            isSaving={isPricingSaving}
            isLoading={matrixLoading}
          />
        </TabsContent>

        <TabsContent value="commissions" className="mt-6">
          <PackageCommissionMatrixEditor
            vehicleTypes={vehicleTypes}
            sizes={sizes}
            initialRows={pkg.commissions}
            onSave={upsertCommissions}
            isSaving={isCommissionSaving}
            isLoading={matrixLoading}
          />
        </TabsContent>
      </Tabs>

      {/* Edit sheet */}
      <Sheet open={editOpen} onOpenChange={setEditOpen}>
        <SheetContent className="sm:max-w-lg overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Edit Package</SheetTitle>
            <SheetDescription>Update package name, description, and included services</SheetDescription>
          </SheetHeader>
          <div className="mt-6">
            <PackageForm
              defaultValues={{
                name: pkg.name,
                description: pkg.description ?? '',
                serviceIds: pkg.services.map((s) => s.serviceId),
              }}
              onSubmit={handleUpdate}
              submitLabel="Save Changes"
            />
          </div>
        </SheetContent>
      </Sheet>
    </div>
  )
}
