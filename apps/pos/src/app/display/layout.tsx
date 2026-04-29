import { auth } from '@clerk/nextjs/server'
import { redirect } from 'next/navigation'

/**
 * Customer Display routes — fullscreen, customer-facing screens that mirror a
 * paired POS station. Auth is required (the device operator logs in once, then
 * leaves the tablet running) but we deliberately omit the POS chrome
 * (navbar, shift banner, lock guard) — anything visible here is seen by
 * customers, so internal UI must not leak through.
 */
export default async function DisplayLayout({ children }: { children: React.ReactNode }) {
  const { userId } = await auth()
  if (!userId) redirect('/sign-in')

  return <>{children}</>
}
