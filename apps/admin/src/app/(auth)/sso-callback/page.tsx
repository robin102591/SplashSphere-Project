'use client'

import { useEffect } from 'react'
import { useClerk, useOrganizationList } from '@clerk/nextjs'
import { useRouter } from 'next/navigation'

export default function SSOCallbackPage() {
  const { handleRedirectCallback, loaded } = useClerk()
  const router = useRouter()
  const { isLoaded: orgsLoaded, setActive, userMemberships } = useOrganizationList({
    userMemberships: { infinite: true },
  })

  useEffect(() => {
    if (!loaded) return

    void handleRedirectCallback(
      {
        afterSignInUrl: '/sso-callback?activateOrg=1',
        afterSignUpUrl: '/onboarding',
      },
      (to: string) => {
        router.push(to)
        return Promise.resolve()
      }
    )
  }, [loaded, handleRedirectCallback, router])

  // After sign-in redirect back here with ?activateOrg=1, activate the org then go to dashboard
  useEffect(() => {
    if (typeof window === 'undefined') return
    const params = new URLSearchParams(window.location.search)
    if (!params.has('activateOrg')) return
    if (!orgsLoaded) return
    if (userMemberships.isLoading) return

    const membership = userMemberships.data?.[0]
    if (membership && setActive) {
      setActive({ organization: membership.organization.id }).then(() => {
        router.push('/dashboard')
      })
    } else {
      router.push('/onboarding')
    }
  }, [orgsLoaded, userMemberships.isLoading, userMemberships.data, setActive, router])

  return (
    <div className="min-h-screen flex items-center justify-center">
      <p className="text-muted-foreground animate-pulse">Completing sign in\u2026</p>
    </div>
  )
}
