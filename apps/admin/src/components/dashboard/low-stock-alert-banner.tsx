'use client'

import Link from 'next/link'
import { AlertTriangle } from 'lucide-react'
import { useQueryClient } from '@tanstack/react-query'
import { useLowStockItems, merchandiseKeys } from '@/hooks/use-merchandise'
import { useSignalREvent } from '@/lib/signalr-context'

export function LowStockAlertBanner() {
  const qc = useQueryClient()
  const { data: items } = useLowStockItems()

  // Refresh when a low-stock alert is broadcast
  useSignalREvent('LowStockAlert', () => {
    qc.invalidateQueries({ queryKey: merchandiseKeys.lowStock })
  })

  if (!items || items.length === 0) return null

  return (
    <div className="flex items-start gap-3 rounded-lg border border-amber-500/30 bg-amber-500/10 px-4 py-3 dark:border-amber-500/20 dark:bg-amber-500/5">
      <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-amber-600 dark:text-amber-400" />
      <div className="min-w-0 flex-1">
        <p className="text-sm font-medium text-amber-800 dark:text-amber-300">
          Low Stock Alert — {items.length} item{items.length > 1 ? 's' : ''} below threshold
        </p>
        <ul className="mt-1 space-y-0.5">
          {items.slice(0, 5).map((item) => (
            <li key={item.id} className="text-xs text-amber-700 dark:text-amber-400/80">
              {item.name} ({item.sku}) — <span className="font-medium">{item.stockQuantity}</span> remaining
              (threshold: {item.lowStockThreshold})
            </li>
          ))}
          {items.length > 5 && (
            <li className="text-xs text-amber-700 dark:text-amber-400/80">
              ...and {items.length - 5} more
            </li>
          )}
        </ul>
        <Link
          href="/dashboard/merchandise"
          className="mt-1.5 inline-block text-xs font-medium text-amber-700 underline underline-offset-2 hover:text-amber-900 dark:text-amber-400 dark:hover:text-amber-300"
        >
          View merchandise
        </Link>
      </div>
    </div>
  )
}
