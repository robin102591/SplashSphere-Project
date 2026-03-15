'use client'

import type { ColumnDef } from '@tanstack/react-table'
import type { Employee } from '@splashsphere/types'
import { EmployeeType } from '@splashsphere/types'
import { Badge } from '@/components/ui/badge'
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

export function getEmployeeColumns({ onToggleStatus }: ColumnActions): ColumnDef<Employee>[] {
  return [
    {
      accessorKey: 'fullName',
      header: 'Employee',
      cell: ({ row }) => (
        <Link
          href={`/dashboard/employees/${row.original.id}`}
          className="font-medium hover:underline"
        >
          {row.original.fullName}
        </Link>
      ),
    },
    {
      accessorKey: 'branchName',
      header: 'Branch',
      cell: ({ row }) => (
        <span className="text-sm text-muted-foreground">{row.original.branchName}</span>
      ),
    },
    {
      accessorKey: 'employeeType',
      header: 'Type',
      cell: ({ row }) =>
        row.original.employeeType === EmployeeType.Commission ? (
          <Badge variant="default" className="bg-blue-500/15 text-blue-700 border-blue-200">
            Commission
          </Badge>
        ) : (
          <Badge variant="outline">Daily Rate</Badge>
        ),
    },
    {
      id: 'rateOrNote',
      header: 'Rate',
      cell: ({ row }) => {
        const emp = row.original
        if (emp.employeeType === EmployeeType.Daily && emp.dailyRate != null) {
          return (
            <span className="text-sm">
              {new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' }).format(
                emp.dailyRate
              )}
              /day
            </span>
          )
        }
        return <span className="text-sm text-muted-foreground">Commission-based</span>
      },
    },
    {
      accessorKey: 'isActive',
      header: 'Status',
      cell: ({ row }) =>
        row.original.isActive ? (
          <Badge variant="default" className="bg-green-500/15 text-green-700 border-green-200">
            Active
          </Badge>
        ) : (
          <Badge variant="secondary">Inactive</Badge>
        ),
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const emp = row.original
        return (
          <DropdownMenu>
            <DropdownMenuTrigger render={<Button variant="ghost" size="icon" className="h-8 w-8" />}>
              <MoreHorizontal className="h-4 w-4" />
              <span className="sr-only">Open menu</span>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem>
                <Link href={`/dashboard/employees/${emp.id}`} className="flex items-center w-full">
                  <Eye className="mr-2 h-4 w-4" />
                  View / Edit
                </Link>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={() => onToggleStatus(emp.id)}>
                {emp.isActive ? (
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
