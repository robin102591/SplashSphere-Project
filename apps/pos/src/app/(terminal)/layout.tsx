import { auth } from '@clerk/nextjs/server'
import { redirect } from 'next/navigation'
import { PosNavbar } from '@/components/layout/pos-navbar'
import { SignalRProvider } from '@/lib/signalr-context'
import { BranchProvider } from '@/lib/branch-context'
import { BranchSignalRSync } from '@/components/branch-signalr-sync'
import { ShiftStatusBanner } from '@/components/shift-status-banner'
import { PosLockGuard } from '@/components/pos-lock-guard'

export default async function TerminalLayout({ children }: { children: React.ReactNode }) {
  const { userId } = await auth()
  if (!userId) redirect('/sign-in')

  return (
    <div className="min-h-screen bg-gray-950 flex flex-col">
      <BranchProvider>
        <SignalRProvider>
          <BranchSignalRSync />
          <PosLockGuard />
          <PosNavbar />
          <ShiftStatusBanner />
          <main className="flex-1 overflow-auto">{children}</main>
        </SignalRProvider>
      </BranchProvider>
    </div>
  )
}
