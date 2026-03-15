'use client'

import { use, useState } from 'react'
import { useRouter } from 'next/navigation'
import { ArrowLeft, Pencil, Power, PowerOff, Mail, Phone, Calendar } from 'lucide-react'
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
  useEmployee,
  useUpdateEmployee,
  useToggleEmployeeStatus,
  useEmployeeCommissions,
  useAttendance,
} from '@/hooks/use-employees'
import { EmployeeType } from '@splashsphere/types'
import type { Employee, EmployeeCommissionDto, AttendanceDto } from '@splashsphere/types'
import { EditEmployeeForm } from '../_components/employee-form'
import type { UpdateEmployeeValues } from '@/hooks/use-employees'
import { toast } from 'sonner'

const php = new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' })

function formatDate(iso: string) {
  return new Date(iso).toLocaleString('en-PH', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// ── Details tab ───────────────────────────────────────────────────────────────

function DetailsTab({ emp }: { emp: Employee }) {
  return (
    <div className="space-y-6 max-w-lg">
      <dl className="grid grid-cols-2 gap-x-8 gap-y-4">
        <div>
          <dt className="text-sm text-muted-foreground">Branch</dt>
          <dd className="mt-0.5 font-medium">{emp.branchName}</dd>
        </div>
        <div>
          <dt className="text-sm text-muted-foreground">Employee type</dt>
          <dd className="mt-0.5">
            {emp.employeeType === EmployeeType.Commission ? (
              <Badge className="bg-blue-500/15 text-blue-700 border-blue-200">Commission</Badge>
            ) : (
              <Badge variant="outline">Daily Rate</Badge>
            )}
          </dd>
        </div>

        {emp.employeeType === EmployeeType.Daily && emp.dailyRate != null && (
          <div>
            <dt className="text-sm text-muted-foreground">Daily rate</dt>
            <dd className="mt-0.5 font-medium">{php.format(emp.dailyRate)}</dd>
          </div>
        )}

        {emp.hiredDate && (
          <div>
            <dt className="text-sm text-muted-foreground">Hired date</dt>
            <dd className="mt-0.5 flex items-center gap-1.5 text-sm">
              <Calendar className="h-3.5 w-3.5 text-muted-foreground" />
              {new Date(emp.hiredDate).toLocaleDateString('en-PH', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
              })}
            </dd>
          </div>
        )}

        {emp.email && (
          <div>
            <dt className="text-sm text-muted-foreground">Email</dt>
            <dd className="mt-0.5 flex items-center gap-1.5 text-sm">
              <Mail className="h-3.5 w-3.5 text-muted-foreground" />
              {emp.email}
            </dd>
          </div>
        )}

        {emp.contactNumber && (
          <div>
            <dt className="text-sm text-muted-foreground">Contact</dt>
            <dd className="mt-0.5 flex items-center gap-1.5 text-sm">
              <Phone className="h-3.5 w-3.5 text-muted-foreground" />
              {emp.contactNumber}
            </dd>
          </div>
        )}

        <div>
          <dt className="text-sm text-muted-foreground">Created</dt>
          <dd className="mt-0.5 text-sm">{formatDate(emp.createdAt)}</dd>
        </div>
        <div>
          <dt className="text-sm text-muted-foreground">Last updated</dt>
          <dd className="mt-0.5 text-sm">{formatDate(emp.updatedAt)}</dd>
        </div>
      </dl>
    </div>
  )
}

// ── Commission history tab ────────────────────────────────────────────────────

function CommissionsTab({ employeeId }: { employeeId: string }) {
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')

  const { data, isLoading } = useEmployeeCommissions(employeeId, {
    from: from || undefined,
    to: to || undefined,
    pageSize: 50,
  })

  const rows: EmployeeCommissionDto[] = data ? [...data.items] : []
  const grandTotal = rows.reduce((sum, r) => sum + r.totalCommission, 0)

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">From</label>
          <input
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            className="h-9 rounded-md border border-input bg-background px-3 text-sm"
          />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">To</label>
          <input
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            className="h-9 rounded-md border border-input bg-background px-3 text-sm"
          />
        </div>
      </div>

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-10 w-full" />
          ))}
        </div>
      ) : rows.length === 0 ? (
        <p className="text-sm text-muted-foreground italic py-4">No commission records found.</p>
      ) : (
        <>
          <div className="rounded-lg border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-4 py-3 text-left font-medium">Transaction #</th>
                  <th className="px-4 py-3 text-left font-medium">Date</th>
                  <th className="px-4 py-3 text-left font-medium">Branch</th>
                  <th className="px-4 py-3 text-right font-medium">Commission</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {rows.map((r) => (
                  <tr key={r.transactionId} className="hover:bg-muted/30">
                    <td className="px-4 py-3 font-mono text-xs">{r.transactionNumber}</td>
                    <td className="px-4 py-3 text-muted-foreground">
                      {new Date(r.transactionDate).toLocaleDateString('en-PH', {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                      })}
                    </td>
                    <td className="px-4 py-3 text-muted-foreground">{r.branchName}</td>
                    <td className="px-4 py-3 text-right font-medium">
                      {php.format(r.totalCommission)}
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="border-t bg-muted/50">
                <tr>
                  <td colSpan={3} className="px-4 py-3 font-medium">
                    Total ({rows.length} transactions)
                  </td>
                  <td className="px-4 py-3 text-right font-bold">{php.format(grandTotal)}</td>
                </tr>
              </tfoot>
            </table>
          </div>
          {data && data.totalCount > rows.length && (
            <p className="text-xs text-muted-foreground text-center">
              Showing {rows.length} of {data.totalCount} records. Narrow the date range to see
              more.
            </p>
          )}
        </>
      )}
    </div>
  )
}

