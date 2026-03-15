'use client'

import { useState } from 'react'
import { Plus, Package } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { DataTable } from '@/components/ui/data-table'
import { Skeleton } from '@/components/ui/skeleton'
import { usePackages, useTogglePackageStatus } from '@/hooks/use-packages'
import { getPackageColumns } from './_components/package-columns'
import { CreatePackageDialog } from './_components/create-package-dialog'
import { toast } from 'sonner'

export default function PackagesPage() {
  const [createOpen, setCreateOpen] = useState(false)
  const { data, isLoading, isError } = usePackages({ pageSize: 100 })
  const { mutate: toggleStatus } = useTogglePackageStatus()

  const handleToggle = (id: string) => {
    toggleStatus(id, {
      onSuccess: () => toast.success('Package status updated'),
      onError: () => toast.error('Failed to update status'),
    })
  }

  const columns = getPackageColumns({ onToggleStatus: handleToggle })
  const packages = data ? [...data.items] : []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Packages</h1>
          <p className="text-muted-foreground">Bundle services into packages with shared pricing</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          New Package
        </Button>
      </div>

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
          Failed to load packages.
        </div>
      )}

      {!isLoading && !isError && packages.length === 0 && (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed p-16 text-center gap-3">
          <Package className="h-10 w-10 text-muted-foreground/40" />
          <div>
            <p className="font-medium">No packages yet</p>
            <p className="text-sm text-muted-foreground">
              Bundle services together to offer package deals
            </p>
          </div>
          <Button variant="outline" onClick={() => setCreateOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            New Package
          </Button>
        </div>
      )}

      {!isLoading && packages.length > 0 && (
        <DataTable
          columns={columns}
          data={packages}
          searchKey="name"
          searchPlaceholder="Search packages…"
        />
      )}

      <CreatePackageDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  )
}
