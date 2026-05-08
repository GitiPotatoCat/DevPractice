using System.Text.Json;
using FootballApp.Api.Data;
using FootballApp.Api.Dtos;
using FootballApp.Api.Entities;
using FootballApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FootballApp.Api.Services;

public sealed class FootballClubService : IFootballClubService
{
    private readonly FootballDbContext _db;
    private readonly ILogger<FootballClubService> _logger;

    public FootballClubService(
        FootballDbContext db,
        ILogger<FootballClubService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ---- Column allowlists ------------------------------------------------
    //
    // These dictionaries are the ONLY place where a client-supplied column id
    // becomes a real entity property. Any id not present in the dictionary is
    // silently ignored. This is what protects us from:
    //   - SQL errors from typos
    //   - Sorting by columns we never intended to expose
    //   - Filtering on PII-ish columns we forget about later

    private static readonly IReadOnlyDictionary<string, Func<IQueryable<FootballClub>, bool, IOrderedQueryable<FootballClub>>>
        SortMap = new Dictionary<string, Func<IQueryable<FootballClub>, bool, IOrderedQueryable<FootballClub>>>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = (q, desc) => desc ? q.OrderByDescending(c => c.Id) : q.OrderBy(c => c.Id),
            ["clubName"] = (q, desc) => desc ? q.OrderByDescending(c => c.ClubName) : q.OrderBy(c => c.ClubName),
            ["country"] = (q, desc) => desc ? q.OrderByDescending(c => c.Country) : q.OrderBy(c => c.Country),
            ["league"] = (q, desc) => desc ? q.OrderByDescending(c => c.League) : q.OrderBy(c => c.League),
            ["foundedYear"] = (q, desc) => desc ? q.OrderByDescending(c => c.FoundedYear) : q.OrderBy(c => c.FoundedYear),
            ["stadium"] = (q, desc) => desc ? q.OrderByDescending(c => c.Stadium) : q.OrderBy(c => c.Stadium),
            ["titlesWon"] = (q, desc) => desc ? q.OrderByDescending(c => c.TitlesWon) : q.OrderBy(c => c.TitlesWon),
        };

