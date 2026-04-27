'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  EquipmentDto,
  EquipmentDetailDto,
  EquipmentMaintenanceReportDto,
  PagedResult,
} from '@splashsphere/types'

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

// ── Reports ──────────────────────────────────────────────────────────────────

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
