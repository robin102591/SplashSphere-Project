/**
 * Small date helpers for the Connect app. Everything here uses
 * `Asia/Manila` as the canonical display timezone — servers send UTC,
 * customers read Manila.
 */

const MANILA_TZ = 'Asia/Manila'

const monthLabelFormatter = new Intl.DateTimeFormat('en-PH', {
  month: 'long',
  year: 'numeric',
  timeZone: MANILA_TZ,
})

const monthKeyFormatter = new Intl.DateTimeFormat('en-CA', {
  // ISO-ish key "YYYY-MM" when we ask for year + numeric month.
  year: 'numeric',
  month: '2-digit',
  timeZone: MANILA_TZ,
})

const shortDateFormatter = new Intl.DateTimeFormat('en-PH', {
  month: 'short',
  day: 'numeric',
  hour: 'numeric',
  minute: '2-digit',
  hour12: true,
  timeZone: MANILA_TZ,
})

/**
 * A month bucket produced by `groupByMonth`.
 */
export interface MonthGroup<T> {
  /** Stable key like "2026-04" suitable for React keys and comparisons. */
  key: string
  /** Human label like "April 2026". */
  label: string
  /** Items belonging to this bucket, in original order. */
  items: T[]
}

/**
 * Group a list of items by Manila-time calendar month, preserving the
 * input order of items inside each bucket. Groups are returned in the
 * order their first item was encountered — if you hand in a
 * newest-first list, the groups come back newest-first too.
 */
export function groupByMonth<T>(
  items: readonly T[],
  getDate: (item: T) => string | Date,
): MonthGroup<T>[] {
  const buckets = new Map<string, MonthGroup<T>>()
  for (const item of items) {
    const raw = getDate(item)
    const date = raw instanceof Date ? raw : new Date(raw)
    if (Number.isNaN(date.getTime())) continue
    const key = monthKeyFormatter.format(date).replace('/', '-')
    let bucket = buckets.get(key)
    if (!bucket) {
      bucket = {
        key,
        label: monthLabelFormatter.format(date),
        items: [],
      }
      buckets.set(key, bucket)
    }
    bucket.items.push(item)
  }
  return Array.from(buckets.values())
}

/**
 * Short Manila-time label, e.g. "Apr 22, 2:30 PM". Safe to call with
 * either an ISO string or a Date. Returns an em-dash for invalid input.
 */
export function formatShortDateTime(value: string | Date): string {
  const date = value instanceof Date ? value : new Date(value)
  if (Number.isNaN(date.getTime())) return '—'
  return shortDateFormatter.format(date)
}
