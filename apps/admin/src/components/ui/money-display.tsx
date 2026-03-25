import { cn } from '@/lib/utils'
import { formatPeso } from '@/lib/format'

const SIZE_CLASSES = {
  sm: 'text-sm',
  md: 'text-base',
  lg: 'text-2xl',
  xl: 'text-4xl',
} as const

interface MoneyDisplayProps {
  amount: number
  size?: keyof typeof SIZE_CLASSES
  className?: string
  trend?: { value: number; label: string }
}

export function MoneyDisplay({ amount, size = 'md', className, trend }: MoneyDisplayProps) {
  return (
    <span className={cn('inline-flex items-baseline gap-2', className)}>
      <span className={cn('font-bold tabular-nums font-mono', SIZE_CLASSES[size])}>
        {formatPeso(amount)}
      </span>
      {trend && (
        <span
          className={cn(
            'text-xs font-medium',
            trend.value >= 0 ? 'text-emerald-600' : 'text-red-600',
          )}
        >
          {trend.value >= 0 ? '▲' : '▼'} {Math.abs(trend.value)}% {trend.label}
        </span>
      )}
    </span>
  )
}
