using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Commands.UpsertLoyaltyTiers;

public sealed class UpsertLoyaltyTiersCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<UpsertLoyaltyTiersCommand, Result>
{
    public async Task<Result> Handle(
        UpsertLoyaltyTiersCommand request,
        CancellationToken cancellationToken)
    {
        var settings = await context.LoyaltyProgramSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
            return Result.Failure(Error.Validation("Configure loyalty program settings first."));

        // Remove existing tiers and replace atomically
        var existingTiers = await context.LoyaltyTierConfigs
            .Where(t => t.LoyaltyProgramSettingsId == settings.Id)
            .ToListAsync(cancellationToken);

        context.LoyaltyTierConfigs.RemoveRange(existingTiers);

        foreach (var tier in request.Tiers)
        {
            context.LoyaltyTierConfigs.Add(new LoyaltyTierConfig(
                tenantContext.TenantId,
                settings.Id,
                tier.Tier,
                tier.Name,
                tier.MinimumLifetimePoints,
                tier.PointsMultiplier));
        }

        return Result.Success();
    }
}
