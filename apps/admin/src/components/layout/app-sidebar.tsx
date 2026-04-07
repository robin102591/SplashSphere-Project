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
  ScrollText,
  Crown,
  Receipt,
  Coins,
  PieChart,
  Award,
} from 'lucide-react'
import { useTranslations } from 'next-intl'
import { useHasFeature } from '@/hooks/use-plan'
import { FeatureKeys } from '@splashsphere/types'
import { PwaInstallPrompt } from '@/components/pwa-install-prompt'

interface NavItem {
  labelKey: string
  href: string
  icon: React.ElementType
  feature?: string | null
}

const navGroups: { labelKey: string; items: NavItem[] }[] = [
  {
    labelKey: 'overview',
    items: [
      { labelKey: 'dashboard', href: '/dashboard', icon: LayoutDashboard },
    ],
  },
  {
    labelKey: 'operations',
    items: [
      { labelKey: 'branches', href: '/dashboard/branches', icon: GitBranch },
      { labelKey: 'services', href: '/dashboard/services', icon: Wrench },
      { labelKey: 'packages', href: '/dashboard/packages', icon: Package },
      { labelKey: 'merchandise', href: '/dashboard/merchandise', icon: ShoppingBag },
      { labelKey: 'pricingRules', href: '/dashboard/pricing-modifiers', icon: Percent, feature: FeatureKeys.PricingModifiers },
    ],
  },
  {
    labelKey: 'people',
    items: [
      { labelKey: 'employees', href: '/dashboard/employees', icon: Users },
      { labelKey: 'attendance', href: '/dashboard/attendance', icon: CalendarCheck },
      { labelKey: 'customers', href: '/dashboard/customers', icon: Users },
      { labelKey: 'loyalty', href: '/dashboard/loyalty', icon: Award, feature: FeatureKeys.CustomerLoyalty },
      { labelKey: 'vehicles', href: '/dashboard/vehicles', icon: Car },
    ],
  },
  {
    labelKey: 'finance',
    items: [
      { labelKey: 'transactions', href: '/dashboard/transactions', icon: CreditCard },
      { labelKey: 'payroll', href: '/dashboard/payroll', icon: CreditCard },
      { labelKey: 'cashAdvances', href: '/dashboard/cash-advances', icon: Banknote, feature: FeatureKeys.CashAdvanceTracking },
      { labelKey: 'expenses', href: '/dashboard/expenses', icon: Coins, feature: FeatureKeys.ExpenseTracking },
      { labelKey: 'shifts', href: '/dashboard/shifts', icon: Wallet, feature: FeatureKeys.ShiftManagement },
      { labelKey: 'shiftVariance', href: '/dashboard/reports/shift-variance', icon: TrendingDown, feature: FeatureKeys.ShiftManagement },
      { labelKey: 'reports', href: '/dashboard/reports', icon: BarChart3 },
      { labelKey: 'profitLoss', href: '/dashboard/reports/profit-loss', icon: PieChart, feature: FeatureKeys.ProfitLossReports },
    ],
  },
  {
    labelKey: 'account',
    items: [
      { labelKey: 'subscription', href: '/dashboard/subscription', icon: Crown },
      { labelKey: 'billing', href: '/dashboard/billing', icon: Receipt },
    ],
  },
  {
    labelKey: 'configuration',
    items: [
      { labelKey: 'settings', href: '/dashboard/settings', icon: Settings },
      { labelKey: 'auditLogs', href: '/dashboard/audit-logs', icon: ScrollText },
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

function NavItemRow({ item, pathname, t }: { item: NavItem; pathname: string; t: (key: string) => string }) {
  const hasFeature = useHasFeature(item.feature ?? '')
  const locked = !!item.feature && !hasFeature
  const label = t(item.labelKey)

  const active =
    pathname === item.href ||
    (item.href !== '/dashboard' && pathname.startsWith(item.href))

  return (
    <SidebarMenuItem>
      <SidebarMenuButton
        render={locked ? <span /> : <Link href={item.href} />}
        isActive={active}
        tooltip={locked ? `${label} (upgrade required)` : label}
        className={
          locked
            ? 'opacity-50 cursor-not-allowed'
            : active
              ? 'bg-splash-50 text-splash-700 border-l-2 border-splash-500 hover:bg-splash-100'
              : ''
        }
      >
        <item.icon className="h-4 w-4" />
        <span className="flex-1">{label}</span>
        {locked && <Lock className="h-3 w-3 text-muted-foreground" />}
      </SidebarMenuButton>
    </SidebarMenuItem>
  )
}

export function AppSidebar() {
  const pathname = usePathname()
  const t = useTranslations('nav')

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader>
        <SidebarLogo />
      </SidebarHeader>

      <SidebarContent>
        {navGroups.map((group) => (
          <SidebarGroup key={group.labelKey}>
            <SidebarGroupLabel>{t(group.labelKey)}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {group.items.map((item) => (
                  <NavItemRow key={item.href} item={item} pathname={pathname} t={t} />
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        ))}
      </SidebarContent>

      <SidebarFooter>
        <PwaInstallPrompt />
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  )
}
