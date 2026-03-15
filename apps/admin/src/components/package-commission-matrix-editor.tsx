'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import type { VehicleType, Size, PackageCommissionRow } from '@splashsphere/types'
import { cn } from '@/lib/utils'
import { toast } from 'sonner'
import { Save } from 'lucide-react'
import type { PackageCommissionRowPayload } from '@/hooks/use-packages'

// matrix state: key = `${vehicleTypeId}|${sizeId}`, value = rate as string
type CommissionMatrix = Record<string, string>

function matrixKey(vtId: string, sizeId: string) {
  return `${vtId}|${sizeId}`
}

function buildInitialMatrix(rows: readonly PackageCommissionRow[]): CommissionMatrix {
  const matrix: CommissionMatrix = {}
  for (const row of rows) {
    matrix[matrixKey(row.vehicleTypeId, row.sizeId)] = String(row.percentageRate)
  }
  return matrix
}

interface PackageCommissionMatrixEditorProps {
  vehicleTypes: VehicleType[]
  sizes: Size[]
  initialRows: readonly PackageCommissionRow[]
  onSave: (rows: PackageCommissionRowPayload[]) => Promise<void>
  isSaving: boolean
  isLoading?: boolean
}

export function PackageCommissionMatrixEditor({
  vehicleTypes,
  sizes,
  initialRows,
  onSave,
  isSaving,
  isLoading,
}: PackageCommissionMatrixEditorProps) {
  const [matrix, setMatrix] = useState<CommissionMatrix>(() =>
    buildInitialMatrix(initialRows)
  )
  const [isDirty, setIsDirty] = useState(false)

  // Sync when server data refreshes after a save
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
        const rate = parseFloat(raw)
        if (!raw || isNaN(rate) || rate <= 0 || rate > 100) return []
        return [{ vehicleTypeId: vt.id, sizeId: s.id, percentageRate: rate }]
      })
    )
    try {
      await onSave(rows)
      toast.success('Commission matrix saved')
      setIsDirty(false)
    } catch {
      toast.error('Failed to save commission matrix')
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
          Package commissions are always percentage-based and split equally among assigned
          employees.
        </p>
        <Button size="sm" onClick={handleSave} disabled={!isDirty || isSaving}>
          <Save className="mr-2 h-3.5 w-3.5" />
          {isSaving ? 'Saving…' : 'Save Commissions'}
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
                        <input
                          type="number"
                          min="0"
                          max="100"
                          step="0.01"
                          value={val}
                          placeholder="0"
                          onChange={(e) => setCell(vt.id, s.id, e.target.value)}
                          className={cn(
                            'w-full pl-2 pr-7 py-1.5 rounded-md border text-right text-sm',
                            'bg-background focus:outline-none focus:ring-2 focus:ring-ring focus:border-transparent',
                            val ? 'border-input' : 'border-dashed border-muted-foreground/30'
                          )}
                        />
                        <span className="absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground text-xs select-none">
                          %
                        </span>
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
        Enter percentage rates (0–100). Leave a cell empty to assign ₱0 commission for that
        combination.
      </p>
    </div>
  )
}
