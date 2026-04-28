using MediatR;
using Microsoft.AspNetCore.Mvc;
using SplashSphere.API.Extensions;
using SplashSphere.Application.Features.Settings.Commands.DeleteLogo;
using SplashSphere.Application.Features.Settings.Commands.UpdateCompanyProfile;
using SplashSphere.Application.Features.Settings.Commands.UpdateReceiptSetting;
using SplashSphere.Application.Features.Settings.Commands.UploadLogo;
using SplashSphere.Application.Features.Settings.Queries.GetCompanyProfile;
using SplashSphere.Application.Features.Settings.Queries.GetReceiptSetting;
using SplashSphere.Domain.Enums;

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

        // ── Receipt designer ──────────────────────────────────────────────────
        group.MapGet("/receipt", GetReceiptSetting)
            .WithName("GetReceiptSetting")
            .WithSummary("Get receipt-design settings (tenant default; branchId reserved for slice 4)");

        group.MapPut("/receipt", UpdateReceiptSetting)
            .WithName("UpdateReceiptSetting")
            .WithSummary("Update receipt-design settings (upserts the tenant default)");

        // ── Company logo (multipart upload) ───────────────────────────────────
        group.MapPost("/company/logo", UploadLogo)
            .WithName("UploadCompanyLogo")
            .WithSummary("Upload a logo image (multipart/form-data, field name 'file'). Resized to 500/200/80px PNG variants and stored in R2.")
            .DisableAntiforgery();

        group.MapDelete("/company/logo", DeleteLogo)
            .WithName("DeleteCompanyLogo")
            .WithSummary("Remove the current tenant's logo");

        return app;
    }

    // ── Company profile handlers ──────────────────────────────────────────────

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

    // ── Receipt setting handlers ──────────────────────────────────────────────

    private static async Task<IResult> GetReceiptSetting(
        [FromQuery] string? branchId,
        ISender sender,
        CancellationToken ct)
    {
        var setting = await sender.Send(new GetReceiptSettingQuery(branchId), ct);
        return TypedResults.Ok(setting);
    }

    private static async Task<IResult> UpdateReceiptSetting(
        [FromBody] UpdateReceiptSettingRequest body,
        [FromQuery] string? branchId,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateReceiptSettingCommand(
            branchId,
            // Header
            body.ShowLogo,
            body.LogoSize,
            body.LogoPosition,
            body.ShowBusinessName,
            body.ShowTagline,
            body.ShowBranchName,
            body.ShowBranchAddress,
            body.ShowBranchContact,
            body.ShowTIN,
            body.CustomHeaderText,
            // Body
            body.ShowServiceDuration,
            body.ShowEmployeeNames,
            body.ShowVehicleInfo,
            body.ShowDiscountBreakdown,
            body.ShowTaxLine,
            body.ShowTransactionNumber,
            body.ShowDateTime,
            body.ShowCashierName,
            // Customer
            body.ShowCustomerName,
            body.ShowCustomerPhone,
            body.ShowLoyaltyPointsEarned,
            body.ShowLoyaltyBalance,
            body.ShowLoyaltyTier,
            // Footer
            body.ThankYouMessage,
            body.PromoText,
            body.ShowSocialMedia,
            body.ShowGCashQr,
            body.ShowGCashNumber,
            body.CustomFooterText,
            // Format
            body.ReceiptWidth,
            body.FontSize,
            body.AutoCutPaper), ct);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Logo upload handlers ──────────────────────────────────────────────────

    private static async Task<IResult> UploadLogo(
        HttpRequest request,
        ISender sender,
        CancellationToken ct)
    {
        if (!request.HasFormContentType)
            return TypedResults.Problem(new ProblemDetails
            {
                Title  = "VALIDATION",
                Detail = "Expected multipart/form-data.",
                Status = StatusCodes.Status400BadRequest,
            });

        var form = await request.ReadFormAsync(ct);
        var file = form.Files.GetFile("file");

        if (file is null || file.Length == 0)
            return TypedResults.Problem(new ProblemDetails
            {
                Title  = "VALIDATION",
                Detail = "Form field 'file' is required.",
                Status = StatusCodes.Status400BadRequest,
            });

        await using var stream = file.OpenReadStream();
        var result = await sender.Send(new UploadLogoCommand(
            stream, file.ContentType, file.Length), ct);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> DeleteLogo(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteLogoCommand(), ct);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    // ── Request bodies ────────────────────────────────────────────────────────

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

    /// <summary>
    /// PUT body for <c>UpdateReceiptSetting</c>. The branchId comes from the
    /// query string (so the URL alone identifies which row is being upserted)
    /// rather than the body.
    /// </summary>
    private sealed record UpdateReceiptSettingRequest(
        // Header
        bool ShowLogo,
        LogoSize LogoSize,
        LogoPosition LogoPosition,
        bool ShowBusinessName,
        bool ShowTagline,
        bool ShowBranchName,
        bool ShowBranchAddress,
        bool ShowBranchContact,
        bool ShowTIN,
        string? CustomHeaderText,
        // Body
        bool ShowServiceDuration,
        bool ShowEmployeeNames,
        bool ShowVehicleInfo,
        bool ShowDiscountBreakdown,
        bool ShowTaxLine,
        bool ShowTransactionNumber,
        bool ShowDateTime,
        bool ShowCashierName,
        // Customer
        bool ShowCustomerName,
        bool ShowCustomerPhone,
        bool ShowLoyaltyPointsEarned,
        bool ShowLoyaltyBalance,
        bool ShowLoyaltyTier,
        // Footer
        string ThankYouMessage,
        string? PromoText,
        bool ShowSocialMedia,
        bool ShowGCashQr,
        bool ShowGCashNumber,
        string? CustomFooterText,
        // Format
        ReceiptWidth ReceiptWidth,
        ReceiptFontSize FontSize,
        bool AutoCutPaper);
}
