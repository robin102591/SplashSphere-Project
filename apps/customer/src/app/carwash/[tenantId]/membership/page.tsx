'use client'

import { use, useState } from 'react'
import Link from 'next/link'
import { useTranslations } from 'next-intl'
import {
  Award,
  CheckCircle2,
  Copy,
  Gift,
  History,
  Loader2,
  Lock,
  Medal,
  Share2,
  Sparkles,
  Ticket,
  Users,
  XCircle,
} from 'lucide-react'
import {
  LoyaltyTier,
  PointTransactionType,
  ReferralStatus,
  RewardType,
  type ConnectPointTransactionDto,
  type ConnectReferralCodeDto,
  type ConnectReferralHistoryItemDto,
  type ConnectRewardDto,
  type RedeemRewardResponseDto,
} from '@splashsphere/types'
import { formatDate } from '@splashsphere/format'
import { AppBar } from '@/components/layout/app-bar'
import { useCarwashDetail } from '@/hooks/use-carwash'
import {
  usePointsHistory,
  useLoyaltyCard,
  useRedeemReward,
  useRewards,
} from '@/hooks/use-loyalty'
import {
  useReferralCode,
  useReferralHistory,
} from '@/hooks/use-referral'
import { cn } from '@/lib/utils'

interface MembershipPageProps {
  params: Promise<{ tenantId: string }>
}

/**
 * Loyalty / membership detail page for a single tenant. Aggregates the
 * customer's card, available rewards, recent points history, and their
 * referral code + share stats into one scrollable column.
 *
 * Sections gracefully degrade when the tenant does not offer loyalty:
 * the card renders a "not enrolled" empty state, rewards hide, and the
 * referral section swaps in an explanatory message instead of a code.
 */
export default function MembershipPage({ params }: MembershipPageProps) {
  const { tenantId } = use(params)
  const t = useTranslations('membership')

  const detail = useCarwashDetail(tenantId)
  const card = useLoyaltyCard(tenantId)
  const rewards = useRewards(tenantId)
  const history = usePointsHistory(tenantId, 10)
  const referralCode = useReferralCode(tenantId)
  const referralHistory = useReferralHistory(tenantId)

  const tenantName = detail.data?.tenantName ?? ''

  const [redeeming, setRedeeming] = useState<ConnectRewardDto | null>(null)
  const [lastRedemption, setLastRedemption] =
    useState<RedeemRewardResponseDto | null>(null)

  return (
    <>
      <AppBar title={tenantName || t('title')} backHref="/" />

      <div className="space-y-6 p-4 pb-10">
        {/* ── Membership header ─────────────────────────────────────────── */}
        <MembershipHeader
          tenantName={tenantName}
          card={card.data}
          isLoading={card.isPending || detail.isPending}
        />

        {/* ── Available rewards ─────────────────────────────────────────── */}
        <RewardsSection
          rewards={rewards.data}
          isLoading={rewards.isPending}
          isEnrolled={card.data?.isEnrolled ?? false}
          onRedeem={(reward) => {
            setLastRedemption(null)
            setRedeeming(reward)
          }}
        />

        {/* ── Points history ────────────────────────────────────────────── */}
        <PointsHistorySection
          tenantId={tenantId}
          items={history.data}
          isLoading={history.isPending}
        />

        {/* ── Referral ──────────────────────────────────────────────────── */}
        <ReferralSection
          tenantName={tenantName}
          code={referralCode.data}
          isLoading={referralCode.isPending}
          isError={referralCode.isError}
          history={referralHistory.data}
        />
      </div>

      {redeeming && (
        <RedeemDialog
          tenantId={tenantId}
          reward={redeeming}
          onClose={() => setRedeeming(null)}
          onRedeemed={(res) => {
            setLastRedemption(res)
            setRedeeming(null)
          }}
        />
      )}

      {lastRedemption && (
        <RedemptionSuccess
          result={lastRedemption}
          onClose={() => setLastRedemption(null)}
        />
      )}
    </>
  )
}

