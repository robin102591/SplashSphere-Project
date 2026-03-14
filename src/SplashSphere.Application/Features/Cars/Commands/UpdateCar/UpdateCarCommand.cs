using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Cars.Commands.UpdateCar;

/// <summary>
/// Updates car details. PlateNumber and CustomerId are immutable after creation.
/// VehicleTypeId and SizeId can be corrected if entered wrong.
/// </summary>
public sealed record UpdateCarCommand(
    string Id,
    string VehicleTypeId,
    string SizeId,
    string? MakeId,
    string? ModelId,
    string? Color,
    int? Year,
    string? Notes) : ICommand;
