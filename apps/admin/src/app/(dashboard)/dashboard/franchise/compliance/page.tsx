'use client'

import { useTranslations } from 'next-intl'
import { CheckCircle2, XCircle, AlertTriangle, ShieldCheck } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'
import { PageHeader } from '@/components/ui/page-header'
import { useComplianceReport } from '@/hooks/use-franchise'

// ── Helpers ───────────────────────────────────────────────────────────────────

function ComplianceIcon({ compliant }: { compliant: boolean }) {
  return compliant ? (
    <CheckCircle2 className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
  ) : (
    <XCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
  )
}

function ScoreBadge({ score }: { score: number }) {
  const rounded = Math.round(score)
  let colorClasses: string

  if (rounded >= 80) {
    colorClasses = 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800'
  } else if (rounded >= 60) {
    colorClasses = 'bg-amber-500/15 text-amber-700 dark:text-amber-400 border-amber-200 dark:border-amber-800'
  } else {
    colorClasses = 'bg-red-500/15 text-red-700 dark:text-red-400 border-red-200 dark:border-red-800'
  }

  return (
    <span className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium ${colorClasses}`}>
      {rounded}%
    </span>
  )
}

// ── Loading Skeleton ──────────────────────────────────────────────────────────

function ComplianceSkeleton() {
  return (
    <Card>
      <CardContent className="p-0">
        <div className="space-y-0 divide-y">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="px-4 py-3 flex gap-4">
              <Skeleton className="h-5 w-40" />
              <Skeleton className="h-5 w-24" />
              <Skeleton className="h-5 w-8" />
              <Skeleton className="h-5 w-8" />
              <Skeleton className="h-5 w-8" />
              <Skeleton className="h-5 w-8" />
              <Skeleton className="h-5 w-16" />
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function CompliancePage() {
  const t = useTranslations('franchise')
  const { data: items, isLoading } = useComplianceReport()

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('compliance')}
        back="/dashboard/franchise"
        description="Review compliance status across all franchisees in your network."
      />

      {isLoading ? (
        <ComplianceSkeleton />
      ) : !items || items.length === 0 ? (
        <EmptyState
          icon={ShieldCheck}
          title="No compliance data"
          description="Compliance data will appear once you have active franchisees in your network."
        />
      ) : (
        <Card>
          <CardContent className="p-0">
            <div className="rounded-lg overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Franchisee</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('territory')}</th>
                    <th className="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider text-muted-foreground">Std. Services</th>
                    <th className="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider text-muted-foreground">Pricing</th>
                    <th className="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider text-muted-foreground">Royalties</th>
                    <th className="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider text-muted-foreground">Agreement</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">Score</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {items.map((item) => (
                    <tr key={item.tenantId} className="hover:bg-muted/40 transition-colors">
                      <td className="px-4 py-3 font-medium">{item.name}</td>
                      <td className="px-4 py-3 text-muted-foreground">{item.territoryName}</td>
                      <td className="px-4 py-3">
                        <div className="flex justify-center">
                          <ComplianceIcon compliant={item.usingStandardServices} />
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex justify-center">
                          <ComplianceIcon compliant={item.pricingCompliant} />
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex justify-center">
                          <ComplianceIcon compliant={item.royaltiesCurrent} />
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex justify-center">
                          {item.agreementExpiringSoon ? (
                            <AlertTriangle className="h-4 w-4 text-amber-600 dark:text-amber-400" />
                          ) : (
                            <CheckCircle2 className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-right">
                        <ScoreBadge score={item.complianceScore} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
