namespace SplashSphere.Application.Features.Search;

public sealed record GlobalSearchResultDto(
    List<SearchHitDto> Customers,
    List<SearchHitDto> Employees,
    List<SearchHitDto> Transactions,
    List<SearchHitDto> Vehicles,
    List<SearchHitDto> Services,
    List<SearchHitDto> Merchandise);

public sealed record SearchHitDto(
    string Id,
    string Title,
    string? Subtitle,
    string Category);
