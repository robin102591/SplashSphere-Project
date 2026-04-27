import { AppShell } from '@/components/layout/app-shell'
import { AuthGuard } from '@/lib/auth/auth-guard'

export default function TabsLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <AuthGuard>
      <AppShell>{children}</AppShell>
    </AuthGuard>
  )
}
