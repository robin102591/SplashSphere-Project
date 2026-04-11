using MediatR;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;

namespace SplashSphere.Application.Features.Billing.Queries.GetCurrentPlan;

public sealed class GetCurrentPlanQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenantContext)
    : IRequestHandler<GetCurrentPlanQuery, TenantPlanDto>
{
    public async Task<TenantPlanDto> Handle(
        GetCurrentPlanQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        var sub = await db.TenantSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        var tenantType = await db.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => t.TenantType)
            .FirstOrDefaultAsync(cancellationToken);

        var plan = PlanCatalog.GetEffectivePlan(sub?.PlanTier ?? PlanTier.Starter, tenantType);

        var branchCount = await db.Branches
            .CountAsync(b => b.IsActive, cancellationToken);

        var employeeCount = await db.Employees
            .CountAsync(e => e.IsActive, cancellationToken);

        var limits = new PlanLimitsDto(
            sub?.MaxBranchesOverride ?? plan.MaxBranches,
            branchCount,
            sub?.MaxEmployeesOverride ?? plan.MaxEmployees,
            employeeCount,
            sub?.SmsPerMonthOverride ?? plan.SmsPerMonth,
            sub?.SmsUsedThisMonth ?? 0);

        TrialInfoDto? trial = null;
        if (sub is not null && sub.Status == SubscriptionStatus.Trial)
        {
            var daysRemaining = Math.Max(0, (int)(sub.TrialEndDate - DateTime.UtcNow).TotalDays);
            trial = new TrialInfoDto(
                sub.TrialStartDate,
                sub.TrialEndDate,
                daysRemaining,
                sub.TrialExpired);
        }

        BillingInfoDto? billing = null;
        if (sub?.CurrentPeriodStart is not null)
        {
            billing = new BillingInfoDto(
                sub.NextBillingDate,
                sub.LastPaymentDate,
                sub.CurrentPeriodStart,
                sub.CurrentPeriodEnd);
        }

        var status = sub?.Status ?? SubscriptionStatus.Trial;
        var tierName = (sub?.PlanTier ?? PlanTier.Starter).ToString().ToLowerInvariant();
        var statusName = status.ToString().ToLowerInvariant();
        // Convert PastDue to past_due for frontend
        if (status == SubscriptionStatus.PastDue)
            statusName = "past_due";

        return new TenantPlanDto(
            tierName,
            statusName,
            plan.Name,
            plan.MonthlyPrice,
            plan.Features.ToList(),
            limits,
            trial,
            billing);
    }
}
