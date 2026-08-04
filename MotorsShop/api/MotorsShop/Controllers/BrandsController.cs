using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorsShop.Data;
using MotorsShop.Dtos;
using MotorsShop.Exceptions;
using MotorsShop.Models;


namespace MotorsShop.Controllers;


[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly MotorsShopDbContext _db;
    public BrandsController(MotorsShopDbContext db) => _db = db;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BrandReadDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<BrandReadDto>>> GetAll() =>
        Ok(await _db.Brands
            .Select(b => new BrandReadDto(b.Id, b.Name, b.Country))
            .ToListAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BrandReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<ActionResult<BrandReadDto>> GetById(int id)
    {
        var b = await _db.Brands.FindAsync(id);
        return b is null ? NotFound() : Ok(new BrandReadDto(b.Id, b.Name, b.Country));
    }

    [HttpPost]
    [ProducesResponseType(typeof(BrandReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BrandReadDto>> Create(BrandCreateDto dto)
    {
        var brand = new Brand { Name = dto.Name, Country = dto.Country };
        _db.Brands.Add(brand);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = brand.Id },
            new BrandReadDto(brand.Id, brand.Name, brand.Country));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, BrandUpdateDto dto)
    {
        var brand = await _db.Brands.FindAsync(id) ?? throw new NotFoundException("Brand", id);

        brand.Name = dto.Name;
        brand.Country = dto.Country;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var brand = await _db.Brands.FindAsync(id)
            ?? throw new NotFoundException("Brand", id);

        try {
            _db.Brands.Remove(brand);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException) {
            throw new ConflictException("Cannot delete brand: motorcycles still reference it.");
        }
    }
}