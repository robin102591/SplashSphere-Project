import type { Metadata } from 'next'
import { redirect } from 'next/navigation'

export const metadata: Metadata = {
  title: 'Sign Up',
}

export default function SignUpRedirectPage() {
  redirect('https://app.splashsphere.ph/sign-up')
}
