'use client'

import { useAuth } from '@clerk/nextjs'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { PagedResult, PackageSummary, PackageDetail } from '@splashsphere/types'
import type { PricingRow } from '@/components/pricing-matrix-editor'

export const packageKeys = {
  all:    ['packages'] as const,
  list:   (p: PackageListParams) => ['packages', 'list', p] as const,
  detail: (id: string)           => ['packages', id] as const,
}

export interface PackageListParams {
  search?: string
  page?: number
  pageSize?: number
}

export interface PackageFormValues {
  name: string
  description?: string
  serviceIds: string[]
}

export interface PackageCommissionRowPayload {
  vehicleTypeId: string
  sizeId: string
  percentageRate: number
}

// ── Queries ───────────────────────────────────────────────────────────────────

export function usePackages(params: PackageListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: packageKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.search)   qs.set('search', params.search)
      if (params.page)     qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      const query = qs.toString()
      return apiClient.get<PagedResult<PackageSummary>>(
        `/packages${query ? `?${query}` : ''}`,
        token ?? undefined
      )
    },
  })
}

export function usePackage(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: packageKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PackageDetail>(`/packages/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

// ── Mutations ─────────────────────────────────────────────────────────────────

export function useCreatePackage() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: PackageFormValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/packages', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.all }),
  })
}

export function useUpdatePackage(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: PackageFormValues) => {
      const token = await getToken()
      return apiClient.put<void>(`/packages/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: packageKeys.detail(id) })
      qc.invalidateQueries({ queryKey: packageKeys.all })
    },
  })
}

export function useTogglePackageStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/packages/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: (_v, id) => {
      qc.invalidateQueries({ queryKey: packageKeys.detail(id) })
      qc.invalidateQueries({ queryKey: packageKeys.all })
    },
  })
}

export function useUpsertPackagePricing(packageId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (rows: PricingRow[]) => {
      const token = await getToken()
      return apiClient.put<void>(
        `/packages/${packageId}/pricing`,
        { rows },
        token ?? undefined
      )
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.detail(packageId) }),
  })
}

export function useUpsertPackageCommissions(packageId: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (rows: PackageCommissionRowPayload[]) => {
      const token = await getToken()
      return apiClient.put<void>(
        `/packages/${packageId}/commissions`,
        { rows },
        token ?? undefined
      )
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: packageKeys.detail(packageId) }),
  })
}
