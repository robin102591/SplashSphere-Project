'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { DollarSign, ChevronLeft, ChevronRight } from 'lucide-react'
import { useMyRoyalties } from '@/hooks/use-franchise'
import { formatPeso, formatDate } from '@/lib/format'
import { PageHeader } from '@/components/ui/page-header'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from '@/components/ui/table'

// ── Royalty status enum → StatusBadge mapping ───────────────────────────────

const ROYALTY_STATUS: Record<number, { status: string; label: string }> = {
  0: { status: 'Pending', label: 'Pending' },
  1: { status: 'Open', label: 'Invoiced' },
  2: { status: 'Completed', label: 'Paid' },
  3: { status: 'Flagged', label: 'Overdue' },
}

function RoyaltiesSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-6 w-40" />
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      </CardContent>
    </Card>
  )
}

export default function MyRoyaltiesPage() {
  const t = useTranslations('franchise')
  const [page, setPage] = useState(1)
  const { data, isLoading } = useMyRoyalties(page)

  const items = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.ceil(totalCount / 10)

  return (
    <div className="space-y-6">
      <PageHeader title={t('myRoyalties')} />

      {isLoading && <RoyaltiesSkeleton />}

      {!isLoading && items.length === 0 && (
        <EmptyState
          icon={DollarSign}
          title={t('noRoyaltyPeriods')}
          description={t('noRoyaltyPeriodsDescription')}
        />
      )}

      {!isLoading && items.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>{t('royaltyPeriods')}</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t('period')}</TableHead>
                  <TableHead>{t('grossRevenue')}</TableHead>
                  <TableHead>{t('royaltyRate')}</TableHead>
                  <TableHead>{t('royaltyAmount')}</TableHead>
                  <TableHead>{t('marketingFee')}</TableHead>
                  <TableHead>{t('technologyFee')}</TableHead>
                  <TableHead>{t('totalDue')}</TableHead>
                  <TableHead>{t('status')}</TableHead>
                  <TableHead>{t('paidDate')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {items.map((r) => (
                  <TableRow key={r.id}>
                    <TableCell className="whitespace-nowrap">
                      {formatDate(r.periodStart)} &mdash; {formatDate(r.periodEnd)}
                    </TableCell>
                    <TableCell className="font-mono tabular-nums">
                      {formatPeso(r.grossRevenue)}
                    </TableCell>
                    <TableCell>
                      {(r.royaltyRate * 100).toFixed(1)}%
                    </TableCell>
                    <TableCell className="font-mono tabular-nums">
                      {formatPeso(r.royaltyAmount)}
                    </TableCell>
                    <TableCell className="font-mono tabular-nums">
                      {formatPeso(r.marketingFeeAmount)}
                    </TableCell>
                    <TableCell className="font-mono tabular-nums">
                      {formatPeso(r.technologyFeeAmount)}
                    </TableCell>
                    <TableCell className="font-mono tabular-nums font-semibold">
                      {formatPeso(r.totalDue)}
                    </TableCell>
                    <TableCell>
                      <StatusBadge
                        status={ROYALTY_STATUS[r.status]?.status ?? 'Pending'}
                        label={ROYALTY_STATUS[r.status]?.label ?? 'Unknown'}
                      />
                    </TableCell>
                    <TableCell>
                      {r.paidDate ? formatDate(r.paidDate) : '\u2014'}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex items-center justify-between pt-4 border-t mt-4">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page <= 1}
                  onClick={() => setPage((p) => p - 1)}
                >
                  <ChevronLeft className="h-4 w-4 mr-1" />
                  {t('previous')}
                </Button>
                <span className="text-sm text-muted-foreground">
                  {t('pageOf', { current: page, total: totalPages })}
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={page >= totalPages}
                  onClick={() => setPage((p) => p + 1)}
                >
                  {t('next')}
                  <ChevronRight className="h-4 w-4 ml-1" />
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  )
}
