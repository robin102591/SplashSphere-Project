/**
 * Shared formatting utilities for all SplashSphere apps.
 * Centralizes currency, date, and time formatting in Asia/Manila locale.
 *
 * Why a package: each app previously re-declared these formatters locally,
 * causing drift (different decimal-fraction settings, different "no symbol"
 * variants) and duplicating cached Intl instances across the bundle.
 */

// ── Currency (PHP) ──────────────────────────────────────────────────────────

const pesoFormatter = new Intl.NumberFormat('en-PH', {
  style: 'currency',
  currency: 'PHP',
})

const pesoNoSymbolFormatter = new Intl.NumberFormat('en-PH', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

/** Format a number as Philippine Peso — e.g. ₱1,234.56. Treats null/undefined as 0. */
export function formatPeso(amount: number | null | undefined): string {
  return pesoFormatter.format(amount ?? 0)
}

/** Format a peso amount without the ₱ symbol — e.g. "1,234.56". For tables / cells where the column header already says PHP. */
export function formatPesoNoSymbol(amount: number | null | undefined): string {
  return pesoNoSymbolFormatter.format(amount ?? 0)
}

/** Compact peso for chart axes — e.g. ₱12k, ₱1.2M */
export function formatPesoCompact(amount: number): string {
  if (Math.abs(amount) >= 1_000_000) return `₱${(amount / 1_000_000).toFixed(1)}M`
  if (Math.abs(amount) >= 1_000) return `₱${(amount / 1_000).toFixed(0)}k`
  return `₱${amount}`
}

// ── Date / Time (Asia/Manila) ───────────────────────────────────────────────

const dateFormatter = new Intl.DateTimeFormat('en-PH', {
  timeZone: 'Asia/Manila',
  year: 'numeric',
  month: 'short',
  day: 'numeric',
})

const timeFormatter = new Intl.DateTimeFormat('en-PH', {
  timeZone: 'Asia/Manila',
  hour: '2-digit',
  minute: '2-digit',
})

const dateTimeFormatter = new Intl.DateTimeFormat('en-PH', {
  timeZone: 'Asia/Manila',
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

/** Format an ISO string as a date — e.g. "Mar 25, 2026" */
export function formatDate(iso: string): string {
  return dateFormatter.format(new Date(iso))
}

/** Format an ISO string as a time — e.g. "02:34 PM" */
export function formatTime(iso: string): string {
  return timeFormatter.format(new Date(iso))
}

/** Format an ISO string as date + time — e.g. "Mar 25, 2026, 02:34 PM" */
export function formatDateTime(iso: string): string {
  return dateTimeFormatter.format(new Date(iso))
}