// ── Membership header ───────────────────────────────────────────────────────

function MembershipHeader({
  tenantName,
  card,
  isLoading,
}: {
  tenantName: string
  card: ReturnType<typeof useLoyaltyCard>['data']
  isLoading: boolean
}) {
  const t = useTranslations('membership')

  if (isLoading) {
    return (
      <div
        className="h-40 animate-pulse rounded-2xl border border-border bg-muted/60"
        aria-hidden
      />
    )
  }

  if (!card) {
    return null
  }

  if (!card.isEnrolled) {
    return (
      <section className="rounded-2xl border border-border bg-card p-5">
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-muted text-muted-foreground">
            <Lock className="h-5 w-5" aria-hidden />
          </div>
          <div className="min-w-0">
            <h2 className="text-base font-semibold leading-tight">
              {t('notEnrolled.title')}
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              {t('notEnrolled.body')}
            </p>
          </div>
        </div>
      </section>
    )
  }

  const accent = tierAccent(card.currentTier)
  const isTopTier = card.pointsToNextTier === null

  const progressPct = (() => {
    if (isTopTier) return 100
    const earned = card.lifetimePointsEarned
    const toNext = card.pointsToNextTier ?? 0
    const denom = earned + toNext
    if (denom === 0) return 0
    // Clamp to 4% so the bar is visually present even at low earned values.
    return Math.max(4, Math.min(100, Math.round((earned / denom) * 100)))
  })()

  return (
    <section
      className={cn(
        'rounded-2xl border p-5',
        'shadow-sm',
        accent.wrapper,
      )}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="text-xs font-medium uppercase tracking-wide text-primary/80">
            {tenantName || t('title')}
          </p>
          <h2 className="mt-1 flex items-center gap-2 text-lg font-semibold leading-tight">
            <Medal className={cn('h-5 w-5', accent.icon)} aria-hidden />
            {card.tierName}
          </h2>
        </div>
        <div className={cn('rounded-full px-3 py-1 text-xs font-semibold', accent.badge)}>
          {t('tierBadge', { tier: card.tierName })}
        </div>
      </div>

      <div className="mt-4 grid grid-cols-2 gap-3">
        <div>
          <p className="text-xs text-muted-foreground">
            {t('pointsBalance')}
          </p>
          <p className="mt-1 font-mono text-3xl font-semibold tabular-nums leading-none">
            {formatNumber(card.pointsBalance)}
          </p>
          <p className="mt-1 text-xs text-muted-foreground">{t('pointsUnit')}</p>
        </div>
        <div>
          <p className="text-xs text-muted-foreground">{t('lifetime')}</p>
          <p className="mt-1 font-mono text-xl font-semibold tabular-nums leading-none">
            {formatNumber(card.lifetimePointsEarned)}
          </p>
          <p className="mt-1 text-xs text-muted-foreground">{t('pointsUnit')}</p>
        </div>
      </div>

      {!isTopTier && card.nextTierName && (
        <div className="mt-5">
          <div className="flex items-baseline justify-between">
            <p className="text-xs font-medium text-muted-foreground">
              {t('progressTo', { tier: card.nextTierName })}
            </p>
            <p className="font-mono text-xs font-semibold tabular-nums">
              {formatNumber(card.pointsToNextTier ?? 0)} {t('pointsUnit')}
            </p>
          </div>
          <div
            className="mt-2 h-2 w-full overflow-hidden rounded-full bg-primary/10"
            role="progressbar"
            aria-valuemin={0}
            aria-valuemax={100}
            aria-valuenow={progressPct}
          >
            <div
              className={cn('h-full rounded-full transition-all', accent.bar)}
              style={{ width: `${progressPct}%` }}
            />
          </div>
        </div>
      )}

      {isTopTier && (
        <p className="mt-4 flex items-center gap-2 text-xs font-medium text-primary">
          <Sparkles className="h-4 w-4" aria-hidden />
          {t('topTier')}
        </p>
      )}
    </section>
  )
}

