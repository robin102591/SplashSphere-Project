'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import type { BranchFormValues } from '@/hooks/use-branches'

const branchSchema = z.object({
  name: z.string().min(2, 'Name is required'),
  code: z
    .string()
    .min(2, 'Code is required')
    .max(10, 'Max 10 characters')
    .regex(/^[A-Z0-9]+$/, 'Uppercase letters and numbers only'),
  address: z.string().min(5, 'Address is required'),
  contactNumber: z.string().min(7, 'Contact number is required'),
})

interface BranchFormProps {
  defaultValues?: Partial<BranchFormValues>
  onSubmit: (values: BranchFormValues) => Promise<void>
  submitLabel?: string
}

export function BranchForm({ defaultValues, onSubmit, submitLabel = 'Save' }: BranchFormProps) {
  const { register, handleSubmit, setValue, watch, formState } = useForm<BranchFormValues>({
    resolver: zodResolver(branchSchema),
    defaultValues: {
      name: '',
      code: '',
      address: '',
      contactNumber: '',
      ...defaultValues,
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <div className="col-span-2 space-y-1.5">
          <Label htmlFor="name">Branch name</Label>
          <Input id="name" placeholder="Makati Main Branch" {...register('name')} />
          {formState.errors.name && (
            <p className="text-xs text-destructive">{formState.errors.name.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="code">Branch code</Label>
          <Input
            id="code"
            placeholder="MKT"
            className="uppercase"
            {...register('code', {
              onChange: (e) => {
                e.target.value = e.target.value.toUpperCase()
                setValue('code', e.target.value)
              },
            })}
          />
          <p className="text-xs text-muted-foreground">
            Used in transaction numbers (e.g. {watch('code') || 'MKT'}-20240101-001)
          </p>
          {formState.errors.code && (
            <p className="text-xs text-destructive">{formState.errors.code.message}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="contactNumber">Contact number</Label>
          <Input id="contactNumber" placeholder="+63 917 123 4567" {...register('contactNumber')} />
          {formState.errors.contactNumber && (
            <p className="text-xs text-destructive">{formState.errors.contactNumber.message}</p>
          )}
        </div>

        <div className="col-span-2 space-y-1.5">
          <Label htmlFor="address">Address</Label>
          <Input id="address" placeholder="123 Ayala Ave, Makati City" {...register('address')} />
          {formState.errors.address && (
            <p className="text-xs text-destructive">{formState.errors.address.message}</p>
          )}
        </div>
      </div>

      <div className="flex justify-end pt-2">
        <Button type="submit" disabled={formState.isSubmitting}>
          {formState.isSubmitting ? 'Saving…' : submitLabel}
        </Button>
      </div>
    </form>
  )
}
