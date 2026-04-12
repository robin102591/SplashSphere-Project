using System.Net;
using SplashSphere.API.Tests.TestFixtures;

namespace SplashSphere.API.Tests.Middleware;

/// <summary>
/// Verifies that plan-based feature gates are enforced correctly.
/// Tenant A (Starter) should be blocked from Growth/Enterprise features.
/// Tenant B (Growth) should have access to Growth features.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public sealed class PlanEnforcementTests(IntegrationTestFixture fixture)
{
    private readonly TestWebApplicationFactory _factory = fixture.Factory;

    // ── Cash Advances (Growth feature: cash_advance_tracking) ────────────────

    [Fact]
    public async Task StarterPlan_CannotAccess_CashAdvances()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantAId, TestDataBuilder.UserAId);

        var response = await client.GetAsync("/api/v1/cash-advances");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GrowthPlan_CanAccess_CashAdvances()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantBId, TestDataBuilder.UserBId);

        var response = await client.GetAsync("/api/v1/cash-advances");

        // Should not be 403 — might be 200 (empty list) or any non-forbidden status
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // ── Shifts (Growth feature: shift_management) ────────────────────────────

    [Fact]
    public async Task StarterPlan_CannotAccess_Shifts()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantAId, TestDataBuilder.UserAId);

        var response = await client.GetAsync("/api/v1/shifts");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GrowthPlan_CanAccess_Shifts()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantBId, TestDataBuilder.UserBId);

        var response = await client.GetAsync("/api/v1/shifts");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // ── Expenses (Growth feature: expense_tracking) ──────────────────────────

    [Fact]
    public async Task StarterPlan_CannotAccess_Expenses()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantAId, TestDataBuilder.UserAId);

        var response = await client.GetAsync("/api/v1/expenses");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GrowthPlan_CanAccess_Expenses()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantBId, TestDataBuilder.UserBId);

        var response = await client.GetAsync("/api/v1/expenses");

        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    // ── Services (Core feature — accessible to all plans) ────────────────────

    [Fact]
    public async Task StarterPlan_CanAccess_CoreFeatures()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantAId, TestDataBuilder.UserAId);

        var response = await client.GetAsync("/api/v1/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GrowthPlan_CanAccess_CoreFeatures()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantBId, TestDataBuilder.UserBId);

        var response = await client.GetAsync("/api/v1/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Franchise (Enterprise-only feature) ──────────────────────────────────

    [Fact]
    public async Task StarterPlan_CannotAccess_Franchise()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantAId, TestDataBuilder.UserAId);

        var response = await client.GetAsync("/api/v1/franchise/settings");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GrowthPlan_CannotAccess_Franchise()
    {
        var client = _factory.CreateClientForTenant(TestDataBuilder.TenantBId, TestDataBuilder.UserBId);

        var response = await client.GetAsync("/api/v1/franchise/settings");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
