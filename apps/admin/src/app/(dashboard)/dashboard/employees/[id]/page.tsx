'use client'

import { use, useState } from 'react'
import { useAuth } from '@clerk/nextjs'
import { Pencil, Power, PowerOff, Mail, Phone, Calendar, KeyRound, Check, AlertCircle, Send, CheckCircle2, Clock, FileText, TrendingUp, CalendarCheck, Receipt, ShieldCheck } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { StatusBadge } from '@/components/ui/status-badge'
import { PageHeader } from '@/components/ui/page-header'
import { Skeleton } from '@/components/ui/skeleton'
import { SectionNav } from '@/components/ui/section-nav'
import { useSectionParam } from '@/hooks/use-section-param'
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import {
  useEmployee,
  useUpdateEmployee,
  useToggleEmployeeStatus,
  useEmployeeCommissions,
  useAttendance,
  useInviteEmployee,
  useEmployeePayrollHistory,
} from '@/hooks/use-employees'
import { EmployeeType } from '@splashsphere/types'
import type { Employee, EmployeeCommissionDto, AttendanceDto } from '@splashsphere/types'
import { EditEmployeeForm } from '../_components/employee-form'
import type { UpdateEmployeeValues } from '@/hooks/use-employees'
import { toast } from 'sonner'
import { apiClient } from '@/lib/api-client'
import { Input } from '@/components/ui/input'
import { DatePicker } from '@/components/ui/date-picker'
import { Label } from '@/components/ui/label'
import { formatPeso } from '@/lib/format'

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
            <StatusBadge status={emp.employeeType === EmployeeType.Commission ? 'Commission' : 'Daily'} />
          </dd>
        </div>

        {emp.employeeType === EmployeeType.Daily && emp.dailyRate != null && (
          <div>
            <dt className="text-sm text-muted-foreground">Daily rate</dt>
            <dd className="mt-0.5 font-medium">{formatPeso(emp.dailyRate)}</dd>
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
          <DatePicker value={from} onChange={setFrom} className="w-40" />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">To</label>
          <DatePicker value={to} onChange={setTo} className="w-40" />
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
                      {formatPeso(r.totalCommission)}
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="border-t bg-muted/50">
                <tr>
                  <td colSpan={3} className="px-4 py-3 font-medium">
                    Total ({rows.length} transactions)
                  </td>
                  <td className="px-4 py-3 text-right font-bold">{formatPeso(grandTotal)}</td>
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
          <DatePicker value={from} onChange={setFrom} className="w-40" />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">To</label>
          <DatePicker value={to} onChange={setTo} className="w-40" />
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

// ── Security tab (invitation + PIN management) ──────────────────────────────

function InvitationSection({ emp }: { emp: Employee }) {
  const { mutate: invite, isPending } = useInviteEmployee()
  const [dialogOpen, setDialogOpen] = useState(false)

  const handleInvite = () => {
    invite(emp.id, {
      onSuccess: () => {
        setDialogOpen(false)
        toast.success(`Invitation sent to ${emp.email}`)
      },
      onError: () => {
        setDialogOpen(false)
        toast.error('Failed to send invitation')
      },
    })
  }

  // Already linked — show success state
  if (emp.userId) {
    return (
      <div className="rounded-lg border border-green-200 bg-green-50 p-4 flex items-start gap-3">
        <CheckCircle2 className="h-5 w-5 text-green-600 shrink-0 mt-0.5" />
        <div>
          <p className="text-sm font-medium text-green-800">Account Linked</p>
          <p className="text-sm text-green-700 mt-1">
            This employee has a linked user account and can log into the system.
          </p>
        </div>
      </div>
    )
  }

  // No email — cannot invite
  if (!emp.email) {
    return (
      <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 flex items-start gap-3">
        <AlertCircle className="h-5 w-5 text-amber-600 shrink-0 mt-0.5" />
        <div>
          <p className="text-sm font-medium text-amber-800">Email required</p>
          <p className="text-sm text-amber-700 mt-1">
            Add an email address to this employee before sending an invitation.
            Use the Edit button above to add one.
          </p>
        </div>
      </div>
    )
  }

  // Invitation pending
  if (emp.invitedAt) {
    return (
      <div className="rounded-lg border border-blue-200 bg-blue-50 p-4 flex items-start gap-3">
        <Clock className="h-5 w-5 text-blue-600 shrink-0 mt-0.5" />
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <p className="text-sm font-medium text-blue-800">Invitation Pending</p>
            <Badge className="bg-blue-500/15 text-blue-700 border-blue-200 text-xs">Pending</Badge>
          </div>
          <p className="text-sm text-blue-700 mt-1">
            An invitation was sent to <span className="font-medium">{emp.email}</span> on{' '}
            {new Date(emp.invitedAt).toLocaleDateString('en-PH', {
              year: 'numeric',
              month: 'short',
              day: 'numeric',
              hour: '2-digit',
              minute: '2-digit',
            })}
            . The employee needs to accept the email invitation to create their account.
          </p>
          <AlertDialog open={dialogOpen} onOpenChange={setDialogOpen}>
            <AlertDialogTrigger
              render={<Button variant="outline" size="sm" className="mt-3" disabled={isPending} />}
            >
              <Send className="mr-2 h-3.5 w-3.5" />
              {isPending ? 'Sending…' : 'Resend Invitation'}
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Resend invitation?</AlertDialogTitle>
                <AlertDialogDescription>
                  A new invitation email will be sent to <strong>{emp.email}</strong>.
                  The previous invitation will be replaced.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction onClick={handleInvite} disabled={isPending}>
                  {isPending ? 'Sending…' : 'Resend'}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </div>
    )
  }

  // Not invited yet
  return (
    <div className="rounded-lg border border-amber-200 bg-amber-50 p-4 flex items-start gap-3">
      <AlertCircle className="h-5 w-5 text-amber-600 shrink-0 mt-0.5" />
      <div className="flex-1">
        <p className="text-sm font-medium text-amber-800">No linked user account</p>
        <p className="text-sm text-amber-700 mt-1">
          Send an invitation to <span className="font-medium">{emp.email}</span> so they can create
          a system account. Once accepted, you can set their POS PIN.
        </p>
        <AlertDialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <AlertDialogTrigger
            render={<Button size="sm" className="mt-3" disabled={isPending} />}
          >
            <Send className="mr-2 h-3.5 w-3.5" />
            {isPending ? 'Sending…' : 'Send Invitation'}
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Send invitation?</AlertDialogTitle>
              <AlertDialogDescription>
                An invitation email will be sent to <strong>{emp.email}</strong>.
                They will be able to create an account and join the organization.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction onClick={handleInvite} disabled={isPending}>
                {isPending ? 'Sending…' : 'Send Invitation'}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>
  )
}

function PinManagementSection({ emp }: { emp: Employee }) {
  const { getToken } = useAuth()
  const [pin, setPin] = useState('')
  const [confirm, setConfirm] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const pinValid = /^\d{6}$/.test(pin)
  const pinsMatch = pin === confirm
  const canSave = pinValid && pinsMatch && !saving

  const handleSetPin = async () => {
    if (!canSave || !emp.userId) return
    setSaving(true)
    setError(null)
    setSuccess(false)
    try {
      const token = await getToken()
      await apiClient.patch(`/auth/users/${emp.userId}/pin`, { pin }, token ?? undefined)
      setSuccess(true)
      setPin('')
      setConfirm('')
      toast.success('PIN updated successfully')
    } catch {
      setError('Failed to set PIN. Please try again.')
      toast.error('Failed to set PIN')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="max-w-sm space-y-5">
      <div>
        <h3 className="text-sm font-medium mb-1 flex items-center gap-2">
          <KeyRound className="h-4 w-4 text-muted-foreground" />
          POS Lock PIN
        </h3>
        <p className="text-sm text-muted-foreground">
          Set a 6-digit PIN for this employee to unlock the POS screen.
        </p>
      </div>

      <div className="space-y-3">
        <div className="space-y-1.5">
          <Label htmlFor="pin">New PIN</Label>
          <Input
            id="pin"
            type="password"
            inputMode="numeric"
            maxLength={6}
            placeholder="Enter 6-digit PIN"
            value={pin}
            onChange={(e) => {
              const v = e.target.value.replace(/\D/g, '').slice(0, 6)
              setPin(v)
              setError(null)
              setSuccess(false)
            }}
          />
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="confirm-pin">Confirm PIN</Label>
          <Input
            id="confirm-pin"
            type="password"
            inputMode="numeric"
            maxLength={6}
            placeholder="Re-enter PIN"
            value={confirm}
            onChange={(e) => {
              const v = e.target.value.replace(/\D/g, '').slice(0, 6)
              setConfirm(v)
              setError(null)
              setSuccess(false)
            }}
          />
          {confirm.length > 0 && !pinsMatch && (
            <p className="text-xs text-destructive">PINs do not match</p>
          )}
        </div>
      </div>

      {error && (
        <p className="text-sm text-destructive">{error}</p>
      )}

      {success && (
        <div className="flex items-center gap-2 text-sm text-green-600">
          <Check className="h-4 w-4" />
          PIN has been set successfully.
        </div>
      )}

      <Button onClick={handleSetPin} disabled={!canSave} size="sm">
        <KeyRound className="mr-2 h-3.5 w-3.5" />
        {saving ? 'Saving…' : 'Set PIN'}
      </Button>
    </div>
  )
}

// ── Payroll History tab ───────────────────────────────────────────────────────

const PAYROLL_STATUS_LABELS: Record<number, string> = {
  1: 'Open',
  2: 'Closed',
  3: 'Processed',
  4: 'Released',
}

function PayrollHistoryTab({ employeeId }: { employeeId: string }) {
  const [page, setPage] = useState(1)
  const { data, isLoading } = useEmployeePayrollHistory(employeeId, page)

  if (isLoading) return <Skeleton className="h-48 w-full" />

  const items = data?.items ?? []
  const totalPages = data ? Math.ceil(data.totalCount / data.pageSize) : 0

  if (items.length === 0) {
    return (
      <p className="text-sm text-muted-foreground py-8 text-center">
        No payroll records found for this employee.
      </p>
    )
  }

  return (
    <div className="space-y-4">
      <div className="overflow-x-auto rounded-lg border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left text-xs text-muted-foreground">
            <tr>
              <th className="px-4 py-2">Period</th>
              <th className="px-4 py-2">Status</th>
              <th className="px-4 py-2 text-center">Days</th>
              <th className="px-4 py-2 text-right">Base</th>
              <th className="px-4 py-2 text-right">Commissions</th>
              <th className="px-4 py-2 text-right">Bonuses</th>
              <th className="px-4 py-2 text-right">Deductions</th>
              <th className="px-4 py-2 text-right font-semibold">Net Pay</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {items.map((h) => (
              <tr key={h.entryId} className="hover:bg-muted/30">
                <td className="px-4 py-2.5">
                  <a
                    href={`/dashboard/payroll/${h.periodId}`}
                    className="text-primary hover:underline"
                  >
                    {new Date(h.periodStart).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' })}
                    {' – '}
                    {new Date(h.periodEnd).toLocaleDateString('en-PH', { month: 'short', day: 'numeric', year: 'numeric' })}
                  </a>
                </td>
                <td className="px-4 py-2.5">
                  <StatusBadge status={PAYROLL_STATUS_LABELS[h.periodStatus] ?? 'Unknown'} />
                </td>
                <td className="px-4 py-2.5 text-center tabular-nums">{h.daysWorked}</td>
                <td className="px-4 py-2.5 text-right tabular-nums">{formatPeso(h.baseSalary)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums">{formatPeso(h.totalCommissions)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums">{formatPeso(h.bonuses)}</td>
                <td className="px-4 py-2.5 text-right tabular-nums">{formatPeso(h.deductions)}</td>
                <td className="px-4 py-2.5 text-right font-semibold tabular-nums">{formatPeso(h.netPay)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2">
          <Button
            size="sm"
            variant="outline"
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
          >
            Previous
          </Button>
          <span className="text-xs text-muted-foreground">
            Page {page} of {totalPages}
          </span>
          <Button
            size="sm"
            variant="outline"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  )
}

// ── Security tab ─────────────────────────────────────────────────────────────

function SecurityTab({ emp }: { emp: Employee }) {
  return (
    <div className="max-w-lg space-y-6">
      <InvitationSection emp={emp} />
      {emp.userId && <PinManagementSection emp={emp} />}
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
  const [editOpen, setEditOpen] = useState(false)
  const [section] = useSectionParam('section', 'details')

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
        <PageHeader title="Employee" back />
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Employee not found or failed to load.
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={emp.fullName}
        description={emp.branchName}
        back
        badge={
          <>
            <StatusBadge status={emp.isActive ? 'Active' : 'Inactive'} />
            <StatusBadge status={emp.employeeType === EmployeeType.Commission ? 'Commission' : 'Daily'} />
          </>
        }
        actions={
          <>
            <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
              <Pencil className="mr-2 h-3.5 w-3.5" />
              Edit
            </Button>
            <Button variant="outline" size="sm" onClick={handleToggle} disabled={isToggling}>
              {emp.isActive ? (
                <><PowerOff className="mr-2 h-3.5 w-3.5" />Deactivate</>
              ) : (
                <><Power className="mr-2 h-3.5 w-3.5" />Activate</>
              )}
            </Button>
          </>
        }
      />

      {/* Section nav + content */}
      <div className="flex flex-col gap-6 md:flex-row md:gap-8">
        <SectionNav
          className="md:w-56 md:shrink-0"
          defaultValue="details"
          items={[
            { value: 'details', label: 'Details', icon: FileText },
            { value: 'commissions', label: 'Commission History', icon: TrendingUp },
            { value: 'attendance', label: 'Attendance', icon: CalendarCheck },
            { value: 'payroll', label: 'Payroll History', icon: Receipt },
            { value: 'security', label: 'Security', icon: ShieldCheck },
          ]}
        />

        <div className="min-w-0 flex-1">
          {section === 'details' && <DetailsTab emp={emp} />}
          {section === 'commissions' && <CommissionsTab employeeId={id} />}
          {section === 'attendance' && <AttendanceTab employeeId={id} />}
          {section === 'payroll' && <PayrollHistoryTab employeeId={id} />}
          {section === 'security' && <SecurityTab emp={emp} />}
        </div>
      </div>

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
