import { SignedIn, SignedOut } from '@clerk/nextjs'

export default function OnboardingLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <SignedOut>
        {/* redirect handled client-side by clerk */}
      </SignedOut>
      <SignedIn>
        <div className="min-h-screen bg-gradient-to-br from-blue-50 to-cyan-100 flex items-center justify-center p-4">
          {children}
        </div>
      </SignedIn>
    </>
  )
}
