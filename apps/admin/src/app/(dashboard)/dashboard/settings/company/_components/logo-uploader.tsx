'use client'

import { useRef, useState } from 'react'
import Image from 'next/image'
import { ImageIcon, Loader2, Trash2, Upload } from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { useUploadLogo, useDeleteLogo } from '@/hooks/use-company-profile'
import type { ApiError } from '@splashsphere/types'

const MAX_BYTES = 2 * 1024 * 1024
const ACCEPTED = 'image/png,image/jpeg,image/webp'

interface Props {
  /** Current logo URL (any size — we display whatever is given). */
  currentUrl: string | null
  /** Optional thumbnail variant for a denser preview if available. */
  thumbnailUrl?: string | null
}

/**
 * Drag-and-drop or click-to-upload logo widget. Validates client-side
 * (size + mime) before sending; the server runs the same checks again
 * via FluentValidation. Shows the existing logo with a Replace / Remove
 * action set when one is present.
 */
export function LogoUploader({ currentUrl, thumbnailUrl }: Props) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [isDragging, setIsDragging] = useState(false)

  const { mutateAsync: upload, isPending: uploading } = useUploadLogo()
  const { mutateAsync: remove, isPending: removing } = useDeleteLogo()
  const busy = uploading || removing

  const previewUrl = thumbnailUrl ?? currentUrl

  const handleFiles = async (files: FileList | null) => {
    const file = files?.[0]
    if (!file) return

    if (file.size > MAX_BYTES) {
      toast.error(`Logo must be ${MAX_BYTES / 1024 / 1024}MB or smaller.`)
      return
    }
    if (!ACCEPTED.split(',').includes(file.type)) {
      toast.error('Logo must be PNG, JPEG, or WebP.')
      return
    }

    try {
      await upload(file)
      toast.success('Logo uploaded.')
    } catch (err) {
      const apiErr = err as ApiError
      toast.error(apiErr?.detail ?? apiErr?.title ?? 'Failed to upload logo.')
    } finally {
      // Reset the input so re-uploading the same file fires onChange again.
      if (inputRef.current) inputRef.current.value = ''
    }
  }

  const handleRemove = async () => {
    try {
      await remove()
      toast.success('Logo removed.')
    } catch (err) {
      const apiErr = err as ApiError
      toast.error(apiErr?.detail ?? apiErr?.title ?? 'Failed to remove logo.')
    }
  }

  const onDrop = (e: React.DragEvent) => {
    e.preventDefault()
    setIsDragging(false)
    if (busy) return
    void handleFiles(e.dataTransfer.files)
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <ImageIcon className="h-4 w-4" /> Logo
        </CardTitle>
        <CardDescription>
          Shown on receipts, reports, and the customer-facing Connect listing. Resized to 500/200/80 px PNG variants on upload.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
          {/* Preview */}
          <div className="flex h-28 w-28 shrink-0 items-center justify-center overflow-hidden rounded-md border bg-muted">
            {previewUrl ? (
              // The URL has a cache-busting `?v=…` suffix from the server;
              // unoptimized={true} skips Next's image optimizer so R2 URLs
              // don't need to be added to next.config remotePatterns.
              <Image
                src={previewUrl}
                alt="Company logo"
                width={112}
                height={112}
                unoptimized
                className="h-28 w-28 object-contain"
              />
            ) : (
              <ImageIcon className="h-8 w-8 text-muted-foreground/40" aria-hidden />
            )}
          </div>

          {/* Drop zone + actions */}
          <div className="flex-1 space-y-3">
            <button
              type="button"
              onClick={() => inputRef.current?.click()}
              onDragOver={(e) => { e.preventDefault(); setIsDragging(true) }}
              onDragLeave={() => setIsDragging(false)}
              onDrop={onDrop}
              disabled={busy}
              className={`flex w-full items-center justify-center gap-2 rounded-md border border-dashed px-4 py-6 text-sm transition-colors ${
                isDragging ? 'border-primary bg-primary/5' : 'border-muted-foreground/25 hover:bg-muted/50'
              } ${busy ? 'cursor-wait opacity-60' : 'cursor-pointer'}`}
            >
              {uploading ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Uploading…
                </>
              ) : (
                <>
                  <Upload className="h-4 w-4" />
                  {currentUrl ? 'Replace logo' : 'Drop a file or click to upload'}
                </>
              )}
            </button>

            <input
              ref={inputRef}
              type="file"
              accept={ACCEPTED}
              className="hidden"
              onChange={(e) => void handleFiles(e.target.files)}
            />

            <p className="text-xs text-muted-foreground">
              PNG, JPEG, or WebP. Max 2 MB. Recommended: 500×500 px square.
            </p>

            {currentUrl && (
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={handleRemove}
                disabled={busy}
              >
                <Trash2 className="mr-1.5 h-3.5 w-3.5" />
                {removing ? 'Removing…' : 'Remove logo'}
              </Button>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
