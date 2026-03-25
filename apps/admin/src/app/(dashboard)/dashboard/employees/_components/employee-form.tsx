'use client'

import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { DatePicker } from '@/components/ui/date-picker'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useBranches } from '@/hooks/use-branches'
import { EmployeeType } from '@splashsphere/types'
import type { CreateEmployeeValues, UpdateEmployeeValues } from '@/hooks/use-employees'

// ── Schemas ───────────────────────────────────────────────────────────────────

const createSchema = z
  .object({
    branchId: z.string().min(1, 'Branch is required'),
    firstName: z.string().min(1, 'First name is required'),
    lastName: z.string().min(1, 'Last name is required'),
    employeeType: z.nativeEnum(EmployeeType),
    dailyRate: z.coerce.number().positive('Must be positive').optional(),
    email: z.string().email('Invalid email').optional().or(z.literal('')),
    contactNumber: z.string().optional(),
    hiredDate: z.string().optional(),
  })
  .refine(
    (v) =>
      v.employeeType !== EmployeeType.Daily ||
      (v.dailyRate !== undefined && v.dailyRate > 0),
    { message: 'Daily rate is required for daily-rate employees', path: ['dailyRate'] }
  )

const updateSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  dailyRate: z.coerce.number().positive('Must be positive').optional(),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  contactNumber: z.string().optional(),
  hiredDate: z.string().optional(),
})

type CreateFormValues = z.infer<typeof createSchema>
type UpdateFormValues = z.infer<typeof updateSchema>

// ── Create form ───────────────────────────────────────────────────────────────

interface CreateEmployeeFormProps {
  onSubmit: (values: CreateEmployeeValues) => Promise<void>
  submitLabel?: string
}

