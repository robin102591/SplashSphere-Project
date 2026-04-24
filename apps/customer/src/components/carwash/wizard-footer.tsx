'use client'

import { Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'

interface WizardFooterProps {
  label: string
  onClick: () => void
  disabled?: boolean
  loading?: boolean
  variant?: 'primary' | 'secondary'
  helperText?: string | null
}

/**
 * Sticky primary-action footer used at the bottom of every wizard step.
 * Pads itself with `safe-area-inset-bottom` so on iOS the button stays
 * clear of the home-indicator.
 *
 * `helperText` renders as a one-line status above the button (totals,
 * inline errors). Empty/null values collapse the row.
 */
export function WizardFooter({
  label,
  onClick,
  disabled = false,
  loading = false,
  variant = 'primary',
  helperText,
}: WizardFooterProps) {
  return (
    <div
      className="sticky bottom-0 z-20 border-t border-border bg-background/95 px-4 pt-3 backdrop-blur"
      style={{ paddingBottom: 'calc(env(safe-area-inset-bottom, 0px) + 12px)' }}
    >
      {helperText && (
        <p className="mb-2 text-center text-xs text-muted-foreground">
          {helperText}
        </p>
      )}
      <button
        type="button"
        onClick={onClick}
        disabled={disabled || loading}
        className={cn(
          'flex min-h-[56px] w-full items-center justify-center gap-2 rounded-2xl px-4 text-base font-semibold transition-transform active:scale-[0.97] disabled:cursor-not-allowed disabled:opacity-50',
          variant === 'primary'
            ? 'bg-primary text-primary-foreground'
            : 'border border-border bg-background text-foreground hover:bg-muted',
        )}
      >
        {loading && <Loader2 className="h-4 w-4 animate-spin" aria-hidden />}
        {label}
      </button>
    </div>
  )
}
