namespace SplashSphere.Application.Features.Connect.Profile;

/// <summary>The authenticated customer's global profile (no per-tenant data).</summary>
public sealed record ConnectProfileDto(
    string Id,
    string Phone,
    string Name,
    string? Email,
    string? AvatarUrl,
    DateTime CreatedAt,
    IReadOnlyList<ConnectVehicleDto> Vehicles);

/// <summary>
/// Vehicle as stored in the Connect app. Deliberately lacks type/size — those are
/// assigned per-tenant by cashiers on the customer's first physical visit.
/// </summary>
public sealed record ConnectVehicleDto(
    string Id,
    string MakeId,
    string MakeName,
    string ModelId,
    string ModelName,
    string PlateNumber,
    string? Color,
    int? Year);
