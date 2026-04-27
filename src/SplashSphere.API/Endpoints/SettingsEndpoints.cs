using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Settings.Commands.UpdateCompanyProfile;
using SplashSphere.Application.Features.Settings.Queries.GetCompanyProfile;

namespace SplashSphere.API.Endpoints;

/// <summary>
/// Tenant-level admin settings: company profile (slice 1), receipt designer
/// (slice 2), file uploads (slice 3). Each settings concern lives under
/// <c>/api/v1/settings/{concern}</c>.
/// </summary>
public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/settings")
            .RequireAuthorization()
            .WithTags("Settings");

        // ── Company profile ───────────────────────────────────────────────────
        group.MapGet("/company", GetCompanyProfile)
            .WithName("GetCompanyProfile")
            .WithSummary("Get the current tenant's company profile");

        group.MapPut("/company", UpdateCompanyProfile)
            .WithName("UpdateCompanyProfile")
            .WithSummary("Update the current tenant's company profile");

        return app;
    }

    private static async Task<IResult> GetCompanyProfile(ISender sender, CancellationToken ct)
    {
        var profile = await sender.Send(new GetCompanyProfileQuery(), ct);
        return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
    }

    private static async Task<IResult> UpdateCompanyProfile(
        [FromBody] UpdateCompanyProfileRequest body,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCompanyProfileCommand(
            body.Name,
            body.Tagline,
            body.Email,
            body.ContactNumber,
            body.Website,
            body.StreetAddress,
            body.Barangay,
            body.City,
            body.Province,
            body.ZipCode,
            body.TaxId,
            body.BusinessPermitNo,
            body.IsVatRegistered,
            body.FacebookUrl,
            body.InstagramHandle,
            body.GCashNumber), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    /// <summary>
    /// PUT body for <c>UpdateCompanyProfile</c>. Mirrors
    /// <see cref="UpdateCompanyProfileCommand"/> but lives at the API surface
    /// so the wire shape is decoupled from the application command.
    /// </summary>
    private sealed record UpdateCompanyProfileRequest(
        string Name,
        string? Tagline,
        string Email,
        string ContactNumber,
        string? Website,
        string? StreetAddress,
        string? Barangay,
        string? City,
        string? Province,
        string? ZipCode,
        string? TaxId,
        string? BusinessPermitNo,
        bool IsVatRegistered,
        string? FacebookUrl,
        string? InstagramHandle,
        string? GCashNumber);
}
