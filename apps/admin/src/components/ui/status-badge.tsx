import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'

/**
 * Canonical status → style mapping for every status string used in the admin app.
 * Each entry maps to: [bgClass, textClass, borderClass, label].
 * If no label is provided, the status key is used as-is.
 */
const STATUS_STYLES: Record<string, [bg: string, text: string, border: string, label?: string]> = {
  // ── Transaction ──────────────────────────────────────────────
  Pending:      ['bg-amber-500/15', 'text-amber-700', 'border-amber-200'],
  InProgress:   ['bg-blue-500/15', 'text-blue-700', 'border-blue-200', 'In Progress'],
  Completed:    ['bg-emerald-500/15', 'text-emerald-700', 'border-emerald-200'],
  Cancelled:    ['bg-gray-500/15', 'text-gray-600', 'border-gray-200'],
  Refunded:     ['bg-red-500/15', 'text-red-700', 'border-red-200'],

  // ── Payroll ──────────────────────────────────────────────────
  Open:         ['bg-blue-500/15', 'text-blue-700', 'border-blue-200'],
  Closed:       ['bg-amber-500/15', 'text-amber-700', 'border-amber-200'],
  Processed:    ['bg-emerald-500/15', 'text-emerald-700', 'border-emerald-200'],

  // ── Active / Inactive ────────────────────────────────────────
  Active:       ['bg-emerald-500/15', 'text-emerald-700', 'border-emerald-200'],
  Inactive:     ['bg-gray-100', 'text-gray-500', 'border-gray-200'],

  // ── Employee type ────────────────────────────────────────────
  Commission:   ['bg-purple-500/15', 'text-purple-700', 'border-purple-200'],
  Daily:        ['bg-sky-500/15', 'text-sky-700', 'border-sky-200'],

  // ── Queue ────────────────────────────────────────────────────
  Waiting:      ['bg-blue-500/15', 'text-blue-700', 'border-blue-200'],
  Called:        ['bg-amber-500/15', 'text-amber-700', 'border-amber-200'],
  InService:    ['bg-emerald-500/15', 'text-emerald-700', 'border-emerald-200', 'In Service'],
  NoShow:       ['bg-red-500/15', 'text-red-700', 'border-red-200', 'No Show'],

  // ── Queue priority ───────────────────────────────────────────
  Vip:          ['bg-purple-500/15', 'text-purple-700', 'border-purple-200', 'VIP'],
  Express:      ['bg-blue-500/15', 'text-blue-700', 'border-blue-200'],
  Regular:      ['bg-gray-100', 'text-gray-600', 'border-gray-200'],

  // ── Shift ────────────────────────────────────────────────────
  Voided:       ['bg-red-500/15', 'text-red-700', 'border-red-200'],

  // ── Review status ────────────────────────────────────────────
  Approved:     ['bg-emerald-500/15', 'text-emerald-700', 'border-emerald-200'],
  Flagged:      ['bg-red-500/15', 'text-red-700', 'border-red-200'],
  // "Pending" review reuses the Pending entry above

  // ── Stock ────────────────────────────────────────────────────
  'Low Stock':  ['bg-amber-500/15', 'text-amber-700', 'border-amber-200'],

  // ── Variance ─────────────────────────────────────────────────
  Watch:        ['bg-red-500/15', 'text-red-700', 'border-red-200'],
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
  const [bg, text, border, defaultLabel] = entry ?? ['bg-gray-100', 'text-gray-600', 'border-gray-200']
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
