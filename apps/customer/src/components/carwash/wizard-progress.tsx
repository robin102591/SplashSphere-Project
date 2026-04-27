'use client'

import { Check } from 'lucide-react'
import { useTranslations } from 'next-intl'
import { cn } from '@/lib/utils'
import type { WizardStep } from '@/hooks/use-booking-wizard'

interface WizardProgressProps {
  current: WizardStep
}

/**
 * 4-segment progress indicator rendered across the top of the booking
 * wizard. Completed segments display a checkmark, the current one is
 * highlighted, and future steps stay muted.
 *
 * Labels come from `booking.steps.*` and wrap below the bar on narrow
 * viewports so each step stays readable without squeezing the bar.
 */
export function WizardProgress({ current }: WizardProgressProps) {
  const t = useTranslations('booking.steps')
  const labels = [t('vehicle'), t('services'), t('schedule'), t('confirm')]

  return (
    <div className="px-4 pb-3 pt-1">
      <ol className="flex items-center gap-1.5" aria-label={t('ariaLabel')}>
        {labels.map((label, index) => {
          const state =
            index < current ? 'done' : index === current ? 'active' : 'todo'
          return (
            <li
              key={label}
              className="flex flex-1 flex-col items-center gap-1.5"
              aria-current={index === current ? 'step' : undefined}
            >
              <div className="flex w-full items-center gap-1.5">
                <span
                  className={cn(
                    'flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-[11px] font-semibold',
                    state === 'done' && 'bg-primary text-primary-foreground',
                    state === 'active' &&
                      'bg-primary text-primary-foreground ring-4 ring-primary/20',
                    state === 'todo' &&
                      'border border-border bg-background text-muted-foreground',
                  )}
                >
                  {state === 'done' ? (
                    <Check className="h-3.5 w-3.5" aria-hidden />
                  ) : (
                    index + 1
                  )}
                </span>
                {index < labels.length - 1 && (
                  <span
                    className={cn(
                      'h-[2px] flex-1 rounded-full',
                      state === 'done' ? 'bg-primary' : 'bg-border',
                    )}
                  />
                )}
              </div>
              <span
                className={cn(
                  'text-[11px] font-medium leading-tight',
                  state === 'active'
                    ? 'text-foreground'
                    : 'text-muted-foreground',
                )}
              >
                {label}
              </span>
            </li>
          )
        })}
      </ol>
    </div>
  )
}
