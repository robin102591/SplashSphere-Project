using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Cars.Queries.GetCarById;

public sealed record GetCarByIdQuery(string Id) : IQuery<CarDto>;
