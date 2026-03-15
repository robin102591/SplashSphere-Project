'use client'

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { useCreateEmployee } from '@/hooks/use-employees'
import { CreateEmployeeForm } from './employee-form'
import type { CreateEmployeeValues } from '@/hooks/use-employees'
import { toast } from 'sonner'
import { useRouter } from 'next/navigation'

interface CreateEmployeeDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function CreateEmployeeDialog({ open, onOpenChange }: CreateEmployeeDialogProps) {
  const router = useRouter()
  const { mutateAsync: createEmployee } = useCreateEmployee()

  const handleSubmit = async (values: CreateEmployeeValues) => {
    try {
      const { id } = await createEmployee(values)
      toast.success('Employee created')
      onOpenChange(false)
      router.push(`/dashboard/employees/${id}`)
    } catch {
      toast.error('Failed to create employee')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg overflow-y-auto max-h-[90vh]">
        <DialogHeader>
          <DialogTitle>New Employee</DialogTitle>
          <DialogDescription>
            Add an employee to a branch. Employee type cannot be changed after creation.
          </DialogDescription>
        </DialogHeader>
        <div className="pt-2">
          <CreateEmployeeForm onSubmit={handleSubmit} />
        </div>
      </DialogContent>
    </Dialog>
  )
}
