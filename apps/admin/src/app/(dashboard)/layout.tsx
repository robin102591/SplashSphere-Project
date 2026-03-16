import { AppSidebar } from '@/components/layout/app-sidebar'
import { AppHeader } from '@/components/layout/app-header'
import { SidebarProvider, SidebarInset } from '@/components/ui/sidebar'
import { SignalRProvider } from '@/lib/signalr-context'
import { auth } from '@clerk/nextjs/server'
import { redirect } from 'next/navigation'

export default async function DashboardLayout({ children }: { children: React.ReactNode }) {
  const { userId } = await auth()
  if (!userId) redirect('/sign-in')

  return (
    <SidebarProvider>
      <AppSidebar />
      <SignalRProvider>
        <SidebarInset>
          <AppHeader />
          <main className="flex-1 p-6">{children}</main>
        </SidebarInset>
      </SignalRProvider>
    </SidebarProvider>
  )
}
