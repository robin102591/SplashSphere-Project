'use client'

import { useEffect, useRef } from 'react'
import { useBranch } from '@/lib/branch-context'
import { useSignalRStatus, useSignalRInvoke } from '@/lib/signalr-context'

/**
 * Joins the branch-scoped SignalR group whenever the selected branch or
 * connection state changes. Must be rendered inside both BranchProvider
 * and SignalRProvider.
 */
export function BranchSignalRSync() {
  const { branchId } = useBranch()
  const connState = useSignalRStatus()
  const invoke = useSignalRInvoke()
  const prevBranchRef = useRef<string>('')

  useEffect(() => {
    if (connState !== 'connected' || !branchId) return

    // Leave the previous branch group when switching branches
    if (prevBranchRef.current && prevBranchRef.current !== branchId) {
      invoke('LeaveBranch', prevBranchRef.current)
    }

    invoke('JoinBranch', branchId)
    prevBranchRef.current = branchId
  }, [branchId, connState, invoke])

  return null
}
