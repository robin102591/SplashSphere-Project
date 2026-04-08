using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.UpdateFranchiseSettings;

public sealed class UpdateFranchiseSettingsCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<UpdateFranchiseSettingsCommand, Result>
{
    public async Task<Result> Handle(
        UpdateFranchiseSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant", tenantContext.TenantId));

        if (tenant.TenantType != TenantType.Franchisor)
            return Result.Failure(Error.Forbidden("Only Franchisor tenants can manage franchise settings."));

        var settings = await db.FranchiseSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            settings = new FranchiseSettings(tenantContext.TenantId);
            db.FranchiseSettings.Add(settings);
        }

        settings.RoyaltyRate = request.RoyaltyRate;
        settings.MarketingFeeRate = request.MarketingFeeRate;
        settings.TechnologyFeeRate = request.TechnologyFeeRate;
        settings.RoyaltyBasis = request.RoyaltyBasis;
        settings.RoyaltyFrequency = request.RoyaltyFrequency;
        settings.EnforceStandardServices = request.EnforceStandardServices;
        settings.EnforceStandardPricing = request.EnforceStandardPricing;
        settings.AllowLocalServices = request.AllowLocalServices;
        settings.MaxPriceVariance = request.MaxPriceVariance;
        settings.EnforceBranding = request.EnforceBranding;
        settings.DefaultFranchiseePlan = request.DefaultFranchiseePlan;
        settings.MaxBranchesPerFranchisee = request.MaxBranchesPerFranchisee;

        return Result.Success();
    }
}
