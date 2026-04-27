'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useTranslations } from 'next-intl'
import { Droplets, Loader2 } from 'lucide-react'
import type { SendOtpResponse, ApiError } from '@splashsphere/types'
import { apiClient, AUTH_PATHS } from '@/lib/api-client'
import { isValidPhNumber, stripPhoneInput, toE164 } from '@/lib/auth/phone'

interface PhoneStepProps {
  initialPhone: string
  onSent: (normalizedPhone: string) => void
}

const phoneSchema = z.object({
  phone: z
    .string()
    .min(1)
    .refine((v) => isValidPhNumber(v), { message: 'invalid' }),
})

type PhoneForm = z.infer<typeof phoneSchema>

export function PhoneStep({ initialPhone, onSent }: PhoneStepProps) {
  const t = useTranslations('auth.phoneStep')
  const tApp = useTranslations('app')
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<PhoneForm>({
    resolver: zodResolver(phoneSchema),
    defaultValues: { phone: initialPhone },
  })

  const onSubmit = handleSubmit(async (values) => {
    setServerError(null)
    const phoneNumber = toE164(values.phone)
    try {
      await apiClient.post<SendOtpResponse>(
        AUTH_PATHS.otpSend,
        { phoneNumber },
        { skipAuth: true },
      )
      onSent(phoneNumber)
    } catch (e) {
      const err = e as ApiError
      // Backend uses 429 for rate-limit and returns a ProblemDetails shape.
      if (err.status === 429) {
        const detail = (err.detail ?? '').toLowerCase()
        setServerError(
          detail.includes('daily') ? t('errorDailyLimit') : t('errorRateLimit'),
        )
      } else if (err.status === 400) {
        setServerError(t('errorInvalid'))
      } else {
        setServerError(t('errorGeneric'))
      }
    }
  })

  const phoneError = errors.phone ? t('errorInvalid') : null

  return (
    <div className="space-y-8">
      <header className="flex flex-col items-center text-center gap-3">
        <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary text-primary-foreground">
          <Droplets className="h-7 w-7" aria-hidden />
        </div>
        <div>
          <h1 className="text-2xl font-semibold leading-tight">
            {tApp('name')}
          </h1>
          <p className="text-sm text-muted-foreground">{tApp('tagline')}</p>
        </div>
      </header>

      <div className="rounded-2xl border border-border bg-card p-6">
        <h2 className="text-lg font-semibold">{t('title')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">{t('subtitle')}</p>

        <form onSubmit={onSubmit} className="mt-6 space-y-4" noValidate>
          <div className="space-y-1.5">
            <label
              htmlFor="connect-phone"
              className="block text-sm font-medium"
            >
              {t('phoneLabel')}
            </label>
            <input
              id="connect-phone"
              type="tel"
              inputMode="tel"
              autoComplete="tel"
              autoFocus
              placeholder={t('phonePlaceholder')}
              className="w-full rounded-xl border border-border bg-background px-4 py-3 text-base focus:outline-none focus:ring-2 focus:ring-ring focus-visible:border-ring"
              aria-invalid={errors.phone ? 'true' : 'false'}
              aria-describedby="connect-phone-hint"
              {...register('phone', {
                onChange: (e) => {
                  // Strip junk in real time so the maxLength check on the
                  // server-side matches what the user sees.
                  e.target.value = stripPhoneInput(e.target.value)
                },
              })}
            />
            <p
              id="connect-phone-hint"
              className="text-xs text-muted-foreground"
            >
              {t('phoneHint')}
            </p>
            {phoneError && (
              <p className="text-sm text-red-600" role="alert">
                {phoneError}
              </p>
            )}
            {serverError && (
              <p className="text-sm text-red-600" role="alert">
                {serverError}
              </p>
            )}
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="flex w-full min-h-[52px] items-center justify-center gap-2 rounded-xl bg-primary px-4 py-3 text-base font-semibold text-primary-foreground transition-opacity active:scale-[0.97] disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? (
              <>
                <Loader2 className="h-5 w-5 animate-spin" aria-hidden />
                <span>{t('submit')}</span>
              </>
            ) : (
              <span>{t('submit')}</span>
            )}
          </button>
        </form>
      </div>
    </div>
  )
}
