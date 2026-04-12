'use client'

import { useState, useCallback, useMemo } from 'react'
import { ArrowLeft, Bell, MessageSquare, Mail, Lock, Loader2, Save } from 'lucide-react'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Switch } from '@/components/ui/switch'
import { toast } from 'sonner'
import {
  useNotificationPreferences,
  useUpdateNotificationPreferences,
} from '@/hooks/use-notifications'
import type { NotificationPreferenceDto } from '@splashsphere/types'
import { cn } from '@/lib/utils'

// ── Category labels ──────────────────────────────────────────────────────────

const CATEGORY_LABELS: Record<number, string> = {
  1: 'Operations',
  2: 'Inventory',
  3: 'Finance',
  4: 'Queue',
  5: 'Billing',
  7: 'Platform',
}

const CATEGORY_ORDER = [1, 2, 3, 4, 5, 7]

// Friendlier display names for notification types
const TYPE_LABELS: Record<string, string> = {
  TransactionVoided: 'Transaction Voided',
  ShiftClosed: 'Shift Closed',
  ShiftFlagged: 'Shift Flagged',
  LowStockAlert: 'Low Stock Alert',
  OutOfStock: 'Out of Stock',
  PayrollProcessed: 'Payroll Processed',
  PayrollReadyForReview: 'Payroll Ready for Review',
  CashAdvanceRequested: 'Cash Advance Requested',
  CashAdvanceApproved: 'Cash Advance Approved',
  BillingInvoiceCreated: 'Invoice Created',
  BillingPaymentReminder: 'Payment Reminder',
  BillingPaymentReceived: 'Payment Received',
  BillingPaymentFailed: 'Payment Failed',
  BillingOverdue: 'Account Overdue',
  BillingSuspended: 'Account Suspended',
  BillingTrialExpiring: 'Trial Expiring',
  PlatformAnnouncement: 'Platform Announcement',
}

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function NotificationPreferencesPage() {
  const { data: preferences, isLoading } = useNotificationPreferences()
  const updateMutation = useUpdateNotificationPreferences()

  // Local state for toggled values
  const [localChanges, setLocalChanges] = useState<
    Record<number, { smsEnabled?: boolean; emailEnabled?: boolean }>
  >({})

  const hasChanges = Object.keys(localChanges).length > 0

  const getMergedPref = useCallback(
    (pref: NotificationPreferenceDto) => ({
      smsEnabled: localChanges[pref.notificationType]?.smsEnabled ?? pref.smsEnabled,
      emailEnabled: localChanges[pref.notificationType]?.emailEnabled ?? pref.emailEnabled,
    }),
    [localChanges],
  )

  const handleToggle = useCallback(
    (type: number, channel: 'sms' | 'email', value: boolean) => {
      setLocalChanges(prev => ({
        ...prev,
        [type]: {
          ...prev[type],
          [`${channel}Enabled`]: value,
        },
      }))
    },
    [],
  )

  const handleSave = useCallback(async () => {
    if (!preferences) return

    const allPrefs = preferences.map(pref => ({
      notificationType: pref.notificationType,
      smsEnabled: localChanges[pref.notificationType]?.smsEnabled ?? pref.smsEnabled,
      emailEnabled: localChanges[pref.notificationType]?.emailEnabled ?? pref.emailEnabled,
    }))

    try {
      await updateMutation.mutateAsync({ preferences: allPrefs })
      setLocalChanges({})
      toast.success('Notification preferences saved')
    } catch {
      toast.error('Failed to save preferences')
    }
  }, [preferences, localChanges, updateMutation])

  // Group preferences by category
  const grouped = useMemo(() => {
    if (!preferences) return {}
    const map: Record<number, NotificationPreferenceDto[]> = {}
    for (const pref of preferences) {
      if (!map[pref.category]) map[pref.category] = []
      map[pref.category].push(pref)
    }
    return map
  }, [preferences])

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-64 w-full" />
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/dashboard/settings">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-4 w-4" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold">Notification Preferences</h1>
            <p className="text-sm text-muted-foreground">
              Choose how you receive notifications for each event type
            </p>
          </div>
        </div>
        {hasChanges && (
          <Button onClick={handleSave} disabled={updateMutation.isPending}>
            {updateMutation.isPending ? (
              <><Loader2 className="mr-2 h-4 w-4 animate-spin" /> Saving...</>
            ) : (
              <><Save className="mr-2 h-4 w-4" /> Save Changes</>
            )}
          </Button>
        )}
      </div>

      {/* Channel Legend */}
      <div className="flex items-center gap-6 text-sm text-muted-foreground">
        <div className="flex items-center gap-2">
          <Bell className="h-4 w-4" /> In-App — always on
        </div>
        <div className="flex items-center gap-2">
          <MessageSquare className="h-4 w-4" /> SMS
        </div>
        <div className="flex items-center gap-2">
          <Mail className="h-4 w-4" /> Email
        </div>
        <div className="flex items-center gap-2">
          <Lock className="h-3 w-3" /> = mandatory
        </div>
      </div>

      {/* Preference Groups */}
      {CATEGORY_ORDER.map(catId => {
        const items = grouped[catId]
        if (!items?.length) return null
        return (
          <Card key={catId}>
            <CardHeader>
              <CardTitle>{CATEGORY_LABELS[catId] ?? `Category ${catId}`}</CardTitle>
              <CardDescription>
                {catId === 5
                  ? 'Billing notifications — email cannot be disabled for compliance'
                  : `Configure ${(CATEGORY_LABELS[catId] ?? '').toLowerCase()} notifications`}
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-0 divide-y">
                {items.map(pref => {
                  const merged = getMergedPref(pref)
                  const label = TYPE_LABELS[pref.typeName] ?? pref.typeName
                  return (
                    <div key={pref.notificationType} className="flex items-center justify-between py-3">
                      <span className="text-sm font-medium">{label}</span>
                      <div className="flex items-center gap-6">
                        {/* SMS */}
                        <div className="flex items-center gap-2 min-w-[80px]">
                          {pref.smsAvailable ? (
                            <>
                              {pref.smsMandatory && <Lock className="h-3 w-3 text-muted-foreground" />}
                              <Switch
                                checked={merged.smsEnabled}
                                disabled={pref.smsMandatory}
                                onCheckedChange={v => handleToggle(pref.notificationType, 'sms', v)}
                              />
                              <MessageSquare className={cn(
                                'h-4 w-4',
                                merged.smsEnabled ? 'text-foreground' : 'text-muted-foreground/40'
                              )} />
                            </>
                          ) : (
                            <span className="text-xs text-muted-foreground">—</span>
                          )}
                        </div>
                        {/* Email */}
                        <div className="flex items-center gap-2 min-w-[80px]">
                          {pref.emailAvailable ? (
                            <>
                              {pref.emailMandatory && <Lock className="h-3 w-3 text-muted-foreground" />}
                              <Switch
                                checked={merged.emailEnabled}
                                disabled={pref.emailMandatory}
                                onCheckedChange={v => handleToggle(pref.notificationType, 'email', v)}
                              />
                              <Mail className={cn(
                                'h-4 w-4',
                                merged.emailEnabled ? 'text-foreground' : 'text-muted-foreground/40'
                              )} />
                            </>
                          ) : (
                            <span className="text-xs text-muted-foreground">—</span>
                          )}
                        </div>
                      </div>
                    </div>
                  )
                })}
              </div>
            </CardContent>
          </Card>
        )
      })}
    </div>
  )
}
