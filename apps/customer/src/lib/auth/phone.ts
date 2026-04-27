/**
 * Philippine mobile-number helpers.
 *
 * The Connect backend stores E.164 form (`+639XXXXXXXXX`), but users type
 * `09XXXXXXXXX`. We accept either, strip everything non-digit, and normalise
 * on submit.
 */

const PH_LOCAL_RE = /^09\d{9}$/
const PH_E164_RE = /^\+639\d{9}$/

/** Strips spaces, dashes, parens, and leading zeros in prefixes. */
export function stripPhoneInput(raw: string): string {
  return raw.replace(/[\s()\-]/g, '')
}

export function isValidPhNumber(raw: string): boolean {
  const cleaned = stripPhoneInput(raw)
  return PH_LOCAL_RE.test(cleaned) || PH_E164_RE.test(cleaned)
}

/** Convert either accepted format to E.164 (`+639XXXXXXXXX`). */
export function toE164(raw: string): string {
  const cleaned = stripPhoneInput(raw)
  if (PH_E164_RE.test(cleaned)) return cleaned
  if (PH_LOCAL_RE.test(cleaned)) return `+63${cleaned.slice(1)}`
  return cleaned
}

/** Format for display: `+63 9XX XXX XXXX`. Falls back to raw on invalid input. */
export function formatForDisplay(raw: string): string {
  const e164 = toE164(raw)
  if (!PH_E164_RE.test(e164)) return raw
  // +63 9XX XXX XXXX
  return `+63 ${e164.slice(3, 6)} ${e164.slice(6, 9)} ${e164.slice(9)}`
}
