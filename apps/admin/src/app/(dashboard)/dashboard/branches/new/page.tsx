'use client'

import { useRouter } from 'next/navigation'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
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
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">New Branch</h1>
          <p className="text-muted-foreground">Add a new car wash location</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Branch details</CardTitle>
          <CardDescription>
            This branch will appear in the POS app and can be assigned employees and services.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <BranchForm onSubmit={handleSubmit} submitLabel="Create Branch" />
        </CardContent>
      </Card>
    </div>
  )
}
