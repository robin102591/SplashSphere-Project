'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { Pencil, Plus, User as UserIcon } from 'lucide-react'
import type { ConnectVehicleDto } from '@splashsphere/types'
import { useProfile, useDeleteVehicle } from '@/hooks/use-profile'
import { formatForDisplay } from '@/lib/auth/phone'
import { VehicleCard } from '@/components/profile/vehicle-card'
import { VehicleFormSheet } from '@/components/profile/vehicle-form-sheet'
import { ProfileEditSheet } from '@/components/profile/profile-edit-sheet'
import { LanguageToggle } from '@/components/profile/language-toggle'
import { SignOutButton } from '@/components/auth/sign-out-button'

export default function ProfilePage() {
  const t = useTranslations('profile')
  const tNav = useTranslations('nav')
  const { data: profile, isPending, isError } = useProfile()
  const deleteVehicle = useDeleteVehicle()

  const [editingProfile, setEditingProfile] = useState(false)
  const [vehicleSheet, setVehicleSheet] = useState<{
    open: boolean
    editing: ConnectVehicleDto | null
  }>({ open: false, editing: null })

  const openAddVehicle = () =>
    setVehicleSheet({ open: true, editing: null })
  const openEditVehicle = (vehicle: ConnectVehicleDto) =>
    setVehicleSheet({ open: true, editing: vehicle })
  const closeVehicleSheet = () =>
    setVehicleSheet({ open: false, editing: null })

  const handleDelete = async (vehicle: ConnectVehicleDto) => {
    if (
      !window.confirm(
        t('vehicles.deleteConfirm', { plate: vehicle.plateNumber }),
      )
    ) {
      return
    }
    try {
      await deleteVehicle.mutateAsync(vehicle.id)
    } catch {
      window.alert(t('vehicles.deleteError'))
    }
  }

  return (
    <section className="space-y-6">
      <h1 className="text-xl font-semibold">{tNav('profile')}</h1>

      {/* Profile card */}
      {isPending && <ProfileCardSkeleton />}

      {isError && (
        <p className="rounded-2xl border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
          {t('loadError')}
        </p>
      )}

      {profile && (
        <div className="rounded-2xl border border-border bg-card p-5">
          <div className="flex items-start gap-4">
            <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-muted text-lg font-semibold text-muted-foreground">
              {initialsOf(profile.name) || (
                <UserIcon className="h-6 w-6" aria-hidden />
              )}
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-base font-semibold">
                {profile.name || profile.phone}
              </p>
              <p className="truncate text-sm text-muted-foreground">
                {formatForDisplay(profile.phone)}
              </p>
              {profile.email && (
                <p className="truncate text-xs text-muted-foreground">
                  {profile.email}
                </p>
              )}
            </div>
          </div>
          <button
            type="button"
            onClick={() => setEditingProfile(true)}
            className="mt-4 flex min-h-[44px] w-full items-center justify-center gap-1.5 rounded-xl border border-border bg-background px-3 text-sm font-medium text-foreground transition-colors active:scale-[0.97] hover:bg-muted"
          >
            <Pencil className="h-4 w-4" aria-hidden />
            {t('editProfile')}
          </button>
        </div>
      )}

      {/* Vehicles */}
      <div className="space-y-3">
        <div className="flex items-center justify-between">
          <h2 className="text-base font-semibold">{t('vehicles.title')}</h2>
          <button
            type="button"
            onClick={openAddVehicle}
            className="flex min-h-[40px] items-center gap-1 rounded-xl bg-primary px-3 text-sm font-semibold text-primary-foreground transition-colors active:scale-[0.97]"
          >
            <Plus className="h-4 w-4" aria-hidden />
            {t('vehicles.add')}
          </button>
        </div>

        {profile && profile.vehicles.length === 0 && (
          <div className="rounded-2xl border border-dashed border-border bg-card p-5 text-center">
            <p className="text-sm font-medium text-foreground">
              {t('vehicles.emptyTitle')}
            </p>
            <p className="mt-1 text-xs text-muted-foreground">
              {t('vehicles.emptyHint')}
            </p>
          </div>
        )}

        {profile && profile.vehicles.length > 0 && (
          <ul className="space-y-3">
            {profile.vehicles.map((v) => (
              <li key={v.id}>
                <VehicleCard
                  vehicle={v}
                  onEdit={openEditVehicle}
                  onDelete={handleDelete}
                />
              </li>
            ))}
          </ul>
        )}
      </div>

      {/* Settings */}
      <div className="space-y-3">
        <h2 className="text-base font-semibold">{t('settings.title')}</h2>
        <LanguageToggle />
      </div>

      {/* Sign out */}
      <div className="pt-2">
        <SignOutButton />
      </div>

      {/* Sheets */}
      {profile && (
        <ProfileEditSheet
          open={editingProfile}
          profile={profile}
          onClose={() => setEditingProfile(false)}
        />
      )}
      <VehicleFormSheet
        open={vehicleSheet.open}
        editing={vehicleSheet.editing}
        onClose={closeVehicleSheet}
      />
    </section>
  )
}

/** Build up to 2 uppercase initials from a full name, or null if empty. */
function initialsOf(name: string): string | null {
  if (!name) return null
  const parts = name.split(/\s+/).filter(Boolean).slice(0, 2)
  const result = parts.map((p) => p[0]?.toUpperCase() ?? '').join('')
  return result || null
}

function ProfileCardSkeleton() {
  return (
    <div
      className="h-[144px] animate-pulse rounded-2xl border border-border bg-card"
      aria-busy="true"
    />
  )
}
