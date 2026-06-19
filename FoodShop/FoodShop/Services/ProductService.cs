using FoodShop.Data;
using FoodShop.DTOs;
using FoodShop.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodShop.Services;

public class ProductService : IProductService
{
    private readonly FoodShopDbContext _db;

    public ProductService(FoodShopDbContext db)
        => _db = db;


    public async Task<IEnumerable<ProductReadDto>> GetAllProductsAsync()
    {
        return await _db.Products
            .Include(p => p.Category)
            .Select(p => new ProductReadDto (
                        p.Id, p.Name,
                        p.Description, p.Price,
                        p.Stock, p.Category!.Name
                    ))
                     .ToListAsync(); 
    }

    public async Task<ProductReadDto> GetProductByIdAsync(int id)
    {
        var p = await _db.Products.Include(x => x.Category)
                                  .FirstOrDefaultAsync(y => y.Id == id);

        if (p == null)
            return null!;

        return new ProductReadDto 
            (
                p.Id, p.Name,
                p.Description, p.Price, 
                p.Stock, p.Category!.Name
            );
    }

    public async Task<ProductReadDto> CreateNewProductAsync(ProductCreateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryID = dto.CategoryId
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var category = await _db.Categories.FindAsync(dto.CategoryId);


        return new ProductReadDto(
                product.Id, product.Name, 
                product.Description, product.Price, 
                product.Stock, category!.Name ?? ""
            );
    }

    public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto)
    {
        var product = await _db.Products.FindAsync(id);

        if (product == null)
            return false;

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryID = dto.CategoryId;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);

        if (product == null)
            return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return true;
    }
}
