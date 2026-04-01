'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { ExpenseDto, ExpenseCategoryDto, ProfitLossReport, PagedResult } from '@splashsphere/types'

export interface ExpenseListParams {
  branchId?: string
  categoryId?: string
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export function useExpenses(params: ExpenseListParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['expenses', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.categoryId) qs.set('categoryId', params.categoryId)
      if (params.from) qs.set('from', params.from)
      if (params.to) qs.set('to', params.to)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<ExpenseDto>>(`/expenses?${qs}`, token ?? undefined)
    },
  })
}

export function useExpenseCategories() {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['expense-categories'],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<ExpenseCategoryDto[]>('/expense-categories', token ?? undefined)
    },
  })
}

export function useRecordExpense() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      branchId: string; categoryId: string; amount: number; description: string;
      expenseDate: string; vendor?: string; receiptReference?: string;
      frequency?: number; isRecurring?: boolean;
    }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/expenses', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['expenses'] }),
  })
}

export function useCreateExpenseCategory() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (data: { name: string; icon?: string }) => {
      const token = await getToken()
      return apiClient.post<{ id: string }>('/expense-categories', data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['expense-categories'] }),
  })
}

export function useUpdateExpense() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string; categoryId: string; amount: number; description: string;
      expenseDate: string; vendor?: string; receiptReference?: string;
      frequency?: number; isRecurring?: boolean;
    }) => {
      const token = await getToken()
      return apiClient.put<void>(`/expenses/${id}`, data, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['expenses'] }),
  })
}

export function useDeleteExpense() {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.delete<void>(`/expenses/${id}`, token ?? undefined)
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ['expenses'] }),
  })
}

export function useProfitLossReport(params: { from: string; to: string; branchId?: string }) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['reports', 'profit-loss', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({ from: params.from, to: params.to })
      if (params.branchId) qs.set('branchId', params.branchId)
      return apiClient.get<ProfitLossReport>(`/reports/profit-loss?${qs}`, token ?? undefined)
    },
    enabled: !!params.from && !!params.to,
  })
}
