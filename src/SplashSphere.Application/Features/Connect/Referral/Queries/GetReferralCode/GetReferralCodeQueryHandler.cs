using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Referral.Queries.GetReferralCode;

public sealed class GetReferralCodeQueryHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser,
    IPlanEnforcementService planService)
    : IRequestHandler<GetReferralCodeQuery, Result<ConnectReferralCodeDto>>
{
    // Default rewards when no tenant-configured amounts exist.
    // Docs (CUSTOMER_APP.md) specify 100/50; admin-configurable later.
    private const int DefaultReferrerReward = 100;
    private const int DefaultReferredReward = 50;

    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no O/0/1/I
    private const int CodeSuffixLength = 4;

    public async Task<Result<ConnectReferralCodeDto>> Handle(
        GetReferralCodeQuery request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
            return Result.Failure<ConnectReferralCodeDto>(Error.Unauthorized("Sign in required."));

        var hasFeature = await planService.HasFeatureAsync(
            request.TenantId, FeatureKeys.CustomerLoyalty, cancellationToken);
        if (!hasFeature)
            return Result.Failure<ConnectReferralCodeDto>(
                Error.Forbidden("This car wash does not offer a referral program."));

        var userId = connectUser.ConnectUserId;

        var linkRow = await (
            from link in db.ConnectUserTenantLinks.IgnoreQueryFilters()
            join customer in db.Customers.IgnoreQueryFilters()
                on link.CustomerId equals customer.Id
            where link.ConnectUserId == userId
               && link.TenantId == request.TenantId
               && link.IsActive
            select new { link.CustomerId, CustomerFirstName = customer.FirstName })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (linkRow is null)
            return Result.Failure<ConnectReferralCodeDto>(
                Error.Forbidden("Join this car wash before sharing a referral code."));

        // ── Existing code? ───────────────────────────────────────────────────
        var existing = await db.Referrals
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == request.TenantId
                  && r.ReferrerCustomerId == linkRow.CustomerId
                  && r.ReferredCustomerId == null
                  && r.Status == ReferralStatus.Pending,
                cancellationToken);

        if (existing is null)
        {
            var code = await GenerateUniqueCodeAsync(
                request.TenantId, linkRow.CustomerFirstName, cancellationToken);

            existing = new Domain.Entities.Referral(
                tenantId: request.TenantId,
                referrerCustomerId: linkRow.CustomerId,
                referralCode: code,
                referrerPointsReward: DefaultReferrerReward,
                referredPointsReward: DefaultReferredReward);

            db.Referrals.Add(existing);
        }

        // ── Stats across all referrals this customer owns at this tenant ────
        var stats = await db.Referrals
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.TenantId == request.TenantId
                     && r.ReferrerCustomerId == linkRow.CustomerId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(r => r.ReferredCustomerId != null),
                Completed = g.Count(r => r.Status == ReferralStatus.Completed),
                Pending = g.Count(r => r.Status == ReferralStatus.Pending
                                    && r.ReferredCustomerId != null),
                PointsEarned = g.Sum(r => r.Status == ReferralStatus.Completed
                    ? r.ReferrerPointsEarned
                    : 0),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new ConnectReferralCodeDto(
            Code: existing.ReferralCode,
            ReferrerPointsReward: existing.ReferrerPointsEarned,
            ReferredPointsReward: existing.ReferredPointsEarned,
            TotalReferrals: stats?.Total ?? 0,
            CompletedReferrals: stats?.Completed ?? 0,
            PendingReferrals: stats?.Pending ?? 0,
            PointsEarned: stats?.PointsEarned ?? 0));
    }

    private async Task<string> GenerateUniqueCodeAsync(
        string tenantId, string firstName, CancellationToken ct)
    {
        // Sanitize the first name down to a short uppercase prefix (max 6 chars,
        // alphabetic only). Falls back to "USER" for unusual inputs.
        var prefix = new string((firstName ?? string.Empty)
            .Where(char.IsLetter)
            .Take(6)
            .ToArray())
            .ToUpperInvariant();
        if (prefix.Length == 0) prefix = "USER";

        for (var attempt = 0; attempt < 10; attempt++)
        {
            var suffix = new string(Enumerable.Range(0, CodeSuffixLength)
                .Select(_ => CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)])
                .ToArray());
            var code = $"{prefix}-{suffix}";

            var collision = await db.Referrals
                .IgnoreQueryFilters()
                .AnyAsync(r => r.TenantId == tenantId && r.ReferralCode == code, ct);

            if (!collision) return code;
        }

        // Last-resort fallback — 8-char fully random code.
        return new string(Enumerable.Range(0, 8)
            .Select(_ => CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)])
            .ToArray());
    }
}
