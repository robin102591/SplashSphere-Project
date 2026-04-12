'use client'

import { useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { useQueryClient } from '@tanstack/react-query'
import { Bell, Check, CheckCheck, Receipt, AlertTriangle, Package, Users, CreditCard, Megaphone } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import {
  useNotifications,
  useUnreadCount,
  useMarkRead,
  useMarkAllRead,
  notificationKeys,
} from '@/hooks/use-notifications'
import { useSignalREvent } from '@/lib/signalr-context'
import type { NotificationDto, NotificationReceivedPayload } from '@splashsphere/types'

const categoryIcons: Record<number, typeof Bell> = {
  1: Receipt,        // Operations
  2: Package,        // Inventory
  3: AlertTriangle,  // Finance
  4: Users,          // Queue
  5: CreditCard,     // Billing
  6: Users,          // Customer
  7: Megaphone,      // Platform
}

// Severity: 0=Info, 1=Warning, 2=Critical
const SEVERITY_COLORS: Record<number, string> = {
  0: 'bg-blue-500',
  1: 'bg-amber-500',
  2: 'bg-red-500',
}

function getRoute(n: NotificationDto): string | null {
  if (n.actionUrl) return n.actionUrl
  if (!n.referenceId || !n.referenceType) return null
  switch (n.referenceType) {
    case 'Transaction': return `/dashboard/transactions/${n.referenceId}`
    case 'Merchandise': return '/dashboard/merchandise'
    case 'Shift':          return `/dashboard/shifts/${n.referenceId}`
    case 'QueueEntry':     return '/dashboard/queue'
    case 'PayrollPeriod':  return `/dashboard/payroll/${n.referenceId}`
    default:               return null
  }
}

function showNotificationToast(payload: NotificationReceivedPayload, router: ReturnType<typeof useRouter>) {
  const severity = payload.severity ?? 0
  const duration = severity === 2 ? Infinity : severity === 1 ? 10000 : 5000

  const toastFn = severity === 2 ? toast.error : severity === 1 ? toast.warning : toast.info

  toastFn(payload.title, {
    description: payload.message,
    duration,
    action: payload.actionUrl
      ? { label: payload.actionLabel ?? 'View', onClick: () => router.push(payload.actionUrl!) }
      : undefined,
  })
}

function timeAgo(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 1) return 'just now'
  if (mins < 60) return `${mins}m ago`
  const hrs = Math.floor(mins / 60)
  if (hrs < 24) return `${hrs}h ago`
  const days = Math.floor(hrs / 24)
  return `${days}d ago`
}

export function NotificationDropdown() {
  const router = useRouter()
  const qc = useQueryClient()
  const { data: unread } = useUnreadCount()
  const { data: notifications } = useNotifications({ pageSize: 10 })
  const markRead = useMarkRead()
  const markAllRead = useMarkAllRead()

  const count = unread?.count ?? 0
  const items = notifications?.items ?? []

  // Listen for real-time new notifications — invalidate queries and show toast
  useSignalREvent('NotificationReceived', (payload: NotificationReceivedPayload) => {
    qc.invalidateQueries({ queryKey: notificationKeys.all })
    showNotificationToast(payload, router)
  })

  const handleClick = useCallback(
    (n: NotificationDto) => {
      if (!n.isRead) markRead.mutate(n.id)
      const route = getRoute(n)
      if (route) router.push(route)
    },
    [markRead, router],
  )

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button variant="ghost" size="icon" className="h-8 w-8 relative">
          <Bell className="h-4 w-4" />
          {count > 0 && (
            <span className="absolute -top-0.5 -right-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-bold text-white">
              {count > 99 ? '99+' : count}
            </span>
          )}
        </Button>
      </PopoverTrigger>

      <PopoverContent align="end" className="w-80 p-0">
        {/* Header */}
        <div className="flex items-center justify-between border-b px-3 py-2">
          <span className="text-sm font-medium">Notifications</span>
          {count > 0 && (
            <Button
              variant="ghost"
              size="sm"
              className="h-7 text-xs gap-1"
              onClick={() => markAllRead.mutate()}
              disabled={markAllRead.isPending}
            >
              <CheckCheck className="h-3 w-3" />
              Mark all read
            </Button>
          )}
        </div>

        {/* List */}
        <div className="max-h-80 overflow-y-auto">
          {items.length === 0 ? (
            <div className="px-4 py-8 text-center text-sm text-muted-foreground">
              No notifications yet
            </div>
          ) : (
            items.map((n) => {
              const Icon = categoryIcons[n.category] ?? Bell
              return (
                <button
                  key={n.id}
                  onClick={() => handleClick(n)}
                  className={`flex w-full items-start gap-3 px-3 py-2.5 text-left transition-colors hover:bg-accent/50 ${
                    !n.isRead ? 'bg-accent/20' : ''
                  }`}
                >
                  <div className="relative mt-0.5 rounded-md bg-muted p-1.5">
                    <Icon className="h-3.5 w-3.5 text-muted-foreground" />
                    {(n.severity ?? 0) > 0 && (
                      <span className={`absolute -top-0.5 -right-0.5 h-2 w-2 rounded-full ${SEVERITY_COLORS[n.severity ?? 0]}`} />
                    )}
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <span className={`text-sm truncate ${!n.isRead ? 'font-medium' : ''}`}>
                        {n.title}
                      </span>
                      {!n.isRead && (
                        <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-primary" />
                      )}
                    </div>
                    <p className="text-xs text-muted-foreground line-clamp-2">{n.message}</p>
                    <span className="text-[11px] text-muted-foreground/70">{timeAgo(n.createdAt)}</span>
                  </div>
                </button>
              )
            })
          )}
        </div>
      </PopoverContent>
    </Popover>
  )
}
