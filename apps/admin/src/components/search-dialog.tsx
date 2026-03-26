'use client'

import { useCallback, useEffect, useRef, useState } from 'react'
import { useRouter } from 'next/navigation'
import {
  Dialog,
  DialogContent,
  DialogTitle,
} from '@/components/ui/dialog'
import { useGlobalSearch } from '@/hooks/use-search'
import type { SearchHit } from '@splashsphere/types'
import {
  Loader2,
  Search,
  Users,
  UserCog,
  Receipt,
  Car,
  Sparkles,
  Package,
} from 'lucide-react'

const categoryConfig: Record<
  string,
  { label: string; icon: typeof Users; route: string }
> = {
  customer: { label: 'Customers', icon: Users, route: '/dashboard/customers' },
  employee: { label: 'Employees', icon: UserCog, route: '/dashboard/employees' },
  transaction: { label: 'Transactions', icon: Receipt, route: '/dashboard/transactions' },
  vehicle: { label: 'Vehicles', icon: Car, route: '/dashboard/vehicles' },
  service: { label: 'Services', icon: Sparkles, route: '/dashboard/services' },
  merchandise: { label: 'Merchandise', icon: Package, route: '/dashboard/merchandise' },
}

interface SearchDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function SearchDialog({ open, onOpenChange }: SearchDialogProps) {
  const [query, setQuery] = useState('')
  const [selectedIndex, setSelectedIndex] = useState(0)
  const inputRef = useRef<HTMLInputElement>(null)
  const listRef = useRef<HTMLDivElement>(null)
  const router = useRouter()
  const { data, isFetching } = useGlobalSearch(query)

  // Build flat list of results grouped by category
  const groups = data
    ? (Object.entries(data) as [string, SearchHit[]][])
        .map(([key, hits]) => ({
          key,
          config: categoryConfig[key === 'customers' ? 'customer'
            : key === 'employees' ? 'employee'
            : key === 'transactions' ? 'transaction'
            : key === 'vehicles' ? 'vehicle'
            : key === 'services' ? 'service'
            : 'merchandise'],
          hits,
        }))
        .filter((g) => g.hits.length > 0)
    : []

  const flatItems = groups.flatMap((g) =>
    g.hits.map((hit) => ({ ...hit, groupKey: g.key, config: g.config })),
  )

  // Reset on open/close
  useEffect(() => {
    if (open) {
      setQuery('')
      setSelectedIndex(0)
      // Focus input after dialog renders
      requestAnimationFrame(() => inputRef.current?.focus())
    }
  }, [open])

  // Reset selection when results change
  useEffect(() => {
    setSelectedIndex(0)
  }, [data])

  // Scroll selected item into view
  useEffect(() => {
    const el = listRef.current?.querySelector('[data-selected="true"]')
    el?.scrollIntoView({ block: 'nearest' })
  }, [selectedIndex])

  const navigate = useCallback(
    (item: (typeof flatItems)[0]) => {
      onOpenChange(false)
      router.push(`${item.config.route}/${item.id}`)
    },
    [onOpenChange, router],
  )

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setSelectedIndex((i) => Math.min(i + 1, flatItems.length - 1))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setSelectedIndex((i) => Math.max(i - 1, 0))
    } else if (e.key === 'Enter' && flatItems[selectedIndex]) {
      e.preventDefault()
      navigate(flatItems[selectedIndex])
    }
  }

  const hasQuery = query.trim().length >= 2
  const noResults = hasQuery && !isFetching && flatItems.length === 0

  let itemIndex = -1

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        showCloseButton={false}
        className="sm:max-w-lg p-0 gap-0 overflow-hidden"
      >
        <DialogTitle className="sr-only">Search</DialogTitle>

        {/* Search input */}
        <div className="flex items-center gap-2 border-b px-3">
          {isFetching ? (
            <Loader2 className="h-4 w-4 shrink-0 text-muted-foreground animate-spin" />
          ) : (
            <Search className="h-4 w-4 shrink-0 text-muted-foreground" />
          )}
          <input
            ref={inputRef}
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Search customers, employees, transactions..."
            className="flex-1 bg-transparent py-3 text-sm outline-none placeholder:text-muted-foreground"
          />
          <kbd className="hidden sm:inline-flex h-5 items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium text-muted-foreground">
            ESC
          </kbd>
        </div>

        {/* Results */}
        <div
          ref={listRef}
          className="max-h-[min(60vh,360px)] overflow-y-auto overscroll-contain"
        >
          {!hasQuery && (
            <div className="px-4 py-8 text-center text-sm text-muted-foreground">
              Type at least 2 characters to search...
            </div>
          )}

          {noResults && (
            <div className="px-4 py-8 text-center text-sm text-muted-foreground">
              No results found for &ldquo;{query.trim()}&rdquo;
            </div>
          )}

          {groups.map((group) => {
            const Icon = group.config.icon
            return (
              <div key={group.key}>
                <div className="sticky top-0 z-10 bg-background/95 backdrop-blur-sm px-3 py-1.5 text-xs font-medium text-muted-foreground flex items-center gap-1.5">
                  <Icon className="h-3.5 w-3.5" />
                  {group.config.label}
                </div>
                {group.hits.map((hit) => {
                  itemIndex++
                  const idx = itemIndex
                  const isSelected = idx === selectedIndex
                  return (
                    <button
                      key={`${group.key}-${hit.id}`}
                      data-selected={isSelected}
                      className={`flex w-full items-center gap-3 px-3 py-2 text-left text-sm transition-colors hover:bg-accent/50 ${
                        isSelected ? 'bg-accent text-accent-foreground' : ''
                      }`}
                      onClick={() => navigate({ ...hit, groupKey: group.key, config: group.config })}
                      onMouseEnter={() => setSelectedIndex(idx)}
                    >
                      <div className="min-w-0 flex-1">
                        <div className="truncate font-medium">{hit.title}</div>
                        {hit.subtitle && (
                          <div className="truncate text-xs text-muted-foreground">
                            {hit.subtitle}
                          </div>
                        )}
                      </div>
                    </button>
                  )
                })}
              </div>
            )
          })}
        </div>

        {/* Footer hints */}
        {flatItems.length > 0 && (
          <div className="flex items-center gap-3 border-t px-3 py-2 text-[11px] text-muted-foreground">
            <span className="flex items-center gap-1">
              <kbd className="rounded border bg-muted px-1 font-mono">↑↓</kbd>
              navigate
            </span>
            <span className="flex items-center gap-1">
              <kbd className="rounded border bg-muted px-1 font-mono">↵</kbd>
              open
            </span>
            <span className="flex items-center gap-1">
              <kbd className="rounded border bg-muted px-1 font-mono">esc</kbd>
              close
            </span>
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}
