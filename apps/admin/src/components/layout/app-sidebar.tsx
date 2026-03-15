'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from '@/components/ui/sidebar'
import {
  LayoutDashboard,
  GitBranch,
  Wrench,
  Package,
  Users,
  Car,
  ShoppingBag,
  CreditCard,
  BarChart3,
  Settings,
  Droplets,
} from 'lucide-react'

const navGroups = [
  {
    label: 'Overview',
    items: [
      { label: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
    ],
  },
  {
    label: 'Operations',
    items: [
      { label: 'Branches', href: '/dashboard/branches', icon: GitBranch },
      { label: 'Services', href: '/dashboard/services', icon: Wrench },
      { label: 'Packages', href: '/dashboard/packages', icon: Package },
      { label: 'Merchandise', href: '/dashboard/merchandise', icon: ShoppingBag },
    ],
  },
  {
    label: 'People',
    items: [
      { label: 'Employees', href: '/dashboard/employees', icon: Users },
      { label: 'Customers', href: '/dashboard/customers', icon: Car },
    ],
  },
  {
    label: 'Finance',
    items: [
      { label: 'Transactions', href: '/dashboard/transactions', icon: CreditCard },
      { label: 'Payroll', href: '/dashboard/payroll', icon: CreditCard },
      { label: 'Reports', href: '/dashboard/reports', icon: BarChart3 },
    ],
  },
  {
    label: 'Configuration',
    items: [
      { label: 'Pricing Modifiers', href: '/dashboard/pricing-modifiers', icon: Settings },
      { label: 'Vehicle Types', href: '/dashboard/vehicle-types', icon: Car },
      { label: 'Settings', href: '/dashboard/settings', icon: Settings },
    ],
  },
]

export function AppSidebar() {
  const pathname = usePathname()

  return (
    <Sidebar>
      <SidebarHeader>
        <div className="flex items-center gap-2 px-2 py-3">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Droplets className="h-4 w-4" />
          </div>
          <div>
            <p className="text-sm font-semibold leading-none">SplashSphere</p>
            <p className="text-xs text-muted-foreground">Admin</p>
          </div>
        </div>
      </SidebarHeader>

      <SidebarContent>
        {navGroups.map((group) => (
          <SidebarGroup key={group.label}>
            <SidebarGroupLabel>{group.label}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {group.items.map((item) => (
                  <SidebarMenuItem key={item.href}>
                    <SidebarMenuButton
                      render={<Link href={item.href} />}
                      isActive={
                        pathname === item.href ||
                        (item.href !== '/dashboard' && pathname.startsWith(item.href))
                      }
                    >
                      <item.icon className="h-4 w-4" />
                      <span>{item.label}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        ))}
      </SidebarContent>

      <SidebarFooter />
    </Sidebar>
  )
}
