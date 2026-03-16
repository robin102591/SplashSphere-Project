'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Car, Search } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { useCars } from '@/hooks/use-cars'

export default function VehiclesPage() {
  const router = useRouter()
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')

  const { data, isLoading, isError } = useCars({ search: debouncedSearch, pageSize: 50 })
  const cars = data ? [...data.items] : []

  const handleSearchChange = (value: string) => {
    setSearch(value)
    clearTimeout((handleSearchChange as { _t?: ReturnType<typeof setTimeout> })._t)
    ;(handleSearchChange as { _t?: ReturnType<typeof setTimeout> })._t = setTimeout(
      () => setDebouncedSearch(value),
      300
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Vehicles</h1>
        <p className="text-muted-foreground">All registered vehicles across customers</p>
      </div>

      <div className="relative max-w-sm">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          className="pl-9"
          placeholder="Search by plate number…"
          value={search}
          onChange={(e) => handleSearchChange(e.target.value)}
        />
      </div>

      {isLoading && (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </div>
      )}

      {isError && (
        <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
          Failed to load vehicles.
        </div>
      )}

      {!isLoading && !isError && cars.length === 0 && (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed p-16 text-center gap-3">
          <Car className="h-10 w-10 text-muted-foreground/40" />
          <div>
            <p className="font-medium">No vehicles found</p>
            <p className="text-sm text-muted-foreground">
              {debouncedSearch ? 'Try a different plate number' : 'Vehicles are registered through customer profiles'}
            </p>
          </div>
        </div>
      )}

      {!isLoading && cars.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left font-medium">Plate</th>
                <th className="px-4 py-3 text-left font-medium">Type / Size</th>
                <th className="px-4 py-3 text-left font-medium">Make / Model</th>
                <th className="px-4 py-3 text-left font-medium">Color / Year</th>
                <th className="px-4 py-3 text-left font-medium">Owner</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {cars.map((car) => (
                <tr
                  key={car.id}
                  className="hover:bg-muted/40 cursor-pointer transition-colors"
                  onClick={() => router.push(`/dashboard/vehicles/${car.id}`)}
                >
                  <td className="px-4 py-3 font-mono font-semibold text-sm">{car.plateNumber}</td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {car.vehicleTypeName} · {car.sizeName}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {car.makeName
                      ? `${car.makeName}${car.modelName ? ` ${car.modelName}` : ''}`
                      : '—'}
                  </td>
                  <td className="px-4 py-3 text-muted-foreground">
                    {[car.color, car.year].filter(Boolean).join(' · ') || '—'}
                  </td>
                  <td className="px-4 py-3">
                    {car.customerFullName ? (
                      <span
                        className="text-primary hover:underline"
                        onClick={(e) => {
                          e.stopPropagation()
                          router.push(`/dashboard/customers/${car.customerId}`)
                        }}
                      >
                        {car.customerFullName}
                      </span>
                    ) : (
                      <span className="text-muted-foreground italic">Walk-in</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {data && data.totalCount > cars.length && (
            <p className="px-4 py-3 text-xs text-center text-muted-foreground border-t">
              Showing {cars.length} of {data.totalCount} vehicles
            </p>
          )}
        </div>
      )}
    </div>
  )
}