export function CreateEmployeeForm({
  onSubmit,
  submitLabel = 'Create Employee',
}: CreateEmployeeFormProps) {
  const { data: branches = [] } = useBranches()

  const { register, handleSubmit, watch, setValue, control, formState } = useForm<CreateFormValues>({
    resolver: zodResolver(createSchema),
    defaultValues: {
      branchId: '',
      firstName: '',
      lastName: '',
      employeeType: EmployeeType.Commission,
    },
  })

  const employeeType = watch('employeeType')

  const handleFormSubmit = async (values: CreateFormValues) => {
    await onSubmit({
      ...values,
      email: values.email || undefined,
      contactNumber: values.contactNumber || undefined,
      hiredDate: values.hiredDate || undefined,
    })
  }

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
      <div className="space-y-1.5">
        <Label>Branch <span className="text-destructive">*</span></Label>
        <Select onValueChange={(v) => setValue('branchId', v, { shouldValidate: true })}>
          <SelectTrigger>
            <SelectValue placeholder="Select branch…" />
          </SelectTrigger>
          <SelectContent>
            {branches.map((b) => (
              <SelectItem key={b.id} value={b.id}>
                {b.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {formState.errors.branchId && (
          <p className="text-xs text-destructive">{formState.errors.branchId.message}</p>
        )}
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1.5">
          <Label htmlFor="emp-first">First name <span className="text-destructive">*</span></Label>
          <Input id="emp-first" placeholder="Juan" {...register('firstName')} />
          {formState.errors.firstName && (
            <p className="text-xs text-destructive">{formState.errors.firstName.message}</p>
          )}
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="emp-last">Last name <span className="text-destructive">*</span></Label>
          <Input id="emp-last" placeholder="dela Cruz" {...register('lastName')} />
          {formState.errors.lastName && (
            <p className="text-xs text-destructive">{formState.errors.lastName.message}</p>
          )}
        </div>
      </div>

      <div className="space-y-1.5">
        <Label>Employee type <span className="text-destructive">*</span></Label>
        <Select
          defaultValue={String(EmployeeType.Commission)}
          onValueChange={(v) =>
            setValue('employeeType', Number(v) as EmployeeType, { shouldValidate: true })
          }
        >
          <SelectTrigger>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={String(EmployeeType.Commission)}>Commission</SelectItem>
            <SelectItem value={String(EmployeeType.Daily)}>Daily Rate</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {employeeType === EmployeeType.Daily && (
        <div className="space-y-1.5">
          <Label htmlFor="emp-daily-rate">Daily rate (₱) <span className="text-destructive">*</span></Label>
          <Input
            id="emp-daily-rate"
            type="number"
            step="0.01"
            placeholder="500.00"
            {...register('dailyRate')}
          />
          {formState.errors.dailyRate && (
            <p className="text-xs text-destructive">{formState.errors.dailyRate.message}</p>
          )}
        </div>
      )}

      <div className="space-y-1.5">
        <Label htmlFor="emp-email">Email (optional)</Label>
        <Input
          id="emp-email"
          type="email"
          placeholder="juan@example.com"
          {...register('email')}
        />
        {formState.errors.email && (
          <p className="text-xs text-destructive">{formState.errors.email.message}</p>
        )}
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="emp-contact">Contact number (optional)</Label>
        <Input id="emp-contact" placeholder="09XXXXXXXXX" {...register('contactNumber')} />
      </div>

      <div className="space-y-1.5">
        <Label>Hired date (optional)</Label>
        <Controller
          control={control}
          name="hiredDate"
          render={({ field }) => (
            <DatePicker
              value={field.value ?? ''}
              onChange={field.onChange}
              placeholder="Select hired date"
              className="w-full"
            />
          )}
        />
      </div>

      <div className="flex justify-end pt-1">
        <Button type="submit" disabled={formState.isSubmitting}>
          {formState.isSubmitting ? 'Saving…' : submitLabel}
        </Button>
      </div>
    </form>
  )
}

// ── Edit form ─────────────────────────────────────────────────────────────────

interface EditEmployeeFormProps {
  defaultValues: UpdateEmployeeValues & { employeeType: EmployeeType }
  onSubmit: (values: UpdateEmployeeValues) => Promise<void>
}

export function EditEmployeeForm({ defaultValues, onSubmit }: EditEmployeeFormProps) {
  const { register, handleSubmit, watch, control, formState } = useForm<UpdateFormValues>({
    resolver: zodResolver(updateSchema),
    defaultValues: {
      firstName: defaultValues.firstName,
      lastName: defaultValues.lastName,
      dailyRate: defaultValues.dailyRate ?? undefined,
      email: defaultValues.email ?? '',
      contactNumber: defaultValues.contactNumber ?? '',
      hiredDate: defaultValues.hiredDate ?? '',
    },
  })

  const handleFormSubmit = async (values: UpdateFormValues) => {
    await onSubmit({
      ...values,
      email: values.email || undefined,
      contactNumber: values.contactNumber || undefined,
      hiredDate: values.hiredDate || undefined,
    })
  }

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
      <div className="grid grid-cols-2 gap-3">
        <div className="space-y-1.5">
          <Label htmlFor="edit-emp-first">First name <span className="text-destructive">*</span></Label>
          <Input id="edit-emp-first" {...register('firstName')} />
          {formState.errors.firstName && (
            <p className="text-xs text-destructive">{formState.errors.firstName.message}</p>
          )}
        </div>
        <div className="space-y-1.5">
          <Label htmlFor="edit-emp-last">Last name <span className="text-destructive">*</span></Label>
          <Input id="edit-emp-last" {...register('lastName')} />
          {formState.errors.lastName && (
            <p className="text-xs text-destructive">{formState.errors.lastName.message}</p>
          )}
        </div>
      </div>

      <div className="space-y-1 rounded-md border bg-muted/40 px-3 py-2">
        <p className="text-xs text-muted-foreground">Employee type</p>
        <p className="text-sm font-medium">
          {defaultValues.employeeType === EmployeeType.Commission ? 'Commission' : 'Daily Rate'}
        </p>
        <p className="text-xs text-muted-foreground italic">
          Employee type cannot be changed after creation.
        </p>
      </div>

      {defaultValues.employeeType === EmployeeType.Daily && (
        <div className="space-y-1.5">
          <Label htmlFor="edit-emp-daily-rate">Daily rate (₱) <span className="text-destructive">*</span></Label>
          <Input
            id="edit-emp-daily-rate"
            type="number"
            step="0.01"
            {...register('dailyRate')}
          />
          {formState.errors.dailyRate && (
            <p className="text-xs text-destructive">{formState.errors.dailyRate.message}</p>
          )}
        </div>
      )}

      <div className="space-y-1.5">
        <Label htmlFor="edit-emp-email">Email (optional)</Label>
        <Input id="edit-emp-email" type="email" {...register('email')} />
        {formState.errors.email && (
          <p className="text-xs text-destructive">{formState.errors.email.message}</p>
        )}
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="edit-emp-contact">Contact number (optional)</Label>
        <Input id="edit-emp-contact" {...register('contactNumber')} />
      </div>

      <div className="space-y-1.5">
        <Label>Hired date (optional)</Label>
        <Controller
          control={control}
          name="hiredDate"
          render={({ field }) => (
            <DatePicker
              value={field.value ?? ''}
              onChange={field.onChange}
              placeholder="Select hired date"
              className="w-full"
            />
          )}
        />
      </div>

      <div className="flex justify-end pt-1">
        <Button type="submit" disabled={formState.isSubmitting}>
          {formState.isSubmitting ? 'Saving…' : 'Save Changes'}
        </Button>
      </div>
    </form>
  )
}
