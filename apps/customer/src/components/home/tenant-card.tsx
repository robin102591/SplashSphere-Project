'use client'

import Link from 'next/link'
import { Building2, ChevronRight } from 'lucide-react'
import type { ConnectTenantSummaryDto } from '@splashsphere/types'

interface TenantCardProps {
  tenant: ConnectTenantSummaryDto
}

/**
 * Home-screen card for a car wash the user has joined. Links to the
 * tenant's membership detail page where loyalty points + tier are shown.
 * The card deliberately leaves tier/points off the summary because
 * `/my-carwashes` does not return per-tenant loyalty data — fetching it
 * eagerly per row would fan out requests on every render.
 */
export function TenantCard({ tenant }: TenantCardProps) {
  return (
    <Link
      href={`/carwash/${tenant.tenantId}/membership`}
      className="group flex min-h-[72px] items-center gap-3 rounded-2xl border border-border bg-card p-4 transition-colors active:scale-[0.99] hover:border-primary/40 hover:bg-accent/30"
    >
      <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
        <Building2 className="h-5 w-5" aria-hidden />
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-base font-semibold text-foreground">
          {tenant.tenantName}
        </p>
        <p className="truncate text-xs text-muted-foreground">
          {tenant.address}
        </p>
      </div>
      <ChevronRight
        className="h-5 w-5 shrink-0 text-muted-foreground group-hover:text-foreground"
        aria-hidden
      />
    </Link>
  )
}
