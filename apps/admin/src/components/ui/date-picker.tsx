'use client'

import * as React from 'react'
import { format, parse } from 'date-fns'
import { CalendarIcon } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { Calendar } from '@/components/ui/calendar'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'

interface DatePickerProps {
  /** ISO date string (YYYY-MM-DD) */
  value?: string
  /** Called with ISO date string (YYYY-MM-DD) or empty string */
  onChange: (value: string) => void
  placeholder?: string
  className?: string
  disabled?: boolean
}

export function DatePicker({
  value,
  onChange,
  placeholder = 'Pick a date',
  className,
  disabled,
}: DatePickerProps) {
  const [open, setOpen] = React.useState(false)

  const date = value ? parse(value, 'yyyy-MM-dd', new Date()) : undefined
  const isValidDate = date && !isNaN(date.getTime())

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          disabled={disabled}
          className={cn(
            'justify-start text-left font-normal h-9',
            !isValidDate && 'text-muted-foreground',
            className
          )}
        >
          <CalendarIcon className="mr-2 h-4 w-4 shrink-0" />
          {isValidDate ? format(date, 'MMM d, yyyy') : <span>{placeholder}</span>}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="single"
          selected={isValidDate ? date : undefined}
          onSelect={(day) => {
            if (day) {
              const iso = format(day, 'yyyy-MM-dd')
              onChange(iso)
            } else {
              onChange('')
            }
            setOpen(false)
          }}
          defaultMonth={isValidDate ? date : undefined}
        />
      </PopoverContent>
    </Popover>
  )
}
