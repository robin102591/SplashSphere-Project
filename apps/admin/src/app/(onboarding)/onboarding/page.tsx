'use client'

import { useState } from 'react'
import { useAuth } from '@clerk/nextjs'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Progress } from '@/components/ui/progress'
import { apiClient } from '@/lib/api-client'
import type { CreateOnboardingRequest } from '@splashsphere/types'

const businessSchema = z.object({
  businessName: z.string().min(2, 'Business name is required'),
  businessEmail: z.string().email('Invalid email'),
  contactNumber: z.string().min(7, 'Contact number is required'),
  address: z.string().min(5, 'Address is required'),
})
type BusinessValues = z.infer<typeof businessSchema>

const branchSchema = z.object({
  firstBranchName: z.string().min(2, 'Branch name is required'),
  firstBranchAddress: z.string().min(5, 'Branch address is required'),
  firstBranchCity: z.string().min(2, 'City is required'),
  firstBranchContactNumber: z.string().min(7, 'Contact number is required'),
})
type BranchValues = z.infer<typeof branchSchema>

type Step = 'welcome' | 'business' | 'branch' | 'confirm'
const STEPS: Step[] = ['welcome', 'business', 'branch', 'confirm']
const STEP_PROGRESS: Record<Step, number> = {
  welcome: 0,
  business: 33,
  branch: 66,
  confirm: 90,
}

