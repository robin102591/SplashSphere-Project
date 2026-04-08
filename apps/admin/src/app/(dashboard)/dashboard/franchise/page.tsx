'use client'

import { Network, Users, DollarSign, AlertTriangle, Building2, TrendingUp, Ban } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { StatCard } from '@/components/ui/stat-card'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import { useNetworkSummary, useFranchisees } from '@/hooks/use-franchise'
import { formatPeso } from '@/lib/format'
import { useTranslations } from 'next-intl'
import Link from 'next/link'

// ── Helpers ──────────────────────────────────────────────────────────────────

const AGREEMENT_STATUS_LABELS: Record<number, string> = {
  0: 'Draft',
  1: 'Active',
  2: 'Expired',
  3: 'Terminated',
  4: 'Suspended',
}

function agreementStatusBadge(status: number) {
  const label = AGREEMENT_STATUS_LABELS[status] ?? 'Unknown'
  return <StatusBadge status={label} />
}

// ── Loading Skeleton ─────────────────────────────────────────────────────────

function OverviewSkeleton() {
  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-28" />
        ))}
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-28" />
        ))}
      </div>
      <Skeleton className="h-64 w-full" />
    </div>
  )
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function FranchiseOverviewPage() {
  const t = useTranslations('franchise')
  const { data: summary, isLoading: summaryLoading } = useNetworkSummary()
  const { data: franchiseesData, isLoading: franchiseesLoading } = useFranchisees(1)

  const isLoading = summaryLoading || franchiseesLoading

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
            <Network className="h-6 w-6" />
            {t('title')}
          </h1>
          <p className="text-muted-foreground">{t('networkOverview')}</p>
        </div>
        <OverviewSkeleton />
      </div>
    )
  }

  const franchisees = franchiseesData?.items ?? []

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
          <Network className="h-6 w-6" />
          {t('title')}
        </h1>
        <p className="text-muted-foreground">{t('networkOverview')}</p>
      </div>

      {/* KPI Row 1 */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title={t('totalFranchisees')}
          value={String(summary?.totalFranchisees ?? 0)}
          icon={Users}
        />
        <StatCard
          title={t('activeFranchisees')}
          value={String(summary?.activeFranchisees ?? 0)}
          icon={Building2}
        />
        <StatCard
          title={t('totalRevenue')}
          value={formatPeso(summary?.networkRevenueThisMonth ?? 0)}
          icon={TrendingUp}
        />
        <StatCard
          title={t('pendingRoyalties')}
          value={formatPeso(summary?.pendingRoyalties ?? 0)}
          icon={DollarSign}
        />
      </div>

      {/* KPI Row 2 */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          title={t('collectedRoyalties')}
          value={formatPeso(summary?.totalRoyaltiesCollected ?? 0)}
          icon={DollarSign}
        />
        <StatCard
          title={t('overdueRoyalties')}
          value={formatPeso(summary?.overdueRoyalties ?? 0)}
          icon={AlertTriangle}
          highlight={!!summary && summary.overdueRoyalties > 0}
        />
        <StatCard
          title={t('averageRevenue')}
          value={formatPeso(summary?.averageRevenuePerFranchisee ?? 0)}
          icon={TrendingUp}
        />
        <StatCard
          title={t('suspended')}
          value={String(summary?.suspendedFranchisees ?? 0)}
          icon={Ban}
          highlight={!!summary && summary.suspendedFranchisees > 0}
        />
      </div>

      {/* Franchisees Table */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">{t('franchisees')}</CardTitle>
        </CardHeader>
        <CardContent>
          {franchisees.length === 0 ? (
            <EmptyState
              icon={Users}
              title={t('noFranchisees')}
              description="Invite your first franchisee to get started."
            />
          ) : (
            <div className="rounded-lg border overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Name</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('territory')}</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('branches')}</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('revenue')}</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('totalDue')}</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('status')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {franchisees.map((f) => (
                    <tr key={f.tenantId} className="hover:bg-muted/40 transition-colors">
                      <td className="px-4 py-3">
                        <Link
                          href={`/dashboard/franchise/franchisees/${f.tenantId}`}
                          className="font-medium text-primary hover:underline"
                        >
                          {f.name}
                        </Link>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{f.territoryName}</td>
                      <td className="px-4 py-3 text-right font-mono tabular-nums">{f.branchCount}</td>
                      <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(f.revenueThisMonth)}</td>
                      <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(f.royaltyDue)}</td>
                      <td className="px-4 py-3">{agreementStatusBadge(f.agreementStatus)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
