using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Application.Features.Search.Queries.GlobalSearch;

public sealed record GlobalSearchQuery(
    string Q,
    int Limit = 5) : IQuery<GlobalSearchResultDto>;
