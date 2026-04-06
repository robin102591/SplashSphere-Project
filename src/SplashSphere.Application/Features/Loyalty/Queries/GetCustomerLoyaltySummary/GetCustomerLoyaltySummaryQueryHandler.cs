using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetCustomerLoyaltySummary;

public sealed class GetCustomerLoyaltySummaryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomerLoyaltySummaryQuery, CustomerLoyaltySummaryDto?>
{
    public async Task<CustomerLoyaltySummaryDto?> Handle(
        GetCustomerLoyaltySummaryQuery request,
        CancellationToken cancellationToken)
    {
        var card = await context.MembershipCards
            .AsNoTracking()
            .Where(m => m.CustomerId == request.CustomerId && m.IsActive)
            .Select(m => new
            {
                m.Id,
                m.CardNumber,
                m.CurrentTier,
                m.PointsBalance,
                m.LifetimePointsEarned,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (card is null)
            return null;

        // Load tier configs to determine next tier
        var tiers = await context.LoyaltyTierConfigs
            .AsNoTracking()
            .OrderBy(t => t.MinimumLifetimePoints)
            .Select(t => new { t.Tier, t.Name, t.MinimumLifetimePoints })
            .ToListAsync(cancellationToken);

        int? pointsToNextTier = null;
        string? nextTierName = null;

        var nextTier = tiers
            .FirstOrDefault(t => t.MinimumLifetimePoints > card.LifetimePointsEarned);

        if (nextTier is not null)
        {
            pointsToNextTier = nextTier.MinimumLifetimePoints - card.LifetimePointsEarned;
            nextTierName = nextTier.Name;
        }

        // Available rewards the customer can afford
        var availableRewards = await context.LoyaltyRewards
            .AsNoTracking()
            .Where(r => r.IsActive && r.PointsCost <= card.PointsBalance)
            .OrderBy(r => r.PointsCost)
            .Select(r => new AvailableRewardDto(
                r.Id, r.Name, r.RewardType, r.PointsCost,
                r.DiscountAmount, r.DiscountPercent))
            .ToListAsync(cancellationToken);

        var currentTierName = tiers
            .LastOrDefault(t => t.MinimumLifetimePoints <= card.LifetimePointsEarned)?.Name
            ?? card.CurrentTier.ToString();

        return new CustomerLoyaltySummaryDto(
            card.Id,
            card.CardNumber,
            card.CurrentTier,
            currentTierName,
            card.PointsBalance,
            card.LifetimePointsEarned,
            pointsToNextTier,
            nextTierName,
            availableRewards);
    }
}
