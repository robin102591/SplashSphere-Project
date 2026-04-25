'use client'

import { useCallback, useMemo } from 'react'
import { usePathname, useRouter, useSearchParams } from 'next/navigation'

/**
 * Read/write a single URL query parameter that names the active in-page
 * section (e.g., `?section=transactions`). Used by `SectionNav` to drive
 * the highlighted item and by pages to render the matching content panel.
 *
 * Writes use `router.replace` so each section change does not push a new
 * history entry — the user's browser Back returns to the previous page,
 * not to every section they viewed.
 */
export function useSectionParam(
  paramKey = 'section',
  fallback?: string,
): [string, (next: string) => void] {
  const router = useRouter()
  const pathname = usePathname()
  const searchParams = useSearchParams()

  const current = useMemo(() => {
    const value = searchParams.get(paramKey)
    return value && value.length > 0 ? value : (fallback ?? '')
  }, [searchParams, paramKey, fallback])

  const setSection = useCallback(
    (next: string) => {
      const params = new URLSearchParams(searchParams.toString())
      if (next && next !== fallback) {
        params.set(paramKey, next)
      } else {
        params.delete(paramKey)
      }
      const qs = params.toString()
      router.replace(qs ? `${pathname}?${qs}` : pathname, { scroll: false })
    },
    [router, pathname, searchParams, paramKey, fallback],
  )

  return [current, setSection]
}
