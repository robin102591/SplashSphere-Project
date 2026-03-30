import { cookies } from 'next/headers'
import { AppSidebar } from '@/components/layout/app-sidebar'
import { AppHeader } from '@/components/layout/app-header'
import { TrialBanner } from '@/components/plan/trial-banner'
import { SidebarProvider, SidebarInset } from '@/components/ui/sidebar'
import { SignalRProvider } from '@/lib/signalr-context'
import { auth } from '@clerk/nextjs/server'
import { redirect } from 'next/navigation'

export default async function DashboardLayout({ children }: { children: React.ReactNode }) {
  const { userId } = await auth()
  if (!userId) redirect('/sign-in')

  const cookieStore = await cookies()
  const sidebarState = cookieStore.get('sidebar_state')?.value
  const defaultOpen = sidebarState !== 'false'

  return (
    <SidebarProvider defaultOpen={defaultOpen}>
      <AppSidebar />
      <SignalRProvider>
        <SidebarInset>
          <TrialBanner />
          <AppHeader />
          <main className="flex-1 p-6">{children}</main>
        </SidebarInset>
      </SignalRProvider>
    </SidebarProvider>
  )
}