export default function OnboardingPage() {
  const router = useRouter()
  const { getToken } = useAuth()
  const [step, setStep] = useState<Step>('welcome')
  const [serverError, setServerError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const [businessData, setBusinessData] = useState<BusinessValues>({
    businessName: '',
    businessEmail: '',
    contactNumber: '',
    address: '',
  })
  const [branchData, setBranchData] = useState<BranchValues>({
    firstBranchName: '',
    firstBranchAddress: '',
    firstBranchCity: '',
    firstBranchContactNumber: '',
  })

  const businessForm = useForm<BusinessValues>({
    resolver: zodResolver(businessSchema),
    defaultValues: businessData,
  })

  const branchForm = useForm<BranchValues>({
    resolver: zodResolver(branchSchema),
    defaultValues: branchData,
  })

  const onBusinessNext = (values: BusinessValues) => {
    setBusinessData(values)
    setStep('branch')
  }

  const onBranchNext = (values: BranchValues) => {
    setBranchData(values)
    setStep('confirm')
  }

  const onSubmit = async () => {
    setServerError(null)
    setIsSubmitting(true)
    try {
      const token = await getToken()
      const payload: CreateOnboardingRequest = { ...businessData, ...branchData }
      await apiClient.post('/onboarding', payload, token ?? undefined)
      router.push('/dashboard')
    } catch (err: unknown) {
      const apiErr = err as { detail?: string; title?: string }
      setServerError(apiErr.detail ?? apiErr.title ?? 'Onboarding failed. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="w-full max-w-lg">
      <div className="mb-6">
        <p className="text-sm text-muted-foreground mb-2 text-center">
          Step {STEPS.indexOf(step) + 1} of {STEPS.length}
        </p>
        <Progress value={STEP_PROGRESS[step]} className="h-2" />
      </div>

      {step === 'welcome' && (
        <Card>
          <CardHeader>
            <CardTitle className="text-2xl">Welcome to SplashSphere</CardTitle>
            <CardDescription>
              Let&apos;s set up your car wash business in just a few steps. You&apos;ll be ready to
              manage your operations in under 5 minutes.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ul className="space-y-2 text-sm text-muted-foreground">
              <li>Configure your business details</li>
              <li>Set up your first branch</li>
              <li>Start managing your queue and transactions</li>
            </ul>
          </CardContent>
          <CardFooter>
            <Button className="w-full" onClick={() => setStep('business')}>
              Get started
            </Button>
          </CardFooter>
        </Card>
      )}

      {step === 'business' && (
        <Card>
          <CardHeader>
            <CardTitle>Business details</CardTitle>
            <CardDescription>Tell us about your car wash business</CardDescription>
          </CardHeader>
          <CardContent>
            <form id="business-form" onSubmit={businessForm.handleSubmit(onBusinessNext)} className="space-y-4">
              <div className="space-y-1">
                <Label htmlFor="businessName">Business name</Label>
                <Input id="businessName" placeholder="SparkleWash Philippines" {...businessForm.register('businessName')} />
                {businessForm.formState.errors.businessName && (
                  <p className="text-xs text-destructive">{businessForm.formState.errors.businessName.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="businessEmail">Business email</Label>
                <Input id="businessEmail" type="email" placeholder="info@sparklewash.ph" {...businessForm.register('businessEmail')} />
                {businessForm.formState.errors.businessEmail && (
                  <p className="text-xs text-destructive">{businessForm.formState.errors.businessEmail.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="contactNumber">Contact number</Label>
                <Input id="contactNumber" placeholder="+63 917 123 4567" {...businessForm.register('contactNumber')} />
                {businessForm.formState.errors.contactNumber && (
                  <p className="text-xs text-destructive">{businessForm.formState.errors.contactNumber.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="address">Business address</Label>
                <Input id="address" placeholder="123 Ayala Ave, Makati City" {...businessForm.register('address')} />
                {businessForm.formState.errors.address && (
                  <p className="text-xs text-destructive">{businessForm.formState.errors.address.message}</p>
                )}
              </div>
            </form>
          </CardContent>
          <CardFooter className="gap-2">
            <Button variant="outline" onClick={() => setStep('welcome')}>Back</Button>
            <Button type="submit" form="business-form" className="flex-1">Next</Button>
          </CardFooter>
        </Card>
      )}

      {step === 'branch' && (
        <Card>
          <CardHeader>
            <CardTitle>First branch</CardTitle>
            <CardDescription>Set up your first car wash location</CardDescription>
          </CardHeader>
          <CardContent>
            <form id="branch-form" onSubmit={branchForm.handleSubmit(onBranchNext)} className="space-y-4">
              <div className="space-y-1">
                <Label htmlFor="firstBranchName">Branch name</Label>
                <Input id="firstBranchName" placeholder="Main Branch" {...branchForm.register('firstBranchName')} />
                {branchForm.formState.errors.firstBranchName && (
                  <p className="text-xs text-destructive">{branchForm.formState.errors.firstBranchName.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="firstBranchAddress">Branch address</Label>
                <Input id="firstBranchAddress" placeholder="123 Main St" {...branchForm.register('firstBranchAddress')} />
                {branchForm.formState.errors.firstBranchAddress && (
                  <p className="text-xs text-destructive">{branchForm.formState.errors.firstBranchAddress.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="firstBranchCity">City</Label>
                <Input id="firstBranchCity" placeholder="Makati City" {...branchForm.register('firstBranchCity')} />
                {branchForm.formState.errors.firstBranchCity && (
                  <p className="text-xs text-destructive">{branchForm.formState.errors.firstBranchCity.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="firstBranchContactNumber">Branch contact</Label>
                <Input id="firstBranchContactNumber" placeholder="+63 917 123 4567" {...branchForm.register('firstBranchContactNumber')} />
                {branchForm.formState.errors.firstBranchContactNumber && (
                  <p className="text-xs text-destructive">{branchForm.formState.errors.firstBranchContactNumber.message}</p>
                )}
              </div>
            </form>
          </CardContent>
          <CardFooter className="gap-2">
            <Button variant="outline" onClick={() => setStep('business')}>Back</Button>
            <Button type="submit" form="branch-form" className="flex-1">Next</Button>
          </CardFooter>
        </Card>
      )}

      {step === 'confirm' && (
        <Card>
          <CardHeader>
            <CardTitle>Confirm &amp; launch</CardTitle>
            <CardDescription>Review your details before creating your account</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="rounded-lg border p-4 space-y-2">
              <p className="text-sm font-semibold text-muted-foreground uppercase tracking-wider">Business</p>
              <p className="font-medium">{businessData.businessName}</p>
              <p className="text-sm text-muted-foreground">{businessData.businessEmail}</p>
              <p className="text-sm text-muted-foreground">{businessData.address}</p>
            </div>
            <div className="rounded-lg border p-4 space-y-2">
              <p className="text-sm font-semibold text-muted-foreground uppercase tracking-wider">First Branch</p>
              <p className="font-medium">{branchData.firstBranchName}</p>
              <p className="text-sm text-muted-foreground">{branchData.firstBranchAddress}, {branchData.firstBranchCity}</p>
              <p className="text-sm text-muted-foreground">{branchData.firstBranchContactNumber}</p>
            </div>
            {serverError && <p className="text-sm text-destructive">{serverError}</p>}
          </CardContent>
          <CardFooter className="gap-2">
            <Button variant="outline" onClick={() => setStep('branch')}>Back</Button>
            <Button className="flex-1" onClick={onSubmit} disabled={isSubmitting}>
              {isSubmitting ? 'Setting up your business\u2026' : 'Launch SplashSphere'}
            </Button>
          </CardFooter>
        </Card>
      )}
    </div>
  )
}
