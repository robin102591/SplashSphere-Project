'use client'

import { useState, useCallback, useRef } from 'react'
import { ArrowLeft, ArrowRight, Upload, Download, FileSpreadsheet, Users, Car, Wrench, UserCog, Check, AlertTriangle, XCircle, Loader2, RotateCcw } from 'lucide-react'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
} from '@/components/ui/select'
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from '@/components/ui/table'
import { toast } from 'sonner'
import {
  useDownloadTemplate,
  useDetectColumns,
  useValidateImport,
  useExecuteImport,
  ImportType,
} from '@/hooks/use-import'
import type { ImportValidationResult, ImportResult } from '@/hooks/use-import'
import { cn } from '@/lib/utils'

// ── Constants ─────────────────────────────────────────────────────────────────

const IMPORT_TYPES = [
  { value: ImportType.Customers, label: 'Customers', icon: Users, description: 'Names, contacts, emails, addresses' },
  { value: ImportType.Vehicles, label: 'Vehicles', icon: Car, description: 'Plates, types, makes, models, linked to customers' },
  { value: ImportType.Employees, label: 'Employees', icon: UserCog, description: 'Names, types, rates, contacts' },
  { value: ImportType.Services, label: 'Services', icon: Wrench, description: 'Service names, categories, prices, durations' },
] as const

const TARGET_FIELDS: Record<number, { value: string; label: string }[]> = {
  [ImportType.Customers]: [
    { value: 'Name', label: 'Name' },
    { value: 'Contact', label: 'Contact Number' },
    { value: 'Email', label: 'Email' },
    { value: 'Address', label: 'Address' },
  ],
  [ImportType.Vehicles]: [
    { value: 'PlateNumber', label: 'Plate Number' },
    { value: 'VehicleType', label: 'Vehicle Type' },
    { value: 'Size', label: 'Size' },
    { value: 'Make', label: 'Make' },
    { value: 'Model', label: 'Model' },
    { value: 'Color', label: 'Color' },
    { value: 'Year', label: 'Year' },
    { value: 'CustomerName', label: 'Customer Name' },
    { value: 'CustomerContact', label: 'Customer Contact' },
  ],
  [ImportType.Employees]: [
    { value: 'Name', label: 'Name' },
    { value: 'Type', label: 'Employee Type' },
    { value: 'DailyRate', label: 'Daily Rate' },
    { value: 'DateHired', label: 'Date Hired' },
    { value: 'Contact', label: 'Contact Number' },
    { value: 'Email', label: 'Email' },
  ],
  [ImportType.Services]: [
    { value: 'Name', label: 'Service Name' },
    { value: 'Category', label: 'Category' },
    { value: 'BasePrice', label: 'Base Price' },
    { value: 'DurationMinutes', label: 'Duration (mins)' },
    { value: 'Description', label: 'Description' },
  ],
}

const STEPS = ['Upload', 'Map Columns', 'Validate', 'Import'] as const

// ── Main Page ─────────────────────────────────────────────────────────────────

