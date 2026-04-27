using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Connect.Catalogue.Queries.GetGlobalModels;

/// <summary>
/// List active global vehicle models for a given make.
/// Ordered by DisplayOrder then Name.
/// </summary>
public sealed record GetGlobalModelsQuery(string MakeId) : IQuery<IReadOnlyList<GlobalModelDto>>;