    private static readonly IReadOnlyDictionary<string, Func<IQueryable<FootballClub>, bool, IOrderedQueryable<FootballClub>>>
        ThenSortMap = new Dictionary<string, Func<IQueryable<FootballClub>, bool, IOrderedQueryable<FootballClub>>>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = (q, desc) => desc ? ((IOrderedQueryable<FootballClub>)q).ThenByDescending(c => c.Id) : ((IOrderedQueryable<FootballClub>)q).ThenBy(c => c.Id),
            ["clubName"] = (q, desc) => desc ? ((IOrderedQueryable<FootballClub>)q).ThenByDescending(c => c.ClubName) : ((IOrderedQueryable<FootballClub>)q).ThenBy(c => c.ClubName),
            ["country"] = (q, desc) => desc ? ((IOrderedQueryable<FootballClub>)q).ThenByDescending(c => c.Country) : ((IOrderedQueryable<FootballClub>)q).ThenBy(c => c.Country),
            ["league"] = (q, desc) => desc ? ((IOrderedQueryable<FootballClub>)q).ThenByDescending(c => c.League) : ((IOrderedQueryable<FootballClub>)q).ThenBy(c => c.League),
            ["foundedYear"] = (q, desc) => desc ? ((IOrderedQueryable<FootballClub>)q).ThenByDescending(c => c.FoundedYear) : ((IOrderedQueryable<FootballClub>)q).ThenBy(c => c.FoundedYear),
            ["stadium"] = (q, desc) => desc ? ((IOrderedQueryable<FootballClub>)q).ThenByDescending(c => c.Stadium) : ((IOrderedQueryable<FootballClub>)q).ThenBy(c => c.Stadium),
            ["titlesWon"] = (q, desc) => desc ? ((IOrderedQueryable<FootballClub>)q).ThenByDescending(c => c.TitlesWon) : ((IOrderedQueryable<FootballClub>)q).ThenBy(c => c.TitlesWon),
        };

    public async Task<PagedResult<FootballClubDto>> QueryAsync(
        PagedQueryRequest request,
        CancellationToken cancellationToken)
    {
        // We start from a NoTracking query because this is a pure read.
        // No-tracking is faster and uses less memory.
        IQueryable<FootballClub> query = _db.FootballClubs.AsNoTracking();

        query = ApplyGlobalFilter(query, request.GlobalFilter);
        query = ApplyColumnFilters(query, request.Filters);

        // Always count BEFORE paging - that's the whole point of pagination.
        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, request.Sort);
        query = ApplyPaging(query, request.PageIndex, request.PageSize);

        // Project to DTO at the database level so EF only SELECTs the columns
        // we actually serialize. Saves bandwidth and memory.
        var items = await query
            .Select(c => new FootballClubDto(
                c.Id,
                c.ClubName,
                c.Country,
                c.League,
                c.FoundedYear,
                c.Stadium,
                c.TitlesWon))
            .ToListAsync(cancellationToken);

        return new PagedResult<FootballClubDto>(
            items,
            totalCount,
            request.PageIndex,
            request.PageSize);
    }

    // ---- Filtering, sorting, paging --------------------------------------
    // (Implemented in 5.3 - 5.5)
    private IQueryable<FootballClub> ApplyGlobalFilter
    (
        IQueryable<FootballClub> query,
        string? term
    )
    {
        if (string.IsNullOrWhiteSpace(term))
            return query;

        var trimmed = term.Trim();

        // Use EF.Functions.Like so EF translates this to T-SQL LIKE with wildcards.
        // Case-insensitivity comes from the SQL Server collation (default
        // collations are CI). EscapeLike protects against [, %, _ in user input.
        var pattern = $"%{EscapeLike(trimmed)}%";

        return query.Where(c =>
            EF.Functions.Like(c.ClubName, pattern) ||
            EF.Functions.Like(c.Country, pattern) ||
            EF.Functions.Like(c.League, pattern) ||
            EF.Functions.Like(c.Stadium, pattern));
    }

    private static string EscapeLike(string input)
    {
        // Escape the three LIKE wildcard chars so user input can't change the
        // meaning of the pattern. Order matters: replace the escape char first.
        return input
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }

    private IQueryable<FootballClub> ApplyColumnFilters(
    IQueryable<FootballClub> query,
    IReadOnlyList<ColumnFilterDescriptor> filters)
    {
        if (filters is null || filters.Count == 0)
            return query;

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Id) || filter.Value is null)
                continue;

            // Pull the raw value as a JsonElement (System.Text.Json gives us this
            // when a property is typed as object?). We extract the right shape
            // depending on the column type.
            var value = filter.Value;

            switch (filter.Id.ToLowerInvariant())
            {
                // -------- Text columns: contains-match -----------------------
                case "clubname":
                    query = ApplyContains(query, value, c => c.ClubName);
                    break;
                case "country":
                    query = ApplyContains(query, value, c => c.Country);
                    break;
                case "league":
                    query = ApplyContains(query, value, c => c.League);
                    break;
                case "stadium":
                    query = ApplyContains(query, value, c => c.Stadium);
                    break;

                // -------- Numeric columns: equals OR [min, max] range --------
                case "foundedyear":
                    query = ApplyNumberOrRange(query, value, c => c.FoundedYear);
                    break;
                case "titleswon":
                    query = ApplyNumberOrRange(query, value, c => c.TitlesWon);
                    break;
                case "id":
                    query = ApplyNumberOrRange(query, value, c => c.Id);
                    break;

                // Anything else: silently ignored. Allowlist semantics.
                default:
                    _logger.LogDebug("Ignoring unknown filter column: {ColumnId}", filter.Id);
                    break;
            }
        }

        return query;
    }

    // ---- Per-type filter helpers -------------------------------------------

    private static IQueryable<FootballClub> ApplyContains(
        IQueryable<FootballClub> query,
        object? rawValue,
        System.Linq.Expressions.Expression<Func<FootballClub, string>> selector)
    {
        var text = ExtractString(rawValue);
        if (string.IsNullOrWhiteSpace(text))
            return query;

        var pattern = $"%{EscapeLike(text.Trim())}%";

        // Build: c => EF.Functions.Like(selector(c), pattern)
        var parameter = selector.Parameters[0];
        var likeCall = System.Linq.Expressions.Expression.Call(
            typeof(DbFunctionsExtensions),
            nameof(DbFunctionsExtensions.Like),
            Type.EmptyTypes,
            System.Linq.Expressions.Expression.Constant(EF.Functions),
            selector.Body,
            System.Linq.Expressions.Expression.Constant(pattern));

        var lambda = System.Linq.Expressions.Expression
            .Lambda<Func<FootballClub, bool>>(likeCall, parameter);

        return query.Where(lambda);
    }

    private static IQueryable<FootballClub> ApplyNumberOrRange(
        IQueryable<FootballClub> query,
        object? rawValue,
        System.Linq.Expressions.Expression<Func<FootballClub, int>> selector)
    {
        if (rawValue is not JsonElement element)
            return query;

        var parameter = selector.Parameters[0];

        // Case 1: scalar number  -> equality filter
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var n))
        {
            var equalsExpr = System.Linq.Expressions.Expression.Equal(
                selector.Body,
                System.Linq.Expressions.Expression.Constant(n));

            return query.Where(System.Linq.Expressions.Expression
                .Lambda<Func<FootballClub, bool>>(equalsExpr, parameter));
        }

        // Case 2: array [min, max] -> inclusive range filter (either bound optional)
        if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() == 2)
        {
            int? min = TryGetInt(element[0]);
            int? max = TryGetInt(element[1]);

            if (min is not null)
            {
                var ge = System.Linq.Expressions.Expression.GreaterThanOrEqual(
                    selector.Body,
                    System.Linq.Expressions.Expression.Constant(min.Value));
                query = query.Where(System.Linq.Expressions.Expression
                    .Lambda<Func<FootballClub, bool>>(ge, parameter));
            }

            if (max is not null)
            {
                var le = System.Linq.Expressions.Expression.LessThanOrEqual(
                    selector.Body,
                    System.Linq.Expressions.Expression.Constant(max.Value));
                query = query.Where(System.Linq.Expressions.Expression
                    .Lambda<Func<FootballClub, bool>>(le, parameter));
            }
        }

        return query;
    }

    private static string? ExtractString(object? raw)
    {
        return raw switch
        {
            null => null,
            string s => s,
            JsonElement j when j.ValueKind == JsonValueKind.String => j.GetString(),
            JsonElement j when j.ValueKind == JsonValueKind.Number => j.ToString(),
            _ => raw.ToString()
        };
    }

    private static int? TryGetInt(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var n))
            return n;
        if (element.ValueKind == JsonValueKind.Null)
            return null;
        return null;
    }

    private IQueryable<FootballClub> ApplySorting(
    IQueryable<FootballClub> query,
    IReadOnlyList<SortDescriptor> sorts)
    {
        if (sorts is null || sorts.Count == 0)
        {
            // Default sort: stable order by primary key. Required so paging
            // returns deterministic rows when the client doesn't ask for sort.
            return query.OrderBy(c => c.Id);
        }

        IOrderedQueryable<FootballClub>? ordered = null;

        for (int i = 0; i < sorts.Count; i++)
        {
            var sort = sorts[i];
            if (string.IsNullOrWhiteSpace(sort.Id))
                continue;

            if (i == 0)
            {
                if (SortMap.TryGetValue(sort.Id, out var primary))
                    ordered = primary(query, sort.Desc);
            }
            else
            {
                if (ordered is not null && ThenSortMap.TryGetValue(sort.Id, out var then))
                    ordered = then(ordered, sort.Desc);
            }
        }

        // Final tiebreaker by Id so paging is always stable, even when the
        // client-supplied sort columns have ties.
        return ordered is null
            ? query.OrderBy(c => c.Id)
            : ordered.ThenBy(c => c.Id);
    }

    private static IQueryable<FootballClub> ApplyPaging(
        IQueryable<FootballClub> query,
        int pageIndex,
        int pageSize)
    {
        return query
            .Skip(pageIndex * pageSize)
            .Take(pageSize);
    }
}