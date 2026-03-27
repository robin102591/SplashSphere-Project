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
  SidebarRail,
  useSidebar,
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
  Wallet,
  TrendingDown,
  Percent,
  Banknote,
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
      { label: 'Pricing Rules', href: '/dashboard/pricing-modifiers', icon: Percent },
    ],
  },
  {
    label: 'People',
    items: [
      { label: 'Employees', href: '/dashboard/employees', icon: Users },
      { label: 'Customers', href: '/dashboard/customers', icon: Users },
      { label: 'Vehicles', href: '/dashboard/vehicles', icon: Car },
    ],
  },
  {
    label: 'Finance',
    items: [
      { label: 'Transactions', href: '/dashboard/transactions', icon: CreditCard },
      { label: 'Payroll', href: '/dashboard/payroll', icon: CreditCard },
      { label: 'Cash Advances', href: '/dashboard/cash-advances', icon: Banknote },
      { label: 'Shifts', href: '/dashboard/shifts', icon: Wallet },
      { label: 'Shift Variance', href: '/dashboard/reports/shift-variance', icon: TrendingDown },
      { label: 'Reports', href: '/dashboard/reports', icon: BarChart3 },
    ],
  },
  {
    label: 'Configuration',
    items: [
      { label: 'Settings', href: '/dashboard/settings', icon: Settings },
    ],
  },
]

function SidebarLogo() {
  const { state } = useSidebar()
  return (
    <div className="flex items-center gap-2 px-2 py-3">
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground">
        <Droplets className="h-4 w-4" />
      </div>
      {state === 'expanded' && (
        <div className="overflow-hidden">
          <p className="text-sm font-semibold leading-none">SplashSphere</p>
          <p className="text-xs text-muted-foreground">Admin</p>
        </div>
      )}
    </div>
  )
}

export function AppSidebar() {
  const pathname = usePathname()

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader>
        <SidebarLogo />
      </SidebarHeader>

      <SidebarContent>
        {navGroups.map((group) => (
          <SidebarGroup key={group.label}>
            <SidebarGroupLabel>{group.label}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {group.items.map((item) => {
                  const active =
                    pathname === item.href ||
                    (item.href !== '/dashboard' && pathname.startsWith(item.href))
                  return (
                    <SidebarMenuItem key={item.href}>
                      <SidebarMenuButton
                        render={<Link href={item.href} />}
                        isActive={active}
                        tooltip={item.label}
                        className={active ? 'bg-splash-50 text-splash-700 border-l-2 border-splash-500 hover:bg-splash-100' : ''}
                      >
                        <item.icon className="h-4 w-4" />
                        <span>{item.label}</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  )
                })}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        ))}
      </SidebarContent>

      <SidebarFooter />
      <SidebarRail />
    </Sidebar>
  )
}
