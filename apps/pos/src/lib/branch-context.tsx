'use client'

import { createContext, useContext, useEffect, useState } from 'react'
import { useAuth } from '@clerk/nextjs'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import type { Branch } from '@splashsphere/types'
import type { PagedResult } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

const BRANCH_KEY = 'pos-branch-id'

interface BranchContextValue {
  branchId: string
  branchName: string
  branches: Branch[]
  setBranchId: (id: string) => void
}

const BranchContext = createContext<BranchContextValue>({
  branchId: '',
  branchName: '',
  branches: [],
  setBranchId: () => {},
})

export function useBranch() {
  return useContext(BranchContext)
}

export function BranchProvider({ children }: { children: React.ReactNode }) {
  const { getToken } = useAuth()
  const queryClient = useQueryClient()
  const [branchId, setBranchIdState] = useState<string>('')

  // Restore from localStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem(BRANCH_KEY)
    if (saved) setBranchIdState(saved)
  }, [])

  const { data: branches = [] } = useQuery({
    queryKey: ['branches'],
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<Branch>>('/branches', token ?? undefined)
      return res.items as Branch[]
    },
  })

  // Auto-select the only branch when there is just one
  useEffect(() => {
    if (branches.length === 1 && !branchId) {
      const id = branches[0].id
      setBranchIdState(id)
      localStorage.setItem(BRANCH_KEY, id)
    }
  }, [branches, branchId])

  const setBranchId = (id: string) => {
    setBranchIdState(id)
    localStorage.setItem(BRANCH_KEY, id)
    // Invalidate all branch-scoped queries so pages refresh
    void queryClient.invalidateQueries({ queryKey: ['queue'] })
    void queryClient.invalidateQueries({ queryKey: ['transactions'] })
    void queryClient.invalidateQueries({ queryKey: ['daily-summary'] })
    void queryClient.invalidateQueries({ queryKey: ['queue-stats'] })
    void queryClient.invalidateQueries({ queryKey: ['transactions-recent'] })
    void queryClient.invalidateQueries({ queryKey: ['attendance'] })
  }

  const branchName = branches.find(b => b.id === branchId)?.name ?? ''

  return (
    <BranchContext.Provider value={{ branchId, branchName, branches, setBranchId }}>
      {children}
    </BranchContext.Provider>
  )
}
