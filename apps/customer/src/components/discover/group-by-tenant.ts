import type { ConnectDiscoveryResultDto } from '@splashsphere/types'

/**
 * Collapse the branch-level rows returned by the discovery API into one entry
 * per tenant. The first row for a tenant wins (the server already sorted by
 * distance when coords were supplied), and we count the remaining branches
 * for the "+N more" label.
 */
export interface TenantGroup {
  primary: ConnectDiscoveryResultDto
  extraBranchCount: number
}

export function groupByTenant(
  rows: readonly ConnectDiscoveryResultDto[],
): TenantGroup[] {
  const byTenant = new Map<string, TenantGroup>()
  for (const row of rows) {
    const existing = byTenant.get(row.tenantId)
    if (existing) {
      existing.extraBranchCount += 1
    } else {
      byTenant.set(row.tenantId, { primary: row, extraBranchCount: 0 })
    }
  }
  return Array.from(byTenant.values())
}
