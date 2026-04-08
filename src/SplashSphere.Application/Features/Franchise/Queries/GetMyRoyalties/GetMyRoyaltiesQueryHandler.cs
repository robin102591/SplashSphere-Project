using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Queries.GetMyRoyalties;

public sealed class GetMyRoyaltiesQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetMyRoyaltiesQuery, PagedResult<RoyaltyPeriodDto>>
{
    public async Task<PagedResult<RoyaltyPeriodDto>> Handle(
        GetMyRoyaltiesQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(rp => rp.FranchiseeTenantId == tenantContext.TenantId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(rp => rp.PeriodStart)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(
                db.Tenants.IgnoreQueryFilters().AsNoTracking(),
                rp => rp.FranchisorTenantId,
                t => t.Id,
                (rp, t) => new RoyaltyPeriodDto(
                    rp.Id,
                    rp.FranchiseeTenantId,
                    t.Name,
                    rp.AgreementId,
                    rp.PeriodStart,
                    rp.PeriodEnd,
                    rp.GrossRevenue,
                    rp.RoyaltyRate,
                    rp.RoyaltyAmount,
                    rp.MarketingFeeRate,
                    rp.MarketingFeeAmount,
                    rp.TechnologyFeeRate,
                    rp.TechnologyFeeAmount,
                    rp.TotalDue,
                    rp.Status,
                    rp.PaidDate,
                    rp.PaymentReference))
            .ToListAsync(cancellationToken);

        return PagedResult<RoyaltyPeriodDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
