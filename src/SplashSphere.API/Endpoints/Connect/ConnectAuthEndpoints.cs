using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Connect.Auth.Commands.RefreshToken;
using SplashSphere.Application.Features.Connect.Auth.Commands.SendOtp;
using SplashSphere.Application.Features.Connect.Auth.Commands.SignOut;
using SplashSphere.Application.Features.Connect.Auth.Commands.VerifyOtp;

namespace SplashSphere.API.Endpoints.Connect;

/// <summary>
/// Phone-OTP authentication endpoints for the Customer Connect app.
/// All four endpoints are anonymous — the caller is establishing identity, not using one.
/// </summary>
public static class ConnectAuthEndpoints
{
    public static IEndpointRouteBuilder MapConnectAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/connect/auth")
            .AllowAnonymous()
            .WithTags("Connect.Auth");

        // POST /api/v1/connect/auth/otp/send
        group.MapPost("/otp/send", async (
            [FromBody] SendOtpRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SendOtpCommand(body.PhoneNumber), ct);
            return result.IsFailure ? result.ToProblem() : Results.Ok(result.Value);
        })
        .WithName("Connect.SendOtp")
        .WithSummary("Send a one-time code to a Philippine mobile number.");

        // POST /api/v1/connect/auth/otp/verify
        group.MapPost("/otp/verify", async (
            [FromBody] VerifyOtpRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new VerifyOtpCommand(body.PhoneNumber, body.Code), ct);
            return result.IsFailure ? result.ToProblem() : Results.Ok(result.Value);
        })
        .WithName("Connect.VerifyOtp")
        .WithSummary("Verify the OTP and receive access + refresh tokens.");

        // POST /api/v1/connect/auth/refresh
        group.MapPost("/refresh", async (
            [FromBody] RefreshRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RefreshTokenCommand(body.RefreshToken), ct);
            return result.IsFailure ? result.ToProblem() : Results.Ok(result.Value);
        })
        .WithName("Connect.Refresh")
        .WithSummary("Rotate refresh token and receive a new access token.");

        // POST /api/v1/connect/auth/sign-out
        group.MapPost("/sign-out", async (
            [FromBody] SignOutRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new SignOutCommand(body.RefreshToken), ct);
            return result.IsFailure ? result.ToProblem() : Results.NoContent();
        })
        .WithName("Connect.SignOut")
        .WithSummary("Revoke a refresh token.");

        return app;
    }
}

// ── Request bodies ──────────────────────────────────────────────────────────

internal sealed record SendOtpRequest(string PhoneNumber);
internal sealed record VerifyOtpRequest(string PhoneNumber, string Code);
internal sealed record RefreshRequest(string RefreshToken);
internal sealed record SignOutRequest(string RefreshToken);
