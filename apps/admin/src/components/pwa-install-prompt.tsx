'use client'

import { Download, X } from 'lucide-react'
import { useState } from 'react'
import { usePwaInstall } from '@/hooks/use-pwa-install'

export function PwaInstallPrompt() {
  const { canInstall, install } = usePwaInstall()
  const [dismissed, setDismissed] = useState(false)

  if (!canInstall || dismissed) return null

  return (
    <div className="mx-3 mb-3 p-3 rounded-lg bg-primary/10 border border-primary/20 text-sm">
      <div className="flex items-start justify-between gap-2">
        <div className="flex items-start gap-2 min-w-0">
          <Download className="h-4 w-4 mt-0.5 shrink-0 text-primary" />
          <span className="text-muted-foreground">Install app for quick access</span>
        </div>
        <button
          onClick={() => setDismissed(true)}
          className="text-muted-foreground hover:text-foreground transition-colors shrink-0"
        >
          <X className="h-3.5 w-3.5" />
        </button>
      </div>
      <button
        onClick={install}
        className="mt-2 w-full bg-primary text-primary-foreground font-medium px-3 py-1.5 rounded-md text-xs hover:bg-primary/90 transition-colors"
      >
        Install SplashSphere
      </button>
    </div>
  )
}
