'use client'

import { useState } from 'react'
import { useSignIn } from '@clerk/nextjs'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useRouter } from 'next/navigation'
import { Droplets, Loader2 } from 'lucide-react'

const signInSchema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(1, 'Password required'),
})
type SignInValues = z.infer<typeof signInSchema>

export default function PosSignInPage() {
  const router = useRouter()
  const { signIn, setActive, isLoaded } = useSignIn()
  const [serverError, setServerError] = useState<string | null>(null)

  const { register, handleSubmit, formState } = useForm<SignInValues>({
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
        router.push('/queue')
      }
    } catch (err: unknown) {
      const clerkError = err as { errors?: { message: string }[] }
      setServerError(clerkError.errors?.[0]?.message ?? 'Sign in failed. Try again.')
    }
  }

  return (
    <div className="space-y-6">
      {/* Branding */}
      <div className="text-center space-y-2">
        <div className="flex items-center justify-center gap-2">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-500">
            <Droplets className="h-6 w-6 text-white" />
          </div>
        </div>
        <h1 className="text-2xl font-bold text-white">SplashSphere POS</h1>
        <p className="text-gray-400 text-sm">Sign in to access the terminal</p>
      </div>

      {/* Form */}
      <div className="bg-gray-800 rounded-2xl p-6 space-y-4">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1.5">
            <label htmlFor="email" className="text-sm font-medium text-gray-300">
              Email
            </label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              placeholder="staff@sparklewash.ph"
              className="w-full min-h-14 px-4 rounded-xl bg-gray-700 border border-gray-600 text-white placeholder:text-gray-500 text-base focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              {...register('email')}
            />
            {formState.errors.email && (
              <p className="text-xs text-red-400">{formState.errors.email.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <label htmlFor="password" className="text-sm font-medium text-gray-300">
              Password
            </label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              className="w-full min-h-14 px-4 rounded-xl bg-gray-700 border border-gray-600 text-white placeholder:text-gray-500 text-base focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              {...register('password')}
            />
            {formState.errors.password && (
              <p className="text-xs text-red-400">{formState.errors.password.message}</p>
            )}
          </div>

          {serverError && (
            <div className="rounded-lg bg-red-900/50 border border-red-700 px-4 py-3">
              <p className="text-sm text-red-300">{serverError}</p>
            </div>
          )}

          <button
            type="submit"
            disabled={formState.isSubmitting}
            className="w-full min-h-14 rounded-xl bg-blue-600 hover:bg-blue-500 disabled:bg-blue-800 disabled:cursor-not-allowed text-white font-semibold text-base transition-colors flex items-center justify-center gap-2"
          >
            {formState.isSubmitting ? (
              <>
                <Loader2 className="h-5 w-5 animate-spin" />
                Signing in…
              </>
            ) : (
              'Sign in'
            )}
          </button>
        </form>
      </div>
    </div>
  )
}
