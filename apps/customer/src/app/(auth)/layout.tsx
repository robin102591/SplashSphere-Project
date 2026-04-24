/**
 * Auth layout — opts out of the `(tabs)` bottom-nav shell. We keep the same
 * max-w-lg centered column so the OTP flow feels continuous with the rest of
 * the app on desktop previews.
 */
export default function AuthLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <div className="min-h-[100svh] bg-background">
      <main className="mx-auto max-w-lg w-full px-4 py-8">{children}</main>
    </div>
  )
}
