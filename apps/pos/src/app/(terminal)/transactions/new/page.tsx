'use client'

import { useSearchParams } from 'next/navigation'
import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Suspense } from 'react'

function NewTransactionContent() {
  const searchParams = useSearchParams()
  const queueEntryId = searchParams.get('queueEntryId')

  return (
    <div className="p-4 max-w-2xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <Link
          href="/queue"
          className="flex items-center justify-center h-10 w-10 rounded-xl bg-gray-800 hover:bg-gray-700 text-gray-400 transition-colors"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>
        <div>
          <h1 className="text-xl font-bold text-white">New Transaction</h1>
          <p className="text-sm text-gray-400">
            {queueEntryId ? `Queue entry: ${queueEntryId}` : 'Direct walk-in'}
          </p>
        </div>
      </div>

      <div className="rounded-xl border border-dashed border-gray-700 p-12 text-center">
        <p className="text-gray-500">Transaction form coming soon</p>
        <p className="text-xs text-gray-600 mt-1">Vehicle lookup → Service selection → Employee assignment → Payment</p>
      </div>
    </div>
  )
}

export default function NewTransactionPage() {
  return (
    <Suspense>
      <NewTransactionContent />
    </Suspense>
  )
}
