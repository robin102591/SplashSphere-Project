'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { Droplets, MonitorPlay, ArrowRight } from 'lucide-react'
import type { Branch, PagedResult, PosStation } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

/**
 * Customer Display setup screen. Branch + station picker, then a "Start
 * Display" button that requests fullscreen mode and pushes to /display/live.
 *
 * The picker remembers its last choice in localStorage so the device can
 * survive a page refresh without re-prompting.
 */
export default function DisplaySetupPage() {
  const router = useRouter()
  const { getToken } = useAuth()

  const [branchId, setBranchId] = useState<string>('')
  const [stationId, setStationId] = useState<string>('')

  // Restore from localStorage on mount.
  useEffect(() => {
    const savedBranch = localStorage.getItem('display-branch-id') ?? ''
    const savedStation = localStorage.getItem('display-station-id') ?? ''
    if (savedBranch) setBranchId(savedBranch)
    if (savedStation) setStationId(savedStation)
  }, [])

  const { data: branches = [], isLoading: branchesLoading } = useQuery({
    queryKey: ['display-branches'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<Branch>>('/branches', token ?? undefined)
      return res.items as Branch[]
    },
  })

  const { data: stations = [], isLoading: stationsLoading } = useQuery({
    queryKey: ['display-stations', branchId],
    enabled: !!branchId,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PosStation[]>(`/branches/${branchId}/stations`, token ?? undefined)
    },
  })

  // Auto-pick the first station whenever the branch changes (and clear stale
  // values that no longer match the current branch).
  useEffect(() => {
    if (!branchId) return
    const valid = stations.find((s) => s.id === stationId)
    if (!valid && stations.length > 0) {
      setStationId(stations[0].id)
    } else if (stations.length === 0) {
      setStationId('')
    }
  }, [branchId, stations, stationId])

  const canStart = !!branchId && !!stationId

  const handleStart = async () => {
    if (!canStart) return

    localStorage.setItem('display-branch-id', branchId)
    localStorage.setItem('display-station-id', stationId)

    // Request fullscreen on the user gesture. If the browser blocks it
    // (some PWA contexts do), we still navigate — the operator can press F11.
    try {
      if (document.documentElement.requestFullscreen) {
        await document.documentElement.requestFullscreen()
      }
    } catch {
      // Best-effort — fullscreen denied isn't fatal.
    }

    router.push(`/display/live?branchId=${branchId}&stationId=${stationId}`)
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-950 to-slate-900 flex items-center justify-center p-6">
      <div className="w-full max-w-md space-y-6">

        {/* ── Brand header ───────────────────────────────────────────────── */}
        <div className="text-center space-y-3">
          <div className="inline-flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-500 shadow-lg shadow-blue-900/40">
            <Droplets className="h-9 w-9 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-white tracking-tight">SplashSphere</h1>
          <p className="text-sm text-slate-400 inline-flex items-center gap-1.5">
            <MonitorPlay className="h-4 w-4" /> Customer Display Setup
          </p>
        </div>

        {/* ── Setup form ──────────────────────────────────────────────────── */}
        <div className="rounded-2xl bg-slate-800/50 border border-slate-700/50 p-6 space-y-5 backdrop-blur">

          <div className="space-y-1.5">
            <label htmlFor="branch" className="text-sm font-medium text-slate-300">
              Branch <span className="text-red-400">*</span>
            </label>
            <select
              id="branch"
              value={branchId}
              onChange={(e) => setBranchId(e.target.value)}
              disabled={branchesLoading}
              className="w-full rounded-md bg-slate-900 border border-slate-700 px-3 py-2.5 text-sm text-white outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 disabled:opacity-60"
            >
              <option value="">{branchesLoading ? 'Loading branches…' : 'Select a branch'}</option>
              {branches.map((b) => (
                <option key={b.id} value={b.id}>{b.name}</option>
              ))}
            </select>
          </div>

          <div className="space-y-1.5">
            <label htmlFor="station" className="text-sm font-medium text-slate-300">
              POS Station <span className="text-red-400">*</span>
            </label>
            <select
              id="station"
              value={stationId}
              onChange={(e) => setStationId(e.target.value)}
              disabled={!branchId || stationsLoading}
              className="w-full rounded-md bg-slate-900 border border-slate-700 px-3 py-2.5 text-sm text-white outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 disabled:opacity-60"
            >
              <option value="">
                {!branchId ? 'Select a branch first' : stationsLoading ? 'Loading…' : stations.length === 0 ? 'No stations — create one in Admin → Settings' : 'Select a station'}
              </option>
              {stations.map((s) => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </select>
            {branchId && !stationsLoading && stations.length === 0 && (
              <p className="text-xs text-amber-300">
                This branch has no POS stations yet. Create one in the admin app under Settings → POS Stations.
              </p>
            )}
          </div>

          <button
            type="button"
            onClick={handleStart}
            disabled={!canStart}
            className="w-full rounded-md bg-blue-500 hover:bg-blue-400 disabled:bg-slate-700 disabled:text-slate-500 disabled:cursor-not-allowed text-white font-semibold py-3 px-4 text-sm transition-colors flex items-center justify-center gap-2"
          >
            Start Display
            <ArrowRight className="h-4 w-4" />
          </button>

          <p className="text-xs text-slate-500 text-center pt-1">
            The display will open fullscreen. Press <kbd className="px-1.5 py-0.5 rounded bg-slate-700 text-slate-300 font-mono text-[10px]">F11</kbd> if your browser blocks the auto-fullscreen.
          </p>
        </div>
      </div>
    </div>
  )
}
