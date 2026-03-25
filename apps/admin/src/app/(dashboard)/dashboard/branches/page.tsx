'use client'

import { useRouter } from 'next/navigation'
import { Plus, GitBranch } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/ui/empty-state'
import { DataTable } from '@/components/ui/data-table'
import { Skeleton } from '@/components/ui/skeleton'
import { useBranches, useToggleBranchStatus } from '@/hooks/use-branches'
import { getBranchColumns } from './_components/branch-columns'
import type { Branch } from '@splashsphere/types'
import { toast } from 'sonner'

export default function BranchesPage() {
  const router = useRouter()
  const { data: branches, isLoading, isError } = useBranches()
  const { mutate: toggleStatus } = useToggleBranchStatus()

  const handleToggleStatus = (branch: Branch) => {
    toggleStatus(
      { id: branch.id, isActive: !branch.isActive },
      {
        onSuccess: () =>
          toast.success(`Branch ${branch.isActive ? 'deactivated' : 'activated'}`),
        onError: () => toast.error('Failed to update branch status'),
      }
    )
  }

  const columns = getBranchColumns({ onToggleStatus: handleToggleStatus })

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Branches</h1>
          <p className="text-muted-foreground">Manage your car wash locations</p>
        </div>
        <Button onClick={() => router.push('/dashboard/branches/new')}>
          <Plus className="mr-2 h-4 w-4" />
          New Branch
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
          Failed to load branches. Check your connection and try again.
        </div>
      )}

      {!isLoading && !isError && (!branches || branches.length === 0) && (
        <EmptyState
          icon={GitBranch}
          title="No branches yet"
          description="Create your first branch to get started"
          action={{ label: 'New Branch', onClick: () => router.push('/dashboard/branches/new'), icon: Plus }}
        />
      )}

      {!isLoading && branches && branches.length > 0 && (
        <DataTable
          columns={columns}
          data={branches}
          searchKey="name"
          searchPlaceholder="Search branches…"
        />
      )}
    </div>
  )
}
