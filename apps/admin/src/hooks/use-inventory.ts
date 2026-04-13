'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  SupplyCategoryDto,
  SupplyItemDto,
  SupplyItemDetailDto,
  StockMovementDto,
  SupplierDto,
  PurchaseOrderDto,
  PurchaseOrderDetailDto,
  EquipmentDto,
  EquipmentDetailDto,
  ServiceSupplyUsageDto,
  ServiceCostBreakdownDto,
  InventorySummaryDto,
  EquipmentMaintenanceReportDto,
  PagedResult,
} from '@splashsphere/types'

// ── Supply Categories ────────────────────────────────────────────────────────

export function useSupplyCategories() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['supply-categories'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<SupplyCategoryDto[]>('/supplies/categories', token ?? undefined)
    },
  })
}

export function useCreateSupplyCategory() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { name: string; description?: string }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/supplies/categories', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['supply-categories'] }),
  })
}

// ── Supply Items ─────────────────────────────────────────────────────────────

export interface SupplyListParams {
  categoryId?: string
  branchId?: string
  stockStatus?: 'low' | 'out' | 'ok'
  page?: number
  pageSize?: number
}

export function useSupplies(params: SupplyListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['supplies', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.categoryId) qs.set('categoryId', params.categoryId)
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.stockStatus) qs.set('stockStatus', params.stockStatus)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<SupplyItemDto>>(`/supplies?${qs}`, token ?? undefined)
    },
  })
}

export function useSupplyById(id: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['supplies', id],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<SupplyItemDetailDto>(`/supplies/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useCreateSupply() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      branchId: string; categoryId?: string; name: string; description?: string;
      unit: string; reorderLevel?: number;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/supplies', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['supplies'] }),
  })
}

export function useUpdateSupply() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; categoryId?: string; name: string; description?: string;
      unit: string; reorderLevel?: number; isActive?: boolean;
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/supplies/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['supplies'] })
    },
  })
}

export function useDeleteSupply() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.delete<void>(`/supplies/${id}`, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['supplies'] }),
  })
}

// ── Stock Movements ──────────────────────────────────────────────────────────

export interface StockMovementListParams {
  supplyItemId?: string
  branchId?: string
  type?: number
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export function useStockMovements(params: StockMovementListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['stock-movements', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.supplyItemId) qs.set('supplyItemId', params.supplyItemId)
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.type != null) qs.set('type', String(params.type))
      if (params.from) qs.set('from', params.from)
      if (params.to) qs.set('to', params.to)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<StockMovementDto>>(`/stock-movements?${qs}`, token ?? undefined)
    },
  })
}

export function useRecordStockMovement() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      supplyItemId: string; type: number; quantity: number;
      unitCost?: number; reference?: string; notes?: string;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/stock-movements', data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['stock-movements'] })
      qc.invalidateQueries({ queryKey: ['supplies'] })
    },
  })
}

export function useRecordBulkUsage() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      transactionId?: string; items: { supplyItemId: string; quantity: number }[];
      notes?: string;
    }) => {
      const token = await getToken()
      return apiClient.post<void>('/stock-movements/bulk-usage', data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['stock-movements'] })
      qc.invalidateQueries({ queryKey: ['supplies'] })
    },
  })
}

// ── Suppliers ────────────────────────────────────────────────────────────────

export function useSuppliers() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['suppliers'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<SupplierDto[]>('/suppliers', token ?? undefined)
    },
  })
}

export function useCreateSupplier() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      name: string; contactPerson?: string; phone?: string;
      email?: string; address?: string;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/suppliers', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['suppliers'] }),
  })
}

export function useUpdateSupplier() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; name: string; contactPerson?: string; phone?: string;
      email?: string; address?: string; isActive?: boolean;
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/suppliers/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['suppliers'] }),
  })
}

// ── Purchase Orders ──────────────────────────────────────────────────────────

export interface PurchaseOrderListParams {
  supplierId?: string
  branchId?: string
  status?: number
  page?: number
  pageSize?: number
}

export function usePurchaseOrders(params: PurchaseOrderListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['purchase-orders', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.supplierId) qs.set('supplierId', params.supplierId)
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.status != null) qs.set('status', String(params.status))
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<PurchaseOrderDto>>(`/purchase-orders?${qs}`, token ?? undefined)
    },
  })
}

export function usePurchaseOrderById(id: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['purchase-orders', id],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PurchaseOrderDetailDto>(`/purchase-orders/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useCreatePurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      supplierId: string; branchId: string; expectedDeliveryDate?: string; notes?: string;
      lines: { supplyItemId?: string; merchandiseId?: string; itemName: string; quantity: number; unitCost: number }[];
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/purchase-orders', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-orders'] }),
  })
}

