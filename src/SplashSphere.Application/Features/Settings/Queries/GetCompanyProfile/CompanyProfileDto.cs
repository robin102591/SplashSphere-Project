namespace SplashSphere.Application.Features.Settings.Queries.GetCompanyProfile;

/// <summary>
/// Snapshot of a tenant's editable company-profile fields, sent to the
/// admin "/settings/company" page.
/// </summary>
/// <remarks>
/// `Address` is the legacy single-string address still consumed by other
/// handlers (Auth, Billing, Connect, Franchise). It is derived from the
/// structured fields when the profile is updated, so callers that read
/// either shape stay consistent.
/// </remarks>
public sealed record CompanyProfileDto(
    // Identity
    string Name,
    string? Tagline,

    // Contact
    string Email,
    string ContactNumber,
    string? Website,

    // Address (legacy single-string + structured)
    string Address,
    string? StreetAddress,
    string? Barangay,
    string? City,
    string? Province,
    string? ZipCode,

    // Tax & registration
    string? TaxId,
    string? BusinessPermitNo,
    bool IsVatRegistered,

    // Social & payment
    string? FacebookUrl,
    string? InstagramHandle,
    string? GCashNumber,

    // Logo (slice 3) — never modified by UpdateCompanyProfile; managed by
    // POST /settings/company/logo and DELETE /settings/company/logo.
    string? LogoUrl,
    string? LogoThumbnailUrl,
    string? LogoIconUrl);
