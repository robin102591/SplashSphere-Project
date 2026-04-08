'use client'

import { useParams } from 'next/navigation'
import {
  Building2, Mail, Phone, MapPin, FileText, Calendar, DollarSign, Shield,
} from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import {
  useFranchiseeDetail, useSuspendFranchisee, useReactivateFranchisee,
} from '@/hooks/use-franchise'
import { formatPeso, formatDate } from '@/lib/format'
import { useTranslations } from 'next-intl'
import { toast } from 'sonner'
import Link from 'next/link'

// ── Helpers ──────────────────────────────────────────────────────────────────

const AGREEMENT_STATUS_LABELS: Record<number, string> = {
  0: 'Draft',
  1: 'Active',
  2: 'Expired',
  3: 'Terminated',
  4: 'Suspended',
}

const ROYALTY_STATUS_LABELS: Record<number, string> = {
  0: 'Pending',
  1: 'Invoiced',
  2: 'Paid',
  3: 'Overdue',
}

function agreementStatusBadge(status: number) {
  const label = AGREEMENT_STATUS_LABELS[status] ?? 'Unknown'
  return <StatusBadge status={label} />
}

function royaltyStatusBadge(status: number) {
  const label = ROYALTY_STATUS_LABELS[status] ?? 'Unknown'
  return <StatusBadge status={label} />
}

// ── Loading Skeleton ─────────────────────────────────────────────────────────

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-8 w-64" />
      <div className="grid gap-6 lg:grid-cols-3">
        <Skeleton className="h-48" />
        <Skeleton className="h-48" />
        <Skeleton className="h-48" />
      </div>
      <Skeleton className="h-64 w-full" />
    </div>
  )
}

// ── Info Row ─────────────────────────────────────────────────────────────────

function InfoRow({ icon: Icon, label, value }: { icon: React.ElementType; label: string; value: string }) {
  return (
    <div className="flex items-start gap-2 text-sm">
      <Icon className="h-4 w-4 text-muted-foreground mt-0.5 shrink-0" />
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="font-medium">{value}</p>
      </div>
    </div>
  )
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function FranchiseeDetailPage() {
  const t = useTranslations('franchise')
  const params = useParams()
  const id = params.id as string

  const { data: detail, isLoading } = useFranchiseeDetail(id)
  const { mutateAsync: suspend, isPending: suspending } = useSuspendFranchisee()
  const { mutateAsync: reactivate, isPending: reactivating } = useReactivateFranchisee()

  const handleSuspend = async () => {
    try {
      await suspend(id)
      toast.success('Franchisee suspended successfully')
    } catch {
      toast.error('Failed to suspend franchisee')
    }
  }

  const handleReactivate = async () => {
    try {
      await reactivate(id)
      toast.success('Franchisee reactivated successfully')
    } catch {
      toast.error('Failed to reactivate franchisee')
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="text-sm text-muted-foreground">
          <Link href="/dashboard/franchise/franchisees" className="hover:underline">
            {t('franchisees')}
          </Link>
          {' > '}
          <span>Loading...</span>
        </div>
        <DetailSkeleton />
      </div>
    )
  }

  if (!detail) {
    return (
      <div className="space-y-6">
        <div className="text-sm text-muted-foreground">
          <Link href="/dashboard/franchise/franchisees" className="hover:underline">
            {t('franchisees')}
          </Link>
          {' > '}
          <span>Not Found</span>
        </div>
        <p className="text-muted-foreground">Franchisee not found.</p>
      </div>
    )
  }

  const agreement = detail.agreement
  const royalties = detail.recentRoyalties ?? []

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="text-sm text-muted-foreground">
        <Link href="/dashboard/franchise/franchisees" className="hover:underline">
          {t('franchisees')}
        </Link>
        {' > '}
        <span className="text-foreground font-medium">{detail.name}</span>
      </div>

      <h1 className="text-2xl font-bold tracking-tight">{detail.name}</h1>

      {/* Info Cards */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Business Info */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <Building2 className="h-4 w-4" />
              Business Info
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <InfoRow icon={Building2} label="Name" value={detail.name} />
            <InfoRow icon={Mail} label={t('email')} value={detail.email} />
            <InfoRow icon={Phone} label="Contact" value={detail.contactNumber} />
            <InfoRow icon={MapPin} label="Address" value={detail.address} />
            {detail.franchiseCode && (
              <InfoRow icon={Shield} label="Franchise Code" value={detail.franchiseCode} />
            )}
          </CardContent>
        </Card>

        {/* Agreement Info */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <FileText className="h-4 w-4" />
              Agreement
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {agreement ? (
              <>
                <InfoRow icon={FileText} label={t('agreementNumber')} value={agreement.agreementNumber} />
                <InfoRow icon={MapPin} label={t('territory')} value={agreement.territoryName} />
                <InfoRow icon={Calendar} label={t('startDate')} value={formatDate(agreement.startDate)} />
                {agreement.endDate && (
                  <InfoRow icon={Calendar} label={t('endDate')} value={formatDate(agreement.endDate)} />
                )}
                <InfoRow icon={DollarSign} label="Initial Fee" value={formatPeso(agreement.initialFranchiseFee)} />
                {agreement.customRoyaltyRate != null && (
                  <InfoRow icon={DollarSign} label="Custom Royalty Rate" value={`${(agreement.customRoyaltyRate * 100).toFixed(1)}%`} />
                )}
                {agreement.customMarketingFeeRate != null && (
                  <InfoRow icon={DollarSign} label="Custom Marketing Fee" value={`${(agreement.customMarketingFeeRate * 100).toFixed(1)}%`} />
                )}
                <div className="pt-1">
                  <p className="text-xs text-muted-foreground mb-1">{t('status')}</p>
                  {agreementStatusBadge(agreement.status)}
                </div>
              </>
            ) : (
              <p className="text-sm text-muted-foreground">No agreement on file.</p>
            )}
          </CardContent>
        </Card>

        {/* Actions */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center gap-2 text-sm">
              <span className="text-muted-foreground">{t('status')}:</span>
              <StatusBadge status={detail.isActive ? 'Active' : 'Suspended'} />
            </div>
            <div className="flex flex-col gap-2 pt-2">
              {detail.isActive ? (
                <Button
                  variant="destructive"
                  onClick={handleSuspend}
                  disabled={suspending}
                >
                  {suspending ? 'Suspending...' : t('suspend')}
                </Button>
              ) : (
                <Button
                  variant="default"
                  onClick={handleReactivate}
                  disabled={reactivating}
                >
                  {reactivating ? 'Reactivating...' : t('reactivate')}
                </Button>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Recent Royalties */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">{t('royalties')}</CardTitle>
        </CardHeader>
        <CardContent>
          {royalties.length === 0 ? (
            <p className="text-sm text-muted-foreground">{t('noRoyalties')}</p>
          ) : (
            <div className="rounded-lg border overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('period')}</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('grossRevenue')}</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('royaltyAmount')}</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('totalDue')}</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('status')}</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('paidDate')}</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {royalties.map((r) => (
                    <tr key={r.id} className="hover:bg-muted/40 transition-colors">
                      <td className="px-4 py-3 text-muted-foreground">
                        {formatDate(r.periodStart)} - {formatDate(r.periodEnd)}
                      </td>
                      <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(r.grossRevenue)}</td>
                      <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(r.royaltyAmount)}</td>
                      <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(r.totalDue)}</td>
                      <td className="px-4 py-3">{royaltyStatusBadge(r.status)}</td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {r.paidDate ? formatDate(r.paidDate) : '-'}
                      </td>
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
