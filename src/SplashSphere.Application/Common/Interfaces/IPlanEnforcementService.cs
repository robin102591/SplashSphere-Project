using SplashSphere.Domain.Subscription;

namespace SplashSphere.Application.Common.Interfaces;

public interface IPlanEnforcementService
{
    /// <summary>Check if the tenant's active plan includes a specific feature.</summary>
    Task<bool> HasFeatureAsync(string tenantId, string featureKey, CancellationToken ct);

    /// <summary>Check if the tenant is within a resource limit (branches, employees, SMS).</summary>
    Task<PlanLimitResult> CheckLimitAsync(string tenantId, LimitType limitType, CancellationToken ct);

    /// <summary>
    /// Per-branch limit check for POS stations. Kept separate from
    /// <see cref="CheckLimitAsync"/> because the count is scoped to a
    /// specific branch rather than the tenant as a whole.
    /// </summary>
    Task<PlanLimitResult> CheckPosStationLimitAsync(string tenantId, string branchId, CancellationToken ct);

    /// <summary>Get the resolved plan definition for the tenant.</summary>
    Task<PlanDefinition> GetActivePlanAsync(string tenantId, CancellationToken ct);

    /// <summary>Get remaining SMS budget for the current month.</summary>
    Task<int> GetSmsBudgetRemainingAsync(string tenantId, CancellationToken ct);

    /// <summary>Check if the tenant has remaining SMS quota this month.</summary>
    Task<bool> HasSmsQuotaAsync(string tenantId, CancellationToken ct);

    /// <summary>Increment the SMS usage counter for the tenant.</summary>
    Task IncrementSmsUsageAsync(string tenantId, CancellationToken ct);

    /// <summary>Evict cached subscription for a tenant (call after plan change).</summary>
    void EvictCache(string tenantId);
}

public enum LimitType { Branches, Employees, SmsPerMonth }

public sealed record PlanLimitResult(
    bool Allowed,
    int CurrentCount,
    int MaxAllowed,
    string Message);
