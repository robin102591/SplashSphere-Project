'use client'

import { useReducer, type Dispatch } from 'react'

/** Zero-indexed step identifier. */
export type WizardStep = 0 | 1 | 2 | 3

/**
 * Minimal, serializable wizard state. Kept intentionally small — we only
 * track the user's *selections*, not the hydrated server data (services,
 * slots, branches). Server data lives in React Query and is refetched
 * whenever the wizard remounts.
 *
 * Selections cascade: changing the vehicle wipes services (pricing
 * changes); changing the branch or date wipes the slot (capacity
 * changes).
 */
export interface WizardState {
  step: WizardStep
  vehicleId: string | null
  serviceIds: readonly string[]
  branchId: string | null
  /** Manila-local `YYYY-MM-DD`. */
  date: string | null
  /** ISO-8601 UTC instant selected from the availability grid. */
  slotStartUtc: string | null
  notes: string
}

export type WizardAction =
  | { type: 'setStep'; step: WizardStep }
  | { type: 'next' }
  | { type: 'back' }
  | { type: 'pickVehicle'; vehicleId: string }
  | { type: 'toggleService'; serviceId: string }
  | { type: 'setServices'; serviceIds: readonly string[] }
  | { type: 'pickBranch'; branchId: string }
  | { type: 'pickDate'; date: string }
  | { type: 'pickSlot'; slotStartUtc: string }
  | { type: 'setNotes'; notes: string }
  | { type: 'reset' }

export const initialWizardState: WizardState = {
  step: 0,
  vehicleId: null,
  serviceIds: [],
  branchId: null,
  date: null,
  slotStartUtc: null,
  notes: '',
}

function clampStep(n: number): WizardStep {
  if (n <= 0) return 0
  if (n >= 3) return 3
  return n as WizardStep
}

function reducer(state: WizardState, action: WizardAction): WizardState {
  switch (action.type) {
    case 'setStep':
      return { ...state, step: action.step }

    case 'next':
      return { ...state, step: clampStep(state.step + 1) }

    case 'back':
      return { ...state, step: clampStep(state.step - 1) }

    case 'pickVehicle':
      // Vehicle change invalidates every downstream selection — per-vehicle
      // pricing means the service set + totals become stale.
      if (state.vehicleId === action.vehicleId) return state
      return {
        ...state,
        vehicleId: action.vehicleId,
        serviceIds: [],
        slotStartUtc: null,
      }

    case 'toggleService': {
      const exists = state.serviceIds.includes(action.serviceId)
      const next = exists
        ? state.serviceIds.filter((id) => id !== action.serviceId)
        : [...state.serviceIds, action.serviceId]
      return { ...state, serviceIds: next }
    }

    case 'setServices':
      return { ...state, serviceIds: [...action.serviceIds] }

    case 'pickBranch':
      // Branch change invalidates slot (capacity is branch-scoped).
      if (state.branchId === action.branchId) return state
      return {
        ...state,
        branchId: action.branchId,
        slotStartUtc: null,
      }

    case 'pickDate':
      if (state.date === action.date) return state
      return { ...state, date: action.date, slotStartUtc: null }

    case 'pickSlot':
      return { ...state, slotStartUtc: action.slotStartUtc }

    case 'setNotes':
      return { ...state, notes: action.notes }

    case 'reset':
      return initialWizardState
  }
}

/**
 * React hook wrapping the wizard reducer. The wizard page is the single
 * consumer so we don't promote this to a context — a plain `useReducer`
 * is enough and keeps the route self-contained.
 */
export function useBookingWizard(): readonly [
  WizardState,
  Dispatch<WizardAction>,
] {
  const [state, dispatch] = useReducer(reducer, initialWizardState)
  return [state, dispatch] as const
}

// ── Per-step validation ─────────────────────────────────────────────────────

/**
 * Returns `true` when the user has entered enough data at the current step
 * to advance. The wizard page uses this to enable/disable the "Continue"
 * button without expanding the reducer with dedicated flags.
 */
export function canAdvance(state: WizardState): boolean {
  switch (state.step) {
    case 0:
      return Boolean(state.vehicleId)
    case 1:
      return state.serviceIds.length > 0
    case 2:
      return (
        Boolean(state.branchId) &&
        Boolean(state.date) &&
        Boolean(state.slotStartUtc)
      )
    case 3:
      // Confirm step — the "advance" action here is Submit, handled by the
      // page. We still return true so the submit button isn't disabled by
      // the same guard used by earlier steps.
      return (
        Boolean(state.vehicleId) &&
        state.serviceIds.length > 0 &&
        Boolean(state.branchId) &&
        Boolean(state.slotStartUtc)
      )
  }
}
