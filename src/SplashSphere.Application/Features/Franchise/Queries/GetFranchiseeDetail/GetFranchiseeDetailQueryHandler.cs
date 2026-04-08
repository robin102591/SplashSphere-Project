using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Exceptions;

namespace SplashSphere.Application.Features.Franchise.Queries.GetFranchiseeDetail;

public sealed class GetFranchiseeDetailQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetFranchiseeDetailQuery, FranchiseeDetailDto>
{
    public async Task<FranchiseeDetailDto> Handle(
        GetFranchiseeDetailQuery request,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(t => t.Branches)
            .Where(t => t.Id == request.FranchiseeTenantId
                        && t.ParentTenantId == tenantContext.TenantId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                $"Franchisee tenant '{request.FranchiseeTenantId}' was not found.");

        var agreement = await db.FranchiseAgreements
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.FranchiseeTenantId == request.FranchiseeTenantId
                        && a.FranchisorTenantId == tenantContext.TenantId)
            .Select(a => new FranchiseAgreementDto(
                a.Id,
                a.FranchisorTenantId,
                a.FranchiseeTenantId,
                a.AgreementNumber,
                a.TerritoryName,
                a.TerritoryDescription,
                a.ExclusiveTerritory,
                a.StartDate,
                a.EndDate,
                a.InitialFranchiseFee,
                a.Status,
                a.CustomRoyaltyRate,
                a.CustomMarketingFeeRate,
                a.Notes,
                a.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        var recentRoyalties = await db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(rp => rp.FranchiseeTenantId == request.FranchiseeTenantId
                         && rp.FranchisorTenantId == tenantContext.TenantId)
            .OrderByDescending(rp => rp.PeriodStart)
            .Take(6)
            .Select(rp => new RoyaltyPeriodDto(
                rp.Id,
                rp.FranchiseeTenantId,
                tenant.Name,
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

        return new FranchiseeDetailDto(
            tenant.Id,
            tenant.Name,
            tenant.FranchiseCode,
            tenant.Email,
            tenant.ContactNumber,
            tenant.Address,
            agreement?.TerritoryName ?? string.Empty,
            tenant.Branches.Count,
            tenant.IsActive,
            agreement?.Status ?? Domain.Enums.AgreementStatus.Draft,
            0m, // RevenueThisMonth — will come from royalty periods
            0m, // RoyaltyDue — will come from royalty periods
            agreement,
            recentRoyalties);
    }
}
