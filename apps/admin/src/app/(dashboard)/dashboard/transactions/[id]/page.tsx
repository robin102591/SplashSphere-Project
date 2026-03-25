'use client'

import { use } from 'react'
import { Car, User, MapPin, ExternalLink } from 'lucide-react'
import { StatusBadge } from '@/components/ui/status-badge'
import { PageHeader } from '@/components/ui/page-header'
import { Skeleton } from '@/components/ui/skeleton'
import { useTransaction } from '@/hooks/use-transactions'
import { TransactionStatus, PaymentMethod } from '@splashsphere/types'
import type {
  TransactionDetail,
  TransactionServiceLine,
  TransactionPackageLine,
  TransactionMerchandiseLine,
  TransactionEmployeeSummary,
  Payment,
} from '@splashsphere/types'
import Link from 'next/link'
import { formatPeso } from '@/lib/format'

const STATUS_KEYS: Record<TransactionStatus, string> = {
  [TransactionStatus.Pending]: 'Pending',
  [TransactionStatus.InProgress]: 'InProgress',
  [TransactionStatus.Completed]: 'Completed',
  [TransactionStatus.Cancelled]: 'Cancelled',
  [TransactionStatus.Refunded]: 'Refunded',
}

const PAYMENT_LABELS: Record<PaymentMethod, string> = {
  [PaymentMethod.Cash]: 'Cash',
  [PaymentMethod.GCash]: 'GCash / Maya',
  [PaymentMethod.CreditCard]: 'Credit Card',
  [PaymentMethod.DebitCard]: 'Debit Card',
  [PaymentMethod.BankTransfer]: 'Bank Transfer',
}

// ── Service lines section ─────────────────────────────────────────────────────

