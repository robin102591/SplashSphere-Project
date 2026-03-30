'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { AuditLogDto, PagedResult } from '@splashsphere/types'

export interface AuditLogParams {
  entityType?: string
  entityId?: string
  userId?: string
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export function useAuditLogs(params: AuditLogParams = {}) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: ['audit-logs', params],
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      if (params.entityType) qs.set('entityType', params.entityType)
      if (params.entityId) qs.set('entityId', params.entityId)
      if (params.userId) qs.set('userId', params.userId)
      if (params.from) qs.set('from', params.from)
      if (params.to) qs.set('to', params.to)
      if (params.page) qs.set('page', String(params.page))
      if (params.pageSize) qs.set('pageSize', String(params.pageSize))
      return apiClient.get<PagedResult<AuditLogDto>>(`/audit-logs?${qs}`, token ?? undefined)
    },
  })
}
