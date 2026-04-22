'use client'

import { useAuth } from '@clerk/nextjs'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type {
  BookingAdminDetailDto,
  BookingListItemDto,
  BookingSettingDto,
} from '@splashsphere/types'

// ── Query keys ────────────────────────────────────────────────────────────────

export const bookingKeys = {
  all: ['bookings'] as const,
  list: (params: object) => ['bookings', 'list', params] as const,
  detail: (id: string) => ['bookings', id] as const,
  settings: (branchId: string) => ['booking-settings', branchId] as const,
}

// ── Admin list / detail ──────────────────────────────────────────────────────

export interface GetBookingsParams {
  fromDate: string // ISO (yyyy-mm-dd or full)
  toDate: string
  branchId?: string
  /** Optional BookingStatus as stringified number (matches backend query). */
  status?: string
}

export function useBookings(params: GetBookingsParams, enabled = true) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: bookingKeys.list(params),
    queryFn: async () => {
      const token = await getToken()
      const qs = new URLSearchParams()
      qs.set('fromDate', params.fromDate)
      qs.set('toDate', params.toDate)
      if (params.branchId) qs.set('branchId', params.branchId)
      if (params.status)   qs.set('status', params.status)
      return apiClient.get<BookingListItemDto[]>(`/bookings?${qs}`, token ?? undefined)
    },
    enabled,
  })
}

export function useBookingDetail(id: string | null) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: bookingKeys.detail(id ?? ''),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<BookingAdminDetailDto>(`/bookings/${id}`, token ?? undefined)
    },
    enabled: !!id,
  })
}

// ── Booking settings (per-branch) ────────────────────────────────────────────

export function useBookingSettings(branchId: string | undefined) {
  const { getToken } = useAuth()
  return useQuery({
    queryKey: bookingKeys.settings(branchId ?? ''),
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<BookingSettingDto | null>(
        `/booking-settings?branchId=${encodeURIComponent(branchId ?? '')}`,
        token ?? undefined,
      )
    },
    enabled: !!branchId,
    staleTime: 60_000,
  })
}

export interface UpsertBookingSettingBody {
  openTime: string // "HH:mm"
  closeTime: string
  slotIntervalMinutes: number
  maxBookingsPerSlot: number
  advanceBookingDays: number
  minLeadTimeMinutes: number
  noShowGraceMinutes: number
  isBookingEnabled: boolean
  showInPublicDirectory: boolean
}

export function useUpsertBookingSettings(branchId: string | undefined) {
  const { getToken } = useAuth()
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (body: UpsertBookingSettingBody) => {
      const token = await getToken()
      return apiClient.put<BookingSettingDto>(
        `/booking-settings?branchId=${encodeURIComponent(branchId ?? '')}`,
        body,
        token ?? undefined,
      )
    },
    onSuccess: (data) => {
      if (branchId) {
        qc.setQueryData(bookingKeys.settings(branchId), data)
      }
    },
  })
}
