'use client'

import { useAuth } from '@clerk/nextjs'
import { useMutation } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'

// ---------------------------------------------------------------------------
// Types (matching backend ImportTypes.cs, camelCase)
// ---------------------------------------------------------------------------

export enum ImportType {
  Customers = 0,
  Vehicles = 1,
  Employees = 2,
  Services = 3,
}

export interface ImportRowError {
  row: number
  column: string
  message: string
}

export interface ImportRowWarning {
  row: number
  column: string
  message: string
  correctedValue: string | null
}

export interface ImportValidationResult {
  totalRows: number
  validRows: number
  warningRows: number
  errorRows: number
  errors: ImportRowError[]
  warnings: ImportRowWarning[]
  detectedColumns: string[]
  previewRows: Record<string, string>[]
}

export interface ImportResult {
  imported: number
  corrected: number
  skipped: number
  skippedErrors: ImportRowError[]
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function buildFormData(
  file: File,
  type: number,
  mapping?: Record<string, string>
): FormData {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('type', String(type))
  if (mapping) {
    formData.append('mapping', JSON.stringify(mapping))
  }
  return formData
}

// ---------------------------------------------------------------------------
// useDownloadTemplate
// ---------------------------------------------------------------------------

export function useDownloadTemplate() {
  const { getToken } = useAuth()

  const download = async (type: ImportType) => {
    const token = await getToken()
    const fallback = `import-template-${ImportType[type]?.toLowerCase() ?? type}.csv`
    await apiClient.download(`/import/templates/${type}`, fallback, token ?? undefined)
  }

  return { download }
}

// ---------------------------------------------------------------------------
// useDetectColumns
// ---------------------------------------------------------------------------

export function useDetectColumns() {
  const { getToken } = useAuth()
  return useMutation({
    mutationFn: async ({ file, type }: { file: File; type: number }) => {
      const token = await getToken()
      const formData = buildFormData(file, type)
      return apiClient.upload<ImportValidationResult>('/import/detect', formData, token ?? undefined)
    },
  })
}

// ---------------------------------------------------------------------------
// useValidateImport
// ---------------------------------------------------------------------------

export function useValidateImport() {
  const { getToken } = useAuth()
  return useMutation({
    mutationFn: async ({ file, type, mapping }: {
      file: File
      type: number
      mapping: Record<string, string>
    }) => {
      const token = await getToken()
      const formData = buildFormData(file, type, mapping)
      return apiClient.upload<ImportValidationResult>('/import/validate', formData, token ?? undefined)
    },
  })
}

// ---------------------------------------------------------------------------
// useExecuteImport
// ---------------------------------------------------------------------------

export function useExecuteImport() {
  const { getToken } = useAuth()
  return useMutation({
    mutationFn: async ({ file, type, mapping }: {
      file: File
      type: number
      mapping: Record<string, string>
    }) => {
      const token = await getToken()
      const formData = buildFormData(file, type, mapping)
      return apiClient.upload<ImportResult>('/import/execute', formData, token ?? undefined)
    },
  })
}
