'use client'

import { useState } from 'react'
import { useSignUp } from '@clerk/nextjs'
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

const registerSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
})
type RegisterValues = z.infer<typeof registerSchema>

const verifySchema = z.object({
  code: z.string().length(6, 'Code must be 6 digits'),
})
type VerifyValues = z.infer<typeof verifySchema>

export default function SignUpPage() {
  const router = useRouter()
  const { signUp, setActive, isLoaded } = useSignUp()
  const [step, setStep] = useState<'register' | 'verify'>('register')
  const [serverError, setServerError] = useState<string | null>(null)

  const registerForm = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { firstName: '', lastName: '', email: '', password: '' },
  })

  const verifyForm = useForm<VerifyValues>({
    resolver: zodResolver(verifySchema),
    defaultValues: { code: '' },
  })

  const onRegister = async (values: RegisterValues) => {
    if (!isLoaded) return
    setServerError(null)
    try {
      await signUp.create({
        firstName: values.firstName,
        lastName: values.lastName,
        emailAddress: values.email,
        password: values.password,
      })
      await signUp.prepareEmailAddressVerification({ strategy: 'email_code' })
      setStep('verify')
    } catch (err: unknown) {
      const clerkError = err as { errors?: { message: string }[] }
      setServerError(clerkError.errors?.[0]?.message ?? 'Registration failed')
    }
  }

  const onVerify = async (values: VerifyValues) => {
    if (!isLoaded) return
    setServerError(null)
    try {
      const result = await signUp.attemptEmailAddressVerification({ code: values.code })
      if (result.status === 'complete') {
        await setActive({ session: result.createdSessionId })
        router.push('/onboarding')
      }
    } catch (err: unknown) {
      const clerkError = err as { errors?: { message: string }[] }
      setServerError(clerkError.errors?.[0]?.message ?? 'Verification failed')
    }
  }

  const handleSocial = async (provider: 'oauth_google' | 'oauth_facebook' | 'oauth_microsoft') => {
    if (!isLoaded) return
    await signUp.authenticateWithRedirect({
      strategy: provider,
      redirectUrl: '/sso-callback',
      redirectUrlComplete: '/onboarding',
    })
  }

  if (step === 'verify') {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">Verify your email</CardTitle>
          <CardDescription>We sent a 6-digit code to your email address</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={verifyForm.handleSubmit(onVerify)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="code">Verification code</Label>
              <Input
                id="code"
                placeholder="123456"
                maxLength={6}
                className="text-center text-lg tracking-widest"
                {...verifyForm.register('code')}
              />
              {verifyForm.formState.errors.code && (
                <p className="text-xs text-destructive">{verifyForm.formState.errors.code.message}</p>
              )}
            </div>
            {serverError && <p className="text-sm text-destructive">{serverError}</p>}
            <Button type="submit" className="w-full" disabled={verifyForm.formState.isSubmitting}>
              {verifyForm.formState.isSubmitting ? 'Verifying\u2026' : 'Verify email'}
            </Button>
          </form>
        </CardContent>
        <CardFooter className="justify-center">
          <Button variant="link" onClick={() => setStep('register')}>
            Back to registration
          </Button>
        </CardFooter>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-2xl">Create your account</CardTitle>
        <CardDescription>Start your SplashSphere free trial today</CardDescription>
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
            or register with email
          </span>
        </div>
        <form onSubmit={registerForm.handleSubmit(onRegister)} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1">
              <Label htmlFor="firstName">First name</Label>
              <Input id="firstName" placeholder="Juan" {...registerForm.register('firstName')} />
              {registerForm.formState.errors.firstName && (
                <p className="text-xs text-destructive">{registerForm.formState.errors.firstName.message}</p>
              )}
            </div>
            <div className="space-y-1">
              <Label htmlFor="lastName">Last name</Label>
              <Input id="lastName" placeholder="Cruz" {...registerForm.register('lastName')} />
              {registerForm.formState.errors.lastName && (
                <p className="text-xs text-destructive">{registerForm.formState.errors.lastName.message}</p>
              )}
            </div>
          </div>
          <div className="space-y-1">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" placeholder="you@example.com" {...registerForm.register('email')} />
            {registerForm.formState.errors.email && (
              <p className="text-xs text-destructive">{registerForm.formState.errors.email.message}</p>
            )}
          </div>
          <div className="space-y-1">
            <Label htmlFor="password">Password</Label>
            <Input id="password" type="password" {...registerForm.register('password')} />
            {registerForm.formState.errors.password && (
              <p className="text-xs text-destructive">{registerForm.formState.errors.password.message}</p>
            )}
          </div>
          {serverError && <p className="text-sm text-destructive">{serverError}</p>}
          <Button type="submit" className="w-full" disabled={registerForm.formState.isSubmitting}>
            {registerForm.formState.isSubmitting ? 'Creating account\u2026' : 'Create account'}
          </Button>
        </form>
      </CardContent>
      <CardFooter className="justify-center">
        <p className="text-sm text-muted-foreground">
          Already have an account?{' '}
          <Link href="/sign-in" className="text-primary hover:underline">
            Sign in
          </Link>
        </p>
      </CardFooter>
    </Card>
  )
}
