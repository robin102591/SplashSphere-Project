'use client'

import { use, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, Printer, Car, User2, Wrench, Package,
  ShoppingCart, Users, CreditCard, CheckCircle2,
  XCircle, RotateCcw, Clock, BadgeCheck, Edit2,
  Banknote, Smartphone, Building2, RefreshCw, AlertCircle,
} from 'lucide-react'
import Link from 'next/link'
import type { TransactionDetail } from '@splashsphere/types'
import { TransactionStatus, PaymentMethod } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { cn } from '@/lib/utils'

// ── Config ─────────────────────────────────────────────────────────────────────

const TX_STATUS: Record<number, { label: string; icon: React.ReactNode; cls: string }> = {
  [TransactionStatus.Pending]:    { label: 'Pending',     icon: <Clock className="h-3.5 w-3.5" />,        cls: 'bg-yellow-500/20 text-yellow-300 border-yellow-500/30' },
  [TransactionStatus.InProgress]: { label: 'In Progress', icon: <Wrench className="h-3.5 w-3.5" />,       cls: 'bg-blue-500/20 text-blue-300 border-blue-500/30' },
  [TransactionStatus.Completed]:  { label: 'Completed',   icon: <CheckCircle2 className="h-3.5 w-3.5" />, cls: 'bg-green-500/20 text-green-300 border-green-500/30' },
  [TransactionStatus.Cancelled]:  { label: 'Cancelled',   icon: <XCircle className="h-3.5 w-3.5" />,      cls: 'bg-red-500/20 text-red-300 border-red-500/30' },
  [TransactionStatus.Refunded]:   { label: 'Refunded',    icon: <RotateCcw className="h-3.5 w-3.5" />,    cls: 'bg-purple-500/20 text-purple-300 border-purple-500/30' },
}

const PAYMENT_METHODS: { value: PaymentMethod; label: string; Icon: React.ComponentType<{ className?: string }> }[] = [
  { value: PaymentMethod.Cash,         label: 'Cash',   Icon: Banknote   },
  { value: PaymentMethod.GCash,        label: 'GCash',  Icon: Smartphone },
  { value: PaymentMethod.CreditCard,   label: 'Credit', Icon: CreditCard },
  { value: PaymentMethod.DebitCard,    label: 'Debit',  Icon: CreditCard },
  { value: PaymentMethod.BankTransfer, label: 'Bank',   Icon: Building2  },
]

const PAYMENT_LABEL: Record<number, string> = {
  [PaymentMethod.Cash]:         'Cash',
  [PaymentMethod.GCash]:        'GCash / Maya',
  [PaymentMethod.CreditCard]:   'Credit Card',
  [PaymentMethod.DebitCard]:    'Debit Card',
  [PaymentMethod.BankTransfer]: 'Bank Transfer',
}