export default function ImportPage() {
  const [step, setStep] = useState(0)
  const [importType, setImportType] = useState<ImportType>(ImportType.Customers)
  const [file, setFile] = useState<File | null>(null)
  const [mapping, setMapping] = useState<Record<string, string>>({})
  const [detectResult, setDetectResult] = useState<ImportValidationResult | null>(null)
  const [validateResult, setValidateResult] = useState<ImportValidationResult | null>(null)
  const [importResult, setImportResult] = useState<ImportResult | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const { download: downloadTemplate } = useDownloadTemplate()
  const detectMutation = useDetectColumns()
  const validateMutation = useValidateImport()
  const executeMutation = useExecuteImport()

  const reset = useCallback(() => {
    setStep(0)
    setFile(null)
    setMapping({})
    setDetectResult(null)
    setValidateResult(null)
    setImportResult(null)
    detectMutation.reset()
    validateMutation.reset()
    executeMutation.reset()
  }, [detectMutation, validateMutation, executeMutation])

  // ── Step 1: Upload + Detect ──────────────────────────────────────────────

  const handleFileSelect = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const f = e.target.files?.[0]
    if (!f) return
    const ext = f.name.split('.').pop()?.toLowerCase()
    if (!['csv', 'xlsx', 'xls'].includes(ext ?? '')) {
      toast.error('Please upload a CSV or Excel file')
      return
    }
    setFile(f)
  }, [])

  const handleDetect = useCallback(async () => {
    if (!file) return
    try {
      const result = await detectMutation.mutateAsync({ file, type: importType })
      setDetectResult(result)
      // Build initial mapping from detected columns (auto-match by name)
      const initialMapping: Record<string, string> = {}
      const targets = TARGET_FIELDS[importType] ?? []
      for (const col of result.detectedColumns) {
        const match = targets.find(
          t => t.value.toLowerCase() === col.toLowerCase() ||
               t.label.toLowerCase() === col.toLowerCase()
        )
        if (match) initialMapping[col] = match.value
      }
      setMapping(initialMapping)
      setStep(1)
    } catch {
      toast.error('Failed to detect columns. Check your file format.')
    }
  }, [file, importType, detectMutation])

  // ── Step 2: Validate ─────────────────────────────────────────────────────

  const handleValidate = useCallback(async () => {
    if (!file) return
    // Build the mapping in target→source format expected by backend
    const backendMapping: Record<string, string> = {}
    for (const [source, target] of Object.entries(mapping)) {
      if (target) backendMapping[target] = source
    }
    try {
      const result = await validateMutation.mutateAsync({
        file,
        type: importType,
        mapping: backendMapping,
      })
      setValidateResult(result)
      setStep(2)
    } catch {
      toast.error('Validation failed. Please check your file and mappings.')
    }
  }, [file, importType, mapping, validateMutation])

  // ── Step 3: Execute ──────────────────────────────────────────────────────

  const handleExecute = useCallback(async () => {
    if (!file) return
    const backendMapping: Record<string, string> = {}
    for (const [source, target] of Object.entries(mapping)) {
      if (target) backendMapping[target] = source
    }
    try {
      const result = await executeMutation.mutateAsync({
        file,
        type: importType,
        mapping: backendMapping,
      })
      setImportResult(result)
      setStep(3)
      toast.success(`Successfully imported ${result.imported} records!`)
    } catch {
      toast.error('Import failed. No records were imported.')
    }
  }, [file, importType, mapping, executeMutation])

  const handleDownloadTemplate = useCallback(async () => {
    try {
      await downloadTemplate(importType)
    } catch {
      toast.error('Failed to download template')
    }
  }, [importType, downloadTemplate])

  // ── Render ───────────────────────────────────────────────────────────────

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/dashboard/settings">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-2xl font-bold">Import Data</h1>
          <p className="text-sm text-muted-foreground">
            Import customers, vehicles, employees, or services from CSV or Excel files
          </p>
        </div>
      </div>

      {/* Progress Steps */}
      <div className="flex items-center gap-2">
        {STEPS.map((label, i) => (
          <div key={label} className="flex items-center gap-2">
            <div
              className={cn(
                'flex h-8 w-8 items-center justify-center rounded-full text-sm font-medium',
                i < step ? 'bg-primary text-primary-foreground' :
                i === step ? 'bg-primary text-primary-foreground' :
                'bg-muted text-muted-foreground'
              )}
            >
              {i < step ? <Check className="h-4 w-4" /> : i + 1}
            </div>
            <span className={cn(
              'text-sm font-medium',
              i <= step ? 'text-foreground' : 'text-muted-foreground'
            )}>
              {label}
            </span>
            {i < STEPS.length - 1 && (
              <div className={cn(
                'h-px w-8',
                i < step ? 'bg-primary' : 'bg-border'
              )} />
            )}
          </div>
        ))}
      </div>

      {/* Step Content */}
      {step === 0 && (
        <UploadStep
          importType={importType}
          onTypeChange={setImportType}
          file={file}
          onFileSelect={handleFileSelect}
          fileInputRef={fileInputRef}
          onDetect={handleDetect}
          onDownloadTemplate={handleDownloadTemplate}
          isPending={detectMutation.isPending}
        />
      )}

      {step === 1 && detectResult && (
        <MappingStep
          importType={importType}
          detectedColumns={detectResult.detectedColumns}
          previewRows={detectResult.previewRows}
          mapping={mapping}
          onMappingChange={setMapping}
          onBack={() => setStep(0)}
          onValidate={handleValidate}
          isPending={validateMutation.isPending}
        />
      )}

      {step === 2 && validateResult && (
        <ValidationStep
          result={validateResult}
          onBack={() => setStep(1)}
          onExecute={handleExecute}
          isPending={executeMutation.isPending}
        />
      )}

      {step === 3 && importResult && (
        <ResultStep
          result={importResult}
          importType={importType}
          onReset={reset}
        />
      )}
    </div>
  )
}

