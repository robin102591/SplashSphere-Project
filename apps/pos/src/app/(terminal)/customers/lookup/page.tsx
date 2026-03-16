'use client'

import { useState, useEffect, useRef } from 'react'
import Link from 'next/link'
import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import {
  Search, X, Car, User2, Phone, Mail,
  Plus, ListOrdered, ChevronDown, ChevronUp,
} from 'lucide-react'
import type { Car as CarType, CustomerDetail, Customer } from '@splashsphere/types'
import type { PagedResult } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'
import { cn } from '@/lib/utils'

type Mode = 'plate' | 'name'

export default function CustomerLookupPage() {
  const { getToken } = useAuth()
  const [mode, setMode] = useState<Mode>('plate')
  const [plateInput, setPlateInput] = useState('')
  const [nameInput, setNameInput] = useState('')
  const [expandedCustomerId, setExpandedCustomerId] = useState<string | null>(null)
  const plateRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    plateRef.current?.focus()
  }, [mode])

  // ── Plate lookup ────────────────────────────────────────────────────────────

  const plate = plateInput.trim().toUpperCase().replace(/\s+/g, ' ')

  const {
    data: carResult,
    isFetching: plateFetching,
    error: plateError,
    refetch: lookupPlate,
  } = useQuery({
    queryKey: ['car-lookup', plate],
    enabled: false,
    staleTime: 60_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CarType>(`/cars/lookup/${encodeURIComponent(plate)}`, token ?? undefined)
    },
  })

  const handlePlateLookup = () => {
    if (plate.length >= 2) lookupPlate()
  }

  // ── Name search ─────────────────────────────────────────────────────────────

  const debouncedName = useDebounce(nameInput, 400)

  const { data: customerResults, isFetching: nameFetching } = useQuery({
    queryKey: ['customers-search', debouncedName],
    enabled: debouncedName.trim().length >= 2,
    staleTime: 30_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<PagedResult<Customer>>(
        `/customers?search=${encodeURIComponent(debouncedName.trim())}&page=1&pageSize=15`,
        token ?? undefined,
      )
    },
  })

  const { data: expandedCustomer } = useQuery({
    queryKey: ['customer-detail', expandedCustomerId],
    enabled: !!expandedCustomerId,
    staleTime: 60_000,
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<CustomerDetail>(`/customers/${expandedCustomerId}`, token ?? undefined)
    },
  })

  return (
    <div className="p-4 space-y-4 max-w-lg mx-auto">

      {/* Header */}
      <div>
        <h1 className="text-xl font-bold text-white">Customer Lookup</h1>
        <p className="text-sm text-gray-400">Find a customer by plate or name</p>
      </div>

      {/* Mode toggle */}
      <div className="flex rounded-xl overflow-hidden border border-gray-700 bg-gray-800 p-1 gap-1">
        {(['plate', 'name'] as const).map((m) => (
          <button
            key={m}
            onClick={() => setMode(m)}
            className={cn(
              'flex-1 min-h-[40px] rounded-lg text-sm font-medium transition-colors',
              mode === m ? 'bg-blue-600 text-white' : 'text-gray-400 hover:text-white',
            )}
          >
            {m === 'plate' ? 'By Plate' : 'By Name'}
          </button>
        ))}
      </div>

      {/* ── Plate mode ─────────────────────────────────────────────────────── */}
      {mode === 'plate' && (
        <div className="space-y-3">
          <div className="flex gap-2">
            <div className="relative flex-1">
              <Car className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-500 pointer-events-none" />
              <input
                ref={plateRef}
                type="text"
                value={plateInput}
                onChange={(e) => setPlateInput(e.target.value.toUpperCase())}
                onKeyDown={(e) => e.key === 'Enter' && handlePlateLookup()}
                placeholder="ABC 1234"
                maxLength={12}
                className="w-full min-h-[52px] pl-10 pr-10 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 font-mono text-base tracking-widest focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              {plateInput && (
                <button
                  onClick={() => setPlateInput('')}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-white"
                >
                  <X className="h-4 w-4" />
                </button>
              )}
            </div>
            <button
              onClick={handlePlateLookup}
              disabled={plate.length < 2 || plateFetching}
              className="min-h-[52px] px-5 rounded-xl bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white text-sm font-semibold transition-colors"
            >
              {plateFetching ? 'Looking…' : 'Lookup'}
            </button>
          </div>

          {/* Plate result */}
          {plateError && (
            <div className="rounded-xl bg-red-900/20 border border-red-800 p-4 text-center">
              <p className="text-red-400 text-sm">No vehicle found with plate "{plate}"</p>
            </div>
          )}

          {carResult && !plateError && (
            <CarResultCard car={carResult} />
          )}
        </div>
      )}

      {/* ── Name mode ──────────────────────────────────────────────────────── */}
      {mode === 'name' && (
        <div className="space-y-3">
          <div className="relative">
            <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-500 pointer-events-none" />
            <input
              type="text"
              value={nameInput}
              onChange={(e) => setNameInput(e.target.value)}
              placeholder="Juan Cruz or 09171234567"
              autoFocus
              className="w-full min-h-[52px] pl-10 pr-10 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {nameInput && (
              <button
                onClick={() => setNameInput('')}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-white"
              >
                <X className="h-4 w-4" />
              </button>
            )}
          </div>

          {nameFetching && (
            <div className="space-y-2">
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="h-16 rounded-xl bg-gray-800 animate-pulse" />
              ))}
            </div>
          )}

          {customerResults && customerResults.items.length === 0 && debouncedName.length >= 2 && (
            <div className="rounded-xl border border-dashed border-gray-700 py-10 text-center">
              <p className="text-gray-500 text-sm">No customers found for "{debouncedName}"</p>
            </div>
          )}

          {customerResults && customerResults.items.length > 0 && (
            <div className="space-y-2">
              {customerResults.items.map((customer) => {
                const isExpanded = expandedCustomerId === customer.id
                const detail = isExpanded ? expandedCustomer : null
                return (
                  <div key={customer.id} className="rounded-xl bg-gray-800 border border-gray-700 overflow-hidden">
                    <button
                      onClick={() => setExpandedCustomerId(isExpanded ? null : customer.id)}
                      className="w-full flex items-center justify-between px-4 py-3 hover:bg-gray-700/50 transition-colors text-left"
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="h-9 w-9 rounded-full bg-blue-600/20 border border-blue-600/30 flex items-center justify-center shrink-0">
                          <User2 className="h-4 w-4 text-blue-400" />
                        </div>
                        <div className="min-w-0">
                          <p className="text-sm font-semibold text-white">{customer.fullName}</p>
                          <div className="flex items-center gap-2 text-xs text-gray-500">
                            {customer.contactNumber && (
                              <span className="flex items-center gap-1">
                                <Phone className="h-3 w-3" />{customer.contactNumber}
                              </span>
                            )}
                            {customer.email && (
                              <span className="flex items-center gap-1 truncate">
                                <Mail className="h-3 w-3" />{customer.email}
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                      {isExpanded
                        ? <ChevronUp className="h-4 w-4 text-gray-500 shrink-0" />
                        : <ChevronDown className="h-4 w-4 text-gray-500 shrink-0" />
                      }
                    </button>

                    {isExpanded && (
                      <div className="border-t border-gray-700 px-4 py-3 space-y-2">
                        {detail ? (
                          detail.cars.length > 0 ? (
                            detail.cars.map((car) => (
                              <div key={car.id} className="flex items-center justify-between gap-2">
                                <div>
                                  <p className="text-sm font-mono font-bold text-white tracking-wide">{car.plateNumber}</p>
                                  <p className="text-xs text-gray-500">
                                    {car.vehicleTypeName} · {car.sizeName}
                                    {car.makeName && ` · ${car.makeName}${car.modelName ? ` ${car.modelName}` : ''}`}
                                    {car.color && ` · ${car.color}`}
                                  </p>
                                </div>
                                <div className="flex gap-1.5 shrink-0">
                                  <Link
                                    href={`/queue/add?plate=${encodeURIComponent(car.plateNumber)}`}
                                    className="flex items-center gap-1 min-h-[36px] px-2.5 rounded-lg bg-gray-700 hover:bg-gray-600 text-xs text-white transition-colors"
                                  >
                                    <ListOrdered className="h-3.5 w-3.5" />
                                    Queue
                                  </Link>
                                  <Link
                                    href={`/transactions/new?carId=${car.id}`}
                                    className="flex items-center gap-1 min-h-[36px] px-2.5 rounded-lg bg-blue-600 hover:bg-blue-500 text-xs text-white transition-colors"
                                  >
                                    <Plus className="h-3.5 w-3.5" />
                                    Transact
                                  </Link>
                                </div>
                              </div>
                            ))
                          ) : (
                            <p className="text-xs text-gray-500 text-center py-2">No vehicles on file</p>
                          )
                        ) : (
                          <div className="h-12 rounded-lg bg-gray-700 animate-pulse" />
                        )}
                      </div>
                    )}
                  </div>
                )
              })}
            </div>
          )}
        </div>
      )}
    </div>
  )
}

