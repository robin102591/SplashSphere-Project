using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.VehicleTypes.Commands.UpdateVehicleType;

public sealed record UpdateVehicleTypeCommand(string Id, string Name) : ICommand;
