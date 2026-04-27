using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Subscription;

namespace SplashSphere.Application.Features.Connect.Loyalty.Queries.GetRewards;

public sealed class GetRewardsQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser,
    IPlanEnforcementService planService)
    : IRequestHandler<GetRewardsQuery, IReadOnlyList<ConnectRewardDto>>
{
    public async Task<IReadOnlyList<ConnectRewardDto>> Handle(
        GetRewardsQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated) return [];

        var hasFeature = await planService.HasFeatureAsync(
            request.TenantId, FeatureKeys.CustomerLoyalty, cancellationToken);
        if (!hasFeature) return [];

        var userId = connectUser.ConnectUserId;

        // Find caller's point balance at this tenant (null if not enrolled yet
        // — still show rewards, just mark none as affordable).
        var balance = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.ConnectUserId == userId
                     && l.TenantId == request.TenantId
                     && l.IsActive)
            .Join(
                db.MembershipCards.IgnoreQueryFilters()
                    .Where(m => m.TenantId == request.TenantId && m.IsActive),
                l => l.CustomerId,
                m => m.CustomerId,
                (l, m) => (int?)m.PointsBalance)
            .FirstOrDefaultAsync(cancellationToken) ?? 0;

        var rewards = await db.LoyaltyRewards
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.TenantId == request.TenantId && r.IsActive)
            .OrderBy(r => r.PointsCost)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                r.RewardType,
                r.PointsCost,
                r.ServiceId,
                ServiceName = r.Service != null ? r.Service.Name : null,
                r.PackageId,
                PackageName = r.Package != null ? r.Package.Name : null,
                r.DiscountAmount,
                r.DiscountPercent,
            })
            .ToListAsync(cancellationToken);

        return rewards
            .Select(r => new ConnectRewardDto(
                r.Id,
                r.Name,
                r.Description,
                r.RewardType,
                r.PointsCost,
                r.ServiceId,
                r.ServiceName,
                r.PackageId,
                r.PackageName,
                r.DiscountAmount,
                r.DiscountPercent,
                IsAffordable: balance >= r.PointsCost))
            .ToList();
    }
}
