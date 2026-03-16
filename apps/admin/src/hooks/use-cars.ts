'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Car, Make, VehicleModel, PagedResult } from '@splashsphere/types'

export const carKeys = {
  all: ['cars'] as const,
  list: (params: CarListParams) => ['cars', 'list', params] as const,
  detail: (id: string) => ['cars', id] as const,
}

export interface CarListParams {
  customerId?: string
  search?: string
  page?: number
  pageSize?: number
}

export interface CreateCarValues {
  plateNumber: string
  vehicleTypeId: string
  sizeId: string
  customerId?: string
  makeId?: string
  modelId?: string
  color?: string
  year?: number
  notes?: string
}

export interface UpdateCarValues {
  vehicleTypeId: string
  sizeId: string
  makeId?: string
  modelId?: string
  color?: string
  year?: number
  notes?: string
}

export function useCars(params: CarListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: carKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.customerId) qs.set('customerId', params.customerId)
      if (params.search) qs.set('search', params.search)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<Car>>(`/cars?${qs.toString()}`, token ?? undefined)
    },
  })
}

export function useCar(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: carKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Car>(`/cars/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useCreateCar() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateCarValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/cars', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: carKeys.all }),
  })
}

export function useUpdateCar(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: UpdateCarValues) => {
      const token = await getToken()
      return apiClient.put<void>(`/cars/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: carKeys.detail(id) })
      qc.invalidateQueries({ queryKey: carKeys.all })
    },
  })
}

// ── Makes & Models CRUD ────────────────────────────────────────────────────────

export function useCreateMake() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (name: string) => {
      const token = await getToken()
      return apiClient.post<Make>('/makes', { name }, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['makes'] }),
  })
}

export function useToggleMake() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/makes/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['makes'] }),
  })
}

export function useCreateModel() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { makeId: string; name: string }) => {
      const token = await getToken()
      return apiClient.post<VehicleModel>('/models', data, token ?? undefined)
    },
    onSuccess: (_data, vars) => qc.invalidateQueries({ queryKey: ['models', vars.makeId] }),
  })
}

export function useToggleModel() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, makeId }: { id: string; makeId: string }) => {
      const token = await getToken()
      return apiClient.patch<void>(`/models/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: (_data, vars) => qc.invalidateQueries({ queryKey: ['models', vars.makeId] }),
  })
}

// ── Reference data for car forms ──────────────────────────────────────────────

export function useMakes() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['makes'],
    queryFn: async () => {
      const token = await getToken()
      const result = await apiClient.get<PagedResult<Make>>('/makes?pageSize=100', token ?? undefined)
      return result.items
    },
    staleTime: 1000 * 60 * 10,
  })
}

export function useModelsByMake(makeId: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['models', makeId],
    queryFn: async () => {
      const token = await getToken()
      const result = await apiClient.get<PagedResult<VehicleModel>>(
        `/models?makeId=${makeId}&pageSize=100`,
        token ?? undefined
      )
      return result.items
    },
    enabled: !!makeId,
    staleTime: 1000 * 60 * 10,
  })
}
