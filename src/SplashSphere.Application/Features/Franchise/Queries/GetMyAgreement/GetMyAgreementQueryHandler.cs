using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Queries.GetMyAgreement;

public sealed class GetMyAgreementQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetMyAgreementQuery, FranchiseAgreementDto?>
{
    public async Task<FranchiseAgreementDto?> Handle(
        GetMyAgreementQuery request,
        CancellationToken cancellationToken)
    {
        var agreement = await db.FranchiseAgreements
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.FranchiseeTenantId == tenantContext.TenantId)
            .OrderByDescending(a => a.CreatedAt)
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

        return agreement;
    }
}
