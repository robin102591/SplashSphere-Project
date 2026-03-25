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
  Wallet,
  Lock,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { useState, useEffect, useRef } from 'react'
import { ConnectionStatusDot } from '@/components/connection-status'
import { useBranch } from '@/lib/branch-context'
import { useLockStore } from '@/lib/use-lock-store'

const navItems = [
  { label: 'Home', href: '/home', icon: Home },
  { label: 'Queue', href: '/queue', icon: ListOrdered },
  { label: 'Transaction', href: '/transactions/new', icon: LayoutGrid },
  { label: 'History', href: '/history', icon: Clock },
  { label: 'Customers', href: '/customers/lookup', icon: Users },
  { label: 'Attendance', href: '/attendance', icon: Fingerprint },
  { label: 'Shift', href: '/shift', icon: Wallet },
]

// ── Live clock ─────────────────────────────────────────────────────────────────

function LiveClock() {
  const [time, setTime] = useState('')

  useEffect(() => {
    const fmt = new Intl.DateTimeFormat('en-PH', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true,
      timeZone: 'Asia/Manila',
    })
    const tick = () => setTime(fmt.format(new Date()))
    tick()
    const id = setInterval(tick, 1000)
    return () => clearInterval(id)
  }, [])

  return (
    <span className="text-sm font-mono text-gray-300 tabular-nums">{time}</span>
  )
}

// ── Branch selector dropdown ──────────────────────────────────────────────────

