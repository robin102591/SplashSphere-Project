'use client'

import { useCallback, useEffect, useState } from 'react'
import Link from 'next/link'
import { Droplets, Menu, X } from 'lucide-react'
import { cn } from '@/lib/utils'

const navLinks = [
  { label: 'Features', href: '/features' },
  { label: 'Pricing', href: '/pricing' },
  { label: 'About', href: '/about' },
  { label: 'Blog', href: '/blog' },
  { label: 'Contact', href: '/contact' },
] as const

const CTA_HREF = 'https://app.splashsphere.ph/sign-up'

export function Navbar() {
  const [scrolled, setScrolled] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)

  useEffect(() => {
    function handleScroll() {
      setScrolled(window.scrollY > 10)
    }

    handleScroll()
    window.addEventListener('scroll', handleScroll, { passive: true })
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  // Lock body scroll when mobile menu is open
  useEffect(() => {
    if (mobileOpen) {
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
    return () => {
      document.body.style.overflow = ''
    }
  }, [mobileOpen])

  const closeMobile = useCallback(() => setMobileOpen(false), [])

  return (
    <header
      className={cn(
        'fixed top-0 w-full z-50 transition-all duration-300',
        scrolled || mobileOpen
          ? 'bg-white/95 backdrop-blur-md shadow-sm'
          : 'bg-transparent'
      )}
    >
      <nav className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4 lg:px-8">
        {/* Logo */}
        <Link href="/" className="flex items-center gap-2" onClick={closeMobile}>
          <Droplets
            className={cn(
              'h-7 w-7 transition-colors duration-300',
              scrolled || mobileOpen ? 'text-splash-500' : 'text-white'
            )}
            strokeWidth={2.25}
          />
          <span
            className={cn(
              'text-xl font-bold tracking-tight transition-colors duration-300',
              scrolled || mobileOpen ? 'text-splash-500' : 'text-white'
            )}
          >
            SplashSphere
          </span>
        </Link>

        {/* Desktop nav links */}
        <ul className="hidden items-center gap-8 lg:flex">
          {navLinks.map((link) => (
            <li key={link.href}>
              <Link
                href={link.href}
                className={cn(
                  'text-sm font-medium transition-colors duration-200',
                  scrolled
                    ? 'text-foreground/70 hover:text-foreground'
                    : 'text-white/80 hover:text-white'
                )}
              >
                {link.label}
              </Link>
            </li>
          ))}
        </ul>

        {/* Desktop CTA + mobile toggle */}
        <div className="flex items-center gap-4">
          <a
            href={CTA_HREF}
            className="hidden rounded-lg bg-splash-500 px-5 py-2.5 text-sm font-semibold text-white transition-colors duration-200 hover:bg-splash-600 lg:inline-block"
          >
            Start Free Trial
          </a>

          <button
            type="button"
            aria-label={mobileOpen ? 'Close menu' : 'Open menu'}
            aria-expanded={mobileOpen}
            className={cn(
              'inline-flex h-10 w-10 items-center justify-center rounded-md transition-colors duration-200 lg:hidden',
              scrolled || mobileOpen
                ? 'text-foreground hover:bg-muted'
                : 'text-white hover:bg-white/10'
            )}
            onClick={() => setMobileOpen((prev) => !prev)}
          >
            {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
        </div>
      </nav>

      {/* Mobile menu */}
      <div
        className={cn(
          'overflow-hidden transition-all duration-300 lg:hidden',
          mobileOpen ? 'max-h-[400px] opacity-100' : 'max-h-0 opacity-0'
        )}
        aria-hidden={!mobileOpen}
      >
        <div className="border-t border-border bg-white px-6 pb-6 pt-4">
          <ul className="flex flex-col gap-1">
            {navLinks.map((link) => (
              <li key={link.href}>
                <Link
                  href={link.href}
                  className="block rounded-md px-3 py-2.5 text-sm font-medium text-foreground/70 transition-colors hover:bg-muted hover:text-foreground"
                  onClick={closeMobile}
                >
                  {link.label}
                </Link>
              </li>
            ))}
          </ul>

          <div className="mt-4 border-t border-border pt-4">
            <a
              href={CTA_HREF}
              className="block w-full rounded-lg bg-splash-500 px-5 py-2.5 text-center text-sm font-semibold text-white transition-colors duration-200 hover:bg-splash-600"
              onClick={closeMobile}
            >
              Start Free Trial
            </a>
          </div>
        </div>
      </div>
    </header>
  )
}
