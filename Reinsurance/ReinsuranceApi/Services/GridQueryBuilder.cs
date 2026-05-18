using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Text;
using ReinsuranceApi.Dtos;

namespace ReinsuranceApi.Services;

/// <summary>
/// Builds dynamic IQueryable for paging, sorting, filtering (incl. EMF) and grouping.
/// Works against any IQueryable&lt;T&gt; — fully translated to SQL by EF Core.
///
/// Field names refer to C# property names. EF [Column("...")] attributes
/// transparently map them to physical SQL columns (e.g. Status → Treaty_Status).
/// </summary>
public static class GridQueryBuilder
{
    /// <summary>Whitelist of properties the client is allowed to query against.</summary>
    private static readonly HashSet<string> _allowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "TreatyId","TreatyCode","TreatyName","CedingCompany","Reinsurer",
        "TreatyType","LineOfBusiness","Country","Currency",
        "SumInsured","PremiumAmount","CommissionPct","RetentionPct",
        "InceptionDate","ExpiryDate","Status","UnderwriterName","CreatedDate"
    };

    /// <summary>Properties scanned by the GlobalSearch box.</summary>
    private static readonly string[] _searchableStringFields =
    {
        "TreatyCode","TreatyName","CedingCompany","Reinsurer",
        "TreatyType","LineOfBusiness","Country","Currency","Status","UnderwriterName"
    };

    // =========================================================
    // Filters (Contains, EMF/Equals, NotEquals, Starts/Ends, comparisons, Between, In)
    // =========================================================
    public static IQueryable<T> ApplyFilters<T>(IQueryable<T> source, List<FilterDescriptor> filters)
    {
        if (filters == null || filters.Count == 0) return source;

        var parameters = new List<object?>();
        var sb = new StringBuilder();

        foreach (var f in filters)
        {
            if (string.IsNullOrWhiteSpace(f.Field) || !_allowedFields.Contains(f.Field))
                continue;

            var prop = typeof(T).GetProperty(f.Field);
            if (prop is null) continue;

            var isString = prop.PropertyType == typeof(string);

            if (sb.Length > 0) sb.Append(" AND ");

            switch (f.Operator)
            {
                case FilterOperator.Contains:
                    sb.Append($"({f.Field} != null && {f.Field}.ToLower().Contains(@{parameters.Count}))");
                    parameters.Add((f.Value?.ToString() ?? string.Empty).ToLower());
                    break;

                case FilterOperator.Equals: // EMF
                    if (isString)
                    {
                        sb.Append($"({f.Field} != null && {f.Field}.ToLower() == @{parameters.Count})");
                        parameters.Add((f.Value?.ToString() ?? string.Empty).ToLower());
                    }
                    else
                    {
                        sb.Append($"({f.Field} == @{parameters.Count})");
                        parameters.Add(ConvertValue(f.Value, typeof(T), f.Field));
                    }
                    break;

                case FilterOperator.NotEquals:
                    if (isString)
                    {
                        sb.Append($"({f.Field} == null || {f.Field}.ToLower() != @{parameters.Count})");
                        parameters.Add((f.Value?.ToString() ?? string.Empty).ToLower());
                    }
                    else
                    {
                        sb.Append($"({f.Field} != @{parameters.Count})");
                        parameters.Add(ConvertValue(f.Value, typeof(T), f.Field));
                    }
                    break;

                case FilterOperator.StartsWith:
                    sb.Append($"({f.Field} != null && {f.Field}.ToLower().StartsWith(@{parameters.Count}))");
                    parameters.Add((f.Value?.ToString() ?? string.Empty).ToLower());
                    break;

                case FilterOperator.EndsWith:
                    sb.Append($"({f.Field} != null && {f.Field}.ToLower().EndsWith(@{parameters.Count}))");
                    parameters.Add((f.Value?.ToString() ?? string.Empty).ToLower());
                    break;

                case FilterOperator.GreaterThan:
                    sb.Append($"({f.Field} > @{parameters.Count})");
                    parameters.Add(ConvertValue(f.Value, typeof(T), f.Field));
                    break;

                case FilterOperator.GreaterThanOrEqual:
                    sb.Append($"({f.Field} >= @{parameters.Count})");
                    parameters.Add(ConvertValue(f.Value, typeof(T), f.Field));
                    break;

                case FilterOperator.LessThan:
                    sb.Append($"({f.Field} < @{parameters.Count})");
                    parameters.Add(ConvertValue(f.Value, typeof(T), f.Field));
                    break;

                case FilterOperator.LessThanOrEqual:
                    sb.Append($"({f.Field} <= @{parameters.Count})");
                    parameters.Add(ConvertValue(f.Value, typeof(T), f.Field));
                    break;

                case FilterOperator.Between:
                    sb.Append($"({f.Field} >= @{parameters.Count} && {f.Field} <= @{parameters.Count + 1})");
                    parameters.Add(ConvertValue(f.Value, typeof(T), f.Field));
                    parameters.Add(ConvertValue(f.Value2, typeof(T), f.Field));
                    break;

                case FilterOperator.In: // multi-value EMF — expand as OR chain for reliable SQL translation
                    if (f.Values is { Count: > 0 })
                    {
                        var orParts = new List<string>();
                        foreach (var v in f.Values)
                        {
                            if (isString)
                            {
                                orParts.Add($"({f.Field} != null && {f.Field}.ToLower() == @{parameters.Count})");
                                parameters.Add((v?.ToString() ?? string.Empty).ToLower());
                            }
                            else
                            {
                                orParts.Add($"({f.Field} == @{parameters.Count})");
                                parameters.Add(ConvertValue(v, typeof(T), f.Field));
                            }
                        }
                        sb.Append('(').Append(string.Join(" OR ", orParts)).Append(')');
                    }
                    break;
            }
        }

        return sb.Length == 0 ? source : source.Where(sb.ToString(), parameters.ToArray());
    }

    // =========================================================
    // Global search — cross-column "contains" with per-column placeholders
    // =========================================================
    public static IQueryable<T> ApplyGlobalSearch<T>(IQueryable<T> source, string? term)
    {
        if (string.IsNullOrWhiteSpace(term)) return source;

        var lowered = term.ToLower();
        var parameters = new List<object?>();
        var parts = new List<string>();

        foreach (var f in _searchableStringFields)
        {
            if (typeof(T).GetProperty(f) is null) continue;

            parts.Add($"({f} != null && {f}.ToLower().Contains(@{parameters.Count}))");
            parameters.Add(lowered);
        }

        return parts.Count == 0
            ? source
            : source.Where(string.Join(" OR ", parts), parameters.ToArray());
    }

    // =========================================================
    // Sorting
    // =========================================================
    public static IQueryable<T> ApplySorting<T>(IQueryable<T> source, List<SortDescriptor> sorts)
    {
        if (sorts == null || sorts.Count == 0)
            return source.OrderBy("TreatyId asc");

        var ordering = string.Join(",", sorts
            .Where(s => !string.IsNullOrWhiteSpace(s.Field) && _allowedFields.Contains(s.Field))
            .Select(s => $"{s.Field} {(s.Direction?.ToLower() == "desc" ? "desc" : "asc")}"));

        return string.IsNullOrWhiteSpace(ordering)
            ? source.OrderBy("TreatyId asc")
            : source.OrderBy(ordering);
    }

    // =========================================================
    // Grouping — returns per-key counts for each requested column
    // =========================================================
    public static async Task<List<Dtos.GroupResult>> BuildGroupsAsync<T>(IQueryable<T> source, List<GroupDescriptor> groups)
    {
        var results = new List<Dtos.GroupResult>();
        if (groups == null || groups.Count == 0) return results;

        foreach (var g in groups.Where(g => !string.IsNullOrWhiteSpace(g.Field) && _allowedFields.Contains(g.Field)))
        {
            var grouped = await source
                .GroupBy(g.Field, "it")
                .Select("new (Key as Key, Count() as Count)")
                .OrderBy(g.Direction?.ToLower() == "desc" ? "Key desc" : "Key asc")
                .ToDynamicListAsync();

            foreach (dynamic row in grouped)
            {
                results.Add(new Dtos.GroupResult
                {
                    Field = g.Field,
                    Key = row.Key?.ToString(),
                    Count = (int)row.Count
                });
            }
        }

        return results;
    }

    // =========================================================
    // Helpers
    // =========================================================
    private static object? ConvertValue(object? value, Type entityType, string fieldName)
    {
        if (value is null) return null;

        var prop = entityType.GetProperty(fieldName);
        if (prop is null) return value;

        var target = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        try
        {
            // JSON numbers can arrive as JsonElement/double; normalise via invariant string parse
            return Convert.ChangeType(value.ToString(), target, CultureInfo.InvariantCulture);
        }
        catch
        {
            return value;
        }
    }
}