function fmt(amount: number) {
  return `₱${amount.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function fmtDateTime(iso: string) {
  return new Date(iso).toLocaleString('en-PH', {
    month: 'short', day: 'numeric', year: 'numeric',
    hour: 'numeric', minute: '2-digit', hour12: true,
  })
}

// ── Page ───────────────────────────────────────────────────────────────────────

interface Props {
  params: Promise<{ id: string }>
}

export default function TransactionDetailPage({ params }: Props) {
  const { id } = use(params)
  const router = useRouter()
  const { getToken } = useAuth()
  const queryClient = useQueryClient()

  // ── Payment form state ─────────────────────────────────────────────────────
  const [showPayForm, setShowPayForm] = useState(false)
  const [payMethod, setPayMethod] = useState<PaymentMethod>(PaymentMethod.Cash)
  const [payAmount, setPayAmount] = useState('')
  const [payRef, setPayRef] = useState('')
  const [payError, setPayError] = useState<string | null>(null)
  const [isAddingPayment, setIsAddingPayment] = useState(false)

  // ── Queries ────────────────────────────────────────────────────────────────
  const { data: tx, isLoading, error } = useQuery({
    queryKey: ['transaction', id],
    staleTime: 30_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<TransactionDetail>(`/transactions/${id}`, token ?? undefined)
    },
  })

  // ── Mutations ──────────────────────────────────────────────────────────────
  const cancelMutation = useMutation({
    mutationFn: async () => {
      const token = await getToken()
      return apiClient.patch(
        `/transactions/${id}/status`,
        { newStatus: TransactionStatus.Cancelled },
        token ?? undefined
      )
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transaction', id] })
      queryClient.invalidateQueries({ queryKey: ['transactions-recent'] })
    },
  })

  const handleAddPayment = async () => {
    const amount = parseFloat(payAmount)
    if (!amount || amount <= 0) return
    setIsAddingPayment(true)
    setPayError(null)
    try {
      const token = await getToken()
      await apiClient.post(
        `/transactions/${id}/payments`,
        { paymentMethod: payMethod, amount, referenceNumber: payRef || null },
        token ?? undefined
      )
      setPayAmount('')
      setPayRef('')
      setShowPayForm(false)
      queryClient.invalidateQueries({ queryKey: ['transaction', id] })
    } catch (err) {
      const e = err as { detail?: string; title?: string }
      setPayError(e?.detail ?? e?.title ?? 'Payment failed.')
    } finally {
      setIsAddingPayment(false)
    }
  }

  // ── Loading / error states ─────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="p-4 max-w-2xl mx-auto space-y-4">
        {[...Array(5)].map((_, i) => (
          <div key={i} className="h-24 rounded-xl bg-gray-800 animate-pulse" />
        ))}
      </div>
    )
  }

  if (error || !tx) {
    return (
      <div className="p-4 max-w-2xl mx-auto">
        <button onClick={() => router.back()} className="flex items-center gap-1.5 text-gray-400 hover:text-white mb-4">
          <ArrowLeft className="h-4 w-4" /> Back
        </button>
        <div className="rounded-xl bg-red-900/20 border border-red-800 p-6 text-center">
          <p className="text-red-400">Transaction not found.</p>
        </div>
      </div>
    )
  }

  const status   = TX_STATUS[tx.status] ?? TX_STATUS[TransactionStatus.Pending]
  const canEdit   = tx.status === TransactionStatus.InProgress
  const canCancel = tx.status === TransactionStatus.Pending || tx.status === TransactionStatus.InProgress

  const alreadyPaid = tx.payments.reduce((s, p) => s + p.amount, 0)
  const remaining   = Math.max(0, tx.finalAmount - alreadyPaid)
  const isFullyPaid = remaining < 0.01

  return (
    <>
      {/* ── Screen view ────────────────────────────────────────────────────── */}
      <div className="p-4 max-w-2xl mx-auto space-y-4 print:hidden">

        {/* Top bar */}
        <div className="flex items-center justify-between gap-2">
          <button
            onClick={() => router.back()}
            className="flex items-center gap-1.5 text-sm text-gray-400 hover:text-white transition-colors min-h-[44px] px-2 -ml-2"
          >
            <ArrowLeft className="h-4 w-4" /> Back
          </button>
          <div className="flex items-center gap-2">
            {canEdit && (
              <Link
                href={`/transactions/new?editId=${id}`}
                className="flex items-center gap-1.5 min-h-[44px] px-3 rounded-lg text-sm text-blue-300 hover:bg-blue-900/20 border border-blue-800/50 transition-colors"
              >
                <Edit2 className="h-4 w-4" />
                Edit Items
              </Link>
            )}
            {canCancel && (
              <button
                onClick={() => {
                  if (confirm('Cancel this transaction?')) cancelMutation.mutate()
                }}
                disabled={cancelMutation.isPending}
                className="flex items-center gap-1.5 min-h-[44px] px-3 rounded-lg text-sm text-red-400 hover:bg-red-900/20 border border-red-800/50 transition-colors disabled:opacity-50"
              >
                <XCircle className="h-4 w-4" />
                Cancel
              </button>
            )}
            <button
              onClick={() => window.print()}
              className="flex items-center gap-1.5 min-h-[44px] px-3 rounded-lg text-sm text-gray-300 hover:bg-gray-800 border border-gray-700 transition-colors"
            >
              <Printer className="h-4 w-4" />
              Print
            </button>
          </div>
        </div>

        {/* Header card */}
        <div className="rounded-xl bg-gray-800 border border-gray-700 p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="text-xs text-gray-500">Transaction</p>
              <p className="text-lg font-mono font-bold text-white">{tx.transactionNumber}</p>
              <p className="text-xs text-gray-500 mt-0.5">{fmtDateTime(tx.createdAt)}</p>
            </div>
            <span className={cn('flex items-center gap-1.5 text-xs font-medium px-2.5 py-1.5 rounded-full border', status.cls)}>
              {status.icon}
              {status.label}
            </span>
          </div>
          <div className="mt-3 pt-3 border-t border-gray-700 grid grid-cols-2 gap-2 text-sm">
            <div>
              <p className="text-xs text-gray-500">Branch</p>
              <p className="text-white">{tx.branchName}</p>
            </div>
            <div>
              <p className="text-xs text-gray-500">Cashier</p>
              <p className="text-white">{tx.cashierName}</p>
            </div>
          </div>
        </div>

        {/* Vehicle */}
        <Section icon={<Car className="h-4 w-4" />} title="Vehicle">
          <div className="flex items-center gap-3">
            <p className="text-2xl font-mono font-bold text-white tracking-widest">{tx.plateNumber}</p>
          </div>
          <p className="text-sm text-gray-400 mt-1">
            {tx.vehicleTypeName} · {tx.sizeName}
          </p>
        </Section>

        {/* Customer */}
        {tx.customerName && (
          <Section icon={<User2 className="h-4 w-4" />} title="Customer">
            <p className="text-white">{tx.customerName}</p>
          </Section>
        )}

        {/* Services */}
        {tx.services.length > 0 && (
          <Section icon={<Wrench className="h-4 w-4" />} title="Services">
            <div className="space-y-3">
              {tx.services.map((svc) => (
                <div key={svc.id}>
                  <div className="flex justify-between items-start">
                    <div>
                      <p className="text-sm font-medium text-white">{svc.serviceName}</p>
                      <p className="text-xs text-gray-500">{svc.vehicleTypeName} · {svc.sizeName}</p>
                    </div>
                    <p className="text-sm font-semibold text-white ml-2 shrink-0">{fmt(svc.unitPrice)}</p>
                  </div>
                  {svc.employeeAssignments.length > 0 && (
                    <div className="mt-1.5 flex flex-wrap gap-1.5">
                      {svc.employeeAssignments.map((a) => (
                        <span key={a.id} className="text-xs bg-gray-700 text-gray-300 px-2 py-0.5 rounded-full">
                          {a.employeeName} · {fmt(a.commissionAmount)}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>
          </Section>
        )}

        {/* Packages */}
        {tx.packages.length > 0 && (
          <Section icon={<Package className="h-4 w-4" />} title="Packages">
            <div className="space-y-3">
              {tx.packages.map((pkg) => (
                <div key={pkg.id}>
                  <div className="flex justify-between items-start">
                    <div>
                      <p className="text-sm font-medium text-white">{pkg.packageName}</p>
                      <p className="text-xs text-gray-500">{pkg.vehicleTypeName} · {pkg.sizeName}</p>
                    </div>
                    <p className="text-sm font-semibold text-white ml-2 shrink-0">{fmt(pkg.unitPrice)}</p>
                  </div>
                  {pkg.employeeAssignments.length > 0 && (
                    <div className="mt-1.5 flex flex-wrap gap-1.5">
                      {pkg.employeeAssignments.map((a) => (
                        <span key={a.id} className="text-xs bg-gray-700 text-gray-300 px-2 py-0.5 rounded-full">
                          {a.employeeName} · {fmt(a.commissionAmount)}
                        </span>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>
          </Section>
        )}

        {/* Merchandise */}
        {tx.merchandise.length > 0 && (
          <Section icon={<ShoppingCart className="h-4 w-4" />} title="Merchandise">
            <div className="space-y-2">
              {tx.merchandise.map((m) => (
                <div key={m.id} className="flex justify-between items-center text-sm">
                  <span className="text-white">{m.merchandiseName}</span>
                  <div className="flex items-center gap-3 text-gray-400 shrink-0">
                    <span>×{m.quantity}</span>
                    <span className="text-white font-medium">{fmt(m.lineTotal)}</span>
                  </div>
                </div>
              ))}
            </div>
          </Section>
        )}

        {/* Employee earnings */}
        {tx.employees.length > 0 && (
          <Section icon={<Users className="h-4 w-4" />} title="Employee Commissions">
            <div className="space-y-1.5">
              {tx.employees.map((e) => (
                <div key={e.id} className="flex justify-between text-sm">
                  <span className="text-gray-300">{e.employeeName}</span>
                  <span className="text-green-400 font-medium">{fmt(e.totalCommission)}</span>
                </div>
              ))}
            </div>
          </Section>
        )}

        {/* Payments */}
        <Section icon={<CreditCard className="h-4 w-4" />} title="Payments">
          {tx.payments.length > 0 && (
            <div className="space-y-1.5 mb-3">
              {tx.payments.map((p) => (
                <div key={p.id} className="flex justify-between text-sm">
                  <div>
                    <span className="text-gray-300">{PAYMENT_LABEL[p.method] ?? 'Payment'}</span>
                    {p.reference && (
                      <span className="text-xs text-gray-500 ml-2">Ref: {p.reference}</span>
                    )}
                  </div>
                  <span className="text-white font-medium">{fmt(p.amount)}</span>
                </div>
              ))}
              <div className="flex justify-between text-sm pt-1.5 border-t border-gray-700">
                <span className="text-gray-400">Paid</span>
                <span className={cn('font-mono font-bold', isFullyPaid ? 'text-green-400' : 'text-white')}>
                  {fmt(alreadyPaid)}
                </span>
              </div>
              {!isFullyPaid && (
                <div className="flex justify-between text-sm text-orange-400">
                  <span>Remaining</span>
                  <span className="font-mono font-bold">{fmt(remaining)}</span>
                </div>
              )}
            </div>
          )}

          {/* Add Payment — only when InProgress and not yet fully paid */}
          {canEdit && !isFullyPaid && (
            <div>
              {!showPayForm ? (
                <button
                  type="button"
                  onClick={() => { setShowPayForm(true); setPayAmount(remaining.toFixed(2)) }}
                  className="flex items-center gap-1.5 text-sm text-blue-400 hover:text-blue-300 transition-colors min-h-[40px]"
                >
                  <BadgeCheck className="h-4 w-4" />
                  Add Payment
                </button>
              ) : (
                <div className="space-y-2 pt-2 border-t border-gray-700">
                  {/* Method selector */}
                  <div className="flex gap-1">
                    {PAYMENT_METHODS.map(({ value, label }) => (
                      <button
                        key={value}
                        type="button"
                        onClick={() => setPayMethod(value)}
                        className={`flex-1 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                          payMethod === value
                            ? 'bg-blue-600 text-white'
                            : 'bg-gray-700 text-gray-400 hover:text-gray-300 border border-gray-600'
                        }`}
                      >
                        {label}
                      </button>
                    ))}
                  </div>
                  {/* Amount + ref */}
                  <div className="flex gap-2">
                    <input
                      type="number"
                      min={0}
                      step={0.01}
                      value={payAmount}
                      onChange={(e) => setPayAmount(e.target.value)}
                      placeholder={remaining.toFixed(2)}
                      className="flex-1 min-h-10 px-3 rounded-xl bg-gray-700 border border-gray-600 text-white placeholder:text-gray-500 text-sm font-mono focus:outline-none focus:ring-1 focus:ring-blue-500"
                    />
                    {payMethod !== PaymentMethod.Cash && (
                      <input
                        type="text"
                        value={payRef}
                        onChange={(e) => setPayRef(e.target.value)}
                        placeholder="Ref #"
                        className="w-24 min-h-10 px-2 rounded-xl bg-gray-700 border border-gray-600 text-white placeholder:text-gray-500 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                      />
                    )}
                  </div>
                  {payError && (
                    <div className="flex items-center gap-1.5 text-xs text-red-400">
                      <AlertCircle className="h-3.5 w-3.5 shrink-0" />
                      {payError}
                    </div>
                  )}
                  <div className="flex gap-2">
                    <button
                      type="button"
                      onClick={() => { setShowPayForm(false); setPayError(null) }}
                      className="flex-1 min-h-10 rounded-xl bg-gray-700 hover:bg-gray-600 text-gray-300 text-sm transition-colors"
                    >
                      Cancel
                    </button>
                    <button
                      type="button"
                      onClick={() => void handleAddPayment()}
                      disabled={isAddingPayment || !payAmount || parseFloat(payAmount) <= 0}
                      className="flex-1 min-h-10 rounded-xl bg-green-600 hover:bg-green-500 disabled:opacity-50 text-white text-sm font-semibold transition-colors flex items-center justify-center gap-1.5"
                    >
                      {isAddingPayment
                        ? <RefreshCw className="h-3.5 w-3.5 animate-spin" />
                        : <><CheckCircle2 className="h-4 w-4" /> Confirm Payment</>}
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}

          {tx.payments.length === 0 && !canEdit && (
            <p className="text-sm text-gray-500">No payments recorded.</p>
          )}
        </Section>

        {/* Totals */}
        <div className="rounded-xl bg-gray-800 border border-gray-700 p-4 space-y-2">
          <TotalRow label="Subtotal" value={fmt(tx.totalAmount)} />
          {tx.discountAmount > 0 && (
            <TotalRow label="Discount" value={`-${fmt(tx.discountAmount)}`} valueClass="text-green-400" />
          )}
          {tx.taxAmount > 0 && (
            <TotalRow label="Tax" value={fmt(tx.taxAmount)} />
          )}
          <div className="pt-2 border-t border-gray-700">
            <TotalRow label="Total" value={fmt(tx.finalAmount)} labelClass="font-bold text-white" valueClass="text-xl font-bold text-white" />
          </div>
        </div>

        {/* Notes */}
        {tx.notes && (
          <div className="rounded-xl bg-gray-800 border border-gray-700 p-4">
            <p className="text-xs text-gray-500 mb-1">Notes</p>
            <p className="text-sm text-gray-300">{tx.notes}</p>
          </div>
        )}
      </div>

      {/* ── Print / Receipt view ───────────────────────────────────────────── */}
      <div className="hidden print:block p-8 max-w-sm mx-auto text-black font-mono text-sm leading-relaxed">
        <div className="text-center mb-4">
          <p className="text-lg font-bold">SplashSphere</p>
          <p className="text-xs">{tx.branchName}</p>
          <p className="text-xs mt-1">{fmtDateTime(tx.createdAt)}</p>
        </div>
        <div className="border-t border-b border-black border-dashed py-2 my-2 text-center">
          <p className="font-bold">{tx.transactionNumber}</p>
        </div>
        <div className="my-2">
          <p>Plate: <strong>{tx.plateNumber}</strong></p>
          <p>Vehicle: {tx.vehicleTypeName} · {tx.sizeName}</p>
          {tx.customerName && <p>Customer: {tx.customerName}</p>}
          <p>Cashier: {tx.cashierName}</p>
        </div>
        <div className="border-t border-dashed border-black my-2 pt-2">
          {tx.services.map((svc) => (
            <div key={svc.id} className="flex justify-between">
              <span>{svc.serviceName}</span>
              <span>{fmt(svc.unitPrice)}</span>
            </div>
          ))}
          {tx.packages.map((pkg) => (
            <div key={pkg.id} className="flex justify-between">
              <span>{pkg.packageName}</span>
              <span>{fmt(pkg.unitPrice)}</span>
            </div>
          ))}
          {tx.merchandise.map((m) => (
            <div key={m.id} className="flex justify-between">
              <span>{m.merchandiseName} ×{m.quantity}</span>
              <span>{fmt(m.lineTotal)}</span>
            </div>
          ))}
        </div>
        <div className="border-t border-dashed border-black my-2 pt-2 space-y-0.5">
          <div className="flex justify-between"><span>Subtotal</span><span>{fmt(tx.totalAmount)}</span></div>
          {tx.discountAmount > 0 && (
            <div className="flex justify-between"><span>Discount</span><span>-{fmt(tx.discountAmount)}</span></div>
          )}
          {tx.taxAmount > 0 && (
            <div className="flex justify-between"><span>Tax</span><span>{fmt(tx.taxAmount)}</span></div>
          )}
          <div className="flex justify-between font-bold border-t border-black pt-1 mt-1">
            <span>TOTAL</span><span>{fmt(tx.finalAmount)}</span>
          </div>
        </div>
        {tx.payments.length > 0 && (
          <div className="border-t border-dashed border-black my-2 pt-2">
            {tx.payments.map((p) => (
              <div key={p.id} className="flex justify-between">
                <span>{PAYMENT_LABEL[p.method] ?? 'Payment'}</span>
                <span>{fmt(p.amount)}</span>
              </div>
            ))}
          </div>
        )}
        <div className="text-center mt-4 text-xs">
          <p>Thank you for choosing SplashSphere!</p>
          <p className="mt-1">Status: {TX_STATUS[tx.status]?.label}</p>
        </div>
      </div>
    </>
  )
}

// ── Sub-components ─────────────────────────────────────────────────────────────

function Section({
  icon, title, children,
}: {
  icon: React.ReactNode
  title: string
  children: React.ReactNode
}) {
  return (
    <div className="rounded-xl bg-gray-800 border border-gray-700 p-4">
      <div className="flex items-center gap-2 mb-3 text-gray-400">
        {icon}
        <h3 className="text-xs font-semibold uppercase tracking-wider">{title}</h3>
      </div>
      {children}
    </div>
  )
}

function TotalRow({
  label, value, labelClass, valueClass,
}: {
  label: string
  value: string
  labelClass?: string
  valueClass?: string
}) {
  return (
    <div className="flex justify-between items-center">
      <span className={cn('text-sm text-gray-400', labelClass)}>{label}</span>
      <span className={cn('text-sm text-white', valueClass)}>{value}</span>
    </div>
  )
}