// ── Attendance tab ────────────────────────────────────────────────────────────

function AttendanceTab({ employeeId }: { employeeId: string }) {
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')

  const { data, isLoading } = useAttendance({
    employeeId,
    from: from || undefined,
    to: to || undefined,
    pageSize: 50,
  })

  const rows: AttendanceDto[] = data ? [...data.items] : []

  function formatTime(iso: string) {
    return new Date(iso).toLocaleTimeString('en-PH', {
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">From</label>
          <input
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            className="h-9 rounded-md border border-input bg-background px-3 text-sm"
          />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">To</label>
          <input
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            className="h-9 rounded-md border border-input bg-background px-3 text-sm"
          />
        </div>
      </div>

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-10 w-full" />
          ))}
        </div>
      ) : rows.length === 0 ? (
        <p className="text-sm text-muted-foreground italic py-4">No attendance records found.</p>
      ) : (
        <>
          <div className="rounded-lg border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-4 py-3 text-left font-medium">Date</th>
                  <th className="px-4 py-3 text-left font-medium">Time In</th>
                  <th className="px-4 py-3 text-left font-medium">Time Out</th>
                  <th className="px-4 py-3 text-left font-medium">Notes</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {rows.map((r) => (
                  <tr key={r.id} className="hover:bg-muted/30">
                    <td className="px-4 py-3">
                      {new Date(r.date).toLocaleDateString('en-PH', {
                        weekday: 'short',
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                      })}
                    </td>
                    <td className="px-4 py-3">{formatTime(r.timeIn)}</td>
                    <td className="px-4 py-3">
                      {r.timeOut ? (
                        formatTime(r.timeOut)
                      ) : (
                        <Badge variant="outline" className="text-xs">
                          Active
                        </Badge>
                      )}
                    </td>
                    <td className="px-4 py-3 text-muted-foreground text-xs">
                      {r.notes ?? '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {data && data.totalCount > rows.length && (
            <p className="text-xs text-muted-foreground text-center">
              Showing {rows.length} of {data.totalCount} records. Narrow the date range to see
              more.
            </p>
          )}
        </>
      )}
    </div>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function EmployeeDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = use(params)
  const router = useRouter()
  const [editOpen, setEditOpen] = useState(false)

  const { data: emp, isLoading, isError } = useEmployee(id)
  const { mutate: toggleStatus, isPending: isToggling } = useToggleEmployeeStatus()
  const { mutateAsync: updateEmployee } = useUpdateEmployee(id)

  const handleToggle = () => {
    if (!emp) return
    toggleStatus(id, {
      onSuccess: () => toast.success(`Employee ${emp.isActive ? 'deactivated' : 'activated'}`),
      onError: () => toast.error('Failed to update status'),
    })
  }

  const handleUpdate = async (values: UpdateEmployeeValues) => {
    try {
      await updateEmployee(values)
      toast.success('Employee updated')
      setEditOpen(false)
    } catch {
      toast.error('Failed to update employee')
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

  if (isError || !emp) {
    return (
      <div className="space-y-4">
        <Button variant="ghost" size="sm" onClick={() => router.back()}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back
        </Button>
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Employee not found or failed to load.
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
              <h1 className="text-2xl font-bold tracking-tight">{emp.fullName}</h1>
              {emp.isActive ? (
                <Badge className="bg-green-500/15 text-green-700 border-green-200">Active</Badge>
              ) : (
                <Badge variant="secondary">Inactive</Badge>
              )}
              {emp.employeeType === EmployeeType.Commission ? (
                <Badge className="bg-blue-500/15 text-blue-700 border-blue-200">Commission</Badge>
              ) : (
                <Badge variant="outline">Daily Rate</Badge>
              )}
            </div>
            <p className="text-sm text-muted-foreground mt-0.5">{emp.branchName}</p>
          </div>
        </div>
        <div className="flex items-center gap-2 shrink-0">
          <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
            <Pencil className="mr-2 h-3.5 w-3.5" />
            Edit
          </Button>
          <Button variant="outline" size="sm" onClick={handleToggle} disabled={isToggling}>
            {emp.isActive ? (
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
          <TabsTrigger value="commissions">Commission History</TabsTrigger>
          <TabsTrigger value="attendance">Attendance</TabsTrigger>
        </TabsList>

        <TabsContent value="details" className="mt-6">
          <DetailsTab emp={emp} />
        </TabsContent>

        <TabsContent value="commissions" className="mt-6">
          <CommissionsTab employeeId={id} />
        </TabsContent>

        <TabsContent value="attendance" className="mt-6">
          <AttendanceTab employeeId={id} />
        </TabsContent>
      </Tabs>

      {/* Edit sheet */}
      <Sheet open={editOpen} onOpenChange={setEditOpen}>
        <SheetContent className="sm:max-w-lg overflow-y-auto">
          <SheetHeader>
            <SheetTitle>Edit Employee</SheetTitle>
            <SheetDescription>Update employee details. Employee type is immutable.</SheetDescription>
          </SheetHeader>
          <div className="mt-6">
            <EditEmployeeForm
              defaultValues={{
                firstName: emp.firstName,
                lastName: emp.lastName,
                dailyRate: emp.dailyRate ?? undefined,
                email: emp.email ?? '',
                contactNumber: emp.contactNumber ?? '',
                hiredDate: emp.hiredDate ?? '',
                employeeType: emp.employeeType,
              }}
              onSubmit={handleUpdate}
            />
          </div>
        </SheetContent>
      </Sheet>
    </div>
  )
}
