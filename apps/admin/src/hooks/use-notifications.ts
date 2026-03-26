'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import type { PagedResult, NotificationDto, UnreadCountDto } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

export const notificationKeys = {
  all: ['notifications'] as const,
  list: (params: { page?: number; unreadOnly?: boolean }) =>
    ['notifications', 'list', params] as const,
  unreadCount: ['notifications', 'unread-count'] as const,
}

export function useNotifications(params: { page?: number; pageSize?: number; unreadOnly?: boolean } = {}) {
  const { getToken } = useAuth()
  const { page = 1, pageSize = 10, unreadOnly = false } = params

  return useQuery({
    queryKey: notificationKeys.list({ page, unreadOnly }),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
        unreadOnly: String(unreadOnly),
      })
      return apiClient.get<PagedResult<NotificationDto>>(
        `/notifications?${qs}`,
        token ?? undefined,
      )
    },
  })
}

export function useUnreadCount() {
  const { getToken } = useAuth()

  return useQuery({
    queryKey: notificationKeys.unreadCount,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<UnreadCountDto>(
        '/notifications/unread-count',
        token ?? undefined,
      )
    },
    refetchInterval: 60_000,
  })
}

export function useMarkRead() {
  const { getToken } = useAuth()
  const qc = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      const token = await getToken()
      return apiClient.patch<void>(`/notifications/${id}/read`, {}, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: notificationKeys.all })
    },
  })
}

export function useMarkAllRead() {
  const { getToken } = useAuth()
  const qc = useQueryClient()

  return useMutation({
    mutationFn: async () => {
      const token = await getToken()
      return apiClient.post<void>('/notifications/mark-all-read', {}, token ?? undefined)
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: notificationKeys.all })
    },
  })
}
