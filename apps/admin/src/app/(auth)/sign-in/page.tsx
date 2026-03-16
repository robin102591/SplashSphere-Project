'use client'

import { useState, useEffect } from 'react'
import { useSignIn, useOrganizationList } from '@clerk/nextjs'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Separator } from '@/components/ui/separator'

const signInSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(1, 'Password is required'),
})
type SignInValues = z.infer<typeof signInSchema>

export default function SignInPage() {
  const router = useRouter()
  const { signIn, setActive, isLoaded } = useSignIn()
  const { isLoaded: orgsLoaded, setActive: setActiveOrg, userMemberships } = useOrganizationList({
    userMemberships: { infinite: true },
  })
  const [serverError, setServerError] = useState<string | null>(null)
  const [activatingOrg, setActivatingOrg] = useState(false)

  // After session is created, wait for memberships to finish loading then activate the org
  useEffect(() => {
    if (!activatingOrg) return
    if (!orgsLoaded) return
    if (userMemberships.isLoading) return

    const membership = userMemberships.data?.[0]
    if (membership && setActiveOrg) {
      setActiveOrg({ organization: membership.organization.id }).then(() => {
        router.push('/dashboard')
      })
    } else {
      // No org membership — send to onboarding
      router.push('/onboarding')
    }
  }, [activatingOrg, orgsLoaded, userMemberships.isLoading, userMemberships.data])

  const form = useForm<SignInValues>({
    resolver: zodResolver(signInSchema),
    defaultValues: { email: '', password: '' },
  })

  const onSubmit = async (values: SignInValues) => {
    if (!isLoaded) return
    setServerError(null)
    try {
      const result = await signIn.create({
        identifier: values.email,
        password: values.password,
      })
      if (result.status === 'complete') {
        await setActive({ session: result.createdSessionId })
        // Don't navigate yet — useEffect will activate the org then redirect
        setActivatingOrg(true)
      }
    } catch (err: unknown) {
      const clerkError = err as { errors?: { message: string }[] }
      setServerError(clerkError.errors?.[0]?.message ?? 'Sign in failed')
    }
  }

  const handleSocial = async (provider: 'oauth_google' | 'oauth_facebook' | 'oauth_microsoft') => {
    if (!isLoaded) return
    await signIn.authenticateWithRedirect({
      strategy: provider,
      redirectUrl: '/sso-callback',
      redirectUrlComplete: '/dashboard',
    })
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-2xl">Sign in to SplashSphere</CardTitle>
        <CardDescription>Enter your credentials to access the admin dashboard</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-3 gap-2">
          <Button variant="outline" type="button" onClick={() => handleSocial('oauth_google')}>
            Google
          </Button>
          <Button variant="outline" type="button" onClick={() => handleSocial('oauth_facebook')}>
            Facebook
          </Button>
          <Button variant="outline" type="button" onClick={() => handleSocial('oauth_microsoft')}>
            Microsoft
          </Button>
        </div>
        <div className="relative">
          <Separator />
          <span className="absolute left-1/2 -translate-x-1/2 -translate-y-1/2 bg-white px-2 text-xs text-muted-foreground">
            or continue with email
          </span>
        </div>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" placeholder="you@example.com" {...form.register('email')} />
            {form.formState.errors.email && (
              <p className="text-xs text-destructive">{form.formState.errors.email.message}</p>
            )}
          </div>
          <div className="space-y-1">
            <div className="flex items-center justify-between">
              <Label htmlFor="password">Password</Label>
              <Link href="/forgot-password" className="text-xs text-primary hover:underline">
                Forgot password?
              </Link>
            </div>
            <Input id="password" type="password" {...form.register('password')} />
            {form.formState.errors.password && (
              <p className="text-xs text-destructive">{form.formState.errors.password.message}</p>
            )}
          </div>
          {serverError && <p className="text-sm text-destructive">{serverError}</p>}
          <Button type="submit" className="w-full" disabled={form.formState.isSubmitting || activatingOrg}>
            {activatingOrg ? 'Setting up session\u2026' : form.formState.isSubmitting ? 'Signing in\u2026' : 'Sign in'}
          </Button>
        </form>
      </CardContent>
      <CardFooter className="justify-center">
        <p className="text-sm text-muted-foreground">
          Don&apos;t have an account?{' '}
          <Link href="/sign-up" className="text-primary hover:underline">
            Sign up
          </Link>
        </p>
      </CardFooter>
    </Card>
  )
}
