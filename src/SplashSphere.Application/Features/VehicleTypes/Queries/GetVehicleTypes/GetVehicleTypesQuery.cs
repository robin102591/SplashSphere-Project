using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.VehicleTypes.Queries.GetVehicleTypes;

public sealed record GetVehicleTypesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IQuery<PagedResult<VehicleTypeDto>>;
