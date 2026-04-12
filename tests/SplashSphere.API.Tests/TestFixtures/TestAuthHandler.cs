using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.API.Tests.TestFixtures;

/// <summary>
/// Authentication handler for integration tests that reads tenant/user info
/// from custom request headers and populates TenantContext accordingly.
/// This allows each test to impersonate any tenant/user combination by
/// setting headers on the HttpClient.
/// </summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TenantContext tenantContext)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestAuth";

    public const string TenantIdHeader = "X-Test-TenantId";
    public const string UserIdHeader = "X-Test-UserId";
    public const string RoleHeader = "X-Test-Role";

    private const string DefaultTenantId = "org_test_sparklwash";
    private const string DefaultUserId = "user_test_juan";
    private const string DefaultRole = "org:admin";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var tenantId = GetHeaderOrDefault(TenantIdHeader, DefaultTenantId);
        var userId = GetHeaderOrDefault(UserIdHeader, DefaultUserId);
        var role = GetHeaderOrDefault(RoleHeader, DefaultRole);

        // Populate TenantContext so global query filters and handlers work correctly.
        tenantContext.ClerkUserId = userId;
        tenantContext.TenantId = tenantId;
        tenantContext.Role = role;

        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim("org_id", tenantId),
            new Claim("org_role", role),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string GetHeaderOrDefault(string headerName, string defaultValue)
    {
        return Request.Headers.TryGetValue(headerName, out var values) && values.Count > 0
            ? values.ToString()
            : defaultValue;
    }
}
