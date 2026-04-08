'use client'

import { useTranslations } from 'next-intl'
import { FileText } from 'lucide-react'
import { useMyAgreement } from '@/hooks/use-franchise'
import { formatPeso, formatDate } from '@/lib/format'
import { PageHeader } from '@/components/ui/page-header'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'

// ── Agreement status enum → StatusBadge mapping ─────────────────────────────

const AGREEMENT_STATUS: Record<number, { status: string; label: string }> = {
  0: { status: 'Pending', label: 'Draft' },
  1: { status: 'Active', label: 'Active' },
  2: { status: 'Cancelled', label: 'Expired' },
  3: { status: 'Cancelled', label: 'Terminated' },
  4: { status: 'Watch', label: 'Suspended' },
}

function AgreementSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-6 w-48" />
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="space-y-2">
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-5 w-48" />
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}

export default function MyAgreementPage() {
  const t = useTranslations('franchise')
  const { data, isLoading } = useMyAgreement()

  return (
    <div className="space-y-6">
      <PageHeader title={t('myAgreement')} />

      {isLoading && <AgreementSkeleton />}

      {!isLoading && !data && (
        <EmptyState
          icon={FileText}
          title={t('noAgreementFound')}
          description={t('noAgreementDescription')}
        />
      )}

      {!isLoading && data && (
        <Card>
          <CardHeader>
            <CardTitle>{t('agreementDetails')}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <DetailField
                label={t('agreementNumber')}
                value={data.agreementNumber}
              />
              <DetailField
                label={t('territory')}
                value={data.territoryName}
              />
              {data.territoryDescription && (
                <DetailField
                  label={t('territoryDescription')}
                  value={data.territoryDescription}
                />
              )}
              <DetailField
                label={t('exclusiveTerritory')}
                value={data.exclusiveTerritory ? t('yes') : t('no')}
              />
              <DetailField
                label={t('startDate')}
                value={formatDate(data.startDate)}
              />
              <DetailField
                label={t('endDate')}
                value={data.endDate ? formatDate(data.endDate) : t('ongoing')}
              />
              <DetailField
                label={t('initialFranchiseFee')}
                value={formatPeso(data.initialFranchiseFee)}
                mono
              />
              <div className="space-y-1">
                <p className="text-sm font-medium text-muted-foreground">
                  {t('status')}
                </p>
                <div>
                  <StatusBadge
                    status={AGREEMENT_STATUS[data.status]?.status ?? 'Pending'}
                    label={AGREEMENT_STATUS[data.status]?.label ?? 'Unknown'}
                  />
                </div>
              </div>
              <DetailField
                label={t('customRoyaltyRate')}
                value={
                  data.customRoyaltyRate != null
                    ? `${(data.customRoyaltyRate * 100).toFixed(1)}%`
                    : t('standard')
                }
              />
              <DetailField
                label={t('customMarketingFeeRate')}
                value={
                  data.customMarketingFeeRate != null
                    ? `${(data.customMarketingFeeRate * 100).toFixed(1)}%`
                    : t('standard')
                }
              />
              {data.notes && (
                <div className="md:col-span-2 space-y-1">
                  <p className="text-sm font-medium text-muted-foreground">
                    {t('notes')}
                  </p>
                  <p className="text-sm whitespace-pre-wrap">{data.notes}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}

// ── Detail field helper ─────────────────────────────────────────────────────

function DetailField({
  label,
  value,
  mono,
}: {
  label: string
  value: string
  mono?: boolean
}) {
  return (
    <div className="space-y-1">
      <p className="text-sm font-medium text-muted-foreground">{label}</p>
      <p className={mono ? 'text-sm font-mono tabular-nums' : 'text-sm'}>
        {value}
      </p>
    </div>
  )
}
