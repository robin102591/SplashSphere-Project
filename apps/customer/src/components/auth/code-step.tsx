'use client'

import { useEffect, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useTranslations } from 'next-intl'
import { Loader2 } from 'lucide-react'
import type {
  ApiError,
  SendOtpResponse,
  VerifyOtpResponse,
} from '@splashsphere/types'
import { apiClient, AUTH_PATHS } from '@/lib/api-client'
import { formatForDisplay } from '@/lib/auth/phone'

interface CodeStepProps {
  /** E.164-normalized phone number set by the previous step. */
  phone: string
  onBack: () => void
  onVerified: (response: VerifyOtpResponse) => void
}

const RESEND_COOLDOWN_SECONDS = 60
const DEV_CODE = process.env.NEXT_PUBLIC_DEV_OTP_CODE ?? ''

const codeSchema = z.object({
  code: z
    .string()
    .regex(/^\d{6}$/, { message: 'invalid' }),
})

type CodeForm = z.infer<typeof codeSchema>

export function CodeStep({ phone, onBack, onVerified }: CodeStepProps) {
  const t = useTranslations('auth.codeStep')
  const tCommon = useTranslations('common')
  const [serverError, setServerError] = useState<string | null>(null)
  const [resendSeconds, setResendSeconds] = useState<number>(
    RESEND_COOLDOWN_SECONDS,
  )
  const [isResending, setIsResending] = useState(false)

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<CodeForm>({
    resolver: zodResolver(codeSchema),
    defaultValues: { code: DEV_CODE || '' },
  })

  // Pre-fill the dev code (handy when Otp:FixedCode is configured upstream).
  const devApplied = useRef(false)
  useEffect(() => {
    if (DEV_CODE && !devApplied.current) {
      devApplied.current = true
      setValue('code', DEV_CODE)
    }
  }, [setValue])

  // Countdown timer for the Resend button.
  useEffect(() => {
    if (resendSeconds <= 0) return
    const id = window.setInterval(() => {
      setResendSeconds((s) => (s > 0 ? s - 1 : 0))
    }, 1000)
    return () => window.clearInterval(id)
  }, [resendSeconds])

  const handleResend = async () => {
    if (resendSeconds > 0 || isResending) return
    setServerError(null)
    setIsResending(true)
    try {
      await apiClient.post<SendOtpResponse>(
        AUTH_PATHS.otpSend,
        { phoneNumber: phone },
        { skipAuth: true },
      )
      setResendSeconds(RESEND_COOLDOWN_SECONDS)
    } catch (e) {
      const err = e as ApiError
      setServerError(
        err.status === 429
          ? err.detail?.toLowerCase().includes('daily')
            ? tCommon('sending')
            : t('resendIn', { seconds: RESEND_COOLDOWN_SECONDS })
          : t('errorGeneric' as const),
      )
    } finally {
      setIsResending(false)
    }
  }

  const onSubmit = handleSubmit(async (values) => {
    setServerError(null)
    try {
      const response = await apiClient.post<VerifyOtpResponse>(
        AUTH_PATHS.otpVerify,
        { phoneNumber: phone, code: values.code },
        { skipAuth: true },
      )
      onVerified(response)
    } catch (e) {
      const err = e as ApiError
      if (err.status === 400 || err.status === 401) {
        setServerError(t('errorWrongCode'))
      } else {
        setServerError(t('errorGeneric'))
      }
    }
  })

  const codeError = errors.code ? t('errorInvalid') : null

  return (
    <div className="space-y-6">
      <div className="rounded-2xl border border-border bg-card p-6">
        <h2 className="text-lg font-semibold">{t('title')}</h2>
        <p className="mt-1 text-sm text-muted-foreground">
          {t('subtitle', { phone: formatForDisplay(phone) })}{' '}
          <button
            type="button"
            onClick={onBack}
            className="font-medium text-primary underline-offset-2 hover:underline"
          >
            {tCommon('change')}
          </button>
        </p>

        <form onSubmit={onSubmit} className="mt-6 space-y-4" noValidate>
          <div className="space-y-1.5">
            <label
              htmlFor="connect-code"
              className="block text-sm font-medium"
            >
              {t('codeLabel')}
            </label>
            <input
              id="connect-code"
              type="text"
              inputMode="numeric"
              pattern="\d{6}"
              maxLength={6}
              autoComplete="one-time-code"
              autoFocus
              placeholder={t('codePlaceholder')}
              className="w-full rounded-xl border border-border bg-background px-4 py-3 text-center text-2xl font-semibold tracking-[0.5em] tabular-nums focus:outline-none focus:ring-2 focus:ring-ring focus-visible:border-ring"
              aria-invalid={errors.code ? 'true' : 'false'}
              {...register('code', {
                onChange: (e) => {
                  // Strip non-digits as the user types.
                  const digits = e.target.value.replace(/\D/g, '').slice(0, 6)
                  e.target.value = digits
                },
              })}
            />
            {DEV_CODE && (
              <p className="text-xs text-muted-foreground">{t('devHint')}</p>
            )}
            {codeError && (
              <p className="text-sm text-red-600" role="alert">
                {codeError}
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
                <span>{tCommon('verifying')}</span>
              </>
            ) : (
              <span>{t('submit')}</span>
            )}
          </button>

          <button
            type="button"
            onClick={handleResend}
            disabled={resendSeconds > 0 || isResending}
            className="flex w-full min-h-[48px] items-center justify-center gap-2 rounded-xl border border-border bg-background px-4 py-2 text-sm font-medium text-foreground transition-colors active:scale-[0.97] disabled:cursor-not-allowed disabled:opacity-60 hover:bg-muted"
          >
            {isResending ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
                <span>{t('resending')}</span>
              </>
            ) : resendSeconds > 0 ? (
              <span>{t('resendIn', { seconds: resendSeconds })}</span>
            ) : (
              <span>{t('resend')}</span>
            )}
          </button>
        </form>
      </div>
    </div>
  )
}
