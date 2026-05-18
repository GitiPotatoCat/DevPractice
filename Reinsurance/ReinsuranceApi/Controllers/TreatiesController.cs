using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReinsuranceApi.Data;
using ReinsuranceApi.Dtos;
using ReinsuranceApi.Models;
using ReinsuranceApi.Services;

namespace ReinsuranceApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TreatiesController : ControllerBase
{
    private readonly ReinsuranceDbContext _db;

    public TreatiesController(ReinsuranceDbContext db) => _db = db;

    /// <summary>
    /// Server-side grid endpoint — paging, sorting, filtering (incl. EMF), grouping.
    /// </summary>
    [HttpPost("grid")]
    public async Task<ActionResult<GridResponse<Treaty>>> GetGrid([FromBody] GridRequest req)
    {
        if (req.PageSize <= 0 || req.PageSize > 500) req.PageSize = 25;
        if (req.PageIndex < 0) req.PageIndex = 0;

        IQueryable<Treaty> query = _db.Treaties.AsNoTracking();

        query = GridQueryBuilder.ApplyGlobalSearch(query, req.GlobalSearch);
        query = GridQueryBuilder.ApplyFilters(query, req.Filters);

        var total = await query.CountAsync();

        var groups = await GridQueryBuilder.BuildGroupsAsync(query, req.Groups);

        query = GridQueryBuilder.ApplySorting(query, req.Sorts);

        var rows = await query
            .Skip(req.PageIndex * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync();

        return Ok(new GridResponse<Treaty>
        {
            Rows = rows,
            TotalCount = total,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Groups = groups.Count > 0 ? groups : null
        });
    }

    /// <summary>Distinct values for a column — populates EMF dropdown lists.</summary>
    [HttpGet("distinct/{field}")]
    public async Task<ActionResult<List<string>>> GetDistinct(string field)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TreatyType","LineOfBusiness","Country","Currency","Status","UnderwriterName","CedingCompany","Reinsurer"
        };
        if (!allowed.Contains(field)) return BadRequest("Field not allowed.");

        var values = field switch
        {
            "TreatyType" => await _db.Treaties.Select(t => t.TreatyType).Distinct().OrderBy(x => x).ToListAsync(),
            "LineOfBusiness" => await _db.Treaties.Select(t => t.LineOfBusiness).Distinct().OrderBy(x => x).ToListAsync(),
            "Country" => await _db.Treaties.Select(t => t.Country).Distinct().OrderBy(x => x).ToListAsync(),
            "Currency" => await _db.Treaties.Select(t => t.Currency).Distinct().OrderBy(x => x).ToListAsync(),
            "Status" => await _db.Treaties.Select(t => t.Status).Distinct().OrderBy(x => x).ToListAsync(),
            "UnderwriterName" => await _db.Treaties.Select(t => t.UnderwriterName).Distinct().OrderBy(x => x).ToListAsync(),
            "CedingCompany" => await _db.Treaties.Select(t => t.CedingCompany).Distinct().OrderBy(x => x).ToListAsync(),
            "Reinsurer" => await _db.Treaties.Select(t => t.Reinsurer).Distinct().OrderBy(x => x).ToListAsync(),
            _ => new List<string>()
        };

        return Ok(values);
    }
}