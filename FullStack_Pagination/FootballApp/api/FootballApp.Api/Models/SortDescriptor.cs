namespace FootballApp.Api.Models;

/// <summary>
/// One sort instruction. Mirrors TanStack Table's ColumnSort:
///   { id: "clubName", desc: false }
/// </summary>
public sealed class SortDescriptor
{
    /// <summary>
    /// Column id as known by the client (e.g. "clubName", "titlesWon").
    /// The service layer will map this to a real entity property.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// True for descending, false for ascending.
    /// </summary>
    public bool Desc { get; set; }
}