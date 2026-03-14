namespace SplashSphere.SharedKernel.Results;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    /// <summary>Creates a paged result from a pre-fetched page of items.</summary>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int page, int pageSize) =>
        new() { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };

    /// <summary>Projects each item using <paramref name="map"/>.</summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        PagedResult<TOut>.Create([.. Items.Select(map)], TotalCount, Page, PageSize);
}
