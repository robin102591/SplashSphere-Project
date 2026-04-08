using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Franchise.Queries.GetBenchmarks;

public sealed record GetBenchmarksQuery : IQuery<IReadOnlyList<FranchiseBenchmarkDto>>;
