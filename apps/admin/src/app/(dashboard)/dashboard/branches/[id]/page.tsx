'use client'

import { use, useState } from 'react'
import { useRouter } from 'next/navigation'
import { ArrowLeft, Pencil, Power, PowerOff, MapPin, Phone, Hash } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
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
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useBranch, useUpdateBranch, useToggleBranchStatus } from '@/hooks/use-branches'
import { useEmployeesByBranch } from '@/hooks/use-employees'
import { useTransactionsByBranch } from '@/hooks/use-transactions'
import { BranchForm } from '../_components/branch-form'
import type { BranchFormValues } from '@/hooks/use-branches'
import { EmployeeType, TransactionStatus } from '@splashsphere/types'
import { toast } from 'sonner'
import { cn } from '@/lib/utils'

// ── helpers ──────────────────────────────────────────────────────────────────

function formatPHP(amount: number) {
  return new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' }).format(amount)
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-PH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

const STATUS_STYLES: Record<TransactionStatus, string> = {
  [TransactionStatus.Pending]: 'bg-yellow-500/15 text-yellow-700 border-yellow-200',
  [TransactionStatus.InProgress]: 'bg-blue-500/15 text-blue-700 border-blue-200',
  [TransactionStatus.Completed]: 'bg-green-500/15 text-green-700 border-green-200',
  [TransactionStatus.Cancelled]: 'bg-gray-500/15 text-gray-600 border-gray-200',
  [TransactionStatus.Refunded]: 'bg-red-500/15 text-red-700 border-red-200',
}

const STATUS_LABELS: Record<TransactionStatus, string> = {
  [TransactionStatus.Pending]: 'Pending',
  [TransactionStatus.InProgress]: 'In Progress',
  [TransactionStatus.Completed]: 'Completed',
  [TransactionStatus.Cancelled]: 'Cancelled',
  [TransactionStatus.Refunded]: 'Refunded',
}

// ── sub-components ────────────────────────────────────────────────────────────

function EmployeesTab({ branchId }: { branchId: string }) {
  const { data: employees, isLoading } = useEmployeesByBranch(branchId)

  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-10 w-full" />
        ))}
      </div>
    )
  }

  if (!employees || employees.length === 0) {
    return (
      <div className="rounded-lg border border-dashed p-12 text-center">
        <p className="text-muted-foreground">No employees assigned to this branch</p>
      </div>
    )
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Name</TableHead>
          <TableHead>Type</TableHead>
          <TableHead>Daily Rate</TableHead>
          <TableHead>Hired</TableHead>
          <TableHead>Status</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {employees.map((emp) => (
          <TableRow key={emp.id}>
            <TableCell className="font-medium">{emp.fullName}</TableCell>
            <TableCell>
              <Badge variant="outline" className="text-xs">
                {emp.employeeType === EmployeeType.Commission ? 'Commission' : 'Daily'}
              </Badge>
            </TableCell>
            <TableCell className="text-sm text-muted-foreground">
              {emp.dailyRate != null ? formatPHP(emp.dailyRate) : '—'}
            </TableCell>
            <TableCell className="text-sm text-muted-foreground">
              {emp.hiredDate ? formatDate(emp.hiredDate) : '—'}
            </TableCell>
            <TableCell>
              {emp.isActive ? (
                <Badge variant="default" className="bg-green-500/15 text-green-700 border-green-200 text-xs">
                  Active
                </Badge>
              ) : (
                <Badge variant="secondary" className="text-xs">Inactive</Badge>
              )}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}

function TransactionsTab({ branchId }: { branchId: string }) {
  const { data, isLoading } = useTransactionsByBranch(branchId)

  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-10 w-full" />
        ))}
      </div>
    )
  }

  if (!data || data.items.length === 0) {
    return (
      <div className="rounded-lg border border-dashed p-12 text-center">
        <p className="text-muted-foreground">No transactions for this branch yet</p>
      </div>
    )
  }

  return (
    <>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Transaction #</TableHead>
            <TableHead>Plate</TableHead>
            <TableHead>Vehicle</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Amount</TableHead>
            <TableHead>Date</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.items.map((tx) => (
            <TableRow key={tx.id}>
              <TableCell className="font-mono text-sm font-medium">
                {tx.transactionNumber}
              </TableCell>
              <TableCell className="font-mono text-sm">{tx.plateNumber}</TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {tx.vehicleTypeName} · {tx.sizeName}
              </TableCell>
              <TableCell>
                <Badge
                  variant="outline"
                  className={cn('text-xs', STATUS_STYLES[tx.status])}
                >
                  {STATUS_LABELS[tx.status]}
                </Badge>
              </TableCell>
              <TableCell className="text-right font-medium text-sm">
                {formatPHP(tx.finalAmount)}
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {formatDate(tx.createdAt)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <p className="text-xs text-muted-foreground text-right pt-2">
        Showing {data.items.length} of {data.totalCount} transactions
      </p>
    </>
  )
}

// ── main page ─────────────────────────────────────────────────────────────────

export default function BranchDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const router = useRouter()
  const [editOpen, setEditOpen] = useState(false)

  const { data: branch, isLoading, isError } = useBranch(id)
  const { mutateAsync: updateBranch } = useUpdateBranch(id)
  const { mutate: toggleStatus, isPending: isToggling } = useToggleBranchStatus()

  const handleUpdate = async (values: BranchFormValues) => {
    try {
      await updateBranch(values)
      toast.success('Branch updated')
      setEditOpen(false)
    } catch {
      toast.error('Failed to update branch')
    }
  }

  const handleToggle = () => {
    if (!branch) return
    toggleStatus(
      { id: branch.id, isActive: !branch.isActive },
      {
        onSuccess: () =>
          toast.success(`Branch ${branch.isActive ? 'deactivated' : 'activated'}`),
        onError: () => toast.error('Failed to update status'),
      }
    )
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

  if (isError || !branch) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" size="sm" onClick={() => router.back()}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back
        </Button>
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Branch not found or failed to load.
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
              <h1 className="text-2xl font-bold tracking-tight">{branch.name}</h1>
              {branch.isActive ? (
                <Badge className="bg-green-500/15 text-green-700 border-green-200">Active</Badge>
              ) : (
                <Badge variant="secondary">Inactive</Badge>
              )}
            </div>
            <div className="flex items-center gap-4 mt-1 text-sm text-muted-foreground">
              <span className="flex items-center gap-1">
                <Hash className="h-3.5 w-3.5" />
                {branch.code}
              </span>
              <span className="flex items-center gap-1">
                <MapPin className="h-3.5 w-3.5" />
                {branch.address}
              </span>
              <span className="flex items-center gap-1">
                <Phone className="h-3.5 w-3.5" />
                {branch.contactNumber}
              </span>
            </div>
          </div>
        </div>
        <div className="flex items-center gap-2 shrink-0">
          <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
            <Pencil className="mr-2 h-3.5 w-3.5" />
            Edit
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleToggle}
            disabled={isToggling}
          >
            {branch.isActive ? (
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
      <Tabs defaultValue="employees">
        <TabsList>
          <TabsTrigger value="employees">Employees</TabsTrigger>
          <TabsTrigger value="transactions">Transactions</TabsTrigger>
        </TabsList>

        <TabsContent value="employees" className="mt-4">
          <EmployeesTab branchId={id} />
        </TabsContent>

        <TabsContent value="transactions" className="mt-4">
          <TransactionsTab branchId={id} />
        </TabsContent>
      </Tabs>

      {/* Edit sheet */}
      <Sheet open={editOpen} onOpenChange={setEditOpen}>
        <SheetContent className="sm:max-w-md">
          <SheetHeader>
            <SheetTitle>Edit Branch</SheetTitle>
            <SheetDescription>Update the details for {branch.name}</SheetDescription>
          </SheetHeader>
          <div className="mt-6">
            <BranchForm
              defaultValues={{
                name: branch.name,
                code: branch.code,
                address: branch.address,
                contactNumber: branch.contactNumber,
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
