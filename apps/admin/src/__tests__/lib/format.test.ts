import { describe, it, expect } from 'vitest'
import { formatPeso, formatPesoCompact } from '@/lib/format'

describe('formatPeso', () => {
  it('formats positive amount with peso sign', () => {
    const result = formatPeso(2999.5)
    // Intl may use a narrow currency symbol or PHP prefix depending on locale support
    expect(result).toMatch(/₱|PHP/)
    expect(result).toContain('2,999.50')
  })

  it('formats zero', () => {
    const result = formatPeso(0)
    expect(result).toMatch(/₱|PHP/)
    expect(result).toContain('0.00')
  })

  it('formats negative amount', () => {
    const result = formatPeso(-150)
    expect(result).toContain('150.00')
  })

  it('rounds to 2 decimal places', () => {
    const result = formatPeso(99.999)
    expect(result).toContain('100.00')
  })

  it('formats large amount with commas', () => {
    const result = formatPeso(1234567.89)
    expect(result).toContain('1,234,567.89')
  })
})

describe('formatPesoCompact', () => {
  it('formats millions with M suffix', () => {
    expect(formatPesoCompact(1500000)).toBe('₱1.5M')
    expect(formatPesoCompact(2000000)).toBe('₱2.0M')
  })

  it('formats thousands with k suffix', () => {
    expect(formatPesoCompact(12000)).toBe('₱12k')
    expect(formatPesoCompact(1500)).toBe('₱2k') // rounds via toFixed(0)
  })

  it('formats small amounts without suffix', () => {
    expect(formatPesoCompact(500)).toBe('₱500')
    expect(formatPesoCompact(0)).toBe('₱0')
  })

  it('handles negative millions', () => {
    expect(formatPesoCompact(-1500000)).toBe('₱-1.5M')
  })

  it('handles negative thousands', () => {
    expect(formatPesoCompact(-5000)).toBe('₱-5k')
  })
})
