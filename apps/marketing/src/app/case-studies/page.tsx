import type { Metadata } from 'next'
import Link from 'next/link'

export const metadata: Metadata = {
  title: 'Case Studies',
}

export default function CaseStudiesPage() {
  return (
    <>
      {/* Hero Banner */}
      <section className="bg-splash-900 py-12 text-white">
        <div className="mx-auto max-w-3xl px-6 text-center">
          <h1 className="text-4xl font-bold tracking-tight">Case Studies</h1>
          <p className="mt-3 text-splash-200">
            Real stories from real car wash businesses
          </p>
        </div>
      </section>

      {/* Coming Soon Content */}
      <section className="mx-auto max-w-3xl px-6 py-20">
        <div className="rounded-2xl border border-border bg-white px-8 py-16 text-center shadow-sm">
          {/* Illustration Placeholder */}
          <div className="mx-auto flex h-32 w-32 items-center justify-center rounded-full bg-splash-50">
            <svg
              className="h-16 w-16 text-splash-400"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={1.5}
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 6.042A8.967 8.967 0 0 0 6 3.75c-1.052 0-2.062.18-3 .512v14.25A8.987 8.987 0 0 1 6 18c2.305 0 4.408.867 6 2.292m0-14.25a8.966 8.966 0 0 1 6-2.292c1.052 0 2.062.18 3 .512v14.25A8.987 8.987 0 0 0 18 18a8.967 8.967 0 0 0-6 2.292m0-14.25v14.25"
              />
            </svg>
          </div>

          <h2 className="mt-8 text-3xl font-bold text-foreground">
            Coming Soon
          </h2>
          <p className="mx-auto mt-4 max-w-md leading-relaxed text-muted-foreground">
            We&apos;re gathering success stories from car wash owners across the
            Philippines. Check back soon!
          </p>

          <div className="mt-10">
            <Link
              href="/"
              className="inline-flex items-center gap-2 rounded-lg bg-splash-600 px-6 py-3 font-semibold text-white transition-colors hover:bg-splash-700"
            >
              &larr; Back to Home
            </Link>
          </div>
        </div>
      </section>
    </>
  )
}
