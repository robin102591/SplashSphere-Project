'use client'

import { useState, useEffect } from 'react'
import { Plus, Pencil, Trash2, Tags } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Textarea } from '@/components/ui/textarea'
import { DatePicker } from '@/components/ui/date-picker'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import {
  useExpenses, useExpenseCategories, useRecordExpense, useUpdateExpense,
  useDeleteExpense, useCreateExpenseCategory,
} from '@/hooks/use-expenses'
import { useBranches } from '@/hooks/use-branches'
import { formatPeso } from '@/lib/format'
import { toast } from 'sonner'
import { ExpenseFrequency } from '@splashsphere/types'
import type { ExpenseDto } from '@splashsphere/types'

function dateStr(d: Date) { return d.toISOString().split('T')[0] }

export default function ExpensesPage() {
  const defaults = (() => {
    const to = new Date(); const from = new Date(); from.setDate(to.getDate() - 29)
    return { from: dateStr(from), to: dateStr(to) }
  })()

  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [branchId, setBranchId] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [editExpense, setEditExpense] = useState<ExpenseDto | null>(null)
  const [categoriesOpen, setCategoriesOpen] = useState(false)

  const { data: branches } = useBranches()
  const { data: categories } = useExpenseCategories()
  const { data, isLoading } = useExpenses({
    branchId: branchId || undefined,
    categoryId: categoryId || undefined,
    from: from || undefined,
    to: to || undefined,
    pageSize: 100,
  })
  const { mutate: deleteExpense } = useDeleteExpense()

  const totalAmount = data?.items.reduce((sum, e) => sum + e.amount, 0) ?? 0

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Expenses</h1>
          <p className="text-sm text-muted-foreground">Track operating costs and overhead</p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => setCategoriesOpen(true)}>
            <Tags className="mr-2 h-4 w-4" /> Categories
          </Button>
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="mr-2 h-4 w-4" /> Record Expense
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-3">
        <DatePicker value={from} onChange={setFrom} placeholder="From" className="w-[150px]" />
        <span className="text-muted-foreground text-sm">to</span>
        <DatePicker value={to} onChange={setTo} placeholder="To" className="w-[150px]" />
        <Select value={branchId || 'all'} onValueChange={(v) => setBranchId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Branches" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Branches</SelectItem>
            {branches?.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
          </SelectContent>
        </Select>
        <Select value={categoryId || 'all'} onValueChange={(v) => setCategoryId(v === 'all' ? '' : v)}>
          <SelectTrigger className="w-[180px] h-9"><SelectValue placeholder="All Categories" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Categories</SelectItem>
            {categories?.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
          </SelectContent>
        </Select>
        {data && (
          <div className="ml-auto text-sm font-medium tabular-nums">
            Total: {formatPeso(totalAmount)}
          </div>
        )}
      </div>

      {/* Table */}
      {isLoading ? (
        <Skeleton className="h-96 w-full" />
      ) : data && data.items.length > 0 ? (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Date</th>
                <th className="px-4 py-2.5 text-left font-medium">Category</th>
                <th className="px-4 py-2.5 text-left font-medium">Description</th>
                <th className="px-4 py-2.5 text-left font-medium">Branch</th>
                <th className="px-4 py-2.5 text-left font-medium">Vendor</th>
                <th className="px-4 py-2.5 text-right font-medium">Amount</th>
                <th className="px-4 py-2.5 text-right font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {data.items.map((e) => (
                <tr key={e.id} className="hover:bg-muted/30">
                  <td className="px-4 py-2 text-muted-foreground whitespace-nowrap">
                    {new Date(e.expenseDate).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' })}
                  </td>
                  <td className="px-4 py-2">{e.categoryName}</td>
                  <td className="px-4 py-2 max-w-[200px] truncate">{e.description}</td>
                  <td className="px-4 py-2 text-muted-foreground">{e.branchName}</td>
                  <td className="px-4 py-2 text-muted-foreground">{e.vendor ?? '—'}</td>
                  <td className="px-4 py-2 text-right font-medium tabular-nums">{formatPeso(e.amount)}</td>
                  <td className="px-4 py-2 text-right space-x-1">
                    <Button variant="ghost" size="sm" className="h-7 text-xs"
                      onClick={() => setEditExpense(e)}>
                      <Pencil className="h-3 w-3" />
                    </Button>
                    <Button variant="ghost" size="sm" className="h-7 text-xs text-destructive"
                      onClick={() => { if (confirm('Delete this expense?')) deleteExpense(e.id, { onSuccess: () => toast.success('Deleted.') }) }}>
                      <Trash2 className="h-3 w-3" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="flex h-48 items-center justify-center rounded-lg border border-dashed text-sm text-muted-foreground">
          No expenses found for this period.
        </div>
      )}

      <RecordExpenseDialog open={createOpen} onOpenChange={setCreateOpen} />
      <EditExpenseDialog expense={editExpense} onOpenChange={(v) => { if (!v) setEditExpense(null) }} />
      <ManageCategoriesDialog open={categoriesOpen} onOpenChange={setCategoriesOpen} />
    </div>
  )
}

// ── Record Expense Dialog ────────────────────────────────────────────────────

function RecordExpenseDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (v: boolean) => void }) {
  const { data: branches } = useBranches()
  const { data: categories } = useExpenseCategories()
  const { mutate: record, isPending } = useRecordExpense()

  const [branchId, setBranchId] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [amount, setAmount] = useState('')
  const [description, setDescription] = useState('')
  const [vendor, setVendor] = useState('')
  const [expenseDate, setExpenseDate] = useState(new Date().toISOString().split('T')[0])
  const [frequency, setFrequency] = useState<string>(String(ExpenseFrequency.OneTime))
  const [isRecurring, setIsRecurring] = useState(false)

  const handleSubmit = () => {
    if (!branchId || !categoryId || !amount || !description) return
    record({
      branchId, categoryId, amount: parseFloat(amount), description,
      expenseDate: new Date(expenseDate).toISOString(), vendor: vendor || undefined,
      frequency: Number(frequency), isRecurring,
    }, {
      onSuccess: () => {
        toast.success('Expense recorded')
        onOpenChange(false)
        setBranchId(''); setCategoryId(''); setAmount(''); setDescription(''); setVendor('')
        setFrequency(String(ExpenseFrequency.OneTime)); setIsRecurring(false)
      },
      onError: () => toast.error('Failed to record expense.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Record Expense</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Amount (₱)</Label>
            <Input type="number" min="0" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)}
              placeholder="0.00" className="text-lg font-bold" autoFocus />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Category</Label>
              <Select value={categoryId} onValueChange={setCategoryId}>
                <SelectTrigger><SelectValue placeholder="Select" /></SelectTrigger>
                <SelectContent>
                  {categories?.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Branch</Label>
              <Select value={branchId} onValueChange={setBranchId}>
                <SelectTrigger><SelectValue placeholder="Select" /></SelectTrigger>
                <SelectContent>
                  {branches?.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Description</Label>
            <Textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={2} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Date</Label>
              <Input type="date" value={expenseDate} onChange={(e) => setExpenseDate(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Vendor (optional)</Label>
              <Input value={vendor} onChange={(e) => setVendor(e.target.value)} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Frequency</Label>
              <Select value={frequency} onValueChange={(v) => {
                setFrequency(v)
                setIsRecurring(v !== String(ExpenseFrequency.OneTime))
              }}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value={String(ExpenseFrequency.OneTime)}>One-time</SelectItem>
                  <SelectItem value={String(ExpenseFrequency.Daily)}>Daily</SelectItem>
                  <SelectItem value={String(ExpenseFrequency.Weekly)}>Weekly</SelectItem>
                  <SelectItem value={String(ExpenseFrequency.Monthly)}>Monthly</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !branchId || !categoryId || !amount}>
            {isPending ? 'Saving...' : 'Record'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Edit Expense Dialog ──────────────────────────────────────────────────────

function EditExpenseDialog({ expense, onOpenChange }: { expense: ExpenseDto | null; onOpenChange: (v: boolean) => void }) {
  const { data: categories } = useExpenseCategories()
  const { mutate: update, isPending } = useUpdateExpense()

  const [categoryId, setCategoryId] = useState('')
  const [amount, setAmount] = useState('')
  const [description, setDescription] = useState('')
  const [vendor, setVendor] = useState('')
  const [expenseDate, setExpenseDate] = useState('')
  const [frequency, setFrequency] = useState<string>(String(ExpenseFrequency.OneTime))

  useEffect(() => {
    if (expense) {
      setCategoryId(categories?.find((c) => c.name === expense.categoryName)?.id ?? '')
      setAmount(String(expense.amount))
      setDescription(expense.description)
      setVendor(expense.vendor ?? '')
      setExpenseDate(expense.expenseDate.split('T')[0])
      setFrequency(String(expense.frequency))
    }
  }, [expense, categories])

  const handleSubmit = () => {
    if (!expense || !categoryId || !amount || !description) return
    update({
      id: expense.id, categoryId, amount: parseFloat(amount), description,
      expenseDate: new Date(expenseDate).toISOString(),
      vendor: vendor || undefined,
      frequency: Number(frequency),
      isRecurring: Number(frequency) !== ExpenseFrequency.OneTime,
    }, {
      onSuccess: () => {
        toast.success('Expense updated')
        onOpenChange(false)
      },
      onError: () => toast.error('Failed to update expense.'),
    })
  }

  return (
    <Dialog open={!!expense} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Edit Expense</DialogTitle>
        </DialogHeader>
        <div className="space-y-3 py-2">
          <div className="space-y-1.5">
            <Label>Amount (₱)</Label>
            <Input type="number" min="0" step="0.01" value={amount} onChange={(e) => setAmount(e.target.value)}
              placeholder="0.00" className="text-lg font-bold" autoFocus />
          </div>
          <div className="space-y-1.5">
            <Label>Category</Label>
            <Select value={categoryId} onValueChange={setCategoryId}>
              <SelectTrigger><SelectValue placeholder="Select" /></SelectTrigger>
              <SelectContent>
                {categories?.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-1.5">
            <Label>Description</Label>
            <Textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={2} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Date</Label>
              <Input type="date" value={expenseDate} onChange={(e) => setExpenseDate(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>Vendor (optional)</Label>
              <Input value={vendor} onChange={(e) => setVendor(e.target.value)} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>Frequency</Label>
              <Select value={frequency} onValueChange={setFrequency}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value={String(ExpenseFrequency.OneTime)}>One-time</SelectItem>
                  <SelectItem value={String(ExpenseFrequency.Daily)}>Daily</SelectItem>
                  <SelectItem value={String(ExpenseFrequency.Weekly)}>Weekly</SelectItem>
                  <SelectItem value={String(ExpenseFrequency.Monthly)}>Monthly</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleSubmit} disabled={isPending || !categoryId || !amount}>
            {isPending ? 'Saving...' : 'Save Changes'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Manage Categories Dialog ─────────────────────────────────────────────────

function ManageCategoriesDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (v: boolean) => void }) {
  const { data: categories, isLoading } = useExpenseCategories()
  const { mutate: createCategory, isPending } = useCreateExpenseCategory()
  const [newName, setNewName] = useState('')

  const handleCreate = () => {
    const name = newName.trim()
    if (!name) return
    createCategory({ name }, {
      onSuccess: () => {
        toast.success(`Category "${name}" created`)
        setNewName('')
      },
      onError: () => toast.error('Failed to create category.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Expense Categories</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-2">
          <div className="flex gap-2">
            <Input placeholder="New category name" value={newName} onChange={(e) => setNewName(e.target.value)}
              onKeyDown={(e) => { if (e.key === 'Enter') handleCreate() }} />
            <Button size="sm" onClick={handleCreate} disabled={isPending || !newName.trim()}>
              {isPending ? '...' : 'Add'}
            </Button>
          </div>
          {isLoading ? (
            <Skeleton className="h-32 w-full" />
          ) : categories && categories.length > 0 ? (
            <ul className="space-y-1 max-h-64 overflow-y-auto">
              {categories.map((c) => (
                <li key={c.id} className="flex items-center gap-2 rounded-md px-3 py-2 text-sm bg-muted/50">
                  <span className="flex-1">{c.name}</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-sm text-muted-foreground text-center py-4">No categories yet.</p>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Done</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
