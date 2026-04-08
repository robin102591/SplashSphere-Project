using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Queries.GetRoyaltyPeriods;

public sealed class GetRoyaltyPeriodsQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetRoyaltyPeriodsQuery, PagedResult<RoyaltyPeriodDto>>
{
    public async Task<PagedResult<RoyaltyPeriodDto>> Handle(
        GetRoyaltyPeriodsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(rp => rp.FranchisorTenantId == tenantContext.TenantId);

        if (!string.IsNullOrWhiteSpace(request.FranchiseeTenantId))
            query = query.Where(rp => rp.FranchiseeTenantId == request.FranchiseeTenantId);

        if (request.Status.HasValue)
            query = query.Where(rp => rp.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(rp => rp.PeriodStart)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(
                db.Tenants.IgnoreQueryFilters().AsNoTracking(),
                rp => rp.FranchiseeTenantId,
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
