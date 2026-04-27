'use client'

import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Loader2, X } from 'lucide-react'
import { useTranslations } from 'next-intl'
import type {
  ConnectVehicleDto,
  ConnectVehicleUpsertRequest,
} from '@splashsphere/types'
import {
  useAddVehicle,
  useUpdateVehicle,
} from '@/hooks/use-profile'
import { useMakes, useModelsByMake } from '@/hooks/use-catalogue'

interface VehicleFormSheetProps {
  open: boolean
  onClose: () => void
  /** If present, the sheet opens in edit mode for this vehicle. */
  editing?: ConnectVehicleDto | null
}

const currentYear = new Date().getFullYear()

const schema = z.object({
  makeId: z.string().min(1, 'required'),
  modelId: z.string().min(1, 'required'),
  plateNumber: z
    .string()
    .trim()
    .min(2, 'required')
    .max(16, 'tooLong')
    .transform((v) => v.toUpperCase()),
  color: z.string().max(32, 'tooLong').optional().or(z.literal('')),
  year: z
    .union([
      z.coerce
        .number()
        .int()
        .min(1990, 'yearRange')
        .max(currentYear + 1, 'yearRange'),
      z.literal('').transform(() => undefined),
    ])
    .optional(),
})

type FormValues = z.infer<typeof schema>

/**
 * Full-screen modal for adding or editing a Connect vehicle. Uses
 * react-hook-form + zod for validation and drives Make/Model selects off
 * `/catalogue/*`. Plate is always uppercased on submit.
 */
export function VehicleFormSheet({
  open,
  onClose,
  editing,
}: VehicleFormSheetProps) {
  const t = useTranslations('profile.vehicleForm')
  const tCommon = useTranslations('common')

  const addMutation = useAddVehicle()
  const updateMutation = useUpdateVehicle()

  const { data: makes, isLoading: makesLoading } = useMakes()

  const defaultValues = useMemo<FormValues>(
    () => ({
      makeId: editing?.makeId ?? '',
      modelId: editing?.modelId ?? '',
      plateNumber: editing?.plateNumber ?? '',
      color: editing?.color ?? '',
      year: editing?.year ?? undefined,
    }),
    [editing],
  )

  const {
    register,
    handleSubmit,
    watch,
    reset,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues,
  })

  const selectedMakeId = watch('makeId')
  const { data: models, isLoading: modelsLoading } =
    useModelsByMake(selectedMakeId)

  // Reset form when the sheet opens for a different vehicle or closes.
  useEffect(() => {
    if (open) {
      reset(defaultValues)
    }
  }, [open, defaultValues, reset])

  // When the make changes and the currently-selected model doesn't belong to
  // the new make, clear it so the user is forced to pick again.
  useEffect(() => {
    if (!selectedMakeId || !models) return
    const currentModelId = watch('modelId')
    if (
      currentModelId &&
      !models.some((m) => m.id === currentModelId)
    ) {
      setValue('modelId', '')
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedMakeId, models])

  const [submitError, setSubmitError] = useState<string | null>(null)

  if (!open) return null

  const onSubmit = handleSubmit(async (values) => {
    setSubmitError(null)
    const body: ConnectVehicleUpsertRequest = {
      makeId: values.makeId,
      modelId: values.modelId,
      plateNumber: values.plateNumber,
      color: values.color ? values.color.trim() : null,
      year: values.year ?? null,
    }
    try {
      if (editing) {
        await updateMutation.mutateAsync({ id: editing.id, body })
      } else {
        await addMutation.mutateAsync(body)
      }
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
      <div className="relative flex h-[100svh] w-full flex-col bg-background sm:h-auto sm:max-h-[90svh] sm:max-w-md sm:rounded-2xl">
        <header className="flex items-center justify-between border-b border-border px-4 py-3">
          <h2 className="text-lg font-semibold">
            {editing ? t('titleEdit') : t('titleAdd')}
          </h2>
          <button
            type="button"
            onClick={onClose}
            aria-label={tCommon('close')}
            className="flex h-10 w-10 items-center justify-center rounded-full text-muted-foreground transition-colors active:scale-[0.95] hover:bg-muted hover:text-foreground"
          >
            <X className="h-5 w-5" aria-hidden />
          </button>
        </header>

        <form
          onSubmit={onSubmit}
          className="flex flex-1 flex-col gap-4 overflow-y-auto px-4 py-4"
        >
          {/* Make */}
          <label className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-foreground">
              {t('makeLabel')}
            </span>
            <select
              {...register('makeId')}
              disabled={makesLoading}
              className="min-h-[48px] rounded-xl border border-border bg-background px-3 text-base text-foreground focus:border-primary focus:outline-none disabled:opacity-60"
            >
              <option value="">{t('makePlaceholder')}</option>
              {makes?.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.name}
                </option>
              ))}
            </select>
            {errors.makeId && (
              <span className="text-xs text-destructive">
                {t('errorRequired')}
              </span>
            )}
          </label>

          {/* Model */}
          <label className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-foreground">
              {t('modelLabel')}
            </span>
            <select
              {...register('modelId')}
              disabled={!selectedMakeId || modelsLoading}
              className="min-h-[48px] rounded-xl border border-border bg-background px-3 text-base text-foreground focus:border-primary focus:outline-none disabled:opacity-60"
            >
              <option value="">{t('modelPlaceholder')}</option>
              {models?.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.name}
                </option>
              ))}
            </select>
            {errors.modelId && (
              <span className="text-xs text-destructive">
                {t('errorRequired')}
              </span>
            )}
          </label>

          {/* Plate */}
          <label className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-foreground">
              {t('plateLabel')}
            </span>
            <input
              type="text"
              autoCapitalize="characters"
              autoComplete="off"
              {...register('plateNumber', {
                onChange: (e) => {
                  e.target.value = e.target.value.toUpperCase()
                },
              })}
              placeholder="ABC 1234"
              className="min-h-[48px] rounded-xl border border-border bg-background px-3 font-mono text-base uppercase tracking-wider text-foreground focus:border-primary focus:outline-none"
            />
            {errors.plateNumber && (
              <span className="text-xs text-destructive">
                {t('errorRequired')}
              </span>
            )}
          </label>

          {/* Color */}
          <label className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-foreground">
              {t('colorLabel')}
            </span>
            <input
              type="text"
              {...register('color')}
              placeholder={t('colorPlaceholder')}
              className="min-h-[48px] rounded-xl border border-border bg-background px-3 text-base text-foreground focus:border-primary focus:outline-none"
            />
          </label>

          {/* Year */}
          <label className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-foreground">
              {t('yearLabel')}
            </span>
            <input
              type="number"
              inputMode="numeric"
              min={1990}
              max={currentYear + 1}
              {...register('year')}
              placeholder={String(currentYear)}
              className="min-h-[48px] rounded-xl border border-border bg-background px-3 text-base text-foreground focus:border-primary focus:outline-none"
            />
            {errors.year && (
              <span className="text-xs text-destructive">
                {t('errorYearRange', {
                  min: 1990,
                  max: currentYear + 1,
                })}
              </span>
            )}
          </label>

          {submitError && (
            <p className="rounded-xl bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {submitError}
            </p>
          )}

          <div className="mt-auto flex flex-col gap-2 pt-4">
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex min-h-[52px] items-center justify-center gap-2 rounded-xl bg-primary px-4 text-base font-semibold text-primary-foreground transition-colors active:scale-[0.97] disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting && (
                <Loader2 className="h-5 w-5 animate-spin" aria-hidden />
              )}
              {editing ? tCommon('save') : t('submitAdd')}
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
