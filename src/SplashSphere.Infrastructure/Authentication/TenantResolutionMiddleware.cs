using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.Infrastructure.Auth;

namespace SplashSphere.Infrastructure.Authentication;

/// <summary>
/// Runs immediately after <c>UseAuthentication</c>.
/// For every authenticated request it resolves the internal database <c>UserId</c>
/// from the JWT <c>ClerkUserId</c> and enforces the onboarding gate:
/// <list type="bullet">
///   <item>If the user has no tenant (<c>org_id</c> was absent from the JWT) and the
///   path is not in the allow-list → 403 with a "Complete onboarding first" message.</item>
///   <item>If the JWT is valid but no internal <see cref="Domain.Entities.User"/> row
///   exists → 403 (account setup is not complete).</item>
/// </list>
/// Unauthenticated requests (anonymous endpoints) pass through untouched.
/// </summary>
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    // Paths accessible to authenticated users who have not yet completed onboarding.
    // Checked with StartsWith — order does not matter.
    private static readonly string[] TenantlessPaths =
    [
        "/api/v1/auth/me",
        "/api/v1/onboarding",      // POST /api/v1/onboarding
        "/api/v1/onboarding/",     // GET  /api/v1/onboarding/status (prefix)
        "/webhooks/",
        // Dev / infra paths that must not be blocked:
        "/openapi",
        "/scalar",
        "/hangfire",
        "/health",
    ];

    public async Task InvokeAsync(
        HttpContext httpContext,
        IApplicationDbContext db,
        TenantContext tenantContext)
    {
        // Skip unauthenticated requests — auth middleware handles 401.
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            await next(httpContext);
            return;
        }

        // Always skip public / infra paths.
        var path = httpContext.Request.Path.Value ?? string.Empty;
        if (IsPublicPath(path))
        {
            await next(httpContext);
            return;
        }

        // ── Resolve internal User record ──────────────────────────────────────
        // IgnoreQueryFilters because User.TenantId may be null (pre-onboarding).
        var user = await db.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(u => u.ClerkUserId == tenantContext.ClerkUserId)
            .Select(u => new { u.Id, u.TenantId })
            .FirstOrDefaultAsync(httpContext.RequestAborted);

        if (user is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                status  = 403,
                title   = "ACCOUNT_SETUP_INCOMPLETE",
                detail  = "Your account has not been fully set up. Please sign in again.",
            }, httpContext.RequestAborted);
            return;
        }

        // Populate the internal UserId that command handlers use as CashierId, etc.
        tenantContext.UserId = user.Id;

        // ── Enforce onboarding gate ───────────────────────────────────────────
        // TenantId is empty when the JWT has no org_id (new user, not yet onboarded).
        if (string.IsNullOrEmpty(tenantContext.TenantId))
        {
            if (!IsTenantlessAllowed(path))
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    status  = 403,
                    title   = "ONBOARDING_REQUIRED",
                    detail  = "Complete onboarding before accessing this resource.",
                }, httpContext.RequestAborted);
                return;
            }
        }

        await next(httpContext);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool IsPublicPath(string path)
        => TenantlessPaths.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    private static bool IsTenantlessAllowed(string path)
        => path.Equals("/api/v1/auth/me", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/api/v1/onboarding", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/webhooks/", StringComparison.OrdinalIgnoreCase);
}
