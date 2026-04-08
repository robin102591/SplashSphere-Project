import type { Metadata, Viewport } from 'next'
import { Plus_Jakarta_Sans, JetBrains_Mono } from 'next/font/google'
import { Navbar } from '@/components/marketing/navbar'
import { Footer } from '@/components/marketing/footer'
import './globals.css'

const plusJakarta = Plus_Jakarta_Sans({
  subsets: ['latin'],
  variable: '--font-sans',
})

const jetbrainsMono = JetBrains_Mono({
  subsets: ['latin'],
  variable: '--font-mono',
})

export const metadata: Metadata = {
  title: {
    default: 'SplashSphere — Car Wash Management Software for the Philippines',
    template: '%s | SplashSphere',
  },
  description:
    'POS, commission tracking, weekly payroll, and multi-branch management built for Philippine car wash businesses. Start your free trial today.',
  metadataBase: new URL('https://splashsphere.ph'),
  openGraph: {
    title: 'SplashSphere — Smart Car Wash Management',
    description:
      'Manage commissions, payroll, queues, and daily cash — all in one platform designed for Philippine car wash operations.',
    url: 'https://splashsphere.ph',
    siteName: 'SplashSphere',
    locale: 'en_PH',
    type: 'website',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'SplashSphere — Smart Car Wash Management',
    description:
      'Manage commissions, payroll, queues, and daily cash — all in one platform designed for Philippine car wash operations.',
  },
  robots: {
    index: true,
    follow: true,
  },
}

export const viewport: Viewport = {
  themeColor: '#2563eb',
  width: 'device-width',
  initialScale: 1,
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={`${plusJakarta.variable} ${jetbrainsMono.variable}`}>
      <body className="font-[family-name:var(--font-sans)]">
        <Navbar />
        <main>{children}</main>
        <Footer />
      </body>
    </html>
  )
}
