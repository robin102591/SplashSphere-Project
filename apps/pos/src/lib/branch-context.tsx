'use client'

import { createContext, useContext, useEffect, useState } from 'react'
import { useAuth } from '@clerk/nextjs'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import type { Branch, PosStation } from '@splashsphere/types'
import type { PagedResult } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

const BRANCH_KEY = 'pos-branch-id'
const STATION_KEY = 'pos-station-id'

interface BranchContextValue {
  branchId: string
  branchName: string
  branches: Branch[]
  setBranchId: (id: string) => void

  /** Currently selected POS station — drives customer-display routing. */
  stationId: string
  stationName: string
  stations: PosStation[]
  setStationId: (id: string) => void
}

const BranchContext = createContext<BranchContextValue>({
  branchId: '',
  branchName: '',
  branches: [],
  setBranchId: () => {},
  stationId: '',
  stationName: '',
  stations: [],
  setStationId: () => {},
})

export function useBranch() {
  return useContext(BranchContext)
}

export function BranchProvider({ children }: { children: React.ReactNode }) {
  const { getToken } = useAuth()
  const queryClient = useQueryClient()
  const [branchId, setBranchIdState] = useState<string>('')
  const [stationId, setStationIdState] = useState<string>('')

  // Restore from localStorage on mount
  useEffect(() => {
    const savedBranch = localStorage.getItem(BRANCH_KEY)
    if (savedBranch) setBranchIdState(savedBranch)
    const savedStation = localStorage.getItem(STATION_KEY)
    if (savedStation) setStationIdState(savedStation)
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

  // Stations for the current branch (only fetched when a branch is selected).
  const { data: stations = [] } = useQuery({
    queryKey: ['pos-stations', branchId],
    enabled: !!branchId,
    staleTime: 5 * 60_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PosStation[]>(
        `/branches/${branchId}/stations`,
        token ?? undefined,
      )
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

  // When stations load, clear stale selections that don't belong to this
  // branch and auto-pick if there's exactly one station.
  useEffect(() => {
    if (!branchId) return
    const valid = stations.find((s) => s.id === stationId && s.isActive)
    if (!valid) {
      const fallback = stations.find((s) => s.isActive) ?? null
      const next = stations.length === 1 && fallback ? fallback.id : ''
      if (next !== stationId) {
        setStationIdState(next)
        if (next) localStorage.setItem(STATION_KEY, next)
        else localStorage.removeItem(STATION_KEY)
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [stations, branchId])

  const setBranchId = (id: string) => {
    setBranchIdState(id)
    localStorage.setItem(BRANCH_KEY, id)
    // Clear station — the new branch's stations may not include the old ID.
    setStationIdState('')
    localStorage.removeItem(STATION_KEY)
    // Invalidate all branch-scoped queries so pages refresh
    void queryClient.invalidateQueries({ queryKey: ['queue'] })
    void queryClient.invalidateQueries({ queryKey: ['transactions'] })
    void queryClient.invalidateQueries({ queryKey: ['daily-summary'] })
    void queryClient.invalidateQueries({ queryKey: ['queue-stats'] })
    void queryClient.invalidateQueries({ queryKey: ['transactions-recent'] })
    void queryClient.invalidateQueries({ queryKey: ['attendance'] })
  }

  const setStationId = (id: string) => {
    setStationIdState(id)
    if (id) localStorage.setItem(STATION_KEY, id)
    else localStorage.removeItem(STATION_KEY)
  }

  const branchName = branches.find(b => b.id === branchId)?.name ?? ''
  const stationName = stations.find(s => s.id === stationId)?.name ?? ''

  return (
    <BranchContext.Provider value={{
      branchId, branchName, branches, setBranchId,
      stationId, stationName, stations, setStationId,
    }}>
      {children}
    </BranchContext.Provider>
  )
}
