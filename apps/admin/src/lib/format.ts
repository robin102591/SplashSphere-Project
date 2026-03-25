/**
 * Shared formatting utilities for the Admin app.
 * Centralizes currency, date, and time formatting.
 */

const pesoFormatter = new Intl.NumberFormat('en-PH', {
  style: 'currency',
  currency: 'PHP',
})

/** Format a number as Philippine Peso — e.g. ₱1,234.56 */
export function formatPeso(amount: number): string {
  return pesoFormatter.format(amount)
}

/** Compact peso for chart axes — e.g. ₱12k, ₱1.2M */
export function formatPesoCompact(amount: number): string {
  if (Math.abs(amount) >= 1_000_000) return `₱${(amount / 1_000_000).toFixed(1)}M`
  if (Math.abs(amount) >= 1_000) return `₱${(amount / 1_000).toFixed(0)}k`
  return `₱${amount}`
}

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
