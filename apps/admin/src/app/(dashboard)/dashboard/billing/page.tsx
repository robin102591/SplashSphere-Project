'use client'

import { useAuth } from '@clerk/nextjs'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Download } from 'lucide-react'
import { usePlan, useBillingHistory, useCancelSubscription, usePayInvoice } from '@/hooks/use-plan'
import { apiClient } from '@/lib/api-client'
import { cn } from '@/lib/utils'
import { toast } from 'sonner'

export default function BillingPage() {
  const { getToken } = useAuth()
  const { data: plan } = usePlan()
  const { data: history, isLoading } = useBillingHistory()
  const { mutate: cancel, isPending: cancelling } = useCancelSubscription()
  const { mutate: payInvoice, isPending: paying } = usePayInvoice()

  const downloadInvoicePdf = async (billingRecordId: string) => {
    try {
      const token = await getToken()
      await apiClient.download(
        `/billing/invoices/${billingRecordId}/pdf`,
        `invoice_${billingRecordId.slice(0, 8)}.pdf`,
        token ?? undefined,
      )
    } catch {
      toast.error('Failed to download invoice.')
    }
  }

  const handlePayInvoice = (billingRecordId: string) => {
    payInvoice({
      billingRecordId,
      successUrl: `${window.location.origin}/dashboard/billing?payment=success`,
      cancelUrl: `${window.location.origin}/dashboard/billing?payment=cancelled`,
    }, {
      onSuccess: (result) => {
        if (result?.checkoutUrl) window.location.href = result.checkoutUrl
      },
      onError: () => toast.error('Failed to create payment session.'),
    })
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Billing</h1>
        <p className="text-sm text-muted-foreground">Payment history, invoices, and subscription management</p>
      </div>

      {/* Next Billing Info */}
      {plan && plan.status === 'active' && plan.billing?.nextBillingDate && (
        <div className="rounded-lg border p-5 flex items-center justify-between">
          <div>
            <p className="text-sm text-muted-foreground">Next billing date</p>
            <p className="text-lg font-semibold">
              {new Date(plan.billing.nextBillingDate).toLocaleDateString('en-PH', { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}
            </p>
          </div>
          <div className="text-right">
            <p className="text-sm text-muted-foreground">Amount</p>
            <p className="text-lg font-semibold tabular-nums">₱{plan.monthlyPrice.toLocaleString('en-PH', { minimumFractionDigits: 2 })}</p>
          </div>
        </div>
      )}

      {/* Payment History */}
      <div>
        <h3 className="text-sm font-semibold mb-4">Payment History</h3>
        {isLoading ? (
          <Skeleton className="h-48 w-full" />
        ) : history && history.items.length > 0 ? (
          <div className="rounded-lg border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-4 py-2.5 text-left font-medium">Invoice</th>
                  <th className="px-4 py-2.5 text-left font-medium">Date</th>
                  <th className="px-4 py-2.5 text-right font-medium">Amount</th>
                  <th className="px-4 py-2.5 text-left font-medium">Method</th>
                  <th className="px-4 py-2.5 text-left font-medium">Status</th>
                  <th className="px-4 py-2.5 text-right font-medium">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {history.items.map((b) => (
                  <tr key={b.id} className="hover:bg-muted/30">
                    <td className="px-4 py-2.5 font-medium">{b.invoiceNumber ?? '—'}</td>
                    <td className="px-4 py-2.5 text-muted-foreground">
                      {new Date(b.billingDate).toLocaleDateString('en-PH', { month: 'short', day: 'numeric', year: 'numeric' })}
                    </td>
                    <td className="px-4 py-2.5 text-right tabular-nums">
                      ₱{b.amount.toLocaleString('en-PH', { minimumFractionDigits: 2 })}
                    </td>
                    <td className="px-4 py-2.5 text-muted-foreground">{b.paymentMethod ?? '—'}</td>
                    <td className="px-4 py-2.5">
                      <span className={cn(
                        'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium',
                        b.status === 1 ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400' :
                        b.status === 2 ? 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400' :
                        'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300'
                      )}>
                        {b.status === 0 ? 'Pending' : b.status === 1 ? 'Paid' : b.status === 2 ? 'Failed' : b.status === 3 ? 'Refunded' : 'Voided'}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={() => downloadInvoicePdf(b.id)}>
                          <Download className="h-3 w-3 mr-1" />PDF
                        </Button>
                        {b.status === 0 && (
                          <Button variant="default" size="sm" className="h-7 text-xs" disabled={paying} onClick={() => handlePayInvoice(b.id)}>
                            Pay Now
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="flex h-32 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
            No billing records yet.
          </div>
        )}
      </div>

      {/* Cancel Subscription */}
      {plan && plan.status === 'active' && (
        <div className="rounded-lg border border-destructive/20 p-5">
          <h3 className="text-sm font-semibold text-destructive">Danger Zone</h3>
          <p className="text-sm text-muted-foreground mt-1">
            Cancelling your subscription will block access to plan-gated features. Your data will be preserved.
          </p>
          <Button
            variant="outline"
            size="sm"
            className="mt-4 text-destructive border-destructive/30 hover:bg-destructive/10 hover:text-destructive"
            onClick={() => {
              if (confirm('Are you sure you want to cancel your subscription? You will lose access to plan features.'))
                cancel(undefined, {
                  onSuccess: () => toast.success('Subscription cancelled.'),
                  onError: () => toast.error('Failed to cancel subscription.'),
                })
            }}
            disabled={cancelling}
          >
            {cancelling ? 'Cancelling...' : 'Cancel Subscription'}
          </Button>
        </div>
      )}
    </div>
  )
}