export function useUpdatePurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; supplierId: string; expectedDeliveryDate?: string; notes?: string;
      lines: { supplyItemId?: string; merchandiseId?: string; itemName: string; quantity: number; unitCost: number }[];
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/purchase-orders/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-orders'] }),
  })
}

export function useSendPurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/purchase-orders/${id}/send`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-orders'] }),
  })
}

export function useReceivePurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; lines: { lineId: string; receivedQuantity: number; unitCost?: number }[];
    }) => {
      const token = await getToken()
      return apiClient.post<void>(`/purchase-orders/${id}/receive`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['purchase-orders'] })
      qc.invalidateQueries({ queryKey: ['supplies'] })
      qc.invalidateQueries({ queryKey: ['stock-movements'] })
    },
  })
}

export function useCancelPurchaseOrder() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/purchase-orders/${id}/cancel`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-orders'] }),
  })
}

// ── Equipment ────────────────────────────────────────────────────────────────

export interface EquipmentListParams {
  branchId?: string
  status?: number
  page?: number
  pageSize?: number
}

export function useEquipment(params: EquipmentListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['equipment', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.status != null) qs.set('status', String(params.status))
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<EquipmentDto>>(`/equipment?${qs}`, token ?? undefined)
    },
  })
}

export function useEquipmentById(id: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['equipment', id],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<EquipmentDetailDto>(`/equipment/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useRegisterEquipment() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      branchId: string; name: string; brand?: string; model?: string;
      serialNumber?: string; location?: string; purchaseDate?: string;
      purchaseCost?: number; warrantyExpiry?: string; notes?: string;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/equipment', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['equipment'] }),
  })
}

export function useUpdateEquipment() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; name: string; brand?: string; model?: string;
      serialNumber?: string; location?: string; purchaseDate?: string;
      purchaseCost?: number; warrantyExpiry?: string; notes?: string;
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/equipment/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['equipment'] }),
  })
}

export function useUpdateEquipmentStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: number }) => {
      const token = await getToken()
      return apiClient.patch<void>(`/equipment/${id}/status`, { status }, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['equipment'] }),
  })
}

export function useLogMaintenance() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ equipmentId, ...data }: {
      equipmentId: string; type: number; description: string; cost?: number;
      performedBy?: string; performedDate: string; nextDueDate?: string;
      nextDueHours?: number; notes?: string;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>(`/equipment/${equipmentId}/maintenance`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['equipment'] }),
  })
}

// ── Service Supply Usage ─────────────────────────────────────────────────────

export function useServiceSupplyUsage(serviceId: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['services', serviceId, 'supply-usage'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ServiceSupplyUsageDto[]>(`/services/${serviceId}/supply-usage`, token ?? undefined)
    },
    enabled: !!serviceId,
  })
}

export function useUpdateServiceSupplyUsage() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ serviceId, ...data }: {
      serviceId: string;
      usages: { supplyItemId: string; sizeUsages: { sizeId?: string; quantityPerUse: number }[] }[];
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/services/${serviceId}/supply-usage`, data, token ?? undefined)
    },
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['services', variables.serviceId, 'supply-usage'] })
    },
  })
}

export function useServiceCostBreakdown(serviceId: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['services', serviceId, 'cost-breakdown'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ServiceCostBreakdownDto>(`/services/${serviceId}/cost-breakdown`, token ?? undefined)
    },
    enabled: !!serviceId,
  })
}

// ── Reports ──────────────────────────────────────────────────────────────────

export function useInventorySummary(branchId?: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'inventory-summary', branchId],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (branchId) qs.set('branchId', branchId)
      return apiClient.get<InventorySummaryDto>(`/reports/inventory-summary?${qs}`, token ?? undefined)
    },
  })
}

export function useEquipmentMaintenanceReport(branchId?: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'equipment-maintenance', branchId],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (branchId) qs.set('branchId', branchId)
      return apiClient.get<EquipmentMaintenanceReportDto>(`/reports/equipment-maintenance?${qs}`, token ?? undefined)
    },
  })
}
