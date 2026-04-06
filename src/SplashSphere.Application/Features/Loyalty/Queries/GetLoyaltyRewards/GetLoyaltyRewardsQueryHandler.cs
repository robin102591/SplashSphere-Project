using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltyRewards;

public sealed class GetLoyaltyRewardsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetLoyaltyRewardsQuery, PagedResult<LoyaltyRewardDto>>
{
    public async Task<PagedResult<LoyaltyRewardDto>> Handle(
        GetLoyaltyRewardsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.LoyaltyRewards.AsNoTracking().AsQueryable();

        if (request.ActiveOnly == true)
            query = query.Where(r => r.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(r => r.PointsCost)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new LoyaltyRewardDto(
                r.Id,
                r.Name,
                r.Description,
                r.RewardType,
                r.PointsCost,
                r.ServiceId,
                r.Service != null ? r.Service.Name : null,
                r.PackageId,
                r.Package != null ? r.Package.Name : null,
                r.DiscountAmount,
                r.DiscountPercent,
                r.IsActive,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<LoyaltyRewardDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
