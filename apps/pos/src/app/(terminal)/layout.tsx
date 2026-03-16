import { auth } from '@clerk/nextjs/server'
import { redirect } from 'next/navigation'
import { PosNavbar } from '@/components/layout/pos-navbar'
import { SignalRProvider } from '@/lib/signalr-context'
import { BranchProvider } from '@/lib/branch-context'

export default async function TerminalLayout({ children }: { children: React.ReactNode }) {
  const { userId } = await auth()
  if (!userId) redirect('/sign-in')

  return (
    <div className="min-h-screen bg-gray-950 flex flex-col">
      <SignalRProvider>
        <BranchProvider>
          <PosNavbar />
          <main className="flex-1 overflow-auto">{children}</main>
        </BranchProvider>
      </SignalRProvider>
    </div>
  )
}
