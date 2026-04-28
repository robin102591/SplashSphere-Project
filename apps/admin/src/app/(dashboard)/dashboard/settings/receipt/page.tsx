'use client'

import { useEffect } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { Receipt as ReceiptIcon } from 'lucide-react'

import {
  LogoPosition,
  LogoSize,
  ReceiptFontSize,
  ReceiptWidth,
} from '@splashsphere/types'
import type {
  ApiError,
  ReceiptSettingDto,
  UpdateReceiptSettingPayload,
} from '@splashsphere/types'

import { PageHeader } from '@/components/ui/page-header'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Switch } from '@/components/ui/switch'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Card, CardContent, CardDescription, CardHeader, CardTitle,
} from '@/components/ui/card'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  useReceiptSetting,
  useUpdateReceiptSetting,
} from '@/hooks/use-receipt-settings'
import { useCompanyProfile } from '@/hooks/use-company-profile'
import { formatPeso } from '@splashsphere/format'

// ── Schema ────────────────────────────────────────────────────────────────────

const formSchema = z.object({
  // Header
  showLogo: z.boolean(),
  logoSize: z.nativeEnum(LogoSize),
  logoPosition: z.nativeEnum(LogoPosition),
  showBusinessName: z.boolean(),
  showTagline: z.boolean(),
  showBranchName: z.boolean(),
  showBranchAddress: z.boolean(),
  showBranchContact: z.boolean(),
  showTIN: z.boolean(),
  customHeaderText: z.string().max(256).or(z.literal('')).nullable().optional(),

  // Body
  showServiceDuration: z.boolean(),
  showEmployeeNames: z.boolean(),
  showVehicleInfo: z.boolean(),
  showDiscountBreakdown: z.boolean(),
  showTaxLine: z.boolean(),
  showTransactionNumber: z.boolean(),
  showDateTime: z.boolean(),
  showCashierName: z.boolean(),

  // Customer
  showCustomerName: z.boolean(),
  showCustomerPhone: z.boolean(),
  showLoyaltyPointsEarned: z.boolean(),
  showLoyaltyBalance: z.boolean(),
  showLoyaltyTier: z.boolean(),

  // Footer
  thankYouMessage: z.string().trim().min(1, 'Thank-you message is required.').max(256),
  promoText: z.string().max(512).or(z.literal('')).nullable().optional(),
  showSocialMedia: z.boolean(),
  showGCashQr: z.boolean(),
  showGCashNumber: z.boolean(),
  customFooterText: z.string().max(512).or(z.literal('')).nullable().optional(),

  // Format
  receiptWidth: z.nativeEnum(ReceiptWidth),
  fontSize: z.nativeEnum(ReceiptFontSize),
  autoCutPaper: z.boolean(),
})

type FormValues = z.infer<typeof formSchema>

function toNullable(s: string | null | undefined): string | null {
  if (s === null || s === undefined) return null
  const trimmed = s.trim()
  return trimmed === '' ? null : trimmed
}

function dtoToForm(d: ReceiptSettingDto): FormValues {
  return {
    ...d,
    customHeaderText: d.customHeaderText ?? '',
    promoText: d.promoText ?? '',
    customFooterText: d.customFooterText ?? '',
  }
}

