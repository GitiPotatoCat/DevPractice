using FootballApp.Api.Dtos;
using FootballApp.Api.Models;
using FootballApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FootballApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class FootballClubsController : ControllerBase
{
    private readonly IFootballClubService _service;

    public FootballClubsController(IFootballClubService service)
    {
        _service = service;
    }

    /// <summary>
    /// Server-side paged/sorted/filtered list of football clubs.
    /// Designed to be driven directly by TanStack Table state.
    /// </summary>
    [HttpPost("query")]
    [ProducesResponseType(typeof(PagedResult<FootballClubDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<FootballClubDto>>> Query(
        [FromBody] PagedQueryRequest request,
        CancellationToken cancellationToken)
    {
        // [ApiController] makes ModelState validation automatic, so by the
        // time we get here pageIndex / pageSize are already in valid ranges.

        var result = await _service.QueryAsync(request, cancellationToken);
        return Ok(result);
    }
}