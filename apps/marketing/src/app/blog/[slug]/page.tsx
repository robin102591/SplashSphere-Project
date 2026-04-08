import type { Metadata } from 'next'
import Link from 'next/link'
import { notFound } from 'next/navigation'
import { getBlogPost, getBlogPosts } from '@/lib/blog-data'

type Props = {
  params: Promise<{ slug: string }>
}

export async function generateStaticParams() {
  const posts = getBlogPosts()
  return posts.map((post) => ({ slug: post.slug }))
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params
  const post = getBlogPost(slug)
  if (!post) return {}

  return {
    title: post.title,
    description: post.excerpt,
    openGraph: {
      title: post.title,
      description: post.excerpt,
      type: 'article',
      publishedTime: post.date,
      authors: [post.author],
    },
  }
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-PH', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}

export default async function BlogPostPage({ params }: Props) {
  const { slug } = await params
  const post = getBlogPost(slug)

  if (!post) {
    notFound()
  }

  return (
    <article className="py-12 md:py-16">
      <div className="mx-auto max-w-3xl px-6">
        {/* Breadcrumb */}
        <nav className="mb-8 text-sm text-muted-foreground">
          <Link
            href="/blog"
            className="transition-colors hover:text-splash-600"
          >
            Blog
          </Link>
          <span className="mx-2">/</span>
          <span className="text-foreground">{post.title}</span>
        </nav>

        {/* Meta */}
        <div className="flex flex-wrap items-center gap-3 text-sm text-muted-foreground">
          <time dateTime={post.date}>{formatDate(post.date)}</time>
          <span aria-hidden="true">&middot;</span>
          <span>{post.author}</span>
          <span aria-hidden="true">&middot;</span>
          <span>{post.readTime}</span>
        </div>

        {/* Title */}
        <h1 className="mt-4 text-3xl font-bold tracking-tight text-foreground md:text-4xl">
          {post.title}
        </h1>

        {/* Content */}
        <div
          className="prose prose-splash mt-10 max-w-none [&_a]:text-splash-600 [&_a]:underline hover:[&_a]:text-splash-700 [&_h2]:mb-4 [&_h2]:mt-8 [&_h2]:text-xl [&_h2]:font-semibold [&_h2]:text-foreground [&_li]:text-muted-foreground [&_p]:leading-relaxed [&_p]:text-muted-foreground [&_ul]:my-4 [&_ul]:list-disc [&_ul]:pl-6"
          dangerouslySetInnerHTML={{ __html: post.content }}
        />

        {/* CTA Banner */}
        <div className="mt-16 rounded-xl bg-gradient-to-br from-splash-50 to-splash-100 p-8 text-center">
          <h2 className="text-xl font-semibold text-splash-900">
            Try SplashSphere free for 14 days
          </h2>
          <p className="mx-auto mt-2 max-w-md text-sm text-splash-700">
            Automate commissions, manage queues, and run payroll in minutes
            — built for Philippine car wash businesses.
          </p>
          <Link
            href="/pricing"
            className="mt-4 inline-block rounded-lg bg-splash-600 px-6 py-2.5 text-sm font-semibold text-white transition-colors hover:bg-splash-700"
          >
            View Pricing
          </Link>
        </div>

        {/* Back link */}
        <div className="mt-10">
          <Link
            href="/blog"
            className="text-sm font-medium text-splash-600 transition-colors hover:text-splash-700"
          >
            &larr; Back to Blog
          </Link>
        </div>
      </div>
    </article>
  )
}
