'use client'

import type { ColumnDef } from '@tanstack/react-table'
import type { PackageSummary } from '@splashsphere/types'
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

interface ColumnActions {
  onToggleStatus: (id: string) => void
}

export function getPackageColumns({ onToggleStatus }: ColumnActions): ColumnDef<PackageSummary>[] {
  return [
    {
      accessorKey: 'name',
      header: 'Package',
      cell: ({ row }) => (
        <Link
          href={`/dashboard/packages/${row.original.id}`}
          className="font-medium hover:underline"
        >
          {row.original.name}
        </Link>
      ),
    },
    {
      accessorKey: 'description',
      header: 'Description',
      cell: ({ row }) => (
        <span className="text-sm text-muted-foreground">
          {row.original.description ?? '—'}
        </span>
      ),
    },
    {
      accessorKey: 'serviceCount',
      header: 'Services',
      cell: ({ row }) => (
        <Badge variant="outline" className="text-xs">
          {row.original.serviceCount} {row.original.serviceCount === 1 ? 'service' : 'services'}
        </Badge>
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
        const pkg = row.original
        return (
          <DropdownMenu>
            <DropdownMenuTrigger render={<Button variant="ghost" size="icon" className="h-8 w-8" />}>
              <MoreHorizontal className="h-4 w-4" />
              <span className="sr-only">Open menu</span>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem>
                <Link href={`/dashboard/packages/${pkg.id}`} className="flex items-center w-full">
                  <Eye className="mr-2 h-4 w-4" />
                  View / Edit
                </Link>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={() => onToggleStatus(pkg.id)}>
                {pkg.isActive ? (
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
