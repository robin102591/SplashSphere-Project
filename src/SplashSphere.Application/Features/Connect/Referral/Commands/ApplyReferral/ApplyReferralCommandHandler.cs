using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Connect.Referral.Commands.ApplyReferral;

public sealed class ApplyReferralCommandHandler(
    IApplicationDbContext db,
    IConnectUserContext connectUser,
    IPlanEnforcementService planService)
    : IRequestHandler<ApplyReferralCommand, Result<ApplyReferralResultDto>>
{
    public async Task<Result<ApplyReferralResultDto>> Handle(
        ApplyReferralCommand request,
        CancellationToken cancellationToken)
    {
        if (!connectUser.IsAuthenticated)
            return Result.Failure<ApplyReferralResultDto>(Error.Unauthorized("Sign in required."));

        var hasFeature = await planService.HasFeatureAsync(
            request.TenantId, FeatureKeys.CustomerLoyalty, cancellationToken);
        if (!hasFeature)
            return Result.Failure<ApplyReferralResultDto>(
                Error.Forbidden("This car wash does not offer a referral program."));

        var userId = connectUser.ConnectUserId;
        var normalized = request.Code.Trim().ToUpperInvariant();

        // Caller's customer at this tenant.
        var customerId = await db.ConnectUserTenantLinks
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.ConnectUserId == userId
                     && l.TenantId == request.TenantId
                     && l.IsActive)
            .Select(l => l.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId is null)
            return Result.Failure<ApplyReferralResultDto>(
                Error.Forbidden("Join this car wash before applying a referral code."));

        // Find the code at this tenant, Pending and unclaimed.
        var referral = await db.Referrals
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == request.TenantId
                  && r.ReferralCode == normalized
                  && r.Status == ReferralStatus.Pending,
                cancellationToken);

        if (referral is null)
            return Result.Failure<ApplyReferralResultDto>(
                Error.NotFound("Referral", normalized));

        if (referral.ReferredCustomerId is not null)
            return Result.Failure<ApplyReferralResultDto>(
                Error.Conflict("This referral code has already been used."));

        if (referral.ReferrerCustomerId == customerId)
            return Result.Failure<ApplyReferralResultDto>(
                Error.Validation("You cannot apply your own referral code."));

        // One referred row per customer per tenant.
        var alreadyReferred = await db.Referrals
            .IgnoreQueryFilters()
            .AnyAsync(
                r => r.TenantId == request.TenantId
                  && r.ReferredCustomerId == customerId,
                cancellationToken);

        if (alreadyReferred)
            return Result.Failure<ApplyReferralResultDto>(
                Error.Conflict("You have already been referred at this car wash."));

        referral.ReferredCustomerId = customerId;

        var tenantName = await db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.Id == request.TenantId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        return Result.Success(new ApplyReferralResultDto(
            ReferralId: referral.Id,
            TenantId: request.TenantId,
            TenantName: tenantName,
            ReferredPointsReward: referral.ReferredPointsEarned));
    }
}
