'use client'

import { useCallback, useEffect, useState } from 'react'
import { useUser, useAuth, useOrganization } from '@clerk/nextjs'
import { useTheme } from 'next-themes'
import { useRouter } from 'next/navigation'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { SidebarTrigger } from '@/components/ui/sidebar'
import { Separator } from '@/components/ui/separator'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import {
  LogOut, Moon, Search, Settings, Sun, User,
} from 'lucide-react'
import { ConnectionStatusDot } from '@/components/connection-status'
import { SearchDialog } from '@/components/search-dialog'
import { NotificationDropdown } from '@/components/layout/notification-dropdown'
import { LanguageSwitcher } from '@/components/layout/language-switcher'

export function AppHeader() {
  const { user } = useUser()
  const { signOut } = useAuth()
  const { organization, membership } = useOrganization()
  const router = useRouter()
  const [searchOpen, setSearchOpen] = useState(false)

  // Cmd+K / Ctrl+K global shortcut
  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
        e.preventDefault()
        setSearchOpen((prev) => !prev)
      }
    }
    document.addEventListener('keydown', onKeyDown)
    return () => document.removeEventListener('keydown', onKeyDown)
  }, [])

  const initials = [user?.firstName, user?.lastName]
    .filter(Boolean)
    .map((n) => n![0])
    .join('')
    .toUpperCase()

  const role = membership?.role?.replace('org:', '') ?? 'member'

  const handleSignOut = async () => {
    await signOut()
    router.push('/sign-in')
  }

  return (
    <header className="flex h-14 items-center gap-3 border-b px-4 lg:px-6">
      <SidebarTrigger className="-ml-1" />
      <Separator orientation="vertical" className="h-4" />

      {/* Global search — desktop trigger */}
      <div className="hidden md:flex flex-1 justify-center">
        <button
          onClick={() => setSearchOpen(true)}
          className="inline-flex items-center gap-2 w-full max-w-sm rounded-md border bg-muted/50 px-3 py-1.5 text-sm text-muted-foreground hover:bg-muted transition-colors"
        >
          <Search className="h-4 w-4" />
          <span className="flex-1 text-left">Search...</span>
          <kbd className="hidden sm:inline-flex h-5 items-center gap-0.5 rounded border bg-background px-1.5 font-mono text-[10px] font-medium">
            ⌘K
          </kbd>
        </button>
      </div>
      {/* Global search — mobile toggle */}
      <div className="flex-1 md:hidden" />
      <Tooltip>
        <TooltipTrigger render={
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 md:hidden"
            onClick={() => setSearchOpen(true)}
          />
        }>
          <Search className="h-4 w-4" />
        </TooltipTrigger>
        <TooltipContent>Search</TooltipContent>
      </Tooltip>

      <SearchDialog open={searchOpen} onOpenChange={setSearchOpen} />

      <div className="flex items-center gap-1.5">
        <ConnectionStatusDot className="hidden sm:block" />

        {/* Language switcher */}
        <LanguageSwitcher />

        {/* Dark mode toggle */}
        <DarkModeToggle />

        {/* Notification bell */}
        <NotificationDropdown />

        {/* Profile dropdown */}
        <DropdownMenu>
          <DropdownMenuTrigger className="focus:outline-none">
            <Avatar className="h-8 w-8 cursor-pointer">
              <AvatarImage src={user?.imageUrl} alt={user?.fullName ?? ''} />
              <AvatarFallback className="text-xs" suppressHydrationWarning>{initials || 'U'}</AvatarFallback>
            </Avatar>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56">
            <DropdownMenuGroup>
              <DropdownMenuLabel>
                <div className="flex flex-col space-y-1">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium truncate">{user?.fullName}</p>
                    <Badge variant="outline" className="text-[10px] px-1.5 py-0 capitalize">
                      {role}
                    </Badge>
                  </div>
                  <p className="text-xs text-muted-foreground truncate">
                    {user?.primaryEmailAddress?.emailAddress}
                  </p>
                  {organization && (
                    <p className="text-xs text-muted-foreground truncate">
                      {organization.name}
                    </p>
                  )}
                </div>
              </DropdownMenuLabel>
            </DropdownMenuGroup>
            <DropdownMenuSeparator />
            <DropdownMenuGroup>
              <DropdownMenuItem onClick={() => router.push('/dashboard/settings')}>
                <User className="mr-2 h-4 w-4" />
                Profile
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => router.push('/dashboard/settings')}>
                <Settings className="mr-2 h-4 w-4" />
                Settings
              </DropdownMenuItem>
            </DropdownMenuGroup>
            <DropdownMenuSeparator />
            <DropdownMenuGroup>
              <DropdownMenuItem
                onClick={handleSignOut}
                className="text-destructive focus:text-destructive"
              >
                <LogOut className="mr-2 h-4 w-4" />
                Sign out
              </DropdownMenuItem>
            </DropdownMenuGroup>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  )
}

// ── Dark mode toggle ─────────────────────────────────────────────────────────

function DarkModeToggle() {
  const { resolvedTheme, setTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  return (
    <Tooltip>
      <TooltipTrigger render={
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={() => setTheme(isDark ? 'light' : 'dark')}
        />
      }>
        {isDark ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
      </TooltipTrigger>
      <TooltipContent>{isDark ? 'Light mode' : 'Dark mode'}</TooltipContent>
    </Tooltip>
  )
}
