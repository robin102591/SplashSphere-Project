using SplashSphere.Application.Features.Settings.Queries.GetDisplaySetting;

namespace SplashSphere.Application.Features.Display.Queries.GetDisplayConfig;

/// <summary>
/// Combined render config for the customer-facing display device. The display
/// app fetches this once at boot so it has both the configurable toggles and
/// the tenant branding it needs to render — settings + a curated subset of
/// company profile that's safe to show on a customer-facing screen.
/// <para>
/// We deliberately do NOT include private branding fields like the tax ID or
/// business permit — those belong on the receipt, not on a public-facing
/// display.
/// </para>
/// </summary>
public sealed record DisplayConfigDto(
    DisplaySettingDto Settings,
    DisplayBrandingDto Branding);

/// <summary>
/// Customer-display-safe subset of the tenant's company profile.
/// </summary>
public sealed record DisplayBrandingDto(
    string BusinessName,
    string? Tagline,
    string? LogoUrl,           // 200px thumbnail — sized for display headers
    string? FacebookUrl,
    string? InstagramHandle,
    string? GCashNumber,
    string? GCashQrUrl);        // QR image; null until slice 3 of company profile wires it
