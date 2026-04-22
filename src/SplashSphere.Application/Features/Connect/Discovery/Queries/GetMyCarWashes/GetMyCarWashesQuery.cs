using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Discovery.Queries.GetMyCarWashes;

/// <summary>
/// List all tenants the authenticated Connect user has linked to (joined).
/// Returns an empty list for unauthenticated callers.
/// </summary>
public sealed record GetMyCarWashesQuery : IQuery<IReadOnlyList<MyCarWashDto>>;
