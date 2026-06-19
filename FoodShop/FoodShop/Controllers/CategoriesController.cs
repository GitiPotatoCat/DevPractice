using FoodShop.Data;
using FoodShop.DTOs;
using FoodShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodShop.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly FoodShopDbContext _db;
    public CategoriesController(FoodShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetAll()
        => Ok(await _db.Categories
            .Select(c => new CategoryReadDto(c.Id, c.Name))
            .ToListAsync());

    [HttpGet("{id:int}/products")]
    public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProductsInCategory(int id)
    {
        var exists = await _db.Categories.AnyAsync(c => c.Id == id);
        if (!exists) return NotFound();

        var products = await _db.Products
            .Where(p => p.CategoryID == id)
            .Include(p => p.Category)
            .Select(p => new ProductReadDto(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category!.Name))
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryReadDto>> Create(CategoryCreateDto dto)
    {
        var category = new Category { Name = dto.Name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new CategoryReadDto(category.Id, category.Name));
    }
}