using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.VehicleTypes.Queries.GetVehicleTypeById;

public sealed record GetVehicleTypeByIdQuery(string Id) : IQuery<VehicleTypeDto>;
