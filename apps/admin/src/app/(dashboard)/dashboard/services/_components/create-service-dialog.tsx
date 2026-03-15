'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { useCreateService } from '@/hooks/use-services'
import { useServiceCategories } from '@/hooks/use-service-categories'
import type { ServiceFormValues } from '@/hooks/use-services'
import { toast } from 'sonner'
import { useRouter } from 'next/navigation'

const schema = z.object({
  name: z.string().min(2, 'Name is required'),
  categoryId: z.string().min(1, 'Category is required'),
  basePrice: z.coerce.number().min(0, 'Price must be 0 or more'),
  description: z.string().optional(),
})
type FormValues = z.infer<typeof schema>

interface CreateServiceDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function CreateServiceDialog({ open, onOpenChange }: CreateServiceDialogProps) {
  const router = useRouter()
  const { mutateAsync: createService } = useCreateService()
  const { data: categories, isLoading: catLoading } = useServiceCategories()

  const { register, handleSubmit, reset, formState } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: '', categoryId: '', basePrice: 0, description: '' },
  })

  const onSubmit = async (values: FormValues) => {
    const payload: ServiceFormValues = {
      name: values.name,
      categoryId: values.categoryId,
      basePrice: values.basePrice,
      description: values.description || undefined,
    }
    try {
      const { id } = await createService(payload)
      toast.success('Service created')
      reset()
      onOpenChange(false)
      router.push(`/dashboard/services/${id}`)
    } catch {
      toast.error('Failed to create service')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>New Service</DialogTitle>
          <DialogDescription>
            Add a car wash service. You can configure the pricing and commission matrices after
            creation.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label htmlFor="name">Service name</Label>
            <Input id="name" placeholder="Basic Wash" {...register('name')} />
            {formState.errors.name && (
              <p className="text-xs text-destructive">{formState.errors.name.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="categoryId">Category</Label>
            <select
              id="categoryId"
              disabled={catLoading}
              className="w-full h-10 rounded-md border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              {...register('categoryId')}
            >
              <option value="">Select category…</option>
              {categories?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
            {formState.errors.categoryId && (
              <p className="text-xs text-destructive">{formState.errors.categoryId.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="basePrice">Base price (₱)</Label>
            <div className="relative">
              <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm select-none">
                ₱
              </span>
              <Input
                id="basePrice"
                type="number"
                min="0"
                step="0.01"
                className="pl-7"
                placeholder="0.00"
                {...register('basePrice')}
              />
            </div>
            <p className="text-xs text-muted-foreground">
              Fallback when no pricing matrix entry exists for a vehicle/size combination.
            </p>
            {formState.errors.basePrice && (
              <p className="text-xs text-destructive">{formState.errors.basePrice.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label htmlFor="description">Description (optional)</Label>
            <Input id="description" placeholder="Brief description…" {...register('description')} />
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button type="submit" disabled={formState.isSubmitting}>
              {formState.isSubmitting ? 'Creating…' : 'Create Service'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}
