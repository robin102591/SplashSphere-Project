'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@clerk/nextjs'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useQuery } from '@tanstack/react-query'
import Link from 'next/link'
import { ArrowLeft, Search, Check, Car, User, AlertCircle, RefreshCw } from 'lucide-react'
import { QueuePriority } from '@splashsphere/types'
import type { Car as CarType, ServiceSummary, Branch } from '@splashsphere/types'
import type { PagedResult, ApiError } from '@splashsphere/types'
import { apiClient } from '@/lib/api-client'

// ── Schema ─────────────────────────────────────────────────────────────────────

const schema = z.object({
  plateNumber: z.string().min(2, 'Plate number is required'),
  priority: z.nativeEnum(QueuePriority),
  notes: z.string().optional(),
})
type FormValues = z.infer<typeof schema>

// ── Priority options ───────────────────────────────────────────────────────────

const PRIORITY_OPTIONS = [
  {
    value: QueuePriority.Regular,
    label: 'Regular',
    desc: 'Standard queue',
    activeCls: 'border-gray-500 bg-gray-700/50 text-white',
    dotCls: 'bg-gray-400',
  },
  {
    value: QueuePriority.Express,
    label: 'Express',
    desc: 'Skip ahead',
    activeCls: 'border-blue-500 bg-blue-950/60 text-blue-300',
    dotCls: 'bg-blue-400',
  },
  {
    value: QueuePriority.Vip,
    label: 'VIP',
    desc: 'Priority service',
    activeCls: 'border-yellow-500 bg-yellow-950/60 text-yellow-300',
    dotCls: 'bg-yellow-400',
  },
] as const

// ── Page ───────────────────────────────────────────────────────────────────────

