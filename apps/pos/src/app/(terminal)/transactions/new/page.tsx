'use client'

import { useEffect, useRef, useState, Suspense, useMemo } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQuery, useQueries } from '@tanstack/react-query'
import Link from 'next/link'
import {
  ArrowLeft, Search, RefreshCw, X, Plus, Minus,
  Banknote, Smartphone, CreditCard, Building2, CheckCircle2,
  ChevronDown, ChevronUp, AlertCircle, Layers, BadgeCheck,
  CalendarClock,
} from 'lucide-react'

import { PaymentMethod, TransactionStatus, EmployeeType } from '@splashsphere/types'
import type {
  Car, QueueEntry, ServiceSummary, PackageSummary, Merchandise,
  Employee, VehicleType, Size, PackageDetail, ServiceDetail, TransactionSummary,
  ApiError, BookingAdminDetailDto,
} from '@splashsphere/types'
import type { PagedResult } from '@splashsphere/types'
import { formatPeso } from '@splashsphere/format'

import { apiClient } from '@/lib/api-client'
import { useBranch } from '@/lib/branch-context'
import { useCurrentShift, isShiftOpen } from '@/lib/use-shift'
import { useCustomerLoyalty } from '@/lib/use-loyalty'
import { useDisplayControl } from '@/hooks/use-display-control'
import {
  useDraftDisplayBroadcast,
  type DraftDisplayLineItem,
  type DraftDisplayPayload,
} from '@/hooks/use-draft-display-broadcast'
import {
  useTransactionStore,
  type ServiceLineItem,
  type PackageLineItem,
  type MerchandiseLine,
} from '@/lib/use-transaction-store'

// ── Helpers ───────────────────────────────────────────────────────────────────

const PAYMENT_METHODS: {
  value: PaymentMethod
  label: string
  Icon: React.ComponentType<{ className?: string }>
  activeCls: string
}[] = [
  { value: PaymentMethod.Cash,         label: 'Cash',   Icon: Banknote,    activeCls: 'bg-emerald-600 text-white' },
  { value: PaymentMethod.GCash,        label: 'GCash',  Icon: Smartphone,  activeCls: 'bg-blue-600 text-white' },
  { value: PaymentMethod.CreditCard,   label: 'Credit', Icon: CreditCard,  activeCls: 'bg-purple-600 text-white' },
  { value: PaymentMethod.DebitCard,    label: 'Debit',  Icon: CreditCard,  activeCls: 'bg-purple-600 text-white' },
  { value: PaymentMethod.BankTransfer, label: 'Bank',   Icon: Building2,   activeCls: 'bg-indigo-600 text-white' },
]

function parseServiceIds(raw: string | null): string[] {
  if (!raw) return []
  try { return JSON.parse(raw) as string[] } catch { return [] }
}

// ── Employee chip row ─────────────────────────────────────────────────────────

function EmployeePicker({
  selectedIds,
  employees,
  onToggle,
  itemPrice,
}: {
  selectedIds: string[]
  employees: Employee[]
  onToggle: (id: string) => void
  itemPrice?: number
}) {
  if (!employees.length)
    return <p className="text-sm text-gray-600 mt-1.5">No employees available</p>

  const count = selectedIds.length
  const commissionEach = count > 0 && itemPrice ? itemPrice / count : 0

  return (
    <div className="mt-1.5 space-y-2">
      <div className="grid grid-cols-2 gap-1.5">
        {employees.map((emp) => {
          const on = selectedIds.includes(emp.id)
          return (
            <button
              key={emp.id}
              type="button"
              onClick={() => onToggle(emp.id)}
              className={`flex items-center gap-2 px-2.5 py-2 rounded-lg text-sm transition-colors duration-150 active:scale-[0.97] ${
                on
                  ? 'bg-blue-600/20 text-blue-300 border border-blue-500/40'
                  : 'bg-gray-700/50 text-gray-400 border border-transparent hover:bg-gray-700 hover:text-white'
              }`}
            >
              <div className={`h-4 w-4 rounded border-2 flex items-center justify-center shrink-0 ${
                on ? 'border-blue-400 bg-blue-500' : 'border-gray-600'
              }`}>
                {on && <CheckCircle2 className="h-3 w-3 text-white" />}
              </div>
              <span className="truncate">{emp.firstName} {emp.lastName?.charAt(0)}.</span>
            </button>
          )
        })}
      </div>
      {count > 0 && (
        <p className="text-xs text-gray-500">
          {count} assigned{commissionEach > 0 && <> &middot; <span className="font-mono tabular-nums text-gray-400">{formatPeso(commissionEach)}</span> each</>}
        </p>
      )}
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
          <span className="text-sm font-mono tabular-nums font-semibold text-white">
            {displayPrice > 0 ? formatPeso(displayPrice) : '₱—'}
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
        <EmployeePicker
          selectedIds={item.employeeIds}
          employees={employees}
          onToggle={onToggleEmployee}
          itemPrice={displayPrice}
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
        <p className="text-xs text-gray-500 font-mono tabular-nums">{formatPeso(item.unitPrice)} each</p>
      </div>
      <div className="flex items-center gap-1 shrink-0">
        <button
          type="button"
          onClick={() => onQtyChange(item.quantity - 1)}
          className="h-8 w-8 flex items-center justify-center rounded-lg bg-gray-700 hover:bg-gray-600 text-gray-300 transition-colors duration-150 active:scale-[0.97]"
        >
          <Minus className="h-3 w-3" />
        </button>
        <span className="w-8 text-center text-sm font-mono tabular-nums font-bold text-white">{item.quantity}</span>
        <button
          type="button"
          onClick={() => onQtyChange(item.quantity + 1)}
          className="h-8 w-8 flex items-center justify-center rounded-lg bg-gray-700 hover:bg-gray-600 text-gray-300 transition-colors duration-150 active:scale-[0.97]"
        >
          <Plus className="h-3 w-3" />
        </button>
      </div>
      <span className="text-sm font-mono tabular-nums font-semibold text-white w-20 text-right shrink-0">
        {formatPeso(item.unitPrice * item.quantity)}
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
  displayPrice,
  onToggle,
}: {
  service: ServiceSummary
  inCart: boolean
  /** Resolved price from pricing matrix; falls back to basePrice when undefined */
  displayPrice?: number
  onToggle: () => void
}) {
  const price = displayPrice ?? service.basePrice
  const isPriceFromMatrix = displayPrice !== undefined && displayPrice !== service.basePrice
  return (
    <button
      type="button"
      onClick={onToggle}
      className={`w-full text-left p-3 rounded-xl border-2 transition-all duration-150 active:scale-[0.97] min-h-[72px] flex flex-col justify-between gap-1 relative ${
        inCart
          ? 'border-blue-500 bg-blue-600/15 text-blue-300 ring-2 ring-blue-500/20'
          : 'border-gray-700 bg-gray-800 text-gray-300 hover:border-blue-400/50 hover:text-white'
      }`}
    >
      {inCart && (
        <div className="absolute top-1.5 right-1.5">
          <CheckCircle2 className="h-4 w-4 text-blue-400" />
        </div>
      )}
      <p className="text-sm font-medium leading-tight line-clamp-2 pr-5">{service.name}</p>
      <div className="flex items-end justify-between gap-1">
        <span className="text-xs text-gray-500 truncate">{service.categoryName}</span>
        <span className={`text-lg font-mono tabular-nums font-semibold shrink-0 ${inCart ? 'text-blue-400' : isPriceFromMatrix ? 'text-emerald-400' : 'text-gray-400'}`}>
          {formatPeso(price)}
        </span>
      </div>
    </button>
  )
}

