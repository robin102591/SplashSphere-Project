'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Customer, CustomerDetail, PagedResult } from '@splashsphere/types'

export const customerKeys = {
  all: ['customers'] as const,
  list: (params: CustomerListParams) => ['customers', 'list', params] as const,
  detail: (id: string) => ['customers', id] as const,
}

export interface CustomerListParams {
  search?: string
  page?: number
  pageSize?: number
}

export interface CustomerFormValues {
  firstName: string
  lastName: string
  email?: string
  contactNumber?: string
  notes?: string
}

export function useCustomers(params: CustomerListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: customerKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.search) qs.set('search', params.search)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<Customer>>(`/customers?${qs.toString()}`, token ?? undefined)
    },
  })
}

export function useCustomer(id: string) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: customerKeys.detail(id),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CustomerDetail>(`/customers/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

export function useCreateCustomer() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: CustomerFormValues) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/customers', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: customerKeys.all }),
  })
}

export function useUpdateCustomer(id: string) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: CustomerFormValues) => {
      const token = await getToken()
      return apiClient.put<void>(`/customers/${id}`, data, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: customerKeys.detail(id) })
      qc.invalidateQueries({ queryKey: customerKeys.all })
    },
  })
}

export function useToggleCustomerStatus() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/customers/${id}/status`, {}, token ?? undefined)
    },
    onSuccess: (_data, id) => {
      qc.invalidateQueries({ queryKey: customerKeys.detail(id) })
      qc.invalidateQueries({ queryKey: customerKeys.all })
    },
  })
}
