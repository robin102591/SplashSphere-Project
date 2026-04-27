using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.AddVehicle;

/// <summary>
/// Register a new vehicle on the authenticated Connect user's profile.
/// Type/size are intentionally not captured here — the cashier classifies
/// the vehicle on arrival.
/// </summary>
public sealed record AddVehicleCommand(
    string MakeId,
    string ModelId,
    string PlateNumber,
    string? Color,
    int? Year) : ICommand<ConnectVehicleDto>;
