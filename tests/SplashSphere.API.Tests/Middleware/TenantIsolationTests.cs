using System.Net;
using System.Net.Http.Json;
using SplashSphere.API.Tests.TestFixtures;

namespace SplashSphere.API.Tests.Middleware;

/// <summary>
/// Verifies that tenant isolation works correctly — Tenant A cannot see Tenant B's data
/// and vice versa. Uses real PostgreSQL via Testcontainers.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public sealed class TenantIsolationTests(IntegrationTestFixture fixture)
{
    private readonly TestWebApplicationFactory _factory = fixture.Factory;

    [Fact]
    public async Task TenantA_CanSee_OwnServices()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantAId, TestDataBuilder.UserAId);

        var response = await client.GetAsync("/api/v1/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ServiceItem>>();
        body.Should().NotBeNull();
        body!.Items.Should().Contain(s => s.Name == "Basic Wash");
    }

    [Fact]
    public async Task TenantA_CannotSee_TenantB_Services()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantAId, TestDataBuilder.UserAId);

        var response = await client.GetAsync("/api/v1/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ServiceItem>>();
        body.Should().NotBeNull();
        body!.Items.Should().NotContain(s => s.Name == "Premium Wash");
    }

    [Fact]
    public async Task TenantB_CanSee_OwnServices()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantBId, TestDataBuilder.UserBId);

        var response = await client.GetAsync("/api/v1/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ServiceItem>>();
        body.Should().NotBeNull();
        body!.Items.Should().Contain(s => s.Name == "Premium Wash");
    }

    [Fact]
    public async Task TenantB_CannotSee_TenantA_Services()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantBId, TestDataBuilder.UserBId);

        var response = await client.GetAsync("/api/v1/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<ServiceItem>>();
        body.Should().NotBeNull();
        body!.Items.Should().NotContain(s => s.Name == "Basic Wash");
    }

    [Fact]
    public async Task TenantA_CanSee_OwnBranches_Only()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantAId, TestDataBuilder.UserAId);

        var response = await client.GetAsync("/api/v1/branches");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BranchItem[]>();
        body.Should().NotBeNull();
        body!.Should().AllSatisfy(b => b.Name.Should().Contain("SparkleWash"));
        body.Should().NotContain(b => b.Name.Contains("AquaShine"));
    }

    // ── Minimal response DTOs for deserialization ────────────────────────────

    private sealed record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize);
    private sealed record ServiceItem(string Id, string Name, decimal BasePrice, bool IsActive);
    private sealed record BranchItem(string Id, string Name, string Code, bool IsActive);
}
