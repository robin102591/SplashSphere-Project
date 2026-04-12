using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Enums;
using SplashSphere.Infrastructure.Auth;
using SplashSphere.Infrastructure.Persistence;

namespace SplashSphere.API.Tests.TestFixtures;

/// <summary>
/// Seeds deterministic test data for two isolated tenants.
/// Each tenant gets a branch, user, subscription, service category, and sample service.
/// Call <see cref="SeedAsync"/> once per test run (typically in the collection fixture).
/// </summary>
public static class TestDataBuilder
{
    // ── Tenant A: SparkleWash (Starter plan) ─────────────────────────────────
    public const string TenantAId = "org_test_sparklwash";
    public const string TenantABranchId = "branch_test_makati";
    public const string TenantACategoryId = "cat_test_exterior_a";
    public const string TenantAServiceId = "svc_test_basic_wash";
    public const string UserAId = "user_test_juan";

    // ── Tenant B: AquaShine (Growth plan) ────────────────────────────────────
    public const string TenantBId = "org_test_aquashine";
    public const string TenantBBranchId = "branch_test_cebu";
    public const string TenantBCategoryId = "cat_test_exterior_b";
    public const string TenantBServiceId = "svc_test_premium_wash";
    public const string UserBId = "user_test_maria";

    /// <summary>
    /// Seeds two tenants with branches, users, subscriptions, categories, and services.
    /// Uses separate DI scopes per tenant so that TenantContext is set correctly
    /// for each batch of entities.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        await SeedTenantAAsync(services);
        await SeedTenantBAsync(services);
    }

    private static async Task SeedTenantAAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();
        tenantContext.TenantId = TenantAId;

        // Already seeded guard
        if (await context.Tenants.AnyAsync(t => t.Id == TenantAId))
            return;

        var tenant = new Tenant(TenantAId, "SparkleWash Philippines", "admin@sparklewash.ph", "+639171234567", "123 Makati Ave, Makati City")
        {
            TenantType = TenantType.Independent,
        };

        var branch = new Branch(TenantAId, "SparkleWash - Makati", "MKT", "123 Makati Ave, Makati City", "+639171234567")
        {
            Id = TenantABranchId,
            IsActive = true,
        };

        var user = new User(UserAId, "juan@sparklewash.ph", "Juan", "Dela Cruz")
        {
            TenantId = TenantAId,
            Role = "org:admin",
            IsActive = true,
        };

        var subscription = new TenantSubscription(TenantAId, PlanTier.Starter, SubscriptionStatus.Active)
        {
            CurrentPeriodStart = DateTime.UtcNow.AddDays(-15),
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(15),
            TrialStartDate = DateTime.UtcNow.AddDays(-30),
            TrialEndDate = DateTime.UtcNow.AddDays(-16),
            SmsUsedThisMonth = 0,
            SmsCountResetDate = DateTime.UtcNow,
        };

        var category = new ServiceCategory(TenantAId, "Exterior Wash")
        {
            Id = TenantACategoryId,
            IsActive = true,
        };

        var service = new Service(TenantAId, TenantACategoryId, "Basic Wash", 220m)
        {
            Id = TenantAServiceId,
            IsActive = true,
        };

        context.Tenants.Add(tenant);
        context.Branches.Add(branch);
        context.Users.Add(user);
        context.Set<TenantSubscription>().Add(subscription);
        context.ServiceCategories.Add(category);
        context.Services.Add(service);

        await context.SaveChangesAsync();
    }

    private static async Task SeedTenantBAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();
        tenantContext.TenantId = TenantBId;

        // Already seeded guard
        if (await context.Tenants.AnyAsync(t => t.Id == TenantBId))
            return;

        var tenant = new Tenant(TenantBId, "AquaShine Cebu", "admin@aquashine.ph", "+639281234567", "456 Osmena Blvd, Cebu City")
        {
            TenantType = TenantType.Independent,
        };

        var branch = new Branch(TenantBId, "AquaShine - Cebu", "CEB", "456 Osmena Blvd, Cebu City", "+639281234567")
        {
            Id = TenantBBranchId,
            IsActive = true,
        };

        var user = new User(UserBId, "maria@aquashine.ph", "Maria", "Santos")
        {
            TenantId = TenantBId,
            Role = "org:admin",
            IsActive = true,
        };

        var subscription = new TenantSubscription(TenantBId, PlanTier.Growth, SubscriptionStatus.Active)
        {
            CurrentPeriodStart = DateTime.UtcNow.AddDays(-10),
            CurrentPeriodEnd = DateTime.UtcNow.AddDays(20),
            TrialStartDate = DateTime.UtcNow.AddDays(-40),
            TrialEndDate = DateTime.UtcNow.AddDays(-26),
            SmsUsedThisMonth = 0,
            SmsCountResetDate = DateTime.UtcNow,
        };

        var category = new ServiceCategory(TenantBId, "Exterior Wash")
        {
            Id = TenantBCategoryId,
            IsActive = true,
        };

        var service = new Service(TenantBId, TenantBCategoryId, "Premium Wash", 380m)
        {
            Id = TenantBServiceId,
            IsActive = true,
        };

        context.Tenants.Add(tenant);
        context.Branches.Add(branch);
        context.Users.Add(user);
        context.Set<TenantSubscription>().Add(subscription);
        context.ServiceCategories.Add(category);
        context.Services.Add(service);

        await context.SaveChangesAsync();
    }
}
