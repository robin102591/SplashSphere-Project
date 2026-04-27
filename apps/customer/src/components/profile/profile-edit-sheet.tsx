'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2, X } from 'lucide-react'
import { useTranslations } from 'next-intl'
import type { ConnectProfileDto } from '@splashsphere/types'
import { useUpdateProfile } from '@/hooks/use-profile'

interface ProfileEditSheetProps {
  open: boolean
  onClose: () => void
  profile: ConnectProfileDto
}

const schema = z.object({
  name: z.string().trim().min(1, 'required').max(80, 'tooLong'),
  email: z
    .string()
    .trim()
    .email('invalid')
    .max(120, 'tooLong')
    .optional()
    .or(z.literal('')),
})

type FormValues = z.infer<typeof schema>

/**
 * Bottom-sheet / modal form for editing the signed-in user's name + email.
 * Phone and avatar are not editable here — phone is the globally unique
 * identity, avatar upload isn't wired up yet.
 */
export function ProfileEditSheet({
  open,
  onClose,
  profile,
}: ProfileEditSheetProps) {
  const t = useTranslations('profile.edit')
  const tCommon = useTranslations('common')
  const mutation = useUpdateProfile()
  const [submitError, setSubmitError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: profile.name,
      email: profile.email ?? '',
    },
  })

  // Sheet unmounts when closed, so the next open remounts with fresh state —
  // no need for a reset effect here. The submit error is local state that
  // only persists across re-renders while the sheet is open.
  if (!open) return null

  const onSubmit = handleSubmit(async (values) => {
    setSubmitError(null)
    try {
      await mutation.mutateAsync({
        name: values.name.trim(),
        email: values.email ? values.email.trim() : null,
        avatarUrl: profile.avatarUrl,
      })
      onClose()
    } catch (err) {
      const problem = err as { title?: string; detail?: string }
      setSubmitError(problem.detail ?? problem.title ?? t('errorGeneric'))
    }
  })

  return (
    <div
      role="dialog"
      aria-modal="true"
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 backdrop-blur-sm sm:items-center"
    >
      <div className="relative flex h-auto w-full flex-col rounded-t-2xl bg-background sm:max-w-md sm:rounded-2xl">
        <header className="flex items-center justify-between border-b border-border px-4 py-3">
          <h2 className="text-lg font-semibold">{t('title')}</h2>
          <button
            type="button"
            onClick={onClose}
            aria-label={tCommon('close')}
            className="flex h-10 w-10 items-center justify-center rounded-full text-muted-foreground transition-colors active:scale-[0.95] hover:bg-muted hover:text-foreground"
          >
            <X className="h-5 w-5" aria-hidden />
          </button>
        </header>

        <form onSubmit={onSubmit} className="flex flex-col gap-4 px-4 py-4">
          <label className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-foreground">
              {t('nameLabel')}
            </span>
            <input
              type="text"
              {...register('name')}
              placeholder={t('namePlaceholder')}
              className="min-h-[48px] rounded-xl border border-border bg-background px-3 text-base text-foreground focus:border-primary focus:outline-none"
            />
            {errors.name && (
              <span className="text-xs text-destructive">
                {t('errorRequired')}
              </span>
            )}
          </label>

          <label className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-foreground">
              {t('emailLabel')}
            </span>
            <input
              type="email"
              autoComplete="email"
              {...register('email')}
              placeholder={t('emailPlaceholder')}
              className="min-h-[48px] rounded-xl border border-border bg-background px-3 text-base text-foreground focus:border-primary focus:outline-none"
            />
            {errors.email && (
              <span className="text-xs text-destructive">
                {t('errorEmail')}
              </span>
            )}
          </label>

          {submitError && (
            <p className="rounded-xl bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {submitError}
            </p>
          )}

          <div className="flex flex-col gap-2 pt-2">
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex min-h-[52px] items-center justify-center gap-2 rounded-xl bg-primary px-4 text-base font-semibold text-primary-foreground transition-colors active:scale-[0.97] disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting && (
                <Loader2 className="h-5 w-5 animate-spin" aria-hidden />
              )}
              {tCommon('save')}
            </button>
            <button
              type="button"
              onClick={onClose}
              className="flex min-h-[48px] items-center justify-center rounded-xl border border-border bg-background px-4 text-base font-medium text-foreground transition-colors active:scale-[0.97] hover:bg-muted"
            >
              {tCommon('cancel')}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
