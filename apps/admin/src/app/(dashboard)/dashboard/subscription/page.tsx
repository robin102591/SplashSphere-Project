'use client'

import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { usePlan, useCreateCheckout } from '@/hooks/use-plan'
import { cn } from '@/lib/utils'
import { toast } from 'sonner'
import { Crown, Check } from 'lucide-react'

const PLANS = [
  {
    tier: 'starter', name: 'Starter', price: 1499, highlight: false,
    limits: { branches: '1 branch', employees: '5 employees', sms: 'No SMS' },
    features: ['POS & Transactions', 'Commission Tracking', 'Weekly Payroll', 'Basic Reports', 'Customer & Vehicle Management', 'Merchandise Management'],
  },
  {
    tier: 'growth', name: 'Growth', price: 2999, highlight: true,
    limits: { branches: '3 branches', employees: '15 employees', sms: '50 SMS/mo' },
    features: ['Everything in Starter', 'Queue Management', 'Shift Management', 'Cash Advances', 'Pricing Modifiers', 'P&L Reports', 'SMS Notifications'],
  },
  {
    tier: 'enterprise', name: 'Enterprise', price: 4999, highlight: false,
    limits: { branches: 'Unlimited branches', employees: 'Unlimited employees', sms: '200 SMS/mo' },
    features: ['Everything in Growth', 'API Access', 'Custom Integrations', 'Priority Support'],
  },
]

const PLAN_TIERS: Record<string, number> = { starter: 1, growth: 2, enterprise: 3 }

export default function SubscriptionPage() {
  const { data: plan, isLoading } = usePlan()
  const { mutate: checkout, isPending: checkingOut } = useCreateCheckout()

  const handleUpgrade = (tier: string) => {
    const planNum = PLAN_TIERS[tier]
    if (!planNum) return
    checkout({
      targetPlan: planNum,
      successUrl: `${window.location.origin}/dashboard/subscription?payment=success`,
      cancelUrl: `${window.location.origin}/dashboard/subscription?payment=cancelled`,
    }, {
      onSuccess: (result) => {
        if (result?.checkoutUrl) window.location.href = result.checkoutUrl
      },
      onError: () => toast.error('Failed to create checkout session.'),
    })
  }

  if (isLoading) return (
    <div className="space-y-6">
      <Skeleton className="h-8 w-48" />
      <Skeleton className="h-32 w-full" />
      <div className="grid grid-cols-3 gap-4">
        <Skeleton className="h-80" /><Skeleton className="h-80" /><Skeleton className="h-80" />
      </div>
    </div>
  )

  if (!plan) return null

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Subscription</h1>
        <p className="text-sm text-muted-foreground">Manage your plan and feature access</p>
      </div>

      {/* Current Plan Card */}
      <div className="rounded-xl border p-6 bg-gradient-to-r from-primary/5 to-primary/10">
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center gap-2">
              <Crown className="h-5 w-5 text-primary" />
              <h2 className="text-xl font-bold">{plan.planName}</h2>
            </div>
            <p className="text-sm text-muted-foreground mt-1">
              {plan.status === 'trial' ? (
                <>{plan.trial?.daysRemaining ?? 0} day(s) remaining in your free trial</>
              ) : plan.status === 'active' ? (
                <>₱{plan.monthlyPrice.toLocaleString()}/month</>
              ) : (
                <>Status: {plan.status.replace('_', ' ')}</>
              )}
            </p>
          </div>
          <span className={cn(
            'inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold',
            plan.status === 'active' ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400' :
            plan.status === 'trial' ? 'bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400' :
            'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400'
          )}>
            {plan.status === 'past_due' ? 'Past Due' : plan.status.charAt(0).toUpperCase() + plan.status.slice(1)}
          </span>
        </div>

        {/* Usage meters */}
        <div className="grid grid-cols-3 gap-6 mt-6 pt-4 border-t border-primary/10">
          <UsageMeter label="Branches" current={plan.limits.currentBranches} max={plan.limits.maxBranches} />
          <UsageMeter label="Employees" current={plan.limits.currentEmployees} max={plan.limits.maxEmployees} />
          <UsageMeter label="SMS this month" current={plan.limits.smsUsedThisMonth} max={plan.limits.smsPerMonth} />
        </div>
      </div>

      {/* Plan Comparison */}
      <div>
        <h3 className="text-lg font-semibold mb-4">Available Plans</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
          {PLANS.map((p) => {
            const isCurrent = plan.tier === p.tier
            return (
              <div key={p.tier} className={cn(
                'rounded-xl border p-6 flex flex-col',
                p.highlight && 'border-primary ring-2 ring-primary/20',
                isCurrent && 'bg-muted/40'
              )}>
                {p.highlight && (
                  <span className="text-xs font-semibold text-primary mb-2">Most Popular</span>
                )}
                <p className="text-lg font-bold">{p.name}</p>
                <p className="text-3xl font-bold mt-2">
                  ₱{p.price.toLocaleString()}
                  <span className="text-sm font-normal text-muted-foreground">/mo</span>
                </p>

                <div className="mt-4 space-y-1 text-sm text-muted-foreground">
                  <p>{p.limits.branches}</p>
                  <p>{p.limits.employees}</p>
                  <p>{p.limits.sms}</p>
                </div>

                <ul className="mt-4 pt-4 border-t space-y-2 flex-1">
                  {p.features.map((f) => (
                    <li key={f} className="flex items-start gap-2 text-sm">
                      <Check className="h-4 w-4 text-green-500 mt-0.5 shrink-0" />
                      <span>{f}</span>
                    </li>
                  ))}
                </ul>

                <div className="mt-6">
                  {isCurrent ? (
                    <Button variant="outline" className="w-full" disabled>Current Plan</Button>
                  ) : (
                    <Button
                      variant={p.highlight ? 'default' : 'outline'}
                      className="w-full"
                      disabled={checkingOut}
                      onClick={() => handleUpgrade(p.tier)}
                    >
                      {(plan.tier === 'enterprise') || (plan.tier === 'growth' && p.tier === 'starter') ? 'Downgrade' : 'Upgrade'}
                    </Button>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      </div>
    </div>
  )
}

function UsageMeter({ label, current, max }: { label: string; current: number; max: number }) {
  const isUnlimited = max >= 2147483647
  const pct = isUnlimited ? 0 : max > 0 ? Math.min(100, (current / max) * 100) : 0
  const isHigh = pct >= 80

  return (
    <div>
      <div className="flex items-baseline justify-between mb-1">
        <span className="text-xs text-muted-foreground">{label}</span>
        <span className="text-sm font-semibold tabular-nums">
          {current}{isUnlimited ? '' : ` / ${max}`}
        </span>
      </div>
      {!isUnlimited && (
        <div className="h-1.5 rounded-full bg-muted overflow-hidden">
          <div
            className={cn('h-full rounded-full transition-all', isHigh ? 'bg-red-500' : 'bg-primary')}
            style={{ width: `${pct}%` }}
          />
        </div>
      )}
      {isUnlimited && <p className="text-xs text-muted-foreground">Unlimited</p>}
    </div>
  )
}
