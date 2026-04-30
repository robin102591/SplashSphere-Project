using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.Infrastructure.Authentication;

/// <summary>
/// Registers the Clerk JWT Bearer authentication scheme for production.
/// <para>
/// <see cref="JwtBearerEvents.OnTokenValidated"/> populates <see cref="TenantContext"/>
/// from the validated JWT claims so every request has its <c>ClerkUserId</c>,
/// <c>TenantId</c>, and <c>Role</c> available for downstream handlers and global query filters.
/// </para>
/// </summary>
public static class ClerkJwtSetup
{
    public static AuthenticationBuilder AddClerkJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Clerk:Authority"];

                // Prevent the JWT middleware from remapping standard claim names (e.g. "sub"
                // → long URI). With this off, claims arrive exactly as they appear in the token.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer   = true,
                    ValidIssuer      = configuration["Clerk:Authority"],
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    NameClaimType    = "sub",
                };

                options.Events = new JwtBearerEvents
                {
                    // SignalR's WebSocket transport can't set the Authorization
                    // header on the upgrade request (browser limitation), so it
                    // appends the token as ?access_token=... when targeting the
                    // hub path. The JwtBearer middleware only reads from the
                    // Authorization header by default, so without this hook the
                    // WebSocket connection is unauthenticated even though the
                    // negotiate (HTTP POST) was authenticated. Hub methods
                    // protected by [Authorize] then reject every invocation
                    // with "user is unauthorized".
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(token)
                            && path.StartsWithSegments("/hubs"))
                        {
                            ctx.Token = token;
                        }
                        return Task.CompletedTask;
                    },

                    OnTokenValidated = ctx =>
                    {
                        var tenantCtx = ctx.HttpContext.RequestServices
                            .GetRequiredService<TenantContext>();

                        var claims = ctx.Principal!.Claims;

                        tenantCtx.ClerkUserId = claims
                            .FirstOrDefault(c => c.Type == "sub")?.Value
                            ?? string.Empty;

                        // org_id is absent for users who haven't completed onboarding.
                        tenantCtx.TenantId = claims
                            .FirstOrDefault(c => c.Type == "org_id")?.Value
                            ?? string.Empty;

                        tenantCtx.Role = claims
                            .FirstOrDefault(c => c.Type == "org_role")?.Value;

                        return Task.CompletedTask;
                    },
                };
            });
    }
}
