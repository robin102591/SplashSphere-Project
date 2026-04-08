import Link from 'next/link'
import { Droplets, Facebook, Instagram, Linkedin } from 'lucide-react'

const productLinks = [
  { label: 'Features', href: '/features' },
  { label: 'Pricing', href: '/pricing' },
  { label: 'Blog', href: '/blog' },
] as const

const companyLinks = [
  { label: 'About', href: '/about' },
  { label: 'Contact', href: '/contact' },
  { label: 'Careers', href: '/careers' },
] as const

const resourceLinks = [
  { label: 'Case Studies', href: '/case-studies' },
  { label: 'Help Center', href: '/help' },
] as const

const legalLinks = [
  { label: 'Privacy Policy', href: '/privacy' },
  { label: 'Terms of Service', href: '/terms' },
] as const

const socialLinks = [
  {
    label: 'Facebook',
    href: 'https://facebook.com/splashsphere',
    icon: Facebook,
  },
  {
    label: 'Instagram',
    href: 'https://instagram.com/splashsphere',
    icon: Instagram,
  },
  {
    label: 'LinkedIn',
    href: 'https://linkedin.com/company/splashsphere',
    icon: Linkedin,
  },
  {
    label: 'TikTok',
    href: 'https://tiktok.com/@splashsphere',
    icon: null,
  },
] as const

function TikTokIcon({ className }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      aria-hidden="true"
    >
      <path d="M9 12a4 4 0 1 0 4 4V4a5 5 0 0 0 5 5" />
    </svg>
  )
}

function FooterLinkColumn({
  heading,
  links,
}: {
  heading: string
  links: ReadonlyArray<{ label: string; href: string }>
}) {
  return (
    <div>
      <h3 className="mb-4 text-sm font-semibold uppercase tracking-wider text-white">
        {heading}
      </h3>
      <ul className="space-y-3">
        {links.map((link) => (
          <li key={link.href}>
            <Link
              href={link.href}
              className="text-sm text-white/60 transition-colors hover:text-white"
            >
              {link.label}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  )
}

export function Footer() {
  return (
    <footer className="bg-splash-950 text-white">
      <div className="mx-auto max-w-6xl px-6 py-16">
        {/* Main grid */}
        <div className="grid grid-cols-1 gap-12 sm:grid-cols-2 lg:grid-cols-4">
          {/* Brand column */}
          <div className="sm:col-span-2 lg:col-span-1">
            <Link href="/" className="inline-flex items-center gap-2">
              <Droplets className="h-7 w-7 text-splash-400" strokeWidth={2.25} />
              <span className="text-xl font-bold tracking-tight text-white">
                SplashSphere
              </span>
            </Link>
            <p className="mt-4 max-w-xs text-sm leading-relaxed text-white/60">
              The smart car wash management platform by LezanobTech.
            </p>
          </div>

          {/* Product + Resources */}
          <div className="space-y-8">
            <FooterLinkColumn heading="Product" links={productLinks} />
            <FooterLinkColumn heading="Resources" links={resourceLinks} />
          </div>

          {/* Company + Legal */}
          <div className="space-y-8">
            <FooterLinkColumn heading="Company" links={companyLinks} />
            <FooterLinkColumn heading="Legal" links={legalLinks} />
          </div>

          {/* Empty spacer on large screens, collapses on small */}
          <div className="hidden lg:block" />
        </div>

        {/* Bottom bar */}
        <div className="mt-16 flex flex-col items-center gap-6 border-t border-white/10 pt-8 sm:flex-row sm:justify-between">
          <div className="text-center sm:text-left">
            <p className="text-sm text-white/40">
              &copy; {new Date().getFullYear()} LezanobTech. All rights reserved.
            </p>
            <p className="mt-1 text-sm text-white/40">
              Made with{' '}
              <span role="img" aria-label="water drop">
                &#x1F4A7;
              </span>{' '}
              in the Philippines
            </p>
          </div>

          {/* Social icons */}
          <div className="flex items-center gap-4">
            {socialLinks.map((social) => (
              <a
                key={social.label}
                href={social.href}
                target="_blank"
                rel="noopener noreferrer"
                aria-label={social.label}
                className="flex h-9 w-9 items-center justify-center rounded-full text-white/40 transition-colors hover:bg-white/10 hover:text-white"
              >
                {social.icon ? (
                  <social.icon className="h-4 w-4" />
                ) : (
                  <TikTokIcon className="h-4 w-4" />
                )}
              </a>
            ))}
          </div>
        </div>
      </div>
    </footer>
  )
}
