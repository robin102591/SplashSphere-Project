import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MoneyDisplay } from '@/components/ui/money-display'

describe('MoneyDisplay', () => {
  it('renders formatted peso amount', () => {
    render(<MoneyDisplay amount={2999.5} />)
    // Check the amount text is present (Intl formatting varies by env)
    expect(screen.getByText(/2,999\.50/)).toBeInTheDocument()
  })

  it('renders with different sizes', () => {
    const { container } = render(<MoneyDisplay amount={100} size="lg" />)
    expect(container.querySelector('.text-2xl')).toBeInTheDocument()
  })

  it('shows positive trend indicator', () => {
    render(<MoneyDisplay amount={1000} trend={{ value: 12, label: 'vs last week' }} />)
    expect(screen.getByText(/12%/)).toBeInTheDocument()
    expect(screen.getByText(/vs last week/)).toBeInTheDocument()
  })

  it('shows negative trend indicator in red', () => {
    render(<MoneyDisplay amount={1000} trend={{ value: -5, label: 'vs last week' }} />)
    const trend = screen.getByText(/5%/)
    expect(trend).toBeInTheDocument()
    expect(trend.className).toContain('text-red-600')
  })

  it('does not show trend when not provided', () => {
    render(<MoneyDisplay amount={500} />)
    expect(screen.queryByText(/%/)).not.toBeInTheDocument()
  })
})
