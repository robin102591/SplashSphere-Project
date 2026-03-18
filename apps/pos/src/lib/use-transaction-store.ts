/**
 * Zustand store for the POS transaction screen.
 * Holds cart state (services, packages, merchandise, vehicle, payments).
 * All API calls are handled separately via TanStack Query.
 */
import { create } from 'zustand'
import { PaymentMethod } from '@splashsphere/types'

// ── Counter for stable local React keys ───────────────────────────────────────

let _seq = 0
const uid = () => `li-${++_seq}`

// ── Line item shapes ──────────────────────────────────────────────────────────

export interface ServiceLineItem {
  localId: string
  serviceId: string
  serviceName: string
  categoryName: string
  /** Base price from catalog — used as fallback when no pricing matrix row exists */
  basePrice: number
  unitPrice: number
  employeeIds: string[]
}

export interface PackageLineItem {
  localId: string
  packageId: string
  packageName: string
  /** 0 means price not yet resolved; resolved from pricing matrix by vehicleType+size */
  unitPrice: number
  employeeIds: string[]
}

export interface MerchandiseLine {
  localId: string
  merchandiseId: string
  merchandiseName: string
  unitPrice: number
  quantity: number
}

export interface PaymentEntry {
  localId: string
  method: PaymentMethod
  amount: number
  reference: string
}

// ── State + actions ───────────────────────────────────────────────────────────

interface TxState {
  branchId: string
  // Vehicle
  plateNumber: string
  carId: string | null
  customerId: string | null
  vehicleTypeId: string
  sizeId: string
  vehicleTypeName: string
  sizeName: string
  // Queue link
  queueEntryId: string | null
  // Cart
  services: ServiceLineItem[]
  packages: PackageLineItem[]
  merchandise: MerchandiseLine[]
  // Order meta
  discountAmount: number
  tipAmount: number
  notes: string
  // Payments
  payments: PaymentEntry[]
}

interface TxActions {
  setBranch(id: string): void
  setQueueEntry(id: string | null): void
  setVehicle(v: {
    plateNumber: string
    carId: string | null
    customerId: string | null
    vehicleTypeId: string
    sizeId: string
    vehicleTypeName: string
    sizeName: string
  }): void
  setVehicleType(id: string, name: string): void
  setVehicleSize(id: string, name: string): void
  // Services
  addService(s: Omit<ServiceLineItem, 'localId' | 'employeeIds'>): void
  updateServicePrice(localId: string, price: number): void
  removeService(localId: string): void
  toggleServiceEmployee(localId: string, empId: string): void
  // Packages
  addPackage(p: Omit<PackageLineItem, 'localId' | 'employeeIds'>): void
  removePackage(localId: string): void
  togglePackageEmployee(localId: string, empId: string): void
  updatePackagePrice(localId: string, price: number): void
  // Merchandise
  addMerchandise(m: Omit<MerchandiseLine, 'localId'>): void
  updateMerchandiseQty(localId: string, qty: number): void
  removeMerchandise(localId: string): void
  // Payments
  addPayment(p: Omit<PaymentEntry, 'localId'>): void
  removePayment(localId: string): void
  // Meta
  setDiscount(n: number): void
  setTip(n: number): void
  setNotes(s: string): void
  reset(): void
}

// ── Initial state ─────────────────────────────────────────────────────────────

const INIT: TxState = {
  branchId: '',
  plateNumber: '',
  carId: null,
  customerId: null,
  vehicleTypeId: '',
  sizeId: '',
  vehicleTypeName: '',
  sizeName: '',
  queueEntryId: null,
  services: [],
  packages: [],
  merchandise: [],
  discountAmount: 0,
  tipAmount: 0,
  notes: '',
  payments: [],
}

// ── Store ─────────────────────────────────────────────────────────────────────

export const useTransactionStore = create<TxState & TxActions>((set) => ({
  ...INIT,

  setBranch: (id) => set({ branchId: id }),
  setQueueEntry: (id) => set({ queueEntryId: id }),
  setVehicle: (v) => set(v),
  setVehicleType: (id, name) => set({ vehicleTypeId: id, vehicleTypeName: name }),
  setVehicleSize: (id, name) => set({ sizeId: id, sizeName: name }),

  addService: (s) =>
    set((st) => ({
      services: [...st.services, { ...s, localId: uid(), employeeIds: [] }],
    })),
  removeService: (localId) =>
    set((st) => ({ services: st.services.filter((s) => s.localId !== localId) })),
  updateServicePrice: (localId, price) =>
    set((st) => ({
      services: st.services.map((s) =>
        s.localId === localId ? { ...s, unitPrice: price } : s
      ),
    })),
  toggleServiceEmployee: (localId, empId) =>
    set((st) => ({
      services: st.services.map((s) =>
        s.localId !== localId
          ? s
          : {
              ...s,
              employeeIds: s.employeeIds.includes(empId)
                ? s.employeeIds.filter((id) => id !== empId)
                : [...s.employeeIds, empId],
            }
      ),
    })),

  addPackage: (p) =>
    set((st) => ({
      packages: [...st.packages, { ...p, localId: uid(), employeeIds: [] }],
    })),
  removePackage: (localId) =>
    set((st) => ({ packages: st.packages.filter((p) => p.localId !== localId) })),
  togglePackageEmployee: (localId, empId) =>
    set((st) => ({
      packages: st.packages.map((p) =>
        p.localId !== localId
          ? p
          : {
              ...p,
              employeeIds: p.employeeIds.includes(empId)
                ? p.employeeIds.filter((id) => id !== empId)
                : [...p.employeeIds, empId],
            }
      ),
    })),
  updatePackagePrice: (localId, price) =>
    set((st) => ({
      packages: st.packages.map((p) =>
        p.localId === localId ? { ...p, unitPrice: price } : p
      ),
    })),

  addMerchandise: (m) =>
    set((st) => ({
      merchandise: [...st.merchandise, { ...m, localId: uid() }],
    })),
  updateMerchandiseQty: (localId, qty) =>
    set((st) => ({
      merchandise: st.merchandise.map((m) =>
        m.localId === localId ? { ...m, quantity: Math.max(1, qty) } : m
      ),
    })),
  removeMerchandise: (localId) =>
    set((st) => ({ merchandise: st.merchandise.filter((m) => m.localId !== localId) })),

  addPayment: (p) =>
    set((st) => ({
      payments: [...st.payments, { ...p, localId: uid() }],
    })),
  removePayment: (localId) =>
    set((st) => ({ payments: st.payments.filter((p) => p.localId !== localId) })),

  setDiscount: (n) => set({ discountAmount: n }),
  setTip: (n) => set({ tipAmount: n }),
  setNotes: (s) => set({ notes: s }),
  reset: () => set({ ...INIT }),
}))
