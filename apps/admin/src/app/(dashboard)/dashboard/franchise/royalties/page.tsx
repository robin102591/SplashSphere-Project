'use client'

import { useState } from 'react'
import { DollarSign, Search } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import { useRoyaltyPeriods, useMarkRoyaltyPaid } from '@/hooks/use-franchise'
import { formatPeso, formatDate } from '@/lib/format'
import { useTranslations } from 'next-intl'
import { toast } from 'sonner'

// ── Helpers ──────────────────────────────────────────────────────────────────

const ROYALTY_STATUS_LABELS: Record<number, string> = {
  0: 'Pending',
  1: 'Invoiced',
  2: 'Paid',
  3: 'Overdue',
}

function royaltyStatusBadge(status: number) {
  const label = ROYALTY_STATUS_LABELS[status] ?? 'Unknown'
  return <StatusBadge status={label} />
}

// ── Loading Skeleton ─────────────────────────────────────────────────────────

function TableSkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: 5 }).map((_, i) => (
        <Skeleton key={i} className="h-12 w-full" />
      ))}
    </div>
  )
}

// ── Main Page ────────────────────────────────────────────────────────────────

export default function RoyaltiesPage() {
  const t = useTranslations('franchise')

  const [page, setPage] = useState(1)
  const [franchiseeIdInput, setFranchiseeIdInput] = useState('')
  const [franchiseeId, setFranchiseeId] = useState<string | undefined>(undefined)
  const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined)

  const { data, isLoading } = useRoyaltyPeriods(page, franchiseeId, statusFilter)
  const { mutateAsync: markPaid, isPending: markingPaid } = useMarkRoyaltyPaid()

  const royalties = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.ceil(totalCount / 10)

  const handleFilter = () => {
    setPage(1)
    setFranchiseeId(franchiseeIdInput.trim() || undefined)
  }

  const handleMarkPaid = async (id: string) => {
    try {
      await markPaid(id)
      toast.success('Royalty marked as paid')
    } catch {
      toast.error('Failed to mark royalty as paid')
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t('royalties')}</h1>
        <p className="text-muted-foreground">Track and manage franchise royalty payments.</p>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap items-end gap-3">
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-muted-foreground">Franchisee ID</label>
              <Input
                placeholder="Filter by franchisee ID..."
                value={franchiseeIdInput}
                onChange={(e) => setFranchiseeIdInput(e.target.value)}
                className="w-64"
              />
            </div>
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-muted-foreground">{t('status')}</label>
              <Select
                value={statusFilter !== undefined ? String(statusFilter) : 'all'}
                onValueChange={(v) => setStatusFilter(v === 'all' ? undefined : Number(v))}
              >
                <SelectTrigger className="w-40">
                  <SelectValue placeholder="All Statuses" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Statuses</SelectItem>
                  <SelectItem value="0">{t('pending')}</SelectItem>
                  <SelectItem value="1">{t('invoiced')}</SelectItem>
                  <SelectItem value="2">{t('paid')}</SelectItem>
                  <SelectItem value="3">{t('overdue')}</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <Button variant="outline" onClick={handleFilter}>
              <Search className="mr-2 h-4 w-4" />
              Filter
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">{t('royalties')}</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <TableSkeleton />
          ) : royalties.length === 0 ? (
            <EmptyState
              icon={DollarSign}
              title={t('noRoyalties')}
              description="Royalty periods will appear here after calculation."
            />
          ) : (
            <>
              <div className="rounded-lg border overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">Franchisee</th>
                      <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">{t('period')}</th>
                      <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">{t('grossRevenue')}</th>
                      <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">{t('royaltyAmount')}</th>
                      <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">{t('marketingFee')}</th>
                      <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">{t('technologyFee')}</th>
                      <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">{t('totalDue')}</th>
                      <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">{t('status')}</th>
                      <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground whitespace-nowrap">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y">
                    {royalties.map((r) => (
                      <tr key={r.id} className="hover:bg-muted/40 transition-colors">
                        <td className="px-4 py-3 font-medium whitespace-nowrap">{r.franchiseeName}</td>
                        <td className="px-4 py-3 text-muted-foreground whitespace-nowrap">
                          {formatDate(r.periodStart)} - {formatDate(r.periodEnd)}
                        </td>
                        <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(r.grossRevenue)}</td>
                        <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(r.royaltyAmount)}</td>
                        <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(r.marketingFeeAmount)}</td>
                        <td className="px-4 py-3 text-right font-mono tabular-nums">{formatPeso(r.technologyFeeAmount)}</td>
                        <td className="px-4 py-3 text-right font-mono tabular-nums font-semibold">{formatPeso(r.totalDue)}</td>
                        <td className="px-4 py-3">{royaltyStatusBadge(r.status)}</td>
                        <td className="px-4 py-3 text-right">
                          {(r.status === 0 || r.status === 1) && (
                            <Button
                              variant="outline"
                              size="sm"
                              disabled={markingPaid}
                              onClick={() => handleMarkPaid(r.id)}
                            >
                              {t('markPaid')}
                            </Button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="flex items-center justify-between pt-4">
                  <p className="text-sm text-muted-foreground">
                    Page {page} of {totalPages}
                  </p>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={page <= 1}
                      onClick={() => setPage(page - 1)}
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={page >= totalPages}
                      onClick={() => setPage(page + 1)}
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
