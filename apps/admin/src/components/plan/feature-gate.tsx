'use client'

import { useHasFeature } from '@/hooks/use-plan'
import { Lock } from 'lucide-react'

interface FeatureGateProps {
  feature: string
  children: React.ReactNode
  fallback?: React.ReactNode
}

export function FeatureGate({ feature, children, fallback }: FeatureGateProps) {
  const hasFeature = useHasFeature(feature)

  if (hasFeature) return <>{children}</>

  return fallback ?? (
    <div className="border-2 border-dashed border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-950/30 rounded-xl p-8 text-center">
      <Lock className="h-8 w-8 text-amber-400 mx-auto mb-3" />
      <p className="text-amber-800 dark:text-amber-200 font-semibold mb-1">
        Upgrade to unlock this feature
      </p>
      <p className="text-amber-600 dark:text-amber-400 text-sm mb-4">
        This feature is available on the Growth plan and above.
      </p>
      <a
        href="/dashboard/subscription"
        className="text-sm font-semibold text-primary hover:underline"
      >
        View Plans &rarr;
      </a>
    </div>
  )
}
