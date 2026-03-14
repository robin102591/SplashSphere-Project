using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Cars.Commands.CreateCar;

public sealed record CreateCarCommand(
    string PlateNumber,
    string VehicleTypeId,
    string SizeId,
    string? CustomerId,
    string? MakeId,
    string? ModelId,
    string? Color,
    int? Year,
    string? Notes) : ICommand<string>;
