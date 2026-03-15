'use client'

import { useEffect } from 'react'
import { useClerk } from '@clerk/nextjs'
import { useRouter } from 'next/navigation'

export default function SSOCallbackPage() {
  const { handleRedirectCallback, loaded } = useClerk()
  const router = useRouter()

  useEffect(() => {
    if (!loaded) return

    void handleRedirectCallback(
      {
        afterSignInUrl: '/dashboard',
        afterSignUpUrl: '/onboarding',
      },
      (to: string) => {
        router.push(to)
        return Promise.resolve()
      }
    )
  }, [loaded, handleRedirectCallback, router])

  return (
    <div className="min-h-screen flex items-center justify-center">
      <p className="text-muted-foreground animate-pulse">Completing sign in\u2026</p>
    </div>
  )
}
