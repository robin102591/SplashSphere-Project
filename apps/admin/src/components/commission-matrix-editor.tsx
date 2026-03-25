'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useUpsertServiceCommissions } from '@/hooks/use-services'
import type { VehicleType, Size, ServiceCommissionRow } from '@splashsphere/types'
import { CommissionType } from '@splashsphere/types'
import { cn } from '@/lib/utils'
import { toast } from 'sonner'
import { Save } from 'lucide-react'

// ── Types ─────────────────────────────────────────────────────────────────────

interface CommissionCell {
  type: CommissionType
  fixedAmount: string
  percentageRate: string
}

type CommissionMatrix = Record<string, CommissionCell>

function matrixKey(vtId: string, sizeId: string) {
  return `${vtId}|${sizeId}`
}

const DEFAULT_CELL: CommissionCell = {
  type: CommissionType.Percentage,
  fixedAmount: '',
  percentageRate: '',
}

function buildInitialMatrix(rows: readonly ServiceCommissionRow[]): CommissionMatrix {
  const matrix: CommissionMatrix = {}
  for (const row of rows) {
    matrix[matrixKey(row.vehicleTypeId, row.sizeId)] = {
      type: row.type,
      fixedAmount: row.fixedAmount != null ? String(row.fixedAmount) : '',
      percentageRate: row.percentageRate != null ? String(row.percentageRate) : '',
    }
  }
  return matrix
}

const TYPE_LABELS: Record<CommissionType, string> = {
  [CommissionType.Percentage]: 'Percentage',
  [CommissionType.FixedAmount]: 'Fixed ₱',
  [CommissionType.Hybrid]: 'Hybrid',
}

// ── Cell component ────────────────────────────────────────────────────────────

interface CommissionCellEditorProps {
  cell: CommissionCell | undefined
  onChange: (cell: CommissionCell) => void
}

function CommissionCellEditor({ cell, onChange }: CommissionCellEditorProps) {
  const current = cell ?? DEFAULT_CELL

  const setType = (type: CommissionType) =>
    onChange({ ...current, type })

  const setFixed = (fixedAmount: string) =>
    onChange({ ...current, fixedAmount })

  const setRate = (percentageRate: string) =>
    onChange({ ...current, percentageRate })

  const isEmpty =
    !current.fixedAmount &&
    !current.percentageRate &&
    current.type === CommissionType.Percentage

  return (
    <div className={cn('space-y-1 p-1.5 rounded-md min-w-[140px]', isEmpty && 'opacity-50')}>
      {/* Type selector */}
      <select
        value={current.type}
        onChange={(e) => setType(Number(e.target.value) as CommissionType)}
        className="w-full text-xs rounded border border-input bg-background px-2 py-1 focus:outline-none focus:ring-1 focus:ring-ring"
      >
        {Object.entries(TYPE_LABELS).map(([val, label]) => (
          <option key={val} value={val}>
            {label}
          </option>
        ))}
      </select>

      {/* Fixed amount input */}
      {(current.type === CommissionType.FixedAmount ||
        current.type === CommissionType.Hybrid) && (
        <div className="relative">
          <span className="absolute left-2 top-1/2 -translate-y-1/2 text-muted-foreground text-xs select-none">
            ₱
          </span>
          <input
            type="number"
            min="0"
            step="0.01"
            value={current.fixedAmount}
            placeholder="0.00"
            onChange={(e) => setFixed(e.target.value)}
            className="w-full pl-5 pr-2 py-1 text-xs text-right rounded border border-input bg-background focus:outline-none focus:ring-1 focus:ring-ring"
          />
        </div>
      )}

      {/* Percentage rate input */}
      {(current.type === CommissionType.Percentage ||
        current.type === CommissionType.Hybrid) && (
        <div className="relative">
          <input
            type="number"
            min="0"
            max="100"
            step="0.01"
            value={current.percentageRate}
            placeholder="0.00"
            onChange={(e) => setRate(e.target.value)}
            className="w-full pl-2 pr-6 py-1 text-xs text-right rounded border border-input bg-background focus:outline-none focus:ring-1 focus:ring-ring"
          />
          <span className="absolute right-2 top-1/2 -translate-y-1/2 text-muted-foreground text-xs select-none">
            %
          </span>
        </div>
      )}
    </div>
  )
}

