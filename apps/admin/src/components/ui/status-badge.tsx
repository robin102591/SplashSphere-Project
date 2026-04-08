import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'

/**
 * Canonical status → style mapping for every status string used in the admin app.
 * Each entry maps to: [bgClass, textClass, borderClass, label].
 * If no label is provided, the status key is used as-is.
 */
const STATUS_STYLES: Record<string, [bg: string, text: string, border: string, label?: string]> = {
  // ── Transaction ──────────────────────────────────────────────
  Pending:      ['bg-amber-500/15', 'text-amber-700 dark:text-amber-400', 'border-amber-200 dark:border-amber-800'],
  InProgress:   ['bg-blue-500/15', 'text-blue-700 dark:text-blue-400', 'border-blue-200 dark:border-blue-800', 'In Progress'],
  Completed:    ['bg-emerald-500/15', 'text-emerald-700 dark:text-emerald-400', 'border-emerald-200 dark:border-emerald-800'],
  Cancelled:    ['bg-gray-500/15', 'text-gray-600 dark:text-gray-400', 'border-gray-200 dark:border-gray-700'],
  Refunded:     ['bg-red-500/15', 'text-red-700 dark:text-red-400', 'border-red-200 dark:border-red-800'],

  // ── Payroll ──────────────────────────────────────────────────
  Open:         ['bg-blue-500/15', 'text-blue-700 dark:text-blue-400', 'border-blue-200 dark:border-blue-800'],
  Closed:       ['bg-amber-500/15', 'text-amber-700 dark:text-amber-400', 'border-amber-200 dark:border-amber-800'],
  Processed:    ['bg-emerald-500/15', 'text-emerald-700 dark:text-emerald-400', 'border-emerald-200 dark:border-emerald-800'],
  Released:     ['bg-violet-500/15', 'text-violet-700 dark:text-violet-400', 'border-violet-200 dark:border-violet-800'],

  // ── Active / Inactive ────────────────────────────────────────
  Active:       ['bg-emerald-500/15', 'text-emerald-700 dark:text-emerald-400', 'border-emerald-200 dark:border-emerald-800'],
  Inactive:     ['bg-gray-500/15', 'text-gray-500 dark:text-gray-400', 'border-gray-200 dark:border-gray-700'],

  // ── Employee type ────────────────────────────────────────────
  Commission:   ['bg-purple-500/15', 'text-purple-700 dark:text-purple-400', 'border-purple-200 dark:border-purple-800'],
  Daily:        ['bg-sky-500/15', 'text-sky-700 dark:text-sky-400', 'border-sky-200 dark:border-sky-800'],

  // ── Queue ────────────────────────────────────────────────────
  Waiting:      ['bg-blue-500/15', 'text-blue-700 dark:text-blue-400', 'border-blue-200 dark:border-blue-800'],
  Called:        ['bg-amber-500/15', 'text-amber-700 dark:text-amber-400', 'border-amber-200 dark:border-amber-800'],
  InService:    ['bg-emerald-500/15', 'text-emerald-700 dark:text-emerald-400', 'border-emerald-200 dark:border-emerald-800', 'In Service'],
  NoShow:       ['bg-red-500/15', 'text-red-700 dark:text-red-400', 'border-red-200 dark:border-red-800', 'No Show'],

  // ── Queue priority ───────────────────────────────────────────
  Vip:          ['bg-purple-500/15', 'text-purple-700 dark:text-purple-400', 'border-purple-200 dark:border-purple-800', 'VIP'],
  Express:      ['bg-blue-500/15', 'text-blue-700 dark:text-blue-400', 'border-blue-200 dark:border-blue-800'],
  Regular:      ['bg-gray-500/15', 'text-gray-600 dark:text-gray-400', 'border-gray-200 dark:border-gray-700'],

  // ── Shift ────────────────────────────────────────────────────
  Voided:       ['bg-red-500/15', 'text-red-700 dark:text-red-400', 'border-red-200 dark:border-red-800'],

  // ── Review status ────────────────────────────────────────────
  Approved:     ['bg-emerald-500/15', 'text-emerald-700 dark:text-emerald-400', 'border-emerald-200 dark:border-emerald-800'],
  Flagged:      ['bg-red-500/15', 'text-red-700 dark:text-red-400', 'border-red-200 dark:border-red-800'],
  // "Pending" review reuses the Pending entry above

  // ── Stock ────────────────────────────────────────────────────
  'Low Stock':  ['bg-amber-500/15', 'text-amber-700 dark:text-amber-400', 'border-amber-200 dark:border-amber-800'],

  // ── Cash advance ────────────────────────────────────────────
  'Fully Paid': ['bg-emerald-500/15', 'text-emerald-700 dark:text-emerald-400', 'border-emerald-200 dark:border-emerald-800'],
  Disbursed:    ['bg-blue-500/15', 'text-blue-700 dark:text-blue-400', 'border-blue-200 dark:border-blue-800'],

  // ── Variance ─────────────────────────────────────────────────
  Watch:        ['bg-red-500/15', 'text-red-700 dark:text-red-400', 'border-red-200 dark:border-red-800'],

  // ── Franchise ───────────────────────────────────────────────
  Draft:        ['bg-gray-500/15', 'text-gray-600 dark:text-gray-400', 'border-gray-200 dark:border-gray-700'],
  Expired:      ['bg-amber-500/15', 'text-amber-700 dark:text-amber-400', 'border-amber-200 dark:border-amber-800'],
  Terminated:   ['bg-red-500/15', 'text-red-700 dark:text-red-400', 'border-red-200 dark:border-red-800'],
  Suspended:    ['bg-amber-500/15', 'text-amber-700 dark:text-amber-400', 'border-amber-200 dark:border-amber-800'],
  Invoiced:     ['bg-blue-500/15', 'text-blue-700 dark:text-blue-400', 'border-blue-200 dark:border-blue-800'],
  Overdue:      ['bg-red-500/15', 'text-red-700 dark:text-red-400', 'border-red-200 dark:border-red-800'],
  Paid:         ['bg-emerald-500/15', 'text-emerald-700 dark:text-emerald-400', 'border-emerald-200 dark:border-emerald-800'],
}

interface StatusBadgeProps {
  /** The status string — matched case-sensitively against the mapping. */
  status: string
  /** Optional override for the displayed label. */
  label?: string
  /** Additional CSS classes. */
  className?: string
}

/**
 * Renders a consistently-styled badge for any known status value.
 * Falls back to a neutral gray badge for unknown statuses.
 */
export function StatusBadge({ status, label, className }: StatusBadgeProps) {
  const entry = STATUS_STYLES[status]
  const [bg, text, border, defaultLabel] = entry ?? ['bg-gray-500/15', 'text-gray-600 dark:text-gray-400', 'border-gray-200 dark:border-gray-700']
  const displayLabel = label ?? defaultLabel ?? status

  return (
    <Badge
      variant="outline"
      className={cn('text-xs font-medium', bg, text, border, className)}
    >
      {displayLabel}
    </Badge>
  )
}
