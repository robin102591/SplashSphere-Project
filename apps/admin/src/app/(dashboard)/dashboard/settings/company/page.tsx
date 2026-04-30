'use client'

import { useEffect } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Building2 } from 'lucide-react'

import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Switch } from '@/components/ui/switch'
import { Skeleton } from '@/components/ui/skeleton'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { useCompanyProfile, useUpdateCompanyProfile } from '@/hooks/use-company-profile'
import type { ApiError, UpdateCompanyProfilePayload } from '@splashsphere/types'
import { LogoUploader } from './_components/logo-uploader'

// ── Schema ────────────────────────────────────────────────────────────────────

const optionalUrl = z
  .string()
  .trim()
  .url('Must be a valid URL.')
  .or(z.literal(''))
  .nullable()
  .optional()

const formSchema = z.object({
  name: z.string().trim().min(1, 'Business name is required.').max(256),
  tagline: z.string().trim().max(200).or(z.literal('')).nullable().optional(),

  email: z.string().trim().email('Email must be a valid address.').max(256),
  contactNumber: z.string().trim().min(1, 'Contact number is required.').max(50),
  website: optionalUrl,

  streetAddress: z.string().trim().max(256).or(z.literal('')).nullable().optional(),
  barangay: z.string().trim().max(128).or(z.literal('')).nullable().optional(),
  city: z.string().trim().max(128).or(z.literal('')).nullable().optional(),
  province: z.string().trim().max(128).or(z.literal('')).nullable().optional(),
  zipCode: z.string().trim().max(20).or(z.literal('')).nullable().optional(),

  taxId: z.string().trim().max(50).or(z.literal('')).nullable().optional(),
  businessPermitNo: z.string().trim().max(100).or(z.literal('')).nullable().optional(),
  isVatRegistered: z.boolean(),

  facebookUrl: optionalUrl,
  instagramHandle: z.string().trim().max(64).or(z.literal('')).nullable().optional(),
  gcashNumber: z.string().trim().max(50).or(z.literal('')).nullable().optional(),

  // #RRGGBB or empty. The native <input type="color"> always emits a valid
  // 7-char string; the empty case happens via the explicit "Reset" button.
  primaryColorHex: z.string().regex(/^(#[0-9A-Fa-f]{6})?$/, 'Must be a #RRGGBB hex value.').or(z.literal('')).nullable().optional(),
})

type FormValues = z.infer<typeof formSchema>

/** Convert empty/undefined strings to null so the wire matches the backend's nullable contract. */
function toNullable(s: string | null | undefined): string | null {
  if (s === null || s === undefined) return null
  const trimmed = s.trim()
  return trimmed === '' ? null : trimmed
}

function toPayload(v: FormValues): UpdateCompanyProfilePayload {
  return {
    name: v.name.trim(),
    tagline: toNullable(v.tagline),
    email: v.email.trim(),
    contactNumber: v.contactNumber.trim(),
    website: toNullable(v.website),
    streetAddress: toNullable(v.streetAddress),
    barangay: toNullable(v.barangay),
    city: toNullable(v.city),
    province: toNullable(v.province),
    zipCode: toNullable(v.zipCode),
    taxId: toNullable(v.taxId),
    businessPermitNo: toNullable(v.businessPermitNo),
    isVatRegistered: v.isVatRegistered,
    facebookUrl: toNullable(v.facebookUrl),
    instagramHandle: toNullable(v.instagramHandle),
    gcashNumber: toNullable(v.gcashNumber),
    primaryColorHex: toNullable(v.primaryColorHex),
  }
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function CompanyProfilePage() {
  const { data: profile, isLoading } = useCompanyProfile()
  const { mutateAsync: save, isPending } = useUpdateCompanyProfile()

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      tagline: '',
      email: '',
      contactNumber: '',
      website: '',
      streetAddress: '',
      barangay: '',
      city: '',
      province: '',
      zipCode: '',
      taxId: '',
      businessPermitNo: '',
      isVatRegistered: false,
      facebookUrl: '',
      instagramHandle: '',
      gcashNumber: '',
      primaryColorHex: '',
    },
  })

  // Hydrate the form once the profile loads. The reset() call is a no-op
  // when the user has already started typing (RHF keeps dirty fields).
  useEffect(() => {
    if (!profile) return
    form.reset({
      name: profile.name,
      tagline: profile.tagline ?? '',
      email: profile.email,
      contactNumber: profile.contactNumber,
      website: profile.website ?? '',
      streetAddress: profile.streetAddress ?? '',
      barangay: profile.barangay ?? '',
      city: profile.city ?? '',
      province: profile.province ?? '',
      zipCode: profile.zipCode ?? '',
      taxId: profile.taxId ?? '',
      businessPermitNo: profile.businessPermitNo ?? '',
      isVatRegistered: profile.isVatRegistered,
      facebookUrl: profile.facebookUrl ?? '',
      instagramHandle: profile.instagramHandle ?? '',
      gcashNumber: profile.gcashNumber ?? '',
      primaryColorHex: profile.primaryColorHex ?? '',
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [profile])

  const onSubmit = async (values: FormValues) => {
    try {
      await save(toPayload(values))
      toast.success('Company profile saved.')
    } catch (err) {
      const apiErr = err as ApiError
      toast.error(apiErr?.detail ?? apiErr?.title ?? 'Failed to save profile.')
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <PageHeader title="Company Profile" description="Loading your business identity…" back="/dashboard/settings" />
        <Skeleton className="h-72 w-full" />
        <Skeleton className="h-48 w-full" />
        <Skeleton className="h-56 w-full" />
      </div>
    )
  }

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6 pb-12">
      <PageHeader
        title="Company Profile"
        description="Your business identity. Used on receipts, reports, and the customer-facing Connect listing."
        back="/dashboard/settings"
        actions={
          <Button type="submit" disabled={isPending || !form.formState.isDirty}>
            {isPending ? 'Saving…' : 'Save changes'}
          </Button>
        }
      />

      {/* ── Logo ─────────────────────────────────────────────────────────── */}
      {/*
        Lives outside react-hook-form because uploads happen via their own
        mutation and don't share the form's "dirty" / "save" lifecycle.
        Saving / replacing the logo is committed immediately on file drop.
      */}
      <LogoUploader
        currentUrl={profile?.logoUrl ?? null}
        thumbnailUrl={profile?.logoThumbnailUrl ?? null}
      />

      {/* ── Identity ─────────────────────────────────────────────────────── */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Building2 className="h-4 w-4" /> Business Identity
          </CardTitle>
          <CardDescription>How your business appears on every receipt and listing.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-2">
          <Field label="Business Name" required error={form.formState.errors.name?.message}>
            <Input {...form.register('name')} placeholder="AquaShine Car Wash" />
          </Field>

          <Field label="Tagline" error={form.formState.errors.tagline?.message}>
            <Input {...form.register('tagline')} placeholder="Premium car care since 2020" />
          </Field>
        </CardContent>
      </Card>

      {/* ── Contact ──────────────────────────────────────────────────────── */}
      <Card>
        <CardHeader>
          <CardTitle>Contact Information</CardTitle>
          <CardDescription>How customers and the platform reach you.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-2">
          <Field label="Email" required error={form.formState.errors.email?.message}>
            <Input type="email" {...form.register('email')} placeholder="info@aquashine.ph" />
          </Field>

          <Field label="Phone" required error={form.formState.errors.contactNumber?.message}>
            <Input {...form.register('contactNumber')} placeholder="0917-123-4567" />
          </Field>

          <Field label="Website" className="sm:col-span-2" error={form.formState.errors.website?.message}>
            <Input {...form.register('website')} placeholder="https://aquashine.ph" />
          </Field>
        </CardContent>
      </Card>

      {/* ── Address ──────────────────────────────────────────────────────── */}
      <Card>
        <CardHeader>
          <CardTitle>Headquarters Address</CardTitle>
          <CardDescription>
            The displayed address on email receipts, reports, and invoices. Each branch carries its own
            address separately.
          </CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-2">
          <Field label="Street Address" className="sm:col-span-2">
            <Input {...form.register('streetAddress')} placeholder="123 Makati Avenue" />
          </Field>

          <Field label="Barangay">
            <Input {...form.register('barangay')} placeholder="Brgy. San Lorenzo" />
          </Field>

          <Field label="City / Municipality">
            <Input {...form.register('city')} placeholder="Makati City" />
          </Field>

          <Field label="Province">
            <Input {...form.register('province')} placeholder="Metro Manila" />
          </Field>

          <Field label="Zip Code">
            <Input {...form.register('zipCode')} placeholder="1223" />
          </Field>
        </CardContent>
      </Card>

      {/* ── Tax & Registration ───────────────────────────────────────────── */}
      <Card>
        <CardHeader>
          <CardTitle>Tax & Registration</CardTitle>
          <CardDescription>Used on official receipts and government-mandated reports.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-2">
          <Field label="TIN (Tax Identification Number)">
            <Input {...form.register('taxId')} placeholder="123-456-789-000" />
          </Field>

          <Field label="Business Registration (DTI / SEC / CDA)">
            <Input {...form.register('businessPermitNo')} placeholder="DTI-NCR-2020-12345" />
          </Field>

          <div className="sm:col-span-2 flex items-start justify-between gap-4 rounded-md border p-4">
            <div className="space-y-1">
              <Label htmlFor="isVatRegistered" className="text-sm font-medium">
                VAT Registered
              </Label>
              <p className="text-xs text-muted-foreground">
                Toggle on if your business charges 12% VAT on receipts. Most car washes operate as
                Non-VAT.
              </p>
            </div>
            <Switch
              id="isVatRegistered"
              checked={form.watch('isVatRegistered')}
              onCheckedChange={(v) => form.setValue('isVatRegistered', v, { shouldDirty: true })}
            />
          </div>
        </CardContent>
      </Card>

      {/* ── Social & Payment ─────────────────────────────────────────────── */}
      <Card>
        <CardHeader>
          <CardTitle>Social & Payment</CardTitle>
          <CardDescription>Optional — shown in the receipt footer when enabled in the receipt designer.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 sm:grid-cols-2">
          <Field label="Facebook Page URL" className="sm:col-span-2" error={form.formState.errors.facebookUrl?.message}>
            <Input {...form.register('facebookUrl')} placeholder="https://facebook.com/aquashinecarwash" />
          </Field>

          <Field label="Instagram Handle">
            <Input {...form.register('instagramHandle')} placeholder="@aquashinecarwash" />
          </Field>

          <Field label="GCash Number">
            <Input {...form.register('gcashNumber')} placeholder="0917-123-4567" />
          </Field>
        </CardContent>
      </Card>

      {/* ── Brand color (drives the customer-display "Brand" theme) ───────── */}
      <Card>
        <CardHeader>
          <CardTitle>Brand color</CardTitle>
          <CardDescription>
            Used as accent on the customer-facing display when the &ldquo;Brand&rdquo;
            theme is selected. Leave blank to use the default blue.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <BrandColorField form={form} />
        </CardContent>
      </Card>

      {/* Sticky-ish bottom save (also up top in the header) */}
      <div className="flex justify-end pt-2">
        <Button type="submit" disabled={isPending || !form.formState.isDirty}>
          {isPending ? 'Saving…' : 'Save changes'}
        </Button>
      </div>
    </form>
  )
}

// ── Field ─────────────────────────────────────────────────────────────────────
// Tiny labeled-control wrapper. Kept inline because it has zero state and is
// only useful inside this page; promoting it to /components/ui without a
// second caller would just be premature.

function Field({
  label,
  required,
  error,
  className,
  children,
}: {
  label: string
  required?: boolean
  error?: string
  className?: string
  children: React.ReactNode
}) {
  return (
    <div className={`space-y-1.5 ${className ?? ''}`}>
      <Label className="text-sm font-medium">
        {label}
        {required && <span className="ml-0.5 text-destructive">*</span>}
      </Label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  )
}

// ── BrandColorField ───────────────────────────────────────────────────────────
// Native color picker + a #RRGGBB text mirror + a Reset button. The picker
// always emits a valid 7-char hex; the text input is a convenience for
// designers who want to paste in a known brand color.

function BrandColorField({
  form,
}: {
  form: ReturnType<typeof useForm<FormValues>>
}) {
  const value = useWatch({ control: form.control, name: 'primaryColorHex' }) ?? ''
  const isSet = /^#[0-9A-Fa-f]{6}$/.test(value)
  const swatch = isSet ? value : '#3B82F6' // default blue when unset

  return (
    <div className="flex flex-wrap items-center gap-3">
      <input
        type="color"
        aria-label="Pick brand color"
        value={swatch}
        onChange={(e) => form.setValue('primaryColorHex', e.target.value.toUpperCase(), { shouldDirty: true })}
        className="h-10 w-14 cursor-pointer rounded-md border border-input bg-transparent p-1"
      />
      <Input
        {...form.register('primaryColorHex')}
        placeholder="#3B82F6"
        className="w-32 font-mono uppercase"
        maxLength={7}
      />
      {!isSet && (
        <span className="text-xs text-muted-foreground">Default — splash blue</span>
      )}
      {isSet && (
        <button
          type="button"
          onClick={() => form.setValue('primaryColorHex', '', { shouldDirty: true })}
          className="text-xs text-muted-foreground hover:text-foreground underline-offset-4 hover:underline"
        >
          Reset to default
        </button>
      )}
    </div>
  )
}
