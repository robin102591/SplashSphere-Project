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
  CalendarCheck,
  Lock,
} from 'lucide-react'
import { useHasFeature } from '@/hooks/use-plan'
import { FeatureKeys } from '@splashsphere/types'

interface NavItem {
  label: string
  href: string
  icon: React.ElementType
  feature?: string | null
}

const navGroups: { label: string; items: NavItem[] }[] = [
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
      { label: 'Pricing Rules', href: '/dashboard/pricing-modifiers', icon: Percent, feature: FeatureKeys.PricingModifiers },
    ],
  },
  {
    label: 'People',
    items: [
      { label: 'Employees', href: '/dashboard/employees', icon: Users },
      { label: 'Attendance', href: '/dashboard/attendance', icon: CalendarCheck },
      { label: 'Customers', href: '/dashboard/customers', icon: Users },
      { label: 'Vehicles', href: '/dashboard/vehicles', icon: Car },
    ],
  },
  {
    label: 'Finance',
    items: [
      { label: 'Transactions', href: '/dashboard/transactions', icon: CreditCard },
      { label: 'Payroll', href: '/dashboard/payroll', icon: CreditCard },
      { label: 'Cash Advances', href: '/dashboard/cash-advances', icon: Banknote, feature: FeatureKeys.CashAdvanceTracking },
      { label: 'Shifts', href: '/dashboard/shifts', icon: Wallet, feature: FeatureKeys.ShiftManagement },
      { label: 'Shift Variance', href: '/dashboard/reports/shift-variance', icon: TrendingDown, feature: FeatureKeys.ShiftManagement },
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

function NavItemRow({ item, pathname }: { item: NavItem; pathname: string }) {
  const hasFeature = useHasFeature(item.feature ?? '')
  const locked = !!item.feature && !hasFeature

  const active =
    pathname === item.href ||
    (item.href !== '/dashboard' && pathname.startsWith(item.href))

  return (
    <SidebarMenuItem>
      <SidebarMenuButton
        render={locked ? <span /> : <Link href={item.href} />}
        isActive={active}
        tooltip={locked ? `${item.label} (upgrade required)` : item.label}
        className={
          locked
            ? 'opacity-50 cursor-not-allowed'
            : active
              ? 'bg-splash-50 text-splash-700 border-l-2 border-splash-500 hover:bg-splash-100'
              : ''
        }
      >
        <item.icon className="h-4 w-4" />
        <span className="flex-1">{item.label}</span>
        {locked && <Lock className="h-3 w-3 text-muted-foreground" />}
      </SidebarMenuButton>
    </SidebarMenuItem>
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
                {group.items.map((item) => (
                  <NavItemRow key={item.href} item={item} pathname={pathname} />
                ))}
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