// ── Step 1: Upload ────────────────────────────────────────────────────────────

function UploadStep({
  importType, onTypeChange, file, onFileSelect, fileInputRef,
  onDetect, onDownloadTemplate, isPending,
}: {
  importType: ImportType
  onTypeChange: (t: ImportType) => void
  file: File | null
  onFileSelect: (e: React.ChangeEvent<HTMLInputElement>) => void
  fileInputRef: React.RefObject<HTMLInputElement | null>
  onDetect: () => void
  onDownloadTemplate: () => void
  isPending: boolean
}) {
  return (
    <div className="grid gap-6 md:grid-cols-2">
      {/* Import Type Selection */}
      <Card>
        <CardHeader>
          <CardTitle>Import Type</CardTitle>
          <CardDescription>Select the type of data you want to import</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-3">
          {IMPORT_TYPES.map(t => {
            const Icon = t.icon
            const selected = importType === t.value
            return (
              <button
                key={t.value}
                onClick={() => onTypeChange(t.value)}
                className={cn(
                  'flex items-center gap-3 rounded-lg border p-3 text-left transition-colors',
                  selected ? 'border-primary bg-primary/5' : 'hover:bg-muted/50'
                )}
              >
                <div className={cn(
                  'flex h-10 w-10 items-center justify-center rounded-lg',
                  selected ? 'bg-primary text-primary-foreground' : 'bg-muted'
                )}>
                  <Icon className="h-5 w-5" />
                </div>
                <div>
                  <p className="font-medium">{t.label}</p>
                  <p className="text-xs text-muted-foreground">{t.description}</p>
                </div>
              </button>
            )
          })}
        </CardContent>
      </Card>

      {/* File Upload */}
      <Card>
        <CardHeader>
          <CardTitle>Upload File</CardTitle>
          <CardDescription>Upload a CSV or Excel (.xlsx) file</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div
            onClick={() => fileInputRef.current?.click()}
            className={cn(
              'flex cursor-pointer flex-col items-center justify-center gap-3 rounded-lg border-2 border-dashed p-8 transition-colors hover:bg-muted/50',
              file ? 'border-primary bg-primary/5' : 'border-muted-foreground/25'
            )}
          >
            {file ? (
              <>
                <FileSpreadsheet className="h-10 w-10 text-primary" />
                <div className="text-center">
                  <p className="font-medium">{file.name}</p>
                  <p className="text-sm text-muted-foreground">
                    {(file.size / 1024).toFixed(1)} KB
                  </p>
                </div>
              </>
            ) : (
              <>
                <Upload className="h-10 w-10 text-muted-foreground" />
                <div className="text-center">
                  <p className="font-medium">Click to upload</p>
                  <p className="text-sm text-muted-foreground">CSV or Excel files</p>
                </div>
              </>
            )}
            <input
              ref={fileInputRef}
              type="file"
              accept=".csv,.xlsx,.xls"
              onChange={onFileSelect}
              className="hidden"
            />
          </div>

          <Button variant="outline" className="w-full" onClick={onDownloadTemplate}>
            <Download className="mr-2 h-4 w-4" />
            Download Template
          </Button>

          <Button
            className="w-full"
            disabled={!file || isPending}
            onClick={onDetect}
          >
            {isPending ? (
              <><Loader2 className="mr-2 h-4 w-4 animate-spin" /> Detecting columns...</>
            ) : (
              <><ArrowRight className="mr-2 h-4 w-4" /> Detect Columns &amp; Continue</>
            )}
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}

// ── Step 2: Column Mapping ────────────────────────────────────────────────────

function MappingStep({
  importType, detectedColumns, previewRows, mapping, onMappingChange,
  onBack, onValidate, isPending,
}: {
  importType: ImportType
  detectedColumns: string[]
  previewRows: Record<string, string>[]
  mapping: Record<string, string>
  onMappingChange: (m: Record<string, string>) => void
  onBack: () => void
  onValidate: () => void
  isPending: boolean
}) {
  const targets = TARGET_FIELDS[importType] ?? []
  const mappedTargets = new Set(Object.values(mapping).filter(Boolean))
  const hasMappings = mappedTargets.size > 0

  return (
    <div className="space-y-6">
      {/* Mapping Table */}
      <Card>
        <CardHeader>
          <CardTitle>Column Mapping</CardTitle>
          <CardDescription>
            Map your file columns to SplashSphere fields. Auto-detected mappings are pre-filled.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Your Column</TableHead>
                <TableHead>Sample Data</TableHead>
                <TableHead>Maps To</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {detectedColumns.map(col => {
                const sample = previewRows[0]?.[col] ?? ''
                return (
                  <TableRow key={col}>
                    <TableCell className="font-medium">{col}</TableCell>
                    <TableCell className="text-muted-foreground max-w-[200px] truncate">
                      {sample || <span className="italic">empty</span>}
                    </TableCell>
                    <TableCell>
                      <Select
                        value={mapping[col] || '_skip'}
                        onValueChange={v => {
                          onMappingChange({
                            ...mapping,
                            [col]: v === '_skip' ? '' : v,
                          })
                        }}
                      >
                        <SelectTrigger className="w-[200px]">
                          <SelectValue placeholder="Skip this column" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="_skip">
                            <span className="text-muted-foreground">Skip this column</span>
                          </SelectItem>
                          {targets.map(t => (
                            <SelectItem
                              key={t.value}
                              value={t.value}
                              disabled={mappedTargets.has(t.value) && mapping[col] !== t.value}
                            >
                              {t.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Preview */}
      {previewRows.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Data Preview</CardTitle>
            <CardDescription>First {previewRows.length} rows from your file</CardDescription>
          </CardHeader>
          <CardContent className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-12">#</TableHead>
                  {detectedColumns.map(col => (
                    <TableHead key={col}>{col}</TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {previewRows.slice(0, 5).map((row, i) => (
                  <TableRow key={i}>
                    <TableCell className="text-muted-foreground">{i + 1}</TableCell>
                    {detectedColumns.map(col => (
                      <TableCell key={col} className="max-w-[150px] truncate">
                        {row[col] || ''}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Actions */}
      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          <ArrowLeft className="mr-2 h-4 w-4" /> Back
        </Button>
        <Button disabled={!hasMappings || isPending} onClick={onValidate}>
          {isPending ? (
            <><Loader2 className="mr-2 h-4 w-4 animate-spin" /> Validating...</>
          ) : (
            <><ArrowRight className="mr-2 h-4 w-4" /> Validate</>
          )}
        </Button>
      </div>
    </div>
  )
}

// ── Step 3: Validation Results ────────────────────────────────────────────────

function ValidationStep({
  result, onBack, onExecute, isPending,
}: {
  result: ImportValidationResult
  onBack: () => void
  onExecute: () => void
  isPending: boolean
}) {
  const total = result.totalRows
  const validPct = total > 0 ? Math.round((result.validRows / total) * 100) : 0
  const canImport = result.validRows > 0

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid gap-4 sm:grid-cols-4">
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-2xl font-bold">{result.totalRows}</p>
              <p className="text-sm text-muted-foreground">Total Rows</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-2xl font-bold text-green-600">{result.validRows}</p>
              <p className="text-sm text-muted-foreground">Valid</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-2xl font-bold text-yellow-600">{result.warningRows}</p>
              <p className="text-sm text-muted-foreground">Warnings</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-2xl font-bold text-red-600">{result.errorRows}</p>
              <p className="text-sm text-muted-foreground">Errors</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Progress Bar */}
      <Card>
        <CardContent className="pt-6">
          <div className="space-y-2">
            <div className="flex justify-between text-sm">
              <span>Import readiness</span>
              <span className="font-medium">{validPct}% valid</span>
            </div>
            <Progress value={validPct} className="h-3" />
          </div>
        </CardContent>
      </Card>

      {/* Errors */}
      {result.errors.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-red-600">
              <XCircle className="h-5 w-5" /> Errors ({result.errors.length})
            </CardTitle>
            <CardDescription>These rows will be skipped during import</CardDescription>
          </CardHeader>
          <CardContent className="max-h-64 overflow-y-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-16">Row</TableHead>
                  <TableHead>Column</TableHead>
                  <TableHead>Issue</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {result.errors.slice(0, 50).map((err, i) => (
                  <TableRow key={i}>
                    <TableCell>
                      <Badge variant="destructive">{err.row}</Badge>
                    </TableCell>
                    <TableCell className="font-medium">{err.column}</TableCell>
                    <TableCell>{err.message}</TableCell>
                  </TableRow>
                ))}
                {result.errors.length > 50 && (
                  <TableRow>
                    <TableCell colSpan={3} className="text-center text-muted-foreground">
                      ...and {result.errors.length - 50} more errors
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Warnings */}
      {result.warnings.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-yellow-600">
              <AlertTriangle className="h-5 w-5" /> Warnings ({result.warnings.length})
            </CardTitle>
            <CardDescription>These values will be auto-corrected during import</CardDescription>
          </CardHeader>
          <CardContent className="max-h-64 overflow-y-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-16">Row</TableHead>
                  <TableHead>Column</TableHead>
                  <TableHead>Issue</TableHead>
                  <TableHead>Corrected To</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {result.warnings.slice(0, 50).map((warn, i) => (
                  <TableRow key={i}>
                    <TableCell>
                      <Badge variant="secondary">{warn.row}</Badge>
                    </TableCell>
                    <TableCell className="font-medium">{warn.column}</TableCell>
                    <TableCell>{warn.message}</TableCell>
                    <TableCell className="text-green-600">{warn.correctedValue ?? '-'}</TableCell>
                  </TableRow>
                ))}
                {result.warnings.length > 50 && (
                  <TableRow>
                    <TableCell colSpan={4} className="text-center text-muted-foreground">
                      ...and {result.warnings.length - 50} more warnings
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Actions */}
      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          <ArrowLeft className="mr-2 h-4 w-4" /> Fix Mappings
        </Button>
        <Button disabled={!canImport || isPending} onClick={onExecute}>
          {isPending ? (
            <><Loader2 className="mr-2 h-4 w-4 animate-spin" /> Importing...</>
          ) : (
            <><Check className="mr-2 h-4 w-4" /> Import {result.validRows} Records</>
          )}
        </Button>
      </div>
    </div>
  )
}

// ── Step 4: Import Results ────────────────────────────────────────────────────

function ResultStep({
  result, importType, onReset,
}: {
  result: ImportResult
  importType: ImportType
  onReset: () => void
}) {
  const typeLabel = IMPORT_TYPES.find(t => t.value === importType)?.label ?? 'Records'

  return (
    <div className="space-y-6">
      {/* Success Summary */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col items-center gap-4 py-8">
            <div className="flex h-16 w-16 items-center justify-center rounded-full bg-green-100 dark:bg-green-900/30">
              <Check className="h-8 w-8 text-green-600" />
            </div>
            <div className="text-center">
              <h2 className="text-2xl font-bold">Import Complete!</h2>
              <p className="text-muted-foreground">
                Successfully imported {result.imported} {typeLabel.toLowerCase()}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Stats */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-2xl font-bold text-green-600">{result.imported}</p>
              <p className="text-sm text-muted-foreground">Imported</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-2xl font-bold text-yellow-600">{result.corrected}</p>
              <p className="text-sm text-muted-foreground">Auto-Corrected</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <p className="text-2xl font-bold text-red-600">{result.skipped}</p>
              <p className="text-sm text-muted-foreground">Skipped</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Skipped Errors */}
      {result.skippedErrors.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <XCircle className="h-5 w-5 text-red-600" /> Skipped Row Details
            </CardTitle>
          </CardHeader>
          <CardContent className="max-h-64 overflow-y-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-16">Row</TableHead>
                  <TableHead>Column</TableHead>
                  <TableHead>Reason</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {result.skippedErrors.map((err, i) => (
                  <TableRow key={i}>
                    <TableCell>
                      <Badge variant="destructive">{err.row}</Badge>
                    </TableCell>
                    <TableCell className="font-medium">{err.column}</TableCell>
                    <TableCell>{err.message}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}

      {/* Actions */}
      <div className="flex justify-center gap-4">
        <Button variant="outline" onClick={onReset}>
          <RotateCcw className="mr-2 h-4 w-4" /> Import More Data
        </Button>
        <Link href="/dashboard/settings">
          <Button>
            <ArrowLeft className="mr-2 h-4 w-4" /> Back to Settings
          </Button>
        </Link>
      </div>
    </div>
  )
}