function formToPayload(v: FormValues): UpdateReceiptSettingPayload {
  return {
    ...v,
    customHeaderText: toNullable(v.customHeaderText),
    promoText: toNullable(v.promoText),
    customFooterText: toNullable(v.customFooterText),
    thankYouMessage: v.thankYouMessage.trim(),
  }
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function ReceiptSettingsPage() {
  const { data: setting, isLoading } = useReceiptSetting()
  const { data: company } = useCompanyProfile()
  const { mutateAsync: save, isPending } = useUpdateReceiptSetting()

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    // Empty placeholder defaults — replaced by reset() once the setting loads.
    defaultValues: {
      showLogo: true, logoSize: LogoSize.Medium, logoPosition: LogoPosition.Center,
      showBusinessName: true, showTagline: true, showBranchName: true,
      showBranchAddress: true, showBranchContact: true, showTIN: false,
      customHeaderText: '',
      showServiceDuration: true, showEmployeeNames: true, showVehicleInfo: true,
      showDiscountBreakdown: true, showTaxLine: false, showTransactionNumber: true,
      showDateTime: true, showCashierName: true,
      showCustomerName: true, showCustomerPhone: false, showLoyaltyPointsEarned: true,
      showLoyaltyBalance: true, showLoyaltyTier: true,
      thankYouMessage: 'Thank you for your patronage!',
      promoText: '', showSocialMedia: true, showGCashQr: false, showGCashNumber: false,
      customFooterText: '',
      receiptWidth: ReceiptWidth.Mm58, fontSize: ReceiptFontSize.Normal, autoCutPaper: true,
    },
  })

  useEffect(() => {
    if (setting) form.reset(dtoToForm(setting))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [setting])

  const onSubmit = async (values: FormValues) => {
    try {
      await save({ payload: formToPayload(values) })
      toast.success('Receipt settings saved.')
    } catch (err) {
      const apiErr = err as ApiError
      toast.error(apiErr?.detail ?? apiErr?.title ?? 'Failed to save settings.')
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <PageHeader title="Receipt Designer" description="Loading…" back="/dashboard/settings" />
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6 pb-12">
      <PageHeader
        title="Receipt Designer"
        description="Control what appears on every printed and digital receipt. Live preview on the right."
        back="/dashboard/settings"
        actions={
          <Button type="submit" disabled={isPending || !form.formState.isDirty}>
            {isPending ? 'Saving…' : 'Save changes'}
          </Button>
        }
      />

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_320px]">
        {/* ── Form column ─────────────────────────────────────────────── */}
        <div className="space-y-6 min-w-0">
          <HeaderSection form={form} />
          <BodySection form={form} />
          <CustomerSection form={form} />
          <FooterSection form={form} />
          <FormatSection form={form} />
        </div>

        {/* ── Preview column ─────────────────────────────────────────── */}
        <div className="lg:sticky lg:top-4 lg:self-start">
          <ReceiptPreview
            form={form}
            businessName={company?.name ?? 'Your Business'}
            tagline={company?.tagline ?? null}
            taxId={company?.taxId ?? null}
            address={company?.address ?? null}
            contact={company?.contactNumber ?? null}
            facebookUrl={company?.facebookUrl ?? null}
            instagramHandle={company?.instagramHandle ?? null}
            gcashNumber={company?.gcashNumber ?? null}
            logoUrl={company?.logoThumbnailUrl ?? null}
          />
        </div>
      </div>

      <div className="flex justify-end pt-2">
        <Button type="submit" disabled={isPending || !form.formState.isDirty}>
          {isPending ? 'Saving…' : 'Save changes'}
        </Button>
      </div>
    </form>
  )
}

// ── Sections ──────────────────────────────────────────────────────────────────
//
// Each section is a presentational, parent-controlled component — `form` is
// passed in so we don't recreate `useForm` per section. Splitting them out
// keeps the page file under 350 lines and mirrors the receipt's own zones.

type FormApi = ReturnType<typeof useForm<FormValues>>

function HeaderSection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Header</CardTitle>
        <CardDescription>What appears at the top of every receipt.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <Toggle form={form} name="showLogo" label="Show logo" hint="Upload your logo on the Company Profile page." />

        <div className="grid gap-3 sm:grid-cols-2">
          <SelectField form={form} name="logoSize" label="Logo size" options={[
            { value: LogoSize.Small,  label: 'Small'  },
            { value: LogoSize.Medium, label: 'Medium' },
            { value: LogoSize.Large,  label: 'Large'  },
          ]} />
          <SelectField form={form} name="logoPosition" label="Logo position" options={[
            { value: LogoPosition.Left,   label: 'Left'   },
            { value: LogoPosition.Center, label: 'Center' },
          ]} />
        </div>

        <Toggle form={form} name="showBusinessName"  label="Show business name" />
        <Toggle form={form} name="showTagline"       label="Show tagline" />
        <Toggle form={form} name="showBranchName"    label="Show branch name" />
        <Toggle form={form} name="showBranchAddress" label="Show branch address" />
        <Toggle form={form} name="showBranchContact" label="Show branch contact" />
        <Toggle form={form} name="showTIN"           label="Show TIN" hint="Required for VAT-registered businesses." />

        <div className="space-y-1.5">
          <Label className="text-sm font-medium">Custom header text</Label>
          <Input {...form.register('customHeaderText')} placeholder="VAT-Exempt | Serve with Excellence" />
          <p className="text-xs text-muted-foreground">Appears below the address. Leave blank to omit.</p>
        </div>
      </CardContent>
    </Card>
  )
}

