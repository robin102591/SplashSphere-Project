'use client'

import { useState, useMemo } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import {
  AreaChart, Area, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend,
} from 'recharts'
import { useRevenueReport, useCommissionsReport, useServicePopularityReport } from '@/hooks/use-reports'
import { useBranches } from '@/hooks/use-branches'
import { useEmployees } from '@/hooks/use-employees'
import { formatPeso, formatPesoCompact } from '@/lib/format'
const PIE_COLORS = ['#2563eb', '#16a34a', '#d97706', '#dc2626', '#7c3aed', '#0891b2']

function dateStr(d: Date) { return d.toISOString().split('T')[0] }

function defaultDates() {
  const to = new Date()
  const from = new Date()
  from.setDate(to.getDate() - 29)
  return { from: dateStr(from), to: dateStr(to) }
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border px-4 py-3">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-xl font-bold tabular-nums mt-0.5">{value}</p>
    </div>
  )
}

// ── Revenue Tab ───────────────────────────────────────────────────────────────

function RevenueTab({ from, to, branchId }: { from: string; to: string; branchId?: string }) {
  const { data, isLoading } = useRevenueReport({ from, to, branchId })

  const chartData = data?.dailyBreakdown.map((d) => ({
    date: new Date(d.date).toLocaleDateString('en-PH', { month: 'short', day: 'numeric' }),
    revenue: d.revenue,
    discount: d.discount,
  })) ?? []

  const pieData = data?.byPaymentMethod.map((p) => ({
    name: p.paymentMethod,
    value: p.amount,
  })) ?? []

  if (isLoading) return <Skeleton className="h-96 w-full" />

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <StatCard label="Total Revenue" value={formatPeso(data?.grandTotal ?? 0)} />
        <StatCard label="Total Discount" value={formatPeso(data?.totalDiscount ?? 0)} />
        <StatCard label="Total Tax" value={formatPeso(data?.totalTax ?? 0)} />
        <StatCard label="Transactions" value={String(data?.transactionCount ?? 0)} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card className="lg:col-span-2">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Daily Revenue</CardTitle>
          </CardHeader>
          <CardContent>
            {chartData.length === 0 ? (
              <div className="h-48 flex items-center justify-center text-sm text-muted-foreground">No data for this period</div>
            ) : (
              <ResponsiveContainer width="100%" height={200}>
                <AreaChart data={chartData}>
                  <defs>
                    <linearGradient id="revGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="hsl(var(--chart-1))" stopOpacity={0.25} />
                      <stop offset="95%" stopColor="hsl(var(--chart-1))" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                  <XAxis dataKey="date" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} interval="preserveStartEnd" />
                  <YAxis tick={{ fontSize: 11 }} tickLine={false} axisLine={false} tickFormatter={(v) => formatPesoCompact(v)} />
                  <Tooltip formatter={(v: number, name: string) => [formatPeso(v), name === 'revenue' ? 'Revenue' : 'Discount']} />
                  <Area type="monotone" dataKey="revenue" stroke="hsl(var(--chart-1))" strokeWidth={2} fill="url(#revGrad)" />
                </AreaChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">By Payment Method</CardTitle>
          </CardHeader>
          <CardContent>
            {pieData.length === 0 ? (
              <div className="h-48 flex items-center justify-center text-sm text-muted-foreground">No data</div>
            ) : (
              <ResponsiveContainer width="100%" height={200}>
                <PieChart>
                  <Pie data={pieData} dataKey="value" nameKey="name" cx="50%" cy="42%" outerRadius={65} strokeWidth={1}>
                    {pieData.map((_, i) => <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />)}
                  </Pie>
                  <Tooltip formatter={(v: number) => formatPeso(v)} />
                  <Legend iconSize={10} wrapperStyle={{ fontSize: 11 }} />
                </PieChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Daily breakdown table */}
      {chartData.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Date</th>
                <th className="px-4 py-2.5 text-right font-medium">Revenue</th>
                <th className="px-4 py-2.5 text-right font-medium">Discount</th>
                <th className="px-4 py-2.5 text-right font-medium">Tax</th>
                <th className="px-4 py-2.5 text-right font-medium">Transactions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {data?.dailyBreakdown.map((d) => (
                <tr key={d.date} className="hover:bg-muted/30">
                  <td className="px-4 py-2">{new Date(d.date).toLocaleDateString('en-PH', { weekday: 'short', month: 'short', day: 'numeric' })}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{formatPeso(d.revenue)}</td>
                  <td className="px-4 py-2 text-right tabular-nums text-green-700">{d.discount > 0 ? `−${formatPeso(d.discount)}` : '—'}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{d.tax > 0 ? formatPeso(d.tax) : '—'}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{d.transactionCount}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

// ── Commissions Tab ───────────────────────────────────────────────────────────

function CommissionsTab({
  from, to, branchId, employeeId,
}: { from: string; to: string; branchId?: string; employeeId?: string }) {
  const { data, isLoading } = useCommissionsReport({ from, to, branchId, employeeId })

  const barData = data?.employees.slice(0, 15).map((e) => ({
    name: e.employeeName.length > 18 ? e.employeeName.slice(0, 16) + '…' : e.employeeName,
    commission: e.totalCommissions,
    transactions: e.transactionCount,
  })) ?? []

  if (isLoading) return <Skeleton className="h-96 w-full" />

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-2 gap-4">
        <StatCard label="Total Commissions" value={formatPeso(data?.grandTotalCommissions ?? 0)} />
        <StatCard label="Transactions" value={String(data?.transactionCount ?? 0)} />
      </div>

      {barData.length > 0 && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Top Earners</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={Math.max(200, barData.length * 28)}>
              <BarChart data={barData} layout="vertical" margin={{ left: 8 }}>
                <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="hsl(var(--border))" />
                <XAxis type="number" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} tickFormatter={(v) => formatPesoCompact(v)} />
                <YAxis type="category" dataKey="name" width={130} tick={{ fontSize: 11 }} tickLine={false} axisLine={false} />
                <Tooltip formatter={(v: number) => [formatPeso(v), 'Commission']} />
                <Bar dataKey="commission" fill="hsl(var(--chart-5))" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {data && data.employees.length > 0 && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Employee</th>
                <th className="px-4 py-2.5 text-left font-medium">Branch</th>
                <th className="px-4 py-2.5 text-left font-medium">Type</th>
                <th className="px-4 py-2.5 text-right font-medium">Transactions</th>
                <th className="px-4 py-2.5 text-right font-medium">Commission</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {data.employees.map((e) => (
                <tr key={e.employeeId} className="hover:bg-muted/30">
                  <td className="px-4 py-2 font-medium">{e.employeeName}</td>
                  <td className="px-4 py-2 text-muted-foreground">{e.branchName}</td>
                  <td className="px-4 py-2 text-muted-foreground">{e.employeeType}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{e.transactionCount}</td>
                  <td className="px-4 py-2 text-right tabular-nums font-semibold">{formatPeso(e.totalCommissions)}</td>
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr className="bg-muted/50 font-semibold">
                <td className="px-4 py-2.5" colSpan={3}>Total</td>
                <td className="px-4 py-2.5 text-right tabular-nums">{data.transactionCount}</td>
                <td className="px-4 py-2.5 text-right tabular-nums">{formatPeso(data.grandTotalCommissions)}</td>
              </tr>
            </tfoot>
          </table>
        </div>
      )}

      {data?.employees.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center text-sm text-muted-foreground">
          No commission data for this period
        </div>
      )}
    </div>
  )
}

// ── Service Popularity Tab ────────────────────────────────────────────────────

function ServicePopularityTab({ from, to, branchId }: { from: string; to: string; branchId?: string }) {
  const { data, isLoading } = useServicePopularityReport({ from, to, branchId })

  const serviceBar = data?.services.map((s) => ({
    name: s.serviceName.length > 22 ? s.serviceName.slice(0, 20) + '…' : s.serviceName,
    count: s.timesPerformed,
    revenue: s.totalRevenue,
  })) ?? []

  const pkgBar = data?.packages.map((p) => ({
    name: p.packageName.length > 22 ? p.packageName.slice(0, 20) + '…' : p.packageName,
    count: p.timesPerformed,
    revenue: p.totalRevenue,
  })) ?? []

  if (isLoading) return <Skeleton className="h-96 w-full" />

  return (
    <div className="space-y-6">
      {serviceBar.length > 0 && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Services</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={Math.max(180, serviceBar.length * 28)}>
              <BarChart data={serviceBar} layout="vertical" margin={{ left: 8 }}>
                <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="hsl(var(--border))" />
                <XAxis type="number" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} />
                <YAxis type="category" dataKey="name" width={140} tick={{ fontSize: 11 }} tickLine={false} axisLine={false} />
                <Tooltip />
                <Bar dataKey="count" name="Times Performed" fill="hsl(var(--chart-1))" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {pkgBar.length > 0 && (
        <Card>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">Packages</CardTitle>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={Math.max(160, pkgBar.length * 28)}>
              <BarChart data={pkgBar} layout="vertical" margin={{ left: 8 }}>
                <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="hsl(var(--border))" />
                <XAxis type="number" tick={{ fontSize: 11 }} tickLine={false} axisLine={false} />
                <YAxis type="category" dataKey="name" width={140} tick={{ fontSize: 11 }} tickLine={false} axisLine={false} />
                <Tooltip />
                <Bar dataKey="count" name="Times Performed" fill="hsl(var(--chart-3))" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}

      {/* Combined table */}
      {data && (data.services.length > 0 || data.packages.length > 0) && (
        <div className="rounded-lg border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-2.5 text-left font-medium">Name</th>
                <th className="px-4 py-2.5 text-left font-medium">Category</th>
                <th className="px-4 py-2.5 text-right font-medium">Times</th>
                <th className="px-4 py-2.5 text-right font-medium">Revenue</th>
                <th className="px-4 py-2.5 text-right font-medium">Avg Revenue</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {data.services.map((s) => (
                <tr key={s.serviceId} className="hover:bg-muted/30">
                  <td className="px-4 py-2 font-medium">{s.serviceName}</td>
                  <td className="px-4 py-2 text-muted-foreground">{s.categoryName ?? '—'}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{s.timesPerformed}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{formatPeso(s.totalRevenue)}</td>
                  <td className="px-4 py-2 text-right tabular-nums text-muted-foreground">{formatPeso(s.averageRevenue)}</td>
                </tr>
              ))}
              {data.packages.map((p) => (
                <tr key={p.packageId} className="hover:bg-muted/30 bg-purple-50/30">
                  <td className="px-4 py-2 font-medium">{p.packageName}</td>
                  <td className="px-4 py-2 text-muted-foreground text-xs">Package</td>
                  <td className="px-4 py-2 text-right tabular-nums">{p.timesPerformed}</td>
                  <td className="px-4 py-2 text-right tabular-nums">{formatPeso(p.totalRevenue)}</td>
                  <td className="px-4 py-2 text-right tabular-nums text-muted-foreground">{formatPeso(p.averageRevenue)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {data && data.services.length === 0 && data.packages.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center text-sm text-muted-foreground">
          No data for this period
        </div>
      )}
    </div>
  )
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function ReportsPage() {
  const defaults = useMemo(() => defaultDates(), [])
  const [from, setFrom] = useState(defaults.from)
  const [to, setTo] = useState(defaults.to)
  const [branchId, setBranchId] = useState('')
  const [employeeId, setEmployeeId] = useState('')

  const { data: branches = [] } = useBranches()
  const { data: employeesPage } = useEmployees({ branchId: branchId || undefined, pageSize: 100 })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Reports</h1>
        <p className="text-muted-foreground">Revenue analytics, service popularity, and commission reports</p>
      </div>

      {/* Shared filters */}
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">From</label>
          <Input type="date" value={from} onChange={(e) => setFrom(e.target.value)} className="h-9 w-36" />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-muted-foreground">To</label>
          <Input type="date" value={to} onChange={(e) => setTo(e.target.value)} className="h-9 w-36" />
        </div>
        <Select value={branchId || '__all__'} onValueChange={(v) => { setBranchId(v === '__all__' ? '' : v); setEmployeeId('') }}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="All branches" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="__all__">All branches</SelectItem>
            {branches.map((b) => <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>)}
          </SelectContent>
        </Select>
      </div>

      <Tabs defaultValue="revenue">
        <TabsList variant="line">
          <TabsTrigger value="revenue">Revenue</TabsTrigger>
          <TabsTrigger value="commissions">Commissions</TabsTrigger>
          <TabsTrigger value="popularity">Service Popularity</TabsTrigger>
        </TabsList>

        <TabsContent value="revenue" className="mt-6">
          <RevenueTab from={from} to={to} branchId={branchId || undefined} />
        </TabsContent>

        <TabsContent value="commissions" className="mt-6">
          {/* Employee filter only in commissions tab */}
          <div className="flex items-center gap-3 mb-4">
            <Select value={employeeId || '__all__'} onValueChange={(v) => setEmployeeId(v === '__all__' ? '' : v)}>
              <SelectTrigger className="w-52">
                <SelectValue placeholder="All employees" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__all__">All employees</SelectItem>
                {employeesPage?.items.map((e) => (
                  <SelectItem key={e.id} value={e.id}>{e.fullName}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <CommissionsTab
            from={from}
            to={to}
            branchId={branchId || undefined}
            employeeId={employeeId || undefined}
          />
        </TabsContent>

        <TabsContent value="popularity" className="mt-6">
          <ServicePopularityTab from={from} to={to} branchId={branchId || undefined} />
        </TabsContent>
      </Tabs>
    </div>
  )
}