export default function AddToQueuePage() {
  const router = useRouter()
  const { getToken } = useAuth()

  const [foundCar, setFoundCar] = useState<CarType | null>(null)
  const [lookupMsg, setLookupMsg] = useState<{ text: string; ok: boolean } | null>(null)
  const [isLookingUp, setIsLookingUp] = useState(false)
  const [selectedServiceIds, setSelectedServiceIds] = useState<string[]>([])
  const [submitError, setSubmitError] = useState<string | null>(null)

  // Persisted branch
  const [branchId, setBranchId] = useState<string>(() => {
    if (typeof window !== 'undefined') return localStorage.getItem('pos-branch-id') ?? ''
    return ''
  })

  const { register, handleSubmit, setValue, watch, formState } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { plateNumber: '', priority: QueuePriority.Regular },
  })

  const selectedPriority = watch('priority')
  const plateValue = watch('plateNumber')

  // ── Branches ────────────────────────────────────────────────────────────────

  const { data: branches = [] } = useQuery({
    queryKey: ['branches'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<Branch>>('/branches', token ?? undefined)
      return res.items as Branch[]
    },
  })

  const effectiveBranchId = branchId || branches[0]?.id

  // ── Services ────────────────────────────────────────────────────────────────

  const { data: services = [] } = useQuery({
    queryKey: ['services-compact'],
    queryFn: async () => {
      const token = await getToken()
      const res = await apiClient.get<PagedResult<ServiceSummary>>('/services?pageSize=100', token ?? undefined)
      return res.items as ServiceSummary[]
    },
  })

  const activeServices = services.filter(s => s.isActive)

  // ── Plate lookup ────────────────────────────────────────────────────────────

  const handlePlateLookup = async () => {
    const plate = plateValue.trim().toUpperCase()
    if (!plate || plate.length < 2) return
    setIsLookingUp(true)
    setLookupMsg(null)
    setFoundCar(null)
    try {
      const token = await getToken()
      const car = await apiClient.get<CarType>(
        `/cars/lookup/${encodeURIComponent(plate)}`,
        token ?? undefined
      )
      setFoundCar(car)
      setLookupMsg({ text: 'Vehicle found — details pre-filled.', ok: true })
    } catch {
      setLookupMsg({ text: 'No record found — new vehicle will be created.', ok: false })
    } finally {
      setIsLookingUp(false)
    }
  }

  // ── Service toggle ───────────────────────────────────────────────────────────

  const toggleService = (id: string) => {
    setSelectedServiceIds(prev =>
      prev.includes(id) ? prev.filter(s => s !== id) : [...prev, id]
    )
  }

  // ── Submit ───────────────────────────────────────────────────────────────────

  const onSubmit = async (values: FormValues) => {
    setSubmitError(null)
    try {
      const token = await getToken()
      const body = {
        branchId: effectiveBranchId,
        plateNumber: values.plateNumber.trim().toUpperCase(),
        priority: values.priority,
        carId: foundCar?.id ?? null,
        customerId: foundCar?.customerId ?? null,
        vehicleTypeId: foundCar?.vehicleTypeId ?? null,
        preferredServiceIds: selectedServiceIds.length > 0 ? selectedServiceIds : null,
        notes: values.notes?.trim() || null,
      }
      await apiClient.post('/queue', body, token ?? undefined)
      router.push('/queue')
    } catch (err) {
      const apiErr = err as ApiError
      setSubmitError(apiErr?.detail ?? apiErr?.title ?? 'Failed to add to queue. Please try again.')
    }
  }

  const handleBranchChange = (id: string) => {
    setBranchId(id)
    localStorage.setItem('pos-branch-id', id)
  }

  return (
    <div className="p-4 max-w-lg mx-auto space-y-6 pb-10">

      {/* ── Header ────────────────────────────────────────────────────────── */}
      <div className="flex items-center gap-3">
        <Link
          href="/queue"
          className="flex items-center justify-center h-10 w-10 rounded-xl bg-gray-800 hover:bg-gray-700 text-gray-400 transition-colors shrink-0"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>
        <div>
          <h1 className="text-xl font-bold text-white">Add to Queue</h1>
          <p className="text-sm text-gray-400">Enter vehicle details</p>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">

        {/* ── Branch selector (multi-branch) ────────────────────────────── */}
        {branches.length > 1 && (
          <div className="space-y-2">
            <label className="text-sm font-medium text-gray-300">Branch</label>
            <select
              value={branchId}
              onChange={e => handleBranchChange(e.target.value)}
              className="w-full min-h-12 px-4 rounded-xl bg-gray-800 border border-gray-700 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
            >
              <option value="">Select branch…</option>
              {branches.map(b => (
                <option key={b.id} value={b.id}>{b.name}</option>
              ))}
            </select>
          </div>
        )}

        {/* ── Plate number + lookup ─────────────────────────────────────── */}
        <div className="space-y-2">
          <label className="text-sm font-medium text-gray-300">Plate Number</label>
          <div className="flex gap-2">
            <input
              type="text"
              placeholder="ABC 1234"
              autoComplete="off"
              autoCapitalize="characters"
              className="flex-1 min-h-14 px-4 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 text-lg font-mono tracking-widest focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent uppercase"
              {...register('plateNumber', {
                onChange: e => { e.target.value = (e.target.value as string).toUpperCase() },
              })}
            />
            <button
              type="button"
              onClick={handlePlateLookup}
              disabled={isLookingUp || !plateValue.trim()}
              className="min-h-14 px-4 rounded-xl bg-gray-800 border border-gray-700 text-gray-400 hover:text-white hover:border-gray-500 disabled:opacity-40 transition-colors"
              title="Look up plate"
            >
              {isLookingUp
                ? <RefreshCw className="h-5 w-5 animate-spin" />
                : <Search className="h-5 w-5" />
              }
            </button>
          </div>
          {formState.errors.plateNumber && (
            <p className="text-xs text-red-400">{formState.errors.plateNumber.message}</p>
          )}

          {/* Lookup result */}
          {foundCar && (
            <div className="rounded-xl bg-green-950/40 border border-green-700/50 p-3 space-y-1.5">
              <div className="flex items-center gap-2">
                <Car className="h-4 w-4 text-green-400 shrink-0" />
                <span className="text-sm font-semibold text-green-300">
                  {foundCar.vehicleTypeName} · {foundCar.sizeName}
                  {foundCar.makeName ? ` · ${foundCar.makeName}` : ''}
                  {foundCar.modelName ? ` ${foundCar.modelName}` : ''}
                  {foundCar.color ? ` (${foundCar.color})` : ''}
                </span>
              </div>
              {foundCar.customerFullName && (
                <div className="flex items-center gap-2">
                  <User className="h-4 w-4 text-gray-400 shrink-0" />
                  <span className="text-sm text-gray-300">{foundCar.customerFullName}</span>
                </div>
              )}
            </div>
          )}

          {lookupMsg && !foundCar && (
            <p className={`text-xs ${lookupMsg.ok ? 'text-green-400' : 'text-gray-500'}`}>
              {lookupMsg.text}
            </p>
          )}
        </div>

        {/* ── Priority ──────────────────────────────────────────────────── */}
        <div className="space-y-2">
          <label className="text-sm font-medium text-gray-300">Priority</label>
          <div className="grid grid-cols-3 gap-2">
            {PRIORITY_OPTIONS.map(opt => (
              <button
                key={opt.value}
                type="button"
                onClick={() => setValue('priority', opt.value, { shouldValidate: true })}
                className={`min-h-[4.5rem] rounded-xl border-2 transition-all flex flex-col items-center justify-center gap-1 ${
                  selectedPriority === opt.value
                    ? opt.activeCls
                    : 'border-gray-700 bg-gray-800/50 text-gray-500 hover:border-gray-600'
                }`}
              >
                <div
                  className={`h-2 w-2 rounded-full transition-colors ${
                    selectedPriority === opt.value ? opt.dotCls : 'bg-gray-600'
                  }`}
                />
                <span className="font-bold text-sm">{opt.label}</span>
                <span className="text-xs opacity-70">{opt.desc}</span>
              </button>
            ))}
          </div>
        </div>

        {/* ── Preferred services multi-select ───────────────────────────── */}
        {activeServices.length > 0 && (
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <label className="text-sm font-medium text-gray-300">
                Preferred Services
                <span className="ml-1 text-xs text-gray-500">(optional)</span>
              </label>
              {selectedServiceIds.length > 0 && (
                <span className="text-xs text-blue-400 font-medium">
                  {selectedServiceIds.length} selected
                </span>
              )}
            </div>
            <div className="grid grid-cols-2 gap-2 max-h-52 overflow-y-auto pr-1">
              {activeServices.map(service => {
                const selected = selectedServiceIds.includes(service.id)
                return (
                  <button
                    key={service.id}
                    type="button"
                    onClick={() => toggleService(service.id)}
                    className={`flex items-center gap-2.5 min-h-11 px-3 rounded-xl text-left transition-all border ${
                      selected
                        ? 'bg-blue-600/20 border-blue-500/60 text-blue-300'
                        : 'bg-gray-800 border-gray-700 text-gray-400 hover:border-gray-600 hover:text-gray-300'
                    }`}
                  >
                    <div
                      className={`h-4 w-4 rounded shrink-0 flex items-center justify-center border transition-colors ${
                        selected ? 'bg-blue-500 border-blue-500' : 'border-gray-600'
                      }`}
                    >
                      {selected && <Check className="h-2.5 w-2.5 text-white" />}
                    </div>
                    <div className="min-w-0">
                      <p className="text-xs font-medium truncate">{service.name}</p>
                      {service.categoryName && (
                        <p className="text-xs text-gray-600 truncate">{service.categoryName}</p>
                      )}
                    </div>
                  </button>
                )
              })}
            </div>
          </div>
        )}

        {/* ── Notes ─────────────────────────────────────────────────────── */}
        <div className="space-y-2">
          <label className="text-sm font-medium text-gray-300">
            Notes
            <span className="ml-1 text-xs text-gray-500">(optional)</span>
          </label>
          <textarea
            rows={2}
            placeholder="Special instructions, customer preferences…"
            className="w-full px-4 py-3 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
            {...register('notes')}
          />
        </div>

        {/* ── Submit error ──────────────────────────────────────────────── */}
        {submitError && (
          <div className="flex items-start gap-2 rounded-xl bg-red-950/50 border border-red-800/60 px-4 py-3">
            <AlertCircle className="h-4 w-4 text-red-400 mt-0.5 shrink-0" />
            <p className="text-sm text-red-300">{submitError}</p>
          </div>
        )}

        {/* ── Submit ────────────────────────────────────────────────────── */}
        <button
          type="submit"
          disabled={formState.isSubmitting || !effectiveBranchId}
          className="w-full min-h-14 rounded-xl bg-blue-600 hover:bg-blue-500 disabled:bg-gray-700 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-bold text-base transition-colors"
        >
          {formState.isSubmitting ? (
            <span className="flex items-center justify-center gap-2">
              <RefreshCw className="h-4 w-4 animate-spin" />
              Adding to queue…
            </span>
          ) : 'Add to Queue'}
        </button>
      </form>
    </div>
  )
}
