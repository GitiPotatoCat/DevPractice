namespace FootballApp.Api.Models;

/// <summary>
/// One column filter. Mirrors TanStack Table's ColumnFilter:
///   { id: "country", value: "Spain" }
///
/// Value is intentionally typed as object? because TanStack lets the
/// filter value be any JSON-serializable thing (string, number, bool,
/// array, range object). The service layer interprets it per column.
/// </summary>
public sealed class ColumnFilterDescriptor
{
    public string Id { get; set; } = string.Empty;

    public object? Value { get; set; }
}