import Link from 'next/link'
import { Plus, Clock } from 'lucide-react'
import { QueuePriority } from '@splashsphere/types'

interface QueueCardProps {
  queueNumber: string
  plate: string
  priority: QueuePriority
  services?: string
}

function QueueCard({ queueNumber, plate, priority, services }: QueueCardProps) {
  const priorityLabel =
    priority === QueuePriority.Vip ? 'VIP' :
    priority === QueuePriority.Express ? 'Express' :
    'Regular'

  return (
    <div className="bg-gray-800 rounded-xl p-4 space-y-2 cursor-pointer hover:bg-gray-750 transition-colors border border-gray-700">
      <div className="flex items-center justify-between">
        <span className="text-xl font-bold text-white">{queueNumber}</span>
        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
          priority === QueuePriority.Vip ? 'bg-yellow-500/20 text-yellow-300' :
          priority === QueuePriority.Express ? 'bg-blue-500/20 text-blue-300' :
          'bg-gray-600 text-gray-300'
        }`}>{priorityLabel}</span>
      </div>
      <p className="text-sm font-mono text-gray-300">{plate}</p>
      {services && <p className="text-xs text-gray-500 truncate">{services}</p>}
    </div>
  )
}

interface ColumnProps {
  title: string
  count: number
  color: string
  children: React.ReactNode
}

function Column({ title, count, color, children }: ColumnProps) {
  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center gap-2">
        <div className={`h-2.5 w-2.5 rounded-full ${color}`} />
        <h2 className="font-semibold text-gray-200">{title}</h2>
        <span className="ml-auto text-sm text-gray-500 bg-gray-800 px-2 py-0.5 rounded-full">
          {count}
        </span>
      </div>
      <div className="space-y-2 min-h-32">{children}</div>
    </div>
  )
}

export default function QueuePage() {
  // Placeholder — will be replaced with real data from API
  const waiting: QueueCardProps[] = []
  const called: QueueCardProps[] = []
  const inService: QueueCardProps[] = []

  return (
    <div className="p-4 space-y-4 h-full">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-white">Queue Board</h1>
          <p className="text-sm text-gray-400 flex items-center gap-1">
            <Clock className="h-3.5 w-3.5" />
            <span>0 vehicles waiting</span>
          </p>
        </div>
        <Link
          href="/queue/add"
          className="flex items-center gap-2 px-4 min-h-11 rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-semibold text-sm transition-colors"
        >
          <Plus className="h-4 w-4" />
          Add to Queue
        </Link>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <Column title="Waiting" count={waiting.length} color="bg-gray-400">
          {waiting.length === 0 && (
            <p className="text-center text-sm text-gray-600 py-8">No vehicles waiting</p>
          )}
          {waiting.map((entry) => (
            <QueueCard key={entry.queueNumber} {...entry} />
          ))}
        </Column>

        <Column title="Called" count={called.length} color="bg-yellow-400">
          {called.length === 0 && (
            <p className="text-center text-sm text-gray-600 py-8">No vehicles called</p>
          )}
          {called.map((entry) => (
            <QueueCard key={entry.queueNumber} {...entry} />
          ))}
        </Column>

        <Column title="In Service" count={inService.length} color="bg-green-400">
          {inService.length === 0 && (
            <p className="text-center text-sm text-gray-600 py-8">No vehicles in service</p>
          )}
          {inService.map((entry) => (
            <QueueCard key={entry.queueNumber} {...entry} />
          ))}
        </Column>
      </div>
    </div>
  )
}