function BranchSelector({
  branchId,
  branchName,
  branches,
  onSelect,
}: {
  branchId: string
  branchName: string
  branches: { id: string; name: string }[]
  onSelect: (id: string) => void
}) {
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  return (
    <div className="relative" ref={ref}>
      <button
        onClick={() => setOpen((v) => !v)}
        className="flex items-center gap-1.5 bg-gray-800 border border-gray-700 rounded-lg px-2.5 h-9 text-sm text-gray-300 hover:bg-gray-750 hover:border-gray-600 transition-colors duration-150 active:scale-[0.98] max-w-[200px]"
      >
        <MapPin className="h-3.5 w-3.5 text-blue-400 shrink-0" />
        <span className="truncate">{branchName || 'Select branch…'}</span>
        <ChevronDown className={cn('h-3.5 w-3.5 text-gray-500 shrink-0 transition-transform duration-150', open && 'rotate-180')} />
      </button>
      {open && (
        <div className="absolute left-0 top-full mt-1.5 z-50 min-w-[200px] rounded-xl bg-gray-800 border border-gray-700 shadow-xl py-1 overflow-hidden animate-in fade-in-0 zoom-in-95 slide-in-from-top-2 duration-150">
          {branches.length > 1 && !branchId && (
            <div className="px-3 py-2 text-xs text-gray-500 border-b border-gray-700">
              Select a branch
            </div>
          )}
          {branches.map((b) => (
            <button
              key={b.id}
              onClick={() => {
                onSelect(b.id)
                setOpen(false)
              }}
              className={cn(
                'w-full flex items-center gap-2 px-3 py-2.5 text-sm transition-colors duration-100',
                b.id === branchId
                  ? 'bg-blue-600/15 text-blue-400'
                  : 'text-gray-300 hover:bg-gray-700/60'
              )}
            >
              <MapPin className={cn('h-3.5 w-3.5 shrink-0', b.id === branchId ? 'text-blue-400' : 'text-gray-600')} />
              <span className="truncate">{b.name}</span>
              {b.id === branchId && (
                <svg className="ml-auto h-4 w-4 text-blue-400 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="20 6 9 17 4 12" />
                </svg>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

// ── Top bar ────────────────────────────────────────────────────────────────────

function TopBar() {
  const { user } = useUser()
  const { signOut } = useAuth()
  const router = useRouter()
  const [menuOpen, setMenuOpen] = useState(false)
  const { branchId, branchName, branches, setBranchId } = useBranch()
  const lockScreen = useLockStore((s) => s.lock)

  const handleSignOut = async () => {
    await signOut()
    router.push('/sign-in')
  }

  return (
    <header className="bg-gray-900 border-b border-gray-800">
      <div className="flex items-center h-14 px-4 gap-3">
        {/* Logo + branch */}
        <div className="flex items-center gap-2 shrink-0">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-500 shrink-0">
            <Droplets className="h-4.5 w-4.5 text-white" />
          </div>
          {branches.length > 0 && (
            <BranchSelector
              branchId={branchId}
              branchName={branchName}
              branches={branches}
              onSelect={setBranchId}
            />
          )}
          {!branchId && (
            <span className="text-sm text-yellow-500 font-medium">No branch</span>
          )}
          <ConnectionStatusDot />
        </div>

        {/* Spacer */}
        <div className="flex-1" />

        {/* Cashier name */}
        <span className="hidden sm:block text-sm text-gray-300">
          {user?.fullName ?? user?.firstName}
        </span>

        {/* Live clock */}
        <LiveClock />

        {/* Lock button */}
        <button
          onClick={lockScreen}
          title="Lock screen"
          className="flex items-center justify-center h-10 w-10 rounded-lg text-gray-400 hover:text-white hover:bg-gray-800 transition-colors duration-150 active:scale-[0.97] shrink-0"
        >
          <Lock className="h-5 w-5" />
        </button>

        {/* Profile menu */}
        <div className="relative shrink-0">
          <button
            onClick={() => setMenuOpen((v) => !v)}
            className="flex items-center gap-1.5 px-2.5 h-10 rounded-lg text-gray-400 hover:text-white hover:bg-gray-800 transition-colors duration-150 active:scale-[0.97]"
          >
            <ChevronDown className="h-4 w-4" />
          </button>
          {menuOpen && (
            <>
              <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />
              <div className="absolute right-0 top-full mt-1 z-20 w-52 rounded-xl bg-gray-800 border border-gray-700 shadow-xl py-1 overflow-hidden">
                <div className="px-3 py-2.5 border-b border-gray-700">
                  <p className="text-sm font-medium text-white">{user?.fullName}</p>
                  <p className="text-xs text-gray-400 truncate">
                    {user?.primaryEmailAddress?.emailAddress}
                  </p>
                </div>
                <button
                  onClick={handleSignOut}
                  className="w-full flex items-center gap-2 px-3 py-3 text-sm text-red-400 hover:bg-gray-700 transition-colors duration-150"
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

// ── Desktop nav pills ──────────────────────────────────────────────────────────

function NavPills() {
  const pathname = usePathname()

  return (
    <nav className="hidden md:flex items-center gap-2 bg-gray-900/60 border-b border-gray-800 px-4 py-2">
      {navItems.map((item) => {
        const active =
          pathname === item.href ||
          (item.href !== '/transactions/new' && pathname.startsWith(item.href))
        return (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              'flex items-center gap-2 px-4 min-h-[44px] rounded-xl text-sm font-medium transition-colors duration-150 active:scale-[0.97]',
              active
                ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20'
                : 'text-gray-400 hover:text-white hover:bg-gray-800'
            )}
          >
            <item.icon className="h-5 w-5 shrink-0" />
            <span>{item.label}</span>
          </Link>
        )
      })}
    </nav>
  )
}

// ── Mobile bottom tab bar ──────────────────────────────────────────────────────

function MobileTabBar() {
  const pathname = usePathname()

  return (
    <nav className="md:hidden fixed bottom-0 inset-x-0 z-30 bg-gray-900 border-t border-gray-800 h-16 flex items-stretch px-1 safe-area-pb">
      {navItems.map((item) => {
        const active =
          pathname === item.href ||
          (item.href !== '/transactions/new' && pathname.startsWith(item.href))
        return (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              'flex flex-col items-center justify-center flex-1 gap-0.5 text-[10px] font-medium transition-colors duration-150 active:scale-[0.97]',
              active
                ? 'text-blue-400'
                : 'text-gray-500 hover:text-gray-300'
            )}
          >
            <item.icon className={cn('h-5 w-5', active && 'text-blue-400')} />
            <span className="truncate">{item.label}</span>
          </Link>
        )
      })}
    </nav>
  )
}

// ── Exported composite ─────────────────────────────────────────────────────────

export function PosNavbar() {
  return (
    <>
      <TopBar />
      <NavPills />
      <MobileTabBar />
    </>
  )
}