function tierAccent(tier: LoyaltyTier) {
  switch (tier) {
    case LoyaltyTier.Platinum:
      return {
        wrapper:
          'border-indigo-200 bg-gradient-to-br from-indigo-50 via-card to-card',
        icon: 'text-indigo-500',
        badge: 'bg-indigo-500 text-white',
        bar: 'bg-indigo-500',
      }
    case LoyaltyTier.Gold:
      return {
        wrapper:
          'border-amber-200 bg-gradient-to-br from-amber-50 via-card to-card',
        icon: 'text-amber-500',
        badge: 'bg-amber-500 text-white',
        bar: 'bg-amber-500',
      }
    case LoyaltyTier.Silver:
      return {
        wrapper:
          'border-slate-200 bg-gradient-to-br from-slate-50 via-card to-card',
        icon: 'text-slate-500',
        badge: 'bg-slate-500 text-white',
        bar: 'bg-slate-500',
      }
    default:
      return {
        wrapper: 'border-border bg-card',
        icon: 'text-primary',
        badge: 'bg-primary text-primary-foreground',
        bar: 'bg-primary',
      }
  }
}

// ── Rewards section ─────────────────────────────────────────────────────────

function RewardsSection({
  rewards,
  isLoading,
  isEnrolled,
  onRedeem,
}: {
  rewards: readonly ConnectRewardDto[] | undefined
  isLoading: boolean
  isEnrolled: boolean
  onRedeem: (reward: ConnectRewardDto) => void
}) {
  const t = useTranslations('membership.rewards')

  if (isLoading) {
    return (
      <section className="space-y-3">
        <h3 className="flex items-center gap-2 text-base font-semibold">
          <Gift className="h-4 w-4 text-primary" aria-hidden />
          {t('title')}
        </h3>
        <div className="grid gap-3 sm:grid-cols-2" aria-hidden>
          <div className="h-28 animate-pulse rounded-2xl border border-border bg-muted/60" />
          <div className="h-28 animate-pulse rounded-2xl border border-border bg-muted/60" />
        </div>
      </section>
    )
  }

  if (!rewards || rewards.length === 0) {
    return (
      <section className="space-y-3">
        <h3 className="flex items-center gap-2 text-base font-semibold">
          <Gift className="h-4 w-4 text-primary" aria-hidden />
          {t('title')}
        </h3>
        <p className="rounded-2xl border border-dashed border-border bg-card p-5 text-center text-sm text-muted-foreground">
          {t('empty')}
        </p>
      </section>
    )
  }

  return (
    <section className="space-y-3">
      <h3 className="flex items-center gap-2 text-base font-semibold">
        <Gift className="h-4 w-4 text-primary" aria-hidden />
        {t('title')}
      </h3>
      <ul className="grid gap-3 sm:grid-cols-2">
        {rewards.map((reward) => (
          <li key={reward.id}>
            <RewardCard
              reward={reward}
              isEnrolled={isEnrolled}
              onRedeem={() => onRedeem(reward)}
            />
          </li>
        ))}
      </ul>
    </section>
  )
}

