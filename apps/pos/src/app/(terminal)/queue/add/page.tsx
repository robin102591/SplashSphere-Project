'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import Link from 'next/link'
import { ArrowLeft, Search } from 'lucide-react'
import { QueuePriority } from '@splashsphere/types'

const addQueueSchema = z.object({
  plateNumber: z.string().min(2, 'Plate number required').toUpperCase(),
  priority: z.nativeEnum(QueuePriority),
  vehicleTypeId: z.string().optional(),
  notes: z.string().optional(),
})
type AddQueueValues = z.infer<typeof addQueueSchema>

const priorityOptions = [
  { value: QueuePriority.Regular, label: 'Regular', description: 'Standard queue', color: 'border-gray-600 text-gray-300' },
  { value: QueuePriority.Express, label: 'Express', description: 'Skip ahead', color: 'border-blue-500 text-blue-300' },
  { value: QueuePriority.Vip, label: 'VIP', description: 'Priority service', color: 'border-yellow-500 text-yellow-300' },
]

export default function AddToQueuePage() {
  const router = useRouter()
  const [isLookingUp, setIsLookingUp] = useState(false)

  const { register, handleSubmit, setValue, watch, formState } = useForm<AddQueueValues>({
    resolver: zodResolver(addQueueSchema),
    defaultValues: {
      plateNumber: '',
      priority: QueuePriority.Regular,
    },
  })

  const selectedPriority = watch('priority')

  const onSubmit = async (_values: AddQueueValues) => {
    // TODO: call POST /api/v1/queue
    router.push('/queue')
  }

  const handlePlateLookup = async () => {
    const plate = watch('plateNumber')
    if (!plate) return
    setIsLookingUp(true)
    // TODO: call GET /api/v1/cars/lookup/{plate}
    setTimeout(() => setIsLookingUp(false), 500)
  }

  return (
    <div className="p-4 max-w-lg mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <Link
          href="/queue"
          className="flex items-center justify-center h-10 w-10 rounded-xl bg-gray-800 hover:bg-gray-700 text-gray-400 transition-colors"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>
        <div>
          <h1 className="text-xl font-bold text-white">Add to Queue</h1>
          <p className="text-sm text-gray-400">Enter vehicle details</p>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
        {/* Plate number */}
        <div className="space-y-2">
          <label className="text-sm font-medium text-gray-300">Plate Number</label>
          <div className="flex gap-2">
            <input
              type="text"
              placeholder="ABC 1234"
              className="flex-1 min-h-14 px-4 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 text-base uppercase font-mono tracking-widest focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              {...register('plateNumber', {
                onChange: (e) => {
                  e.target.value = e.target.value.toUpperCase()
                },
              })}
            />
            <button
              type="button"
              onClick={handlePlateLookup}
              disabled={isLookingUp}
              className="min-h-14 px-4 rounded-xl bg-gray-800 border border-gray-700 text-gray-400 hover:text-white hover:border-gray-500 transition-colors"
            >
              <Search className="h-5 w-5" />
            </button>
          </div>
          {formState.errors.plateNumber && (
            <p className="text-xs text-red-400">{formState.errors.plateNumber.message}</p>
          )}
        </div>

        {/* Priority */}
        <div className="space-y-2">
          <label className="text-sm font-medium text-gray-300">Priority</label>
          <div className="grid grid-cols-3 gap-2">
            {priorityOptions.map((opt) => (
              <button
                key={opt.value}
                type="button"
                onClick={() => setValue('priority', opt.value)}
                className={`min-h-16 rounded-xl border-2 transition-colors flex flex-col items-center justify-center gap-0.5 ${
                  selectedPriority === opt.value
                    ? opt.color + ' bg-gray-800'
                    : 'border-gray-700 text-gray-500 hover:border-gray-600'
                }`}
              >
                <span className="font-semibold text-sm">{opt.label}</span>
                <span className="text-xs opacity-70">{opt.description}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Notes */}
        <div className="space-y-2">
          <label className="text-sm font-medium text-gray-300">Notes (optional)</label>
          <textarea
            rows={2}
            placeholder="Customer instructions, preferred services…"
            className="w-full px-4 py-3 rounded-xl bg-gray-800 border border-gray-700 text-white placeholder:text-gray-500 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
            {...register('notes')}
          />
        </div>

        <button
          type="submit"
          disabled={formState.isSubmitting}
          className="w-full min-h-14 rounded-xl bg-blue-600 hover:bg-blue-500 disabled:bg-blue-800 disabled:cursor-not-allowed text-white font-semibold text-base transition-colors"
        >
          {formState.isSubmitting ? 'Adding to queue…' : 'Add to Queue'}
        </button>
      </form>
    </div>
  )
}
