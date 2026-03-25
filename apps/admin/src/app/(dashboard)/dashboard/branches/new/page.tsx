'use client'

import { useRouter } from 'next/navigation'
import { PageHeader } from '@/components/ui/page-header'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { useCreateBranch } from '@/hooks/use-branches'
import { BranchForm } from '../_components/branch-form'
import type { BranchFormValues } from '@/hooks/use-branches'
import { toast } from 'sonner'

export default function NewBranchPage() {
  const router = useRouter()
  const { mutateAsync: createBranch } = useCreateBranch()

  const handleSubmit = async (values: BranchFormValues) => {
    try {
      const branch = await createBranch(values)
      toast.success('Branch created successfully')
      router.push(`/dashboard/branches/${branch.id}`)
    } catch {
      toast.error('Failed to create branch')
    }
  }

  return (
    <div className="max-w-2xl space-y-6">
      <PageHeader title="New Branch" description="Add a new car wash location" back />

      <Card>
        <CardHeader>
          <CardTitle>Branch details</CardTitle>
          <CardDescription>
            This branch will appear in the POS app and can be assigned employees and services.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <BranchForm
            onSubmit={handleSubmit}
            onCancel={() => router.back()}
            submitLabel="Create Branch"
          />
        </CardContent>
      </Card>
    </div>
  )
}
