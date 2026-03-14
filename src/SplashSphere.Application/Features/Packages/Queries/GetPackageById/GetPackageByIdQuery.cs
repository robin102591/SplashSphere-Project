using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Packages.Queries.GetPackageById;

public sealed record GetPackageByIdQuery(string Id) : IQuery<PackageDetailDto>;
