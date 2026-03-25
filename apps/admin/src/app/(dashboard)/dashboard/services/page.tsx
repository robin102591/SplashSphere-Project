'use client'

import { useState } from 'react'
import { Plus, Wrench } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/ui/empty-state'
import { DataTable } from '@/components/ui/data-table'
import { Skeleton } from '@/components/ui/skeleton'
import { useServices, useToggleServiceStatus } from '@/hooks/use-services'
import { getServiceColumns } from './_components/service-columns'
import { CreateServiceDialog } from './_components/create-service-dialog'
import { toast } from 'sonner'

export default function ServicesPage() {
  const [createOpen, setCreateOpen] = useState(false)
  const { data, isLoading, isError } = useServices({ pageSize: 100 })
  const { mutate: toggleStatus } = useToggleServiceStatus()

  const handleToggle = (id: string) => {
    toggleStatus(id, {
      onSuccess: () => toast.success('Service status updated'),
      onError: () => toast.error('Failed to update status'),
    })
  }

  const columns = getServiceColumns({ onToggleStatus: handleToggle })
  const services = data ? [...data.items] : []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Services</h1>
          <p className="text-muted-foreground">Configure car wash services and pricing matrices</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          New Service
        </Button>
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
          Failed to load services.
        </div>
      )}

      {!isLoading && !isError && services.length === 0 && (
        <EmptyState
          icon={Wrench}
          title="No services yet"
          description="Create your first service to start building pricing matrices"
          action={{ label: 'New Service', onClick: () => setCreateOpen(true), icon: Plus }}
        />
      )}

      {!isLoading && services.length > 0 && (
        <DataTable
          columns={columns}
          data={services}
          searchKey="name"
          searchPlaceholder="Search services…"
        />
      )}

      <CreateServiceDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  )
}
