using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorsShop.Dtos;
using MotorsShop.Services;

namespace MotorsShop.Controllers;


[ApiController]
[Route("api/[controller]")]
public class MotorcyclesController : ControllerBase
{
    private readonly IMotorcycleService _service;
    public MotorcyclesController(IMotorcycleService service) => _service = service;


    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MotorcycleReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<MotorcycleReadDto>>> GetAll(
    [FromQuery] MotorcycleQueryDto query)
    => Ok(await _service.GetAllAsync(query));


    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MotorcycleReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<MotorcycleReadDto>> GetById(int id)
    {
        var bike = await _service.GetByIdAsync(id);
        return bike is null ? NotFound() : Ok(bike);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MotorcycleReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MotorcycleReadDto>> Create(MotorcycleCreateDto dto)
    {
        var created = await _service.CreateAsync(dto)
            ?? throw new MotorsShop.Exceptions.ValidationException(
                new Dictionary<string, string[]>
                {
                    ["BrandId or CategoryId"] = new[] { "Invalid BrandId or CategoryId." }
                });
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, MotorcycleUpdateDto dto)
        => await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
        => await _service.DeleteAsync(id) ? NoContent() : NotFound();
}