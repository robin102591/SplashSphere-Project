import { Suspense } from 'react'
import { AuthFlow } from '@/components/auth/auth-flow'

/**
 * Entry point for the Connect phone-OTP sign-in flow.
 * The flow is a single client component with two steps (phone, code) so the
 * transition between them is instant and retains local state (e.g., the
 * phone number the user just typed).
 *
 * Wrapped in <Suspense> because the component reads `?redirect=` via
 * `useSearchParams`, which Next 16 requires to be inside a Suspense boundary.
 */
export default function AuthPage() {
  return (
    <Suspense fallback={null}>
      <AuthFlow />
    </Suspense>
  )
}