function RewardCard({
  reward,
  isEnrolled,
  onRedeem,
}: {
  reward: ConnectRewardDto
  isEnrolled: boolean
  onRedeem: () => void
}) {
  const t = useTranslations('membership.rewards')
  const disabled = !isEnrolled || !reward.isAffordable

  return (
    <div className="flex h-full flex-col rounded-2xl border border-border bg-card p-4">
      <div className="flex items-start gap-2">
        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
          {rewardIcon(reward.rewardType)}
        </div>
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-semibold leading-tight">
            {reward.name}
          </p>
          {reward.description && (
            <p className="mt-0.5 line-clamp-2 text-xs text-muted-foreground">
              {reward.description}
            </p>
          )}
        </div>
      </div>

      <div className="mt-3 flex items-baseline gap-1">
        <span className="font-mono text-lg font-semibold tabular-nums">
          {formatNumber(reward.pointsCost)}
        </span>
        <span className="text-xs text-muted-foreground">{t('pointsUnit')}</span>
      </div>

      <button
        type="button"
        onClick={onRedeem}
        disabled={disabled}
        className={cn(
          'mt-3 flex min-h-[40px] w-full items-center justify-center gap-2 rounded-full px-4 text-sm font-semibold transition',
          'active:scale-[0.98]',
          disabled
            ? 'cursor-not-allowed border border-dashed border-border bg-card text-muted-foreground'
            : 'bg-primary text-primary-foreground',
        )}
      >
        {disabled ? (
          <>
            <Lock className="h-3.5 w-3.5" aria-hidden />
            {!isEnrolled ? t('notEnrolled') : t('notAffordable')}
          </>
        ) : (
          t('redeem')
        )}
      </button>
    </div>
  )
}

function rewardIcon(type: RewardType) {
  switch (type) {
    case RewardType.FreeService:
    case RewardType.FreePackage:
      return <Gift className="h-4 w-4" aria-hidden />
    case RewardType.DiscountAmount:
    case RewardType.DiscountPercent:
      return <Ticket className="h-4 w-4" aria-hidden />
    default:
      return <Award className="h-4 w-4" aria-hidden />
  }
}

// ── Points history ──────────────────────────────────────────────────────────

function PointsHistorySection({
  tenantId,
  items,
  isLoading,
}: {
  tenantId: string
  items: readonly ConnectPointTransactionDto[] | undefined
  isLoading: boolean
}) {
  const t = useTranslations('membership.history')

  if (isLoading) {
    return (
      <section className="space-y-3">
        <h3 className="flex items-center gap-2 text-base font-semibold">
          <History className="h-4 w-4 text-primary" aria-hidden />
          {t('title')}
        </h3>
        <div className="space-y-2" aria-hidden>
          <div className="h-14 animate-pulse rounded-xl border border-border bg-muted/60" />
          <div className="h-14 animate-pulse rounded-xl border border-border bg-muted/60" />
          <div className="h-14 animate-pulse rounded-xl border border-border bg-muted/60" />
        </div>
      </section>
    )
  }

  if (!items || items.length === 0) {
    return (
      <section className="space-y-3">
        <h3 className="flex items-center gap-2 text-base font-semibold">
          <History className="h-4 w-4 text-primary" aria-hidden />
          {t('title')}
        </h3>
        <p className="rounded-2xl border border-dashed border-border bg-card p-5 text-center text-sm text-muted-foreground">
          {t('empty')}
        </p>
      </section>
    )
  }

  return (
    <section className="space-y-3">
      <div className="flex items-baseline justify-between">
        <h3 className="flex items-center gap-2 text-base font-semibold">
          <History className="h-4 w-4 text-primary" aria-hidden />
          {t('title')}
        </h3>
        {items.length >= 10 && (
          <Link
            // TODO: /carwash/{tenantId}/membership/history full page — not in MVP.
            href={`/carwash/${tenantId}/membership`}
            className="text-sm font-medium text-primary"
          >
            {t('viewAll')}
          </Link>
        )}
      </div>
      <ul className="space-y-2">
        {items.map((item) => (
          <li key={item.id}>
            <PointRow item={item} />
          </li>
        ))}
      </ul>
    </section>
  )
}

