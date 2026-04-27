'use client'

import {
  useMutation,
  useQuery,
  useQueryClient,
  type UseMutationResult,
  type UseQueryResult,
} from '@tanstack/react-query'
import type {
  ConnectBookingDetailDto,
  ConnectBookingListItemDto,
} from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

/**
 * React Query keys for the Connect bookings domain. Colocated with the
 * hooks so mutation callbacks and tests agree on identity.
 */
export const bookingKeys = {
  all: ['bookings'] as const,
  lists: () => [...bookingKeys.all, 'list'] as const,
  /** Keyed on the `includePast` flag so upcoming/past caches stay separate. */
  list: (includePast: boolean) =>
    [...bookingKeys.lists(), { includePast }] as const,
  details: () => [...bookingKeys.all, 'detail'] as const,
  detail: (id: string) => [...bookingKeys.details(), id] as const,
}

/**
 * Fetch the authenticated user's bookings.
 *
 * The backend accepts `?includePast=true`, which returns ALL bookings
 * (upcoming + past) ordered newest-first. When `includePast` is false
 * (the default), the backend filters to upcoming statuses
 * (`Confirmed|Arrived|InService`) with `slotStart >= now`.
 *
 * For the "Past" tab we request `includePast=true` and filter client-side
 * to terminal states (`Completed|Cancelled|NoShow`) OR any slot in the
 * past — see `page.tsx`.
 */
export function useBookings(
  includePast: boolean,
): UseQueryResult<readonly ConnectBookingListItemDto[]> {
  return useQuery({
    queryKey: bookingKeys.list(includePast),
    queryFn: () =>
      apiClient.get<readonly ConnectBookingListItemDto[]>(
        `/bookings${includePast ? '?includePast=true' : ''}`,
      ),
    staleTime: 30_000,
  })
}

/** Fetch a single booking's full detail. */
export function useBooking(
  id: string | undefined,
): UseQueryResult<ConnectBookingDetailDto> {
  return useQuery({
    queryKey: bookingKeys.detail(id ?? ''),
    queryFn: () =>
      apiClient.get<ConnectBookingDetailDto>(`/bookings/${id}`),
    enabled: Boolean(id),
    staleTime: 30_000,
  })
}

/**
 * Cancel a booking. The backend endpoint is PATCH, returning 204 No
 * Content on success. On success we invalidate every booking list/detail
 * query so the UI refetches with the new status.
 */
export function useCancelBooking(): UseMutationResult<
  void,
  unknown,
  { id: string; reason?: string }
> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, reason }) =>
      apiClient.patch<void>(
        `/bookings/${id}/cancel`,
        reason ? { reason } : {},
      ),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: bookingKeys.all })
    },
  })
}
