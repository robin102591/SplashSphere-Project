using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.API.Infrastructure;

/// <summary>
/// Development-only authentication handler that auto-authenticates every request
/// using hardcoded seed tenant/user values. Never registered in Production.
/// Populates the same claims that the real Clerk JWT handler would set so that
/// TenantContext is fully populated and global query filters work correctly.
/// </summary>
public sealed class DevAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TenantContext tenantContext)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "DevAuth";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Populate TenantContext with the same values as the seed data.
        tenantContext.ClerkUserId = "user_dev_seed";
        tenantContext.TenantId   = "org_sparklwash_dev";
        tenantContext.Role       = "org:admin";

        var claims = new[]
        {
            new Claim("sub",      tenantContext.ClerkUserId),
            new Claim("org_id",   tenantContext.TenantId),
            new Claim("org_role", tenantContext.Role),
        };

        var identity  = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
