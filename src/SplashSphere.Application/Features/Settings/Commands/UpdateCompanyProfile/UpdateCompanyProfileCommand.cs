using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Settings.Commands.UpdateCompanyProfile;

/// <summary>
/// Updates the editable fields on the current tenant's company profile.
/// The tenant ID itself is resolved from the JWT — never accepted as input.
/// </summary>
public sealed record UpdateCompanyProfileCommand(
    // Identity
    string Name,
    string? Tagline,

    // Contact
    string Email,
    string ContactNumber,
    string? Website,

    // Address (structured — legacy Address string is auto-derived)
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
    string? GCashNumber) : ICommand;
