using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Franchise.Commands.CalculateRoyalties;

public sealed class CalculateRoyaltiesCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<CalculateRoyaltiesCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        CalculateRoyaltiesCommand request,
        CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantContext.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<string>(Error.NotFound("Tenant", tenantContext.TenantId));

        if (tenant.TenantType != TenantType.Franchisor)
            return Result.Failure<string>(Error.Forbidden("Only Franchisor tenants can calculate royalties."));

        // Load the franchise agreement
        var agreement = await db.FranchiseAgreements
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                a => a.FranchisorTenantId == tenantContext.TenantId
                     && a.FranchiseeTenantId == request.FranchiseeTenantId
                     && a.Status == AgreementStatus.Active,
                cancellationToken);

        if (agreement is null)
            return Result.Failure<string>(Error.NotFound("Active FranchiseAgreement", request.FranchiseeTenantId));

        // Load franchise settings
        var settings = await db.FranchiseSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
            return Result.Failure<string>(Error.NotFound("FranchiseSettings", tenantContext.TenantId));

        // Check for duplicate royalty period
        var duplicateExists = await db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .AnyAsync(
                rp => rp.FranchisorTenantId == tenantContext.TenantId
                      && rp.FranchiseeTenantId == request.FranchiseeTenantId
                      && rp.PeriodStart == request.PeriodStart
                      && rp.PeriodEnd == request.PeriodEnd,
                cancellationToken);

        if (duplicateExists)
            return Result.Failure<string>(Error.Conflict("A royalty period already exists for this date range."));

        // Query franchisee's completed transactions in the period
        var grossRevenue = await db.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.TenantId == request.FranchiseeTenantId
                        && t.Status == TransactionStatus.Completed
                        && t.CompletedAt != null
                        && t.CompletedAt >= request.PeriodStart
                        && t.CompletedAt <= request.PeriodEnd)
            .SumAsync(t => t.FinalAmount, cancellationToken);

        // Calculate fees — agreement overrides take precedence over settings
        var royaltyRate = agreement.CustomRoyaltyRate ?? settings.RoyaltyRate;
        var marketingFeeRate = agreement.CustomMarketingFeeRate ?? settings.MarketingFeeRate;
        var technologyFeeRate = settings.TechnologyFeeRate;

        var royaltyAmount = Math.Round(grossRevenue * royaltyRate, 2, MidpointRounding.AwayFromZero);
        var marketingFeeAmount = Math.Round(grossRevenue * marketingFeeRate, 2, MidpointRounding.AwayFromZero);
        var technologyFeeAmount = Math.Round(grossRevenue * technologyFeeRate, 2, MidpointRounding.AwayFromZero);
        var totalDue = royaltyAmount + marketingFeeAmount + technologyFeeAmount;

        var royaltyPeriod = new RoyaltyPeriod(tenantContext.TenantId, request.FranchiseeTenantId, agreement.Id)
        {
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            GrossRevenue = grossRevenue,
            RoyaltyRate = royaltyRate,
            RoyaltyAmount = royaltyAmount,
            MarketingFeeRate = marketingFeeRate,
            MarketingFeeAmount = marketingFeeAmount,
            TechnologyFeeRate = technologyFeeRate,
            TechnologyFeeAmount = technologyFeeAmount,
            TotalDue = totalDue,
            Status = RoyaltyStatus.Pending,
        };

        db.RoyaltyPeriods.Add(royaltyPeriod);
        return Result.Success(royaltyPeriod.Id);
    }
}
