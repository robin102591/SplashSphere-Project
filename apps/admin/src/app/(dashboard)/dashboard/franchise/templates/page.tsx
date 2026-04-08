'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { Plus, Pencil, FileText, Upload } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { StatusBadge } from '@/components/ui/status-badge'
import { EmptyState } from '@/components/ui/empty-state'
import { Switch } from '@/components/ui/switch'
import { PageHeader } from '@/components/ui/page-header'
import {
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter,
} from '@/components/ui/dialog'
import {
  useServiceTemplates, useUpsertServiceTemplate, usePushServiceTemplates,
} from '@/hooks/use-franchise'
import { formatPeso } from '@/lib/format'
import { toast } from 'sonner'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'

// ── Schema ────────────────────────────────────────────────────────────────────

const templateSchema = z.object({
  serviceName: z.string().min(1, 'Service name is required'),
  description: z.string().optional(),
  categoryName: z.string().optional(),
  basePrice: z.coerce.number().min(0, 'Must be 0 or more'),
  durationMinutes: z.coerce.number().min(1, 'Must be at least 1 minute'),
  isRequired: z.boolean(),
  isActive: z.boolean(),
})

type TemplateForm = z.infer<typeof templateSchema>

// ── Types ─────────────────────────────────────────────────────────────────────

interface TemplateDto {
  id: string
  serviceName: string
  description: string | null
  categoryName: string | null
  basePrice: number
  durationMinutes: number
  isRequired: boolean
  isActive: boolean
}

// ── Template Dialog ───────────────────────────────────────────────────────────

