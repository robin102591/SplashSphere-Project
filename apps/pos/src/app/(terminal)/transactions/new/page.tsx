'use client'

import { useEffect, useRef, useState, Suspense, useMemo } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQuery, useQueries } from '@tanstack/react-query'
import Link from 'next/link'
import {
  ArrowLeft, Search, RefreshCw, X, Plus, Minus,
  Banknote, Smartphone, CreditCard, Building2, CheckCircle2,
  ChevronDown, ChevronUp, AlertCircle, Layers,
} from 'lucide-react'

import { PaymentMethod, TransactionStatus, EmployeeType } from '@splashsphere/types'
import type {
  Car, QueueEntry, ServiceSummary, PackageSummary, Merchandise,
  Employee, VehicleType, Size, PackageDetail, TransactionSummary,
  ApiError,
} from '@splashsphere/types'
import type { PagedResult } from '@splashsphere/types'

import { apiClient } from '@/lib/api-client'
import {
  useTransactionStore,
  type ServiceLineItem,
  type PackageLineItem,
  type MerchandiseLine,
} from '@/lib/use-transaction-store'

// ── Helpers ───────────────────────────────────────────────────────────────────

const peso = (n: number) =>
  `₱${n.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

const PAYMENT_METHODS: {
  value: PaymentMethod
  label: string
  Icon: React.ComponentType<{ className?: string }>
}[] = [
  { value: PaymentMethod.Cash,         label: 'Cash',   Icon: Banknote   },
  { value: PaymentMethod.GCash,        label: 'GCash',  Icon: Smartphone },
  { value: PaymentMethod.CreditCard,   label: 'Credit', Icon: CreditCard },
  { value: PaymentMethod.DebitCard,    label: 'Debit',  Icon: CreditCard },
  { value: PaymentMethod.BankTransfer, label: 'Bank',   Icon: Building2  },
]

function parseServiceIds(raw: string | null): string[] {
  if (!raw) return []
  try { return JSON.parse(raw) as string[] } catch { return [] }
}

// ── Employee chip row ─────────────────────────────────────────────────────────

function EmployeeChips({
  selectedIds,
  employees,
  onToggle,
}: {
  selectedIds: string[]
  employees: Employee[]
  onToggle: (id: string) => void
}) {
  if (!employees.length)
    return <p className="text-xs text-gray-600 mt-1.5">No employees available</p>
  return (
    <div className="flex flex-wrap gap-1 mt-1.5">
      {employees.map((emp) => {
        const on = selectedIds.includes(emp.id)
        return (
          <button
            key={emp.id}
            type="button"
            onClick={() => onToggle(emp.id)}
            className={`text-xs px-2.5 py-0.5 rounded-full transition-colors ${
              on
                ? 'bg-blue-600 text-white'
                : 'bg-gray-700 text-gray-400 hover:bg-gray-600 hover:text-white'
            }`}
          >
            {emp.firstName}
          </button>
        )
      })}
    </div>
  )
}

// ── Order line item (service or package) ──────────────────────────────────────

function ServiceOrderRow({
  item,
  employees,
  resolvedPrice,
  onRemove,
  onToggleEmployee,
}: {
  item: ServiceLineItem | PackageLineItem
  employees: Employee[]
  resolvedPrice?: number
  onRemove: () => void
  onToggleEmployee: (empId: string) => void
}) {
  const [expanded, setExpanded] = useState(false)
  const isService = 'serviceId' in item
  const displayPrice = resolvedPrice !== undefined ? resolvedPrice : item.unitPrice

  return (
    <div className="rounded-lg bg-gray-800/60 border border-gray-700/50 p-3 space-y-1">
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-1.5">
            {!isService && <Layers className="h-3 w-3 text-purple-400 shrink-0" />}
            <p className="text-sm font-semibold text-white truncate">
              {isService
                ? (item as ServiceLineItem).serviceName
                : (item as PackageLineItem).packageName}
            </p>
          </div>
          {isService && (
            <p className="text-xs text-gray-500">{(item as ServiceLineItem).categoryName}</p>
          )}
        </div>
        <div className="flex items-center gap-2 shrink-0">
          <span className="text-sm font-mono font-semibold text-white">
            {displayPrice > 0 ? peso(displayPrice) : '₱—'}
          </span>
          <button
            type="button"
            onClick={onRemove}
            className="h-5 w-5 flex items-center justify-center rounded text-gray-600 hover:text-red-400 transition-colors"
          >
            <X className="h-3.5 w-3.5" />
          </button>
        </div>
      </div>

      <button
        type="button"
        onClick={() => setExpanded((v) => !v)}
        className="flex items-center gap-1 text-xs text-gray-500 hover:text-gray-300 transition-colors mt-1"
      >
        {expanded ? <ChevronUp className="h-3 w-3" /> : <ChevronDown className="h-3 w-3" />}
        {item.employeeIds.length > 0
          ? `${item.employeeIds.length} employee${item.employeeIds.length > 1 ? 's' : ''} assigned`
          : 'Assign employees'}
      </button>

      {expanded && (
        <EmployeeChips
          selectedIds={item.employeeIds}
          employees={employees}
          onToggle={onToggleEmployee}
        />
      )}
    </div>
  )
}

function MerchandiseOrderRow({
  item,
  onRemove,
  onQtyChange,
}: {
  item: MerchandiseLine
  onRemove: () => void
  onQtyChange: (qty: number) => void
}) {
  return (
    <div className="flex items-center gap-2 rounded-lg bg-gray-800/60 border border-gray-700/50 p-3">
      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-white truncate">{item.merchandiseName}</p>
        <p className="text-xs text-gray-500">{peso(item.unitPrice)} each</p>
      </div>
      <div className="flex items-center gap-1 shrink-0">
        <button
          type="button"
          onClick={() => onQtyChange(item.quantity - 1)}
          className="h-7 w-7 flex items-center justify-center rounded-lg bg-gray-700 hover:bg-gray-600 text-gray-300 transition-colors"
        >
          <Minus className="h-3 w-3" />
        </button>
        <span className="w-8 text-center text-sm font-mono font-bold text-white">{item.quantity}</span>
        <button
          type="button"
          onClick={() => onQtyChange(item.quantity + 1)}
          className="h-7 w-7 flex items-center justify-center rounded-lg bg-gray-700 hover:bg-gray-600 text-gray-300 transition-colors"
        >
          <Plus className="h-3 w-3" />
        </button>
      </div>
      <span className="text-sm font-mono font-semibold text-white w-20 text-right shrink-0">
        {peso(item.unitPrice * item.quantity)}
      </span>
      <button
        type="button"
        onClick={onRemove}
        className="h-5 w-5 flex items-center justify-center rounded text-gray-600 hover:text-red-400 transition-colors"
      >
        <X className="h-3.5 w-3.5" />
      </button>
    </div>
  )
}

// ── Catalog cards ─────────────────────────────────────────────────────────────

function ServiceCard({
  service,
  inCart,
  onToggle,
}: {
  service: ServiceSummary
  inCart: boolean
  onToggle: () => void
}) {
  return (
    <button
      type="button"
      onClick={onToggle}
      className={`w-full text-left p-3 rounded-xl border-2 transition-all min-h-[4.5rem] flex flex-col justify-between gap-1 ${
        inCart
          ? 'border-blue-500 bg-blue-600/15 text-blue-300'
          : 'border-gray-700 bg-gray-800 text-gray-300 hover:border-gray-500 hover:text-white'
      }`}
    >
      <p className="text-sm font-semibold leading-tight line-clamp-2">{service.name}</p>
      <div className="flex items-end justify-between gap-1">
        <span className="text-xs text-gray-500 truncate">{service.categoryName}</span>
        <span className={`text-xs font-mono font-semibold shrink-0 ${inCart ? 'text-blue-400' : 'text-gray-400'}`}>
          {peso(service.basePrice)}
        </span>
      </div>
    </button>
  )
}

function PackageCard({
  pkg,
  inCart,
  onToggle,
}: {
  pkg: PackageSummary
  inCart: boolean
  onToggle: () => void
}) {
  return (
    <button
      type="button"
      onClick={onToggle}
      className={`w-full text-left p-3 rounded-xl border-2 transition-all min-h-[4.5rem] flex flex-col justify-between gap-1 ${
        inCart
          ? 'border-purple-500 bg-purple-600/15 text-purple-300'
          : 'border-gray-700 bg-gray-800 text-gray-300 hover:border-gray-500 hover:text-white'
      }`}
    >
      <div className="flex items-start justify-between gap-1">
        <p className="text-sm font-semibold leading-tight line-clamp-2">{pkg.name}</p>
        <Layers className="h-3.5 w-3.5 shrink-0 mt-0.5 text-purple-400" />
      </div>
      <p className="text-xs text-gray-500">
        {pkg.serviceCount} service{pkg.serviceCount !== 1 ? 's' : ''} · priced by vehicle
      </p>
    </button>
  )
}

function MerchandiseCard({
  item,
  cartQty,
  onAdd,
}: {
  item: Merchandise
  cartQty: number
  onAdd: () => void
}) {
  const lowStock = item.stockQuantity > 0 && item.stockQuantity <= item.lowStockThreshold
  return (
    <button
      type="button"
      onClick={onAdd}
      disabled={item.stockQuantity === 0}
      className={`w-full text-left p-3 rounded-xl border-2 transition-all min-h-[4.5rem] flex flex-col justify-between gap-1 ${
        cartQty > 0
          ? 'border-green-600 bg-green-700/15 text-green-300'
          : item.stockQuantity === 0
            ? 'border-gray-800 bg-gray-800/40 text-gray-600 cursor-not-allowed opacity-50'
            : 'border-gray-700 bg-gray-800 text-gray-300 hover:border-gray-500 hover:text-white'
      }`}
    >
      <p className="text-sm font-semibold leading-tight line-clamp-2">{item.name}</p>
      <div className="flex items-end justify-between gap-1">
        <span className={`text-xs ${lowStock ? 'text-orange-400' : 'text-gray-500'}`}>
          {item.stockQuantity === 0
            ? 'Out of stock'
            : lowStock
              ? `Low: ${item.stockQuantity}`
              : `Stock: ${item.stockQuantity}`}
        </span>
        <div className="flex items-center gap-1.5 shrink-0">
          {cartQty > 0 && (
            <span className="text-xs bg-green-600/30 text-green-400 px-1.5 py-0.5 rounded-full font-bold">
              ×{cartQty}
            </span>
          )}
          <span className="text-xs font-mono font-semibold text-gray-400">{peso(item.price)}</span>
        </div>
      </div>
    </button>
  )
}

// ── Main content ──────────────────────────────────────────────────────────────

function NewTransactionContent() {
  const { getToken } = useAuth()
  const router = useRouter()
  const searchParams = useSearchParams()
  const queueEntryId = searchParams.get('queueEntryId')

  // ── Store ──────────────────────────────────────────────────────────────────
  const store = useTransactionStore()
  const {
    branchId, plateNumber, carId, customerId,
    vehicleTypeId, sizeId, vehicleTypeName, sizeName,
    services, packages, merchandise,
    discountAmount, notes, payments,
  } = store

  // ── Local UI state ─────────────────────────────────────────────────────────
  const [activeTab, setActiveTab] = useState<'services' | 'packages' | 'merchandise'>('services')
  const [categoryFilter, setCategoryFilter] = useState<string | null>(null)
  const [lookupPlate, setLookupPlate] = useState('')
  const [isLookingUp, setIsLookingUp] = useState(false)
  const [carNotFound, setCarNotFound] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [payMethod, setPayMethod] = useState<PaymentMethod>(PaymentMethod.Cash)
  const [payAmount, setPayAmount] = useState('')
  const [payRef, setPayRef] = useState('')

  const vehicleInitDone = useRef(false)
  const servicesInitDone = useRef(false)

  // ── Boot ───────────────────────────────────────────────────────────────────
  useEffect(() => {
    store.reset()
    const saved = localStorage.getItem('pos-branch-id')
    if (saved) store.setBranch(saved)
    if (queueEntryId) store.setQueueEntry(queueEntryId)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // ── API ────────────────────────────────────────────────────────────────────

  const { data: queueEntry } = useQuery({
    queryKey: ['queue-entry-tx', queueEntryId],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<QueueEntry>(`/queue/${queueEntryId}`, token ?? undefined)
    },
    enabled: !!queueEntryId,
  })

  const { data: queueCar } = useQuery({
    queryKey: ['car-for-tx-queue', queueEntry?.plateNumber],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Car>(
        `/cars/lookup/${encodeURIComponent(queueEntry!.plateNumber)}`,
        token ?? undefined
      )
    },
    enabled: !!queueEntry?.plateNumber,
  })

  const { data: allServices = [] } = useQuery({
    queryKey: ['services-catalog'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<ServiceSummary>>('/services?pageSize=100', token ?? undefined)
      return (res.items as ServiceSummary[]).filter((s) => s.isActive)
    },
  })

  const { data: allPackages = [] } = useQuery({
    queryKey: ['packages-catalog'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<PackageSummary>>('/packages?pageSize=100', token ?? undefined)
      return (res.items as PackageSummary[]).filter((p) => p.isActive)
    },
  })

  const { data: allMerchandise = [] } = useQuery({
    queryKey: ['merchandise-catalog'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<Merchandise>>('/merchandise?pageSize=100', token ?? undefined)
      return (res.items as Merchandise[]).filter((m) => m.isActive)
    },
  })

  const { data: employees = [] } = useQuery({
    queryKey: ['employees-pos', branchId],
    queryFn: async () => {
      const token = await getToken()
      const qs = branchId
        ? `?branchId=${encodeURIComponent(branchId)}&pageSize=100`
        : '?pageSize=100'
      const res = await apiClient.get<PagedResult<Employee>>(`/employees${qs}`, token ?? undefined)
      return (res.items as Employee[]).filter(
        (e) => e.isActive && e.employeeType === EmployeeType.Commission
      )
    },
  })

  const { data: vehicleTypes = [] } = useQuery({
    queryKey: ['vehicle-types'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<VehicleType>>('/vehicle-types?pageSize=100', token ?? undefined)
      return (res.items as VehicleType[]).filter((vt) => vt.isActive)
    },
  })

  const { data: sizes = [] } = useQuery({
    queryKey: ['sizes'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<Size>>('/sizes?pageSize=100', token ?? undefined)
      return (res.items as Size[]).filter((s) => s.isActive)
    },
  })

  // Package pricing: one fetch per cart package (when vehicleTypeId + sizeId are known)
  const packagePriceQueries = useQueries({
    queries: packages.map((pkg) => ({
      queryKey: ['pkg-price', pkg.packageId, vehicleTypeId, sizeId],
      queryFn: async () => {
        const token = await getToken()
        const detail = await apiClient.get<PackageDetail>(
          `/packages/${pkg.packageId}`,
          token ?? undefined
        )
        const row = detail.pricing.find(
          (r) => r.vehicleTypeId === vehicleTypeId && r.sizeId === sizeId
        )
        return { localId: pkg.localId, price: row?.price ?? 0 }
      },
      enabled: !!vehicleTypeId && !!sizeId,
    })),
  })

  // Sync resolved package prices into store
  const priceKey = packagePriceQueries.map((q) => q.data?.price ?? '?').join(',')
  useEffect(() => {
    packagePriceQueries.forEach((q) => {
      if (!q.data) return
      const { localId, price } = q.data
      const pkg = useTransactionStore.getState().packages.find((p) => p.localId === localId)
      if (pkg && pkg.unitPrice !== price) {
        useTransactionStore.getState().updatePackagePrice(localId, price)
      }
    })
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [priceKey])

  // ── Init from queue entry ──────────────────────────────────────────────────

  useEffect(() => {
    if (!queueEntry || vehicleInitDone.current) return
    if (queueEntry.carId && !queueCar) return
    vehicleInitDone.current = true
    setLookupPlate(queueEntry.plateNumber)

    if (queueCar) {
      useTransactionStore.getState().setVehicle({
        plateNumber: queueEntry.plateNumber,
        carId: queueCar.id,
        customerId: queueCar.customerId,
        vehicleTypeId: queueCar.vehicleTypeId,
        sizeId: queueCar.sizeId,
        vehicleTypeName: queueCar.vehicleTypeName,
        sizeName: queueCar.sizeName,
      })
      setCarNotFound(false)
    } else {
      useTransactionStore.getState().setVehicle({
        plateNumber: queueEntry.plateNumber,
        carId: null,
        customerId: queueEntry.customerId,
        vehicleTypeId: '',
        sizeId: '',
        vehicleTypeName: '',
        sizeName: '',
      })
      setCarNotFound(true)
    }
  }, [queueEntry, queueCar])

  useEffect(() => {
    if (!queueEntry || !allServices.length || servicesInitDone.current) return
    servicesInitDone.current = true
    const ids = parseServiceIds(queueEntry.preferredServices)
    const { addService } = useTransactionStore.getState()
    ids.forEach((sid) => {
      const svc = allServices.find((s) => s.id === sid)
      if (svc) {
        addService({
          serviceId: svc.id,
          serviceName: svc.name,
          categoryName: svc.categoryName,
          unitPrice: svc.basePrice,
        })
      }
    })
  }, [queueEntry, allServices])

  // ── Plate lookup (direct flow) ─────────────────────────────────────────────

  const handlePlateLookup = async () => {
    const plate = lookupPlate.trim().toUpperCase()
    if (!plate) return
    setIsLookingUp(true)
    setCarNotFound(false)
    try {
      const token = await getToken()
      const car = await apiClient.get<Car>(
        `/cars/lookup/${encodeURIComponent(plate)}`,
        token ?? undefined
      )
      store.setVehicle({
        plateNumber: plate,
        carId: car.id,
        customerId: car.customerId,
        vehicleTypeId: car.vehicleTypeId,
        sizeId: car.sizeId,
        vehicleTypeName: car.vehicleTypeName,
        sizeName: car.sizeName,
      })
    } catch {
      store.setVehicle({
        plateNumber: plate,
        carId: null,
        customerId: null,
        vehicleTypeId: '',
        sizeId: '',
        vehicleTypeName: '',
        sizeName: '',
      })
      setCarNotFound(true)
    } finally {
      setIsLookingUp(false)
    }
  }

  // ── Cart helpers ───────────────────────────────────────────────────────────

  const serviceInCart  = (id: string) => services.some((s)   => s.serviceId     === id)
  const packageInCart  = (id: string) => packages.some((p)   => p.packageId     === id)
  const merchandiseQty = (id: string) => merchandise.find((m) => m.merchandiseId === id)?.quantity ?? 0

  const toggleService = (svc: ServiceSummary) => {
    const existing = services.find((s) => s.serviceId === svc.id)
    if (existing) { store.removeService(existing.localId) }
    else { store.addService({ serviceId: svc.id, serviceName: svc.name, categoryName: svc.categoryName, unitPrice: svc.basePrice }) }
  }

  const togglePackage = (pkg: PackageSummary) => {
    const existing = packages.find((p) => p.packageId === pkg.id)
    if (existing) { store.removePackage(existing.localId) }
    else { store.addPackage({ packageId: pkg.id, packageName: pkg.name, unitPrice: 0 }) }
  }

  const addMerchandise = (item: Merchandise) => {
    const existing = merchandise.find((m) => m.merchandiseId === item.id)
    if (existing) { store.updateMerchandiseQty(existing.localId, existing.quantity + 1) }
    else { store.addMerchandise({ merchandiseId: item.id, merchandiseName: item.name, unitPrice: item.price, quantity: 1 }) }
  }

  // ── Totals ─────────────────────────────────────────────────────────────────

  const serviceTotal     = useMemo(() => services.reduce((s, i) => s + i.unitPrice, 0), [services])
  const packageTotal     = useMemo(() => packages.reduce((s, i) => s + i.unitPrice, 0), [packages])
  const merchandiseTotal = useMemo(() => merchandise.reduce((s, i) => s + i.unitPrice * i.quantity, 0), [merchandise])
  const subtotal         = serviceTotal + packageTotal + merchandiseTotal
  const discount         = Math.min(discountAmount, subtotal)
  const estimatedTotal   = Math.max(0, subtotal - discount)
  const totalPaid        = useMemo(() => payments.reduce((s, p) => s + p.amount, 0), [payments])
  const balance          = Math.max(0, estimatedTotal - totalPaid)
  const change           = Math.max(0, totalPaid - estimatedTotal)

  // ── Payment helpers ────────────────────────────────────────────────────────

  const handlePayMethodSelect = (method: PaymentMethod) => {
    setPayMethod(method)
    if (balance > 0) setPayAmount(balance.toFixed(2))
  }

  const handleAddPayment = () => {
    const amount = parseFloat(payAmount)
    if (!amount || amount <= 0) return
    store.addPayment({ method: payMethod, amount, reference: payRef })
    setPayAmount('')
    setPayRef('')
  }

  // ── Submit ─────────────────────────────────────────────────────────────────

  const itemCount = services.length + packages.length + merchandise.length

  const canComplete =
    !!plateNumber &&
    !!vehicleTypeId &&
    !!sizeId &&
    itemCount > 0 &&
    estimatedTotal > 0 &&
    totalPaid >= estimatedTotal

  const handleComplete = async () => {
    setIsSubmitting(true)
    setSubmitError(null)
    try {
      const token = await getToken()

      const body = {
        branchId: branchId || undefined,
        carId,
        customerId,
        vehicleTypeId,
        sizeId,
        plateNumber,
        queueEntryId,
        services: services.map((s) => ({
          serviceId: s.serviceId,
          employeeIds: s.employeeIds,
          notes: null,
        })),
        packages: packages.map((p) => ({
          packageId: p.packageId,
          employeeIds: p.employeeIds,
          notes: null,
        })),
        merchandise: merchandise.map((m) => ({
          merchandiseId: m.merchandiseId,
          quantity: m.quantity,
        })),
        discountAmount: discount,
        notes: notes || null,
      }

      const tx = await apiClient.post<TransactionSummary>('/transactions', body, token ?? undefined)

      // Add payments
      for (const p of payments) {
        try {
          await apiClient.post(
            `/transactions/${tx.id}/payments`,
            { method: p.method, amount: p.amount, reference: p.reference || null },
            token ?? undefined
          )
        } catch { /* cashier can add on detail page */ }
      }

      // Attempt status transition to Completed
      try {
        await apiClient.patch(
          `/transactions/${tx.id}/status`,
          { status: TransactionStatus.Completed },
          token ?? undefined
        )
      } catch { /* handle on detail page */ }

      store.reset()
      router.push(`/transactions/${tx.id}`)
    } catch (err) {
      const apiErr = err as ApiError
      setSubmitError(apiErr?.detail ?? apiErr?.title ?? 'Failed to create transaction.')
    } finally {
      setIsSubmitting(false)
    }
  }

  // ── Catalog derived ────────────────────────────────────────────────────────

  const categories = useMemo(
    () => Array.from(new Set(allServices.map((s) => s.categoryName))).sort(),
    [allServices]
  )
  const filteredServices = useMemo(
    () => (categoryFilter ? allServices.filter((s) => s.categoryName === categoryFilter) : allServices),
    [allServices, categoryFilter]
  )

  // ── Render ─────────────────────────────────────────────────────────────────

  return (
    <div className="flex overflow-hidden" style={{ height: 'calc(100vh - 3.5rem)' }}>

      {/* ════════════ LEFT PANEL — Catalog (60%) ════════════ */}
      <div className="flex flex-col border-r border-gray-800 min-h-0 overflow-hidden" style={{ width: '60%' }}>

        {/* Page header */}
        <div className="flex items-center gap-3 px-4 py-3 border-b border-gray-800 shrink-0">
          <Link
            href="/queue"
            className="flex items-center justify-center h-9 w-9 rounded-xl bg-gray-800 hover:bg-gray-700 text-gray-400 transition-colors shrink-0"
          >
            <ArrowLeft className="h-4 w-4" />
          </Link>
          <div>
            <h1 className="text-base font-bold text-white leading-tight">New Transaction</h1>
            <p className="text-xs text-gray-500">
              {queueEntryId ? (
                <span className="text-yellow-400">
                  From Queue · {queueEntry?.queueNumber ?? '…'}
                </span>
              ) : (
                'Direct walk-in'
              )}
            </p>
          </div>
        </div>

        {/* Vehicle bar */}
        <div className="px-4 py-3 border-b border-gray-800 shrink-0 space-y-2">
          <div className="flex gap-2">
            <input
              type="text"
              placeholder="Plate number"
              value={lookupPlate}
              onChange={(e) => setLookupPlate(e.target.value.toUpperCase())}
              onKeyDown={(e) => { if (e.key === 'Enter') void handlePlateLookup() }}
              className="flex-1 min-h-11 px-3 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 text-base font-mono tracking-widest uppercase focus:outline-none focus:ring-2 focus:ring-blue-500"
              autoComplete="off"
            />
            <button
              type="button"
              onClick={() => void handlePlateLookup()}
              disabled={isLookingUp || !lookupPlate.trim()}
              className="min-h-11 px-4 rounded-xl bg-gray-800 border border-gray-700 text-gray-400 hover:text-white hover:border-gray-500 disabled:opacity-40 transition-colors"
            >
              {isLookingUp ? <RefreshCw className="h-4 w-4 animate-spin" /> : <Search className="h-4 w-4" />}
            </button>
          </div>

          {/* Vehicle found info */}
          {vehicleTypeId && (
            <div className="flex items-center gap-2 px-3 py-2 rounded-lg bg-gray-800/60 border border-gray-700/50">
              <span className="text-sm font-bold text-white font-mono">{plateNumber}</span>
              <span className="text-xs bg-gray-700 text-gray-300 px-2 py-0.5 rounded-full">{vehicleTypeName}</span>
              <span className="text-xs bg-gray-700 text-gray-300 px-2 py-0.5 rounded-full">{sizeName}</span>
              {customerId && <span className="text-xs text-blue-400 ml-auto">Linked customer</span>}
            </div>
          )}

          {/* Manual vehicle type + size (car not found) */}
          {(carNotFound || (!vehicleTypeId && plateNumber)) && (
            <div className="flex gap-2">
              <select
                value={vehicleTypeId}
                onChange={(e) => {
                  const vt = vehicleTypes.find((v) => v.id === e.target.value)
                  store.setVehicleType(e.target.value, vt?.name ?? '')
                }}
                className="flex-1 min-h-10 px-3 rounded-xl bg-gray-800 border border-gray-700 text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Vehicle type…</option>
                {vehicleTypes.map((vt) => (
                  <option key={vt.id} value={vt.id}>{vt.name}</option>
                ))}
              </select>
              <select
                value={sizeId}
                onChange={(e) => {
                  const sz = sizes.find((s) => s.id === e.target.value)
                  store.setVehicleSize(e.target.value, sz?.name ?? '')
                }}
                className="flex-1 min-h-10 px-3 rounded-xl bg-gray-800 border border-gray-700 text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Size…</option>
                {sizes.map((s) => (
                  <option key={s.id} value={s.id}>{s.name}</option>
                ))}
              </select>
            </div>
          )}
        </div>

        {/* Catalog tabs */}
        <div className="flex items-center gap-1 px-4 pt-3 shrink-0">
          {(
            [
              { key: 'services'     as const, label: 'Services',   badge: services.length     },
              { key: 'packages'     as const, label: 'Packages',   badge: packages.length     },
              { key: 'merchandise'  as const, label: 'Merch',      badge: merchandise.length  },
            ]
          ).map(({ key, label, badge }) => (
            <button
              key={key}
              type="button"
              onClick={() => setActiveTab(key)}
              className={`flex items-center gap-1.5 px-4 py-2 rounded-t-lg text-sm font-medium transition-colors ${
                activeTab === key
                  ? 'bg-gray-800 text-white border-b-2 border-blue-500'
                  : 'text-gray-500 hover:text-gray-300'
              }`}
            >
              {label}
              {badge > 0 && (
                <span className="text-xs bg-blue-600 text-white px-1.5 py-0.5 rounded-full leading-none font-bold">
                  {badge}
                </span>
              )}
            </button>
          ))}
        </div>

        {/* Catalog grid */}
        <div className="flex-1 overflow-y-auto min-h-0 bg-gray-800/10">
          {activeTab === 'services' && (
            <div className="p-3 space-y-3">
              {categories.length > 1 && (
                <div className="flex flex-wrap gap-1.5">
                  <button
                    type="button"
                    onClick={() => setCategoryFilter(null)}
                    className={`text-xs px-3 py-1 rounded-full transition-colors ${
                      !categoryFilter ? 'bg-blue-600 text-white' : 'bg-gray-700 text-gray-400 hover:bg-gray-600'
                    }`}
                  >
                    All
                  </button>
                  {categories.map((cat) => (
                    <button
                      key={cat}
                      type="button"
                      onClick={() => setCategoryFilter(cat)}
                      className={`text-xs px-3 py-1 rounded-full transition-colors ${
                        categoryFilter === cat ? 'bg-blue-600 text-white' : 'bg-gray-700 text-gray-400 hover:bg-gray-600'
                      }`}
                    >
                      {cat}
                    </button>
                  ))}
                </div>
              )}
              <div className="grid grid-cols-3 gap-2">
                {filteredServices.map((svc) => (
                  <ServiceCard
                    key={svc.id}
                    service={svc}
                    inCart={serviceInCart(svc.id)}
                    onToggle={() => toggleService(svc)}
                  />
                ))}
                {filteredServices.length === 0 && (
                  <p className="col-span-3 text-center text-sm text-gray-600 py-8">No services found</p>
                )}
              </div>
            </div>
          )}

          {activeTab === 'packages' && (
            <div className="p-3">
              <div className="grid grid-cols-3 gap-2">
                {allPackages.map((pkg) => (
                  <PackageCard
                    key={pkg.id}
                    pkg={pkg}
                    inCart={packageInCart(pkg.id)}
                    onToggle={() => togglePackage(pkg)}
                  />
                ))}
                {allPackages.length === 0 && (
                  <p className="col-span-3 text-center text-sm text-gray-600 py-8">No packages found</p>
                )}
              </div>
            </div>
          )}

          {activeTab === 'merchandise' && (
            <div className="p-3">
              <div className="grid grid-cols-3 gap-2">
                {allMerchandise.map((item) => (
                  <MerchandiseCard
                    key={item.id}
                    item={item}
                    cartQty={merchandiseQty(item.id)}
                    onAdd={() => addMerchandise(item)}
                  />
                ))}
                {allMerchandise.length === 0 && (
                  <p className="col-span-3 text-center text-sm text-gray-600 py-8">No merchandise found</p>
                )}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* ════════════ RIGHT PANEL — Order (40%) ════════════ */}
      <div className="flex flex-col min-h-0 overflow-hidden" style={{ width: '40%' }}>

        {/* Order header */}
        <div className="flex items-center justify-between px-4 py-3 border-b border-gray-800 shrink-0">
          <h2 className="font-bold text-white">Order Summary</h2>
          {itemCount > 0 && (
            <span className="text-xs bg-gray-700 text-gray-300 px-2.5 py-0.5 rounded-full">
              {itemCount} item{itemCount !== 1 ? 's' : ''}
            </span>
          )}
        </div>

        {/* Line items — scrollable */}
        <div className="flex-1 overflow-y-auto min-h-0 p-3 space-y-2">
          {itemCount === 0 && (
            <div className="flex flex-col items-center justify-center h-28 text-gray-700">
              <p className="text-sm">No items added yet</p>
              <p className="text-xs mt-1">Select from the catalog ←</p>
            </div>
          )}

          {services.map((item) => (
            <ServiceOrderRow
              key={item.localId}
              item={item}
              employees={employees}
              onRemove={() => store.removeService(item.localId)}
              onToggleEmployee={(empId) => store.toggleServiceEmployee(item.localId, empId)}
            />
          ))}

          {packages.map((item) => {
            const priceResult = packagePriceQueries.find((q) => q.data?.localId === item.localId)
            return (
              <ServiceOrderRow
                key={item.localId}
                item={item}
                employees={employees}
                resolvedPrice={priceResult?.data?.price}
                onRemove={() => store.removePackage(item.localId)}
                onToggleEmployee={(empId) => store.togglePackageEmployee(item.localId, empId)}
              />
            )
          })}

          {merchandise.map((item) => (
            <MerchandiseOrderRow
              key={item.localId}
              item={item}
              onRemove={() => store.removeMerchandise(item.localId)}
              onQtyChange={(qty) => store.updateMerchandiseQty(item.localId, qty)}
            />
          ))}
        </div>

        {/* ── Bottom: totals + payment + complete ───────────────────────────── */}
        <div className="shrink-0 divide-y divide-gray-800 border-t border-gray-800">

          {/* Totals */}
          <div className="px-4 py-3 space-y-1.5">
            <div className="flex justify-between text-sm text-gray-400">
              <span>Subtotal</span>
              <span className="font-mono">{peso(subtotal)}</span>
            </div>
            <div className="flex items-center justify-between text-sm text-gray-400">
              <span>Discount</span>
              <div className="flex items-center gap-1">
                <span className="text-gray-600 font-mono text-xs">₱</span>
                <input
                  type="number"
                  min={0}
                  step={0.01}
                  value={discountAmount || ''}
                  onChange={(e) =>
                    store.setDiscount(Math.max(0, parseFloat(e.target.value) || 0))
                  }
                  placeholder="0.00"
                  className="w-24 text-right bg-gray-800 border border-gray-700 rounded-lg px-2 py-0.5 text-sm font-mono text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>
            <div className="flex justify-between font-bold text-white pt-0.5 border-t border-gray-800/60">
              <span className="text-sm">Est. Total</span>
              <span className="font-mono text-xl text-white">{peso(estimatedTotal)}</span>
            </div>
          </div>

          {/* Payments */}
          <div className="px-4 py-3 space-y-2">
            <p className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Payment</p>

            {/* Added payment rows */}
            {payments.length > 0 && (
              <div className="space-y-1">
                {payments.map((p) => {
                  const m = PAYMENT_METHODS.find((pm) => pm.value === p.method)
                  return (
                    <div key={p.localId} className="flex items-center gap-2 text-sm">
                      <span className="text-gray-500 w-14 text-xs shrink-0">{m?.label}</span>
                      <span className="flex-1 font-mono text-green-400">{peso(p.amount)}</span>
                      {p.reference && (
                        <span className="text-xs text-gray-600 truncate max-w-[4rem]">{p.reference}</span>
                      )}
                      <button
                        type="button"
                        onClick={() => store.removePayment(p.localId)}
                        className="h-4 w-4 flex items-center justify-center rounded text-gray-600 hover:text-red-400 transition-colors"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </div>
                  )
                })}
                <div className="flex justify-between text-sm pt-1 border-t border-gray-800/60">
                  <span className="text-gray-400">Paid</span>
                  <span className={`font-mono font-bold ${totalPaid >= estimatedTotal ? 'text-green-400' : 'text-white'}`}>
                    {peso(totalPaid)}
                  </span>
                </div>
                {change > 0 && (
                  <div className="flex justify-between text-sm text-green-400">
                    <span>Change</span>
                    <span className="font-mono font-bold">{peso(change)}</span>
                  </div>
                )}
                {balance > 0 && (
                  <div className="flex justify-between text-sm text-orange-400">
                    <span>Balance</span>
                    <span className="font-mono font-bold">{peso(balance)}</span>
                  </div>
                )}
              </div>
            )}

            {/* Payment method selector */}
            <div className="flex gap-1">
              {PAYMENT_METHODS.map(({ value, label }) => (
                <button
                  key={value}
                  type="button"
                  onClick={() => handlePayMethodSelect(value)}
                  className={`flex-1 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                    payMethod === value
                      ? 'bg-blue-600 text-white'
                      : 'bg-gray-800 text-gray-500 hover:text-gray-300 border border-gray-700'
                  }`}
                >
                  {label}
                </button>
              ))}
            </div>

            {/* Amount + ref + add */}
            <div className="flex gap-2">
              <input
                type="number"
                min={0}
                step={0.01}
                value={payAmount}
                onChange={(e) => setPayAmount(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') handleAddPayment() }}
                placeholder={balance > 0 ? balance.toFixed(2) : '0.00'}
                className="flex-1 min-h-9 px-3 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-600 text-sm font-mono focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              {payMethod !== PaymentMethod.Cash && (
                <input
                  type="text"
                  value={payRef}
                  onChange={(e) => setPayRef(e.target.value)}
                  placeholder="Ref #"
                  className="w-20 min-h-9 px-2 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-600 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              )}
              <button
                type="button"
                onClick={handleAddPayment}
                disabled={!payAmount || parseFloat(payAmount) <= 0}
                className="min-h-9 px-4 rounded-xl bg-gray-700 hover:bg-gray-600 disabled:opacity-40 text-white text-sm font-semibold transition-colors shrink-0"
              >
                Add
              </button>
            </div>
          </div>

          {/* Notes */}
          <div className="px-4 py-2">
            <input
              type="text"
              value={notes}
              onChange={(e) => store.setNotes(e.target.value)}
              placeholder="Order notes (optional)"
              className="w-full px-3 py-2 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-600 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          {/* Error */}
          {submitError && (
            <div className="mx-4 flex items-start gap-2 rounded-xl bg-red-950/50 border border-red-800/60 px-3 py-2">
              <AlertCircle className="h-4 w-4 text-red-400 mt-0.5 shrink-0" />
              <p className="text-xs text-red-300">{submitError}</p>
            </div>
          )}

          {/* Complete button */}
          <div className="px-4 py-3">
            <button
              type="button"
              onClick={() => void handleComplete()}
              disabled={!canComplete || isSubmitting}
              className="w-full min-h-12 rounded-xl bg-green-600 hover:bg-green-500 disabled:bg-gray-700 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-bold text-base transition-colors flex items-center justify-center gap-2"
            >
              {isSubmitting ? (
                <><RefreshCw className="h-4 w-4 animate-spin" /> Processing…</>
              ) : (
                <><CheckCircle2 className="h-5 w-5" /> Complete Transaction</>
              )}
            </button>
            {!canComplete && !isSubmitting && (
              <p className="text-xs text-gray-600 text-center mt-1.5">
                {!plateNumber
                  ? 'Enter a plate number to start'
                  : !vehicleTypeId || !sizeId
                    ? 'Select vehicle type & size'
                    : itemCount === 0
                      ? 'Add at least one item to the order'
                      : estimatedTotal === 0
                        ? 'Items total must be greater than ₱0'
                        : `Add ${peso(balance)} more to complete payment`}
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

// ── Page wrapper (Suspense required for useSearchParams) ──────────────────────

export default function NewTransactionPage() {
  return (
    <Suspense>
      <NewTransactionContent />
    </Suspense>
  )
}