function BodySection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Body</CardTitle>
        <CardDescription>Transaction lines, totals, and metadata. Pricing itself is always shown.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <Toggle form={form} name="showServiceDuration"   label="Show service duration" />
        <Toggle form={form} name="showEmployeeNames"     label="Show employee names" hint="Who performed the service." />
        <Toggle form={form} name="showVehicleInfo"       label="Show vehicle info" hint="Plate, make/model, type." />
        <Toggle form={form} name="showDiscountBreakdown" label="Show discount breakdown" />
        <Toggle form={form} name="showTaxLine"           label="Show tax line" hint="Off by default — most car washes are non-VAT." />
        <Toggle form={form} name="showTransactionNumber" label="Show transaction number" />
        <Toggle form={form} name="showDateTime"          label="Show date & time" />
        <Toggle form={form} name="showCashierName"       label="Show cashier name" />
      </CardContent>
    </Card>
  )
}

function CustomerSection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Customer & Loyalty</CardTitle>
        <CardDescription>Personal info shown when the transaction has a linked customer.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <Toggle form={form} name="showCustomerName"        label="Show customer name" />
        <Toggle form={form} name="showCustomerPhone"       label="Show customer phone" hint="Privacy-sensitive — off by default." />
        <Toggle form={form} name="showLoyaltyPointsEarned" label="Show loyalty points earned" />
        <Toggle form={form} name="showLoyaltyBalance"      label="Show loyalty balance" />
        <Toggle form={form} name="showLoyaltyTier"         label="Show loyalty tier" />
      </CardContent>
    </Card>
  )
}

function FooterSection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Footer</CardTitle>
        <CardDescription>Thank-you message, promotions, and payment info.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-1.5">
          <Label className="text-sm font-medium">
            Thank-you message <span className="text-destructive">*</span>
          </Label>
          <Input {...form.register('thankYouMessage')} placeholder="Thank you for your patronage!" />
          {form.formState.errors.thankYouMessage && (
            <p className="text-xs text-destructive">{form.formState.errors.thankYouMessage.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label className="text-sm font-medium">Promo text</Label>
          <Textarea {...form.register('promoText')} placeholder="Next wash 10% off! Show this receipt." rows={2} />
          <p className="text-xs text-muted-foreground">Optional marketing message. Leave blank to omit.</p>
        </div>

        <Toggle form={form} name="showSocialMedia" label="Show social media handles" hint="Pulled from the company profile." />
        <Toggle form={form} name="showGCashNumber" label="Show GCash number" />
        <Toggle form={form} name="showGCashQr"     label="Show GCash QR code" hint="Requires uploading the QR image (slice 3)." />

        <div className="space-y-1.5">
          <Label className="text-sm font-medium">Custom footer text</Label>
          <Textarea {...form.register('customFooterText')} placeholder="This serves as your Official Receipt. Items sold are non-refundable." rows={2} />
        </div>
      </CardContent>
    </Card>
  )
}

function FormatSection({ form }: { form: FormApi }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Format</CardTitle>
        <CardDescription>Physical paper width, font, and printer behaviour.</CardDescription>
      </CardHeader>
      <CardContent className="grid gap-3 sm:grid-cols-2">
        <SelectField form={form} name="receiptWidth" label="Receipt width" options={[
          { value: ReceiptWidth.Mm58, label: '58mm thermal' },
          { value: ReceiptWidth.Mm80, label: '80mm thermal' },
        ]} />
        <SelectField form={form} name="fontSize" label="Font size" options={[
          { value: ReceiptFontSize.Small,  label: 'Small'  },
          { value: ReceiptFontSize.Normal, label: 'Normal' },
          { value: ReceiptFontSize.Large,  label: 'Large'  },
        ]} />
        <div className="sm:col-span-2">
          <Toggle form={form} name="autoCutPaper" label="Auto-cut paper after print" hint="If your printer supports it." />
        </div>
      </CardContent>
    </Card>
  )
}

// ── Reusable controls ─────────────────────────────────────────────────────────

function Toggle({
  form, name, label, hint,
}: {
  form: FormApi
  name: keyof FormValues
  label: string
  hint?: string
}) {
  const checked = form.watch(name) as boolean
  return (
    <div className="flex items-start justify-between gap-4">
      <div className="space-y-0.5">
        <Label htmlFor={name} className="text-sm font-medium">{label}</Label>
        {hint && <p className="text-xs text-muted-foreground">{hint}</p>}
      </div>
      <Switch
        id={name}
        checked={checked}
        onCheckedChange={(v) => form.setValue(name, v as never, { shouldDirty: true })}
      />
    </div>
  )
}

