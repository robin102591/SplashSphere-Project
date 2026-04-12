/**
 * API-level types: pagination wrappers, error shapes, SignalR hub payloads,
 * common request params, and onboarding request.
 */

import type { QueueDisplayEntry } from './entities';
import type {
  QueueStatus,
  QueuePriority,
  TransactionStatus,
} from './enums';

export type { QueueDisplayEntry };

// ── Pagination ─────────────────────────────────────────────────────────────────

export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
  readonly hasNextPage: boolean;
  readonly hasPreviousPage: boolean;
}

// ── Error shapes ───────────────────────────────────────────────────────────────

/**
 * RFC 7807 ProblemDetails returned by the API on 4xx / 5xx responses.
 */
export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, readonly string[]>;
  [key: string]: unknown;
}

/** Convenience alias — use this in catch blocks. */
export type ApiError = ProblemDetails;

// ── Common request params ──────────────────────────────────────────────────────

export interface PaginationParams {
  page?: number;
  pageSize?: number;
}

export interface DateRangeParams {
  /** ISO-8601 date string (YYYY-MM-DD) */
  startDate: string;
  /** ISO-8601 date string (YYYY-MM-DD) */
  endDate: string;
}

export interface BranchFilterParams {
  branchId?: string;
}

// ── Onboarding request ─────────────────────────────────────────────────────────

export interface CreateOnboardingRequest {
  businessName: string;
  businessEmail: string;
  contactNumber: string;
  address: string;
  branchName: string;
  branchCode: string;
  branchAddress: string;
  branchContactNumber: string;
  businessType?: number;
}

export interface CreateOnboardingResponse {
  tenantId: string;
}

// ── Global Search ─────────────────────────────────────────────────────────────

export interface SearchHit {
  readonly id: string;
  readonly title: string;
  readonly subtitle: string | null;
  readonly category: string;
}

export interface GlobalSearchResult {
  readonly customers: readonly SearchHit[];
  readonly employees: readonly SearchHit[];
  readonly transactions: readonly SearchHit[];
  readonly vehicles: readonly SearchHit[];
  readonly services: readonly SearchHit[];
  readonly merchandise: readonly SearchHit[];
}

// ── SignalR hub payloads ───────────────────────────────────────────────────────

/**
 * Sent to `tenant:{tenantId}:branch:{branchId}` when a transaction is created
 * or its status changes.
 */
export interface TransactionUpdatedPayload {
  transactionId: string;
  transactionNumber: string;
  status: TransactionStatus;
  finalAmount: number;
  branchId: string;
}

/**
 * Sent to `tenant:{tenantId}` group to refresh dashboard counters.
 */
export interface DashboardMetricsUpdatedPayload {
  branchId: string | null;
  triggeredBy: string;
}

/**
 * Sent to `tenant:{tenantId}:branch:{branchId}` when an employee clocks in
 * or out.
 */
export interface AttendanceUpdatedPayload {
  employeeId: string;
  employeeName: string;
  action: 'clock-in' | 'clock-out';
  branchId: string;
  timestamp: string;
}

/**
 * Sent to `tenant:{tenantId}:branch:{branchId}` when the queue changes.
 * The front office queue board listens to this to refresh the Kanban columns.
 */
export interface QueueUpdatedPayload {
  queueEntryId: string;
  queueNumber: string;
  status: QueueStatus;
  priority: QueuePriority;
  branchId: string;
  /** Masked plate number, e.g. "AB***3". */
  maskedPlate: string;
}

/**
 * Sent to `queue-display:{branchId}` (public) for the wall-mounted TV screen.
 * Contains only the fields needed by the public display.
 */
export interface QueueDisplayUpdatedPayload {
  branchId: string;
  calling: readonly QueueDisplayEntry[];
  inService: readonly QueueDisplayEntry[];
  waitingCount: number;
  servedToday?: number;
  avgWaitMinutes?: number | null;
}

/**
 * Sent to `tenant:{tenantId}` when a persistent notification is created.
 */
export interface NotificationReceivedPayload {
  id: string;
  type: number;
  category: number;
  severity: number;
  title: string;
  message: string;
  referenceId: string | null;
  referenceType: string | null;
  actionUrl: string | null;
  actionLabel: string | null;
  createdAt: string;
}

/**
 * Sent to `tenant:{tenantId}` when the daily stock check finds low items.
 */
export interface LowStockAlertPayload {
  merchandiseId: string;
  merchandiseName: string;
  sku: string;
  currentStock: number;
  lowStockThreshold: number;
}

// ── Hub event names (type-safe constants) ─────────────────────────────────────

export const HubEvents = {
  TransactionUpdated: 'TransactionUpdated',
  DashboardMetricsUpdated: 'DashboardMetricsUpdated',
  AttendanceUpdated: 'AttendanceUpdated',
  QueueUpdated: 'QueueUpdated',
  QueueDisplayUpdated: 'QueueDisplayUpdated',
  NotificationReceived: 'NotificationReceived',
  LowStockAlert: 'LowStockAlert',
} as const;

export type HubEventName = (typeof HubEvents)[keyof typeof HubEvents];
