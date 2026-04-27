'use client'

import { Car, Pencil, Trash2 } from 'lucide-react'
import type { ConnectVehicleDto } from '@splashsphere/types'
import { useTranslations } from 'next-intl'

interface VehicleCardProps {
  vehicle: ConnectVehicleDto
  onEdit: (vehicle: ConnectVehicleDto) => void
  onDelete: (vehicle: ConnectVehicleDto) => void
}

/**
 * One row in the Profile > Vehicles list. Shows make/model/year + plate and
 * exposes Edit/Delete actions.
 *
 * <p>Note: per-tenant vehicle classification (type + size) lives on each car
 * wash's <c>Car</c> record, NOT on the global <c>ConnectVehicle</c>. So we
 * surface a generic "Not yet classified" hint — the cashier locks type/size
 * on the customer's first visit to each branch.</p>
 */
export function VehicleCard({ vehicle, onEdit, onDelete }: VehicleCardProps) {
  const t = useTranslations('profile.vehicles')
  const yearMakeModel = [vehicle.year, vehicle.makeName, vehicle.modelName]
    .filter(Boolean)
    .join(' ')

  return (
    <div className="rounded-2xl border border-border bg-card p-4">
      <div className="flex items-start gap-3">
        <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
          <Car className="h-5 w-5" aria-hidden />
        </div>
        <div className="min-w-0 flex-1">
          <p className="truncate text-base font-semibold text-foreground">
            {yearMakeModel || vehicle.plateNumber}
          </p>
          <p className="mt-0.5 flex flex-wrap items-center gap-x-2 gap-y-0.5 text-sm text-muted-foreground">
            <span className="font-mono font-medium tracking-wide text-foreground">
              {vehicle.plateNumber}
            </span>
            {vehicle.color && (
              <span className="truncate">· {vehicle.color}</span>
            )}
          </p>
          <span
            className="mt-2 inline-flex items-center rounded-full bg-yellow-100 px-2 py-0.5 text-xs font-medium text-yellow-900"
          >
            {t('classificationHint')}
          </span>
        </div>
      </div>
      <div className="mt-3 flex items-center gap-2">
        <button
          type="button"
          onClick={() => onEdit(vehicle)}
          className="flex min-h-[44px] flex-1 items-center justify-center gap-1.5 rounded-xl border border-border bg-background px-3 text-sm font-medium text-foreground transition-colors active:scale-[0.97] hover:bg-muted"
        >
          <Pencil className="h-4 w-4" aria-hidden />
          {t('edit')}
        </button>
        <button
          type="button"
          onClick={() => onDelete(vehicle)}
          className="flex min-h-[44px] items-center justify-center gap-1.5 rounded-xl border border-destructive/30 bg-background px-3 text-sm font-medium text-destructive transition-colors active:scale-[0.97] hover:bg-destructive/10"
        >
          <Trash2 className="h-4 w-4" aria-hidden />
          <span className="sr-only">{t('delete')}</span>
        </button>
      </div>
    </div>
  )
}
