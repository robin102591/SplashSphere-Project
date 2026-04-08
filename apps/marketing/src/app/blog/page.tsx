import type { Metadata } from 'next'
import Link from 'next/link'
import { getBlogPosts } from '@/lib/blog-data'

export const metadata: Metadata = {
  title: 'Blog',
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-PH', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
}

export default function BlogPage() {
  const posts = getBlogPosts()

  return (
    <>
      {/* Hero */}
      <section className="bg-gradient-to-br from-splash-900 to-splash-800 py-20 text-white">
        <div className="mx-auto max-w-6xl px-6 text-center">
          <h1 className="text-4xl font-bold tracking-tight md:text-5xl">
            Blog
          </h1>
          <p className="mx-auto mt-4 max-w-2xl text-lg text-splash-200">
            Tips, guides, and insights for Philippine car wash owners.
          </p>
        </div>
      </section>

      {/* Post Grid */}
      <section className="py-16 md:py-20">
        <div className="mx-auto max-w-6xl px-6">
          <div className="grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
            {posts.map((post) => (
              <article
                key={post.slug}
                className="group overflow-hidden rounded-xl border border-border bg-white shadow-sm transition-shadow hover:shadow-md"
              >
                {/* Color header placeholder */}
                <div className="h-40 bg-gradient-to-br from-splash-500 to-splash-700" />

                <div className="p-6">
                  <div className="flex items-center gap-3 text-sm text-muted-foreground">
                    <time dateTime={post.date}>{formatDate(post.date)}</time>
                    <span aria-hidden="true">&middot;</span>
                    <span>{post.readTime}</span>
                  </div>

                  <h2 className="mt-3 text-lg font-semibold leading-snug text-foreground">
                    <Link
                      href={`/blog/${post.slug}`}
                      className="transition-colors hover:text-splash-600"
                    >
                      {post.title}
                    </Link>
                  </h2>

                  <p className="mt-2 line-clamp-3 text-sm text-muted-foreground">
                    {post.excerpt}
                  </p>

                  <div className="mt-4 flex items-center justify-between text-sm">
                    <span className="font-medium text-foreground">
                      {post.author}
                    </span>
                    <Link
                      href={`/blog/${post.slug}`}
                      className="font-medium text-splash-600 transition-colors hover:text-splash-700"
                    >
                      Read more &rarr;
                    </Link>
                  </div>
                </div>
              </article>
            ))}
          </div>
        </div>
      </section>
    </>
  )
}