function TemplateDialog({
  open,
  onOpenChange,
  template,
}: {
  open: boolean
  onOpenChange: (v: boolean) => void
  template?: TemplateDto | null
}) {
  const t = useTranslations('franchise')
  const { mutateAsync: upsert, isPending } = useUpsertServiceTemplate()
  const isEdit = !!template

  const { register, handleSubmit, formState, control, reset } = useForm<TemplateForm>({
    resolver: zodResolver(templateSchema),
    defaultValues: template
      ? {
          serviceName: template.serviceName,
          description: template.description ?? '',
          categoryName: template.categoryName ?? '',
          basePrice: template.basePrice,
          durationMinutes: template.durationMinutes,
          isRequired: template.isRequired,
          isActive: template.isActive,
        }
      : {
          serviceName: '',
          description: '',
          categoryName: '',
          basePrice: 0,
          durationMinutes: 30,
          isRequired: false,
          isActive: true,
        },
  })

  const onSubmit = async (values: TemplateForm) => {
    try {
      await upsert({
        ...(isEdit ? { id: template.id } : {}),
        name: values.serviceName,
        description: values.description ?? '',
        categoryId: values.categoryName ?? '',
        basePrice: values.basePrice,
        estimatedDuration: values.durationMinutes,
      })
      toast.success(isEdit ? 'Template updated' : 'Template created')
      reset()
      onOpenChange(false)
    } catch {
      toast.error(isEdit ? 'Failed to update template' : 'Failed to create template')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Template' : 'New Template'}</DialogTitle>
          <DialogDescription>
            {isEdit ? 'Update the service template details.' : 'Create a new service template for your franchise network.'}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 pt-2">
          <div className="space-y-1.5">
            <Label>{t('serviceName')} <span className="text-destructive">*</span></Label>
            <Input placeholder="e.g. Premium Exterior Wash" {...register('serviceName')} />
            {formState.errors.serviceName && (
              <p className="text-xs text-destructive">{formState.errors.serviceName.message}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label>Description</Label>
            <Input placeholder="Optional description" {...register('description')} />
          </div>

          <div className="space-y-1.5">
            <Label>Category</Label>
            <Input placeholder="e.g. Exterior, Interior, Detailing" {...register('categoryName')} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>{t('basePrice')} <span className="text-destructive">*</span></Label>
              <Input type="number" step="0.01" min="0" {...register('basePrice')} />
              {formState.errors.basePrice && (
                <p className="text-xs text-destructive">{formState.errors.basePrice.message}</p>
              )}
            </div>
            <div className="space-y-1.5">
              <Label>{t('duration')} <span className="text-destructive">*</span></Label>
              <Input type="number" min="1" {...register('durationMinutes')} />
              {formState.errors.durationMinutes && (
                <p className="text-xs text-destructive">{formState.errors.durationMinutes.message}</p>
              )}
            </div>
          </div>

          <div className="flex items-center justify-between">
            <Label>{t('isRequired')}</Label>
            <Controller
              control={control}
              name="isRequired"
              render={({ field }) => (
                <Switch checked={field.value} onCheckedChange={field.onChange} />
              )}
            />
          </div>

          <div className="flex items-center justify-between">
            <Label>Active</Label>
            <Controller
              control={control}
              name="isActive"
              render={({ field }) => (
                <Switch checked={field.value} onCheckedChange={field.onChange} />
              )}
            />
          </div>

          <DialogFooter>
            <Button variant="outline" type="button" onClick={() => onOpenChange(false)}>Cancel</Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Saving...' : isEdit ? 'Update' : 'Create'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

// ── Push Confirm Dialog ───────────────────────────────────────────────────────

function PushDialog({
  open,
  onOpenChange,
}: {
  open: boolean
  onOpenChange: (v: boolean) => void
}) {
  const t = useTranslations('franchise')
  const { mutateAsync: push, isPending } = usePushServiceTemplates()

  const onConfirm = async () => {
    try {
      await push({ franchiseeId: '', templateIds: [] })
      toast.success('Templates pushed to all franchisees')
      onOpenChange(false)
    } catch {
      toast.error('Failed to push templates')
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{t('pushTemplates')}</DialogTitle>
          <DialogDescription>
            This will sync all templates to franchisee service catalogs. Continue?
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={onConfirm} disabled={isPending}>
            {isPending ? 'Pushing...' : 'Confirm'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function ServiceTemplatesPage() {
  const t = useTranslations('franchise')
  const { data: templates, isLoading } = useServiceTemplates()
  const [createOpen, setCreateOpen] = useState(false)
  const [editing, setEditing] = useState<TemplateDto | null>(null)
  const [pushOpen, setPushOpen] = useState(false)

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('templates')}
        back="/dashboard/franchise"
        actions={
          <div className="flex items-center gap-2">
            <Button variant="outline" onClick={() => setPushOpen(true)}>
              <Upload className="mr-2 h-4 w-4" />
              {t('pushTemplates')}
            </Button>
            <Button onClick={() => setCreateOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Add Template
            </Button>
          </div>
        }
      />

      {isLoading ? (
        <Card>
          <CardContent className="p-0">
            <div className="space-y-0 divide-y">
              {Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className="px-4 py-3">
                  <Skeleton className="h-5 w-full" />
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      ) : !templates || templates.length === 0 ? (
        <EmptyState
          icon={FileText}
          title={t('noTemplates')}
          description="Create service templates that franchisees must follow."
          action={{ label: 'Add Template', onClick: () => setCreateOpen(true), icon: Plus }}
        />
      ) : (
        <Card>
          <CardContent className="p-0">
            <div className="rounded-lg overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('serviceName')}</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">Category</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('basePrice')}</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('duration')}</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('isRequired')}</th>
                    <th className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-muted-foreground">{t('status')}</th>
                    <th className="px-4 py-3 text-right text-xs font-medium uppercase tracking-wider text-muted-foreground">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {templates.map((tmpl) => (
                    <tr key={tmpl.id} className="hover:bg-muted/40 transition-colors">
                      <td className="px-4 py-3">
                        <div>
                          <p className="font-medium">{tmpl.serviceName}</p>
                          {tmpl.description && (
                            <p className="text-xs text-muted-foreground mt-0.5">{tmpl.description}</p>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">
                        {tmpl.categoryName ?? '-'}
                      </td>
                      <td className="px-4 py-3 text-right font-mono tabular-nums">
                        {formatPeso(tmpl.basePrice)}
                      </td>
                      <td className="px-4 py-3 text-right font-mono tabular-nums">
                        {tmpl.durationMinutes}
                      </td>
                      <td className="px-4 py-3">
                        <StatusBadge status={tmpl.isRequired ? 'Active' : 'Inactive'} label={tmpl.isRequired ? 'Yes' : 'No'} />
                      </td>
                      <td className="px-4 py-3">
                        <StatusBadge status={tmpl.isActive ? 'Active' : 'Inactive'} />
                      </td>
                      <td className="px-4 py-3 text-right">
                        <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => setEditing(tmpl)}>
                          <Pencil className="h-3.5 w-3.5" />
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}

      <TemplateDialog open={createOpen} onOpenChange={setCreateOpen} />
      <TemplateDialog
        open={!!editing}
        onOpenChange={(v) => !v && setEditing(null)}
        template={editing}
      />
      <PushDialog open={pushOpen} onOpenChange={setPushOpen} />
    </div>
  )
}
