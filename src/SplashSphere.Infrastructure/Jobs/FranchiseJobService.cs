using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Jobs;

public sealed class FranchiseJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<FranchiseJobService> logger)
{
    [AutomaticRetry(Attempts = 2)]
    public async Task CalculateMonthlyRoyaltiesAsync(CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Previous month date range
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
        var periodEnd = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Get all active franchise agreements
        var agreements = await db.FranchiseAgreements
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.Status == AgreementStatus.Active)
            .ToListAsync(ct);

        var created = 0;
        foreach (var agreement in agreements)
        {
            // Check if period already calculated
            var exists = await db.RoyaltyPeriods
                .IgnoreQueryFilters()
                .AnyAsync(rp =>
                    rp.FranchiseeTenantId == agreement.FranchiseeTenantId &&
                    rp.PeriodStart == periodStart &&
                    rp.PeriodEnd == periodEnd, ct);

            if (exists) continue;

            // Load franchisor settings
            var settings = await db.FranchiseSettings
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(fs => fs.TenantId == agreement.FranchisorTenantId, ct);

            if (settings is null) continue;

            // Sum franchisee completed transactions
            var grossRevenue = await db.Transactions
                .IgnoreQueryFilters()
                .Where(t =>
                    t.TenantId == agreement.FranchiseeTenantId &&
                    t.Status == TransactionStatus.Completed &&
                    t.CreatedAt >= periodStart &&
                    t.CreatedAt < periodEnd)
                .SumAsync(t => t.FinalAmount, ct);

            var royaltyRate = agreement.CustomRoyaltyRate ?? settings.RoyaltyRate;
            var marketingFeeRate = agreement.CustomMarketingFeeRate ?? settings.MarketingFeeRate;
            var technologyFeeRate = settings.TechnologyFeeRate;

            var royaltyAmount = Math.Round(grossRevenue * royaltyRate, 2, MidpointRounding.AwayFromZero);
            var marketingFeeAmount = Math.Round(grossRevenue * marketingFeeRate, 2, MidpointRounding.AwayFromZero);
            var technologyFeeAmount = Math.Round(grossRevenue * technologyFeeRate, 2, MidpointRounding.AwayFromZero);
            var totalDue = royaltyAmount + marketingFeeAmount + technologyFeeAmount;

            var period = new Domain.Entities.RoyaltyPeriod(
                agreement.FranchisorTenantId,
                agreement.FranchiseeTenantId,
                agreement.Id)
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                GrossRevenue = grossRevenue,
                RoyaltyRate = royaltyRate,
                RoyaltyAmount = royaltyAmount,
                MarketingFeeRate = marketingFeeRate,
                MarketingFeeAmount = marketingFeeAmount,
                TechnologyFeeRate = technologyFeeRate,
                TechnologyFeeAmount = technologyFeeAmount,
                TotalDue = totalDue,
                Status = RoyaltyStatus.Pending
            };

            db.RoyaltyPeriods.Add(period);
            created++;
        }

        if (created > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation("FranchiseJob: Calculated {Count} royalty periods for {Month:yyyy-MM}.",
            created, periodStart);
    }

    [AutomaticRetry(Attempts = 2)]
    public async Task SendRoyaltyRemindersAsync(CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var now = DateTime.UtcNow;
        var overduePeriods = await db.RoyaltyPeriods
            .IgnoreQueryFilters()
            .Where(rp =>
                rp.Status == RoyaltyStatus.Pending &&
                rp.PeriodEnd < now.AddDays(-5))
            .ToListAsync(ct);

        foreach (var period in overduePeriods)
        {
            period.Status = RoyaltyStatus.Overdue;
        }

        if (overduePeriods.Count > 0)
            await db.SaveChangesAsync(ct);

        logger.LogInformation("FranchiseJob: Marked {Count} royalty periods as overdue.", overduePeriods.Count);
    }
}
