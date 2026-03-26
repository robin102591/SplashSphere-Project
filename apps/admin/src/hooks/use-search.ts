'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { useState, useEffect } from 'react'
import type { GlobalSearchResult } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

function useDebounce<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState(value)
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(timer)
  }, [value, delay])
  return debounced
}

export const searchKeys = {
  all: ['search'] as const,
  global: (q: string) => ['search', 'global', q] as const,
}

export function useGlobalSearch(query: string) {
  const { getToken } = useAuth()
  const debouncedQuery = useDebounce(query.trim(), 250)

  return useQuery({
    queryKey: searchKeys.global(debouncedQuery),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<GlobalSearchResult>(
        `/search?q=${encodeURIComponent(debouncedQuery)}&limit=5`,
        token ?? undefined,
      )
    },
    enabled: debouncedQuery.length >= 2,
    staleTime: 30_000,
    placeholderData: (prev) => prev,
  })
}
