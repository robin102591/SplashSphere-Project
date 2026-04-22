using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;

namespace SplashSphere.Infrastructure.Jobs;

/// <summary>
/// Daily sweep that expires referral codes that were shared but never redeemed.
/// Rules: Status is <see cref="ReferralStatus.Pending"/>, no <c>ReferredCustomerId</c>,
/// and <c>CreatedAt</c> is older than 90 days.
/// </summary>
public sealed class ReferralJobService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReferralJobService> logger)
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 120)]
    public async Task ExpireReferralsAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-90);
        var now = DateTime.UtcNow;

        logger.LogDebug(
            "ReferralJob: Expiring Pending referrals with CreatedAt < {Cutoff:u} and no ReferredCustomerId.",
            cutoff);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var expirable = await db.Referrals
            .IgnoreQueryFilters()
            .Where(r => r.Status == ReferralStatus.Pending
                     && r.ReferredCustomerId == null
                     && r.CreatedAt < cutoff)
            .ToListAsync(ct);

        if (expirable.Count == 0)
        {
            logger.LogDebug("ReferralJob: No referrals eligible for expiry.");
            return;
        }

        foreach (var r in expirable)
        {
            r.Status    = ReferralStatus.Expired;
            r.ExpiredAt = now;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "ReferralJob: Expired {Count} referral code(s).", expirable.Count);
    }
}
