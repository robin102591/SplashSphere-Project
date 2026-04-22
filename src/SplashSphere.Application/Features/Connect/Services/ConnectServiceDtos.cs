namespace SplashSphere.Application.Features.Connect.Services;

/// <summary>
/// A tenant service as shown to a Connect customer. The shape of
/// <see cref="Price"/>/<see cref="PriceMin"/>/<see cref="PriceMax"/> depends on
/// <see cref="PriceMode"/>:
/// <list type="bullet">
///   <item><c>"exact"</c> — the customer's vehicle is classified at this tenant
///     so only <see cref="Price"/> is populated.</item>
///   <item><c>"estimate"</c> — the vehicle is unclassified; <see cref="PriceMin"/>
///     and <see cref="PriceMax"/> span the full ServicePricing matrix (plus the
///     service BasePrice fallback), with <see cref="Price"/> left null.</item>
/// </list>
/// </summary>
public sealed record ConnectServicePriceDto(
    string ServiceId,
    string Name,
    string? Description,
    string PriceMode,
    decimal? Price,
    decimal? PriceMin,
    decimal? PriceMax);

/// <summary>
/// The response wrapper for the services + pricing query — includes the pricing
/// mode applied to the whole list so the UI can render a single banner.
/// </summary>
public sealed record ConnectServicesWithPricingDto(
    string TenantId,
    string VehicleId,
    string PriceMode,
    IReadOnlyList<ConnectServicePriceDto> Services);
