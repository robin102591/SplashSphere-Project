using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Loyalty.Commands.UpsertLoyaltySettings;

public sealed class UpsertLoyaltySettingsCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenantContext)
    : IRequestHandler<UpsertLoyaltySettingsCommand, Result>
{
    public async Task<Result> Handle(
        UpsertLoyaltySettingsCommand request,
        CancellationToken cancellationToken)
    {
        var settings = await context.LoyaltyProgramSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            settings = new LoyaltyProgramSettings(tenantContext.TenantId);
            context.LoyaltyProgramSettings.Add(settings);
        }

        settings.PointsPerCurrencyUnit = request.PointsPerCurrencyUnit;
        settings.CurrencyUnitAmount = request.CurrencyUnitAmount;
        settings.IsActive = request.IsActive;
        settings.PointsExpirationMonths = request.PointsExpirationMonths;
        settings.AutoEnroll = request.AutoEnroll;

        return Result.Success();
    }
}
