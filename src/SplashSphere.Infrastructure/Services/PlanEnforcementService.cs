using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Domain.Subscription;

namespace SplashSphere.Infrastructure.Services;

public sealed class PlanEnforcementService(IApplicationDbContext db) : IPlanEnforcementService
{
    // Simple in-memory cache: tenantId → (subscription, expiry)
    private static readonly ConcurrentDictionary<string, (TenantSubscription Sub, DateTime Expiry)> Cache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public async Task<bool> HasFeatureAsync(string tenantId, string featureKey, CancellationToken ct)
    {
        var sub = await GetSubscriptionAsync(tenantId, ct);
        if (sub is null) return true; // No subscription record = unrestricted (legacy/dev)

        // Suspended tenants get no gated features
        if (sub.Status == SubscriptionStatus.Suspended)
            return false;

        // Trial expired → block gated features
        if (sub.TrialExpired)
            return false;

        // Cancelled → block everything
        if (sub.Status == SubscriptionStatus.Cancelled)
            return false;

        var plan = PlanCatalog.GetPlan(sub.PlanTier);

        // Check feature overrides first (SaaS admin can grant/revoke)
        if (!string.IsNullOrEmpty(sub.FeatureOverrides))
        {
            var overrides = JsonSerializer.Deserialize<Dictionary<string, bool>>(sub.FeatureOverrides);
            if (overrides?.TryGetValue(featureKey, out var enabled) == true)
                return enabled;
        }

        return plan.Features.Contains(featureKey);
    }

    public async Task<PlanLimitResult> CheckLimitAsync(string tenantId, LimitType limitType, CancellationToken ct)
    {
        var sub = await GetSubscriptionAsync(tenantId, ct);
        if (sub is null) return new PlanLimitResult(true, 0, int.MaxValue, "");

        var plan = PlanCatalog.GetPlan(sub.PlanTier);

        return limitType switch
        {
            LimitType.Branches => await CheckBranchLimitAsync(tenantId, plan, sub, ct),
            LimitType.Employees => await CheckEmployeeLimitAsync(tenantId, plan, sub, ct),
            LimitType.SmsPerMonth => CheckSmsLimit(plan, sub),
            _ => new PlanLimitResult(true, 0, int.MaxValue, "")
        };
    }

    public async Task<PlanDefinition> GetActivePlanAsync(string tenantId, CancellationToken ct)
    {
        var sub = await GetSubscriptionAsync(tenantId, ct);
        return PlanCatalog.GetPlan(sub?.PlanTier ?? PlanTier.Starter);
    }

    public async Task<int> GetSmsBudgetRemainingAsync(string tenantId, CancellationToken ct)
    {
        var sub = await GetSubscriptionAsync(tenantId, ct);
        if (sub is null) return 0;

        var plan = PlanCatalog.GetPlan(sub.PlanTier);
        var max = sub.SmsPerMonthOverride ?? plan.SmsPerMonth;
        return Math.Max(0, max - sub.SmsUsedThisMonth);
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<TenantSubscription?> GetSubscriptionAsync(string tenantId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(tenantId)) return null;

        if (Cache.TryGetValue(tenantId, out var cached) && cached.Expiry > DateTime.UtcNow)
            return cached.Sub;

        var sub = await db.TenantSubscriptions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        if (sub is not null)
            Cache[tenantId] = (sub, DateTime.UtcNow + CacheDuration);

        return sub;
    }

    private async Task<PlanLimitResult> CheckBranchLimitAsync(
        string tenantId, PlanDefinition plan, TenantSubscription sub, CancellationToken ct)
    {
        var currentCount = await db.Branches
            .IgnoreQueryFilters()
            .CountAsync(b => b.TenantId == tenantId && b.IsActive, ct);

        var max = sub.MaxBranchesOverride ?? plan.MaxBranches;
        return new PlanLimitResult(
            currentCount < max, currentCount, max,
            currentCount >= max
                ? $"Your {plan.Name} plan allows {max} branch(es). Upgrade to add more."
                : "");
    }

    private async Task<PlanLimitResult> CheckEmployeeLimitAsync(
        string tenantId, PlanDefinition plan, TenantSubscription sub, CancellationToken ct)
    {
        var currentCount = await db.Employees
            .IgnoreQueryFilters()
            .CountAsync(e => e.TenantId == tenantId && e.IsActive, ct);

        var max = sub.MaxEmployeesOverride ?? plan.MaxEmployees;
        return new PlanLimitResult(
            currentCount < max, currentCount, max,
            currentCount >= max
                ? $"Your {plan.Name} plan allows {max} employee(s). Upgrade to add more."
                : "");
    }

    private static PlanLimitResult CheckSmsLimit(PlanDefinition plan, TenantSubscription sub)
    {
        var max = sub.SmsPerMonthOverride ?? plan.SmsPerMonth;
        var current = sub.SmsUsedThisMonth;
        return new PlanLimitResult(
            current < max, current, max,
            current >= max
                ? $"You've used all {max} SMS messages for this month."
                : "");
    }

    /// <summary>Evict cached subscription for a tenant (call after plan change).</summary>
    public static void EvictCache(string tenantId) => Cache.TryRemove(tenantId, out _);
}
