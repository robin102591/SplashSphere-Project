'use client'

import type { ColumnDef } from '@tanstack/react-table'
import type { Branch } from '@splashsphere/types'
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
  onToggleStatus: (branch: Branch) => void
}

export function getBranchColumns({ onToggleStatus }: ColumnActions): ColumnDef<Branch>[] {
  return [
    {
      accessorKey: 'code',
      header: 'Code',
      cell: ({ row }) => (
        <span className="font-mono text-sm font-medium">{row.original.code}</span>
      ),
    },
    {
      accessorKey: 'name',
      header: 'Branch Name',
      cell: ({ row }) => (
        <Link
          href={`/dashboard/branches/${row.original.id}`}
          className="font-medium hover:underline"
        >
          {row.original.name}
        </Link>
      ),
    },
    {
      accessorKey: 'address',
      header: 'Address',
      cell: ({ row }) => (
        <span className="text-muted-foreground text-sm">{row.original.address}</span>
      ),
    },
    {
      accessorKey: 'contactNumber',
      header: 'Contact',
      cell: ({ row }) => (
        <span className="text-sm">{row.original.contactNumber}</span>
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
        const branch = row.original
        return (
          <DropdownMenu>
            <DropdownMenuTrigger render={<Button variant="ghost" size="icon" className="h-8 w-8" />}>
              <MoreHorizontal className="h-4 w-4" />
              <span className="sr-only">Open menu</span>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem>
                <Link href={`/dashboard/branches/${branch.id}`} className="flex items-center w-full">
                  <Eye className="mr-2 h-4 w-4" />
                  View details
                </Link>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={() => onToggleStatus(branch)}>
                {branch.isActive ? (
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
