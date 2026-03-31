'use client'

import { usePlan } from '@/hooks/use-plan'

export function TrialBanner() {
  const { data: plan } = usePlan()

  if (!plan || plan.status !== 'trial') return null

  const daysLeft = plan.trial?.daysRemaining ?? 0
  const expired = plan.trial?.expired

  if (expired) {
    return (
      <div className="bg-red-600 text-white text-center py-2 text-sm font-medium">
        Your free trial has ended.{' '}
        <a href="/dashboard/subscription" className="underline font-bold">
          Choose a plan
        </a>{' '}
        to continue using SplashSphere.
      </div>
    )
  }

  return (
    <div className="bg-amber-500 text-white text-center py-2 text-sm font-medium">
      Free trial: {daysLeft} day{daysLeft !== 1 ? 's' : ''} remaining.
      <a href="/dashboard/subscription" className="underline font-bold ml-2">
        Upgrade now
      </a>
    </div>
  )
}
