'use client'

import {
  useMutation,
  useQuery,
  useQueryClient,
  type UseMutationResult,
  type UseQueryResult,
} from '@tanstack/react-query'
import type {
  ConnectAvailabilityDto,
  ConnectCarwashDetailDto,
  ConnectServicesWithPricingDto,
} from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { profileKeys } from '@/hooks/use-profile'
import { discoveryKeys } from '@/hooks/use-discovery'

/** React Query keys for the Connect car-wash detail + join domain. */
export const carwashKeys = {
  all: ['carwash'] as const,
  detail: (tenantId: string) => [...carwashKeys.all, 'detail', tenantId] as const,
  services: (tenantId: string, vehicleId: string) =>
    [...carwashKeys.all, 'services', tenantId, vehicleId] as const,
  availability: (tenantId: string, branchId: string, date: string) =>
    [
      ...carwashKeys.all,
      'availability',
      tenantId,
      branchId,
      date,
    ] as const,
}

/**
 * Fetch the full detail for a single car wash tenant — branches, services,
 * and whether the signed-in user has joined. `enabled` is driven by the
 * presence of a non-empty `tenantId`.
 */
export function useCarwashDetail(
  tenantId: string | null | undefined,
): UseQueryResult<ConnectCarwashDetailDto> {
  return useQuery({
    queryKey: carwashKeys.detail(tenantId ?? ''),
    queryFn: () =>
      apiClient.get<ConnectCarwashDetailDto>(`/carwashes/${tenantId}`),
    enabled: Boolean(tenantId),
  })
}

/**
 * Link the signed-in Connect user to a tenant (creates a Customer row +
 * ConnectUserTenantLink on the backend). Idempotent — calling twice is a
 * no-op for an already-joined tenant. On success invalidates:
 *
 * - `profileKeys.tenants()` so the Home "My car washes" list refreshes.
 * - `carwashKeys.detail(tenantId)` so the Join CTA disappears from the
 *   current detail page.
 * - `discoveryKeys.all` so search results re-fetch with the updated
 *   `isJoined` flag.
 */
export function useJoinCarwash(): UseMutationResult<void, unknown, string> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (tenantId: string) =>
      apiClient.post<void>(`/carwashes/${tenantId}/join`, undefined),
    onSuccess: (_data, tenantId) => {
      qc.invalidateQueries({ queryKey: profileKeys.tenants() })
      qc.invalidateQueries({ queryKey: carwashKeys.detail(tenantId) })
      qc.invalidateQueries({ queryKey: discoveryKeys.all })
    },
  })
}

/**
 * Fetch the tenant's services resolved against a specific vehicle. When the
 * vehicle is already classified at this tenant the response's
 * `priceMode === "exact"` and each service carries a single `price`;
 * otherwise `priceMode === "estimate"` and the services expose a
 * `priceMin`/`priceMax` range derived from the ServicePricing matrix.
 *
 * Disabled until both ids are supplied — the booking wizard only enables
 * it once the user has picked a vehicle on step 1.
 */
export function useTenantServices(
  tenantId: string | null | undefined,
  vehicleId: string | null | undefined,
): UseQueryResult<ConnectServicesWithPricingDto> {
  return useQuery({
    queryKey: carwashKeys.services(tenantId ?? '', vehicleId ?? ''),
    queryFn: () =>
      apiClient.get<ConnectServicesWithPricingDto>(
        `/carwashes/${tenantId}/services?vehicleId=${vehicleId}`,
      ),
    enabled: Boolean(tenantId) && Boolean(vehicleId),
    staleTime: 60_000,
  })
}

/**
 * Fetch the available booking slots for a branch on a specific
 * Manila-local date. The backend already filters out full slots and those
 * violating the `minLeadTimeMinutes` gate, so the UI can render every
 * returned slot as tappable.
 *
 * `date` must be a `YYYY-MM-DD` string in Asia/Manila. Disabled until all
 * three args are set.
 */
export function useAvailability(
  tenantId: string | null | undefined,
  branchId: string | null | undefined,
  date: string | null | undefined,
): UseQueryResult<readonly ConnectAvailabilityDto[]> {
  return useQuery({
    queryKey: carwashKeys.availability(
      tenantId ?? '',
      branchId ?? '',
      date ?? '',
    ),
    queryFn: () =>
      apiClient.get<readonly ConnectAvailabilityDto[]>(
        `/carwashes/${tenantId}/slots?branchId=${branchId}&date=${date}`,
      ),
    enabled: Boolean(tenantId) && Boolean(branchId) && Boolean(date),
    // Slot capacity changes every time someone books — keep the data fresh.
    staleTime: 15_000,
  })
}
