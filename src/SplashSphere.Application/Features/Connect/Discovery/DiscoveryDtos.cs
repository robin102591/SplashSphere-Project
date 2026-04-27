namespace SplashSphere.Application.Features.Connect.Discovery;

/// <summary>
/// A car-wash branch surfaced in the Connect app's discovery list. One row per
/// branch — tenants with multiple branches appear multiple times.
/// <para>
/// <c>DistanceKm</c> is the straight-line km from the caller's coordinates when
/// they were supplied, otherwise <c>null</c>. <c>OpenTime</c>/<c>CloseTime</c>
/// are local Manila HH:mm strings, or <c>null</c> when booking is disabled.
/// </para>
/// </summary>
public sealed record CarWashListItemDto(
    string TenantId,
    string TenantName,
    string BranchId,
    string BranchName,
    string Address,
    string ContactNumber,
    decimal? Latitude,
    decimal? Longitude,
    double? DistanceKm,
    string? OpenTime,
    string? CloseTime,
    bool IsBookingEnabled,
    bool IsJoined);

/// <summary>
/// Detail view for a single tenant in the Connect app — tenant overview plus
/// all publicly listed branches and service catalogue.
/// </summary>
public sealed record CarWashDetailDto(
    string TenantId,
    string TenantName,
    string Email,
    string ContactNumber,
    string Address,
    bool IsJoined,
    IReadOnlyList<CarWashBranchDto> Branches,
    IReadOnlyList<CarWashServiceDto> Services);

/// <summary>A publicly listed branch belonging to a tenant.</summary>
public sealed record CarWashBranchDto(
    string Id,
    string Name,
    string Address,
    string ContactNumber,
    decimal? Latitude,
    decimal? Longitude,
    string? OpenTime,
    string? CloseTime,
    bool IsBookingEnabled);

/// <summary>A service offered by the tenant (without per-vehicle pricing — that's a separate query).</summary>
public sealed record CarWashServiceDto(
    string Id,
    string Name,
    string? Description,
    decimal BasePrice);

/// <summary>A tenant the authenticated Connect user has joined/linked to.</summary>
public sealed record MyCarWashDto(
    string TenantId,
    string TenantName,
    string Address,
    DateTime LinkedAt);
