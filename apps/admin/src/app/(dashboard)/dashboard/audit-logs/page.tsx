'use client'

import { useState } from 'react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { DatePicker } from '@/components/ui/date-picker'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { useAuditLogs } from '@/hooks/use-audit-logs'
import { formatDateTime } from '@/lib/format'

const ENTITY_TYPES = [
  'Branch', 'Service', 'ServicePackage', 'Employee', 'Customer', 'Car',
  'Transaction', 'PayrollPeriod', 'PayrollEntry', 'CashAdvance',
  'CashierShift', 'Merchandise', 'PricingModifier', 'TenantSubscription',
]

const ACTION_COLORS: Record<string, string> = {
  Create: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
  Update: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
  Delete: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
}

export default function AuditLogsPage() {
  const [entityType, setEntityType] = useState('')
  const [entityId, setEntityId] = useState('')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [page, setPage] = useState(1)
  const [expanded, setExpanded] = useState<string | null>(null)

  const { data, isLoading } = useAuditLogs({
    entityType: entityType || undefined,
    entityId: entityId || undefined,
    from: from || undefined,
    to: to || undefined,
    page,
    pageSize: 30,
  })

  const totalPages = data ? Math.ceil(data.totalCount / 30) : 0

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Audit Logs</h1>
        <p className="text-sm text-muted-foreground">Track all data changes across the system</p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <Select value={entityType || 'all'} onValueChange={(v) => { setEntityType(v === 'all' ? '' : v); setPage(1) }}>
          <SelectTrigger className="w-[180px] h-9">
            <SelectValue placeholder="All Entity Types" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Entity Types</SelectItem>
            {ENTITY_TYPES.map((t) => (
              <SelectItem key={t} value={t}>{t}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Input
          placeholder="Entity ID"
          value={entityId}
          onChange={(e) => { setEntityId(e.target.value); setPage(1) }}
          className="w-[200px] h-9"
        />

        <DatePicker value={from} onChange={(v) => { setFrom(v); setPage(1) }} placeholder="From" className="w-[150px]" />
        <span className="text-muted-foreground text-sm">to</span>
        <DatePicker value={to} onChange={(v) => { setTo(v); setPage(1) }} placeholder="To" className="w-[150px]" />
      </div>

      {/* Table */}
      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 10 }).map((_, i) => <Skeleton key={i} className="h-10 w-full" />)}
        </div>
      ) : data && data.items.length > 0 ? (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Timestamp</th>
                <th className="px-4 py-2.5 text-left font-medium">Action</th>
                <th className="px-4 py-2.5 text-left font-medium">Entity</th>
                <th className="px-4 py-2.5 text-left font-medium">Entity ID</th>
                <th className="px-4 py-2.5 text-left font-medium">User</th>
                <th className="px-4 py-2.5 text-left font-medium">Changes</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {data.items.map((log) => (
                <tr key={log.id} className="hover:bg-muted/30">
                  <td className="px-4 py-2 text-muted-foreground text-xs whitespace-nowrap">
                    {formatDateTime(log.timestamp)}
                  </td>
                  <td className="px-4 py-2">
                    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${ACTION_COLORS[log.action] ?? 'bg-gray-100 text-gray-800'}`}>
                      {log.action}
                    </span>
                  </td>
                  <td className="px-4 py-2 font-medium">{log.entityType}</td>
                  <td className="px-4 py-2 font-mono text-xs text-muted-foreground">{log.entityId.slice(0, 8)}...</td>
                  <td className="px-4 py-2 text-xs text-muted-foreground">{log.userId?.slice(0, 12) ?? 'system'}...</td>
                  <td className="px-4 py-2">
                    {log.changes ? (
                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-6 text-xs"
                        onClick={() => setExpanded(expanded === log.id ? null : log.id)}
                      >
                        {expanded === log.id ? 'Hide' : 'View'}
                      </Button>
                    ) : (
                      <span className="text-xs text-muted-foreground">—</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Expanded changes */}
          {expanded && data.items.find((l) => l.id === expanded)?.changes && (
            <div className="border-t bg-muted/30 p-4">
              <pre className="text-xs font-mono whitespace-pre-wrap overflow-auto max-h-64">
                {JSON.stringify(JSON.parse(data.items.find((l) => l.id === expanded)!.changes!), null, 2)}
              </pre>
            </div>
          )}
        </div>
      ) : (
        <div className="flex h-48 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
          No audit logs found.
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Page {page} of {totalPages} ({data?.totalCount} total)
          </p>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(page - 1)}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage(page + 1)}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
