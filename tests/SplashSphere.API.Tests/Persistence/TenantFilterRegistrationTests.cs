using Microsoft.EntityFrameworkCore;
using SplashSphere.Domain.Entities;
using SplashSphere.Domain.Interfaces;
using SplashSphere.Infrastructure.Auth;
using SplashSphere.Infrastructure.Persistence;

namespace SplashSphere.API.Tests.Persistence;

/// <summary>
/// Guards the auto-registered tenant query filter against silent regressions.
/// Building the EF Core model does NOT require a live database connection, so
/// these tests can run without Docker / Testcontainers.
/// </summary>
public sealed class TenantFilterRegistrationTests
{
    private static ApplicationDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            // Connection is never opened — only the EF model is needed.
            .UseNpgsql("Host=localhost;Database=fake;Username=fake;Password=fake")
            .Options;

        return new ApplicationDbContext(options, new TenantContext { TenantId = "test" });
    }

    [Fact]
    public void Every_ITenantScoped_entity_has_a_query_filter_registered()
    {
        using var ctx = BuildContext();

        var unfiltered = ctx.Model
            .GetEntityTypes()
            .Where(et => typeof(ITenantScoped).IsAssignableFrom(et.ClrType))
            .Where(et => et.GetQueryFilter() is null)
            .Select(et => et.ClrType.Name)
            .ToList();

        unfiltered.Should().BeEmpty(
            "every entity tagged with ITenantScoped must receive a global query " +
            "filter — missing one would leak rows across tenants. Affected types: " +
            string.Join(", ", unfiltered));
    }

    [Fact]
    public void Globally_scoped_entities_are_not_filtered()
    {
        using var ctx = BuildContext();

        // These types are intentionally NOT tenant-scoped. Adding the marker
        // (or a hand-wired filter) to one of them would be a behavior change
        // that should fail this test until reviewed.
        Type[] expectedUnfiltered =
        [
            typeof(Tenant),
            typeof(GlobalMake),
            typeof(GlobalModel),
            typeof(ConnectUser),
            typeof(ConnectVehicle),
            typeof(ConnectRefreshToken),
            typeof(FranchiseAgreement),
            typeof(RoyaltyPeriod),
            typeof(FranchiseInvitation),
        ];

        foreach (var clrType in expectedUnfiltered)
        {
            var entityType = ctx.Model.FindEntityType(clrType);
            entityType.Should().NotBeNull($"{clrType.Name} should be in the model");
            entityType!.GetQueryFilter().Should().BeNull(
                $"{clrType.Name} is intentionally global — it must not have a tenant filter.");
        }
    }
}
