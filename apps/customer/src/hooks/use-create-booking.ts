'use client'

import {
  useMutation,
  useQueryClient,
  type UseMutationResult,
} from '@tanstack/react-query'
import type {
  ConnectBookingCreatedDto,
  CreateBookingRequest,
} from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { bookingKeys } from '@/hooks/use-bookings'
import { carwashKeys } from '@/hooks/use-carwash'

/**
 * Create a booking for the authenticated Connect user. On success we:
 *
 *  1. Seed the returned detail into the booking-detail cache so the
 *     redirect to `/bookings/{id}` renders instantly without a refetch.
 *  2. Invalidate the bookings lists so the next time "My bookings"
 *     opens it reflects the new entry.
 *  3. Invalidate availability for the just-booked tenant so the capacity
 *     count drops by one if the user goes back and books another slot.
 *
 * Errors surface as `ProblemDetails` objects with `title`/`status`; the
 * wizard page normalises them into an inline banner.
 */
export function useCreateBooking(): UseMutationResult<
  ConnectBookingCreatedDto,
  unknown,
  CreateBookingRequest
> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body) =>
      apiClient.post<ConnectBookingCreatedDto>('/bookings', body),
    onSuccess: (created) => {
      qc.setQueryData(bookingKeys.detail(created.id), created)
      qc.invalidateQueries({ queryKey: bookingKeys.lists() })
      qc.invalidateQueries({
        queryKey: [...carwashKeys.all, 'availability', created.tenantId],
      })
    },
  })
}
