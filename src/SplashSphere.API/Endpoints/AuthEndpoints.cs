using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Auth.Queries.GetCurrentUser;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .RequireAuthorization()
            .WithTags("Auth");

        // GET /api/v1/auth/me
        // Returns the current user's profile + tenant info.
        // Accessible before onboarding is complete (allowed by TenantResolutionMiddleware).
        group.MapGet("/me", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCurrentUserQuery(), ct);

            return result is null
                ? Results.NotFound()
                : Results.Ok(result);
        })
        .WithName("GetCurrentUser")
        .WithSummary("Get the authenticated user's profile and tenant information.");

        // GET /api/v1/onboarding/status
        // Returns whether the current user still needs to complete onboarding.
        // Accessible before onboarding (TenantResolutionMiddleware allows /api/v1/onboarding*).
        app.MapGet("/api/v1/onboarding/status", async (
            ISender sender,
            ITenantContext tenantContext,
            CancellationToken ct) =>
        {
            var user = await sender.Send(new GetCurrentUserQuery(), ct);

            // needsOnboarding = user exists but has no tenant yet
            var needsOnboarding = user is not null && user.Tenant is null;

            return Results.Ok(new { needsOnboarding });
        })
        .RequireAuthorization()
        .WithName("GetOnboardingStatus")
        .WithTags("Onboarding")
        .WithSummary("Check whether the authenticated user needs to complete onboarding.");

        return app;
    }
}
