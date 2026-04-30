using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.PosStations.Commands.UpdatePosStation;

/// <summary>Updates a station's name and active flag. Id comes from the route.</summary>
public sealed record UpdatePosStationCommand(
    string Id,
    string Name,
    bool IsActive) : ICommand;
