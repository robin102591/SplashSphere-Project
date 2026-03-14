using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Services.Queries.GetServiceById;

/// <summary>Returns a service with its full pricing and commission matrices.</summary>
public sealed record GetServiceByIdQuery(string Id) : IQuery<ServiceDetailDto>;