function PackageCard({
  pkg,
  inCart,
  displayPrice,
  isPriceLoading,
  onToggle,
}: {
  pkg: PackageSummary
  inCart: boolean
  /** Resolved price from pricing matrix; null means vehicle not selected or no matrix row */
  displayPrice?: number | null
  isPriceLoading?: boolean
  onToggle: () => void
}) {
  return (
    <button
      type="button"
      onClick={onToggle}
      className={`w-full text-left p-3 rounded-xl border-2 transition-all duration-150 active:scale-[0.97] min-h-[72px] flex flex-col justify-between gap-1 relative ${
        inCart
          ? 'border-purple-500 bg-purple-600/15 text-purple-300 ring-2 ring-purple-500/20'
          : 'border-gray-700 bg-gray-800 text-gray-300 hover:border-purple-400/50 hover:text-white'
      }`}
    >
      {inCart && (
        <div className="absolute top-1.5 right-1.5">
          <CheckCircle2 className="h-4 w-4 text-purple-400" />
        </div>
      )}
      <div className="flex items-start gap-1">
        <Layers className="h-3.5 w-3.5 shrink-0 mt-0.5 text-purple-400" />
        <p className="text-sm font-medium leading-tight line-clamp-2 pr-4">{pkg.name}</p>
      </div>
      <div className="flex items-end justify-between gap-1">
        <span className="text-xs text-gray-500">
          {pkg.serviceCount} service{pkg.serviceCount !== 1 ? 's' : ''}
        </span>
        {isPriceLoading ? (
          <span className="text-xs font-mono tabular-nums text-gray-600">···</span>
        ) : displayPrice != null ? (
          <span className={`text-lg font-mono tabular-nums font-semibold shrink-0 ${inCart ? 'text-purple-400' : 'text-emerald-400'}`}>
            {formatPeso(displayPrice)}
          </span>
        ) : (
          <span className="text-xs text-gray-600 italic">pick vehicle</span>
        )}
      </div>
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
      className={`w-full text-left p-3 rounded-xl border-2 transition-all duration-150 active:scale-[0.97] min-h-[72px] flex flex-col justify-between gap-1 relative ${
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
          <span className="text-xs font-mono tabular-nums font-semibold text-gray-400">{formatPeso(item.price)}</span>
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
  const editId = searchParams.get('editId')          // edit mode — tx already exists
  const prefillCarId = searchParams.get('carId')     // pre-fill from customer lookup
  const prefillPlate = searchParams.get('plate')     // pre-fill from customer lookup (no carId)
  const { branchId: contextBranchId, stationId: contextStationId } = useBranch()
  const { clear: clearDisplay } = useDisplayControl()
  const { data: currentShift, isLoading: shiftLoading } = useCurrentShift()
  const shiftOpen = isShiftOpen(currentShift)

  // ── Store ──────────────────────────────────────────────────────────────────
  const store = useTransactionStore()
  const {
    branchId, plateNumber, carId, customerId,
    vehicleTypeId, sizeId, vehicleTypeName, sizeName,
    services, packages, merchandise,
    discountAmount, tipAmount, notes, payments,
  } = store

  // ── Local UI state ─────────────────────────────────────────────────────────
  const [activeTab, setActiveTab] = useState<'services' | 'packages' | 'merchandise'>('services')
  const [mobilePanel, setMobilePanel] = useState<'catalog' | 'order'>('catalog')
  const [categoryFilter, setCategoryFilter] = useState<string | null>(null)
  const [lookupPlate, setLookupPlate] = useState('')
  const [isLookingUp, setIsLookingUp] = useState(false)
  const [carNotFound, setCarNotFound] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)
  // Cash-out alert shown after completing a non-cash transaction with a tip
  const [cashOutTip, setCashOutTip] = useState<{ amount: number; transactionId: string } | null>(null)
  // Receipt dialog shown after completion
  const [receiptData, setReceiptData] = useState<{
    transactionId: string
    transactionNumber?: string
    plateNumber: string
    vehicleType: string
    size: string
    services: { name: string; price: number }[]
    packages: { name: string; price: number }[]
    merchandise: { name: string; qty: number; price: number }[]
    subtotal: number
    discount: number
    tip: number
    total: number
    payments: { method: string; amount: number }[]
    change: number
  } | null>(null)
  const [payMethod, setPayMethod] = useState<PaymentMethod>(PaymentMethod.Cash)
  const [payAmount, setPayAmount] = useState('')
  const [payRef, setPayRef] = useState('')

  // ── Loyalty ───────────────────────────────────────────────────────────────
  const { data: loyaltySummary } = useCustomerLoyalty(customerId)

  const vehicleInitDone = useRef(false)
  const servicesInitDone = useRef(false)
  const editInitDone = useRef(false)

  /** Reset store and strip query params (queueEntryId, editId, carId, plate) so the page is fully clean. */
  const resetPage = () => {
    store.reset()
    setLookupPlate('')
    setCarNotFound(false)
    if (queueEntryId || editId || prefillCarId || prefillPlate) {
      router.replace('/transactions/new')
    }
  }

  // ── Boot ───────────────────────────────────────────────────────────────────
  useEffect(() => {
    store.reset()
    if (queueEntryId) store.setQueueEntry(queueEntryId)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // Sync branchId to store whenever the branch context resolves (async from localStorage)
  useEffect(() => {
    if (contextBranchId) store.setBranch(contextBranchId)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [contextBranchId])

  // ── API ────────────────────────────────────────────────────────────────────

  const { data: queueEntry } = useQuery({
    queryKey: ['queue-entry-tx', queueEntryId],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<QueueEntry>(`/queue/${queueEntryId}`, token ?? undefined)
    },
    enabled: !!queueEntryId,
  })

  // ── Booking detail (when queue entry is tied to a booking) ────────────────
  const bookingId = queueEntry?.bookingId ?? null
  const { data: bookingDetail } = useQuery({
    queryKey: ['booking-detail-tx', bookingId],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<BookingAdminDetailDto>(
        `/bookings/${encodeURIComponent(bookingId!)}`,
        token ?? undefined,
      )
    },
    enabled: !!bookingId,
    staleTime: 30_000,
  })

  const { data: queueCar, isPending: isQueueCarPending } = useQuery({
    queryKey: ['car-for-tx-queue', queueEntry?.plateNumber],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Car>(
        `/cars/lookup/${encodeURIComponent(queueEntry!.plateNumber)}`,
        token ?? undefined
      )
    },
    enabled: !!queueEntry?.plateNumber,
    retry: false, // 404 = car doesn't exist yet, no need to retry
  })

  // ── Prefill car from ?carId= (e.g. from customer lookup, direct flow) ─────
  const { data: prefillCar } = useQuery({
    queryKey: ['car-for-tx-prefill', prefillCarId],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Car>(`/cars/${prefillCarId}`, token ?? undefined)
    },
    enabled: !!prefillCarId && !queueEntryId && !editId,
    retry: false,
  })

  const prefillInitDone = useRef(false)
  useEffect(() => {
    if (prefillInitDone.current) return
    if (queueEntryId || editId) return
    // Plate-only prefill (no carId): trigger plate lookup on mount.
    if (!prefillCarId && prefillPlate) {
      prefillInitDone.current = true
      const plate = prefillPlate.trim().toUpperCase()
      setLookupPlate(plate)
      void handlePlateLookup(plate)
      return
    }
    // CarId prefill: wait for car fetch, then populate vehicle in store.
    if (prefillCarId && prefillCar) {
      prefillInitDone.current = true
      setLookupPlate(prefillCar.plateNumber)
      useTransactionStore.getState().setVehicle({
        plateNumber: prefillCar.plateNumber,
        carId: prefillCar.id,
        customerId: prefillCar.customerId,
        vehicleTypeId: prefillCar.vehicleTypeId,
        sizeId: prefillCar.sizeId,
        vehicleTypeName: prefillCar.vehicleTypeName,
        sizeName: prefillCar.sizeName,
      })
      setCarNotFound(false)
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [prefillCar, prefillCarId, prefillPlate, queueEntryId, editId])

  // ── Edit mode: load existing transaction ──────────────────────────────────

  const { data: editTx } = useQuery({
    queryKey: ['transaction-edit', editId],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<import('@splashsphere/types').TransactionDetail>(`/transactions/${editId}`, token ?? undefined)
    },
    enabled: !!editId,
    staleTime: 0,
  })

  // Populate store once when editTx loads
  useEffect(() => {
    if (!editTx || editInitDone.current) return
    editInitDone.current = true

    const s = useTransactionStore.getState()
    s.reset()
    if (contextBranchId) s.setBranch(contextBranchId)

    s.setVehicle({
      plateNumber: editTx.plateNumber,
      carId: editTx.carId,
      customerId: editTx.customerId ?? null,
      vehicleTypeId: editTx.vehicleTypeId,
      sizeId: editTx.sizeId,
      vehicleTypeName: editTx.vehicleTypeName,
      sizeName: editTx.sizeName,
    })
    s.setDiscount(editTx.discountAmount)
    s.setNotes(editTx.notes ?? '')

    editTx.services.forEach((svc) => {
      s.addService({
        serviceId: svc.serviceId,
        serviceName: svc.serviceName,
        categoryName: svc.categoryName,
        basePrice: svc.unitPrice,
        unitPrice: svc.unitPrice,
      })
      const added = useTransactionStore.getState().services.at(-1)!
      svc.employeeAssignments.forEach((a) =>
        useTransactionStore.getState().toggleServiceEmployee(added.localId, a.employeeId)
      )
    })

    editTx.packages.forEach((pkg) => {
      s.addPackage({ packageId: pkg.packageId, packageName: pkg.packageName, unitPrice: pkg.unitPrice })
      const added = useTransactionStore.getState().packages.at(-1)!
      pkg.employeeAssignments.forEach((a) =>
        useTransactionStore.getState().togglePackageEmployee(added.localId, a.employeeId)
      )
    })

    editTx.merchandise.forEach((m) => {
      s.addMerchandise({
        merchandiseId: m.merchandiseId,
        merchandiseName: m.merchandiseName,
        unitPrice: m.unitPrice,
        quantity: m.quantity,
      })
    })
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [editTx])

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

  // ── Eager detail fetches for entire catalog (cached; vehicle type doesn't change the data) ──

  const serviceDetailQueries = useQueries({
    queries: allServices.map((svc) => ({
      queryKey: ['service-detail', svc.id],
      queryFn: async () => {
        const token = await getToken()
        return apiClient.get<ServiceDetail>(`/services/${svc.id}`, token ?? undefined)
      },
      enabled: allServices.length > 0,
      staleTime: 5 * 60 * 1000,
    })),
  })

  const packageDetailQueries = useQueries({
    queries: allPackages.map((pkg) => ({
      queryKey: ['package-detail', pkg.id],
      queryFn: async () => {
        const token = await getToken()
        return apiClient.get<PackageDetail>(`/packages/${pkg.id}`, token ?? undefined)
      },
      enabled: allPackages.length > 0,
      staleTime: 5 * 60 * 1000,
    })),
  })

  // Catalog price maps — recomputed when vehicle type/size changes (no re-fetch needed)
  const catalogServicePrices = useMemo(() => {
    const map = new Map<string, number>()
    serviceDetailQueries.forEach((q, i) => {
      const svc = allServices[i]
      if (!svc) return
      if (!q.data || !vehicleTypeId || !sizeId) {
        map.set(svc.id, svc.basePrice)
        return
      }
      const row = q.data.pricing.find(
        (r) => r.vehicleTypeId === vehicleTypeId && r.sizeId === sizeId
      )
      map.set(svc.id, row?.price ?? svc.basePrice)
    })
    return map
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [serviceDetailQueries, vehicleTypeId, sizeId])

  const catalogPackagePrices = useMemo(() => {
    const map = new Map<string, number | null>()
    packageDetailQueries.forEach((q, i) => {
      const pkg = allPackages[i]
      if (!pkg) return
      if (!q.data || !vehicleTypeId || !sizeId) {
        map.set(pkg.id, null)
        return
      }
      const row = q.data.pricing.find(
        (r) => r.vehicleTypeId === vehicleTypeId && r.sizeId === sizeId
      )
      map.set(pkg.id, row?.price ?? null)
    })
    return map
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [packageDetailQueries, vehicleTypeId, sizeId])

  // Sync cart service prices from catalog detail cache when vehicle type/size changes.
  // Skip for booking-sourced carts so we don't clobber the booking's locked prices.
  useEffect(() => {
    if (!vehicleTypeId || !sizeId) return
    if (bookingDetail) return
    const storeServices = useTransactionStore.getState().services
    storeServices.forEach((svcItem) => {
      const idx = allServices.findIndex((s) => s.id === svcItem.serviceId)
      const detail = serviceDetailQueries[idx]?.data
      if (!detail) return
      const row = detail.pricing.find(
        (r) => r.vehicleTypeId === vehicleTypeId && r.sizeId === sizeId
      )
      const price = row?.price ?? svcItem.basePrice
      if (svcItem.unitPrice !== price) {
        useTransactionStore.getState().updateServicePrice(svcItem.localId, price)
      }
    })
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehicleTypeId, sizeId, serviceDetailQueries, bookingDetail])

  // Sync cart package prices from catalog detail cache when vehicle type/size changes
  useEffect(() => {
    if (!vehicleTypeId || !sizeId) return
    const storePackages = useTransactionStore.getState().packages
    storePackages.forEach((pkgItem) => {
      const idx = allPackages.findIndex((p) => p.id === pkgItem.packageId)
      const detail = packageDetailQueries[idx]?.data
      if (!detail) return
      const row = detail.pricing.find(
        (r) => r.vehicleTypeId === vehicleTypeId && r.sizeId === sizeId
      )
      const price = row?.price ?? 0
      if (pkgItem.unitPrice !== price) {
        useTransactionStore.getState().updatePackagePrice(pkgItem.localId, price)
      }
    })
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehicleTypeId, sizeId, packageDetailQueries])

  // ── Init from queue entry ──────────────────────────────────────────────────

  // Pre-fill the plate input as soon as the queue entry loads — don't wait for the car lookup.
  useEffect(() => {
    if (queueEntry?.plateNumber) setLookupPlate(queueEntry.plateNumber)
  }, [queueEntry?.plateNumber])

  // Populate vehicle state once the car lookup settles (found or confirmed not found).
  useEffect(() => {
    if (!queueEntry || vehicleInitDone.current) return
    if (isQueueCarPending) return // still fetching — wait

    vehicleInitDone.current = true

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
  }, [queueEntry, queueCar, isQueueCarPending])

  useEffect(() => {
    if (!queueEntry || !allServices.length || servicesInitDone.current) return
    // If the queue entry is tied to a booking, wait for the booking detail before
    // pre-filling so that we can lock in the booking's exact prices.
    if (queueEntry.bookingId && !bookingDetail) return
    servicesInitDone.current = true

    const { addService } = useTransactionStore.getState()

    // ── Booking-driven pre-fill: use locked booking prices (when available). ──
    if (bookingDetail && bookingDetail.services.length > 0) {
      bookingDetail.services.forEach((bs) => {
        const svc = allServices.find((s) => s.id === bs.serviceId)
        if (!svc) return
        // Prefer exact locked price; fall back to midpoint of range, then basePrice.
        const lockedPrice = bs.price
          ?? (bs.priceMin != null && bs.priceMax != null
                ? (bs.priceMin + bs.priceMax) / 2
                : svc.basePrice)
        addService({
          serviceId: svc.id,
          serviceName: svc.name,
          categoryName: svc.categoryName,
          basePrice: svc.basePrice,
          unitPrice: lockedPrice,
        })
      })
      return
    }

    // ── Walk-in / queue-only pre-fill: use preferredServices at basePrice. ──
    const ids = parseServiceIds(queueEntry.preferredServices)
    ids.forEach((sid) => {
      const svc = allServices.find((s) => s.id === sid)
      if (svc) {
        addService({
          serviceId: svc.id,
          serviceName: svc.name,
          categoryName: svc.categoryName,
          basePrice: svc.basePrice,
          unitPrice: svc.basePrice,
        })
      }
    })
  }, [queueEntry, allServices, bookingDetail])

  // ── Plate lookup (direct flow) ─────────────────────────────────────────────

  const handlePlateLookup = async (plateOverride?: string) => {
    const raw = plateOverride ?? lookupPlate
    const plate = raw.trim().toUpperCase()
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
    else { store.addService({ serviceId: svc.id, serviceName: svc.name, categoryName: svc.categoryName, basePrice: svc.basePrice, unitPrice: catalogServicePrices.get(svc.id) ?? svc.basePrice }) }
  }

  const togglePackage = (pkg: PackageSummary) => {
    const existing = packages.find((p) => p.packageId === pkg.id)
    if (existing) { store.removePackage(existing.localId) }
    else { store.addPackage({ packageId: pkg.id, packageName: pkg.name, unitPrice: catalogPackagePrices.get(pkg.id) ?? 0 }) }
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
  const tip              = Math.max(0, tipAmount)
  const customerPayable  = estimatedTotal + tip
  const totalPaid        = useMemo(() => payments.reduce((s, p) => s + p.amount, 0), [payments])
  const balance          = Math.max(0, customerPayable - totalPaid)
  const change           = Math.max(0, totalPaid - customerPayable)

  // ── Draft customer-display broadcast ───────────────────────────────────────
  // Pushes the in-progress cart to the paired display via SignalR, so the
  // customer sees items build live before the transaction is POSTed. Once
  // editId is set (we're editing an existing tx) or the cashier submits, we
  // switch off — the real TransactionUpdatedEvent pipeline takes over.
  const draftPayload = useMemo<DraftDisplayPayload | null>(() => {
    const items: DraftDisplayLineItem[] = [
      ...services.map((s): DraftDisplayLineItem => ({
        id: s.localId,
        name: s.serviceName,
        type: 'service',
        quantity: 1,
        unitPrice: s.unitPrice,
        totalPrice: s.unitPrice,
      })),
      ...packages.map((p): DraftDisplayLineItem => ({
        id: p.localId,
        name: p.packageName,
        type: 'package',
        quantity: 1,
        unitPrice: p.unitPrice,
        totalPrice: p.unitPrice,
      })),
      ...merchandise.map((m): DraftDisplayLineItem => ({
        id: m.localId,
        name: m.merchandiseName,
        type: 'merchandise',
        quantity: m.quantity,
        unitPrice: m.unitPrice,
        totalPrice: m.unitPrice * m.quantity,
      })),
    ]

    // Empty cart → don't broadcast. Display stays on whatever it had.
    if (items.length === 0) return null

    const vehicleTypeSize = vehicleTypeName && sizeName
      ? `${vehicleTypeName} / ${sizeName}`
      : null

    return {
      transactionId: 'draft',
      vehiclePlate: plateNumber || null,
      vehicleMakeModel: null, // make/model not in cart store
      vehicleTypeSize,
      customerName: null,     // not tracked in cart; surfaces once POSTed
      loyaltyTier: loyaltySummary?.tierName ?? null,
      items,
      subtotal,
      discountAmount: discount,
      discountLabel: discount > 0 ? 'Discount' : null,
      taxAmount: 0,
      total: estimatedTotal,
    }
  }, [
    services, packages, merchandise,
    plateNumber, vehicleTypeName, sizeName,
    loyaltySummary?.tierName,
    subtotal, discount, estimatedTotal,
  ])

  useDraftDisplayBroadcast({
    payload: draftPayload,
    // Disable while editing an existing tx (real events fire) or mid-submit.
    enabled: !editId && !isSubmitting,
  })

  // ── Payment helpers ────────────────────────────────────────────────────────

  const handlePayMethodSelect = (method: PaymentMethod) => {
    setPayMethod(method)
    if (balance > 0) setPayAmount(balance.toFixed(2))
    // When switching to non-cash, auto-fill with the full customer-payable amount if unpaid
    if (method !== PaymentMethod.Cash && totalPaid === 0 && customerPayable > 0) {
      setPayAmount(customerPayable.toFixed(2))
    }
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

  const vehicleReady = !!plateNumber && !!vehicleTypeId && !!sizeId
  const itemsReady   = itemCount > 0 && estimatedTotal > 0

  const canComplete  = vehicleReady && itemsReady && totalPaid >= customerPayable
  const canPayLater  = vehicleReady && itemsReady && !editId
  const canSaveItems = itemsReady && !!editId

  // Shared body builder for create/pay-later
  const buildCreateBody = () => ({
    branchId: contextBranchId || branchId || undefined,
    carId,
    customerId,
    vehicleTypeId,
    sizeId,
    plateNumber,
    queueEntryId,
    // Drives customer-display routing — null when no station is paired.
    posStationId: contextStationId || null,
    services: services.map((s) => ({ serviceId: s.serviceId, employeeIds: s.employeeIds, notes: null })),
    packages: packages.map((p) => ({ packageId: p.packageId, employeeIds: p.employeeIds, notes: null })),
    merchandise: merchandise.map((m) => ({ merchandiseId: m.merchandiseId, quantity: m.quantity })),
    discountAmount: discount,
    taxAmount: 0,
    tipAmount: tip,
    notes: notes || null,
  })

  const handleComplete = async () => {
    setIsSubmitting(true)
    setSubmitError(null)
    try {
      const token = await getToken()
      const { transactionId } = await apiClient.post<{ transactionId: string }>(
        '/transactions', buildCreateBody(), token ?? undefined
      )
      for (const p of payments) {
        try {
          await apiClient.post(
            `/transactions/${transactionId}/payments`,
            { paymentMethod: p.method, amount: p.amount, referenceNumber: p.reference || null },
            token ?? undefined
          )
        } catch { /* cashier can add on detail page */ }
      }
      // How much of the tip was covered by cash?
      // Cash paid beyond the service total absorbs the tip first.
      const totalCashPaid = payments
        .filter((p) => p.method === PaymentMethod.Cash)
        .reduce((s, p) => s + p.amount, 0)
      const tipCoveredByCash = Math.min(tip, Math.max(0, totalCashPaid - estimatedTotal))
      const cashOutAmount = tip - tipCoveredByCash

      // Build receipt data from current cart state before resetting
      const receipt = {
        transactionId,
        plateNumber,
        vehicleType: vehicleTypeName,
        size: sizeName,
        services: services.map(s => ({ name: s.serviceName, price: s.unitPrice })),
        packages: packages.map(p => ({ name: p.packageName, price: p.unitPrice })),
        merchandise: merchandise.map(m => ({ name: m.merchandiseName, qty: m.quantity, price: m.unitPrice * m.quantity })),
        subtotal,
        discount,
        tip,
        total: customerPayable,
        payments: payments.map(p => ({
          method: PAYMENT_METHODS.find(pm => pm.value === p.method)?.label ?? 'Other',
          amount: p.amount,
        })),
        change,
      }

      resetPage()
      if (cashOutAmount > 0) {
        setCashOutTip({ amount: cashOutAmount, transactionId })
      } else {
        setReceiptData(receipt)
      }
    } catch (err) {
      const apiErr = err as ApiError
      setSubmitError(apiErr?.detail ?? apiErr?.title ?? 'Failed to create transaction.')
    } finally {
      setIsSubmitting(false)
    }
  }

  // Pay Later — creates transaction without payments; cashier pays from detail page
  const handlePayLater = async () => {
    setIsSubmitting(true)
    setSubmitError(null)
    try {
      const token = await getToken()
      const { transactionId } = await apiClient.post<{ transactionId: string }>(
        '/transactions', buildCreateBody(), token ?? undefined
      )
      // Customer is walking away — release the display from Tx so the next
      // customer at the counter sees Idle (branding + promos), not the
      // parked bill. The ?parked=1 flag tells the detail page not to
      // re-show on mount.
      void clearDisplay()
      resetPage()
      router.push(`/transactions/${transactionId}?parked=1`)
    } catch (err) {
      const apiErr = err as ApiError
      setSubmitError(apiErr?.detail ?? apiErr?.title ?? 'Failed to create transaction.')
    } finally {
      setIsSubmitting(false)
    }
  }

  // Save Changes — updates items on an existing InProgress transaction
  const handleSaveChanges = async () => {
    if (!editId) return
    setIsSubmitting(true)
    setSubmitError(null)
    try {
      const token = await getToken()
      await apiClient.patch(
        `/transactions/${editId}/items`,
        {
          services: services.map((s) => ({ serviceId: s.serviceId, employeeIds: s.employeeIds })),
          packages: packages.map((p) => ({ packageId: p.packageId, employeeIds: p.employeeIds })),
          merchandise: merchandise.map((m) => ({ merchandiseId: m.merchandiseId, quantity: m.quantity })),
          discountAmount: discount,
          notes: notes || null,
        },
        token ?? undefined
      )
      resetPage()
      router.push(`/transactions/${editId}`)
    } catch (err) {
      const apiErr = err as ApiError
      setSubmitError(apiErr?.detail ?? apiErr?.title ?? 'Failed to update transaction.')
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

  if (shiftLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <RefreshCw className="h-5 w-5 animate-spin text-gray-500" />
      </div>
    )
  }

  if (!shiftOpen) {
    return (
      <div className="p-4 max-w-lg mx-auto space-y-6 pt-16">
        <div className="rounded-xl border border-yellow-700/50 bg-yellow-950/30 p-6 text-center space-y-3">
          <AlertCircle className="h-10 w-10 text-yellow-400 mx-auto" />
          <h2 className="text-xl font-bold text-white">Shift Required</h2>
          <p className="text-base text-gray-400">
            You must open a shift before creating a transaction.
          </p>
          <Link
            href="/shift/open"
            className="inline-flex items-center gap-2 px-5 min-h-[56px] rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-semibold text-base transition-colors duration-150 active:scale-[0.97]"
          >
            Open Shift
          </Link>
        </div>
      </div>
    )
  }

  return (
    <>
    {/* Mobile panel toggle */}
    <div className="md:hidden flex border-b border-gray-800 shrink-0">
      <button
        type="button"
        onClick={() => setMobilePanel('catalog')}
        className={`flex-1 py-3 text-sm font-semibold text-center transition-colors duration-150 ${
          mobilePanel === 'catalog' ? 'text-white border-b-2 border-blue-500' : 'text-gray-500'
        }`}
      >
        Vehicle &amp; Services
      </button>
      <button
        type="button"
        onClick={() => setMobilePanel('order')}
        className={`flex-1 py-3 text-sm font-semibold text-center transition-colors duration-150 relative ${
          mobilePanel === 'order' ? 'text-white border-b-2 border-blue-500' : 'text-gray-500'
        }`}
      >
        Summary &amp; Payment
        {itemCount > 0 && (
          <span className="ml-1.5 text-xs bg-blue-600 text-white px-1.5 py-0.5 rounded-full">{itemCount}</span>
        )}
      </button>
    </div>

    <div className="flex overflow-hidden" style={{ height: 'calc(100vh - 7rem)' }}>

      {/* ════════════ LEFT PANEL — Catalog (60%) ════════════ */}
      <div className={`flex flex-col border-r border-gray-800 min-h-0 overflow-hidden ${mobilePanel === 'order' ? 'hidden md:flex' : 'flex'}`} style={{ width: undefined }} data-panel="catalog">
        <style>{`[data-panel="catalog"] { width: 100%; } @media (min-width: 768px) { [data-panel="catalog"] { width: 60%; } }`}</style>

        {/* Page header */}
        <div className="flex items-center gap-3 px-4 py-3 border-b border-gray-800 shrink-0">
          <Link
            href="/queue"
            className="flex items-center justify-center h-10 w-10 rounded-xl bg-gray-800 hover:bg-gray-700 text-gray-400 transition-colors duration-150 active:scale-[0.97] shrink-0"
          >
            <ArrowLeft className="h-4 w-4" />
          </Link>
          <div>
            <h1 className="text-base font-bold text-white leading-tight">
              {editId ? 'Edit Transaction' : 'New Transaction'}
            </h1>
            <p className="text-xs text-gray-500">
              {editId ? (
                <span className="text-yellow-400">Editing · {editTx?.transactionNumber ?? '…'}</span>
              ) : queueEntryId ? (
                <span className="text-yellow-400">From Queue · {queueEntry?.queueNumber ?? '…'}</span>
              ) : (
                'Direct walk-in'
              )}
            </p>
          </div>
        </div>

        {/* Booking banner — present only when this transaction originated from a booking */}
        {bookingDetail && (
          <div className="mx-4 mt-3 flex items-start gap-2 rounded-xl bg-indigo-500/10 border border-indigo-500/30 px-3 py-2 shrink-0">
            <CalendarClock className="h-4 w-4 text-indigo-400 mt-0.5 shrink-0" />
            <div className="flex-1 min-w-0 text-xs text-indigo-200/90 space-y-0.5">
              <p>
                <span className="font-semibold text-indigo-300">From booking</span>
                {' — '}
                <span>Slot: {new Intl.DateTimeFormat('en-PH', {
                  hour: 'numeric', minute: '2-digit', hour12: true, timeZone: 'Asia/Manila',
                }).format(new Date(bookingDetail.slotStartUtc))}</span>
              </p>
              <p className="text-indigo-200/70">Services auto-filled from the booking.</p>
            </div>
          </div>
        )}

        {/* Vehicle bar */}
        <div className="px-4 py-3 border-b border-gray-800 shrink-0 space-y-2">

          {/* Edit mode: vehicle is locked, show read-only badge */}
          {editId && vehicleTypeId && (
            <div className="flex items-center gap-2 px-3 py-2 rounded-lg bg-yellow-900/20 border border-yellow-700/40">
              <span className="text-sm font-bold text-white font-mono">{plateNumber}</span>
              <span className="text-xs bg-gray-700 text-gray-300 px-2 py-0.5 rounded-full">{vehicleTypeName}</span>
              <span className="text-xs bg-gray-700 text-gray-300 px-2 py-0.5 rounded-full">{sizeName}</span>
              <span className="text-xs text-yellow-400 ml-auto">Vehicle locked</span>
            </div>
          )}

          {!editId && (
            <>
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
                  className="min-h-[44px] px-4 rounded-xl bg-gray-800 border border-gray-700 text-gray-400 hover:text-white hover:border-gray-500 disabled:opacity-40 transition-colors duration-150 active:scale-[0.97]"
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
                  {customerId && (
                    <span className="flex items-center gap-1.5 text-xs ml-auto">
                      <span className="text-blue-400">Linked customer</span>
                      {loyaltySummary && (
                        <>
                          <span className="bg-amber-600/30 text-amber-400 px-1.5 py-0.5 rounded-full">{loyaltySummary.tierName}</span>
                          <span className="font-mono text-gray-400">{loyaltySummary.pointsBalance.toLocaleString()} pts</span>
                        </>
                      )}
                    </span>
                  )}
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
            </>
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
              className={`flex items-center gap-1.5 px-4 py-2 min-h-[44px] rounded-t-lg text-sm font-medium transition-colors duration-150 active:scale-[0.97] ${
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
                    className={`text-xs px-3 py-1.5 rounded-full transition-colors duration-150 active:scale-[0.97] ${
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
                      className={`text-xs px-3 py-1.5 rounded-full transition-colors duration-150 active:scale-[0.97] ${
                        categoryFilter === cat ? 'bg-blue-600 text-white' : 'bg-gray-700 text-gray-400 hover:bg-gray-600'
                      }`}
                    >
                      {cat}
                    </button>
                  ))}
                </div>
              )}
              <div className="grid grid-cols-2 lg:grid-cols-3 gap-2">
                {filteredServices.map((svc) => (
                  <ServiceCard
                    key={svc.id}
                    service={svc}
                    inCart={serviceInCart(svc.id)}
                    displayPrice={catalogServicePrices.get(svc.id)}
                    onToggle={() => toggleService(svc)}
                  />
                ))}
                {filteredServices.length === 0 && (
                  <p className="col-span-full text-center text-sm text-gray-600 py-8">No services found</p>
                )}
              </div>
            </div>
          )}

          {activeTab === 'packages' && (
            <div className="p-3">
              <div className="grid grid-cols-2 lg:grid-cols-3 gap-2">
                {allPackages.map((pkg) => {
                  const pkgIdx = allPackages.indexOf(pkg)
                  return (
                    <PackageCard
                      key={pkg.id}
                      pkg={pkg}
                      inCart={packageInCart(pkg.id)}
                      displayPrice={catalogPackagePrices.get(pkg.id)}
                      isPriceLoading={!!vehicleTypeId && !!sizeId && packageDetailQueries[pkgIdx]?.isLoading}
                      onToggle={() => togglePackage(pkg)}
                    />
                  )
                })}
                {allPackages.length === 0 && (
                  <p className="col-span-full text-center text-sm text-gray-600 py-8">No packages found</p>
                )}
              </div>
            </div>
          )}

          {activeTab === 'merchandise' && (
            <div className="p-3">
              <div className="grid grid-cols-2 lg:grid-cols-3 gap-2">
                {allMerchandise.map((item) => (
                  <MerchandiseCard
                    key={item.id}
                    item={item}
                    cartQty={merchandiseQty(item.id)}
                    onAdd={() => addMerchandise(item)}
                  />
                ))}
                {allMerchandise.length === 0 && (
                  <p className="col-span-full text-center text-sm text-gray-600 py-8">No merchandise found</p>
                )}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* ════════════ RIGHT PANEL — Order (40%) ════════════ */}
      <div className={`flex flex-col min-h-0 overflow-hidden ${mobilePanel === 'catalog' ? 'hidden md:flex' : 'flex'}`} style={{ width: undefined }} data-panel="order">
        <style>{`[data-panel="order"] { width: 100%; } @media (min-width: 768px) { [data-panel="order"] { width: 40%; } }`}</style>

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

          {packages.map((item) => (
            <ServiceOrderRow
              key={item.localId}
              item={item}
              employees={employees}
              resolvedPrice={catalogPackagePrices.get(item.packageId) ?? undefined}
              onRemove={() => store.removePackage(item.localId)}
              onToggleEmployee={(empId) => store.togglePackageEmployee(item.localId, empId)}
            />
          ))}

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
              <span className="font-mono tabular-nums">{formatPeso(subtotal)}</span>
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
            {tip > 0 && (
              <div className="flex justify-between text-sm text-gray-400">
                <span>Service Total</span>
                <span className="font-mono tabular-nums">{formatPeso(estimatedTotal)}</span>
              </div>
            )}
            <div className="flex items-center justify-between text-sm text-gray-400">
              <span>Tip</span>
              <div className="flex items-center gap-1">
                <span className="text-gray-600 font-mono text-xs">₱</span>
                <input
                  type="number"
                  min={0}
                  step={0.01}
                  value={tipAmount || ''}
                  onChange={(e) =>
                    store.setTip(Math.max(0, parseFloat(e.target.value) || 0))
                  }
                  placeholder="0.00"
                  className="w-24 text-right bg-gray-800 border border-gray-700 rounded-lg px-2 py-0.5 text-sm font-mono text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>
            {loyaltySummary && customerPayable > 0 && (
              <div className="flex items-center justify-between text-xs text-amber-400/80">
                <span>{loyaltySummary.tierName} Member</span>
                <span className="font-mono">~{Math.floor(customerPayable / 100).toLocaleString()} pts estimated</span>
              </div>
            )}
            <div className="flex justify-between font-bold text-white pt-0.5 border-t border-gray-800/60">
              <span className="text-base">{tip > 0 ? 'Customer Pays' : 'Est. Total'}</span>
              <span className="font-mono tabular-nums text-2xl font-bold text-white">{formatPeso(customerPayable)}</span>
            </div>
          </div>

          {/* Payments — hidden in edit mode (cashier pays from the detail page) */}
          {!editId && <div className="px-4 py-3 space-y-2">
            <p className="text-xs font-semibold text-gray-500 uppercase tracking-wider">Payment</p>

            {/* Added payment rows */}
            {payments.length > 0 && (
              <div className="space-y-1">
                {payments.map((p) => {
                  const m = PAYMENT_METHODS.find((pm) => pm.value === p.method)
                  return (
                    <div key={p.localId} className="flex items-center gap-2 text-sm">
                      <span className="text-gray-500 w-14 text-xs shrink-0">{m?.label}</span>
                      <span className="flex-1 font-mono tabular-nums text-green-400">{formatPeso(p.amount)}</span>
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
                  <span className={`font-mono tabular-nums font-bold ${totalPaid >= customerPayable ? 'text-green-400' : 'text-white'}`}>
                    {formatPeso(totalPaid)}
                  </span>
                </div>
                {change > 0 && (
                  <div className="flex justify-between text-sm text-green-400">
                    <span>Change</span>
                    <span className="font-mono tabular-nums font-bold">{formatPeso(change)}</span>
                  </div>
                )}
                {balance > 0 && (
                  <div className="flex justify-between text-sm text-orange-400">
                    <span>Balance</span>
                    <span className="font-mono tabular-nums font-bold">{formatPeso(balance)}</span>
                  </div>
                )}
              </div>
            )}

            {/* Payment method selector */}
            <div className="flex gap-1">
              {PAYMENT_METHODS.map(({ value, label, Icon, activeCls }) => (
                <button
                  key={value}
                  type="button"
                  onClick={() => handlePayMethodSelect(value)}
                  className={`flex-1 flex flex-col items-center justify-center gap-0.5 min-h-[44px] rounded-lg text-xs font-medium transition-colors duration-150 active:scale-[0.97] ${
                    payMethod === value
                      ? activeCls
                      : 'bg-gray-800 text-gray-500 hover:text-gray-300 border border-gray-700'
                  }`}
                >
                  <Icon className="h-4 w-4" />
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
                className="flex-1 min-h-[44px] px-3 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-600 text-sm font-mono tabular-nums focus:outline-none focus:ring-1 focus:ring-blue-500"
              />
              {payMethod !== PaymentMethod.Cash && (
                <input
                  type="text"
                  value={payRef}
                  onChange={(e) => setPayRef(e.target.value)}
                  placeholder="Ref #"
                  className="w-20 min-h-[44px] px-2 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-600 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              )}
              <button
                type="button"
                onClick={handleAddPayment}
                disabled={!payAmount || parseFloat(payAmount) <= 0}
                className="min-h-[44px] px-4 rounded-xl bg-gray-700 hover:bg-gray-600 disabled:opacity-40 text-white text-sm font-semibold transition-colors duration-150 active:scale-[0.97] shrink-0"
              >
                Add
              </button>
            </div>
          </div>}  {/* end !editId payment section */}

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

          {/* Action buttons */}
          <div className="px-4 py-3 space-y-2">
            {editId ? (
              /* ── Edit mode: Save Changes ─────────────────────────────── */
              <button
                type="button"
                onClick={() => void handleSaveChanges()}
                disabled={!canSaveItems || isSubmitting}
                className="w-full min-h-[56px] rounded-xl bg-blue-600 hover:bg-blue-500 disabled:bg-gray-700 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-bold text-base transition-colors duration-150 active:scale-[0.97] flex items-center justify-center gap-2"
              >
                {isSubmitting
                  ? <><RefreshCw className="h-4 w-4 animate-spin" /> Saving…</>
                  : <><CheckCircle2 className="h-5 w-5" /> Save Changes</>}
              </button>
            ) : (
              /* ── Create mode: Pay Later + Complete ───────────────────── */
              <>
                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={() => void handlePayLater()}
                    disabled={!canPayLater || isSubmitting}
                    className="flex-1 min-h-[44px] rounded-xl bg-gray-700 hover:bg-gray-600 disabled:bg-gray-800 disabled:text-gray-600 disabled:cursor-not-allowed text-white font-semibold text-sm transition-colors duration-150 active:scale-[0.97] flex items-center justify-center gap-1.5"
                  >
                    {isSubmitting ? <RefreshCw className="h-4 w-4 animate-spin" /> : <><BadgeCheck className="h-4 w-4" /> Pay Later</>}
                  </button>
                  <button
                    type="button"
                    onClick={() => void handleComplete()}
                    disabled={!canComplete || isSubmitting}
                    className="flex-1 min-h-[56px] rounded-xl bg-green-600 hover:bg-green-500 disabled:bg-gray-700 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-bold text-base transition-colors duration-150 active:scale-[0.97] flex items-center justify-center gap-2"
                  >
                    {isSubmitting
                      ? <><RefreshCw className="h-4 w-4 animate-spin" /> Processing…</>
                      : <><CheckCircle2 className="h-5 w-5" /> Complete</>}
                  </button>
                </div>
                {!canPayLater && !canComplete && !isSubmitting && (
                  <p className="text-xs text-gray-600 text-center">
                    {!plateNumber
                      ? 'Enter a plate number to start'
                      : !vehicleTypeId || !sizeId
                        ? 'Select vehicle type & size'
                        : itemCount === 0
                          ? 'Add at least one item to the order'
                          : 'Items total must be greater than ₱0'}
                  </p>
                )}
                {canPayLater && !canComplete && !isSubmitting && (
                  <p className="text-xs text-gray-500 text-center">
                    Add {formatPeso(balance)} to pay now, or use Pay Later
                  </p>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </div>

    {/* ── Receipt dialog ────────────────────────────────────────────────── */}
    {receiptData && (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4">
        <div className="w-full max-w-md bg-white rounded-2xl overflow-hidden print:shadow-none print:rounded-none print:max-w-none">
          {/* Receipt content (printable) */}
          <div id="receipt-content" className="p-6 text-black space-y-4">
            <div className="text-center space-y-1">
              <h2 className="text-lg font-bold">SplashSphere</h2>
              <p className="text-xs text-gray-500">Transaction Complete</p>
            </div>
            <div className="border-t border-dashed border-gray-300 pt-3 text-sm space-y-1">
              <div className="flex justify-between">
                <span className="text-gray-500">Plate</span>
                <span className="font-mono font-bold">{receiptData.plateNumber}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Vehicle</span>
                <span>{receiptData.vehicleType} · {receiptData.size}</span>
              </div>
              {receiptData.transactionNumber && (
                <div className="flex justify-between">
                  <span className="text-gray-500">Tx#</span>
                  <span className="font-mono text-xs">{receiptData.transactionNumber}</span>
                </div>
              )}
            </div>
            <div className="border-t border-dashed border-gray-300 pt-3 space-y-1 text-sm">
              {receiptData.services.map((s, i) => (
                <div key={i} className="flex justify-between">
                  <span>{s.name}</span>
                  <span className="font-mono tabular-nums">{formatPeso(s.price)}</span>
                </div>
              ))}
              {receiptData.packages.map((p, i) => (
                <div key={i} className="flex justify-between">
                  <span>{p.name} <span className="text-gray-400">(pkg)</span></span>
                  <span className="font-mono tabular-nums">{formatPeso(p.price)}</span>
                </div>
              ))}
              {receiptData.merchandise.map((m, i) => (
                <div key={i} className="flex justify-between">
                  <span>{m.name} ×{m.qty}</span>
                  <span className="font-mono tabular-nums">{formatPeso(m.price)}</span>
                </div>
              ))}
            </div>
            <div className="border-t border-dashed border-gray-300 pt-3 space-y-1 text-sm">
              <div className="flex justify-between">
                <span>Subtotal</span>
                <span className="font-mono tabular-nums">{formatPeso(receiptData.subtotal)}</span>
              </div>
              {receiptData.discount > 0 && (
                <div className="flex justify-between text-red-600">
                  <span>Discount</span>
                  <span className="font-mono tabular-nums">-{formatPeso(receiptData.discount)}</span>
                </div>
              )}
              {receiptData.tip > 0 && (
                <div className="flex justify-between">
                  <span>Tip</span>
                  <span className="font-mono tabular-nums">{formatPeso(receiptData.tip)}</span>
                </div>
              )}
              <div className="flex justify-between font-bold text-base pt-1 border-t border-gray-200">
                <span>Total</span>
                <span className="font-mono tabular-nums">{formatPeso(receiptData.total)}</span>
              </div>
            </div>
            <div className="border-t border-dashed border-gray-300 pt-3 space-y-1 text-sm">
              {receiptData.payments.map((p, i) => (
                <div key={i} className="flex justify-between">
                  <span className="text-gray-500">{p.method}</span>
                  <span className="font-mono tabular-nums">{formatPeso(p.amount)}</span>
                </div>
              ))}
              {receiptData.change > 0 && (
                <div className="flex justify-between font-bold text-emerald-600">
                  <span>Change</span>
                  <span className="font-mono tabular-nums">{formatPeso(receiptData.change)}</span>
                </div>
              )}
            </div>
            <p className="text-center text-xs text-gray-400 pt-2">Thank you for your patronage!</p>
          </div>

          {/* Action buttons (hidden when printing) */}
          <div className="flex gap-2 p-4 border-t border-gray-200 bg-gray-50 print:hidden">
            <button
              onClick={() => window.print()}
              className="flex-1 min-h-[44px] rounded-xl border border-gray-300 text-gray-700 font-semibold text-sm hover:bg-gray-100 transition-colors duration-150 active:scale-[0.97]"
            >
              Print Receipt
            </button>
            <button
              onClick={() => {
                setReceiptData(null)
                resetPage()
              }}
              className="flex-1 min-h-[56px] rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-bold text-sm transition-colors duration-150 active:scale-[0.97]"
            >
              New Transaction
            </button>
          </div>
        </div>
      </div>
    )}

    {/* ── Cash-out alert modal ─────────────────────────────────────────────── */}
    {cashOutTip && (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4">
        <div className="w-full max-w-sm bg-gray-900 border-2 border-yellow-500/60 rounded-2xl p-6 space-y-4 text-center">
          <div className="flex items-center justify-center w-14 h-14 rounded-full bg-yellow-500/15 mx-auto">
            <Banknote className="h-7 w-7 text-yellow-400" />
          </div>
          <div>
            <h2 className="text-lg font-bold text-white">Cash Out Tip</h2>
            <p className="text-sm text-gray-400 mt-1">
              Customer paid via non-cash. Give the tip amount from the cash register to the employee(s).
            </p>
          </div>
          <div className="rounded-xl bg-yellow-500/10 border border-yellow-500/30 py-4">
            <p className="text-xs text-yellow-500 uppercase tracking-wider mb-1">Amount to give</p>
            <p className="text-4xl font-mono tabular-nums font-bold text-yellow-400">{formatPeso(cashOutTip.amount)}</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => {
                setCashOutTip(null)
                router.push(`/transactions/${cashOutTip.transactionId}`)
              }}
              className="flex-1 min-h-[44px] rounded-xl border border-gray-700 text-gray-300 font-semibold text-sm hover:bg-gray-800 transition-colors duration-150 active:scale-[0.97]"
            >
              View Transaction
            </button>
            <button
              onClick={() => {
                setCashOutTip(null)
                resetPage()
              }}
              className="flex-1 min-h-[56px] rounded-xl bg-yellow-500 hover:bg-yellow-600 text-black font-bold text-base transition-colors duration-150 active:scale-[0.97]"
            >
              Done — Tip Given
            </button>
          </div>
        </div>
      </div>
    )}
    </>
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
