using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;

namespace SplashSphere.Application.Features.Connect.Loyalty.Queries.GetMyLoyalty;

public sealed class GetMyLoyaltyQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser,
    IPlanEnforcementService planService)
    : IRequestHandler<GetMyLoyaltyQuery, ConnectMembershipDto>
{
    public async Task<ConnectMembershipDto> Handle(
        GetMyLoyaltyQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
            return NotEnrolled();

        // ── Feature gate: tenant must have CustomerLoyalty ───────────────────
        var hasFeature = await planService.HasFeatureAsync(
            request.TenantId, FeatureKeys.CustomerLoyalty, cancellationToken);
        if (!hasFeature)
            return NotEnrolled();

        var userId = connectUser.ConnectUserId;

        // Resolve customer via tenant link.
        var customerId = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.ConnectUserId == userId
                     && l.TenantId == request.TenantId
                     && l.IsActive)
            .Select(l => l.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId is null)
            return NotEnrolled();

        var card = await db.MembershipCards
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(m => m.TenantId == request.TenantId && m.CustomerId == customerId)
            .Select(m => new
            {
                m.Id,
                m.CardNumber,
                m.CurrentTier,
                m.PointsBalance,
                m.LifetimePointsEarned,
                m.LifetimePointsRedeemed,
                m.IsActive,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (card is null || !card.IsActive)
            return NotEnrolled();

        // Tier configs for this tenant (ordered to find current + next).
        var tiers = await db.LoyaltyTierConfigs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.TenantId == request.TenantId)
            .OrderBy(t => t.MinimumLifetimePoints)
            .Select(t => new { t.Tier, t.Name, t.MinimumLifetimePoints, t.PointsMultiplier })
            .ToListAsync(cancellationToken);

        var currentCfg = tiers
            .LastOrDefault(t => t.MinimumLifetimePoints <= card.LifetimePointsEarned);

        var nextCfg = tiers
            .FirstOrDefault(t => t.MinimumLifetimePoints > card.LifetimePointsEarned);

        int? pointsToNext = nextCfg is null
            ? null
            : nextCfg.MinimumLifetimePoints - card.LifetimePointsEarned;

        return new ConnectMembershipDto(
            IsEnrolled: true,
            MembershipCardId: card.Id,
            CardNumber: card.CardNumber,
            CurrentTier: card.CurrentTier,
            TierName: currentCfg?.Name ?? card.CurrentTier.ToString(),
            PointsBalance: card.PointsBalance,
            LifetimePointsEarned: card.LifetimePointsEarned,
            LifetimePointsRedeemed: card.LifetimePointsRedeemed,
            PointsToNextTier: pointsToNext,
            NextTierName: nextCfg?.Name,
            TierMultiplier: currentCfg?.PointsMultiplier ?? 1m);
    }

    private static ConnectMembershipDto NotEnrolled() => new(
        IsEnrolled: false,
        MembershipCardId: null,
        CardNumber: null,
        CurrentTier: LoyaltyTier.Standard,
        TierName: LoyaltyTier.Standard.ToString(),
        PointsBalance: 0,
        LifetimePointsEarned: 0,
        LifetimePointsRedeemed: 0,
        PointsToNextTier: null,
        NextTierName: null,
        TierMultiplier: 1m);
}
