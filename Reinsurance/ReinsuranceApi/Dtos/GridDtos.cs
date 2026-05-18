namespace ReinsuranceApi.Dtos;

/// <summary>
/// Filter operators supported by the grid.
/// EMF = Exact Matching Filter (case-insensitive whole value).
/// </summary>
public enum FilterOperator
{
    Contains = 0,
    Equals = 1,         // EMF — exact match
    NotEquals = 2,
    StartsWith = 3,
    EndsWith = 4,
    GreaterThan = 5,
    GreaterThanOrEqual = 6,
    LessThan = 7,
    LessThanOrEqual = 8,
    Between = 9,
    In = 10             // Multi-value EMF (IN clause)
}

public class FilterDescriptor
{
    public string Field { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; } = FilterOperator.Contains;
    public object? Value { get; set; }
    public object? Value2 { get; set; }              // for Between
    public List<object>? Values { get; set; }        // for In (multi-EMF)
}

public class SortDescriptor
{
    public string Field { get; set; } = string.Empty;
    public string Direction { get; set; } = "asc";   // asc | desc
}

public class GroupDescriptor
{
    public string Field { get; set; } = string.Empty;
    public string Direction { get; set; } = "asc";
}

public class GridRequest
{
    public int PageIndex { get; set; } = 0;          // zero-based
    public int PageSize { get; set; } = 25;
    public List<SortDescriptor> Sorts { get; set; } = new();
    public List<FilterDescriptor> Filters { get; set; } = new();
    public List<GroupDescriptor> Groups { get; set; } = new();
    public string? GlobalSearch { get; set; }        // optional cross-column search
}

public class GroupResult
{
    public string Field { get; set; } = string.Empty;
    public string? Key { get; set; }
    public int Count { get; set; }
}

public class GridResponse<T>
{
    public List<T> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public List<GroupResult>? Groups { get; set; }
}