function ServiceLines({ services }: { services: readonly TransactionServiceLine[] }) {
  if (services.length === 0) return null
  return (
    <section className="space-y-2">
      <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Services ({services.length})
      </h3>
      <div className="rounded-lg border divide-y">
        {services.map((svc) => (
          <div key={svc.id} className="px-4 py-3">
            <div className="flex items-start justify-between gap-4">
              <div>
                <p className="font-medium text-sm">{svc.serviceName}</p>
                <p className="text-xs text-muted-foreground">
                  {svc.vehicleTypeName} · {svc.sizeName}
                </p>
              </div>
              <div className="text-right shrink-0">
                <p className="font-semibold text-sm tabular-nums">{formatPeso(svc.unitPrice)}</p>
                <p className="text-xs text-muted-foreground">
                  commission: {formatPeso(svc.totalCommission)}
                </p>
              </div>
            </div>
            {svc.employeeAssignments.length > 0 && (
              <div className="mt-2 flex flex-wrap gap-1.5">
                {svc.employeeAssignments.map((a) => (
                  <span
                    key={a.id}
                    className="inline-flex items-center gap-1 rounded-full bg-muted px-2 py-0.5 text-xs"
                  >
                    {a.employeeName}
                    <span className="text-muted-foreground">
                      {formatPeso(a.commissionAmount)}
                    </span>
                  </span>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </section>
  )
}

// ── Package lines section ─────────────────────────────────────────────────────

function PackageLines({ packages }: { packages: readonly TransactionPackageLine[] }) {
  if (packages.length === 0) return null
  return (
    <section className="space-y-2">
      <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Packages ({packages.length})
      </h3>
      <div className="rounded-lg border divide-y">
        {packages.map((pkg) => (
          <div key={pkg.id} className="px-4 py-3">
            <div className="flex items-start justify-between gap-4">
              <div>
                <p className="font-medium text-sm">{pkg.packageName}</p>
                <p className="text-xs text-muted-foreground">
                  {pkg.vehicleTypeName} · {pkg.sizeName}
                </p>
              </div>
              <div className="text-right shrink-0">
                <p className="font-semibold text-sm tabular-nums">{formatPeso(pkg.unitPrice)}</p>
                <p className="text-xs text-muted-foreground">
                  commission: {formatPeso(pkg.totalCommission)}
                </p>
              </div>
            </div>
            {pkg.employeeAssignments.length > 0 && (
              <div className="mt-2 flex flex-wrap gap-1.5">
                {pkg.employeeAssignments.map((a) => (
                  <span
                    key={a.id}
                    className="inline-flex items-center gap-1 rounded-full bg-muted px-2 py-0.5 text-xs"
                  >
                    {a.employeeName}
                    <span className="text-muted-foreground">
                      {formatPeso(a.commissionAmount)}
                    </span>
                  </span>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </section>
  )
}

// ── Merchandise lines section ─────────────────────────────────────────────────

function MerchandiseLines({ merchandise }: { merchandise: readonly TransactionMerchandiseLine[] }) {
  if (merchandise.length === 0) return null
  return (
    <section className="space-y-2">
      <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Merchandise ({merchandise.length})
      </h3>
      <div className="rounded-lg border divide-y">
        {merchandise.map((m) => (
          <div key={m.id} className="flex items-center justify-between px-4 py-3">
            <div>
              <p className="font-medium text-sm">{m.merchandiseName}</p>
              <p className="text-xs text-muted-foreground">
                {formatPeso(m.unitPrice)} × {m.quantity}
              </p>
            </div>
            <p className="font-semibold text-sm tabular-nums">{formatPeso(m.lineTotal)}</p>
          </div>
        ))}
      </div>
    </section>
  )
}

// ── Employee commission summary ───────────────────────────────────────────────

function EmployeeSummary({ employees }: { employees: readonly TransactionEmployeeSummary[] }) {
  if (employees.length === 0) return null
  const total = employees.reduce((s, e) => s + e.totalCommission, 0)
  return (
    <section className="space-y-2">
      <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Employee Commissions
      </h3>
      <div className="rounded-lg border divide-y">
        {employees.map((e) => (
          <div key={e.id} className="flex items-center justify-between px-4 py-2.5 text-sm">
            <span>{e.employeeName}</span>
            <span className="font-medium tabular-nums">{formatPeso(e.totalCommission)}</span>
          </div>
        ))}
        <div className="flex items-center justify-between px-4 py-2.5 text-sm bg-muted/50 font-semibold">
          <span>Total commissions</span>
          <span className="tabular-nums">{formatPeso(total)}</span>
        </div>
      </div>
    </section>
  )
}

// ── Payments section ──────────────────────────────────────────────────────────

function PaymentsSection({ payments, finalAmount }: { payments: readonly Payment[]; finalAmount: number }) {
  const paid = payments.reduce((s, p) => s + p.amount, 0)
  const change = paid - finalAmount

  return (
    <section className="space-y-2">
      <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Payments
      </h3>
      {payments.length === 0 ? (
        <p className="text-sm text-muted-foreground italic px-1">No payments recorded</p>
      ) : (
        <div className="rounded-lg border divide-y">
          {payments.map((p) => (
            <div key={p.id} className="flex items-center justify-between px-4 py-2.5 text-sm">
              <div>
                <span className="font-medium">{PAYMENT_LABELS[p.method]}</span>
                {p.reference && (
                  <span className="ml-2 text-xs text-muted-foreground">ref: {p.reference}</span>
                )}
                <p className="text-xs text-muted-foreground">
                  {new Date(p.createdAt).toLocaleTimeString('en-PH', { hour: '2-digit', minute: '2-digit' })}
                </p>
              </div>
              <span className="font-medium tabular-nums">{formatPeso(p.amount)}</span>
            </div>
          ))}
          {change > 0 && (
            <div className="flex items-center justify-between px-4 py-2.5 text-sm bg-muted/30 text-muted-foreground">
              <span>Change</span>
              <span className="tabular-nums">{formatPeso(change)}</span>
            </div>
          )}
        </div>
      )}
    </section>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function TransactionDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)

  const { data: tx, isLoading, isError } = useTransaction(id)

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-96" />
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  if (isError || !tx) {
    return (
      <div className="space-y-4">
        <PageHeader title="Error" back />
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-6 text-sm text-destructive">
          Transaction not found or failed to load.
        </div>
      </div>
    )
  }

  const date = new Date(tx.createdAt).toLocaleString('en-PH', {
    year: 'numeric', month: 'long', day: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })

  return (
    <div className="space-y-6">
      {/* Header */}
      <PageHeader
        title={tx.transactionNumber}
        description={date}
        back
        badge={<StatusBadge status={STATUS_KEYS[tx.status]} />}
      />

      {/* Summary bar */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="rounded-lg border px-4 py-3">
          <p className="text-xs text-muted-foreground">Subtotal</p>
          <p className="font-semibold tabular-nums mt-0.5">{formatPeso(tx.totalAmount)}</p>
        </div>
        {tx.discountAmount > 0 && (
          <div className="rounded-lg border px-4 py-3">
            <p className="text-xs text-muted-foreground">Discount</p>
            <p className="font-semibold tabular-nums mt-0.5 text-green-700">−{formatPeso(tx.discountAmount)}</p>
          </div>
        )}
        {tx.taxAmount > 0 && (
          <div className="rounded-lg border px-4 py-3">
            <p className="text-xs text-muted-foreground">Tax</p>
            <p className="font-semibold tabular-nums mt-0.5">{formatPeso(tx.taxAmount)}</p>
          </div>
        )}
        <div className="rounded-lg border border-primary/30 bg-primary/5 px-4 py-3">
          <p className="text-xs text-muted-foreground">Total</p>
          <p className="text-lg font-bold tabular-nums mt-0.5 text-primary">{formatPeso(tx.finalAmount)}</p>
        </div>
      </div>

      {/* Info cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="rounded-lg border px-4 py-3 space-y-1">
          <p className="text-xs text-muted-foreground flex items-center gap-1"><Car className="h-3.5 w-3.5" /> Vehicle</p>
          <p className="font-mono font-semibold">{tx.plateNumber}</p>
          <p className="text-xs text-muted-foreground">{tx.vehicleTypeName} · {tx.sizeName}</p>
          <Link href={`/dashboard/vehicles/${tx.carId}`} className="text-xs text-primary hover:underline flex items-center gap-1">
            View vehicle <ExternalLink className="h-3 w-3" />
          </Link>
        </div>
        <div className="rounded-lg border px-4 py-3 space-y-1">
          <p className="text-xs text-muted-foreground flex items-center gap-1"><User className="h-3.5 w-3.5" /> Customer</p>
          {tx.customerName && tx.customerId ? (
            <>
              <p className="font-medium">{tx.customerName}</p>
              <Link href={`/dashboard/customers/${tx.customerId}`} className="text-xs text-primary hover:underline flex items-center gap-1">
                View profile <ExternalLink className="h-3 w-3" />
              </Link>
            </>
          ) : (
            <p className="text-sm text-muted-foreground italic">Walk-in</p>
          )}
        </div>
        <div className="rounded-lg border px-4 py-3 space-y-1">
          <p className="text-xs text-muted-foreground flex items-center gap-1"><MapPin className="h-3.5 w-3.5" /> Branch</p>
          <p className="font-medium">{tx.branchName}</p>
          <p className="text-xs text-muted-foreground">Cashier: {tx.cashierName}</p>
          {tx.notes && <p className="text-xs text-muted-foreground">Note: {tx.notes}</p>}
        </div>
      </div>

      {/* Line items */}
      <ServiceLines services={tx.services} />
      <PackageLines packages={tx.packages} />
      <MerchandiseLines merchandise={tx.merchandise} />

      {/* Employee commissions + payments side by side on large screens */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
        <EmployeeSummary employees={tx.employees} />
        <PaymentsSection payments={tx.payments} finalAmount={tx.finalAmount} />
      </div>
    </div>
  )
}
