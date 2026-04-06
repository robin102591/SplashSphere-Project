using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Loyalty.Queries.GetLoyaltySettings;

public sealed class GetLoyaltySettingsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetLoyaltySettingsQuery, LoyaltyProgramSettingsDto?>
{
    public async Task<LoyaltyProgramSettingsDto?> Handle(
        GetLoyaltySettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await context.LoyaltyProgramSettings
            .AsNoTracking()
            .Include(s => s.Tiers)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
            return null;

        return new LoyaltyProgramSettingsDto(
            settings.Id,
            settings.PointsPerCurrencyUnit,
            settings.CurrencyUnitAmount,
            settings.IsActive,
            settings.PointsExpirationMonths,
            settings.AutoEnroll,
            settings.Tiers
                .OrderBy(t => t.MinimumLifetimePoints)
                .Select(t => new LoyaltyTierConfigDto(
                    t.Id, t.Tier, t.Name, t.MinimumLifetimePoints, t.PointsMultiplier))
                .ToList());
    }
}