function PointRow({ item }: { item: ConnectPointTransactionDto }) {
  const t = useTranslations('membership.history')
  const isPositive = item.points > 0
  const isExpired = item.type === PointTransactionType.Expired

  return (
    <div className="flex items-center gap-3 rounded-xl border border-border bg-card px-4 py-3">
      <div
        className={cn(
          'flex h-9 w-9 shrink-0 items-center justify-center rounded-xl',
          isPositive
            ? 'bg-primary/10 text-primary'
            : 'bg-muted text-muted-foreground',
        )}
      >
        {isPositive ? (
          <Sparkles className="h-4 w-4" aria-hidden />
        ) : isExpired ? (
          <XCircle className="h-4 w-4" aria-hidden />
        ) : (
          <Ticket className="h-4 w-4" aria-hidden />
        )}
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium">
          {item.description || pointTypeLabel(item.type, t)}
        </p>
        <p className="truncate text-xs text-muted-foreground">
          {formatDate(item.createdAt)}
        </p>
      </div>
      <span
        className={cn(
          'shrink-0 font-mono text-sm font-semibold tabular-nums',
          isPositive ? 'text-primary' : 'text-muted-foreground',
        )}
      >
        {isPositive ? '+' : ''}
        {formatNumber(item.points)}
      </span>
    </div>
  )
}

function pointTypeLabel(
  type: PointTransactionType,
  t: (key: string) => string,
): string {
  switch (type) {
    case PointTransactionType.Earned:
      return t('types.earned')
    case PointTransactionType.Redeemed:
      return t('types.redeemed')
    case PointTransactionType.Expired:
      return t('types.expired')
    case PointTransactionType.Adjustment:
      return t('types.adjustment')
    default:
      return ''
  }
}

// ── Referral section ────────────────────────────────────────────────────────

