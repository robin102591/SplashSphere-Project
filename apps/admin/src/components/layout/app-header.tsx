'use client'

import { useUser, useAuth, useOrganization } from '@clerk/nextjs'
import { useRouter } from 'next/navigation'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
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
import { Building2, LogOut, Settings, User } from 'lucide-react'
import { ConnectionStatusBadge } from '@/components/connection-status'

export function AppHeader() {
  const { user } = useUser()
  const { signOut } = useAuth()
  const { organization } = useOrganization()
  const router = useRouter()

  const initials = [user?.firstName, user?.lastName]
    .filter(Boolean)
    .map((n) => n![0])
    .join('')
    .toUpperCase()

  const handleSignOut = async () => {
    await signOut()
    router.push('/sign-in')
  }

  return (
    <header className="flex h-14 items-center gap-4 border-b px-4 lg:px-6">
      <SidebarTrigger className="-ml-1" />
      <Separator orientation="vertical" className="h-4" />

      {/* Tenant display — custom, NOT OrganizationSwitcher */}
      {organization && (
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Building2 className="h-4 w-4" />
          <span>{organization.name}</span>
        </div>
      )}

      <div className="ml-auto flex items-center gap-3">
        <ConnectionStatusBadge className="hidden sm:flex" />
        {/* Profile dropdown — custom, NOT UserButton */}
        <DropdownMenu>
          <DropdownMenuTrigger className="focus:outline-none">
            <Avatar className="h-8 w-8 cursor-pointer">
              <AvatarImage src={user?.imageUrl} alt={user?.fullName ?? ''} />
              <AvatarFallback className="text-xs">{initials || 'U'}</AvatarFallback>
            </Avatar>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56">
            <DropdownMenuGroup>
              <DropdownMenuLabel>
                <div className="flex flex-col space-y-1">
                  <p className="text-sm font-medium">{user?.fullName}</p>
                  <p className="text-xs text-muted-foreground truncate">
                    {user?.primaryEmailAddress?.emailAddress}
                  </p>
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
