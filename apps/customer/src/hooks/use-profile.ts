'use client'

import {
  useMutation,
  useQuery,
  useQueryClient,
  type UseMutationResult,
  type UseQueryResult,
} from '@tanstack/react-query'
import type {
  ConnectProfileDto,
  ConnectTenantSummaryDto,
  ConnectVehicleDto,
  ConnectVehicleUpsertRequest,
  UpdateConnectProfileRequest,
} from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

/**
 * React Query keys for the Connect profile domain. Centralized so pages and
 * mutation callbacks agree on cache identity without stringly-typed keys.
 */
export const profileKeys = {
  all: ['profile'] as const,
  me: () => [...profileKeys.all, 'me'] as const,
  vehicles: () => [...profileKeys.all, 'vehicles'] as const,
  tenants: () => [...profileKeys.all, 'tenants'] as const,
}

// ── Queries ─────────────────────────────────────────────────────────────────

/**
 * Fetch the authenticated Connect user's full profile (identity + vehicles).
 * The vehicles array is also exposed via {@link useVehicles} — they share the
 * same source so a single network call powers both screens.
 */
export function useProfile(): UseQueryResult<ConnectProfileDto> {
  return useQuery({
    queryKey: profileKeys.me(),
    queryFn: () => apiClient.get<ConnectProfileDto>('/profile'),
  })
}

/**
 * Fetch the list of car washes (tenants) the user has joined. Used on the
 * Home screen to render loyalty cards.
 */
export function useMyTenants(): UseQueryResult<readonly ConnectTenantSummaryDto[]> {
  return useQuery({
    queryKey: profileKeys.tenants(),
    queryFn: () =>
      apiClient.get<readonly ConnectTenantSummaryDto[]>('/my-carwashes'),
  })
}

/**
 * Derived view over {@link useProfile} — returns only the vehicles slice so
 * components that don't need the user's identity re-render less.
 */
export function useVehicles(): UseQueryResult<readonly ConnectVehicleDto[]> {
  return useQuery({
    queryKey: profileKeys.me(),
    queryFn: () => apiClient.get<ConnectProfileDto>('/profile'),
    select: (data) => data.vehicles,
  })
}

// ── Mutations ───────────────────────────────────────────────────────────────

/**
 * Update the signed-in user's display name / email / avatar. Invalidates the
 * profile query on success so every consumer refetches with the new values.
 */
export function useUpdateProfile(): UseMutationResult<
  ConnectProfileDto,
  unknown,
  UpdateConnectProfileRequest
> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body) =>
      apiClient.patch<ConnectProfileDto>('/profile', body),
    onSuccess: (data) => {
      qc.setQueryData(profileKeys.me(), data)
    },
  })
}

export function useAddVehicle(): UseMutationResult<
  ConnectVehicleDto,
  unknown,
  ConnectVehicleUpsertRequest
> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body) =>
      apiClient.post<ConnectVehicleDto>('/profile/vehicles', body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: profileKeys.me() })
    },
  })
}

export function useUpdateVehicle(): UseMutationResult<
  ConnectVehicleDto,
  unknown,
  { id: string; body: ConnectVehicleUpsertRequest }
> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }) =>
      apiClient.patch<ConnectVehicleDto>(`/profile/vehicles/${id}`, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: profileKeys.me() })
    },
  })
}

export function useDeleteVehicle(): UseMutationResult<void, unknown, string> {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id) => apiClient.delete<void>(`/profile/vehicles/${id}`),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: profileKeys.me() })
    },
  })
}
