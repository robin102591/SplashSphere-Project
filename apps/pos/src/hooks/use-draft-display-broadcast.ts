'use client'

import { useEffect, useMemo, useRef } from 'react'
import { useBranch } from '@/lib/branch-context'
import { useSignalRInvoke } from '@/lib/signalr-context'

/** Mirrors `DisplayTransactionResultDto` on the backend. */
export interface DraftDisplayPayload {
  transactionId: string
  vehiclePlate: string | null
  vehicleMakeModel: string | null
  vehicleTypeSize: string | null
  customerName: string | null
  loyaltyTier: string | null
  items: readonly DraftDisplayLineItem[]
  subtotal: number
  discountAmount: number
  discountLabel: string | null
  taxAmount: number
  total: number
}

export interface DraftDisplayLineItem {
  id: string
  name: string
  type: 'service' | 'package' | 'merchandise'
  quantity: number
  unitPrice: number
  totalPrice: number
}

const DEBOUNCE_MS = 150

/**
 * Pushes the cashier's in-progress cart on /transactions/new to the paired
 * customer display via the SignalR `BroadcastDraftDisplay` hub method, so
 * customers see items build up live before any DB row exists.
 *
 * Behavior:
 * - Silent no-op when no station is selected (customer-display is disabled
 *   for this cashier session). Cashier can still process transactions.
 * - Debounced ~150ms so a flurry of cart edits collapses into one broadcast.
 * - The `enabled` flag lets the caller suppress broadcasting after the
 *   transaction has been POSTed (the real SignalR pipeline takes over from
 *   that point — keeping draft broadcasts running would race with the
 *   server-built payload).
 */
export function useDraftDisplayBroadcast({
  payload,
  enabled = true,
}: {
  payload: DraftDisplayPayload | null
  enabled?: boolean
}) {
  const invoke = useSignalRInvoke()
  const { branchId, stationId } = useBranch()

  // Skip the very first effect run after enabled flips false — we don't want
  // to push a stale "draft Idle" right after the cashier hits Complete.
  const lastPayloadRef = useRef<string | null>(null)

  // Stable JSON repr so we don't broadcast when references shuffle but
  // values haven't changed.
  const serialized = useMemo(
    () => (payload ? JSON.stringify(payload) : null),
    [payload],
  )

  useEffect(() => {
    if (!enabled) return
    if (!branchId || !stationId) return
    if (!payload || !serialized) return

    // Deduplicate equivalent payloads.
    if (serialized === lastPayloadRef.current) return

    const id = setTimeout(() => {
      lastPayloadRef.current = serialized
      invoke('BroadcastDraftDisplay', branchId, stationId, payload)
    }, DEBOUNCE_MS)

    return () => clearTimeout(id)
  }, [enabled, branchId, stationId, serialized, payload, invoke])
}
