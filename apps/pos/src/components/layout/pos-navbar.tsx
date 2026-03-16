'use client'

import Link from 'next/link'
import { usePathname, useRouter } from 'next/navigation'
import { useUser, useAuth } from '@clerk/nextjs'
import {
  Home,
  LayoutGrid,
  ListOrdered,
  Clock,
  Users,
  Fingerprint,
  Droplets,
  LogOut,
  ChevronDown,
  MapPin,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { useState } from 'react'
import { ConnectionStatusDot } from '@/components/connection-status'
import { useBranch } from '@/lib/branch-context'

const navItems = [
  { label: 'Home', href: '/home', icon: Home },
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
  const [menuOpen, setMenuOpen] = useState(false)
  const { branchId, branchName, branches, setBranchId } = useBranch()

  const handleSignOut = async () => {
    await signOut()
    router.push('/sign-in')
  }

  return (
    <header className="bg-gray-900 border-b border-gray-800">
      <div className="flex items-center h-14 px-4 gap-1">
        {/* Logo + branch selector */}
        <div className="flex items-center gap-2 mr-3 shrink-0">
          <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-blue-500 shrink-0">
            <Droplets className="h-4 w-4 text-white" />
          </div>
          {branches.length > 0 && (
            <div className="relative hidden sm:flex items-center gap-1 bg-gray-800 border border-gray-700 rounded-lg px-2 h-8">
              <MapPin className="h-3 w-3 text-gray-500 shrink-0" />
              <select
                value={branchId}
                onChange={e => setBranchId(e.target.value)}
                className="bg-transparent text-xs text-gray-300 focus:outline-none cursor-pointer pr-1 max-w-[120px]"
              >
                {branches.length > 1 && (
                  <option value="">Select branch…</option>
                )}
                {branches.map(b => (
                  <option key={b.id} value={b.id}>{b.name}</option>
                ))}
              </select>
            </div>
          )}
          {!branchId && (
            <span className="hidden sm:block text-xs text-yellow-500 font-medium">No branch</span>
          )}
          <ConnectionStatusDot className="hidden sm:inline-block" />
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
