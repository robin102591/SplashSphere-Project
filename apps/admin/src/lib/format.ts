/**
 * Re-exports of the shared `@splashsphere/format` helpers so existing
 * `@/lib/format` imports keep working. New code should import from
 * `@splashsphere/format` directly.
 */
export {
  formatPeso,
  formatPesoNoSymbol,
  formatPesoCompact,
  formatDate,
  formatTime,
  formatDateTime,
} from '@splashsphere/format'
