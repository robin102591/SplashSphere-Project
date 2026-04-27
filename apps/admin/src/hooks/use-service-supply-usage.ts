'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  ServiceSupplyUsageDto,
  ServiceCostBreakdownDto,
  InventorySummaryDto,
} from '@splashsphere/types'

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

// ── Cross-resource inventory summary ─────────────────────────────────────────

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
