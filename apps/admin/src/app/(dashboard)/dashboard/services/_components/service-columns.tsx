'use client'

import type { ColumnDef } from '@tanstack/react-table'
import type { ServiceSummary } from '@splashsphere/types'
import { Badge } from '@/components/ui/badge'
import { StatusBadge } from '@/components/ui/status-badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { MoreHorizontal, Eye, Power, PowerOff } from 'lucide-react'
import Link from 'next/link'
import { formatPeso } from '@/lib/format'

interface ColumnActions {
  onToggleStatus: (id: string) => void
}

export function getServiceColumns({ onToggleStatus }: ColumnActions): ColumnDef<ServiceSummary>[] {
  return [
    {
      accessorKey: 'name',
      header: 'Service',
      cell: ({ row }) => (
        <Link
          href={`/dashboard/services/${row.original.id}`}
          className="font-medium hover:underline"
        >
          {row.original.name}
        </Link>
      ),
    },
    {
      accessorKey: 'categoryName',
      header: 'Category',
      cell: ({ row }) => (
        <Badge variant="outline" className="text-xs">
          {row.original.categoryName}
        </Badge>
      ),
    },
    {
      accessorKey: 'basePrice',
      header: 'Base Price',
      cell: ({ row }) => (
        <span className="font-mono text-sm">{formatPeso(row.original.basePrice)}</span>
      ),
    },
    {
      accessorKey: 'isActive',
      header: 'Status',
      cell: ({ row }) => (
        <StatusBadge status={row.original.isActive ? 'Active' : 'Inactive'} />
      ),
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const svc = row.original
        return (
          <DropdownMenu>
            <DropdownMenuTrigger render={<Button variant="ghost" size="icon" className="h-8 w-8" />}>
              <MoreHorizontal className="h-4 w-4" />
              <span className="sr-only">Open menu</span>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem>
                <Link href={`/dashboard/services/${svc.id}`} className="flex items-center w-full">
                  <Eye className="mr-2 h-4 w-4" />
                  View / Edit
                </Link>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={() => onToggleStatus(svc.id)}>
                {svc.isActive ? (
                  <>
                    <PowerOff className="mr-2 h-4 w-4" />
                    Deactivate
                  </>
                ) : (
                  <>
                    <Power className="mr-2 h-4 w-4" />
                    Activate
                  </>
                )}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )
      },
    },
  ]
}
