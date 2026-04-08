'use client'

import { useEffect } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { useAuth, useUser } from '@clerk/nextjs'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Droplets, AlertCircle, CheckCircle } from 'lucide-react'
import { useValidateInvitation, useAcceptInvitation } from '@/hooks/use-franchise'

const acceptSchema = z.object({
  businessName: z.string().min(1, 'Business name is required'),
  email: z.string().min(1, 'Email is required').email('Invalid email address'),
  contactNumber: z.string().min(1, 'Contact number is required').max(50),
  address: z.string().min(1, 'Address is required').max(512),
  branchName: z.string().min(1, 'Branch name is required'),
  branchCode: z.string().min(1, 'Branch code is required').max(20),
  branchAddress: z.string().min(1, 'Branch address is required'),
  branchContactNumber: z.string().min(1, 'Branch contact number is required'),
})

type AcceptFormValues = z.infer<typeof acceptSchema>

export default function FranchiseAcceptPage() {
  const searchParams = useSearchParams()
  const router = useRouter()
  const token = searchParams.get('token') ?? ''
  const { isSignedIn, isLoaded: authLoaded } = useAuth()
  const { user } = useUser()

  const {
    data: invitationData,
    isLoading: validating,
    error: validationError,
  } = useValidateInvitation(token)

  const acceptMutation = useAcceptInvitation()

  const form = useForm<AcceptFormValues>({
    resolver: zodResolver(acceptSchema),
    defaultValues: {
      businessName: '',
      email: '',
      contactNumber: '',
      address: '',
      branchName: '',
      branchCode: '',
      branchAddress: '',
      branchContactNumber: '',
    },
  })

  useEffect(() => {
    if (invitationData) {
      form.reset({
        businessName: invitationData.businessName ?? '',
        email: invitationData.email ?? '',
        contactNumber: '',
        address: '',
        branchName: '',
        branchCode: '',
        branchAddress: '',
        branchContactNumber: '',
      })
    }
  }, [invitationData, form])

  const onSubmit = async (values: AcceptFormValues) => {
    try {
      await acceptMutation.mutateAsync({ token, ...values })
      toast.success('Welcome to the franchise network!')
      router.push('/dashboard')
    } catch (err: unknown) {
      const apiErr = err as { title?: string }
      toast.error(apiErr?.title || 'Failed to accept invitation')
    }
  }

  // No token provided
  if (!token) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center gap-4 py-12">
          <AlertCircle className="h-12 w-12 text-destructive" />
          <p className="text-lg font-medium">No invitation token provided</p>
          <p className="text-sm text-muted-foreground">
            Please use the link from your invitation email.
          </p>
          <Link href="/">
            <Button variant="outline">Back to Home</Button>
          </Link>
        </CardContent>
      </Card>
    )
  }

  // Validating token
  if (validating) {
    return (
      <Card>
        <CardHeader className="items-center">
          <Skeleton className="h-12 w-12 rounded-full" />
          <Skeleton className="mt-4 h-6 w-48" />
          <Skeleton className="mt-2 h-4 w-64" />
        </CardHeader>
        <CardContent className="space-y-4">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-3/4" />
          <Skeleton className="h-4 w-1/2" />
        </CardContent>
      </Card>
    )
  }

  // Validation error
  if (validationError || !invitationData) {
    const errorMessage =
      (validationError as { detail?: string })?.detail ??
      (validationError as Error)?.message ??
      'This invitation is expired, already used, or not found.'

    return (
      <Card>
        <CardContent className="flex flex-col items-center gap-4 py-12">
          <AlertCircle className="h-12 w-12 text-destructive" />
          <p className="text-lg font-medium">Invalid Invitation</p>
          <p className="text-center text-sm text-muted-foreground">{errorMessage}</p>
          <Link href="/">
            <Button variant="outline">Back to Home</Button>
          </Link>
        </CardContent>
      </Card>
    )
  }

  const redirectUrl = `/franchise/accept?token=${token}`

  return (
    <Card>
      <CardHeader className="items-center text-center">
        <div className="flex h-14 w-14 items-center justify-center rounded-full bg-blue-100">
          <Droplets className="h-8 w-8 text-blue-600" />
        </div>
        <CardTitle className="mt-4 text-2xl">Franchise Invitation</CardTitle>
        <CardDescription>
          You&apos;ve been invited to join {invitationData.franchisorName}&apos;s franchise network
        </CardDescription>
      </CardHeader>

      <CardContent className="space-y-6">
        {/* Invitation details */}
        <div className="rounded-lg border bg-muted/50 p-4 space-y-2">
          <DetailRow label="Business Name" value={invitationData.businessName} />
          <DetailRow label="Email" value={invitationData.email} />
          {invitationData.territoryName && (
            <DetailRow label="Territory" value={invitationData.territoryName} />
          )}
          {invitationData.franchiseCode && (
            <DetailRow label="Franchise Code" value={invitationData.franchiseCode} />
          )}
          <DetailRow
            label="Expires At"
            value={new Date(invitationData.expiresAt).toLocaleDateString('en-PH', {
              year: 'numeric',
              month: 'long',
              day: 'numeric',
              hour: '2-digit',
              minute: '2-digit',
            })}
          />
        </div>

        {/* Auth check — wait for Clerk to load */}
        {!authLoaded ? (
          <div className="space-y-3">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-4 w-48 mx-auto" />
          </div>
        ) : !isSignedIn ? (
          /* Not signed in */
          <div className="space-y-4 text-center">
            <p className="text-sm text-muted-foreground">
              Please sign in or create an account to accept this invitation.
            </p>
            <div className="flex flex-col gap-2">
              <Link href={`/sign-in?redirect_url=${encodeURIComponent(redirectUrl)}`}>
                <Button className="w-full">Sign In</Button>
              </Link>
              <Link href={`/sign-up?redirect_url=${encodeURIComponent(redirectUrl)}`}>
                <Button variant="outline" className="w-full">Create Account</Button>
              </Link>
            </div>
          </div>
        ) : (
          /* Signed in — show acceptance form */
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            {/* Business Details */}
            <fieldset className="space-y-4">
              <legend className="text-sm font-semibold">Business Details</legend>

              <div className="space-y-1">
                <Label htmlFor="businessName">Business Name</Label>
                <Input id="businessName" {...form.register('businessName')} />
                {form.formState.errors.businessName && (
                  <p className="text-xs text-destructive">
                    {form.formState.errors.businessName.message}
                  </p>
                )}
              </div>

              <div className="space-y-1">
                <Label htmlFor="email">Email</Label>
                <Input id="email" type="email" {...form.register('email')} />
                {form.formState.errors.email && (
                  <p className="text-xs text-destructive">
                    {form.formState.errors.email.message}
                  </p>
                )}
              </div>

              <div className="space-y-1">
                <Label htmlFor="contactNumber">Contact Number</Label>
                <Input id="contactNumber" placeholder="09XX XXX XXXX" {...form.register('contactNumber')} />
                {form.formState.errors.contactNumber && (
                  <p className="text-xs text-destructive">
                    {form.formState.errors.contactNumber.message}
                  </p>
                )}
              </div>

              <div className="space-y-1">
                <Label htmlFor="address">Address</Label>
                <Input id="address" {...form.register('address')} />
                {form.formState.errors.address && (
                  <p className="text-xs text-destructive">
                    {form.formState.errors.address.message}
                  </p>
                )}
              </div>
            </fieldset>

            {/* First Branch */}
            <fieldset className="space-y-4">
              <legend className="text-sm font-semibold">First Branch</legend>

              <div className="space-y-1">
                <Label htmlFor="branchName">Branch Name</Label>
                <Input id="branchName" placeholder="e.g. Makati Branch" {...form.register('branchName')} />
                {form.formState.errors.branchName && (
                  <p className="text-xs text-destructive">
                    {form.formState.errors.branchName.message}
                  </p>
                )}
              </div>

              <div className="space-y-1">
                <Label htmlFor="branchCode">Branch Code</Label>
                <Input
                  id="branchCode"
                  placeholder="e.g. MKT"
                  maxLength={20}
                  {...form.register('branchCode')}
                />
                {form.formState.errors.branchCode && (
                  <p className="text-xs text-destructive">
                    {form.formState.errors.branchCode.message}
                  </p>
                )}
              </div>

              <div className="space-y-1">
                <Label htmlFor="branchAddress">Branch Address</Label>
                <Input id="branchAddress" {...form.register('branchAddress')} />
                {form.formState.errors.branchAddress && (
                  <p className="text-xs text-destructive">
                    {form.formState.errors.branchAddress.message}
                  </p>
                )}
              </div>

              <div className="space-y-1">
                <Label htmlFor="branchContactNumber">Branch Contact Number</Label>
                <Input
                  id="branchContactNumber"
                  placeholder="09XX XXX XXXX"
                  {...form.register('branchContactNumber')}
                />
                {form.formState.errors.branchContactNumber && (
                  <p className="text-xs text-destructive">
                    {form.formState.errors.branchContactNumber.message}
                  </p>
                )}
              </div>
            </fieldset>

            <Button
              type="submit"
              className="w-full"
              disabled={form.formState.isSubmitting || acceptMutation.isPending}
            >
              {form.formState.isSubmitting || acceptMutation.isPending ? (
                'Setting up your franchise...'
              ) : (
                <>
                  <CheckCircle className="mr-2 h-4 w-4" />
                  Accept & Set Up My Franchise
                </>
              )}
            </Button>
          </form>
        )}
      </CardContent>
    </Card>
  )
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between text-sm">
      <span className="text-muted-foreground">{label}</span>
      <span className="font-medium">{value}</span>
    </div>
  )
}
