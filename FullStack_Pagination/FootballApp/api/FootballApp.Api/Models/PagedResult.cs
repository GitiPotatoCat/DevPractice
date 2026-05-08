namespace FootballApp.Api.Models;

/// <summary>
/// Generic paged response wrapper. Reusable for any list endpoint, not
/// just football clubs.
///
/// totalCount is critical for server-side pagination: the client needs
/// it to compute pageCount = ceil(totalCount / pageSize), which drives
/// the page-number controls in TanStack Table.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int TotalCount { get; init; }

    public int PageIndex { get; init; }

    public int PageSize { get; init; }

    /// <summary>
    /// Convenience derived value. Computed once on serialization.
    /// </summary>
    public int PageCount =>
        PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
}