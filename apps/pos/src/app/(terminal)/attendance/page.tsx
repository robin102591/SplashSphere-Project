'use client'

import { useState } from 'react'
import { Fingerprint, CheckCircle2 } from 'lucide-react'

export default function AttendancePage() {
  const [clocked, setClocked] = useState(false)

  return (
    <div className="p-4 space-y-4">
      <div>
        <h1 className="text-xl font-bold text-white">Attendance</h1>
        <p className="text-sm text-gray-400">Clock in or out for your shift</p>
      </div>

      <div className="max-w-sm mx-auto pt-8 flex flex-col items-center gap-6">
        <button
          onClick={() => setClocked((v) => !v)}
          className={`w-40 h-40 rounded-full flex flex-col items-center justify-center gap-2 transition-all text-white font-semibold text-lg ${
            clocked
              ? 'bg-green-600 hover:bg-green-500 shadow-lg shadow-green-900'
              : 'bg-blue-600 hover:bg-blue-500 shadow-lg shadow-blue-900'
          }`}
        >
          {clocked ? (
            <>
              <CheckCircle2 className="h-10 w-10" />
              Clock Out
            </>
          ) : (
            <>
              <Fingerprint className="h-10 w-10" />
              Clock In
            </>
          )}
        </button>

        <p className="text-sm text-gray-400">
          {clocked ? 'Tap to clock out of your shift' : 'Tap to start your shift'}
        </p>
      </div>
    </div>
  )
}
