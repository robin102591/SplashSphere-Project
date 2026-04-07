'use client'

import { QueryClientProvider } from '@tanstack/react-query'
import { SerwistProvider } from '@serwist/next/react'
import { queryClient } from '@/lib/query-client'

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <SerwistProvider
      swUrl="/sw.js"
      disable={process.env.NODE_ENV === 'development'}
      reloadOnOnline
    >
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    </SerwistProvider>
  )
}
