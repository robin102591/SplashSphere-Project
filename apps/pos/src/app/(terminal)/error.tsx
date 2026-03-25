'use client'

import { useEffect } from 'react'
import { AlertTriangle, RotateCcw } from 'lucide-react'

export default function TerminalError({
  error,
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  useEffect(() => {
    console.error('POS error:', error)
  }, [error])

  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 text-center px-4">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-red-500/10">
        <AlertTriangle className="h-8 w-8 text-red-400" />
      </div>
      <div>
        <h2 className="text-xl font-bold text-white">Something went wrong</h2>
        <p className="text-base text-gray-400 mt-1 max-w-md">
          An unexpected error occurred. Please try again.
        </p>
      </div>
      <button
        onClick={reset}
        className="flex items-center gap-2 px-5 min-h-[44px] rounded-xl bg-gray-800 border border-gray-700 hover:bg-gray-700 text-white font-semibold text-sm transition-colors duration-150 active:scale-[0.97]"
      >
        <RotateCcw className="h-4 w-4" />
        Try Again
      </button>
    </div>
  )
}
