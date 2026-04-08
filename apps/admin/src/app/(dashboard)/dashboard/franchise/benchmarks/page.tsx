'use client'

import { useTranslations } from 'next-intl'
import { BarChart2 } from 'lucide-react'
import { useBenchmarks } from '@/hooks/use-franchise'
import { formatPeso } from '@/lib/format'
import { PageHeader } from '@/components/ui/page-header'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/components/ui/empty-state'

// ── Helpers ─────────────────────────────────────────────────────────────────

/** Metrics that represent money amounts. */
const REVENUE_METRICS = ['revenue', 'average revenue', 'total revenue', 'gross revenue', 'net revenue']

function isRevenueMetric(metric: string): boolean {
  return REVENUE_METRICS.some((rm) => metric.toLowerCase().includes(rm))
}

function formatValue(metric: string, value: number): string {
  if (isRevenueMetric(metric)) return formatPeso(value)
  return value.toLocaleString('en-PH', { maximumFractionDigits: 1 })
}

function capitalize(s: string): string {
  return s.charAt(0).toUpperCase() + s.slice(1)
}

// ── Benchmark card ──────────────────────────────────────────────────────────

interface BenchmarkItem {
  metric: string
  yourValue: number
  networkAverage: number
  rank: number
  totalInNetwork: number
}

function BenchmarkCard({ b }: { b: BenchmarkItem }) {
  const t = useTranslations('franchise')
  const aboveAverage = b.yourValue >= b.networkAverage
  const maxVal = Math.max(b.yourValue, b.networkAverage, 1)
  const yourPct = (b.yourValue / maxVal) * 100
  const avgPct = (b.networkAverage / maxVal) * 100

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">{capitalize(b.metric)}</CardTitle>
          {aboveAverage ? (
            <Badge className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800">
              {t('aboveAverage')}
            </Badge>
          ) : (
            <Badge className="bg-amber-500/15 text-amber-700 dark:text-amber-400 border-amber-200 dark:border-amber-800">
              {t('belowAverage')}
            </Badge>
          )}
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Values side-by-side */}
        <div className="grid grid-cols-2 gap-4">
          <div>
            <p className="text-xs font-medium text-muted-foreground mb-1">
              {t('yourValue')}
            </p>
            <p className="text-lg font-bold font-mono tabular-nums">
              {formatValue(b.metric, b.yourValue)}
            </p>
          </div>
          <div>
            <p className="text-xs font-medium text-muted-foreground mb-1">
              {t('networkAverage')}
            </p>
            <p className="text-lg font-bold font-mono tabular-nums text-muted-foreground">
              {formatValue(b.metric, b.networkAverage)}
            </p>
          </div>
        </div>

        {/* Visual comparison bars */}
        <div className="space-y-2">
          <div className="space-y-1">
            <div className="flex items-center justify-between text-xs text-muted-foreground">
              <span>{t('you')}</span>
              <span>{formatValue(b.metric, b.yourValue)}</span>
            </div>
            <div className="relative h-3 rounded-full bg-muted overflow-hidden">
              <div
                className="absolute inset-y-0 left-0 rounded-full bg-blue-500 transition-all duration-500"
                style={{ width: `${yourPct}%` }}
              />
            </div>
          </div>
          <div className="space-y-1">
            <div className="flex items-center justify-between text-xs text-muted-foreground">
              <span>{t('networkAvg')}</span>
              <span>{formatValue(b.metric, b.networkAverage)}</span>
            </div>
            <div className="relative h-3 rounded-full bg-muted overflow-hidden">
              <div
                className="absolute inset-y-0 left-0 rounded-full bg-gray-400 transition-all duration-500"
                style={{ width: `${avgPct}%` }}
              />
            </div>
          </div>
        </div>

        {/* Rank */}
        <p className="text-sm text-muted-foreground">
          {t('rankOf', { rank: b.rank, total: b.totalInNetwork })}
        </p>
      </CardContent>
    </Card>
  )
}

// ── Skeleton ────────────────────────────────────────────────────────────────

function BenchmarksSkeleton() {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {Array.from({ length: 3 }).map((_, i) => (
        <Card key={i}>
          <CardHeader>
            <Skeleton className="h-5 w-40" />
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-4 w-24" />
          </CardContent>
        </Card>
      ))}
    </div>
  )
}

// ── Page ────────────────────────────────────────────────────────────────────

export default function BenchmarksPage() {
  const t = useTranslations('franchise')
  const { data, isLoading } = useBenchmarks()

  // The hook returns FranchiseBenchmarkDto — could be a single object or array.
  // Normalize to array.
  const benchmarks: BenchmarkItem[] = Array.isArray(data) ? data : data ? [data] : []

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('networkBenchmarks')}
        description={t('networkBenchmarksDescription')}
      />

      {isLoading && <BenchmarksSkeleton />}

      {!isLoading && benchmarks.length === 0 && (
        <EmptyState
          icon={BarChart2}
          title={t('noBenchmarks')}
          description={t('noBenchmarksDescription')}
        />
      )}

      {!isLoading && benchmarks.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {benchmarks.map((b) => (
            <BenchmarkCard key={b.metric} b={b} />
          ))}
        </div>
      )}
    </div>
  )
}