function ReferralSection({
  tenantName,
  code,
  isLoading,
  isError,
  history,
}: {
  tenantName: string
  code: ConnectReferralCodeDto | undefined
  isLoading: boolean
  isError: boolean
  history: readonly ConnectReferralHistoryItemDto[] | undefined
}) {
  const t = useTranslations('referral')
  const [copied, setCopied] = useState(false)

  if (isLoading) {
    return (
      <section className="space-y-3">
        <h3 className="flex items-center gap-2 text-base font-semibold">
          <Users className="h-4 w-4 text-primary" aria-hidden />
          {t('title')}
        </h3>
        <div
          className="h-48 animate-pulse rounded-2xl border border-border bg-muted/60"
          aria-hidden
        />
      </section>
    )
  }

  if (isError || !code) {
    return (
      <section className="space-y-3">
        <h3 className="flex items-center gap-2 text-base font-semibold">
          <Users className="h-4 w-4 text-primary" aria-hidden />
          {t('title')}
        </h3>
        <p className="rounded-2xl border border-dashed border-border bg-card p-5 text-center text-sm text-muted-foreground">
          {t('unavailable')}
        </p>
      </section>
    )
  }

  const onCopy = async () => {
    try {
      await navigator.clipboard.writeText(code.code)
      setCopied(true)
      window.setTimeout(() => setCopied(false), 1800)
    } catch {
      // Clipboard API blocked (old iOS Safari, etc.) — fail silently; user
      // can still long-press the code itself to copy.
    }
  }

  const shareText = t('shareText', {
    tenant: tenantName || t('defaultTenant'),
    code: code.code,
  })

  const onShare = async () => {
    const shareData: ShareData = {
      title: t('shareTitle'),
      text: shareText,
    }
    try {
      if (typeof navigator !== 'undefined' && navigator.share) {
        await navigator.share(shareData)
        return
      }
    } catch {
      // User cancelled or share failed — fall back to copy.
    }
    onCopy()
  }

  return (
    <section className="space-y-3">
      <h3 className="flex items-center gap-2 text-base font-semibold">
        <Users className="h-4 w-4 text-primary" aria-hidden />
        {t('title')}
      </h3>
      <p className="text-sm text-muted-foreground">{t('subtitle')}</p>

      <div className="rounded-2xl border border-primary/20 bg-primary/5 p-5">
        <p className="text-xs font-medium uppercase tracking-wide text-primary/80">
          {t('yourCode')}
        </p>
        <p className="mt-2 select-all font-mono text-2xl font-semibold tracking-widest text-primary">
          {code.code}
        </p>

        <div className="mt-4 grid grid-cols-2 gap-2">
          <button
            type="button"
            onClick={onCopy}
            className={cn(
              'flex min-h-[44px] items-center justify-center gap-2 rounded-full',
              'border border-primary/30 bg-card px-4 text-sm font-semibold text-primary',
              'transition active:scale-[0.97]',
            )}
          >
            {copied ? (
              <>
                <CheckCircle2 className="h-4 w-4" aria-hidden />
                {t('copied')}
              </>
            ) : (
              <>
                <Copy className="h-4 w-4" aria-hidden />
                {t('copy')}
              </>
            )}
          </button>
          <button
            type="button"
            onClick={onShare}
            className={cn(
              'flex min-h-[44px] items-center justify-center gap-2 rounded-full',
              'bg-primary px-4 text-sm font-semibold text-primary-foreground',
              'transition active:scale-[0.97]',
            )}
          >
            <Share2 className="h-4 w-4" aria-hidden />
            {t('share')}
          </button>
        </div>
      </div>

      {/* How it works */}
      <div className="rounded-2xl border border-border bg-card p-4">
        <p className="text-sm font-semibold">{t('howItWorks.title')}</p>
        <p className="mt-1 text-sm text-muted-foreground">
          {t('howItWorks.body', {
            referrerPoints: code.referrerPointsReward,
            referredPoints: code.referredPointsReward,
          })}
        </p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 gap-2">
        <Stat label={t('stats.total')} value={code.totalReferrals} />
        <Stat label={t('stats.completed')} value={code.completedReferrals} />
        <Stat label={t('stats.pointsEarned')} value={code.pointsEarned} />
      </div>

      {/* Past referrals */}
      {history && history.length > 0 && (
        <div className="space-y-2">
          <h4 className="text-sm font-semibold">{t('pastReferrals')}</h4>
          <ul className="space-y-2">
            {history.map((r) => (
              <li key={r.id}>
                <ReferralRow item={r} />
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}

function Stat({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-border bg-card p-3 text-center">
      <p className="font-mono text-lg font-semibold tabular-nums">
        {formatNumber(value)}
      </p>
      <p className="mt-0.5 text-[11px] uppercase tracking-wide text-muted-foreground">
        {label}
      </p>
    </div>
  )
}

function ReferralRow({ item }: { item: ConnectReferralHistoryItemDto }) {
  const t = useTranslations('referral')
  const statusLabel =
    item.status === ReferralStatus.Completed
      ? t('status.completed')
      : item.status === ReferralStatus.Expired
        ? t('status.expired')
        : t('status.pending')
  const statusClass =
    item.status === ReferralStatus.Completed
      ? 'bg-emerald-100 text-emerald-700'
      : item.status === ReferralStatus.Expired
        ? 'bg-muted text-muted-foreground'
        : 'bg-amber-100 text-amber-700'

  return (
    <div className="flex items-center gap-3 rounded-xl border border-border bg-card px-4 py-3">
      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary">
        <Users className="h-4 w-4" aria-hidden />
      </div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium">
          {item.referredName || t('anonymousName')}
        </p>
        <p className="truncate text-xs text-muted-foreground">
          {formatDate(item.createdAt)}
        </p>
      </div>
      <div className="flex shrink-0 flex-col items-end gap-1">
        <span
          className={cn(
            'rounded-full px-2 py-0.5 text-[11px] font-semibold',
            statusClass,
          )}
        >
          {statusLabel}
        </span>
        {item.status === ReferralStatus.Completed &&
          item.referrerPointsEarned > 0 && (
            <span className="font-mono text-xs font-semibold tabular-nums text-primary">
              +{formatNumber(item.referrerPointsEarned)}
            </span>
          )}
      </div>
    </div>
  )
}

// ── Redeem confirm dialog ───────────────────────────────────────────────────

function RedeemDialog({
  tenantId,
  reward,
  onClose,
  onRedeemed,
}: {
  tenantId: string
  reward: ConnectRewardDto
  onClose: () => void
  onRedeemed: (result: RedeemRewardResponseDto) => void
}) {
  const t = useTranslations('membership.rewards')
  const tCommon = useTranslations('common')
  const redeem = useRedeemReward(tenantId)
  const [error, setError] = useState<string | null>(null)

  const onConfirm = async () => {
    setError(null)
    try {
      const res = await redeem.mutateAsync(reward.id)
      onRedeemed(res)
    } catch {
      setError(t('redeemError'))
    }
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="redeem-dialog-title"
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 px-4 pb-6 sm:items-center"
    >
      <div className="w-full max-w-md rounded-2xl border border-border bg-card p-5 shadow-xl">
        <h3 id="redeem-dialog-title" className="text-base font-semibold">
          {t('confirmTitle')}
        </h3>
        <p className="mt-2 text-sm text-muted-foreground">
          {t('confirmBody', {
            name: reward.name,
            points: formatNumber(reward.pointsCost),
          })}
        </p>

        {error && (
          <p className="mt-3 rounded-xl border border-destructive/30 bg-destructive/5 p-3 text-sm text-destructive">
            {error}
          </p>
        )}

        <div className="mt-5 flex flex-col gap-2">
          <button
            type="button"
            onClick={onConfirm}
            disabled={redeem.isPending}
            className={cn(
              'flex min-h-[48px] items-center justify-center gap-2 rounded-full bg-primary',
              'px-4 py-3 text-sm font-semibold text-primary-foreground',
              'transition active:scale-[0.98] disabled:opacity-60',
            )}
          >
            {redeem.isPending && (
              <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
            )}
            {redeem.isPending ? t('redeeming') : t('confirmConfirm')}
          </button>
          <button
            type="button"
            onClick={onClose}
            disabled={redeem.isPending}
            className={cn(
              'flex min-h-[48px] items-center justify-center rounded-full border border-border',
              'bg-card px-4 py-3 text-sm font-semibold text-foreground',
              'transition active:scale-[0.98] disabled:opacity-60',
            )}
          >
            {tCommon('cancel')}
          </button>
        </div>
      </div>
    </div>
  )
}

// ── Redemption success toast/dialog ─────────────────────────────────────────

function RedemptionSuccess({
  result,
  onClose,
}: {
  result: RedeemRewardResponseDto
  onClose: () => void
}) {
  const t = useTranslations('membership.rewards')
  const tCommon = useTranslations('common')

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="redemption-success-title"
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 px-4 pb-6 sm:items-center"
    >
      <div className="w-full max-w-md rounded-2xl border border-border bg-card p-5 shadow-xl">
        <div className="flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-emerald-100 text-emerald-600">
            <CheckCircle2 className="h-5 w-5" aria-hidden />
          </div>
          <div>
            <h3
              id="redemption-success-title"
              className="text-base font-semibold"
            >
              {t('redeemedTitle')}
            </h3>
            <p className="text-xs text-muted-foreground">{result.rewardName}</p>
          </div>
        </div>

        <div className="mt-4 rounded-xl border border-dashed border-primary/30 bg-primary/5 p-4 text-center">
          <p className="text-xs font-medium uppercase tracking-wide text-primary/80">
            {t('showCashier')}
          </p>
          <p className="mt-2 select-all font-mono text-lg font-semibold tracking-wider text-primary">
            {result.pointTransactionId}
          </p>
        </div>

        <p className="mt-3 text-center text-xs text-muted-foreground">
          {t('newBalance', { balance: formatNumber(result.newBalance) })}
        </p>

        <button
          type="button"
          onClick={onClose}
          className={cn(
            'mt-5 flex min-h-[48px] w-full items-center justify-center rounded-full bg-primary',
            'px-4 py-3 text-sm font-semibold text-primary-foreground',
            'transition active:scale-[0.98]',
          )}
        >
          {tCommon('close')}
        </button>
      </div>
    </div>
  )
}

// ── Utilities ───────────────────────────────────────────────────────────────

function formatNumber(n: number): string {
  return new Intl.NumberFormat('en-PH').format(n)
}

