using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Billing.Commands.CancelSubscription;

public sealed class CancelSubscriptionCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext,
    IPlanEnforcementService planService)
    : IRequestHandler<CancelSubscriptionCommand, Result>
{
    public async Task<Result> Handle(
        CancelSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var sub = await db.TenantSubscriptions
            .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId, cancellationToken);

        if (sub is null)
            return Result.Failure(Error.NotFound("TenantSubscription", tenantContext.TenantId));

        if (sub.Status == SubscriptionStatus.Cancelled)
            return Result.Failure(Error.Validation("Subscription is already cancelled."));

        var oldPlan = sub.PlanTier;
        sub.Status = SubscriptionStatus.Cancelled;

        db.PlanChangeLogs.Add(new PlanChangeLog(
            tenantContext.TenantId,
            oldPlan,
            oldPlan,
            tenantContext.UserId,
            "Subscription cancelled by tenant admin"));

        planService.EvictCache(tenantContext.TenantId);

        return Result.Success();
    }
}
