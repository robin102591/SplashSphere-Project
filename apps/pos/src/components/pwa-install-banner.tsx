'use client'

import { Download, X } from 'lucide-react'
import { useState } from 'react'
import { usePwaInstall } from '@/hooks/use-pwa-install'

export function PwaInstallBanner() {
  const { canInstall, install } = usePwaInstall()
  const [dismissed, setDismissed] = useState(false)

  if (!canInstall || dismissed) return null

  return (
    <div className="bg-blue-600 text-white px-4 py-2 flex items-center justify-between gap-3 text-sm">
      <div className="flex items-center gap-2 min-w-0">
        <Download className="h-4 w-4 shrink-0" />
        <span className="truncate">Install SplashSphere POS for a better experience</span>
      </div>
      <div className="flex items-center gap-2 shrink-0">
        <button
          onClick={install}
          className="bg-white text-blue-600 font-medium px-3 py-1 rounded-lg text-xs hover:bg-blue-50 transition-colors active:scale-[0.97]"
        >
          Install
        </button>
        <button
          onClick={() => setDismissed(true)}
          className="text-blue-200 hover:text-white transition-colors"
        >
          <X className="h-4 w-4" />
        </button>
      </div>
    </div>
  )
}
