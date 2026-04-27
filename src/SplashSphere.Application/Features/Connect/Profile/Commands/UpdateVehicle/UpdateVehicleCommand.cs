using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.UpdateVehicle;

/// <summary>
/// Edit a vehicle on the authenticated Connect user's profile. Only the owner
/// may update. Make/model may be reassigned; plate may be corrected.
/// </summary>
public sealed record UpdateVehicleCommand(
    string VehicleId,
    string MakeId,
    string ModelId,
    string PlateNumber,
    string? Color,
    int? Year) : ICommand<ConnectVehicleDto>;
