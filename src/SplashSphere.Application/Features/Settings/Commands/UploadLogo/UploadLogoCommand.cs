using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Settings.Commands.UploadLogo;

/// <summary>
/// Resizes an uploaded image into 3 PNG variants (500/200/80px), uploads
/// each to R2 under deterministic keys, and writes the public URLs onto
/// the current tenant's <c>Tenant</c> row.
/// <para>
/// The file payload is passed as a <see cref="Stream"/> + the original
/// content type. Validation (size, mime-type) lives in the validator.
/// </para>
/// </summary>
public sealed record UploadLogoCommand(
    Stream Content,
    string ContentType,
    long ContentLength) : ICommand<UploadLogoResult>;

/// <summary>Returns the three URLs the renderer should consume.</summary>
public sealed record UploadLogoResult(
    string LogoUrl,
    string LogoThumbnailUrl,
    string LogoIconUrl);
