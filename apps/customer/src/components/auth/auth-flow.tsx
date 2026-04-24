'use client'

import { useEffect, useState } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { useConnectAuth } from '@/lib/auth/use-connect-auth'
import { PhoneStep } from '@/components/auth/phone-step'
import { CodeStep } from '@/components/auth/code-step'

type Step = 'phone' | 'code'

/**
 * Two-step phone-OTP sign-in flow. Keeps step + phone in local state so
 * navigating "Change" back to the phone step preserves what the user typed.
 *
 * If the user is already authenticated on mount (stale tab, returning user),
 * we fast-forward to the redirect target immediately.
 */
export function AuthFlow() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { isAuthenticated, isLoading, signIn } = useConnectAuth()
  const [step, setStep] = useState<Step>('phone')
  // E.164 form (e.g. "+639171234567") — set by PhoneStep on successful send.
  const [phone, setPhone] = useState<string>('')

  const redirect = searchParams.get('redirect') || '/'

  // Already signed in? Bounce out of the auth flow.
  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      router.replace(redirect)
    }
  }, [isAuthenticated, isLoading, redirect, router])

  // Don't flash the phone step if we're about to redirect an authed user.
  if (isLoading || isAuthenticated) {
    return null
  }

  if (step === 'phone') {
    return (
      <PhoneStep
        initialPhone={phone}
        onSent={(normalizedPhone) => {
          setPhone(normalizedPhone)
          setStep('code')
        }}
      />
    )
  }

  return (
    <CodeStep
      phone={phone}
      onBack={() => setStep('phone')}
      onVerified={(response) => {
        signIn(response)
        router.replace(redirect)
      }}
    />
  )
}
