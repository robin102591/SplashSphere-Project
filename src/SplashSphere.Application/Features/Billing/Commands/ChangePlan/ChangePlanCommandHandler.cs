using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Billing.Commands.ChangePlan;

public sealed class ChangePlanCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext,
    IPlanEnforcementService planService)
    : IRequestHandler<ChangePlanCommand, Result>
{
    public async Task<Result> Handle(
        ChangePlanCommand request,
        CancellationToken cancellationToken)
    {
        if (request.NewPlan == PlanTier.Trial)
            return Result.Failure(Error.Validation("Cannot change to Trial plan."));

        var sub = await db.TenantSubscriptions
            .FirstOrDefaultAsync(s => s.TenantId == tenantContext.TenantId, cancellationToken);

        if (sub is null)
            return Result.Failure(Error.NotFound("TenantSubscription", tenantContext.TenantId));

        if (sub.PlanTier == request.NewPlan)
            return Result.Failure(Error.Validation("Already on this plan."));

        var oldPlan = sub.PlanTier;

        // Validate downgrade limits
        var newPlanDef = PlanCatalog.GetPlan(request.NewPlan);

        var branchCount = await db.Branches.CountAsync(b => b.IsActive, cancellationToken);
        if (branchCount > newPlanDef.MaxBranches)
            return Result.Failure(Error.Validation(
                $"Cannot downgrade: you have {branchCount} active branches but {newPlanDef.Name} allows {newPlanDef.MaxBranches}."));

        var employeeCount = await db.Employees.CountAsync(e => e.IsActive, cancellationToken);
        if (employeeCount > newPlanDef.MaxEmployees)
            return Result.Failure(Error.Validation(
                $"Cannot downgrade: you have {employeeCount} active employees but {newPlanDef.Name} allows {newPlanDef.MaxEmployees}."));

        sub.PlanTier = request.NewPlan;

        db.PlanChangeLogs.Add(new PlanChangeLog(
            tenantContext.TenantId,
            oldPlan,
            request.NewPlan,
            tenantContext.UserId,
            $"Plan changed from {oldPlan} to {request.NewPlan}"));

        planService.EvictCache(tenantContext.TenantId);

        return Result.Success();
    }
}
