'use client'

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { toast } from 'sonner'
import { useTranslations } from 'next-intl'
import { Loader2 } from 'lucide-react'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'
import { useInviteFranchisee } from '@/hooks/use-franchise'

const inviteSchema = z.object({
  email: z.string().min(1, 'Email is required').email('Invalid email address'),
  businessName: z.string().min(1, 'Business name is required').max(256),
  ownerName: z.string().optional(),
  franchiseCode: z.string().max(20).optional(),
  territoryName: z.string().optional(),
})

type InviteFormValues = z.infer<typeof inviteSchema>

interface InviteFranchiseeDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function InviteFranchiseeDialog({ open, onOpenChange }: InviteFranchiseeDialogProps) {
  const t = useTranslations('franchise')
  const inviteMutation = useInviteFranchisee()

  const form = useForm<InviteFormValues>({
    resolver: zodResolver(inviteSchema),
    defaultValues: {
      email: '',
      businessName: '',
      ownerName: '',
      franchiseCode: '',
      territoryName: '',
    },
  })

  async function onSubmit(values: InviteFormValues) {
    try {
      await inviteMutation.mutateAsync({
        email: values.email,
        businessName: values.businessName,
        ownerName: values.ownerName || undefined,
        franchiseCode: values.franchiseCode || undefined,
        territoryName: values.territoryName || undefined,
      })
      toast.success('Invitation sent successfully')
      form.reset()
      onOpenChange(false)
    } catch {
      toast.error('Failed to send invitation')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[480px]">
        <DialogHeader>
          <DialogTitle>{t('inviteFranchisee')}</DialogTitle>
          <DialogDescription>
            Send a franchise invitation via email. The recipient will have 7 days to accept.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">{t('email')}</Label>
            <Input
              id="email"
              type="email"
              placeholder="franchisee@example.com"
              {...form.register('email')}
            />
            {form.formState.errors.email && (
              <p className="text-sm text-destructive">{form.formState.errors.email.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="businessName">{t('businessName')}</Label>
            <Input
              id="businessName"
              placeholder="SparkleWash Manila"
              {...form.register('businessName')}
            />
            {form.formState.errors.businessName && (
              <p className="text-sm text-destructive">{form.formState.errors.businessName.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="ownerName">{t('ownerName')}</Label>
            <Input
              id="ownerName"
              placeholder="Juan Dela Cruz"
              {...form.register('ownerName')}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="franchiseCode">Franchise Code</Label>
            <Input
              id="franchiseCode"
              placeholder="MNL-001"
              maxLength={20}
              {...form.register('franchiseCode')}
            />
            {form.formState.errors.franchiseCode && (
              <p className="text-sm text-destructive">{form.formState.errors.franchiseCode.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="territoryName">{t('territory')}</Label>
            <Input
              id="territoryName"
              placeholder="Metro Manila - North"
              {...form.register('territoryName')}
            />
          </div>

          <DialogFooter className="gap-2 pt-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={inviteMutation.isPending}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={inviteMutation.isPending}>
              {inviteMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {t('inviteFranchisee')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
