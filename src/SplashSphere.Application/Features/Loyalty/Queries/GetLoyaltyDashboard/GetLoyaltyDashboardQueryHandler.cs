using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltyDashboard;

public sealed class GetLoyaltyDashboardQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetLoyaltyDashboardQuery, LoyaltyDashboardDto>
{
    public async Task<LoyaltyDashboardDto> Handle(
        GetLoyaltyDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var totalMembers = await context.MembershipCards
            .CountAsync(cancellationToken);

        // Tier distribution
        var tierGroups = await context.MembershipCards
            .AsNoTracking()
            .GroupBy(m => m.CurrentTier)
            .Select(g => new { Tier = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var tierDistribution = tierGroups
            .Select(g => new TierDistributionDto(g.Tier, g.Tier.ToString(), g.Count))
            .OrderBy(t => t.Tier)
            .ToList();

        // Points earned/redeemed in period
        var from = DateTime.SpecifyKind(request.From, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(request.To, DateTimeKind.Utc);

        var periodPoints = await context.PointTransactions
            .AsNoTracking()
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .GroupBy(p => p.Type)
            .Select(g => new { Type = g.Key, Total = g.Sum(p => Math.Abs(p.Points)), Count = g.Count() })
            .ToListAsync(cancellationToken);

        var earnedInPeriod = periodPoints
            .Where(p => p.Type == PointTransactionType.Earned)
            .Sum(p => p.Total);

        var redeemedInPeriod = periodPoints
            .Where(p => p.Type == PointTransactionType.Redeemed)
            .Sum(p => p.Total);

        var redemptionCount = periodPoints
            .Where(p => p.Type == PointTransactionType.Redeemed)
            .Sum(p => p.Count);

        // Top 10 customers by lifetime points
        var topCustomers = await context.MembershipCards
            .AsNoTracking()
            .OrderByDescending(m => m.LifetimePointsEarned)
            .Take(10)
            .Select(m => new TopLoyalCustomerDto(
                m.CustomerId,
                m.Customer.FirstName + " " + m.Customer.LastName,
                m.CardNumber,
                m.CurrentTier,
                m.LifetimePointsEarned,
                m.PointsBalance))
            .ToListAsync(cancellationToken);

        return new LoyaltyDashboardDto(
            totalMembers,
            earnedInPeriod,
            redeemedInPeriod,
            redemptionCount,
            tierDistribution,
            topCustomers);
    }
}
