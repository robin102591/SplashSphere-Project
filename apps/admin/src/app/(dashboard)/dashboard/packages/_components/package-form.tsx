'use client'

import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useServices } from '@/hooks/use-services'
import type { PackageFormValues } from '@/hooks/use-packages'
import { cn } from '@/lib/utils'

const packageSchema = z.object({
  name: z.string().min(2, 'Name is required'),
  description: z.string().optional(),
  serviceIds: z.array(z.string()).min(1, 'Select at least one service'),
})
type FormValues = z.infer<typeof packageSchema>

interface PackageFormProps {
  defaultValues?: Partial<PackageFormValues>
  onSubmit: (values: PackageFormValues) => Promise<void>
  submitLabel?: string
}

export function PackageForm({
  defaultValues,
  onSubmit,
  submitLabel = 'Save',
}: PackageFormProps) {
  const { data: servicesData, isLoading: servicesLoading } = useServices({ pageSize: 100 })
  const services = servicesData ? [...servicesData.items].filter((s) => s.isActive) : []

  const { register, control, handleSubmit, formState } = useForm<FormValues>({
    resolver: zodResolver(packageSchema),
    defaultValues: {
      name: '',
      description: '',
      serviceIds: [],
      ...defaultValues,
    },
  })

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <div className="space-y-1.5">
        <Label htmlFor="pkg-name">Package name <span className="text-destructive">*</span></Label>
        <Input id="pkg-name" placeholder="Full Detail Package" {...register('name')} />
        {formState.errors.name && (
          <p className="text-xs text-destructive">{formState.errors.name.message}</p>
        )}
      </div>

      <div className="space-y-1.5">
        <Label htmlFor="pkg-description">Description (optional)</Label>
        <Input
          id="pkg-description"
          placeholder="Brief description of what's included…"
          {...register('description')}
        />
      </div>

      <div className="space-y-1.5">
        <Label>Included services <span className="text-destructive">*</span></Label>
        {servicesLoading ? (
          <p className="text-sm text-muted-foreground">Loading services…</p>
        ) : (
          <Controller
            control={control}
            name="serviceIds"
            render={({ field }) => (
              <div className="rounded-md border divide-y max-h-56 overflow-y-auto">
                {services.length === 0 && (
                  <p className="px-3 py-2 text-sm text-muted-foreground">No active services</p>
                )}
                {services.map((svc) => {
                  const checked = field.value.includes(svc.id)
                  return (
                    <label
                      key={svc.id}
                      className={cn(
                        'flex items-center gap-3 px-3 py-2.5 cursor-pointer hover:bg-muted/50 transition-colors',
                        checked && 'bg-primary/5'
                      )}
                    >
                      <input
                        type="checkbox"
                        checked={checked}
                        onChange={(e) => {
                          if (e.target.checked) {
                            field.onChange([...field.value, svc.id])
                          } else {
                            field.onChange(field.value.filter((id) => id !== svc.id))
                          }
                        }}
                        className="h-4 w-4 rounded border-input accent-primary"
                      />
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium">{svc.name}</p>
                        <p className="text-xs text-muted-foreground">{svc.categoryName}</p>
                      </div>
                    </label>
                  )
                })}
              </div>
            )}
          />
        )}
        {formState.errors.serviceIds && (
          <p className="text-xs text-destructive">{formState.errors.serviceIds.message}</p>
        )}
      </div>

      <div className="flex justify-end pt-1">
        <Button type="submit" disabled={formState.isSubmitting}>
          {formState.isSubmitting ? 'Saving…' : submitLabel}
        </Button>
      </div>
    </form>
  )
}
