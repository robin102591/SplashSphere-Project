'use client'

import { useState } from 'react'
import { Plus, Users } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { DataTable } from '@/components/ui/data-table'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useEmployees, useToggleEmployeeStatus } from '@/hooks/use-employees'
import { useBranches } from '@/hooks/use-branches'
import { getEmployeeColumns } from './_components/employee-columns'
import { CreateEmployeeDialog } from './_components/create-employee-dialog'
import { EmployeeType } from '@splashsphere/types'
import { toast } from 'sonner'

export default function EmployeesPage() {
  const [createOpen, setCreateOpen] = useState(false)
  const [branchFilter, setBranchFilter] = useState<string>('all')
  const [typeFilter, setTypeFilter] = useState<string>('all')

  const { data: branches = [] } = useBranches()
  const { data, isLoading, isError } = useEmployees({
    branchId: branchFilter !== 'all' ? branchFilter : undefined,
    employeeType: typeFilter !== 'all' ? (Number(typeFilter) as EmployeeType) : undefined,
    pageSize: 100,
  })
  const { mutate: toggleStatus } = useToggleEmployeeStatus()

  const handleToggle = (id: string) => {
    toggleStatus(id, {
      onSuccess: () => toast.success('Employee status updated'),
      onError: () => toast.error('Failed to update status'),
    })
  }

  const columns = getEmployeeColumns({ onToggleStatus: handleToggle })
  const employees = data ? [...data.items] : []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Employees</h1>
          <p className="text-muted-foreground">Manage employees, attendance, and commissions</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          New Employee
        </Button>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3">
        <Select value={branchFilter} onValueChange={setBranchFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="All branches" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All branches</SelectItem>
            {branches.map((b) => (
              <SelectItem key={b.id} value={b.id}>
                {b.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={typeFilter} onValueChange={setTypeFilter}>
          <SelectTrigger className="w-44">
            <SelectValue placeholder="All types" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All types</SelectItem>
            <SelectItem value={String(EmployeeType.Commission)}>Commission</SelectItem>
            <SelectItem value={String(EmployeeType.Daily)}>Daily Rate</SelectItem>
          </SelectContent>
        </Select>
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
          Failed to load employees.
        </div>
      )}

      {!isLoading && !isError && employees.length === 0 && (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed p-16 text-center gap-3">
          <Users className="h-10 w-10 text-muted-foreground/40" />
          <div>
            <p className="font-medium">No employees found</p>
            <p className="text-sm text-muted-foreground">
              {branchFilter !== 'all' || typeFilter !== 'all'
                ? 'Try adjusting the filters'
                : 'Add employees to get started'}
            </p>
          </div>
          {branchFilter === 'all' && typeFilter === 'all' && (
            <Button variant="outline" onClick={() => setCreateOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              New Employee
            </Button>
          )}
        </div>
      )}

      {!isLoading && employees.length > 0 && (
        <DataTable
          columns={columns}
          data={employees}
          searchKey="fullName"
          searchPlaceholder="Search employees…"
        />
      )}

      <CreateEmployeeDialog open={createOpen} onOpenChange={setCreateOpen} />
    </div>
  )
}
