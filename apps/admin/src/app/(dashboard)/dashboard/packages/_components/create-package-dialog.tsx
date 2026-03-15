'use client'

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { useCreatePackage } from '@/hooks/use-packages'
import { PackageForm } from './package-form'
import type { PackageFormValues } from '@/hooks/use-packages'
import { toast } from 'sonner'
import { useRouter } from 'next/navigation'

interface CreatePackageDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function CreatePackageDialog({ open, onOpenChange }: CreatePackageDialogProps) {
  const router = useRouter()
  const { mutateAsync: createPackage } = useCreatePackage()

  const handleSubmit = async (values: PackageFormValues) => {
    try {
      const { id } = await createPackage(values)
      toast.success('Package created')
      onOpenChange(false)
      router.push(`/dashboard/packages/${id}`)
    } catch {
      toast.error('Failed to create package')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>New Package</DialogTitle>
          <DialogDescription>
            Bundle multiple services into a package. Configure the pricing and commission matrices
            after creation.
          </DialogDescription>
        </DialogHeader>
        <div className="pt-2">
          <PackageForm onSubmit={handleSubmit} submitLabel="Create Package" />
        </div>
      </DialogContent>
    </Dialog>
  )
}
