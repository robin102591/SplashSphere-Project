import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { cn } from '@/lib/utils'

interface StatCardProps {
  title: string
  value: string
  sub?: string
  icon?: React.ElementType
  trend?: { value: number; label: string }
  highlight?: boolean
  className?: string
}

export function StatCard({ title, value, sub, icon: Icon, trend, highlight, className }: StatCardProps) {
  return (
    <Card className={cn(highlight && 'border-primary/30 bg-primary/5', className)}>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        {Icon && <Icon className="h-4 w-4 text-muted-foreground" />}
      </CardHeader>
      <CardContent>
        <p className={cn('text-2xl font-bold tabular-nums', highlight && 'text-primary')}>
          {value}
        </p>
        {(sub || trend) && (
          <div className="flex items-center gap-2 mt-0.5">
            {trend && (
              <span
                className={cn(
                  'text-xs font-medium',
                  trend.value >= 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400',
                )}
              >
                {trend.value >= 0 ? '▲' : '▼'} {Math.abs(trend.value)}%
                {trend.label && ` ${trend.label}`}
              </span>
            )}
            {sub && <p className="text-xs text-muted-foreground">{sub}</p>}
          </div>
        )}
      </CardContent>
    </Card>
  )
}
