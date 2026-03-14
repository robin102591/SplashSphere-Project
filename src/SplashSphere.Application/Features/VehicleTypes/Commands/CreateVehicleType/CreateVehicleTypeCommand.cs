using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.VehicleTypes.Commands.CreateVehicleType;

public sealed record CreateVehicleTypeCommand(string Name) : ICommand<string>;
