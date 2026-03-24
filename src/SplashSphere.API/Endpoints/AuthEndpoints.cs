using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.Application.Features.Auth.Commands.SetUserPin;
using SplashSphere.Application.Features.Auth.Commands.VerifyPin;
using SplashSphere.Application.Features.Auth.Queries.GetCurrentUser;
using SplashSphere.Application.Common.Interfaces;
using SplashSphere.API.Extensions;
using SplashSphere.SharedKernel.Results;

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

        // POST /api/v1/auth/verify-pin
        // Verify the current user's POS lock PIN.
        group.MapPost("/verify-pin", async (
            [FromBody] VerifyPinRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new VerifyPinCommand(body.Pin), ct);
            return result.IsFailure
                ? result.ToProblem()
                : Results.Ok(new { success = result.Value });
        })
        .WithName("VerifyPin")
        .WithSummary("Verify the authenticated user's POS lock PIN.");

        // PATCH /api/v1/auth/users/{id}/pin
        // Set or reset a user's PIN (admin only).
        group.MapPatch("/users/{id}/pin", async (
            string id,
            [FromBody] SetPinRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SetUserPinCommand(id, body.Pin), ct);
            return result.IsFailure
                ? result.ToProblem()
                : Results.NoContent();
        })
        .WithName("SetUserPin")
        .WithSummary("Set or reset a user's POS lock PIN (admin only).");

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

// ── Request bodies ──────────────────────────────────────────────────────────

internal sealed record VerifyPinRequest(string Pin);
internal sealed record SetPinRequest(string Pin);
