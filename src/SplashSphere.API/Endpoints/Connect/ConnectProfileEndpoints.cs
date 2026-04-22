using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Connect.Profile.Commands.AddVehicle;
using SplashSphere.Application.Features.Connect.Profile.Commands.RemoveVehicle;
using SplashSphere.Application.Features.Connect.Profile.Commands.UpdateProfile;
using SplashSphere.Application.Features.Connect.Profile.Commands.UpdateVehicle;
using SplashSphere.Application.Features.Connect.Profile.Queries.GetMyProfile;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Connect user profile + vehicle CRUD under <c>/api/v1/connect/profile</c>.
/// All endpoints require a valid Connect JWT.
/// </summary>
public static class ConnectProfileEndpoints
{
    public static IEndpointRouteBuilder MapConnectProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapConnectGroup("/api/v1/connect/profile", "Connect.Profile");

        // GET /api/v1/connect/profile
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var profile = await sender.Send(new GetMyProfileQuery(), ct);
            return profile is null ? Results.Unauthorized() : Results.Ok(profile);
        })
        .WithName("Connect.GetMyProfile")
        .WithSummary("Read the authenticated Connect user's profile with vehicles.");

        // PATCH /api/v1/connect/profile
        group.MapPatch("/", async (
            [FromBody] UpdateProfileRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new UpdateProfileCommand(body.Name, body.Email, body.AvatarUrl), ct);
            return result.IsFailure ? result.ToProblem() : Results.Ok(result.Value);
        })
        .WithName("Connect.UpdateProfile")
        .WithSummary("Update the authenticated Connect user's display fields.");

        // POST /api/v1/connect/profile/vehicles
        group.MapPost("/vehicles", async (
            [FromBody] AddVehicleRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new AddVehicleCommand(
                    body.MakeId,
                    body.ModelId,
                    body.PlateNumber,
                    body.Color,
                    body.Year),
                ct);
            return result.IsFailure
                ? result.ToProblem()
                : Results.Created($"/api/v1/connect/profile/vehicles/{result.Value!.Id}", result.Value);
        })
        .WithName("Connect.AddVehicle")
        .WithSummary("Register a new vehicle on the user's profile.");

        // PATCH /api/v1/connect/profile/vehicles/{id}
        group.MapPatch("/vehicles/{id}", async (
            string id,
            [FromBody] UpdateVehicleRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new UpdateVehicleCommand(
                    id,
                    body.MakeId,
                    body.ModelId,
                    body.PlateNumber,
                    body.Color,
                    body.Year),
                ct);
            return result.IsFailure ? result.ToProblem() : Results.Ok(result.Value);
        })
        .WithName("Connect.UpdateVehicle")
        .WithSummary("Edit a vehicle on the user's profile.");

        // DELETE /api/v1/connect/profile/vehicles/{id}
        group.MapDelete("/vehicles/{id}", async (
            string id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RemoveVehicleCommand(id), ct);
            return result.IsFailure ? result.ToProblem() : Results.NoContent();
        })
        .WithName("Connect.RemoveVehicle")
        .WithSummary("Remove a vehicle from the user's profile.");

        return app;
    }
}

// ── Request bodies ──────────────────────────────────────────────────────────

internal sealed record UpdateProfileRequest(string Name, string? Email, string? AvatarUrl);

internal sealed record AddVehicleRequest(
    string MakeId,
    string ModelId,
    string PlateNumber,
    string? Color,
    int? Year);

internal sealed record UpdateVehicleRequest(
    string MakeId,
    string ModelId,
    string PlateNumber,
    string? Color,
    int? Year);
