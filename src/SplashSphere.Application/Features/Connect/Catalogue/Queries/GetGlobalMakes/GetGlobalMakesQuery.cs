using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Catalogue.Queries.GetGlobalMakes;

/// <summary>
/// List all active global vehicle makes for the Connect app's picker.
/// Ordered by DisplayOrder then Name.
/// </summary>
public sealed record GetGlobalMakesQuery : IQuery<IReadOnlyList<GlobalMakeDto>>;
