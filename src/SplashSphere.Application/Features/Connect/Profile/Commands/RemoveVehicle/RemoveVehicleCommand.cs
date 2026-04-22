using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Profile.Commands.RemoveVehicle;

/// <summary>
/// Delete a vehicle from the authenticated Connect user's profile. Only the
/// owner may delete.
/// </summary>
public sealed record RemoveVehicleCommand(string VehicleId) : ICommand;
