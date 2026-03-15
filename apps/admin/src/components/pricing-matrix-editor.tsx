'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useUpsertServicePricing } from '@/hooks/use-services'
import type { VehicleType, Size, ServicePricingRow } from '@splashsphere/types'
import { cn } from '@/lib/utils'
import { toast } from 'sonner'
import { Save } from 'lucide-react'

// matrix state: key = `${vehicleTypeId}|${sizeId}`, value = price as string
type PricingMatrix = Record<string, string>

function matrixKey(vtId: string, sizeId: string) {
  return `${vtId}|${sizeId}`
}

function buildInitialMatrix(rows: readonly ServicePricingRow[]): PricingMatrix {
  const matrix: PricingMatrix = {}
  for (const row of rows) {
    matrix[matrixKey(row.vehicleTypeId, row.sizeId)] = String(row.price)
  }
  return matrix
}

function formatPHP(value: string): string {
  const n = parseFloat(value)
  if (isNaN(n)) return ''
  return new Intl.NumberFormat('en-PH', {
    style: 'currency',
    currency: 'PHP',
    minimumFractionDigits: 0,
  }).format(n)
}

interface PricingMatrixEditorProps {
  serviceId: string
  vehicleTypes: VehicleType[]
  sizes: Size[]
  initialRows: readonly ServicePricingRow[]
  basePrice: number
  isLoading?: boolean
}

export function PricingMatrixEditor({
  serviceId,
  vehicleTypes,
  sizes,
  initialRows,
  basePrice,
  isLoading,
}: PricingMatrixEditorProps) {
  const [matrix, setMatrix] = useState<PricingMatrix>(() =>
    buildInitialMatrix(initialRows)
  )
  const [isDirty, setIsDirty] = useState(false)

  const { mutateAsync: upsert, isPending } = useUpsertServicePricing(serviceId)

  // Rebuild local state when initialRows change (e.g. after a successful save)
  const [prevRows, setPrevRows] = useState(initialRows)
  if (prevRows !== initialRows) {
    setPrevRows(initialRows)
    setMatrix(buildInitialMatrix(initialRows))
    setIsDirty(false)
  }

  const setCell = (vtId: string, sizeId: string, value: string) => {
    setMatrix((m) => ({ ...m, [matrixKey(vtId, sizeId)]: value }))
    setIsDirty(true)
  }

  const handleSave = async () => {
    const rows = vehicleTypes.flatMap((vt) =>
      sizes.flatMap((s) => {
        const raw = matrix[matrixKey(vt.id, s.id)] ?? ''
        const price = parseFloat(raw)
        if (!raw || isNaN(price) || price < 0) return []
        return [{ vehicleTypeId: vt.id, sizeId: s.id, price }]
      })
    )
    try {
      await upsert(rows)
      toast.success('Pricing matrix saved')
      setIsDirty(false)
    } catch {
      toast.error('Failed to save pricing matrix')
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-10 w-full" />
        ))}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          Base price (fallback for empty cells):{' '}
          <span className="font-medium text-foreground">{formatPHP(String(basePrice))}</span>
        </p>
        <Button size="sm" onClick={handleSave} disabled={!isDirty || isPending}>
          <Save className="mr-2 h-3.5 w-3.5" />
          {isPending ? 'Saving…' : 'Save Pricing'}
        </Button>
      </div>

      <div className="overflow-x-auto rounded-lg border">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-muted/50">
              <th className="px-4 py-2.5 text-left font-medium text-muted-foreground w-36">
                Vehicle Type
              </th>
              {sizes.map((s) => (
                <th
                  key={s.id}
                  className="px-4 py-2.5 text-center font-medium text-muted-foreground min-w-28"
                >
                  {s.name}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {vehicleTypes.map((vt, vtIdx) => (
              <tr
                key={vt.id}
                className={cn('border-t', vtIdx % 2 === 0 ? 'bg-background' : 'bg-muted/20')}
              >
                <td className="px-4 py-2 font-medium">{vt.name}</td>
                {sizes.map((s) => {
                  const key = matrixKey(vt.id, s.id)
                  const val = matrix[key] ?? ''
                  return (
                    <td key={s.id} className="px-2 py-1.5">
                      <div className="relative">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-xs select-none">
                          ₱
                        </span>
                        <input
                          type="number"
                          min="0"
                          step="0.01"
                          value={val}
                          placeholder={String(basePrice)}
                          onChange={(e) => setCell(vt.id, s.id, e.target.value)}
                          className={cn(
                            'w-full pl-6 pr-2 py-1.5 rounded-md border text-right text-sm',
                            'bg-background focus:outline-none focus:ring-2 focus:ring-ring focus:border-transparent',
                            val ? 'border-input' : 'border-dashed border-muted-foreground/30'
                          )}
                        />
                      </div>
                    </td>
                  )
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <p className="text-xs text-muted-foreground">
        Leave a cell empty to use the base price for that combination. Changes are not saved until
        you click &ldquo;Save Pricing&rdquo;.
      </p>
    </div>
  )
}
