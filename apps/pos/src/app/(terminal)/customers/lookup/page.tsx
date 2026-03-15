'use client'

import { useState } from 'react'
import { Search } from 'lucide-react'

export default function CustomerLookupPage() {
  const [query, setQuery] = useState('')

  return (
    <div className="p-4 space-y-4">
      <div>
        <h1 className="text-xl font-bold text-white">Customer Lookup</h1>
        <p className="text-sm text-gray-400">Search by plate number or name</p>
      </div>

      <div className="relative">
        <Search className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-500" />
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="ABC 1234 or Juan Cruz"
          autoFocus
          className="w-full min-h-14 pl-12 pr-4 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 text-base focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      {query && (
        <div className="rounded-xl border border-dashed border-gray-700 p-12 text-center">
          <p className="text-gray-500">Search results coming soon</p>
        </div>
      )}
    </div>
  )
}
