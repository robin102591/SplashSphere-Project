'use client'

import { useState } from 'react'
import { useSignIn } from '@clerk/nextjs'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'

const emailSchema = z.object({
  email: z.string().email('Invalid email address'),
})
type EmailValues = z.infer<typeof emailSchema>

const resetSchema = z.object({
  code: z.string().min(1, 'Code is required'),
  newPassword: z.string().min(8, 'Password must be at least 8 characters'),
})
type ResetValues = z.infer<typeof resetSchema>

export default function ForgotPasswordPage() {
  const router = useRouter()
  const { signIn, isLoaded } = useSignIn()
  const [step, setStep] = useState<'email' | 'reset'>('email')
  const [serverError, setServerError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const emailForm = useForm<EmailValues>({
    resolver: zodResolver(emailSchema),
    defaultValues: { email: '' },
  })

  const resetForm = useForm<ResetValues>({
    resolver: zodResolver(resetSchema),
    defaultValues: { code: '', newPassword: '' },
  })

  const onSendCode = async (values: EmailValues) => {
    if (!isLoaded) return
    setServerError(null)
    try {
      await signIn.create({
        strategy: 'reset_password_email_code',
        identifier: values.email,
      })
      setSuccessMessage('Check your email for the reset code')
      setStep('reset')
    } catch (err: unknown) {
      const clerkError = err as { errors?: { message: string }[] }
      setServerError(clerkError.errors?.[0]?.message ?? 'Failed to send reset email')
    }
  }

  const onResetPassword = async (values: ResetValues) => {
    if (!isLoaded) return
    setServerError(null)
    try {
      const result = await signIn.attemptFirstFactor({
        strategy: 'reset_password_email_code',
        code: values.code,
        password: values.newPassword,
      })
      if (result.status === 'complete') {
        router.push('/sign-in?reset=success')
      }
    } catch (err: unknown) {
      const clerkError = err as { errors?: { message: string }[] }
      setServerError(clerkError.errors?.[0]?.message ?? 'Password reset failed')
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-2xl">
          {step === 'email' ? 'Reset your password' : 'Set new password'}
        </CardTitle>
        <CardDescription>
          {step === 'email'
            ? "Enter your email and we'll send you a reset code"
            : 'Enter the code from your email and your new password'}
        </CardDescription>
      </CardHeader>
      <CardContent>
        {successMessage && (
          <p className="mb-4 text-sm text-green-600 bg-green-50 rounded p-3">{successMessage}</p>
        )}
        {step === 'email' ? (
          <form onSubmit={emailForm.handleSubmit(onSendCode)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" placeholder="you@example.com" {...emailForm.register('email')} />
              {emailForm.formState.errors.email && (
                <p className="text-xs text-destructive">{emailForm.formState.errors.email.message}</p>
              )}
            </div>
            {serverError && <p className="text-sm text-destructive">{serverError}</p>}
            <Button type="submit" className="w-full" disabled={emailForm.formState.isSubmitting}>
              {emailForm.formState.isSubmitting ? 'Sending\u2026' : 'Send reset code'}
            </Button>
          </form>
        ) : (
          <form onSubmit={resetForm.handleSubmit(onResetPassword)} className="space-y-4">
            <div className="space-y-1">
              <Label htmlFor="code">Reset code</Label>
              <Input id="code" placeholder="Enter code from email" {...resetForm.register('code')} />
              {resetForm.formState.errors.code && (
                <p className="text-xs text-destructive">{resetForm.formState.errors.code.message}</p>
              )}
            </div>
            <div className="space-y-1">
              <Label htmlFor="newPassword">New password</Label>
              <Input id="newPassword" type="password" {...resetForm.register('newPassword')} />
              {resetForm.formState.errors.newPassword && (
                <p className="text-xs text-destructive">{resetForm.formState.errors.newPassword.message}</p>
              )}
            </div>
            {serverError && <p className="text-sm text-destructive">{serverError}</p>}
            <Button type="submit" className="w-full" disabled={resetForm.formState.isSubmitting}>
              {resetForm.formState.isSubmitting ? 'Resetting\u2026' : 'Reset password'}
            </Button>
          </form>
        )}
      </CardContent>
      <CardFooter className="justify-center">
        <Link href="/sign-in" className="text-sm text-primary hover:underline">
          Back to sign in
        </Link>
      </CardFooter>
    </Card>
  )
}
