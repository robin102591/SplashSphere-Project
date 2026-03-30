using SplashSphere.Domain.Subscription;

namespace SplashSphere.Application.Common.Interfaces;

public interface IPlanEnforcementService
{
    /// <summary>Check if the tenant's active plan includes a specific feature.</summary>
    Task<bool> HasFeatureAsync(string tenantId, string featureKey, CancellationToken ct);

    /// <summary>Check if the tenant is within a resource limit (branches, employees, SMS).</summary>
    Task<PlanLimitResult> CheckLimitAsync(string tenantId, LimitType limitType, CancellationToken ct);

    /// <summary>Get the resolved plan definition for the tenant.</summary>
    Task<PlanDefinition> GetActivePlanAsync(string tenantId, CancellationToken ct);

    /// <summary>Get remaining SMS budget for the current month.</summary>
    Task<int> GetSmsBudgetRemainingAsync(string tenantId, CancellationToken ct);
}

public enum LimitType { Branches, Employees, SmsPerMonth }

public sealed record PlanLimitResult(
    bool Allowed,
    int CurrentCount,
    int MaxAllowed,
    string Message);
