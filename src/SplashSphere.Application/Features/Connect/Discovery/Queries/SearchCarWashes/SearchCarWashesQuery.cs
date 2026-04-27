using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Discovery.Queries.SearchCarWashes;

/// <summary>
/// Discovery query for the Connect app's "Find a Car Wash" page.
/// <para>
/// Returns one entry per publicly listed branch across all tenants on a Connect-eligible
/// plan (Trial, Growth, Enterprise) with <c>IsActive = true</c> and
/// <c>BookingSetting.ShowInPublicDirectory = true</c>.
/// </para>
/// <para>
/// <c>Search</c> matches tenant name, branch name, or branch address (case-insensitive).
/// <c>Latitude</c>/<c>Longitude</c> (optional) enable distance sorting using the
/// Haversine formula — when supplied, results with known coords sort nearest-first.
/// </para>
/// </summary>
public sealed record SearchCarWashesQuery(
    string? Search = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    int Take = 50) : IQuery<IReadOnlyList<CarWashListItemDto>>;