function SelectField<TName extends keyof FormValues>({
  form, name, label, options,
}: {
  form: FormApi
  name: TName
  label: string
  options: { value: number; label: string }[]
}) {
  const value = form.watch(name) as number
  return (
    <div className="space-y-1.5">
      <Label className="text-sm font-medium">{label}</Label>
      <Select
        value={String(value)}
        onValueChange={(v) => form.setValue(name, Number(v) as never, { shouldDirty: true })}
      >
        <SelectTrigger><SelectValue /></SelectTrigger>
        <SelectContent>
          {options.map((o) => (
            <SelectItem key={o.value} value={String(o.value)}>{o.label}</SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  )
}

// ── Live preview ──────────────────────────────────────────────────────────────
//
// Pure-React render of a thermal-style receipt. Reads the form's current
// values via useWatch (subscribes to changes without re-rendering parents).
// Sample data is hard-coded — once the user is happy with the toggles, the
// PDF backend renders the real thing using slice 2.6's wired-up settings.

const SAMPLE = {
  txn: 'TXN-MAK-0426-0012',
  date: 'Apr 26, 2026 2:41 PM',
  cashier: 'Ana',
  vehicle: { plate: 'ABC-1234', makeModel: 'Toyota Vios', typeSize: 'Sedan / Medium' },
  services: [
    { name: 'Basic Wash',       price: 200, duration: 30, employees: ['Juan', 'Pedro'] },
    { name: 'Tire & Rim Shine', price: 120, duration: 10, employees: ['Juan'] },
  ],
  subtotal: 320,
  discountLabel: 'Gold 10%',
  discount: 32,
  tax: 0,
  total: 288,
  paid: 300,
  change: 12,
  customer: { name: 'Maria Santos', phone: '0917-000-1234' },
  loyalty: { tier: 'Gold', earned: 29, balance: 1269 },
}

function ReceiptPreview({
  form, businessName, tagline, taxId, address, contact,
  facebookUrl, instagramHandle, gcashNumber, logoUrl,
}: {
  form: FormApi
  businessName: string
  tagline: string | null
  taxId: string | null
  address: string | null
  contact: string | null
  facebookUrl: string | null
  instagramHandle: string | null
  gcashNumber: string | null
  logoUrl: string | null
}) {
  // Subscribe to all form values so every toggle re-renders the preview.
  const v = useWatch({ control: form.control })

  const fontSize = v.fontSize === ReceiptFontSize.Small ? '11px'
    : v.fontSize === ReceiptFontSize.Large ? '15px'
    : '13px'
  const widthClass = v.receiptWidth === ReceiptWidth.Mm80 ? 'w-[280px]' : 'w-[220px]'

  return (
    <div>
      <div className="mb-2 flex items-center gap-2 text-xs text-muted-foreground">
        <ReceiptIcon className="h-3.5 w-3.5" />
        Preview ({v.receiptWidth === ReceiptWidth.Mm80 ? '80mm' : '58mm'})
      </div>
      <div
        className={`${widthClass} rounded-md border border-dashed bg-white p-3 font-mono text-black shadow-sm`}
        style={{ fontSize }}
      >
        {/* Header */}
        <div className={v.logoPosition === LogoPosition.Left ? 'text-left' : 'text-center'}>
          {v.showLogo && (() => {
            const px = v.logoSize === LogoSize.Small ? 36 : v.logoSize === LogoSize.Large ? 72 : 54
            // Real logo when uploaded; gray placeholder otherwise so the
            // toggle's effect is visible even on a fresh tenant.
            return logoUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={logoUrl}
                alt="Logo"
                className={v.logoPosition === LogoPosition.Left ? 'mb-1.5' : 'mx-auto mb-1.5'}
                style={{ width: px, height: px, objectFit: 'contain' }}
              />
            ) : (
              <div
                className="mx-auto mb-1.5 rounded bg-gray-200 text-[9px] uppercase text-gray-500 flex items-center justify-center"
                style={{ width: px, height: px }}
              >
                logo
              </div>
            )
          })()}
          {v.showBusinessName && <div className="font-bold">{businessName}</div>}
          {v.showTagline && tagline && <div className="text-[0.85em] italic">{tagline}</div>}
          {v.showBranchName    && <div>Makati Branch</div>}
          {v.showBranchAddress && address  && <div className="text-[0.9em]">{address}</div>}
          {v.showBranchContact && contact  && <div className="text-[0.9em]">Tel: {contact}</div>}
          {v.showTIN           && taxId    && <div className="text-[0.9em]">TIN: {taxId}</div>}
          {v.customHeaderText  && <div className="text-[0.9em] mt-1">{v.customHeaderText}</div>}
        </div>

        <Divider />

        {/* Body — meta */}
        {v.showTransactionNumber && <div>TXN: {SAMPLE.txn}</div>}
        {v.showDateTime          && <div>Date: {SAMPLE.date}</div>}
        {v.showCashierName       && <div>Cashier: {SAMPLE.cashier}</div>}

        {(v.showTransactionNumber || v.showDateTime || v.showCashierName) && <Divider />}

        {/* Body — vehicle */}
        {v.showVehicleInfo && (
          <>
            <div>Vehicle: {SAMPLE.vehicle.makeModel}</div>
            <div>Plate: {SAMPLE.vehicle.plate}</div>
            <div>Type: {SAMPLE.vehicle.typeSize}</div>
            <Divider />
          </>
        )}

        {/* Body — line items */}
        {SAMPLE.services.map((s, i) => (
          <div key={i}>
            <div className="flex justify-between">
              <span className="truncate">{s.name}</span>
              <span className="tabular-nums">{formatPeso(s.price)}</span>
            </div>
            {(v.showServiceDuration || v.showEmployeeNames) && (
              <div className="text-[0.85em] text-gray-600">
                {[
                  v.showServiceDuration ? `${s.duration} min` : null,
                  v.showEmployeeNames ? s.employees.join(' & ') : null,
                ].filter(Boolean).join(', ')}
              </div>
            )}
          </div>
        ))}

        <Divider />

        {/* Totals */}
        <div className="flex justify-between"><span>Subtotal</span><span className="tabular-nums">{formatPeso(SAMPLE.subtotal)}</span></div>
        {v.showDiscountBreakdown && (
          <div className="flex justify-between"><span>Discount ({SAMPLE.discountLabel})</span><span className="tabular-nums">-{formatPeso(SAMPLE.discount)}</span></div>
        )}
        {v.showTaxLine && (
          <div className="flex justify-between"><span>VAT</span><span className="tabular-nums">{formatPeso(SAMPLE.tax)}</span></div>
        )}
        <div className="my-1 border-t border-double border-black" />
        <div className="flex justify-between font-bold"><span>TOTAL</span><span className="tabular-nums">{formatPeso(SAMPLE.total)}</span></div>
        <div className="my-1 border-t border-double border-black" />
        <div className="flex justify-between"><span>Cash</span><span className="tabular-nums">{formatPeso(SAMPLE.paid)}</span></div>
        <div className="flex justify-between"><span>Change</span><span className="tabular-nums">{formatPeso(SAMPLE.change)}</span></div>

        {/* Customer & loyalty */}
        {(v.showCustomerName || v.showCustomerPhone || v.showLoyaltyTier || v.showLoyaltyPointsEarned || v.showLoyaltyBalance) && (
          <>
            <Divider />
            {v.showCustomerName        && <div>Customer: {SAMPLE.customer.name}</div>}
            {v.showCustomerPhone       && <div>Phone: {SAMPLE.customer.phone}</div>}
            {v.showLoyaltyTier         && <div>{SAMPLE.loyalty.tier} Member ★</div>}
            {v.showLoyaltyPointsEarned && <div>Points Earned: +{SAMPLE.loyalty.earned}</div>}
            {v.showLoyaltyBalance      && <div>Balance: {SAMPLE.loyalty.balance.toLocaleString()} pts</div>}
          </>
        )}

        {/* Footer */}
        <Divider />
        <div className="text-center font-medium">{v.thankYouMessage}</div>
        {v.promoText && <div className="mt-1.5 text-center text-[0.9em]">{v.promoText}</div>}

        {v.showSocialMedia && (facebookUrl || instagramHandle) && (
          <div className="mt-2 text-center text-[0.85em]">
            {facebookUrl     && <div>FB: {facebookUrl.replace(/^https?:\/\/(www\.)?facebook\.com/, '')}</div>}
            {instagramHandle && <div>IG: {instagramHandle}</div>}
          </div>
        )}

        {v.showGCashQr && (
          <div className="mt-2 flex justify-center">
            <div className="h-16 w-16 rounded border border-dashed border-gray-400 bg-gray-50 text-[8px] text-gray-500 flex items-center justify-center">
              GCash QR
            </div>
          </div>
        )}
        {v.showGCashNumber && gcashNumber && (
          <div className="mt-1 text-center text-[0.9em]">GCash: {gcashNumber}</div>
        )}

        {v.customFooterText && (
          <div className="mt-2 text-center text-[0.85em] text-gray-700">{v.customFooterText}</div>
        )}
      </div>
    </div>
  )
}

function Divider() {
  return <div className="my-1 border-t border-dashed border-gray-400" />
}
