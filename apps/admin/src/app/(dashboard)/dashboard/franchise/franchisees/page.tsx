'use client'

import { useState } from 'react'
import { Users, Plus } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import { useFranchisees } from '@/hooks/use-franchise'
import { formatPeso } from '@/lib/format'
import { useTranslations } from 'next-intl'
import Link from 'next/link'
import { InviteFranchiseeDialog } from './_components/invite-franchisee-dialog'

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

export default function FranchiseesPage() {
  const t = useTranslations('franchise')
  const [page, setPage] = useState(1)
  const [inviteOpen, setInviteOpen] = useState(false)
  const { data, isLoading } = useFranchisees(page)

  const franchisees = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.ceil(totalCount / 10)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{t('franchisees')}</h1>
          <p className="text-muted-foreground">Manage your franchise network members.</p>
        </div>
        <Button onClick={() => setInviteOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          {t('inviteFranchisee')}
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="text-lg">{t('franchisees')}</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <TableSkeleton />
          ) : franchisees.length === 0 ? (
            <EmptyState
              icon={Users}
              title={t('noFranchisees')}
              description="Invite franchisees to grow your network."
            />
          ) : (
            <>
              <div className="rounded-lg border overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Name</th>
                      <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Code</th>
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
                        <td className="px-4 py-3 text-muted-foreground font-mono text-xs">
                          {f.franchiseCode ?? '-'}
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

      <InviteFranchiseeDialog open={inviteOpen} onOpenChange={setInviteOpen} />
    </div>
  )
}