// ── Car result card ────────────────────────────────────────────────────────────

function CarResultCard({ car }: { car: CarType }) {
  return (
    <div className="rounded-xl bg-gray-800 border border-green-700/50 p-4 space-y-3">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-2xl font-mono font-bold text-white tracking-widest">{car.plateNumber}</p>
          <p className="text-sm text-gray-400 mt-0.5">
            {car.vehicleTypeName} · {car.sizeName}
            {car.makeName && ` · ${car.makeName}${car.modelName ? ` ${car.modelName}` : ''}`}
            {car.color && ` · ${car.color}`}
            {car.year && ` (${car.year})`}
          </p>
        </div>
        <div className="h-10 w-10 rounded-full bg-green-600/20 border border-green-600/30 flex items-center justify-center shrink-0">
          <Car className="h-5 w-5 text-green-400" />
        </div>
      </div>

      {car.customerFullName && (
        <div className="flex items-center gap-2 text-sm text-gray-300">
          <User2 className="h-4 w-4 text-gray-500" />
          {car.customerFullName}
        </div>
      )}

      <div className="flex gap-2 pt-1">
        <Link
          href={`/queue/add?plate=${encodeURIComponent(car.plateNumber)}`}
          className="flex-1 flex items-center justify-center gap-2 min-h-[48px] rounded-xl bg-gray-700 hover:bg-gray-600 text-sm font-medium text-white transition-colors"
        >
          <ListOrdered className="h-4 w-4" />
          Add to Queue
        </Link>
        <Link
          href={`/transactions/new?carId=${car.id}`}
          className="flex-1 flex items-center justify-center gap-2 min-h-[48px] rounded-xl bg-blue-600 hover:bg-blue-500 text-sm font-medium text-white transition-colors"
        >
          <Plus className="h-4 w-4" />
          New Transaction
        </Link>
      </div>
    </div>
  )
}

// ── useDebounce hook ───────────────────────────────────────────────────────────

function useDebounce<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState(value)
  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(id)
  }, [value, delay])
  return debounced
}
