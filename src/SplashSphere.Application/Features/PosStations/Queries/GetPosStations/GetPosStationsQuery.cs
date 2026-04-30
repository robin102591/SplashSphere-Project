using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.PosStations.Queries.GetPosStations;

/// <summary>
/// Returns all stations for a branch. Stations are few per branch (typically
/// 1-3) so this query is unpaginated by design.
/// </summary>
public sealed record GetPosStationsQuery(string BranchId) : IQuery<IReadOnlyList<PosStationDto>>;