// ── Main component ────────────────────────────────────────────────────────────

interface CommissionMatrixEditorProps {
  serviceId: string
  vehicleTypes: VehicleType[]
  sizes: Size[]
  initialRows: readonly ServiceCommissionRow[]
  isLoading?: boolean
}

export function CommissionMatrixEditor({
  serviceId,
  vehicleTypes,
  sizes,
  initialRows,
  isLoading,
}: CommissionMatrixEditorProps) {
  const [matrix, setMatrix] = useState<CommissionMatrix>(() =>
    buildInitialMatrix(initialRows)
  )
  const [isDirty, setIsDirty] = useState(false)

  const { mutateAsync: upsert, isPending } = useUpsertServiceCommissions(serviceId)

  // Sync with server data when initialRows updates
  const [prevRows, setPrevRows] = useState(initialRows)
  if (prevRows !== initialRows) {
    setPrevRows(initialRows)
    setMatrix(buildInitialMatrix(initialRows))
    setIsDirty(false)
  }

  const setCell = (vtId: string, sizeId: string, cell: CommissionCell) => {
    setMatrix((m) => ({ ...m, [matrixKey(vtId, sizeId)]: cell }))
    setIsDirty(true)
  }

  const handleSave = async () => {
    const rows = vehicleTypes.flatMap((vt) =>
      sizes.flatMap((s) => {
        const cell = matrix[matrixKey(vt.id, s.id)]
        if (!cell) return []

        const fixedAmount =
          cell.type !== CommissionType.Percentage && cell.fixedAmount
            ? parseFloat(cell.fixedAmount)
            : null
        const percentageRate =
          cell.type !== CommissionType.FixedAmount && cell.percentageRate
            ? parseFloat(cell.percentageRate)
            : null

        // Skip completely empty cells
        if (fixedAmount == null && percentageRate == null) return []
        if (
          (fixedAmount != null && isNaN(fixedAmount)) ||
          (percentageRate != null && isNaN(percentageRate))
        )
          return []

        return [
          {
            vehicleTypeId: vt.id,
            sizeId: s.id,
            type: cell.type,
            fixedAmount,
            percentageRate,
          },
        ]
      })
    )

    try {
      await upsert(rows)
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
          <Skeleton key={i} className="h-16 w-full" />
        ))}
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <p className="text-sm text-muted-foreground">
            Commission is split equally among all assigned employees.
          </p>
          {isDirty && (
            <span className="flex items-center gap-1.5 text-xs text-amber-600 font-medium">
              <span className="h-1.5 w-1.5 rounded-full bg-amber-500" />
              Unsaved changes
            </span>
          )}
        </div>
        <Button size="sm" onClick={handleSave} disabled={!isDirty || isPending}>
          <Save className="mr-2 h-3.5 w-3.5" />
          {isPending ? 'Saving…' : 'Save Commissions'}
        </Button>
      </div>

      <div className="overflow-x-auto rounded-lg border">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-muted/30">
              <th className="px-4 py-2.5 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground w-36">
                Vehicle Type
              </th>
              {sizes.map((s) => (
                <th
                  key={s.id}
                  className="px-2 py-2.5 text-center text-xs font-medium uppercase tracking-wider text-muted-foreground min-w-[160px]"
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
                <td className="px-4 py-2 font-medium align-top pt-3 bg-muted/30">{vt.name}</td>
                {sizes.map((s) => (
                  <td key={s.id} className="px-1 py-1 align-top">
                    <CommissionCellEditor
                      cell={matrix[matrixKey(vt.id, s.id)]}
                      onChange={(cell) => setCell(vt.id, s.id, cell)}
                    />
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="text-xs text-muted-foreground space-y-0.5">
        <p>
          <span className="font-medium">Percentage</span> — commission = price × rate%
        </p>
        <p>
          <span className="font-medium">Fixed ₱</span> — flat amount regardless of price
        </p>
        <p>
          <span className="font-medium">Hybrid</span> — fixed amount + percentage of price
        </p>
        <p className="pt-1">Leave a cell empty to assign ₱0 commission for that combination.</p>
      </div>
    </div>
  )
}
