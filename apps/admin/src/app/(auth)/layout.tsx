import { auth } from '@clerk/nextjs/server'
import { redirect } from 'next/navigation'

export default async function AuthLayout({ children }: { children: React.ReactNode }) {
  const { userId, orgId } = await auth()

  if (userId && orgId) redirect('/dashboard')
  if (userId && !orgId) redirect('/onboarding')

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-cyan-100 p-4">
      <div className="w-full max-w-md">{children}</div>
    </div>
  )
}
