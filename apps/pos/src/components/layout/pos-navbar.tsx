'use client'

import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { useUser, useAuth, useOrganization } from '@clerk/nextjs'
import {
  LayoutGrid,
  ListOrdered,
  Clock,
  Users,
  Fingerprint,
  Droplets,
  LogOut,
  ChevronDown,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { useState } from 'react'

const navItems = [
  { label: 'Queue', href: '/queue', icon: ListOrdered },
  { label: 'Transaction', href: '/transactions/new', icon: LayoutGrid },
  { label: 'History', href: '/history', icon: Clock },
  { label: 'Customers', href: '/customers/lookup', icon: Users },
  { label: 'Attendance', href: '/attendance', icon: Fingerprint },
]

export function PosNavbar() {
  const pathname = usePathname()
  const router = useRouter()
  const { user } = useUser()
  const { signOut } = useAuth()
  const { organization } = useOrganization()
  const [menuOpen, setMenuOpen] = useState(false)

  const handleSignOut = async () => {
    await signOut()
    router.push('/sign-in')
  }

  return (
    <header className="bg-gray-900 border-b border-gray-800">
      <div className="flex items-center h-14 px-4 gap-1">
        {/* Logo + branch */}
        <div className="flex items-center gap-2 mr-4 shrink-0">
          <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-blue-500">
            <Droplets className="h-4 w-4 text-white" />
          </div>
          {organization && (
            <span className="text-xs text-gray-400 hidden sm:block">{organization.name}</span>
          )}
        </div>

        {/* Nav */}
        <nav className="flex items-center gap-1 flex-1">
          {navItems.map((item) => {
            const active =
              pathname === item.href ||
              (item.href !== '/transactions/new' && pathname.startsWith(item.href))
            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  'flex items-center gap-1.5 px-3 h-9 rounded-lg text-sm font-medium transition-colors min-w-0',
                  active
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-400 hover:text-white hover:bg-gray-800'
                )}
              >
                <item.icon className="h-4 w-4 shrink-0" />
                <span className="hidden md:block">{item.label}</span>
              </Link>
            )
          })}
        </nav>

        {/* Profile — custom, NOT UserButton */}
        <div className="relative shrink-0">
          <button
            onClick={() => setMenuOpen((v) => !v)}
            className="flex items-center gap-2 px-3 h-9 rounded-lg text-gray-400 hover:text-white hover:bg-gray-800 transition-colors text-sm"
          >
            <span className="hidden sm:block">{user?.firstName}</span>
            <ChevronDown className="h-4 w-4" />
          </button>
          {menuOpen && (
            <>
              <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />
              <div className="absolute right-0 top-full mt-1 z-20 w-48 rounded-xl bg-gray-800 border border-gray-700 shadow-xl py-1 overflow-hidden">
                <div className="px-3 py-2 border-b border-gray-700">
                  <p className="text-sm font-medium text-white">{user?.fullName}</p>
                  <p className="text-xs text-gray-400 truncate">
                    {user?.primaryEmailAddress?.emailAddress}
                  </p>
                </div>
                <button
                  onClick={handleSignOut}
                  className="w-full flex items-center gap-2 px-3 py-2.5 text-sm text-red-400 hover:bg-gray-700 transition-colors"
                >
                  <LogOut className="h-4 w-4" />
                  Sign out
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  )
}
