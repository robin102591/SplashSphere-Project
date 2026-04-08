using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Queries.GetFranchiseSettings;

public sealed class GetFranchiseSettingsQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetFranchiseSettingsQuery, FranchiseSettingsDto?>
{
    public async Task<FranchiseSettingsDto?> Handle(
        GetFranchiseSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await db.FranchiseSettings
            .AsNoTracking()
            .Where(s => s.TenantId == tenantContext.TenantId)
            .Select(s => new FranchiseSettingsDto(
                s.Id,
                s.TenantId,
                s.RoyaltyRate,
                s.MarketingFeeRate,
                s.TechnologyFeeRate,
                s.RoyaltyBasis,
                s.RoyaltyFrequency,
                s.EnforceStandardServices,
                s.EnforceStandardPricing,
                s.AllowLocalServices,
                s.MaxPriceVariance,
                s.EnforceBranding,
                s.DefaultFranchiseePlan,
                s.MaxBranchesPerFranchisee))
            .FirstOrDefaultAsync(cancellationToken);

        return settings;
    }
}
