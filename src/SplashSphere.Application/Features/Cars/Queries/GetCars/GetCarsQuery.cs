using SplashSphere.Application.Common.Interfaces;
using SplashSphere.SharedKernel.Results;

namespace SplashSphere.Application.Features.Cars.Queries.GetCars;

public sealed record GetCarsQuery(
    int Page = 1,
    int PageSize = 20,
    string? CustomerId = null,
    string? Search = null) : IQuery<PagedResult<CarDto>>;
