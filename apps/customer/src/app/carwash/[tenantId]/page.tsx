'use client'

import { use, useState } from 'react'
import Link from 'next/link'
import { useTranslations } from 'next-intl'
import {
  Award,
  Building2,
  Calendar,
  CheckCircle2,
  Droplets,
  Loader2,
  MapPin,
  Phone,
  Plus,
} from 'lucide-react'
import { useCarwashDetail, useJoinCarwash } from '@/hooks/use-carwash'
import { AppBar } from '@/components/layout/app-bar'
import { BranchCard } from '@/components/carwash/branch-card'
import { ServiceRow } from '@/components/carwash/service-row'

interface CarwashDetailPageProps {
  params: Promise<{ tenantId: string }>
}

/**
 * Tenant detail page. Shows a compact hero, the list of publicly listed
 * branches, the service catalogue (base prices), and the primary CTAs
 * (Book / Rewards / Join). The booking wizard and membership pages are
 * later tasks — their CTAs link to placeholders marked with TODOs.
 */
export default function CarwashDetailPage({ params }: CarwashDetailPageProps) {
  const { tenantId } = use(params)
  const t = useTranslations('carwash')

  const detail = useCarwashDetail(tenantId)
  const join = useJoinCarwash()
  const [joinError, setJoinError] = useState<string | null>(null)

  const onJoin = async () => {
    setJoinError(null)
    try {
      await join.mutateAsync(tenantId)
    } catch {
      setJoinError(t('joinError'))
    }
  }

  // ── Loading / error / not-found states ────────────────────────────────────

  if (detail.isPending) {
    return (
      <>
        <AppBar />
        <div className="space-y-4 p-4">
          <div className="h-28 animate-pulse rounded-2xl border border-border bg-card" />
          <div className="h-20 animate-pulse rounded-2xl border border-border bg-card" />
          <div className="h-20 animate-pulse rounded-2xl border border-border bg-card" />
        </div>
      </>
    )
  }

  if (detail.isError) {
    return (
      <>
        <AppBar />
        <div className="p-4">
          <div className="rounded-2xl border border-destructive/20 bg-destructive/5 p-4 text-center">
            <p className="text-sm font-medium text-destructive">
              {t('loadError')}
            </p>
            <button
              type="button"
              onClick={() => detail.refetch()}
              className="mt-3 inline-flex min-h-[44px] items-center justify-center rounded-xl border border-border bg-background px-4 text-sm font-semibold text-foreground transition-colors active:scale-[0.97] hover:bg-muted"
            >
              {t('retry')}
            </button>
          </div>
        </div>
      </>
    )
  }

  const tenant = detail.data
  if (!tenant) {
    return (
      <>
        <AppBar />
        <div className="p-4">
          <p className="rounded-2xl border border-border bg-card p-6 text-center text-sm text-muted-foreground">
            {t('notFound')}
          </p>
        </div>
      </>
    )
  }

  // Feature-gate defense: if no branch has booking enabled, we still render
  // the tenant (they might only take walk-ins) but hide the Book CTA.
  const anyBranchBooking = tenant.branches.some((b) => b.isBookingEnabled)

  return (
    <>
      <AppBar title={tenant.tenantName} backHref="/discover" />

      <div className="space-y-6 p-4 pb-8">
        {/* ── Hero ─────────────────────────────────────────────────────── */}
        <section className="flex items-start gap-3">
          <div
            className="flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-primary/10 text-base font-semibold text-primary"
            aria-hidden
          >
            {initialsOf(tenant.tenantName) || (
              <Droplets className="h-6 w-6" />
            )}
          </div>
          <div className="min-w-0 flex-1">
            <h2 className="text-lg font-semibold leading-tight">
              {tenant.tenantName}
            </h2>
            <p className="mt-1 flex items-start gap-1.5 text-xs text-muted-foreground">
              <MapPin className="mt-0.5 h-3.5 w-3.5 shrink-0" aria-hidden />
              <span>{tenant.address}</span>
            </p>
            {tenant.contactNumber && (
              <p className="mt-1 flex items-center gap-1.5 text-xs text-muted-foreground">
                <Phone className="h-3.5 w-3.5 shrink-0" aria-hidden />
                <a
                  href={`tel:${tenant.contactNumber}`}
                  className="underline-offset-2 hover:underline"
                >
                  {tenant.contactNumber}
                </a>
              </p>
            )}
            {tenant.isJoined && (
              <span className="mt-2 inline-flex items-center gap-1 rounded-full bg-primary/10 px-2.5 py-0.5 text-[11px] font-semibold uppercase tracking-wide text-primary">
                <CheckCircle2 className="h-3 w-3" aria-hidden />
                {t('joined')}
              </span>
            )}
          </div>
        </section>

        {/* ── Primary CTAs ──────────────────────────────────────────────── */}
        <section className="grid grid-cols-2 gap-3">
          {anyBranchBooking ? (
            <Link
              href={`/carwash/${tenant.tenantId}/book`}
              className="flex min-h-[56px] items-center justify-center gap-2 rounded-2xl bg-primary px-4 text-sm font-semibold text-primary-foreground transition-transform active:scale-[0.97]"
            >
              <Calendar className="h-4 w-4" aria-hidden />
              {t('bookCta')}
            </Link>
          ) : (
            <div className="flex min-h-[56px] items-center justify-center gap-2 rounded-2xl border border-dashed border-border bg-card px-4 text-sm font-semibold text-muted-foreground">
              <Building2 className="h-4 w-4" aria-hidden />
              {t('inStoreOnly')}
            </div>
          )}
          <Link
            href={`/carwash/${tenant.tenantId}/membership`}
            className="flex min-h-[56px] items-center justify-center gap-2 rounded-2xl border border-border bg-card px-4 text-sm font-semibold text-foreground transition-colors active:scale-[0.97] hover:bg-accent/30"
          >
            <Award className="h-4 w-4 text-primary" aria-hidden />
            {t('rewardsCta')}
          </Link>
        </section>

        {/* ── Join CTA (hidden once joined) ─────────────────────────────── */}
        {!tenant.isJoined && (
          <section>
            <button
              type="button"
              onClick={onJoin}
              disabled={join.isPending}
              className="flex min-h-[56px] w-full items-center justify-center gap-2 rounded-2xl border-2 border-primary bg-primary/5 px-4 text-sm font-semibold text-primary transition-colors active:scale-[0.97] disabled:opacity-60"
            >
              {join.isPending ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
                  {t('joining')}
                </>
              ) : (
                <>
                  <Plus className="h-4 w-4" aria-hidden />
                  {t('join')}
                </>
              )}
            </button>
            {joinError && (
              <p className="mt-2 text-center text-xs text-destructive">
                {joinError}
              </p>
            )}
          </section>
        )}

        {/* ── Branches ──────────────────────────────────────────────────── */}
        <section className="space-y-3">
          <h3 className="text-base font-semibold">{t('branchesTitle')}</h3>
          {tenant.branches.length === 0 ? (
            <p className="rounded-2xl border border-dashed border-border bg-card p-4 text-center text-sm text-muted-foreground">
              {t('emptyBranches')}
            </p>
          ) : (
            <ul className="space-y-3">
              {tenant.branches.map((b) => (
                <li key={b.id}>
                  <BranchCard branch={b} />
                </li>
              ))}
            </ul>
          )}
        </section>

        {/* ── Services ──────────────────────────────────────────────────── */}
        <section className="space-y-3">
          <h3 className="text-base font-semibold">{t('servicesTitle')}</h3>
          {tenant.services.length === 0 ? (
            <p className="rounded-2xl border border-dashed border-border bg-card p-4 text-center text-sm text-muted-foreground">
              {t('emptyServices')}
            </p>
          ) : (
            <ul className="space-y-2">
              {tenant.services.map((s) => (
                <li key={s.id}>
                  <ServiceRow service={s} />
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </>
  )
}

/** Up to 2 uppercase initials from a name, or empty string. */
function initialsOf(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean).slice(0, 2)
  return parts.map((p) => p[0]?.toUpperCase() ?? '').join('')
}
