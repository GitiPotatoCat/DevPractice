using System.ComponentModel.DataAnnotations;

namespace FootballApp.Api.Models;

/// <summary>
/// Inbound request body for any paged/sorted/filtered list endpoint.
///
/// Field names mirror TanStack Table's table state exactly so that the
/// Angular layer can post the table state with no translation:
///   pageIndex, pageSize, sort, filters, globalFilter.
/// </summary>
public sealed class PagedQueryRequest
{
    /// <summary>
    /// 0-based page index. TanStack uses 0-based; we keep it consistent.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "pageIndex must be >= 0.")]
    public int PageIndex { get; set; } = 0;

    /// <summary>
    /// Page size. Capped server-side to prevent abuse via huge pages.
    /// </summary>
    [Range(1, 200, ErrorMessage = "pageSize must be between 1 and 200.")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Multi-column sort. Order matters: first item is primary sort,
    /// second is tiebreaker, and so on.
    /// </summary>
    public List<SortDescriptor> Sort { get; set; } = new();

    /// <summary>
    /// Per-column filters. Multiple filters are AND-ed together.
    /// </summary>
    public List<ColumnFilterDescriptor> Filters { get; set; } = new();

    /// <summary>
    /// Free-text search across a fixed set of searchable columns.
    /// Empty / null = no global search.
    /// </summary>
    public string? GlobalFilter { get; set; }
}