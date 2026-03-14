using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.VehicleTypes.Commands.ToggleVehicleTypeStatus;

public sealed record ToggleVehicleTypeStatusCommand(string Id) : ICommand;
